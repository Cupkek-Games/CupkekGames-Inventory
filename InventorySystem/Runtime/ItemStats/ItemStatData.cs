
using System;
using System.Collections.Generic;
using System.Linq;
using CupkekGames.Data;
using UnityEngine;

namespace CupkekGames.InventorySystem
{
    [Serializable]
    public struct ItemStatValue
    {
        [CatalogKeyConstraint(InventoryConstants.ItemStatsCatalogId)]
        public CatalogKey StatKey;
        public float Value;

        public ItemStatValue(CatalogKey statKey, float value)
        {
            StatKey = statKey;
            Value = value;
        }

        public ItemStatValue(string key, float value)
        {
            StatKey = new CatalogKey { Catalog = InventoryConstants.ItemStatsCatalogId, Key = key };
            Value = value;
        }
    }

    [Serializable]
    public class ItemStatData
    {
        [SerializeField] protected List<ItemStatValue> _stats = new();

        public int Count => _stats.Count;
        public IReadOnlyList<ItemStatValue> Stats => _stats;

        public float Get(string key)
        {
            for (int i = 0; i < _stats.Count; i++)
            {
                if (_stats[i].StatKey.Key == key)
                    return _stats[i].Value;
            }

            return 0;
        }

        public void Set(string key, float value)
        {
            for (int i = 0; i < _stats.Count; i++)
            {
                if (_stats[i].StatKey.Key == key)
                {
                    _stats[i] = new ItemStatValue(_stats[i].StatKey, value);
                    return;
                }
            }

            _stats.Add(new ItemStatValue(key, value));
        }

        public bool Has(string key)
        {
            for (int i = 0; i < _stats.Count; i++)
            {
                if (_stats[i].StatKey.Key == key)
                    return true;
            }

            return false;
        }

        public void Copy(ItemStatData other)
        {
            _stats = other._stats.Select(a => new ItemStatValue(a.StatKey, a.Value)).ToList();
        }

        public void Add(ItemStatData other)
        {
            foreach (var stat in other._stats)
            {
                float current = Get(stat.StatKey.Key);
                Set(stat.StatKey.Key, current + stat.Value);
            }
        }

        public void Remove(ItemStatData other)
        {
            foreach (var stat in other._stats)
            {
                float current = Get(stat.StatKey.Key);
                Set(stat.StatKey.Key, current - stat.Value);
            }
        }

        public void Clear()
        {
            _stats.Clear();
        }

        public void MultiplyAll(float multiplier)
        {
            for (int i = 0; i < _stats.Count; i++)
            {
                var stat = _stats[i];
                _stats[i] = new ItemStatValue(stat.StatKey, stat.Value * multiplier);
            }
        }
    }
}
