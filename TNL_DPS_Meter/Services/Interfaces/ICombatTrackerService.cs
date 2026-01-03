using System.Collections.Generic;
using TNL_DPS_Meter.Models;

namespace TNL_DPS_Meter.Services.Interfaces
{
    /// <summary>
    /// Interface for combat state tracking service
    /// </summary>
    public interface ICombatTrackerService
    {
        /// <summary>
        /// Current combat state
        /// </summary>
        bool IsInCombat { get; }

        /// <summary>
        /// Current combat start time
        /// </summary>
        System.DateTime CombatStartTime { get; }

        /// <summary>
        /// Current combat damage
        /// </summary>
        long CurrentCombatDamage { get; }

        /// <summary>
        /// Total damage for session
        /// </summary>
        long TotalDamage { get; }

        /// <summary>
        /// Combat sessions history
        /// </summary>
        IReadOnlyList<CombatSession> CombatHistory { get; }

        /// <summary>
        /// Last combat session
        /// </summary>
        CombatSession? LastCombatSession { get; }

        /// <summary>
        /// Last combat entries
        /// </summary>
        IReadOnlyList<CombatEntry> LastCombatEntries { get; }

        /// <summary>
        /// Initializes combat tracking
        /// </summary>
        void Initialize();

        /// <summary>
        /// Updates state based on new combat log data
        /// </summary>
        /// <param name="combatData">New combat log data</param>
        /// <param name="currentTime">Current time</param>
        void UpdateFromCombatData(CombatData combatData, System.DateTime currentTime);

        /// <summary>
        /// Saves current combat session to history
        /// </summary>
        void SaveCurrentSessionToHistory();

        /// <summary>
        /// Clears combat sessions history
        /// </summary>
        void ClearHistory();

        /// <summary>
        /// Gets combat session by target name
        /// </summary>
        /// <param name="targetName">Target name</param>
        /// <returns>Combat session or null</returns>
        CombatSession? GetSessionByTargetName(string targetName);

        /// <summary>
        /// Gets last combat display data
        /// </summary>
        /// <returns>Tuple with data (damage, dps, duration)</returns>
        (long damage, double dps, System.TimeSpan duration) GetLastCombatDisplayData();

        /// <summary>
        /// Gets overall damage display data
        /// </summary>
        /// <param name="firstActionTime">First action time</param>
        /// <param name="lastActionTime">Last action time</param>
        /// <param name="overallGapSeconds">Gap seconds</param>
        /// <returns>Tuple with data (damage, dps, duration)</returns>
        (long damage, double dps, System.TimeSpan duration) GetOverallDisplayData(System.DateTime firstActionTime, System.DateTime lastActionTime, double overallGapSeconds);
    }
}
