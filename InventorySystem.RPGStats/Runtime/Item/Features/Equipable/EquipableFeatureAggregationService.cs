using System.Collections.Generic;
using CupkekGames.InventorySystem;
using CupkekGames.RPGStats;
using CupkekGames.Services;
using UnityEngine;

namespace CupkekGames.InventorySystem.RPGStats
{
    public interface IEquipableFeatureAggregationService
    {
        ItemStatData BuildAggregatedStats(ItemStatData baseStats, IEnumerable<EquipableFeature> equippedFeatures);
    }

    public class EquipableFeatureAggregationService : IEquipableFeatureAggregationService
    {
        public ItemStatData BuildAggregatedStats(ItemStatData baseStats, IEnumerable<EquipableFeature> equippedFeatures)
        {
            var baseByKey = new Dictionary<string, float>();
            var result = new ItemStatData();

            if (baseStats?.Stats != null)
            {
                foreach (ItemStatValue stat in baseStats.Stats)
                {
                    if (stat.StatKey.IsEmpty)
                        continue;

                    baseByKey[stat.StatKey.Key] = stat.Value;
                    result.Set(stat.StatKey.Key, stat.Value);
                }
            }

            if (equippedFeatures == null)
                return result;

            foreach (EquipableFeature feature in equippedFeatures)
            {
                if (feature?.Effects?.Entries == null)
                    continue;

                foreach (AttributeEffectEntry entry in feature.Effects.Entries)
                {
                    if (entry == null || entry.AttributeKey.IsEmpty)
                        continue;

                    AttributeDefinitionSO def = AttributeDefinitionResolver.TryGet(entry.AttributeKey.Key);
                    if (def == null)
                        continue;

                    string key = entry.AttributeKey.Key;

                    float baseValue = GetBaseValue(baseByKey, def, key);
                    float currentValue = result.Get(key);
                    float deltaFromBase = currentValue - baseValue;
                    float recomputed = (baseValue + deltaFromBase) * entry.Multiplier + entry.Additive;

                    if (def.RoundToInteger)
                        recomputed = Mathf.RoundToInt(recomputed);

                    result.Set(key, recomputed);
                }
            }

            return result;
        }

        private static float GetBaseValue(Dictionary<string, float> baseByKey, AttributeDefinitionSO attribute, string statKey)
        {
            if (attribute == null)
                return 0f;

            if (baseByKey.TryGetValue(statKey, out float value))
                return value;

            if (!string.IsNullOrEmpty(attribute.DisplayName) && baseByKey.TryGetValue(attribute.DisplayName, out value))
                return value;

            return attribute.DefaultBaseValue;
        }
    }
}
