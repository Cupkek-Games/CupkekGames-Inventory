using System;
using System.Collections.Generic;
using CupkekGames.InventorySystem;

namespace CupkekGames.InventorySystem.RPGStats
{
    public enum UnequippedItemEquipmentSlotHighlightMode
    {
        /// <summary>
        /// Items with <see cref="ConsumableFeature"/> (and no <see cref="EquipableFeature"/>) target the consumable-category slot if present.
        /// </summary>
        PreferConsumableSlot,

        /// <summary>
        /// Used when inventory filters use a sentinel index past the last equipment slot (e.g. mobile sample).
        /// </summary>
        SentinelPastLastSlot
    }

    /// <summary>
    /// RPG bridge: maps items to equipment slot indices using <see cref="RpgEquipmentSlotMetadataKeys"/> on descriptors.
    /// </summary>
    public sealed class RpgEquipmentSlotTargeting : IEquipmentSlotTargeting
    {
        private readonly UnequippedItemEquipmentSlotHighlightMode _mode;

        public RpgEquipmentSlotTargeting(UnequippedItemEquipmentSlotHighlightMode mode)
        {
            _mode = mode;
        }

        public int GetTargetSlotIndex(
            InventoryItem item,
            InventoryItemDefinition definition,
            IReadOnlyList<EquipmentSlotDescriptor> descriptors,
            int slotCount)
        {
            EquipableFeature equip = definition?.GetFeature<EquipableFeature>();
            if (equip != null)
            {
                int index = FindSlotIndexForEquipmentType(descriptors, slotCount, equip.EquipmentType.Key);
                if (index >= 0)
                    return index;
                return _mode == UnequippedItemEquipmentSlotHighlightMode.SentinelPastLastSlot
                    ? slotCount
                    : -1;
            }

            if (_mode == UnequippedItemEquipmentSlotHighlightMode.PreferConsumableSlot)
            {
                return definition != null && definition.HasFeature<ConsumableFeature>()
                    ? GetConsumableSlotIndex(descriptors)
                    : -1;
            }

            return slotCount;
        }

        private static int FindSlotIndexForEquipmentType(
            IReadOnlyList<EquipmentSlotDescriptor> descriptors,
            int slotCount,
            string equipmentType)
        {
            int cap = Math.Min(descriptors.Count, slotCount);
            for (int i = 0; i < cap; i++)
            {
                if (RpgEquipmentSlotMetadataInternals.IsConsumableCategory(descriptors[i]))
                    continue;
                if (RpgEquipmentSlotMetadataInternals.TryGetEquipmentType(descriptors[i], out string key) &&
                    key == equipmentType)
                    return i;
            }

            return -1;
        }

        private static int GetConsumableSlotIndex(IReadOnlyList<EquipmentSlotDescriptor> descriptors)
        {
            for (int i = 0; i < descriptors.Count; i++)
            {
                if (RpgEquipmentSlotMetadataInternals.IsConsumableCategory(descriptors[i]))
                    return i;
            }

            return -1;
        }
    }
}
