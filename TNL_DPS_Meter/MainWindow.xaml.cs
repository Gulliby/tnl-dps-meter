using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using TNL_DPS_Meter.Core;
using TNL_DPS_Meter.Models;
using TNL_DPS_Meter.Services;
using TNL_DPS_Meter.Services.Interfaces;

namespace TNL_DPS_Meter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Services
        private readonly ICombatLogParserService _logParserService;
        private readonly ICombatCalculatorService _calculatorService;
        private readonly IFileMonitorService _fileMonitorService;
        private readonly ICombatTrackerService _combatTrackerService;

        // Timers
        private DispatcherTimer _updateTimer;
        private DispatcherTimer _fileCheckTimer;

        // File monitoring
        private string _currentLogFileName;
        private string? _currentLogFilePath;
        private DateTime _lastLogEntryTime;
        private DateTime _firstLogEntryTime;
        private double _overallGapSeconds;
        private int _lastProcessedLineCount;

        // UI state
        private string _currentView = Constants.ViewNames.LastCombat;


        [STAThread]
        public static void Main()
        {
            var app = new Application();
            app.Run(new MainWindow());
        }

        public MainWindow()
        {
            // Initialize services
            _calculatorService = new CombatCalculatorService();
            _logParserService = new CombatLogParserService();
            _fileMonitorService = new FileMonitorService();
            _combatTrackerService = new CombatTrackerService(_calculatorService);

            InitializeComponent();

            // Timer for reading file and updating DPS every 200ms
            _updateTimer = new DispatcherTimer();
            _updateTimer.Interval = TimeSpan.FromMilliseconds(Constants.Timers.UpdateIntervalMs);
            _updateTimer.Tick += UpdateTimer_Tick;
            _updateTimer.Start();

            // Timer for checking new files every 10 seconds
            _fileCheckTimer = new DispatcherTimer();
            _fileCheckTimer.Interval = TimeSpan.FromMilliseconds(Constants.Timers.FileCheckIntervalMs);
            _fileCheckTimer.Tick += FileCheckTimer_Tick;
            _fileCheckTimer.Start();

            // Initialize state
            _combatTrackerService.Initialize();
            _lastLogEntryTime = DateTime.MinValue;
            _firstLogEntryTime = DateTime.MinValue;
            _overallGapSeconds = 0;
            _lastProcessedLineCount = 0;
            _currentLogFileName = "No log file";

            // Initial file check on startup
            CheckForNewLogFile();

            // Initialize ComboBox with default selection
            UpdateCombatHistoryComboBox();

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
            if (string.IsNullOrEmpty(_currentLogFilePath) || !_fileMonitorService.FileExists(_currentLogFilePath))
            {
                return;
            }

            // Read data from current file
            var combatData = _logParserService.ParseCombatLog(_currentLogFilePath);
            if (combatData == null)
                return;

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
                // Save first entry time and gaps for Overall DPS
                if (_firstLogEntryTime == DateTime.MinValue && combatData.FirstActionTime != DateTime.MaxValue)
                {
                    _firstLogEntryTime = combatData.FirstActionTime;
                }
                _overallGapSeconds = combatData.OverallGapSeconds;

                // Update tracker with new data
                _combatTrackerService.UpdateFromCombatData(combatData, DateTime.Now);

                // Handle Last Combat updates
                if (combatData.LastCombatDamage > 0)
                {
                    // Update combo box immediately to show new historical tab
                    UpdateCombatHistoryComboBox();

                    // Flash animation when new tab is created
                    if (_combatTrackerService.CombatHistory.Count > 0)
                    {
                        FlashWindow();
                    }

                    // Update UI to show new Last Combat data (only if Last Combat is selected)
                    if (_currentView == Constants.ViewNames.LastCombat)
                    {
                        ShowLastCombat();
                    }
                }
            }

            // Get display data from services
            var lastCombatData = _combatTrackerService.GetLastCombatDisplayData();
            var overallData = _combatTrackerService.GetOverallDisplayData(
                _firstLogEntryTime, combatData.LastActionTime, _overallGapSeconds);

            // Update UI only for currently selected view
            Dispatcher.Invoke(() =>
            {
                if (_currentView == Constants.ViewNames.LastCombat)
                {
                    // Update Last Combat display
                    CurrentCombatText.Text = $"{_calculatorService.FormatNumber(lastCombatData.damage)} | {_calculatorService.FormatDps(lastCombatData.dps)}";
                    if (lastCombatData.duration > TimeSpan.Zero)
                    {
                        LastCombatTimeText.Text = _calculatorService.FormatTimeSpan(lastCombatData.duration);
                    }
                }
                else if (_currentView == Constants.ViewNames.OverallDamage)
                {
                    // Update Overall Damage display
                    OverallDamageText.Text = $"{_calculatorService.FormatNumber(overallData.damage)} | {_calculatorService.FormatDps(overallData.dps)}";
                    if (overallData.duration > TimeSpan.Zero)
                    {
                        OverallTimeText.Text = _calculatorService.FormatTimeSpan(overallData.duration);
                    }
                }
                // Historical combat views don't update automatically - they show fixed data

                FileInfoText.Text = _currentLogFileName;
            });
        }

        private void CheckForNewLogFile()
        {
            var latestFile = _fileMonitorService.GetLatestCombatLogFile();
            if (latestFile != null && latestFile != _currentLogFilePath)
            {
                // New file found
                _currentLogFilePath = latestFile;
                _currentLogFileName = System.IO.Path.GetFileName(latestFile);

                // Reset data when switching files
                _combatTrackerService.Initialize();
                _lastLogEntryTime = DateTime.MinValue;
                _firstLogEntryTime = DateTime.MinValue;
                _overallGapSeconds = 0;
                _lastProcessedLineCount = 0;

                // Clear combat history when switching to new log file
                _combatTrackerService.ClearHistory();
                _currentView = Constants.ViewNames.LastCombat;

                // Update ComboBox to show only Last Combat and Overall Damage
                UpdateCombatHistoryComboBox();

                // Immediately update UI for Last Combat and Overall Damage
                Dispatcher.Invoke(() =>
                {
                    CurrentCombatText.Text = "0 | 0.0";
                    LastCombatTimeText.Text = "0:000";
                    OverallDamageText.Text = "0 | 0.0";
                    OverallTimeText.Text = "0:000";
                });
            }
            else if (latestFile == null)
            {
                // No files found
                _currentLogFilePath = null;
                _currentLogFileName = "No log file";
            }
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

            // Show damage breakdown button only if not in Overall Damage view
            if (_currentView != Constants.ViewNames.OverallDamage)
            {
                DamageBreakdownButton.Visibility = Visibility.Visible;
            }
        }

        private void Window_MouseLeave(object sender, MouseEventArgs e)
        {
            // Hide tab headers and file info when mouse leaves
            TabHeaders.Visibility = Visibility.Collapsed;
            FileInfoContainer.Visibility = Visibility.Collapsed;
            DamageBreakdownButton.Visibility = Visibility.Collapsed;
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

            if (selectedView.Contains(Constants.ViewNames.LastCombat))
            {
                _currentView = Constants.ViewNames.LastCombat;
                ShowLastCombat();
            }
            else if (selectedView.Contains(Constants.ViewNames.OverallDamage))
            {
                _currentView = Constants.ViewNames.OverallDamage;
                ShowOverallDamage();
            }
            else
            {
                // Show historical combat data
                _currentView = selectedView;
                var session = _combatTrackerService.GetSessionByTargetName(selectedView);
                if (session != null)
                {
                    ShowHistoricalCombat(session);
                }
            }
        }

        private void ShowLastCombat()
        {
            LastCombatView.Visibility = Visibility.Visible;
            OverallDamageView.Visibility = Visibility.Collapsed;

            var lastCombatData = _combatTrackerService.GetLastCombatDisplayData();

            // Update display with current Last Combat data
            Dispatcher.Invoke(() =>
            {
                CurrentCombatText.Text = $"{_calculatorService.FormatNumber(lastCombatData.damage)} | {_calculatorService.FormatDps(lastCombatData.dps)}";
                if (lastCombatData.duration > TimeSpan.Zero)
                {
                    LastCombatTimeText.Text = _calculatorService.FormatTimeSpan(lastCombatData.duration);
                }
            });
        }

        private void ShowOverallDamage()
        {
            LastCombatView.Visibility = Visibility.Collapsed;
            OverallDamageView.Visibility = Visibility.Visible;

            var overallData = _combatTrackerService.GetOverallDisplayData(
                _firstLogEntryTime, DateTime.Now, _overallGapSeconds);

            Dispatcher.Invoke(() =>
            {
                OverallDamageText.Text = $"{_calculatorService.FormatNumber(overallData.damage)} | {_calculatorService.FormatDps(overallData.dps)}";
                if (overallData.duration > TimeSpan.Zero)
                {
                    OverallTimeText.Text = _calculatorService.FormatTimeSpan(overallData.duration);
                }
            });
        }

        private void ShowHistoricalCombat(CombatSession session)
        {
            if (LastCombatView == null || CurrentCombatText == null || LastCombatTimeText == null)
                return;

            LastCombatView.Visibility = Visibility.Visible;
            OverallDamageView.Visibility = Visibility.Collapsed;

            double dps = _calculatorService.CalculateDps(session.Damage, session.StartTime, session.EndTime, session.GapSeconds);

            Dispatcher.Invoke(() =>
            {
                CurrentCombatText.Text = $"{_calculatorService.FormatNumber(session.Damage)} | {_calculatorService.FormatDps(dps)}";
                LastCombatTimeText.Text = _calculatorService.FormatTimeSpan(session.CombatDuration - TimeSpan.FromSeconds(session.GapSeconds));
            });
        }



        private void UpdateCombatHistoryComboBox()
        {
            if (ViewSelector == null) return;

            // Clear all items
            ViewSelector.Items.Clear();

            // Add Last Combat first
            var lastCombatItem = new ComboBoxItem { Content = Constants.ViewNames.LastCombat };
            ViewSelector.Items.Add(lastCombatItem);

            // Add combat history items (they already have numbering in their names)
            foreach (var session in _combatTrackerService.CombatHistory)
            {
                var item = new ComboBoxItem { Content = session.TargetName };
                ViewSelector.Items.Add(item);
            }

            // Add Overall Damage last (always at the end)
            var overallDamageItem = new ComboBoxItem { Content = Constants.ViewNames.OverallDamage };
            ViewSelector.Items.Add(overallDamageItem);

            // Set default selection to "Last Combat" if nothing is selected
            if (ViewSelector.SelectedItem == null)
            {
                ViewSelector.SelectedItem = lastCombatItem;
            }
        }

        private void FlashWindow()
        {
            // Get main window Border
            var border = (Border)this.FindName("MainBorder");

            // Save original color
            var originalBrush = border.Background;

            // Change to purple
            border.Background = (SolidColorBrush)this.FindResource("FlashBrush");

            // Return to original color after specified duration
            var timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(Constants.Timers.FlashAnimationDurationMs);
            timer.Tick += (s, e) =>
            {
                border.Background = originalBrush;
                timer.Stop();
            };
            timer.Start();
        }

        private void DamageBreakdownButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var breakdownWindow = new DamageBreakdownWindow();
                breakdownWindow.Owner = this;

                // Get appropriate combat entries based on current view
                List<CombatEntry> entriesToShow = new List<CombatEntry>();

                if (_currentView == Constants.ViewNames.LastCombat)
                {
                    // Show current last combat entries
                    entriesToShow = new List<CombatEntry>(_combatTrackerService.LastCombatEntries);
                }
                else if (_currentView == Constants.ViewNames.OverallDamage)
                {
                    // For Overall Damage, we don't show breakdown (as requested)
                    MessageBox.Show("Damage breakdown is not available for overall statistics", "Information",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                else
                {
                    // Show historical combat entries
                    var session = _combatTrackerService.GetSessionByTargetName(_currentView);
                    if (session != null)
                    {
                        entriesToShow = new List<CombatEntry>(session.Entries);
                    }
                }

                System.Diagnostics.Debug.WriteLine($"DamageBreakdown: Current view = {_currentView}, Entries count: {entriesToShow.Count}");

                if (entriesToShow.Count > 0)
                {
                    breakdownWindow.SetDamageData(entriesToShow);

                    // Show window and ensure it gets focus
                    breakdownWindow.Show();

                    // Force activation and focus
                    breakdownWindow.Activate();
                    breakdownWindow.Topmost = true;
                    breakdownWindow.Focus();
                }
                else
                {
                    MessageBox.Show("No data to display", "Information",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening damage breakdown window: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


    }
}
