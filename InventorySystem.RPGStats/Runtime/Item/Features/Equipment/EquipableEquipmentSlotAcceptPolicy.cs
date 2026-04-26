using System;
using CupkekGames.InventorySystem;

namespace CupkekGames.InventorySystem.RPGStats
{
    /// <summary>
    /// Back-compat façade over <see cref="RpgEquipmentSlotAcceptPolicies.Default"/> (consumable-category + equipment-type
    /// policies composed with <see cref="AnyMatchingEquipmentSlotAcceptPolicy"/>). Prefer assigning
    /// <see cref="EquipmentSlotDescriptor.AcceptPolicy"/> via <see cref="RpgEquipmentSlotLayoutBuilder"/> / <see cref="RpgEquipmentSlotAcceptPolicies"/>.
    /// </summary>
    public sealed class EquipableEquipmentSlotAcceptPolicy : IEquipmentSlotAcceptPolicy
    {
        private readonly IEquipmentSlotAcceptPolicy _inner;

        public EquipableEquipmentSlotAcceptPolicy()
            : this(RpgEquipmentSlotAcceptPolicies.Default)
        {
        }

        public EquipableEquipmentSlotAcceptPolicy(IEquipmentSlotAcceptPolicy inner)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        }

        public bool Accepts(EquipmentSlotDescriptor descriptor, InventoryItem item, InventoryItemDefinition definition)
            => _inner.Accepts(descriptor, item, definition);
    }
}
