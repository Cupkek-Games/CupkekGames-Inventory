using System;
using System.Collections.Generic;
using CupkekGames.Data;
using CupkekGames.Data.Primitives;
using UnityEngine;
using UnityEngine.UIElements;

namespace CupkekGames.InventorySystem
{
    [Serializable]
    public class InventoryItem
    {
        [SerializeField]
        [CatalogKeyConstraint(InventoryConstants.ItemsCatalogId, typeof(ScriptableObject))]
        public CatalogKey Key;
        [SerializeField] public SerializedGuid ID = SerializedGuid.NewGUID();
        [SerializeField] public int Amount = 1;
        [SerializeField] public int BaseMaxStackAmount = 1;

        [SerializeReference] private List<IFeatureStateData> _featureState = new();

        public InventoryItem()
        {
        }

        public InventoryItem(InventoryItemReference reference)
        {
            Key = reference.ItemKey;
            ID = SerializedGuid.NewGUID();
            SetAmount(reference.Amount);
        }

        public InventoryItem(string key)
        {
            Key = new CatalogKey { Catalog = InventoryConstants.ItemsCatalogId, Key = key };
            ID = SerializedGuid.NewGUID();
        }

        public InventoryItem(string key, Guid id)
        {
            Key = new CatalogKey { Catalog = InventoryConstants.ItemsCatalogId, Key = key };
            ID = new SerializedGuid(id);
        }

        public InventoryItem(string key, int maxStackAmount)
        {
            Key = new CatalogKey { Catalog = InventoryConstants.ItemsCatalogId, Key = key };
            ID = SerializedGuid.NewGUID();
            BaseMaxStackAmount = maxStackAmount;
        }

        public InventoryItem(string key, int maxStackAmount, Guid id)
        {
            Key = new CatalogKey { Catalog = InventoryConstants.ItemsCatalogId, Key = key };
            ID = new SerializedGuid(id);
            BaseMaxStackAmount = maxStackAmount;
        }

        public InventoryItem(InventoryItem other, bool sameId = false)
        {
            if (other == null)
                return;
            Key = other.Key;
            ID = sameId ? other.ID : SerializedGuid.NewGUID();
            Amount = other.Amount;
            BaseMaxStackAmount = other.BaseMaxStackAmount;
            _featureState = new List<IFeatureStateData>();
            if (other._featureState != null)
            {
                for (int i = 0; i < other._featureState.Count; i++)
                {
                    IFeatureStateData s = other._featureState[i];
                    if (s != null)
                        _featureState.Add(s.CloneState());
                }
            }
        }

        public T GetState<T>() where T : class, IFeatureStateData
        {
            for (int i = 0; i < _featureState.Count; i++)
            {
                if (_featureState[i] is T typed)
                    return typed;
            }

            return null;
        }

        public T GetOrCreateState<T>() where T : class, IFeatureStateData, new()
        {
            T existing = GetState<T>();
            if (existing != null)
                return existing;
            T state = new T();
            _featureState.Add(state);
            return state;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            InventoryItem other = (InventoryItem)obj;
            return ID == other.ID;
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }

        public virtual bool CanStackWith(InventoryItem other)
        {
            if (other == null)
                return false;

            return Key == other.Key;
        }

        public int AddAmount(int add)
        {
            int max = GetMaxStackAmount();

            int available = max - Amount;

            if (add > 0)
            {
                if (add <= available)
                {
                    Amount += add;

                    return 0;
                }

                Amount = max;

                return add - available;
            }
            else if (add < 0)
            {
                int removable = Math.Min(-add, Amount);

                Amount -= removable;

                return add + removable;
            }

            return 0;
        }


        public int AvailableStackSpace()
        {
            return GetMaxStackAmount() - Amount;
        }

        public void SetAmount(int amount)
        {
            Amount = Mathf.Clamp(amount, 0, GetMaxStackAmount());
        }

        public virtual InventoryItem Clone(bool sameID) => new InventoryItem(this, sameID);
        public virtual int GetMaxStackAmount() => BaseMaxStackAmount;

        public virtual void BuildItemTooltipStats(VisualElement statsHost, in ItemTooltipContext context)
        {
            if (statsHost == null || context.Definition == null)
                return;

            foreach (IFeature feature in context.Definition.Features)
            {
                if (feature is IItemFeature itemFeature)
                    itemFeature.BuildTooltipContent(statsHost, in context);
            }
        }

        public virtual string DisplayName(InventoryItemDefinition itemDefinition)
        {
            if (itemDefinition != null && !string.IsNullOrEmpty(itemDefinition.Name))
                return itemDefinition.Name;

            return !Key.IsEmpty ? Key.Key : "Unknown Item";
        }
    }
}
