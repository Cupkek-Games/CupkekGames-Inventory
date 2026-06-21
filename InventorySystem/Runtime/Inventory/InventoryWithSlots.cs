using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using CupkekGames.KeyValueDatabases;
using CupkekGames.Luna;
using UnityEngine;

namespace CupkekGames.InventorySystem
{
    [Serializable]
    public class InventoryWithSlots : InventoryBase
    {
        // Slot (int) -> item. KeyValueDatabase round-trips on both Unity and reflection serializers
        // (Newtonsoft, …), so slot inventories persist through saves. The field is bound by Unity;
        // SlotItems below is the surface reflection serializers see.
        [SerializeField] private KeyValueDatabase<int, InventoryItem> _items = new();

        /// <summary>
        /// Public accessor so reflection-based serializers (Newtonsoft, …) persist the slot map.
        /// Unity ignores properties and serializes <c>_items</c> directly.
        /// </summary>
        public KeyValueDatabase<int, InventoryItem> SlotItems
        {
            get => _items;
            set => _items = value ?? new KeyValueDatabase<int, InventoryItem>();
        }

        // Computed (dense) view of the sparse slot map — not the serialized surface (SlotItems is).
        // [IgnoreDataMember] keeps reflection serializers from emitting a redundant, non-round-trippable
        // array here, while the base Inventory.Items (a live list) stays serialized as before.
        [IgnoreDataMember]
        public override List<InventoryItem> Items
        {
            get
            {
                if (_items.Count == 0)
                    return new List<InventoryItem>();

                int maxSlot = _items.Keys.Max();
                List<InventoryItem> result = new List<InventoryItem>(maxSlot + 1);

                // Fill with null values for empty slots so list index == slot index.
                for (int i = 0; i <= maxSlot; i++)
                {
                    result.Add(_items.TryGetValue(i, out InventoryItem item) ? item : null);
                }

                return result;
            }
        }

        public InventoryWithSlots()
        {
        }

        public InventoryWithSlots(Dictionary<int, InventoryItem> items)
        {
            if (items == null)
                return;

            foreach (var pair in items)
                _items.TryAdd(pair.Key, pair.Value);
        }

        public override InventoryItem GetItem(Guid id)
        {
            foreach (InventoryItem item in _items.Values)
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
            return _items.TryGetValue(index, out InventoryItem item) ? item : null;
        }

        public override void AddItem(InventoryItem add)
        {
            // Stack into existing compatible stacks first.
            foreach (InventoryItem stackable in _items.Values)
            {
                if (!stackable.CanStackWith(add))
                    continue;

                int remainingAmount = stackable.AddAmount(add.Amount);
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

                _items.TryAdd(slot, add);
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
            int slotToRemove = GetItemSlot(item.ID);
            if (slotToRemove >= 0)
            {
                _items.TryRemove(slotToRemove);
            }

            // Add the item to the specified slot
            _items.SetValue(index, item);
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

            var item = _items.GetValue(slotToRemove);

            // Calculate the remaining amount after removal.
            item.AddAmount(-amount);

            // Remove the item completely if its amount is reduced to zero or less.
            if (item.Amount == 0)
            {
                return _items.TryRemove(slotToRemove);
            }

            return false;
        }
        public override bool RemoveItem(InventoryItem item)
        {
            int slotToRemove = GetItemSlot(item.ID);
            if (slotToRemove != -1)
            {
                return _items.TryRemove(slotToRemove);
            }

            return false;
        }

        #region Slot Methods

        public override int GetItemSlot(Guid id)
        {
            foreach (int slot in _items.Keys)
            {
                if (_items.GetValue(slot).ID == id)
                {
                    return slot;
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
                _items.SetValue(a, itemB);
                _items.SetValue(b, itemA);
            }
            else if (hasA)
            {
                _items.SetValue(b, itemA);
                _items.TryRemove(a);
            }
            else if (hasB)
            {
                _items.SetValue(a, itemB);
                _items.TryRemove(b);
            }
        }


        #endregion
    }
}
