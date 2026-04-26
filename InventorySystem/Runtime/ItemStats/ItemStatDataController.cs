using System;
using UnityEngine.UIElements;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using CupkekGames.Luna;

namespace CupkekGames.InventorySystem
{
  public class ItemStatDataController
  {
    private VisualElement _container;
    public VisualElement Container => _container;
    private Dictionary<string, ValueComparisonLineController> _controllers = new();
    private Dictionary<string, Sprite> _icons;
    private Dictionary<string, Func<float, string>> _beautify;
    private readonly Func<string, string> _resolveStatDisplayName;
    private GameObject _owner;
    private TooltipController _tooltipController;
    private TooltipPosition _tooltipPosition;

    public ItemStatDataController(VisualElement parent,
      ItemStatData statData, ItemStatData comparison, bool hideOldValue, int itemPerLine,
      bool withName, bool withIcon, Dictionary<string, Sprite> icons = null, GameObject owner = null,
      TooltipController tooltipController = null, TooltipPosition tooltipPosition = TooltipPosition.Right,
      Dictionary<string, Func<float, string>> beautify = null, Func<string, string> resolveStatDisplayName = null)
    {
      if (parent == null)
      {
        throw new Exception("parent cannot be null");
      }

      _icons = icons;
      _beautify = beautify;
      _resolveStatDisplayName = resolveStatDisplayName;
      _owner = owner;
      _tooltipController = tooltipController;
      _tooltipPosition = tooltipPosition;

      _container = new VisualElement();
      _container.AddToClassList("flex-row");

      for (int i = 0; i < itemPerLine; i++)
      {
        VisualElement col = new()
        {
          name = "ItemStatColumn"
        };
        col.AddToClassList("flex-col");
        _container.Add(col);
      }

      List<VisualElement> columns = _container.Children().ToList();

      int statIndex = 0;
      foreach (var stat in statData.Stats)
      {
        if (stat.StatKey.IsEmpty) continue;

        int colIndex = statIndex % itemPerLine;
        VisualElement col = columns[colIndex];

        ValueComparisonLine statLine;
        float value = stat.Value;
        Sprite icon = null;
        if (icons != null && icons.TryGetValue(stat.StatKey.Key, out var i))
        {
          icon = i;
        }
        if (comparison != null)
        {
          float comparisonValue = comparison.Get(stat.StatKey.Key);
          if (Mathf.Approximately(value, 0) && Mathf.Approximately(comparisonValue, 0))
          {
            statIndex++;
            continue;
          }

          string valueStr = null;
          ValueDeltaType deltaType = ValueDeltaUtility.Get(value, comparisonValue);
          if (deltaType != ValueDeltaType.NEUTRAL)
          {
            valueStr = ValueToString(stat.StatKey.Key, value);
          }

          statLine = new ValueComparisonLine(stat.StatKey.Key, DisplayNameFor(stat.StatKey.Key), icon, ValueToString(stat.StatKey.Key, comparisonValue), valueStr, deltaType, hideOldValue);
        }
        else
        {
          if (Mathf.Approximately(value, 0))
          {
            statIndex++;
            continue;
          }
          statLine = new ValueComparisonLine(stat.StatKey.Key, DisplayNameFor(stat.StatKey.Key), icon, ValueToString(stat.StatKey.Key, value));
        }

        ValueComparisonLineController controller = new ValueComparisonLineController(col, withName, withIcon);
        controller.SetData(statLine, owner, tooltipController, tooltipPosition);

        _controllers.Add(stat.StatKey.Key, controller);
        statIndex++;
      }

      parent.Add(_container);
    }

    public ValueComparisonLineController GetStatLineController(string key)
    {
      return _controllers.TryGetValue(key, out var controller) ? controller : null;
    }

    public void Update(ItemStatData statData)
    {
      foreach (var stat in statData.Stats)
      {
        if (_controllers.TryGetValue(stat.StatKey.Key, out var controller))
        {
          controller.SetOldValue(ValueToString(stat.StatKey.Key, stat.Value));
        }
      }
    }

    public void Hide()
    {
      _container.style.display = DisplayStyle.None;
    }
    public void Show()
    {
      _container.style.display = DisplayStyle.Flex;
    }

    private string ValueToString(string key, float value)
    {
      if (_beautify != null && _beautify.TryGetValue(key, out var formatter))
      {
        return formatter(value);
      }
      return value.ToString();
    }

    private string DisplayNameFor(string key)
    {
      if (_resolveStatDisplayName == null)
        return key;
      string resolved = _resolveStatDisplayName(key);
      return string.IsNullOrEmpty(resolved) ? key : resolved;
    }

    public void SetComparison(ItemStatData statData, ItemStatData comparison)
    {
      foreach (var stat in statData.Stats)
      {
        if (!_controllers.TryGetValue(stat.StatKey.Key, out var controller)) continue;

        float value = stat.Value;
        float comparisonValue = comparison.Get(stat.StatKey.Key);
        if (Mathf.Approximately(value, 0) && Mathf.Approximately(comparisonValue, 0))
        {
          continue;
        }

        string valueStr = null;
        ValueDeltaType deltaType = ValueDeltaUtility.Get(value, comparisonValue);
        if (deltaType != ValueDeltaType.NEUTRAL)
        {
          valueStr = ValueToString(stat.StatKey.Key, value);
        }

        Sprite icon = null;
        if (_icons != null && _icons.TryGetValue(stat.StatKey.Key, out var i))
        {
          icon = i;
        }

        ValueComparisonLine statLine = new ValueComparisonLine(stat.StatKey.Key, DisplayNameFor(stat.StatKey.Key), icon, ValueToString(stat.StatKey.Key, comparisonValue), valueStr, deltaType, false);

        controller.SetData(statLine, _owner, _tooltipController, _tooltipPosition);
      }
    }

    public void HideNewValue()
    {
      foreach (ValueComparisonLineController controller in _controllers.Values)
      {
        controller.HideNewValue();
      }
    }
  }
}