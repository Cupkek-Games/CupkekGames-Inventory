using System;
using System.Collections.Generic;
using CupkekGames.Data;
using CupkekGames.InventorySystem;
using CupkekGames.RPGStats;
using UnityEngine;
using UnityEngine.UIElements;

namespace CupkekGames.InventorySystem.RPGStats
{
    [Serializable]
    [FeatureGroup("Equipment")]
    public class EquipableFeature : IItemFeature, IItemSlotVisual
    {
        public EquipableFeature() { }

        [CatalogKeyConstraint("EquipmentType")]
        public CatalogKey EquipmentType;

        public AttributeEffect Effects = new();

        public void BuildTooltipContent(VisualElement host, in ItemTooltipContext context)
        {
            if (host == null || Effects == null || Effects.Entries == null)
                return;

            ItemStatData comparisonStats = null;
            if (context.GetComparison != null && context.Item != null && context.Definition != null)
                comparisonStats = context.GetComparison(context.Item, context.Definition);

            foreach (AttributeEffectEntry entry in Effects.Entries)
            {
                if (entry == null || entry.AttributeKey.IsEmpty)
                    continue;

                AttributeDefinitionSO def = AttributeDefinitionResolver.TryGet(entry.AttributeKey.Key);
                if (def == null)
                    continue;

                float add = entry.Additive;
                float multiply = entry.Multiplier;
                if (Mathf.Approximately(add, 0f) && Mathf.Approximately(multiply, 1f))
                    continue;

                string attributeKey = entry.AttributeKey.Key;
                string attributeDisplayName = string.IsNullOrEmpty(def.DisplayName)
                    ? attributeKey
                    : def.DisplayName;
                if (context.AttributeNames != null && context.AttributeNames.Count > 0 &&
                    !context.AttributeNames.Contains(attributeKey) &&
                    !context.AttributeNames.Contains(attributeDisplayName))
                    continue;

                string text = Effects.GetStringEffect(entry.AttributeKey.Key) ?? $"{attributeDisplayName}: {add}";

                float sign = add + (multiply - 1f);
                Color textColor = Mathf.Approximately(sign, 0f)
                    ? Color.white
                    : (sign > 0f ? Color.green : Color.red);

                if (comparisonStats != null)
                {
                    float comparisonValue = comparisonStats.Get(attributeKey);
                    if (Mathf.Approximately(comparisonValue, 0f) && !string.IsNullOrEmpty(attributeDisplayName))
                        comparisonValue = comparisonStats.Get(attributeDisplayName);
                    float delta = add - comparisonValue;
                    if (!Mathf.Approximately(delta, 0f))
                    {
                        string deltaText = delta > 0 ? $"+{delta}" : $"{delta}";
                        text = $"{text} ({deltaText})";
                    }

                    if (Mathf.Approximately(delta, 0f))
                    {
                        textColor = Color.white;
                    }
                    else
                    {
                        textColor = delta > 0 ? Color.green : Color.red;
                    }
                }

                Label label = new Label(text);
                label.AddToClassList("text-xl");
                label.AddToClassList("mt-0");
                label.AddToClassList("pt-0");
                label.style.color = textColor;
                host.Add(label);
            }
        }

        public static ItemStatData ToItemStatData(AttributeEffect effects)
        {
            var result = new ItemStatData();
            if (effects?.Entries == null)
                return result;

            foreach (AttributeEffectEntry entry in effects.Entries)
            {
                if (entry == null || entry.AttributeKey.IsEmpty)
                    continue;

                result.Set(entry.AttributeKey.Key, entry.Additive);
            }

            return result;
        }

        public void ApplySlotVisual(VisualElement slotRoot, in ItemSlotContext context)
        {
        }

        public void ClearSlotVisual(VisualElement slotRoot)
        {
            if (slotRoot == null)
                return;

            List<VisualElement> stars = slotRoot.Query<VisualElement>("Star").ToList();
            for (int i = 0; i < stars.Count; i++)
                stars[i].style.visibility = Visibility.Hidden;
        }

        public EquipableFeature(EquipableFeature other)
        {
            if (other == null)
                return;
            EquipmentType = other.EquipmentType;
            Effects = other.Effects != null ? new AttributeEffect(other.Effects) : new AttributeEffect();
        }

        public IFeature CloneFeature() => new EquipableFeature(this);
    }
}
