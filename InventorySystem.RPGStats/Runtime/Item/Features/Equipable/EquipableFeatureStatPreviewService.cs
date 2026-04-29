using CupkekGames.Luna;
using CupkekGames.RPGStats;
using CupkekGames.Services;
using UnityEngine;

namespace CupkekGames.InventorySystem.RPGStats
{
    public readonly struct EquipableFeatureStatPreview
    {
        public readonly float CurrentValue;
        public readonly float ProjectedValue;
        public readonly ValueDeltaType ChangeType;

        public EquipableFeatureStatPreview(float currentValue, float projectedValue)
        {
            CurrentValue = currentValue;
            ProjectedValue = projectedValue;
            ChangeType = ValueDeltaUtility.Get(projectedValue, currentValue);
        }
    }

    public interface IEquipableFeatureStatPreviewService
    {
        EquipableFeatureStatPreview PreviewEquip(
            float currentValue,
            string attributeKey,
            EquipableFeature currentlyEquippedFeature,
            EquipableFeature newFeature);

        EquipableFeatureStatPreview PreviewUnequip(
            float currentValue,
            string attributeKey,
            EquipableFeature currentlyEquippedFeature);
    }

    public class EquipableFeatureStatPreviewService : IEquipableFeatureStatPreviewService
    {
        public EquipableFeatureStatPreview PreviewEquip(
            float currentValue,
            string attributeKey,
            EquipableFeature currentlyEquippedFeature,
            EquipableFeature newFeature)
        {
            float baseValue = RemoveEffect(currentValue, currentlyEquippedFeature, attributeKey);
            float projected = ApplyEffect(baseValue, newFeature, attributeKey);
            return new EquipableFeatureStatPreview(currentValue, projected);
        }

        public EquipableFeatureStatPreview PreviewUnequip(
            float currentValue,
            string attributeKey,
            EquipableFeature currentlyEquippedFeature)
        {
            float projected = RemoveEffect(currentValue, currentlyEquippedFeature, attributeKey);
            return new EquipableFeatureStatPreview(currentValue, projected);
        }

        private static float ApplyEffect(float value, EquipableFeature feature, string attributeKey)
        {
            if (!TryGetEffect(feature, attributeKey, out float add, out float mult))
                return value;

            return (value * mult) + add;
        }

        private static float RemoveEffect(float value, EquipableFeature feature, string attributeKey)
        {
            if (!TryGetEffect(feature, attributeKey, out float add, out float mult))
                return value;

            // Inverse of (base * mult) + add. Avoid divide-by-zero for invalid authored multipliers.
            if (Mathf.Approximately(mult, 0f))
                return value - add;

            return (value - add) / mult;
        }

        private static bool TryGetEffect(EquipableFeature feature, string attributeKey, out float add, out float mult)
        {
            add = 0f;
            mult = 1f;
            if (feature?.Effects?.Entries == null || string.IsNullOrEmpty(attributeKey))
                return false;

            foreach (var entry in feature.Effects.Entries)
            {
                if (entry == null || entry.AttributeKey.IsEmpty)
                    continue;

                if (entry.AttributeKey.Key == attributeKey)
                {
                    add = entry.Additive;
                    mult = entry.Multiplier;
                    return true;
                }

                AttributeDefinitionSO def = AttributeDefinitionResolver.TryGet(entry.AttributeKey.Key);
                if (def != null && (def.name == attributeKey || def.DisplayName == attributeKey))
                {
                    add = entry.Additive;
                    mult = entry.Multiplier;
                    return true;
                }
            }

            return false;
        }
    }
}
