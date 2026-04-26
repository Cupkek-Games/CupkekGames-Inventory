using CupkekGames.InventorySystem;

namespace CupkekGames.InventorySystem.RPGStats
{
    internal static class RpgEquipmentSlotMetadataInternals
    {
        internal static bool IsConsumableCategory(EquipmentSlotDescriptor d)
        {
            return d != null &&
                   d.TryGetMetadata(RpgEquipmentSlotMetadataKeys.SlotCategory, out string v) &&
                   v == RpgEquipmentSlotMetadataKeys.SlotCategoryConsumable;
        }

        internal static bool TryGetEquipmentType(EquipmentSlotDescriptor d, out string equipmentType)
        {
            equipmentType = null;
            return d != null &&
                   d.TryGetMetadata(RpgEquipmentSlotMetadataKeys.EquipmentType, out equipmentType) &&
                   !string.IsNullOrEmpty(equipmentType);
        }
    }
}
