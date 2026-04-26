using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using CupkekGames.Luna;

namespace CupkekGames.InventorySystem
{
    public class InventoryItemSlotController
    {
        protected readonly InventoryItemSlotTooltipBuilder TooltipBuilder;
        protected readonly InventoryItemSlotFeatureVisuals FeatureVisuals;

        protected IInventoryItemDatabase _itemDatabase;
        protected int _index = -1;
        protected bool _selected;
        protected InventoryItem _item;
        protected InventoryItemDefinition _itemDefinition;
        protected GameObject _owner;
        protected TooltipController _tooltipController;
        protected TooltipManipulator _tooltipManipulator;
        protected ItemDragAndDrop _dragAndDrop;
        protected ICollection<string> _attributeNames;
        protected Func<InventoryItem, InventoryItemDefinition, ItemStatData> _getAttributeComparison;
        protected Func<InventoryItem, InventoryItemDefinition, bool> _acceptCondition;
        protected Sprite _emptyIcon;
        protected TooltipManipulator _emptyTooltipManipulator;

        public int Index => _index;
        public bool Selected => _selected;
        public InventoryItem Item => _item;
        public InventoryItemDefinition ItemDefinition => _itemDefinition;
        public bool IsEmpty => _item == null;

        public VisualElement Parent;
        public Button Background;
        public Label TopLeft;
        public Label TopRight;
        public Label BottomLeft;
        public Label BottomRight;
        protected TooltipPosition _tooltipPosition;

        public event Action<InventoryItemSlotController> OnClick;

        public InventoryItemSlotController(IInventoryItemDatabase itemDatabase, GameObject owner, VisualElement parent,
            TooltipController tooltipController, TooltipPosition tooltipPosition)
        {
            _itemDatabase = itemDatabase;
            _owner = owner;
            _tooltipPosition = tooltipPosition;
            var notifier = new LogOnceMissingInventoryItemDefinitionNotifier();
            TooltipBuilder = new InventoryItemSlotTooltipBuilder(itemDatabase, owner, notifier);
            FeatureVisuals = new InventoryItemSlotFeatureVisuals();
            ResetVisualElements(parent);
            _tooltipController = tooltipController;
        }

        /// <param name="missingDefinitionNotifier">
        /// Pass a shared instance across slots on the same screen to dedupe missing-definition logs per item key.
        /// </param>
        public InventoryItemSlotController(IInventoryItemDatabase itemDatabase, GameObject owner, VisualElement parent,
            TooltipController tooltipController, TooltipPosition tooltipPosition,
            IMissingInventoryItemDefinitionNotifier missingDefinitionNotifier)
        {
            _itemDatabase = itemDatabase;
            _owner = owner;
            _tooltipPosition = tooltipPosition;
            IMissingInventoryItemDefinitionNotifier notifier =
                missingDefinitionNotifier ?? new LogOnceMissingInventoryItemDefinitionNotifier();
            TooltipBuilder = new InventoryItemSlotTooltipBuilder(itemDatabase, owner, notifier);
            FeatureVisuals = new InventoryItemSlotFeatureVisuals();
            ResetVisualElements(parent);
            _tooltipController = tooltipController;
        }

        /// <summary>Advanced: use when supplying custom tooltip/feature collaborators (e.g. tests).</summary>
        protected InventoryItemSlotController(
            IInventoryItemDatabase itemDatabase,
            GameObject owner,
            VisualElement parent,
            TooltipController tooltipController,
            TooltipPosition tooltipPosition,
            InventoryItemSlotTooltipBuilder tooltipBuilder,
            InventoryItemSlotFeatureVisuals featureVisuals)
        {
            _itemDatabase = itemDatabase;
            _owner = owner;
            _tooltipPosition = tooltipPosition;
            TooltipBuilder = tooltipBuilder ?? throw new ArgumentNullException(nameof(tooltipBuilder));
            FeatureVisuals = featureVisuals ?? throw new ArgumentNullException(nameof(featureVisuals));
            ResetVisualElements(parent);
            _tooltipController = tooltipController;
        }

        public void SetAcceptCondition(Func<InventoryItem, InventoryItemDefinition, bool> condition)
        {
            _acceptCondition = condition;
        }

        public void SetGetStatComparison(Func<InventoryItem, InventoryItemDefinition, ItemStatData> comparison)
        {
            _getAttributeComparison = comparison;
        }

        public virtual void ResetVisualElements(VisualElement parent)
        {
            UnbindItem();

            if (Background != null)
            {
                Background.clicked -= OnButtonClick;
            }

            Parent = parent;

            if (Parent == null)
            {
                return;
            }

            Background = Parent.Q<Button>("Background");
            Background.AddToClassList(_tooltipPosition.GetUssClass());

            Background.clicked += OnButtonClick;

            TopLeft = Parent.Q<Label>("TopLeft");
            TopRight = Parent.Q<Label>("TopRight");
            BottomLeft = Parent.Q<Label>("BottomLeft");
            BottomRight = Parent.Q<Label>("BottomRight");

            ClearItemDisplay();
        }

        public void SetAttributeNames(ICollection<string> attributeNames)
        {
            _attributeNames = attributeNames;
        }

        public void SetEmptyIcon(Sprite icon)
        {
            _emptyIcon = icon;

            if (IsEmpty && Background != null)
            {
                Background.style.backgroundImage = icon != null ? new StyleBackground(icon) : null;
            }
        }

        public void SetEmptyTooltip(TooltipManipulator tooltipManipulator)
        {
            _emptyTooltipManipulator = tooltipManipulator;

            if (_emptyTooltipManipulator != null && IsEmpty && Background != null)
            {
                Background.AddManipulator(_emptyTooltipManipulator);
            }
        }

        public virtual bool BindItem(InventoryItem item, InventoryItemDefinition itemDefinition, int index, bool selected,
            ItemDragAndDrop dragAndDrop, Func<List<TooltipContainerSetup>> getSetups)
        {
            if (_acceptCondition != null && item != null && !_acceptCondition(item, itemDefinition))
                return false;

            _item = item;
            _selected = selected;
            _itemDefinition = itemDefinition;
            _index = index;
            _dragAndDrop = dragAndDrop;

            UpdateItemDisplay();

            UpdateTooltip(getSetups);

            if (_dragAndDrop != null && Background != null)
            {
                Background.AddManipulator(_dragAndDrop);
            }

            return true;
        }

        public virtual void UnbindItem()
        {
            if (Background != null)
                RemoveManipulators();

            if (_emptyTooltipManipulator != null && Background != null)
            {
                Background.AddManipulator(_emptyTooltipManipulator);
            }

            FeatureVisuals.Clear(Parent, _itemDefinition);

            _item = null;
            _itemDefinition = null;
            _index = -1;
            _selected = false;

            ClearItemDisplay();
        }

        protected void OnButtonClick()
        {
            if (_tooltipManipulator != null)
            {
                _tooltipManipulator.Close();
            }

            OnClick?.Invoke(this);

            if (_tooltipManipulator != null && !IsEmpty)
            {
                _tooltipManipulator.Show();
            }
        }

        public TooltipContainerSetup GetTooltipSetup()
        {
            return BuildTooltipSetup(_getAttributeComparison);
        }

        public TooltipContainerSetup GetTooltipSetup(Func<InventoryItem, InventoryItemDefinition, ItemStatData> getComparison)
        {
            return BuildTooltipSetup(getComparison ?? _getAttributeComparison);
        }

        TooltipContainerSetup BuildTooltipSetup(Func<InventoryItem, InventoryItemDefinition, ItemStatData> getComparison)
        {
            if (_item == null || Background == null)
                return new TooltipContainerSetup(null, new Label(""), new Label(""), null);

            return TooltipBuilder.Build(
                _item,
                _itemDefinition,
                Background.style.backgroundImage,
                _attributeNames,
                getComparison);
        }

        public virtual void UpdateItemDisplay()
        {
            FeatureVisuals.Clear(Parent, _itemDefinition);

            if (_item == null || _itemDatabase == null)
            {
                if (Background != null)
                {
                    Background.style.backgroundImage = _emptyIcon != null ? new StyleBackground(_emptyIcon) : null;
                    ClearLabels();
                    Background.RemoveFromClassList("selected");
                    if (_selected)
                        Background.AddToClassList("selected");
                }

                return;
            }

            if (Background == null)
                return;

            Sprite icon = _itemDatabase.GetItemIcon(_item);
            Background.style.backgroundImage = icon != null ? new StyleBackground(icon) : null;
            if (BottomRight != null)
            {
                BottomRight.text = _item.Amount > 1 ? "x" + _item.Amount : "";
            }

            Background.RemoveFromClassList("selected");
            if (_selected)
            {
                Background.AddToClassList("selected");
            }

            FeatureVisuals.Apply(Parent, _itemDatabase, _item, _itemDefinition, _owner);
        }

        public virtual void ClearItemDisplay()
        {
            if (Background != null)
            {
                Background.style.backgroundImage = _emptyIcon != null ? new StyleBackground(_emptyIcon) : null;
            }

            ClearLabels();
        }

        protected void UpdateTooltip(Func<List<TooltipContainerSetup>> getSetups)
        {
            RemoveTooltipManipulator();

            if (_item == null || _tooltipController == null || Background == null)
                return;

            _tooltipManipulator = new TooltipManipulator(_owner, _tooltipController);
            _tooltipManipulator.SetSetup(GetTooltipSetup());
            _tooltipManipulator.SetSetupProviders(getSetups);

            Background.AddManipulator(_tooltipManipulator);
        }

        protected void RemoveManipulators()
        {
            RemoveTooltipManipulator();
            RemoveDragManipulator();
        }

        protected void RemoveTooltipManipulator()
        {
            if (_tooltipManipulator != null && Background != null)
            {
                _tooltipManipulator.Close();
                Background.RemoveManipulator(_tooltipManipulator);
            }

            if (_emptyTooltipManipulator != null && Background != null)
            {
                _emptyTooltipManipulator.Close();
                Background.RemoveManipulator(_emptyTooltipManipulator);
            }
        }

        protected void RemoveDragManipulator()
        {
            if (_dragAndDrop != null && Background != null)
            {
                Background.RemoveManipulator(_dragAndDrop);
            }
        }

        protected void ClearLabels()
        {
            if (TopLeft != null)
            {
                TopLeft.text = "";
            }

            if (TopRight != null)
            {
                TopRight.text = "";
            }

            if (BottomLeft != null)
            {
                BottomLeft.text = "";
            }

            if (BottomRight != null)
            {
                BottomRight.text = "";
            }
        }

        public void ReopenTooltip()
        {
            if (_tooltipManipulator != null)
            {
                _tooltipManipulator.Close();

                if (!IsEmpty)
                {
                    _tooltipManipulator.Show();
                }
            }
        }

        public void SetTooltipPosition(TooltipPosition tooltipPosition)
        {
            if (Background == null)
            {
                _tooltipPosition = tooltipPosition;
                return;
            }

            Background.RemoveFromClassList(_tooltipPosition.GetUssClass());
            Background.AddToClassList(tooltipPosition.GetUssClass());
            _tooltipPosition = tooltipPosition;
        }
    }
}
