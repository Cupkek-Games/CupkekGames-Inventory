using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using CupkekGames.Luna;

namespace CupkekGames.InventorySystem
{
  public class InventoryViewPagination : GridViewPagination<InventoryItem>, IInventoryItemSlotGridView
  {
    // References
    protected IInventoryItemDatabase _itemDatabase;
    // Fields
    protected InventoryBase _inventory;
    protected ICollection<string> _attributes;
    protected Sprite _emptyIcon;
    protected TooltipManipulator _emptyTooltipManipulator;
    protected Func<InventoryItem, InventoryItemDefinition, ItemStatData> _getAttributeComparison;
    protected InventoryItemSlotFactoryDelegate _slotFactory;
    // Properties
    protected override List<InventoryItem> _itemList
    {
      get
      {
        if (_inventory == null)
        {
          return new List<InventoryItem>();
        }

        return _inventory.Items;
      }
      set
      {

      }
    }
    // Tooltip
    protected TooltipPosition _tooltipPosition;
    protected Func<InventoryItem, InventoryItemDefinition, Func<List<TooltipContainerSetup>>> _getTooltipSetups;
    // Drag & Drop
    protected List<VisualElement> _dropSlots;
    protected Func<InventoryItem, List<VisualElement>> _getDropSlots;
    protected Action<int, int, VisualElement, InventoryItemSlotController> _onDrop;
    protected VisualElement _dragArea;
    protected bool _allowDropInventory = false;
    /// <summary>USS classes applied to valid drop targets while dragging (e.g. <c>highlight</c>).</summary>
    protected List<string> _dropSlotHighlightClasses;
    // Events
    public event Action<InventoryItemSlotController> OnItemSlotClick;

    public InventoryViewPagination(
      IInventoryItemDatabase itemDatabase,
      InventoryBase inventory,
      VisualTreeAsset itemSlotTemplate,
      GameObject parent,
      VisualElement parentElement,
      VisualElement slotContainer,
      TooltipController tooltipController,
      UIStartVisibility startVisibility = UIStartVisibility.Visible,
      VisualElement focusElement = null,
      float fadeDuration = 0.5F,
      EasingMode easingMode = EasingMode.EaseOutCirc,
      bool debug = false,
      InventoryItemSlotFactoryDelegate slotFactory = null) :
      base(
        null,
        itemSlotTemplate,
        parent,
        parentElement,
        slotContainer,
        tooltipController,
        startVisibility,
        focusElement,
        fadeDuration,
        easingMode,
        debug)
    {
      _itemDatabase = itemDatabase;
      _inventory = inventory;
      _slotFactory = slotFactory ?? InventoryItemSlotFactory.WithSharedMissingDefinitionNotifier(
          new LogOnceMissingInventoryItemDefinitionNotifier());
    }

    public void SetAttributes(ICollection<string> attributes)
    {
      _attributes = attributes;
    }
    public void SetEmptyIcon(Sprite emptyIcon)
    {
      _emptyIcon = emptyIcon;
    }
    public void SetEmptyTooltip(TooltipManipulator tooltipManipulator)
    {
      _emptyTooltipManipulator = tooltipManipulator;
    }
    public void SetTooltipPosition(TooltipPosition tooltipPosition)
    {
      _tooltipPosition = tooltipPosition;
    }

    public void SetDragAndDrop(
      List<VisualElement> dropSlots,
      Func<InventoryItem, List<VisualElement>> getDropSlots,
      Action<int, int, VisualElement, InventoryItemSlotController> onDrop,
      VisualElement dragArea,
      bool allowDropInventory
    )
    {
      _dropSlots = dropSlots;
      _getDropSlots = getDropSlots;
      _onDrop = onDrop;
      _dragArea = dragArea;
      _allowDropInventory = allowDropInventory;
    }

    public void SetGetTooltipSetups(Func<InventoryItem, InventoryItemDefinition, Func<List<TooltipContainerSetup>>> getTooltipSetups)
    {
      _getTooltipSetups = getTooltipSetups;
    }

    public void SetGetStatComparison(Func<InventoryItem, InventoryItemDefinition, ItemStatData> getAttributeComparison)
    {
      _getAttributeComparison = getAttributeComparison;
    }

    /// <summary>
    /// When non-empty, passed to <see cref="ItemDragAndDrop.SetDropSlotClasses"/> for inventory-origin drags.
    /// </summary>
    public void SetDropSlotHighlightClasses(IReadOnlyList<string> classes)
    {
      _dropSlotHighlightClasses = null;
      if (classes != null && classes.Count > 0)
        _dropSlotHighlightClasses = new List<string>(classes);
    }

    public void HideFilterContainer(VisualElement container)
    {
      if (container == null) return;
      container.style.display = DisplayStyle.None;
    }

    public void ShowOnlyFilterButtons(VisualElement container, string buttonName, List<int> visibleIndices)
    {
      if (container == null) return;
      var buttons = container.Query<Button>(buttonName).ToList();
      for (int i = 0; i < buttons.Count; i++)
      {
        buttons[i].style.display = visibleIndices.Contains(i) ? DisplayStyle.Flex : DisplayStyle.None;
      }
    }

    protected void OnItemSlotClickInner(InventoryItemSlotController controller)
    {
      OnItemSlotClick?.Invoke(controller);
    }
    protected override void OnItemCreate(VisualElement itemSlot)
    {
      InventoryItemSlotController controller = _slotFactory(_itemDatabase, Parent, itemSlot, _tooltipController, _tooltipPosition);
      controller.SetAttributeNames(_attributes);
      controller.SetGetStatComparison(_getAttributeComparison);
      controller.SetEmptyIcon(_emptyIcon);
      controller.SetEmptyTooltip(_emptyTooltipManipulator);
      itemSlot.userData = controller;

      controller.OnClick += OnItemSlotClickInner;
    }
    protected override void BindItem(VisualElement slot, InventoryItem item, int slotIndex, bool selected)
    {
      if (item == null)
      {
        UnbindItem(slot);
        return;
      }

      InventoryItemSlotController slotController = (InventoryItemSlotController)slot.userData;

      InventoryItemDefinition itemDefinition = _itemDatabase.GetItemDefinition(item);

      var itemDragAndDrop = CreateDragAndDrop(slotIndex, slotController, item);

      Func<List<TooltipContainerSetup>> getSetups = null;
      if (_getTooltipSetups != null)
      {
        getSetups = _getTooltipSetups(item, itemDefinition);
      }

      slotController.BindItem(item, itemDefinition, slotIndex, selected, itemDragAndDrop, getSetups);
    }

    protected override void UnbindItem(VisualElement slot)
    {
      InventoryItemSlotController slotController = (InventoryItemSlotController)slot.userData;

      slotController.UnbindItem();
    }

    protected virtual ItemDragAndDrop CreateDragAndDrop(int slotIndex, InventoryItemSlotController slotController, InventoryItem inventoryItem)
    {
      List<VisualElement> dropSlots = GetDropSlots(inventoryItem);
      if (dropSlots.Count == 0)
      {
        return null;
      }

      InventoryItemSlotController dragSlotController = _slotFactory(_itemDatabase, Parent, null, null, _tooltipPosition);
      dragSlotController.SetAttributeNames(_attributes);
      dragSlotController.SetGetStatComparison(_getAttributeComparison);

      var itemDragAndDrop = new ItemDragAndDrop(LunaUIManager, _dragArea, slotIndex, dropSlots, null, _itemSlotTemplate, slotController, dragSlotController);
      itemDragAndDrop.OnItemDrop += _onDrop;
      if (_dropSlotHighlightClasses != null && _dropSlotHighlightClasses.Count > 0)
        itemDragAndDrop.SetDropSlotClasses(new List<string>(_dropSlotHighlightClasses));

      return itemDragAndDrop;
    }

    protected List<VisualElement> GetDropSlots(InventoryItem inventoryItem)
    {
      List<VisualElement> dropSlots = new List<VisualElement>();
      if (_dropSlots != null)
      {
        dropSlots.AddRange(_dropSlots);
      }
      if (_getDropSlots != null)
      {
        dropSlots.AddRange(_getDropSlots(inventoryItem));
      }
      if (_allowDropInventory)
      {
        dropSlots.AddRange(ItemSlots);
      }

      return dropSlots;
    }
  }
}