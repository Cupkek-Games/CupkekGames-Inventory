using System;
using System.Collections.Generic;
using CupkekGames.InventorySystem;

namespace CupkekGames.InventorySystem.RPGStats
{
    /// <summary>
    /// RPG bridge: stat diff for tooltips vs the item currently in the matching equipment slot.
    /// </summary>
    public sealed class RpgStatComparisonProvider
    {
        private readonly EquipmentRackController _controller;

        public RpgStatComparisonProvider(EquipmentRackController controller)
        {
            _controller = controller ?? throw new ArgumentNullException(nameof(controller));
        }

        public ItemStatData GetComparison(
            InventoryItem item,
            InventoryItemDefinition itemDefinition,
            IInventoryItemDatabase database)
        {
            EquipableFeature equip = itemDefinition?.GetFeature<EquipableFeature>();
            if (equip == null)
                return null;

            IReadOnlyList<EquipmentSlotDescriptor> descriptors = _controller.Descriptors;
            IReadOnlyList<InventoryItemSlotController> slots = _controller.Slots;

            int idx = FindSlotIndexForEquipmentType(descriptors, slots.Count, equip.EquipmentType.Key);
            if (idx < 0 || idx >= slots.Count)
                return null;

            InventoryItemSlotController slot = slots[idx];
            if (slot.IsEmpty)
                return null;

            InventoryItemDefinition slotDef = database.GetItemDefinition(slot.Item);
            EquipableFeature slotEquip = slotDef?.GetFeature<EquipableFeature>();
            return slotEquip == null ? null : EquipableFeature.ToItemStatData(slotEquip.Effects);
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
    }
}
