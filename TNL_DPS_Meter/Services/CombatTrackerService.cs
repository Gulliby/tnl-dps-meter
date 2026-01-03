using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TNL_DPS_Meter.Core;
using TNL_DPS_Meter.Models;
using TNL_DPS_Meter.Services.Interfaces;

namespace TNL_DPS_Meter.Services
{
    /// <summary>
    /// Service for tracking combat state and managing combat sessions history
    /// </summary>
    public class CombatTrackerService : ICombatTrackerService
    {
        private readonly List<CombatSession> _combatHistory = new List<CombatSession>();
        private readonly List<CombatEntry> _lastCombatEntries = new List<CombatEntry>();
        private readonly ICombatCalculatorService _calculatorService;

        public CombatTrackerService(ICombatCalculatorService calculatorService)
        {
            _calculatorService = calculatorService ?? throw new ArgumentNullException(nameof(calculatorService));
        }

        public bool IsInCombat { get; private set; }
        public DateTime CombatStartTime { get; private set; }
        public long CurrentCombatDamage { get; private set; }
        public long TotalDamage { get; private set; }
        public IReadOnlyList<CombatSession> CombatHistory => _combatHistory.AsReadOnly();
        public IReadOnlyList<CombatEntry> LastCombatEntries => _lastCombatEntries.AsReadOnly();

        public CombatSession? LastCombatSession => _combatHistory.LastOrDefault();

        // Data for Last Combat
        private long _lastCombatDamage;
        private DateTime _lastCombatFirstTime = DateTime.MinValue;
        private DateTime _lastCombatLastTime = DateTime.MinValue;
        private double _lastCombatGapSeconds;

        public void Initialize()
        {
            CombatStartTime = DateTime.Now;
            CurrentCombatDamage = 0;
            TotalDamage = 0;
            IsInCombat = false;
            _lastCombatDamage = 0;
            _lastCombatFirstTime = DateTime.MinValue;
            _lastCombatLastTime = DateTime.MinValue;
            _lastCombatGapSeconds = 0;
            _lastCombatEntries.Clear();
        }

        public void UpdateFromCombatData(CombatData combatData, DateTime currentTime)
        {
            // Save previous state
            var wasInCombat = IsInCombat;
            var timeSinceLastEntry = currentTime - combatData.LastActionTime;

            // Determine if we are in combat
            IsInCombat = _calculatorService.IsInCombat(combatData.LastActionTime, currentTime);

            // Current Combat logic: new combat starts with new action after pause > 8 sec
            if (combatData.LastActionTime > DateTime.MinValue)
            {
                if (_calculatorService.IsNewCombatStarted(wasInCombat, timeSinceLastEntry))
                {
                    // New combat started after pause >= 8 seconds
                    CombatStartTime = currentTime;
                    CurrentCombatDamage = 0;
                }

                // Update current combat damage
                if (IsInCombat || combatData.LastActionTime == currentTime)
                {
                    CurrentCombatDamage = combatData.TotalDamage - (TotalDamage - combatData.TotalDamage);
                }
            }
            else if (!IsInCombat)
            {
                // Combat ended, reset current combat damage
                CurrentCombatDamage = 0;
            }

            // Update total damage
            TotalDamage = combatData.TotalDamage;

            // Process Last Combat data
            if (combatData.LastCombatDamage > 0)
            {
                // Save previous session to history before updating
                SaveCurrentSessionToHistory();

                // Update last combat data
                _lastCombatDamage = combatData.LastCombatDamage;
                _lastCombatFirstTime = combatData.LastCombatFirstTime;
                _lastCombatLastTime = combatData.LastCombatLastTime;
                _lastCombatGapSeconds = _calculatorService.CalculateDps(
                    combatData.LastCombatDamage,
                    combatData.LastCombatFirstTime,
                    combatData.LastCombatLastTime,
                    combatData.OverallGapSeconds); // Need to pass correct gap seconds here

                _lastCombatEntries.Clear();
                _lastCombatEntries.AddRange(combatData.LastCombatEntries);
            }
        }

        public void SaveCurrentSessionToHistory()
        {
            if (_lastCombatDamage > 0 && _lastCombatFirstTime != DateTime.MinValue && _lastCombatEntries.Count > 0)
            {
                // Check if we haven't saved this session already (prevent duplicates)
                if (_combatHistory.Any(s => s.Damage == _lastCombatDamage && s.StartTime == _lastCombatFirstTime))
                {
                    return; // Skip creating duplicate session
                }

                // Find most frequent target name
                var targetGroups = _lastCombatEntries
                    .GroupBy(e => e.TargetName)
                    .OrderByDescending(g => g.Count())
                    .ToList();

                string mostFrequentTarget = targetGroups.FirstOrDefault()?.Key ?? "Unknown";

                // Find next available number for this target name
                var existingNumbers = _combatHistory
                    .Where(s => s.TargetName.StartsWith(mostFrequentTarget + " ("))
                    .Select(s => {
                        var match = Regex.Match(s.TargetName, $@"^{Regex.Escape(mostFrequentTarget)}\s*\((\d+)\)$");
                        return match.Success ? int.Parse(match.Groups[1].Value) : 0;
                    })
                    .ToList();

                int nextNumber = existingNumbers.Count > 0 ? existingNumbers.Max() + 1 : 1;
                string sessionName = $"{mostFrequentTarget} ({nextNumber})";

                var session = new CombatSession
                {
                    TargetName = sessionName,
                    Damage = _lastCombatDamage,
                    StartTime = _lastCombatFirstTime,
                    EndTime = _lastCombatLastTime,
                    GapSeconds = _lastCombatGapSeconds,
                    Entries = new List<CombatEntry>(_lastCombatEntries)
                };

                _combatHistory.Insert(0, session); // Add to beginning

                // Limit number of sessions to save memory
                if (_combatHistory.Count > Constants.Combat.MaxCombatSessions)
                {
                    _combatHistory.RemoveRange(Constants.Combat.MaxCombatSessions,
                        _combatHistory.Count - Constants.Combat.MaxCombatSessions);
                }
            }
        }

        public void ClearHistory()
        {
            _combatHistory.Clear();
        }

        public CombatSession? GetSessionByTargetName(string targetName)
        {
            return _combatHistory.FirstOrDefault(s => s.TargetName == targetName);
        }

        /// <summary>
        /// Gets last combat display data
        /// </summary>
        public (long damage, double dps, TimeSpan duration) GetLastCombatDisplayData()
        {
            double dps = 0;
            var duration = TimeSpan.Zero;

            if (_lastCombatDamage > 0 && _lastCombatFirstTime != DateTime.MinValue && _lastCombatLastTime != DateTime.MinValue)
            {
                duration = _lastCombatLastTime - _lastCombatFirstTime;
                var activeTimeSeconds = duration.TotalSeconds - _lastCombatGapSeconds;
                if (activeTimeSeconds > 0)
                {
                    dps = _lastCombatDamage / activeTimeSeconds;
                }
            }

            return (_lastCombatDamage, dps, duration);
        }

        /// <summary>
        /// Gets overall damage display data
        /// </summary>
        public (long damage, double dps, TimeSpan duration) GetOverallDisplayData(DateTime firstActionTime, DateTime lastActionTime, double overallGapSeconds)
        {
            double dps = 0;
            var duration = TimeSpan.Zero;

            if (TotalDamage > 0 && firstActionTime != DateTime.MinValue && lastActionTime != DateTime.MinValue)
            {
                duration = lastActionTime - firstActionTime;
                var activeTimeSeconds = duration.TotalSeconds - overallGapSeconds;
                if (activeTimeSeconds > 0)
                {
                    dps = TotalDamage / activeTimeSeconds;
                }
            }

            return (TotalDamage, dps, duration);
        }
    }
}
