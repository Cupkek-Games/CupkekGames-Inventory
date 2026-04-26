using CupkekGames.Data;
using CupkekGames.InventorySystem;

namespace CupkekGames.InventorySystem.RPGStats
{
    /// <summary>
    /// Accepts items only for descriptors with consumable <see cref="RpgEquipmentSlotMetadataKeys.SlotCategory"/> metadata.
    /// </summary>
    public sealed class RpgConsumableCategoryEquipmentSlotAcceptPolicy : IEquipmentSlotAcceptPolicy
    {
        public bool Accepts(EquipmentSlotDescriptor descriptor, InventoryItem item, InventoryItemDefinition definition)
        {
            if (!RpgEquipmentSlotMetadataInternals.IsConsumableCategory(descriptor))
                return false;

            EquipableFeature equip = definition?.GetFeature<EquipableFeature>();
            if (equip != null)
                return false;

            return definition != null && definition.HasFeature<ConsumableFeature>();
        }
    }
}
