namespace CupkekGames.InventorySystem
{
    /// <summary>
    /// Decides whether an item may occupy a slot described by <see cref="EquipmentSlotDescriptor"/> (interpret
    /// <see cref="EquipmentSlotDescriptor.Metadata"/> in bridge/game code if needed). Assign an instance per row via
    /// <see cref="EquipmentSlotDescriptor.AcceptPolicy"/>, or combine mutually exclusive rules with
    /// <see cref="AnyMatchingEquipmentSlotAcceptPolicy"/> as the rack default when rows omit <see cref="EquipmentSlotDescriptor.AcceptPolicy"/>.
    /// </summary>
    public interface IEquipmentSlotAcceptPolicy
    {
        bool Accepts(EquipmentSlotDescriptor descriptor, InventoryItem item, InventoryItemDefinition definition);
    }
}
