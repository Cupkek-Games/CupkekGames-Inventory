using System;
using System.Collections.Generic;
using System.Linq;
using CupkekGames.Luna;
using UnityEngine;
using UnityEngine.UIElements;

namespace CupkekGames.InventorySystem
{
    /// <summary>
    /// Builds <see cref="ItemDragAndDrop"/> and runs <see cref="InventoryBase.MoveItem"/> for inventory-to-equipment drags,
    /// using <see cref="EquipmentRackController.CanAccept"/> for validation.
    /// </summary>
    public sealed class EquipmentDragSession
    {
        private readonly EquipmentRackController _controller;
        private readonly EquipmentDragSessionConfig _config;

        public EquipmentDragSession(EquipmentRackController controller, EquipmentDragSessionConfig config)
        {
            _controller = controller ?? throw new ArgumentNullException(nameof(controller));
            _config = config;
        }

        /// <summary>
        /// Returns <c>null</c> if <paramref name="item"/> is non-null and <see cref="EquipmentRackController.CanAccept"/> fails for <paramref name="equipmentSlotIndex"/>.
        /// </summary>
        public ItemDragAndDrop BeginEquip(
            InventoryBase inventory,
            InventoryItem item,
            InventoryItemDefinition itemDefinition,
            InventoryItemSlotController equipmentSlot,
            int equipmentSlotIndex,
            Action<ItemDragAndDrop> registerDropHandler)
        {
            if (inventory == null)
                throw new ArgumentNullException(nameof(inventory));
            if (equipmentSlot == null)
                throw new ArgumentNullException(nameof(equipmentSlot));
            if (_config.UIManager == null)
                throw new ArgumentNullException(nameof(_config.UIManager));
            if (_config.DragArea == null)
                throw new ArgumentNullException(nameof(_config.DragArea));
            if (_config.GetInventorySlots == null)
                throw new ArgumentNullException(nameof(_config.GetInventorySlots));
            if (_config.ItemSlotTemplate == null)
                throw new ArgumentNullException(nameof(_config.ItemSlotTemplate));
            if (_config.Database == null)
                throw new ArgumentNullException(nameof(_config.Database));
            if (_config.Owner == null)
                throw new ArgumentNullException(nameof(_config.Owner));

            if (item != null && !_controller.CanAccept(equipmentSlotIndex, item, itemDefinition))
                return null;

            InventoryItemSlotFactoryDelegate factory = _config.SlotFactory ?? InventoryItemSlotFactory.Default;
            InventoryItemSlotController ghost = factory(_config.Database, _config.Owner, null, null, _config.GhostTooltipPosition);
            if (_config.AttributeNames != null)
                ghost.SetAttributeNames(_config.AttributeNames);
            if (_config.GetStatComparison != null)
                ghost.SetGetStatComparison(_config.GetStatComparison);

            var dragAndDrop = new ItemDragAndDrop(
                _config.UIManager,
                _config.DragArea,
                equipmentSlotIndex,
                null,
                _config.GetInventorySlots,
                _config.ItemSlotTemplate,
                equipmentSlot,
                ghost);

            IReadOnlyList<string> highlight = _config.DropTargetHighlightClasses;
            if (highlight != null && highlight.Count > 0)
                dragAndDrop.SetDropSlotClasses(highlight.ToList());

            _config.ConfigureDragAndDrop?.Invoke(dragAndDrop);
            registerDropHandler?.Invoke(dragAndDrop);

            inventory.MoveItem(item, itemDefinition, equipmentSlot, equipmentSlotIndex, dragAndDrop, null);
            return dragAndDrop;
        }
    }

    public struct EquipmentDragSessionConfig
    {
        public LunaUIManager UIManager;
        public VisualElement DragArea;
        public VisualTreeAsset ItemSlotTemplate;
        public Func<List<VisualElement>> GetInventorySlots;
        public IInventoryItemDatabase Database;
        public GameObject Owner;
        public ICollection<string> AttributeNames;
        public Func<InventoryItem, InventoryItemDefinition, ItemStatData> GetStatComparison;
        public InventoryItemSlotFactoryDelegate SlotFactory;
        public TooltipPosition GhostTooltipPosition;
        /// <summary>
        /// When non-null and non-empty, applied via <see cref="ItemDragAndDrop.SetDropSlotClasses"/> before <see cref="ConfigureDragAndDrop"/>.
        /// </summary>
        public IReadOnlyList<string> DropTargetHighlightClasses;
        public Action<ItemDragAndDrop> ConfigureDragAndDrop;

        public EquipmentDragSessionConfig(
            LunaUIManager uiManager,
            VisualElement dragArea,
            VisualTreeAsset itemSlotTemplate,
            Func<List<VisualElement>> getInventorySlots,
            IInventoryItemDatabase database,
            GameObject owner)
        {
            UIManager = uiManager;
            DragArea = dragArea;
            ItemSlotTemplate = itemSlotTemplate;
            GetInventorySlots = getInventorySlots;
            Database = database;
            Owner = owner;
            AttributeNames = null;
            GetStatComparison = null;
            SlotFactory = null;
            GhostTooltipPosition = TooltipPosition.Right;
            DropTargetHighlightClasses = null;
            ConfigureDragAndDrop = null;
        }
    }
}
