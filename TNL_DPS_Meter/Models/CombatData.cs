using System;
using System.Collections.Generic;

namespace TNL_DPS_Meter.Models
{
    /// <summary>
    /// Current state data of the combat log
    /// </summary>
    public class CombatData
    {
        /// <summary>
        /// Log file creation time
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Last action time in the log
        /// </summary>
        public DateTime LastActionTime { get; set; }

        /// <summary>
        /// First action time in the log
        /// </summary>
        public DateTime FirstActionTime { get; set; }

        /// <summary>
        /// List of all action timestamps
        /// </summary>
        public List<DateTime> ActionTimestamps { get; set; } = new List<DateTime>();

        /// <summary>
        /// List of all combat entries
        /// </summary>
        public List<CombatEntry> CombatEntries { get; set; } = new List<CombatEntry>();

        /// <summary>
        /// Total number of gap seconds in the log
        /// </summary>
        public double OverallGapSeconds { get; set; }

        /// <summary>
        /// Total damage dealt
        /// </summary>
        public long TotalDamage { get; set; }

        // For Last Combat - statistics for new data

        /// <summary>
        /// Last combat damage
        /// </summary>
        public long LastCombatDamage { get; set; }

        /// <summary>
        /// Last combat start time
        /// </summary>
        public DateTime LastCombatFirstTime { get; set; }

        /// <summary>
        /// Last combat end time
        /// </summary>
        public DateTime LastCombatLastTime { get; set; }

        /// <summary>
        /// Last combat entries
        /// </summary>
        public List<CombatEntry> LastCombatEntries { get; set; } = new List<CombatEntry>();
    }
}
