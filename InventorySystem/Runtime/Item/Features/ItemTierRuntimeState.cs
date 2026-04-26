using System;
using CupkekGames.Data;
using UnityEngine;

namespace CupkekGames.InventorySystem
{
    /// <summary>
    /// Optional runtime tier override for a stack. When <see cref="TierOverride"/> is empty, <see cref="ItemTierFeature"/> uses the definition tier.
    /// </summary>
    [Serializable]
    public sealed class ItemTierRuntimeState : IFeatureStateData
    {
        [SerializeField]
        [CatalogKeyConstraint(InventoryConstants.ItemTierCatalogId)]
        public CatalogKey TierOverride;

        public IFeatureStateData CloneState()
        {
            return new ItemTierRuntimeState { TierOverride = TierOverride };
        }
    }
}
