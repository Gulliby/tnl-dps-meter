using System.Collections.Generic;
using TNL_DPS_Meter.Models;

namespace TNL_DPS_Meter.Services.Interfaces
{
    /// <summary>
    /// Interface for combat log parsing service
    /// </summary>
    public interface ICombatLogParserService
    {
        /// <summary>
        /// Parses combat log file and returns structured data
        /// </summary>
        /// <param name="filePath">Path to the log file</param>
        /// <returns>Combat log data or null on error</returns>
        CombatData? ParseCombatLog(string filePath);

        /// <summary>
        /// Parses CSV content of combat log
        /// </summary>
        /// <param name="content">File content as string</param>
        /// <param name="combatData">Object to fill with data</param>
        /// <param name="lastProcessedLineCount">Number of already processed lines</param>
        void ParseCombatLogCsv(string content, CombatData combatData, ref int lastProcessedLineCount);

        /// <summary>
        /// Calculates gaps between actions in combat log
        /// </summary>
        /// <param name="combatData">Combat log data</param>
        void CalculateGaps(CombatData combatData);

        /// <summary>
        /// Calculates gaps for the last combat
        /// </summary>
        /// <param name="combatData">Combat log data</param>
        /// <returns>Number of gap seconds</returns>
        double CalculateLastCombatGaps(CombatData combatData);
    }
}
