using System;
using System.Collections.Generic;
using UnityEngine;
using CupkekGames.Luna;

namespace CupkekGames.InventorySystem
{
    [Serializable]
    public abstract class InventoryBase
    {
        public abstract List<InventoryItem> Items { get; }

        public abstract InventoryItem GetItem(Guid id);
        public abstract InventoryItem GetItemAt(int index);

        public abstract void AddItem(InventoryItem add);
        public abstract void SetItem(int index, InventoryItem item);

        public abstract bool RemoveItem(Guid id, int amount);

        public abstract bool RemoveItem(InventoryItem item);

        public abstract int GetItemSlot(Guid id);

        public abstract void SwapSlots(int a, int b);

        public bool RemoveItem(Guid id)
        {
            var item = GetItem(id);

            return RemoveItem(item);
        }

        /// <summary>How many of the item keyed <paramref name="itemKey"/> the inventory holds, across its stacks.</summary>
        public int Count(string itemKey)
        {
            int count = 0;
            foreach (InventoryItem item in Items)
            {
                if (item != null && item.Key.Key == itemKey)
                    count += item.Amount;
            }

            return count;
        }

        /// <summary>
        /// Takes every line out, stack by stack (a recipe, a price in items). Throws, taking
        /// nothing, when the inventory does not hold all of them.
        /// </summary>
        public void RemoveAll(IReadOnlyList<InventoryItemReference> lines)
        {
            var needed = new Dictionary<string, int>();
            foreach (InventoryItemReference line in lines)
            {
                needed.TryGetValue(line.ItemKey.Key, out int sum);
                needed[line.ItemKey.Key] = sum + line.Amount;
            }

            foreach (KeyValuePair<string, int> need in needed)
            {
                int held = Count(need.Key);
                if (held < need.Value)
                    throw new InvalidOperationException(
                        $"[Inventory] RemoveAll needs {need.Value} of '{need.Key}' but the inventory holds {held}.");
            }

            foreach (KeyValuePair<string, int> need in needed)
            {
                int left = need.Value;
                foreach (InventoryItem item in new List<InventoryItem>(Items))
                {
                    if (left <= 0) break;
                    if (item == null || item.Key.Key != need.Key) continue;

                    int take = Math.Min(left, item.Amount);
                    RemoveItem(item.ID, take);
                    left -= take;
                }
            }
        }

        /// <summary>
        /// Moves an item into a slot controller (inventory or equipment UI). Does <b>not</b> enforce
        /// <see cref="IEquipmentSlotAcceptPolicy"/> — for equipment targets, call
        /// <see cref="EquipmentRackController.CanAccept"/> (or your own policy) first, or rely on
        /// <see cref="InventoryItemSlotController.SetAcceptCondition"/> when binding through the slot UI.
        /// </summary>
        public void MoveItem(IInventoryItemDatabase manager, Guid id, InventoryItemSlotController to,
            int index, ItemDragAndDrop dragAndDrop, Func<List<TooltipContainerSetup>> getSetups)
        {
            InventoryItem item = GetItem(id);

            MoveItem(item, manager.GetItemDefinition(item), to, index, dragAndDrop, getSetups);
        }

        /// <summary>
        /// See <see cref="MoveItem(IInventoryItemDatabase, Guid, InventoryItemSlotController, int, ItemDragAndDrop, Func{List{TooltipContainerSetup}})"/> for equipment policy notes.
        /// </summary>
        public virtual void MoveItem(InventoryItem item, InventoryItemDefinition itemDefinition, InventoryItemSlotController to,
            int index, ItemDragAndDrop dragAndDrop, Func<List<TooltipContainerSetup>> getSetups)
        {
            bool toSelected = to.Selected;
            if (!to.IsEmpty)
            {
                InventoryItem toItem = to.Item;
                if (item == null)
                {
                    AddItem(toItem);
                    to.UnbindItem();
                }
                else
                {
                    if (toItem.CanStackWith(item))
                    {
                        int left = toItem.AddAmount(item.Amount);
                        item.SetAmount(left);
                        if (item.Amount == 0)
                        {
                            RemoveItem(item);
                        }

                        to.UpdateItemDisplay();
                    }
                    else
                    {
                        int itemSlot = GetItemSlot(item.ID);
                        RemoveItem(item);

                        if (itemSlot != -1)
                        {
                            SetItem(itemSlot, toItem);
                        }
                        else
                        {
                            AddItem(toItem);
                        }

                        to.UnbindItem();
                        to.BindItem(item, itemDefinition, index, toSelected, dragAndDrop, getSetups);
                    }
                }
            }
            else
            {
                if (item != null)
                {
                    RemoveItem(item);
                    to.BindItem(item, itemDefinition, index, toSelected, dragAndDrop, getSetups);
                }
            }
        }
    }
}
