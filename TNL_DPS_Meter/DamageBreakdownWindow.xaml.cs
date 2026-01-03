using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TNL_DPS_Meter.Models;

namespace TNL_DPS_Meter
{
    public partial class DamageBreakdownWindow : Window
    {
        public ObservableCollection<AbilityDamageInfo> AbilityDamageList { get; set; } = new ObservableCollection<AbilityDamageInfo>();

        public DamageBreakdownWindow()
        {
            InitializeComponent();
            this.DataContext = this;

            // Bind data to DataGrid
            DamageDataGrid.ItemsSource = AbilityDamageList;

            // Ensure window gets focus when loaded
            this.Loaded += DamageBreakdownWindow_Loaded;
        }

        private void DamageBreakdownWindow_Loaded(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("DamageBreakdownWindow: Window loaded");

            // Force activation and focus
            this.Activate();
            this.Topmost = true;
            this.Focus();

            System.Diagnostics.Debug.WriteLine($"DamageBreakdownWindow: Data items count: {AbilityDamageList.Count}");
        }

        public void SetDamageData(System.Collections.Generic.List<CombatEntry> entries)
        {
            AbilityDamageList.Clear();

            if (entries == null || entries.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("DamageBreakdownWindow: No entries to display");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"DamageBreakdownWindow: Processing {entries.Count} entries");

            // Group by ability name and calculate totals
            var groupedData = entries
                .GroupBy(e => e.AbilityName) // Group by actual AbilityName
                .Select(g => new AbilityDamageInfo
                {
                    AbilityName = g.Key,
                    DamageDoneByAbility = g.Sum(e => e.Damage),
                    NumberOfHits = g.Count()
                })
                .OrderByDescending(x => x.DamageDoneByAbility)
                .ToList();

            System.Diagnostics.Debug.WriteLine($"DamageBreakdownWindow: Grouped into {groupedData.Count} abilities");

            // Calculate total damage for percentage calculation
            long totalDamage = groupedData.Sum(x => x.DamageDoneByAbility);

            // Add numbers and calculate percentages
            for (int i = 0; i < groupedData.Count; i++)
            {
                groupedData[i].Number = i + 1;
                if (totalDamage > 0)
                {
                    groupedData[i].DamagePercentage = (double)groupedData[i].DamageDoneByAbility / totalDamage * 100.0;
                }
                AbilityDamageList.Add(groupedData[i]);
            }

            System.Diagnostics.Debug.WriteLine($"DamageBreakdownWindow: Added {AbilityDamageList.Count} items to list");
        }

        private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Allow dragging the window by clicking anywhere on it
            if (e.ButtonState == System.Windows.Input.MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }

}
