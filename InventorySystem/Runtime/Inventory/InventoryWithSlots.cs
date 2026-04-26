using System;
using System.Collections.Generic;
using System.Linq;
using CupkekGames.Luna;
using UnityEngine;

namespace CupkekGames.InventorySystem
{
    [Serializable]
    public class InventoryWithSlots : InventoryBase
    {
        [SerializeField] private Dictionary<int, InventoryItem> _items;

        public override List<InventoryItem> Items
        {
            get
            {
                if (_items.Count == 0)
                    return new List<InventoryItem>();

                int maxSlot = _items.Keys.Max();
                List<InventoryItem> result = new List<InventoryItem>(maxSlot + 1);

                // Fill with null values
                for (int i = 0; i <= maxSlot; i++)
                {
                    if (_items.TryGetValue(i, out InventoryItem item))
                    {
                        result.Add(item);
                    }
                    else
                    {
                        result.Add(null);
                    }
                }

                return result;
            }
        }

        public InventoryWithSlots()
        {
            _items = new Dictionary<int, InventoryItem>();
        }
        public InventoryWithSlots(Dictionary<int, InventoryItem> items)
        {
            _items = items;
        }

        public override InventoryItem GetItem(Guid id)
        {
            foreach (var item in _items)
            {
                if (item.Value.ID == id)
                {
                    return item.Value;
                }
            }

            return null; // Item with the specified id not found
        }

        public override InventoryItem GetItemAt(int index)
        {
            if (_items.TryGetValue(index, out InventoryItem item))
            {
                return item;
            }

            return null; // Item not found at the specified index
        }

        public override void AddItem(InventoryItem add)
        {
            // Find all items that can stack with the item to add.
            foreach (var stackableItem in _items.Where(item => item.Value.CanStackWith(add)))
            {
                int remainingAmount = stackableItem.Value.AddAmount(add.Amount);
                add.SetAmount(remainingAmount);

                // Stop if all of the amount has been added.
                if (remainingAmount == 0)
                    return;
            }

            // If there's any amount left, add the item as a new stack.
            if (add.Amount > 0)
            {
                int slot = GetFirstEmptySlot();
                if (slot == -1)
                {
                    // No gap below max index; append after highest occupied slot.
                    slot = _items.Keys.Max() + 1;
                }

                _items.Add(slot, add);
            }
        }

        public override void SetItem(int index, InventoryItem item)
        {
            // Check for null item
            if (item == null)
            {
                Debug.LogError("Cannot set null item in inventory with slots.");
                return;
            }

            // Check if index is valid (negative indices are not allowed)
            if (index < 0)
            {
                Debug.LogError($"Invalid inventory slot index: {index}. Index must be non-negative.");
                return;
            }

            // Remove any existing item with the same ID from any slot
            int slotToRemove = -1;
            foreach (var pair in _items)
            {
                if (pair.Value.ID == item.ID)
                {
                    slotToRemove = pair.Key;
                    break;
                }
            }

            if (slotToRemove >= 0)
            {
                _items.Remove(slotToRemove);
            }

            // Add the item to the specified slot
            _items[index] = item;
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
                return _items.Remove(slotToRemove);
            }

            return false;
        }
        public override bool RemoveItem(InventoryItem item)
        {
            int slotToRemove = GetItemSlot(item.ID);
            if (slotToRemove != -1)
            {
                return _items.Remove(slotToRemove);
            }

            return false;
        }

        #region Dictionary Methods

        public override int GetItemSlot(Guid id)
        {
            foreach (var pair in _items)
            {
                if (pair.Value.ID == id)
                {
                    return pair.Key;
                }
            }

            return -1; // Item not found in any slot.
        }

        public int GetFirstEmptySlot()
        {
            if (_items.Count == 0)
                return 0;

            int maxSlot = _items.Keys.Max();
            // First gap in [0, maxSlot), if any.
            for (int i = 0; i < maxSlot; i++)
            {
                if (!_items.ContainsKey(i))
                {
                    return i;
                }
            }

            // If no empty slot is found, return -1 or any other value indicating no empty slot is available.
            return -1;
        }

        public override void SwapSlots(int a, int b)
        {
            bool hasA = _items.TryGetValue(a, out var itemA);
            bool hasB = _items.TryGetValue(b, out var itemB);

            if (hasA && hasB)
            {
                _items[a] = itemB;
                _items[b] = itemA;
            }
            else if (hasA)
            {
                _items[b] = itemA;
                _items.Remove(a);
            }
            else if (hasB)
            {
                _items[a] = itemB;
                _items.Remove(b);
            }
        }


        #endregion
    }
}