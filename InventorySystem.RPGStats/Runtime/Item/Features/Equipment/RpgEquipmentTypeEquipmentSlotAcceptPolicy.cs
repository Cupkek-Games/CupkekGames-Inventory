using CupkekGames.InventorySystem;

namespace CupkekGames.InventorySystem.RPGStats
{
    /// <summary>
    /// Accepts items only for descriptors that declare <see cref="RpgEquipmentSlotMetadataKeys.EquipmentType"/> and are
    /// not consumable category rows.
    /// </summary>
    public sealed class RpgEquipmentTypeEquipmentSlotAcceptPolicy : IEquipmentSlotAcceptPolicy
    {
        public bool Accepts(EquipmentSlotDescriptor descriptor, InventoryItem item, InventoryItemDefinition definition)
        {
            if (RpgEquipmentSlotMetadataInternals.IsConsumableCategory(descriptor))
                return false;

            if (!RpgEquipmentSlotMetadataInternals.TryGetEquipmentType(descriptor, out string requiredType))
                return false;

            EquipableFeature equip = definition?.GetFeature<EquipableFeature>();
            return equip != null && !equip.EquipmentType.IsEmpty && equip.EquipmentType.Key == requiredType;
        }
    }
}
