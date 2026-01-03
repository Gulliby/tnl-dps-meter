using System;

namespace TNL_DPS_Meter.Core
{
    /// <summary>
    /// Application constants
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// File paths
        /// </summary>
        public static class Paths
        {
            /// <summary>
            /// Path to the game's combat logs folder
            /// </summary>
            public static readonly string CombatLogPath = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "TL", "SAVED", "COMBATLOGS");

            /// <summary>
            /// Combat log files extension
            /// </summary>
            public const string CombatLogExtension = "*.txt";
        }

        /// <summary>
        /// Timer intervals (in milliseconds)
        /// </summary>
        public static class Timers
        {
            /// <summary>
            /// DPS update interval (200ms)
            /// </summary>
            public const int UpdateIntervalMs = 200;

            /// <summary>
            /// New files check interval (10 seconds)
            /// </summary>
            public const int FileCheckIntervalMs = 10000;

            /// <summary>
            /// Flash animation duration (150ms)
            /// </summary>
            public const int FlashAnimationDurationMs = 150;
        }

        /// <summary>
        /// Combat and pause parameters
        /// </summary>
        public static class Combat
        {
            /// <summary>
            /// Maximum pause between actions to determine combat end (seconds)
            /// </summary>
            public const int CombatPauseThresholdSeconds = 8;

            /// <summary>
            /// Minimum pause for DPS calculations (seconds)
            /// </summary>
            public const int GapThresholdSeconds = 10;

            /// <summary>
            /// Maximum number of stored combat sessions
            /// </summary>
            public const int MaxCombatSessions = 10;
        }

        /// <summary>
        /// Date formats for parsing combat logs
        /// </summary>
        public static class DateFormats
        {
            /// <summary>
            /// Date format with milliseconds
            /// </summary>
            public const string FullDateTimeFormat = "yyyyMMdd-HH:mm:ss:fff";

            /// <summary>
            /// Date format without milliseconds
            /// </summary>
            public const string ShortDateTimeFormat = "yyyyMMdd-HH:mm:ss";

            /// <summary>
            /// All supported date formats
            /// </summary>
            public static readonly string[] SupportedFormats = { FullDateTimeFormat, ShortDateTimeFormat };
        }

        /// <summary>
        /// Number formatting parameters
        /// </summary>
        public static class Formatting
        {
            /// <summary>
            /// Threshold for switching to 'k' format
            /// </summary>
            public const long ThousandThreshold = 1000;

            /// <summary>
            /// Format for large numbers (1.2k)
            /// </summary>
            public const string LargeNumberFormat = "N1";

            /// <summary>
            /// Format for normal numbers
            /// </summary>
            public const string NormalNumberFormat = "N1";

            /// <summary>
            /// Time format with milliseconds
            /// </summary>
            public const string TimeSpanFormat = @"mm\:ss\:fff";
        }

        /// <summary>
        /// View names in the interface
        /// </summary>
        public static class ViewNames
        {
            /// <summary>
            /// Last combat view
            /// </summary>
            public const string LastCombat = "Last Combat";

            /// <summary>
            /// Overall damage view
            /// </summary>
            public const string OverallDamage = "Overall Damage";
        }

        /// <summary>
        /// Combat logs CSV format parameters
        /// </summary>
        public static class CsvFormat
        {
            /// <summary>
            /// Record type for damage
            /// </summary>
            public const string DamageDoneType = "DamageDone";

            /// <summary>
            /// Combat log version header
            /// </summary>
            public const string CombatLogVersionHeader = "CombatLogVersion";

            /// <summary>
            /// Timestamp column index
            /// </summary>
            public const int TimestampColumnIndex = 0;

            /// <summary>
            /// Record type column index
            /// </summary>
            public const int RecordTypeColumnIndex = 1;

            /// <summary>
            /// Ability name column index
            /// </summary>
            public const int AbilityNameColumnIndex = 2;

            /// <summary>
            /// Damage column index
            /// </summary>
            public const int DamageColumnIndex = 4;

            /// <summary>
            /// Critical hit flag column index
            /// </summary>
            public const int IsCritColumnIndex = 5;

            /// <summary>
            /// Heavy hit flag column index
            /// </summary>
            public const int IsHeavyColumnIndex = 6;

            /// <summary>
            /// Calculation descriptor column index
            /// </summary>
            public const int CalculationDescriptorColumnIndex = 7;

            /// <summary>
            /// Player name column index
            /// </summary>
            public const int PlayerNameColumnIndex = 8;

            /// <summary>
            /// Target name column index
            /// </summary>
            public const int TargetNameColumnIndex = 9;
        }
    }
}
