using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AutoStation.ViewModels
{
    public class FuelColumnViewModel : INotifyPropertyChanged
    {
        private int _id;
        private int _number;
        private string _fuelTypes;
        private bool _isAvailable;

        public int Id
        {
            get => _id;
            set
            {
                if (_id != value)
                {
                    _id = value;
                    OnPropertyChanged();
                }
            }
        }

        public int Number
        {
            get => _number;
            set
            {
                if (_number != value)
                {
                    _number = value;
                    OnPropertyChanged();
                }
            }
        }

        public string FuelTypes
        {
            get => _fuelTypes;
            set
            {
                if (_fuelTypes != value)
                {
                    _fuelTypes = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsAvailable
        {
            get => _isAvailable;
            set
            {
                if (_isAvailable != value)
                {
                    _isAvailable = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
