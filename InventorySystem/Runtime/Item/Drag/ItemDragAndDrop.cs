using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using CupkekGames.Luna;

namespace CupkekGames.InventorySystem
{
    public class ItemDragAndDrop : DragAndDropManipulator
    {
        protected VisualTreeAsset _itemSlotTemplate;
        protected InventoryItemSlotController _dragStarterSlot;
        protected InventoryItemSlotController _dragElementSlot;
        public event Action<int, int, VisualElement, InventoryItemSlotController> OnItemDrop;
        public ItemDragAndDrop(
            LunaUIManager uiElementManager,
            VisualElement dragArea,
            int key,
            List<VisualElement> dropSlots,
            Func<List<VisualElement>> getDropSlots,
            VisualTreeAsset itemSlotTemplate,
            InventoryItemSlotController dragStarterSlot,
            InventoryItemSlotController dragElementSlot
        )
         : base(uiElementManager, dragArea, key, dropSlots, getDropSlots)
        {
            _itemSlotTemplate = itemSlotTemplate;
            _dragStarterSlot = dragStarterSlot;
            _dragElementSlot = dragElementSlot;
        }

        public override VisualElement CreateDragElement()
        {
            VisualElement dragElement = _itemSlotTemplate.Instantiate();
            dragElement.AddToClassList("size-128");
            dragElement.AddToClassList("item-drag-ghost");

            _dragElementSlot.ResetVisualElements(dragElement);
            _dragElementSlot.BindItem(_dragStarterSlot.Item, _dragStarterSlot.ItemDefinition, _dragStarterSlot.Index, false, null, null);

            OnDrop += OnItemDropInner;

            return dragElement;
        }

        public override void DisposeDragElement()
        {
            OnDrop -= OnItemDropInner;
        }

        protected void OnItemDropInner(int key, int dropIndex, VisualElement dropSlot)
        {
            OnItemDrop?.Invoke(key, dropIndex, dropSlot, _dragStarterSlot);
        }
    }
}
