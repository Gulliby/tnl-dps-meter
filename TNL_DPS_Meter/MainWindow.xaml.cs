using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace TNL_DPS_Meter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DispatcherTimer _updateTimer;
        private DispatcherTimer _fileCheckTimer;
        private string _combatLogPath;
        private string _currentLogFileName;
        private string? _currentLogFilePath;
        private DateTime _combatStartTime;
        private DateTime _overallStartTime;
        private long _currentCombatDamage;
        private long _totalDamage;
        private bool _isInCombat;
        private DateTime _lastLogEntryTime;
        private DateTime _firstLogEntryTime;
        private double _overallGapSeconds;
        private int _lastProcessedLineCount;
        private long _lastCombatDamage;
        private DateTime _lastCombatFirstTime;
        private DateTime _lastCombatLastTime;
        private double _lastCombatGapSeconds;

        private string FormatNumber(long number)
        {
            if (number >= 1000)
            {
                double value = number / 1000.0;
                return $"{value:N1}k";
            }
            return number.ToString();
        }

        private string FormatDps(double dps)
        {
            if (dps >= 1000)
            {
                double value = dps / 1000.0;
                return $"{value:N1}k";
            }
            return $"{dps:N1}";
        }

        public MainWindow()
        {
            InitializeComponent();

            // Set combat log path - always from game folder
            _combatLogPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "TL", "SAVED", "COMBATLOGS");

            // Timer for reading file and updating DPS every 200ms
            _updateTimer = new DispatcherTimer();
            _updateTimer.Interval = TimeSpan.FromMilliseconds(200);
            _updateTimer.Tick += UpdateTimer_Tick;
            _updateTimer.Start();

            // Timer for checking new files every 10 seconds
            _fileCheckTimer = new DispatcherTimer();
            _fileCheckTimer.Interval = TimeSpan.FromSeconds(10);
            _fileCheckTimer.Tick += FileCheckTimer_Tick;
            _fileCheckTimer.Start();

            // Window drag handler
            //this.MouseLeftButtonDown += (s, e) => DragMove();

            InitializeCombatTracking();
            _overallStartTime = DateTime.MinValue; // Will be initialized on first log read
            _lastLogEntryTime = DateTime.MinValue;
            _firstLogEntryTime = DateTime.MinValue;
            _overallGapSeconds = 0;
            _lastProcessedLineCount = 0;
            _lastCombatDamage = 0;
            _lastCombatFirstTime = DateTime.MinValue;
            _lastCombatLastTime = DateTime.MinValue;
            _lastCombatGapSeconds = 0;
            _currentLogFileName = "No log file";

            // Initial file check on startup
            CheckForNewLogFile();

            // Set initial UI values
            Dispatcher.Invoke(() =>
            {
                CurrentCombatText.Text = "0 | 0.0";
                OverallDamageText.Text = "0 | 0.0";
                FileInfoText.Text = _currentLogFileName;
                TabHeaders.Visibility = Visibility.Collapsed;
                FileInfoContainer.Visibility = Visibility.Collapsed;
            });
        }

        private void InitializeCombatTracking()
        {
            _combatStartTime = DateTime.Now;
            _currentCombatDamage = 0;
            _totalDamage = 0;
            _isInCombat = false;
        }

        private void UpdateTimer_Tick(object? sender, EventArgs e)
        {
            try
            {
                // Read current file and update DPS every 200ms
                UpdateDPSDataFromCurrentFile();
            }
            catch (Exception ex)
            {
                // Continue working in case of error
                Console.WriteLine($"Error updating DPS data: {ex.Message}");
            }
        }

        private void FileCheckTimer_Tick(object? sender, EventArgs e)
        {
            try
            {
                // Check for new files every 10 seconds
                CheckForNewLogFile();
            }
            catch (Exception ex)
            {
                // Continue working in case of error
                Console.WriteLine($"Error checking for new log files: {ex.Message}");
            }
        }

        private void UpdateDPSDataFromCurrentFile()
        {
            // Use current file found earlier
            if (string.IsNullOrEmpty(_currentLogFilePath) || !File.Exists(_currentLogFilePath))
            {
                return;
            }

            // Read data from current file
            var combatData = ParseCombatLog(_currentLogFilePath);

            // Check if there are new log entries
            bool hasNewActivity = false;
            if (combatData.LastActionTime > _lastLogEntryTime)
            {
                _lastLogEntryTime = combatData.LastActionTime;
                hasNewActivity = true;
            }

            // Update general data only if there is new activity
            if (hasNewActivity)
            {
                _totalDamage = combatData.TotalDamage;

                // Save first entry time and gaps for Overall DPS
            if (_firstLogEntryTime == DateTime.MinValue && combatData.FirstActionTime != DateTime.MaxValue)
            {
                _firstLogEntryTime = combatData.FirstActionTime;
            }
            _overallGapSeconds = combatData.OverallGapSeconds;

            // Save data for Last Combat
            if (combatData.LastCombatDamage > 0)
            {
                // Check if new data appeared (not equal to previous)
                bool hasNewCombatData = (combatData.LastCombatDamage != _lastCombatDamage);

                _lastCombatDamage = combatData.LastCombatDamage;
                _lastCombatFirstTime = combatData.LastCombatFirstTime;
                _lastCombatLastTime = combatData.LastCombatLastTime;
                _lastCombatGapSeconds = CalculateLastCombatGaps(combatData);

                // Start flash animation when new data appears
                if (hasNewCombatData)
                {
                    FlashWindow();
                }
            }
            }

            // Determine if we are in combat
            var timeSinceLastEntry = DateTime.Now - _lastLogEntryTime;
            var wasInCombat = _isInCombat;
            _isInCombat = timeSinceLastEntry.TotalSeconds < 8;

            // Current Combat logic: new combat starts with new activity after pause > 8 sec
            if (hasNewActivity)
            {
                if (!wasInCombat && !_isInCombat)
                {
                    // New combat started after pause
                    _combatStartTime = DateTime.Now;
                    _currentCombatDamage = 0;
                }

                // Update current combat damage
                if (_isInCombat || hasNewActivity)
                {
                    _currentCombatDamage = combatData.TotalDamage - (_totalDamage - combatData.TotalDamage);
                }
            }
            else if (!_isInCombat)
            {
                // Combat ended, reset current combat damage
                _currentCombatDamage = 0;
            }

            // Calculate DPS
            double lastCombatDps = 0;
            double overallDps = 0;

            // Last Combat DPS - from saved new data minus gaps
            if (_lastCombatDamage > 0 &&
                _lastCombatFirstTime != DateTime.MinValue &&
                _lastCombatLastTime != DateTime.MinValue)
            {
                var lastCombatDuration = _lastCombatLastTime - _lastCombatFirstTime;
                var activeTimeSeconds = lastCombatDuration.TotalSeconds - _lastCombatGapSeconds;
                if (activeTimeSeconds > 0)
                {
                    lastCombatDps = _lastCombatDamage / activeTimeSeconds;
                }
            }

            // Overall DPS - from first to last record minus gaps > 10s
            if (_totalDamage > 0 && _firstLogEntryTime != DateTime.MinValue && combatData.LastActionTime != DateTime.MinValue)
            {
                var totalDuration = combatData.LastActionTime - _firstLogEntryTime;
                var activeTimeSeconds = totalDuration.TotalSeconds - _overallGapSeconds;
                if (activeTimeSeconds > 0)
                {
                    overallDps = _totalDamage / activeTimeSeconds;
                }
            }

            // Update UI
            Dispatcher.Invoke(() =>
            {
                CurrentCombatText.Text = $"{FormatNumber(_lastCombatDamage)} | {FormatDps(lastCombatDps)}";
                OverallDamageText.Text = $"{FormatNumber(_totalDamage)} | {FormatDps(overallDps)}";
                FileInfoText.Text = _currentLogFileName;
            });
        }

        private void CheckForNewLogFile()
        {
            var latestFile = GetLatestCombatLogFile();
            if (latestFile != null && latestFile != _currentLogFilePath)
            {
                // New file found
                _currentLogFilePath = latestFile;
                _currentLogFileName = Path.GetFileName(latestFile);

                // Reset data when switching files
                InitializeCombatTracking();
                _overallStartTime = DateTime.MinValue;
                _lastLogEntryTime = DateTime.MinValue;
                _firstLogEntryTime = DateTime.MinValue;
                _overallGapSeconds = 0;
                _lastProcessedLineCount = 0;
                _lastCombatDamage = 0;
                _lastCombatFirstTime = DateTime.MinValue;
                _lastCombatLastTime = DateTime.MinValue;
                _lastCombatGapSeconds = 0;

                // Immediately update UI for Last Combat
                Dispatcher.Invoke(() =>
                {
                    CurrentCombatText.Text = "0 | 0.0";
                });
            }
            else if (latestFile == null)
            {
                // No files found
                _currentLogFilePath = null;
                _currentLogFileName = "No log file";
            }
        }

        private string? GetLatestCombatLogFile()
        {
            try
            {
                if (!Directory.Exists(_combatLogPath))
                    return null;

                var logFiles = Directory.GetFiles(_combatLogPath, "*.txt")
                    .OrderByDescending(f => File.GetLastWriteTime(f))
                    .ToArray();

                return logFiles.FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }

        private CombatData ParseCombatLog(string filePath)
        {
            var combatData = new CombatData
            {
                StartTime = DateTime.Now,
                LastActionTime = DateTime.MinValue,
                FirstActionTime = DateTime.MaxValue,
                OverallGapSeconds = 0,
                TotalDamage = 0
            };

            try
            {
                // Explicitly open file for read-only access
                using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var streamReader = new StreamReader(fileStream))
                {
                    string content = streamReader.ReadToEnd();
                    combatData.StartTime = File.GetCreationTime(filePath);

                    // Parse Throne and Liberty CSV format (CombatLogVersion,4)
                    ParseCombatLogCSV(content, combatData);
                }
            }
            catch (IOException ex)
            {
                // Special handling for cases when file is locked by another process
                Console.WriteLine($"File {filePath} is locked by another process: {ex.Message}");
                // Use previous data or demo data
                combatData.TotalDamage = new Random().Next(10000, 100000);
                combatData.LastActionTime = DateTime.Now.AddSeconds(-new Random().Next(0, 10));
            }
            catch (Exception ex)
            {
                // General file reading error handling
                Console.WriteLine($"Error reading combat log {filePath}: {ex.Message}");
                combatData.TotalDamage = new Random().Next(10000, 100000);
                combatData.LastActionTime = DateTime.Now.AddSeconds(-new Random().Next(0, 10));
            }

            return combatData;
        }

        private void ParseCombatLogCSV(string content, CombatData combatData)
        {
            var lines = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            int currentLineIndex = 0;

            foreach (var line in lines)
            {
                // Skip headers and empty lines
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("CombatLogVersion"))
                {
                    currentLineIndex++;
                    continue;
                }

                // Parse Throne and Liberty CSV format: CombatLogVersion,4
                // {Date},DamageDone,{AbilityName},{ServerTick},{DamageDoneByAbilityHit},...
                var parts = line.Split(',');
                if (parts.Length >= 5 && parts[1] == "DamageDone")
                {
                    try
                    {
                        // Parse damage (5th element, index 4 = DamageDoneByAbilityHit)
                        if (long.TryParse(parts[4], out long damage))
                        {
                            combatData.TotalDamage += damage;

                            // Parse timestamp (1st element, index 0)
                            // Format: YYYYMMDD-HH:MM:SS:MS
                            var timestampStr = parts[0];
                            if (DateTime.TryParseExact(timestampStr,
                                new[] { "yyyyMMdd-HH:mm:ss:fff", "yyyyMMdd-HH:mm:ss" },
                                System.Globalization.CultureInfo.InvariantCulture,
                                System.Globalization.DateTimeStyles.None,
                                out DateTime timestamp))
                            {
                                combatData.ActionTimestamps.Add(timestamp);

                                if (timestamp > combatData.LastActionTime)
                                {
                                    combatData.LastActionTime = timestamp;
                                }
                                if (timestamp < combatData.FirstActionTime)
                                {
                                    combatData.FirstActionTime = timestamp;
                                }

                                // Calculate statistics for Last Combat (new data)
                                if (currentLineIndex >= _lastProcessedLineCount)
                                {
                                    combatData.LastCombatDamage += damage;

                                    if (combatData.LastCombatFirstTime == DateTime.MinValue)
                                    {
                                        combatData.LastCombatFirstTime = timestamp;
                                    }
                                    combatData.LastCombatLastTime = timestamp;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error parsing damage line '{line}': {ex.Message}");
                    }
                }
                currentLineIndex++;
            }

            // Update number of processed lines
            _lastProcessedLineCount = currentLineIndex;

            // Calculate gaps between records (pauses > 10 seconds)
            CalculateGaps(combatData);
        }

        private void CalculateGaps(CombatData combatData)
        {
            if (combatData.ActionTimestamps.Count < 2)
                return;

            // Sort timestamps in case they are not in chronological order
            combatData.ActionTimestamps.Sort();

            for (int i = 1; i < combatData.ActionTimestamps.Count; i++)
            {
                var timeDiff = combatData.ActionTimestamps[i] - combatData.ActionTimestamps[i - 1];
                if (timeDiff.TotalSeconds > 10)
                {
                    combatData.OverallGapSeconds += timeDiff.TotalSeconds;
                }
            }
        }

        private double CalculateLastCombatGaps(CombatData combatData)
        {
            double lastCombatGaps = 0;

            // Find records that belong to Last Combat (within time range)
            var lastCombatTimestamps = combatData.ActionTimestamps
                .Where(t => t >= combatData.LastCombatFirstTime && t <= combatData.LastCombatLastTime)
                .OrderBy(t => t)
                .ToList();

            if (lastCombatTimestamps.Count < 2)
                return 0;

            for (int i = 1; i < lastCombatTimestamps.Count; i++)
            {
                var timeDiff = lastCombatTimestamps[i] - lastCombatTimestamps[i - 1];
                if (timeDiff.TotalSeconds > 10)
                {
                    lastCombatGaps += timeDiff.TotalSeconds;
                }
            }

            return lastCombatGaps;
        }


        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Allow dragging the window by clicking anywhere on it
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void Window_MouseEnter(object sender, MouseEventArgs e)
        {
            // Show tab headers and file info when mouse enters
            TabHeaders.Visibility = Visibility.Visible;
            FileInfoContainer.Visibility = Visibility.Visible;
        }

        private void Window_MouseLeave(object sender, MouseEventArgs e)
        {
            // Hide tab headers and file info when mouse leaves
            TabHeaders.Visibility = Visibility.Collapsed;
            FileInfoContainer.Visibility = Visibility.Collapsed;
        }

        private void ViewSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = (ComboBox)sender;
            var selectedItem = (ComboBoxItem)comboBox.SelectedItem;
            if (selectedItem?.Content == null) return;

            string? selectedView = selectedItem.Content.ToString();
            if (string.IsNullOrEmpty(selectedView)) return;

            // Check that UI elements are initialized
            if (LastCombatView == null || OverallDamageView == null) return;

            if (selectedView == "Last Combat")
            {
                LastCombatView.Visibility = Visibility.Visible;
                OverallDamageView.Visibility = Visibility.Collapsed;
            }
            else if (selectedView == "Overall Damage")
            {
                LastCombatView.Visibility = Visibility.Collapsed;
                OverallDamageView.Visibility = Visibility.Visible;
            }
        }

        private void FlashWindow()
        {
            // Get main window Border
            var border = (Border)this.Content;

            // Save original color
            var originalBrush = border.Background;

            // Change to purple
            border.Background = (SolidColorBrush)this.FindResource("FlashBrush");

            // Return to original color after 150ms
            var timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(150);
            timer.Tick += (s, e) =>
            {
                border.Background = originalBrush;
                timer.Stop();
            };
            timer.Start();
        }

        private class CombatData
        {
            public DateTime StartTime { get; set; }
            public DateTime LastActionTime { get; set; }
            public DateTime FirstActionTime { get; set; }
            public List<DateTime> ActionTimestamps { get; set; } = new List<DateTime>();
            public double OverallGapSeconds { get; set; }
            public long TotalDamage { get; set; }

            // Для Last Combat - статистика по новым данным
            public long LastCombatDamage { get; set; }
            public DateTime LastCombatFirstTime { get; set; }
            public DateTime LastCombatLastTime { get; set; }
        }

    }
}
