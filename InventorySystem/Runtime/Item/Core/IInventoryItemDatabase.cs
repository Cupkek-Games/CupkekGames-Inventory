using CupkekGames.Data;
using UnityEngine;
using System.Collections.Generic;

namespace CupkekGames.InventorySystem
{
    public interface IInventoryItemDatabase
    {
        InventoryItemDefinition GetItemDefinition(string key);
        InventoryItemDefinition GetItemDefinition(CatalogKey itemKey);

        InventoryItemDefinition GetItemDefinition(InventoryItemReference item) => GetItemDefinition(item.ItemKey);

        InventoryItemDefinition GetItemDefinition(InventoryItem item) => GetItemDefinition(item.Key);

        IEnumerable<string> GetAllItemKeys();
        Sprite GetItemIcon(InventoryItem item);
        InventoryItem CreateItem(InventoryItemReference itemReference);
    }
}
