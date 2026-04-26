using System;
using CupkekGames.Data;
using CupkekGames.InventorySystem;
using UnityEngine;
using UnityEngine.UIElements;

namespace CupkekGames.InventorySystem.RPGStats
{
    /// <summary>
    /// Marker for consumable-category equipment slots (<see cref="RpgConsumableCategoryEquipmentSlotAcceptPolicy"/>).
    /// Includes a small default tooltip (demo UX); subclass and override <see cref="BuildTooltipContent"/> (optionally call
    /// <c>base.BuildTooltipContent</c>), <see cref="ApplySlotVisual"/> / <see cref="ClearSlotVisual"/>, and <see cref="CloneFeature"/> when you add state.
    /// </summary>
    [Serializable]
    [FeatureGroup("Consumable")]
    public class ConsumableFeature : IItemFeature, IItemSlotVisual
    {
        /// <summary>Seconds between uses; authored on the item definition. Runtime timing lives on <see cref="ConsumableCooldownState"/>.</summary>
        public float CooldownDuration = 5f;

        public ConsumableFeature()
        {
        }

        /// <summary>For subclass copy constructors (<c>public MyConsumable(MyConsumable o) : base(o)</c>).</summary>
        protected ConsumableFeature(ConsumableFeature other)
        {
            if (other != null)
                CooldownDuration = other.CooldownDuration;
        }

        public virtual void BuildTooltipContent(VisualElement host, in ItemTooltipContext context)
        {
            if (host == null)
                return;

            var kind = new Label("Consumable");
            kind.AddToClassList("text-xl");
            kind.AddToClassList("mt-0");
            kind.AddToClassList("pt-0");
            kind.style.color = new Color(0.65f, 0.88f, 0.95f);
            host.Add(kind);

            if (context.Item == null)
                return;

            int max = context.Item.GetMaxStackAmount();
            if (max <= 1)
                return;

            var stack = new Label($"Stack: {context.Item.Amount} / {max}");
            stack.AddToClassList("text-xl");
            stack.AddToClassList("mt-0");
            stack.AddToClassList("pt-0");
            stack.style.color = Color.white;
            host.Add(stack);

            if (CooldownDuration > 0f)
            {
                ConsumableCooldownState cooldownState = context.Item.GetState<ConsumableCooldownState>();
                if (cooldownState != null)
                {
                    float elapsed = Time.time - cooldownState.LastUsedTime;
                    if (elapsed < CooldownDuration)
                    {
                        float remaining = CooldownDuration - elapsed;
                        var cd = new Label($"Cooldown: {remaining:F1}s");
                        cd.AddToClassList("text-xl");
                        cd.AddToClassList("mt-0");
                        cd.AddToClassList("pt-0");
                        cd.style.color = Color.yellow;
                        host.Add(cd);
                    }
                }
            }
        }

        public virtual void ApplySlotVisual(VisualElement slotRoot, in ItemSlotContext context)
        {
        }

        public virtual void ClearSlotVisual(VisualElement slotRoot)
        {
        }

        public virtual IFeature CloneFeature() => new ConsumableFeature(this);
    }
}
