using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using CupkekGames.Luna;

namespace CupkekGames.InventorySystem
{
    /// <summary>
    /// Binds <see cref="EquipmentSlotDescriptor"/> entries to <see cref="InventoryItemSlotController"/> instances
    /// (parallel active lists). In-memory equipped state sync only — no save/serialization.
    /// </summary>
    public sealed class EquipmentSlotRack
    {
#if UNITY_EDITOR
        private static readonly HashSet<string> EditorRebuildWarnOnce = new();

        private static void LogRebuildMissOnce(EquipmentSlotDescriptor d, string detail)
        {
            string k = $"{detail}|{d.RootElementName}|{d.ChildElementName}|{d.GetPersistenceKey()}";
            if (!EditorRebuildWarnOnce.Add(k))
                return;
            Debug.LogWarning(
                $"[EquipmentSlotRack] Skipped slot '{d.DisplayName}' (persistence '{d.GetPersistenceKey()}'): {detail}.");
        }
#endif

        private readonly List<EquipmentSlotDescriptor> _activeDescriptors = new();
        private readonly List<InventoryItemSlotController> _slots = new();

        public IReadOnlyList<EquipmentSlotDescriptor> ActiveDescriptors => _activeDescriptors;
        public IReadOnlyList<InventoryItemSlotController> Slots => _slots;

        /// <summary>
        /// Clears built slots and rebuilds from <paramref name="layoutDescriptors"/> in order.
        /// Skips descriptors whose UI cannot be resolved under <paramref name="queryRoot"/>.
        /// Each slot uses <see cref="EquipmentSlotDescriptor.AcceptPolicy"/> when set, otherwise <paramref name="acceptPolicy"/>.
        /// </summary>
        public void Rebuild(
            VisualElement queryRoot,
            IReadOnlyList<EquipmentSlotDescriptor> layoutDescriptors,
            IInventoryItemDatabase itemDatabase,
            GameObject owner,
            TooltipController tooltipController,
            TooltipPosition slotTooltipPosition,
            IEquipmentSlotAcceptPolicy acceptPolicy,
            ICollection<string> attributeNames,
            Func<InventoryItem, InventoryItemDefinition, ItemStatData> getStatComparison,
            InventoryItemSlotFactoryDelegate slotFactory = null,
            Action<InventoryItemSlotController, EquipmentSlotDescriptor> configureSlot = null)
        {
            if (queryRoot == null)
                throw new ArgumentNullException(nameof(queryRoot));
            if (layoutDescriptors == null)
                throw new ArgumentNullException(nameof(layoutDescriptors));
            if (itemDatabase == null)
                throw new ArgumentNullException(nameof(itemDatabase));
            if (acceptPolicy == null)
                throw new ArgumentNullException(nameof(acceptPolicy));

            _activeDescriptors.Clear();
            _slots.Clear();

            InventoryItemSlotFactoryDelegate factory = slotFactory ?? InventoryItemSlotFactory.Default;

            foreach (EquipmentSlotDescriptor d in layoutDescriptors)
            {
                VisualElement root = queryRoot.Q<VisualElement>(d.RootElementName);
                if (root == null)
                {
#if UNITY_EDITOR
                    LogRebuildMissOnce(d, $"no VisualElement named '{d.RootElementName}' under query root");
#endif
                    continue;
                }

                VisualElement slotVe = string.IsNullOrEmpty(d.ChildElementName)
                    ? root
                    : root.Q<VisualElement>(d.ChildElementName);
                if (slotVe == null)
                {
#if UNITY_EDITOR
                    LogRebuildMissOnce(d,
                        $"no VisualElement named '{d.ChildElementName}' under '{d.RootElementName}'");
#endif
                    continue;
                }

                InventoryItemSlotController slot = factory(itemDatabase, owner, slotVe, tooltipController,
                    slotTooltipPosition);
                EquipmentSlotDescriptor captured = d;
                IEquipmentSlotAcceptPolicy slotPolicy = captured.AcceptPolicy ?? acceptPolicy;
                slot.SetAcceptCondition((item, def) => slotPolicy.Accepts(captured, item, def));
                if (attributeNames != null)
                    slot.SetAttributeNames(attributeNames);
                if (getStatComparison != null)
                    slot.SetGetStatComparison(getStatComparison);
                slot.SetEmptyIcon(d.EmptyIcon);
                configureSlot?.Invoke(slot, d);

                _activeDescriptors.Add(d);
                _slots.Add(slot);
            }
        }

        public void UnbindAllOccupiedSlots()
        {
            foreach (InventoryItemSlotController slot in _slots)
            {
                if (slot != null && !slot.IsEmpty)
                    slot.UnbindItem();
            }
        }

        public Dictionary<string, InventoryItem> ToEquippedDictionary()
        {
            var dict = new Dictionary<string, InventoryItem>();
            for (int i = 0; i < _slots.Count && i < _activeDescriptors.Count; i++)
            {
                string key = _activeDescriptors[i].GetPersistenceKey();
                if (!string.IsNullOrEmpty(key) && !_slots[i].IsEmpty)
                    dict[key] = _slots[i].Item;
            }

            return dict;
        }

        /// <summary>
        /// Clears slots, then binds items from <paramref name="equipped"/> using persistence keys on active descriptors.
        /// </summary>
        public void ApplyEquippedDictionary(
            Dictionary<string, InventoryItem> equipped,
            IInventoryItemDatabase database,
            Action<int, InventoryItem, InventoryItemDefinition> bindSlot)
        {
            if (database == null)
                throw new ArgumentNullException(nameof(database));
            if (bindSlot == null)
                throw new ArgumentNullException(nameof(bindSlot));

            UnbindAllOccupiedSlots();

            if (equipped == null)
                return;

            for (int i = 0; i < _slots.Count && i < _activeDescriptors.Count; i++)
            {
                string key = _activeDescriptors[i].GetPersistenceKey();
                if (string.IsNullOrEmpty(key))
                    continue;
                if (equipped.TryGetValue(key, out InventoryItem item) && item != null)
                {
                    InventoryItemDefinition def = database.GetItemDefinition(item);
                    bindSlot(i, item, def);
                }
            }
        }
    }
}
