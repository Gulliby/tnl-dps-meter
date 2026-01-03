using System;
using System.Linq;
using TNL_DPS_Meter.Core;
using TNL_DPS_Meter.Services.Interfaces;

namespace TNL_DPS_Meter.Services
{
    /// <summary>
    /// Service for combat statistics calculations
    /// </summary>
    public class CombatCalculatorService : ICombatCalculatorService
    {
        public string FormatNumber(long number)
        {
            if (number >= Constants.Formatting.ThousandThreshold)
            {
                double value = number / 1000.0;
                return $"{value.ToString(Constants.Formatting.LargeNumberFormat)}k";
            }
            return number.ToString();
        }

        public string FormatDps(double dps)
        {
            if (dps >= Constants.Formatting.ThousandThreshold)
            {
                double value = dps / 1000.0;
                return $"{value.ToString(Constants.Formatting.LargeNumberFormat)}k";
            }
            return dps.ToString(Constants.Formatting.NormalNumberFormat);
        }

        public string FormatTimeSpan(TimeSpan timeSpan)
        {
            var parts = new System.Collections.Generic.List<string>();

            // Add hours only if > 0
            if (timeSpan.Hours > 0)
            {
                parts.Add($"{timeSpan.Hours:D2}");
            }

            // Add minutes if hours > 0 or minutes > 0
            if (timeSpan.Hours > 0 || timeSpan.Minutes > 0)
            {
                parts.Add($"{timeSpan.Minutes:D2}");
            }

            // Always add seconds
            parts.Add($"{timeSpan.Seconds:D2}");

            // Always add milliseconds
            parts.Add($"{timeSpan.Milliseconds:D3}");

            return string.Join(":", parts);
        }

        public double CalculateDps(long damage, DateTime startTime, DateTime endTime, double gapSeconds)
        {
            var duration = endTime - startTime;
            var activeTimeSeconds = duration.TotalSeconds - gapSeconds;
            if (activeTimeSeconds > 0)
            {
                return damage / activeTimeSeconds;
            }
            return 0;
        }

        public bool IsInCombat(DateTime lastActionTime, DateTime currentTime)
        {
            var timeSinceLastEntry = currentTime - lastActionTime;
            return timeSinceLastEntry.TotalSeconds < Constants.Combat.CombatPauseThresholdSeconds;
        }

        public bool IsNewCombatStarted(bool wasInCombat, TimeSpan timeSinceLastEntry)
        {
            return !wasInCombat && timeSinceLastEntry.TotalSeconds >= Constants.Combat.CombatPauseThresholdSeconds;
        }
    }
}
