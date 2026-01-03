using System.ComponentModel;

namespace TNL_DPS_Meter.Models
{
    /// <summary>
    /// Damage information by ability for table display
    /// </summary>
    public class AbilityDamageInfo : INotifyPropertyChanged
    {
        private int _number;
        private string _abilityName = string.Empty;
        private long _damageDoneByAbility;
        private int _numberOfHits;
        private double _damagePercentage;

        /// <summary>
        /// Sequential number in the table
        /// </summary>
        public int Number
        {
            get => _number;
            set
            {
                _number = value;
                OnPropertyChanged(nameof(Number));
            }
        }

        /// <summary>
        /// Ability name
        /// </summary>
        public string AbilityName
        {
            get => _abilityName;
            set
            {
                _abilityName = value;
                OnPropertyChanged(nameof(AbilityName));
            }
        }

        /// <summary>
        /// Total damage from ability
        /// </summary>
        public long DamageDoneByAbility
        {
            get => _damageDoneByAbility;
            set
            {
                _damageDoneByAbility = value;
                OnPropertyChanged(nameof(DamageDoneByAbility));
            }
        }

        /// <summary>
        /// Number of hits
        /// </summary>
        public int NumberOfHits
        {
            get => _numberOfHits;
            set
            {
                _numberOfHits = value;
                OnPropertyChanged(nameof(NumberOfHits));
            }
        }

        /// <summary>
        /// Percentage of total damage
        /// </summary>
        public double DamagePercentage
        {
            get => _damagePercentage;
            set
            {
                _damagePercentage = value;
                OnPropertyChanged(nameof(DamagePercentage));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
