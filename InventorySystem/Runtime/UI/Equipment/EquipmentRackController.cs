using System;
using System.Collections.Generic;
using CupkekGames.Luna;
using UnityEngine;
using UnityEngine.UIElements;

namespace CupkekGames.InventorySystem
{
    /// <summary>
    /// Coordinates an <see cref="EquipmentSlotRack"/> with default acceptance policy and optional targeting/filter strategies.
    /// Per-row policy comes from <see cref="EquipmentSlotDescriptor.AcceptPolicy"/> when set; <see cref="CanAccept"/> uses the
    /// same rule as slot <see cref="InventoryItemSlotController"/> conditions.
    /// </summary>
    public sealed class EquipmentRackController
    {
        private readonly EquipmentSlotRack _rack;
        private readonly IEquipmentSlotAcceptPolicy _policy;
        private readonly IEquipmentSlotTargeting _targeting;
        private readonly IEquipmentItemFilter _filter;

        public EquipmentRackController(
            EquipmentSlotRack rack,
            IEquipmentSlotAcceptPolicy policy,
            IEquipmentSlotTargeting targeting = null,
            IEquipmentItemFilter filter = null)
        {
            _rack = rack ?? throw new ArgumentNullException(nameof(rack));
            _policy = policy ?? throw new ArgumentNullException(nameof(policy));
            _targeting = targeting;
            _filter = filter;
        }

        public EquipmentSlotRack Rack => _rack;
        public IReadOnlyList<EquipmentSlotDescriptor> Descriptors => _rack.ActiveDescriptors;
        public IReadOnlyList<InventoryItemSlotController> Slots => _rack.Slots;

        public bool CanAccept(int slotIndex, InventoryItem item, InventoryItemDefinition definition)
        {
            if (slotIndex < 0 || slotIndex >= _rack.ActiveDescriptors.Count)
                return false;
            EquipmentSlotDescriptor d = _rack.ActiveDescriptors[slotIndex];
            IEquipmentSlotAcceptPolicy slotPolicy = d.AcceptPolicy ?? _policy;
            return slotPolicy.Accepts(d, item, definition);
        }

        public int GetTargetSlot(InventoryItem item, InventoryItemDefinition definition)
        {
            if (_targeting == null)
                return -1;
            return _targeting.GetTargetSlotIndex(item, definition, _rack.ActiveDescriptors, _rack.Slots.Count);
        }

        public int FindSlotForDrop(VisualElement dropElement)
        {
            if (dropElement == null)
                return -1;
            IReadOnlyList<InventoryItemSlotController> slots = _rack.Slots;
            if (slots == null)
                return -1;

            for (VisualElement ve = dropElement; ve != null; ve = ve.parent)
            {
                for (int i = 0; i < slots.Count; i++)
                {
                    InventoryItemSlotController slot = slots[i];
                    if (slot != null && slot.Parent == ve)
                        return i;
                }
            }

            return -1;
        }

        public List<InventoryItem> Filter(List<InventoryItem> items, int filterIndex, IInventoryItemDatabase database)
        {
            if (filterIndex < 0 || _filter == null)
                return items;
            return _filter.Filter(items, filterIndex, database, _rack.ActiveDescriptors);
        }

        public void Build(
            VisualElement queryRoot,
            IReadOnlyList<EquipmentSlotDescriptor> layoutDescriptors,
            IInventoryItemDatabase itemDatabase,
            GameObject owner,
            TooltipController tooltipController,
            TooltipPosition slotTooltipPosition,
            ICollection<string> attributeNames,
            Func<InventoryItem, InventoryItemDefinition, ItemStatData> getStatComparison,
            InventoryItemSlotFactoryDelegate slotFactory = null,
            Action<InventoryItemSlotController, EquipmentSlotDescriptor> configureSlot = null)
        {
            _rack.Rebuild(
                queryRoot,
                layoutDescriptors,
                itemDatabase,
                owner,
                tooltipController,
                slotTooltipPosition,
                _policy,
                attributeNames,
                getStatComparison,
                slotFactory,
                configureSlot);
        }
    }
}
