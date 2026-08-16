using CupkekGames.InventorySystem;
using Unity.Scripting.LifecycleManagement;

namespace CupkekGames.InventorySystem.RPGStats
{
    /// <summary>
    /// Shared RPG bridge policy instances for assigning <see cref="EquipmentSlotDescriptor.AcceptPolicy"/> at layout time.
    /// </summary>
    public static partial class RpgEquipmentSlotAcceptPolicies
    {
        [NoAutoStaticsCleanup]
        private static readonly RpgConsumableCategoryEquipmentSlotAcceptPolicy ConsumableCategoryInstance = new();
        [NoAutoStaticsCleanup]
        private static readonly RpgEquipmentTypeEquipmentSlotAcceptPolicy EquipmentTypeInstance = new();
        [NoAutoStaticsCleanup]
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
