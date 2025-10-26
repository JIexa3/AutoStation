using AutoStation.Models;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace AutoStation.Views
{
    public partial class EditColumnFuelsWindow : Window
    {
        private readonly FuelColumn _fuelColumn;
        private readonly List<FuelViewModel> _fuels;

        public EditColumnFuelsWindow(FuelColumn fuelColumn, IEnumerable<Fuel> availableFuels)
        {
            InitializeComponent();
            _fuelColumn = fuelColumn;

            // Создаем список топлива с отметкой выбранных
            _fuels = availableFuels.Select(f => new FuelViewModel
            {
                Id = f.Id,
                Name = f.Name,
                IsSelected = _fuelColumn.FuelColumnFuels.Any(cf => cf.FuelId == f.Id)
            }).ToList();

            FuelsListBox.ItemsSource = _fuels;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        public List<int> GetSelectedFuelIds()
        {
            return _fuels.Where(f => f.IsSelected).Select(f => f.Id).ToList();
        }
    }

    public class FuelViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public bool IsSelected { get; set; }
    }
}
