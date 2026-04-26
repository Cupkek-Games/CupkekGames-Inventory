using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CupkekGames.Luna;

namespace CupkekGames.InventorySystem
{
    [Serializable]
    public class Inventory : InventoryBase
    {
        [SerializeField] protected List<InventoryItem> _items;

        public override List<InventoryItem> Items => _items;

        public Inventory()
        {
            _items = new List<InventoryItem>();
        }

        public Inventory(List<InventoryItem> items)
        {
            _items = items;
        }

        /// <summary>Deep copy with new item IDs so actual inventory state is isolated from a cloned default.</summary>
        public Inventory(Inventory other)
        {
            _items = new List<InventoryItem>();
            if (other?._items == null)
                return;
            for (int i = 0; i < other._items.Count; i++)
            {
                InventoryItem item = other._items[i];
                _items.Add(item != null ? new InventoryItem(item, sameId: false) : null);
            }
        }

        public override InventoryItem GetItem(Guid id)
        {
            foreach (InventoryItem item in _items)
            {
                if (item.ID == id)
                {
                    return item;
                }
            }

            return null; // Item with the specified id not found
        }

        public override InventoryItem GetItemAt(int index)
        {
            if (index < 0 || index >= _items.Count)
            {
                return null;
            }

            return _items[index];
        }

        public override void AddItem(InventoryItem add)
        {
            // Find all items that can stack with the item to add.
            foreach (var stackableItem in _items.Where(item => item.CanStackWith(add)))
            {
                int remainingAmount = stackableItem.AddAmount(add.Amount);
                add.SetAmount(remainingAmount);

                // Stop if all of the amount has been added.
                if (remainingAmount == 0)
                    return;
            }

            // If there's any amount left, add the item as a new stack.
            if (add.Amount > 0)
            {
                _items.Add(add);
            }
        }

        public override void SetItem(int index, InventoryItem item)
        {
            // Check for null item
            if (item == null)
            {
                Debug.LogError("Cannot set null item in inventory.");
                return;
            }

            // Validate index is within bounds
            if (index < 0 || index > _items.Count)
            {
                Debug.LogError($"Invalid inventory index: {index}. Valid range is 0 to {_items.Count}.");
                return;
            }

            // Find and remove any existing item with the same ID
            var existingItem = _items.FirstOrDefault(i => i.ID == item.ID);
            if (existingItem != null)
            {
                _items.Remove(existingItem);
            }

            // Insert the item at the specified index
            _items.Insert(index, item);
        }

        /// <summary>
        /// Removes a specified amount of an item from the inventory.
        /// </summary>
        /// <param name="id">The unique identifier of the item to remove.</param>
        /// <param name="amount">The amount to remove.</param>
        /// <returns>True if the item was completely removed, false if some amount remains.</returns>
        public override bool RemoveItem(Guid id, int amount)
        {
            int slotToRemove = GetItemSlot(id);
            if (slotToRemove == -1)
                return false; // Item not found.

            var item = _items[slotToRemove];

            // Calculate the remaining amount after removal.
            int remainingAmount = item.AddAmount(-amount);

            // Remove the item completely if its amount is reduced to zero or less.
            if (item.Amount == 0)
            {
                _items.RemoveAt(slotToRemove);
                return true;
            }

            return false;
        }

        public override bool RemoveItem(InventoryItem item)
        {
            if (item != null)
            {
                return _items.Remove(item);
            }

            return false;
        }

        #region List Methods

        public override int GetItemSlot(Guid id)
        {
            for (int i = 0; i < _items.Count; i++)
            {
                if (_items[i].ID == id)
                {
                    return i;
                }
            }

            return -1; // Item not found in any slot.
        }

        public override void SwapSlots(int a, int b)
        {
            if (a < 0 || a >= _items.Count || b < 0 || b >= _items.Count)
            {
                return;
            }

            InventoryItem temp = _items[a];
            _items[a] = _items[b];
            _items[b] = temp;
        }

        #endregion
    }
}