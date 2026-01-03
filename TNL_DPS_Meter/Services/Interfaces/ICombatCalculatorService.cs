using System;
using TNL_DPS_Meter.Models;

namespace TNL_DPS_Meter.Services.Interfaces
{
    /// <summary>
    /// Interface for combat statistics calculation service
    /// </summary>
    public interface ICombatCalculatorService
    {
        /// <summary>
        /// Formats number for display (adds k for large numbers)
        /// </summary>
        /// <param name="number">Number to format</param>
        /// <returns>Formatted string</returns>
        string FormatNumber(long number);

        /// <summary>
        /// Formats DPS for display
        /// </summary>
        /// <param name="dps">DPS value</param>
        /// <returns>Formatted string</returns>
        string FormatDps(double dps);

        /// <summary>
        /// Formats time span
        /// </summary>
        /// <param name="timeSpan">Time span</param>
        /// <returns>Formatted time string</returns>
        string FormatTimeSpan(TimeSpan timeSpan);

        /// <summary>
        /// Calculates DPS for combat session
        /// </summary>
        /// <param name="damage">Total damage</param>
        /// <param name="startTime">Start time</param>
        /// <param name="endTime">End time</param>
        /// <param name="gapSeconds">Gap seconds</param>
        /// <returns>DPS or 0 if calculation is impossible</returns>
        double CalculateDps(long damage, DateTime startTime, DateTime endTime, double gapSeconds);

        /// <summary>
        /// Determines if player is in combat based on last action time
        /// </summary>
        /// <param name="lastActionTime">Last action time</param>
        /// <param name="currentTime">Current time</param>
        /// <returns>true if player is in combat</returns>
        bool IsInCombat(DateTime lastActionTime, DateTime currentTime);

        /// <summary>
        /// Determines if new combat started after pause
        /// </summary>
        /// <param name="wasInCombat">Was player in combat before</param>
        /// <param name="timeSinceLastEntry">Time since last action</param>
        /// <returns>true if new combat started</returns>
        bool IsNewCombatStarted(bool wasInCombat, TimeSpan timeSinceLastEntry);
    }
}
