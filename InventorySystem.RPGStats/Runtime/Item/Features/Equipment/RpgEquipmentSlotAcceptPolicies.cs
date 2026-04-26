using CupkekGames.InventorySystem;

namespace CupkekGames.InventorySystem.RPGStats
{
    /// <summary>
    /// Shared RPG bridge policy instances for assigning <see cref="EquipmentSlotDescriptor.AcceptPolicy"/> at layout time.
    /// </summary>
    public static class RpgEquipmentSlotAcceptPolicies
    {
        private static readonly RpgConsumableCategoryEquipmentSlotAcceptPolicy ConsumableCategoryInstance = new();
        private static readonly RpgEquipmentTypeEquipmentSlotAcceptPolicy EquipmentTypeInstance = new();
        private static readonly AnyMatchingEquipmentSlotAcceptPolicy DefaultComposite = new(
            new IEquipmentSlotAcceptPolicy[] { ConsumableCategoryInstance, EquipmentTypeInstance });

        /// <summary>Consumable-category rows from <see cref="RpgEquipmentSlotLayoutBuilder.AddConsumableSlot"/>.</summary>
        public static IEquipmentSlotAcceptPolicy ConsumableCategory => ConsumableCategoryInstance;

        /// <summary>Typed equipment rows from <see cref="RpgEquipmentSlotLayoutBuilder.AddEquipmentTypeSlots"/>.</summary>
        public static IEquipmentSlotAcceptPolicy EquipmentType => EquipmentTypeInstance;

        /// <summary>OR of consumable-category and equipment-type policies — use as rack default when rows omit <see cref="EquipmentSlotDescriptor.AcceptPolicy"/>.</summary>
        public static IEquipmentSlotAcceptPolicy Default => DefaultComposite;
    }
}
