using UnityEngine;
using UnityEngine.UIElements;

namespace CupkekGames.InventorySystem
{
    /// <summary>
    /// Applies and clears <see cref="IItemSlotVisual"/> layers on a slot root. Stateless; safe to share across controllers.
    /// </summary>
    public sealed class InventoryItemSlotFeatureVisuals
    {
        public void Clear(VisualElement parent, InventoryItemDefinition definition)
        {
            if (parent == null || definition?.Features == null)
                return;

            for (int i = 0; i < definition.Features.Count; i++)
            {
                if (definition.Features[i] is IItemSlotVisual visual)
                    visual.ClearSlotVisual(parent);
            }
        }

        public void Apply(VisualElement parent, IInventoryItemDatabase database, InventoryItem item,
            InventoryItemDefinition definition, GameObject owner)
        {
            if (parent == null || definition?.Features == null)
                return;

            var ctx = new ItemSlotContext(database, item, definition, owner);
            for (int i = 0; i < definition.Features.Count; i++)
            {
                if (definition.Features[i] is IItemSlotVisual visual)
                    visual.ApplySlotVisual(parent, in ctx);
            }
        }
    }
}
