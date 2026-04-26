using System;
using System.Collections.Generic;
using UnityEngine;

namespace CupkekGames.InventorySystem
{
    /// <summary>
    /// Passed to <see cref="IItemFeature.BuildTooltipContent"/> so features can render stats
    /// without a hard dependency on slot UI.
    /// <para>
    /// For runtime state that lives outside the item model (e.g. farm timers, market prices,
    /// quest progress), register an extension provider on the
    /// <see cref="InventoryItemSlotTooltipBuilder"/> and retrieve it here with
    /// <see cref="GetExtension{T}"/>.
    /// </para>
    /// </summary>
    public readonly struct ItemTooltipContext
    {
        public IInventoryItemDatabase ItemDatabase { get; }
        public InventoryItem Item { get; }
        public InventoryItemDefinition Definition { get; }
        public ICollection<string> AttributeNames { get; }
        public Func<InventoryItem, InventoryItemDefinition, ItemStatData> GetComparison { get; }
        public GameObject Owner { get; }

        private readonly IReadOnlyList<KeyValuePair<Type, object>> _extensions;

        /// <summary>
        /// Returns the registered extension of type <typeparamref name="T"/>, or <c>null</c>
        /// if no provider was registered for that type.
        /// </summary>
        public T GetExtension<T>() where T : class
        {
            if (_extensions == null) return null;
            var target = typeof(T);
            for (int i = 0; i < _extensions.Count; i++)
            {
                if (_extensions[i].Key == target)
                    return (T)_extensions[i].Value;
            }
            return null;
        }

        public ItemTooltipContext(
            IInventoryItemDatabase itemDatabase,
            InventoryItem item,
            InventoryItemDefinition definition,
            ICollection<string> attributeNames,
            Func<InventoryItem, InventoryItemDefinition, ItemStatData> getComparison,
            GameObject owner,
            IReadOnlyList<KeyValuePair<Type, object>> extensions = null)
        {
            ItemDatabase = itemDatabase;
            Item = item;
            Definition = definition;
            AttributeNames = attributeNames;
            GetComparison = getComparison;
            Owner = owner;
            _extensions = extensions;
        }
    }
}
