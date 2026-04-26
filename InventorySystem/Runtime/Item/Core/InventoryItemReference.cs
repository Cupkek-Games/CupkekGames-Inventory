

using System;
using CupkekGames.Data;
using UnityEngine;

namespace CupkekGames.InventorySystem
{
    [Serializable]
    public class InventoryItemReference
    {
        [CatalogKeyConstraint(InventoryConstants.ItemsCatalogId, typeof(ScriptableObject))]
        public CatalogKey ItemKey;
        public int Amount;

        public InventoryItemReference(CatalogKey itemKey, int amount)
        {
            ItemKey = itemKey;
            Amount = amount;
        }

        public InventoryItemReference(string key, int amount)
        {
            ItemKey = new CatalogKey { Catalog = InventoryConstants.ItemsCatalogId, Key = key };
            Amount = amount;
        }
    }
}
