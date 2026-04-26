using UnityEngine;

namespace CupkekGames.InventorySystem
{
    /// <summary>
    /// Passed to <see cref="IItemSlotVisual"/> so features can style slots without coupling to slot controller types.
    /// </summary>
    public readonly struct ItemSlotContext
    {
        public IInventoryItemDatabase ItemDatabase { get; }
        public InventoryItem Item { get; }
        public InventoryItemDefinition Definition { get; }
        public GameObject Owner { get; }

        public ItemSlotContext(
            IInventoryItemDatabase itemDatabase,
            InventoryItem item,
            InventoryItemDefinition definition,
            GameObject owner)
        {
            ItemDatabase = itemDatabase;
            Item = item;
            Definition = definition;
            Owner = owner;
        }
    }
}
