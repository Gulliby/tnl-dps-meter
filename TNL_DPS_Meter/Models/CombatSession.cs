using System;
using System.Collections.Generic;

namespace TNL_DPS_Meter.Models
{
    /// <summary>
    /// Represents a combat session with full statistics
    /// </summary>
    public class CombatSession
    {
        /// <summary>
        /// Target name (with number for differentiation)
        /// </summary>
        public string TargetName { get; set; } = string.Empty;

        /// <summary>
        /// Total damage dealt
        /// </summary>
        public long Damage { get; set; }

        /// <summary>
        /// Combat start time
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Combat end time
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// Combat duration (computed property)
        /// </summary>
        public TimeSpan CombatDuration => EndTime - StartTime;

        /// <summary>
        /// Number of gap seconds during combat
        /// </summary>
        public double GapSeconds { get; set; }

        /// <summary>
        /// List of all damage entries in this session
        /// </summary>
        public List<CombatEntry> Entries { get; set; } = new List<CombatEntry>();
    }
}
