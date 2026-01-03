using System;

namespace TNL_DPS_Meter.Models
{
    /// <summary>
    /// Represents a single damage entry from the game's combat log
    /// </summary>
    public class CombatEntry
    {
        /// <summary>
        /// Event timestamp
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Damage dealt
        /// </summary>
        public long Damage { get; set; }

        /// <summary>
        /// Whether it was a critical hit
        /// </summary>
        public bool IsCrit { get; set; }

        /// <summary>
        /// Whether it was a heavy hit
        /// </summary>
        public bool IsHeavy { get; set; }

        /// <summary>
        /// Ability name
        /// </summary>
        public string AbilityName { get; set; } = string.Empty;

        /// <summary>
        /// Damage calculation descriptor
        /// </summary>
        public string CalculationDescriptor { get; set; } = string.Empty;

        /// <summary>
        /// Player name
        /// </summary>
        public string PlayerName { get; set; } = string.Empty;

        /// <summary>
        /// Target name
        /// </summary>
        public string TargetName { get; set; } = string.Empty;
    }
}
