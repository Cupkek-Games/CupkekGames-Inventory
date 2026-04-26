using System;
using System.Collections.Generic;

namespace CupkekGames.InventorySystem
{
    /// <summary>
    /// Returns true when any inner policy accepts. Intended for mutually exclusive slot shapes (each inner returns
    /// false for descriptors it does not own).
    /// </summary>
    public sealed class AnyMatchingEquipmentSlotAcceptPolicy : IEquipmentSlotAcceptPolicy
    {
        private readonly IReadOnlyList<IEquipmentSlotAcceptPolicy> _policies;

        public AnyMatchingEquipmentSlotAcceptPolicy(IReadOnlyList<IEquipmentSlotAcceptPolicy> policies)
        {
            _policies = policies ?? throw new ArgumentNullException(nameof(policies));
        }

        public bool Accepts(EquipmentSlotDescriptor descriptor, InventoryItem item, InventoryItemDefinition definition)
        {
            for (int i = 0; i < _policies.Count; i++)
            {
                if (_policies[i] != null && _policies[i].Accepts(descriptor, item, definition))
                    return true;
            }

            return false;
        }
    }
}
