

using System;
using System.Collections.Generic;
using CupkekGames.Data;
using UnityEngine;

namespace CupkekGames.InventorySystem
{
    [Serializable]
    public class InventoryItemDefinition : IData
    {
        public InventoryItemDefinition() { }

        public string Name = "";
        public string Description = "";
        [CatalogKeyConstraint(InventoryConstants.IconsCatalogId, typeof(Sprite))]
        public CatalogKey IconKey;
        public int MaxStackAmount = 1;

        [SerializeReference]
        public List<IItemFeature> Features = new();

        public T GetFeature<T>() where T : class, IFeature
        {
            for (int i = 0; i < Features.Count; i++)
                if (Features[i] is T typed)
                    return typed;
            return null;
        }

        public bool HasFeature<T>() where T : class, IFeature
            => GetFeature<T>() != null;

        public IEnumerable<T> GetFeatures<T>() where T : class, IFeature
        {
            for (int i = 0; i < Features.Count; i++)
                if (Features[i] is T typed)
                    yield return typed;
        }

        public virtual bool Validate() => !string.IsNullOrEmpty(Name);
        public virtual void OnAfterDeserialize() { }

        public InventoryItemDefinition(InventoryItemDefinition other)
        {
            if (other == null)
                return;
            Name = other.Name;
            Description = other.Description;
            IconKey = other.IconKey;
            MaxStackAmount = other.MaxStackAmount;
            Features = new List<IItemFeature>();
            for (int i = 0; i < other.Features.Count; i++)
            {
                IItemFeature f = other.Features[i];
                if (f != null)
                    Features.Add((IItemFeature)f.CloneFeature());
            }
        }

        public virtual IData CloneData() => new InventoryItemDefinition(this);
    }
}
