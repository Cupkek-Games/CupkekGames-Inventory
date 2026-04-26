using System.Collections.Generic;
using CupkekGames.InventorySystem;
using UnityEngine;

namespace CupkekGames.InventorySystem.RPGStats
{
    /// <summary>
    /// Builds <see cref="EquipmentSlotDescriptor"/> rows with RPG bridge <see cref="RpgEquipmentSlotMetadataKeys"/>.
    /// </summary>
    public sealed class RpgEquipmentSlotLayoutBuilder
    {
        private readonly List<EquipmentSlotDescriptor> _rows = new();

        /// <summary>
        /// True if a non-empty list still has entries with no <see cref="EquipmentSlotDescriptor.Metadata"/> (e.g. Unity
        /// stripped obsolete fields). Clear the list and rebuild from code when true.
        /// </summary>
        public static bool HasLegacyDescriptors(IReadOnlyList<EquipmentSlotDescriptor> list)
        {
            if (list == null || list.Count == 0)
                return false;

            for (int i = 0; i < list.Count; i++)
            {
                EquipmentSlotDescriptor d = list[i];
                if (d == null)
                    continue;
                if (d.Metadata == null || d.Metadata.Count == 0)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Adds up to <c>min(typeKeys.Count, rootElementNames.Count)</c> equipment-type slots (consumable category not used).
        /// </summary>
        public void AddEquipmentTypeSlots(
            IReadOnlyList<string> typeKeys,
            IReadOnlyList<string> rootElementNames,
            IReadOnlyList<Sprite> emptyIcons = null,
            string childElementName = null)
        {
            if (typeKeys == null || rootElementNames == null)
                return;

            int n = Mathf.Min(typeKeys.Count, rootElementNames.Count);
            for (int i = 0; i < n; i++)
            {
                string key = typeKeys[i];
                var d = new EquipmentSlotDescriptor
                {
                    SlotId = key,
                    DisplayName = key,
                    RootElementName = rootElementNames[i],
                    ChildElementName = childElementName ?? "",
                    EmptyIcon = emptyIcons != null && i < emptyIcons.Count ? emptyIcons[i] : null
                };
                d.Metadata.Add(new SlotMetadataEntry
                {
                    Key = RpgEquipmentSlotMetadataKeys.EquipmentType,
                    Value = key
                });
                d.AcceptPolicy = RpgEquipmentSlotAcceptPolicies.EquipmentType;
                _rows.Add(d);
            }
        }

        /// <summary>Consumable-category slot (per <see cref="RpgConsumableCategoryEquipmentSlotAcceptPolicy"/>): requires <see cref="ConsumableFeature"/>, no <see cref="EquipableFeature"/>.</summary>
        public void AddConsumableSlot(
            string slotId,
            string displayName,
            string rootElementName,
            Sprite emptyIcon,
            string childElementName = "")
        {
            var d = new EquipmentSlotDescriptor
            {
                SlotId = slotId,
                DisplayName = displayName,
                RootElementName = rootElementName,
                ChildElementName = childElementName ?? "",
                EmptyIcon = emptyIcon
            };
            d.Metadata.Add(new SlotMetadataEntry
            {
                Key = RpgEquipmentSlotMetadataKeys.SlotCategory,
                Value = RpgEquipmentSlotMetadataKeys.SlotCategoryConsumable
            });
            d.AcceptPolicy = RpgEquipmentSlotAcceptPolicies.ConsumableCategory;
            _rows.Add(d);
        }

        public List<EquipmentSlotDescriptor> Build() => new List<EquipmentSlotDescriptor>(_rows);
    }
}
