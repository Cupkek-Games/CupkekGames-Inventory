namespace CupkekGames.InventorySystem.RPGStats
{
    /// <summary>
    /// Metadata keys for stock RPG bridge slot layout + <see cref="EquipableEquipmentSlotAcceptPolicy"/>.
    /// InventorySystem core does not reference these strings.
    /// </summary>
    public static class RpgEquipmentSlotMetadataKeys
    {
        public const string EquipmentType = "rpg.equipmentType";

        /// <summary>Values: <see cref="SlotCategoryConsumable"/> for non-<see cref="EquipableFeature"/> items only.</summary>
        public const string SlotCategory = "rpg.slotCategory";

        public const string SlotCategoryConsumable = "consumable";
    }
}
