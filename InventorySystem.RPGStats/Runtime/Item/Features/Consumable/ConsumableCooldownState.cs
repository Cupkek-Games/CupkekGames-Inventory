using System;
using CupkekGames.Data;
using UnityEngine;

namespace CupkekGames.InventorySystem.RPGStats
{
    /// <summary>
    /// Runtime-only consumable use timing. <see cref="ConsumableFeature.CooldownDuration"/> stays on the definition feature.
    /// </summary>
    [Serializable]
    public class ConsumableCooldownState : IFeatureStateData
    {
        [NonSerialized] public float LastUsedTime;

        public IFeatureStateData CloneState()
        {
            return new ConsumableCooldownState { LastUsedTime = LastUsedTime };
        }
    }
}
