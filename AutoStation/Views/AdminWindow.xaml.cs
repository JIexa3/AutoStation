using AutoStation.Data;
using AutoStation.Models;
using AutoStation.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace AutoStation.Views
{
    public partial class AdminWindow : Window, INotifyPropertyChanged
    {
        private readonly int _userId;
        private AutoStationContext? _context;
        private ObservableCollection<Transaction>? _transactions;
        private ObservableCollection<Fuel>? _fuels;
        private ObservableCollection<User>? _users;
        private ObservableCollection<FuelColumn>? _fuelColumns;
        private ObservableCollection<Reservation>? _reservations;

        public ObservableCollection<User> Users
        {
            get => _users;
            set
            {
                _users = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Fuel> Fuels
        {
            get => _fuels;
            set
            {
                _fuels = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<FuelColumn> FuelColumns
        {
            get => _fuelColumns;
            set
            {
                _fuelColumns = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Transaction> Transactions
        {
            get => _transactions;
            set
            {
                _transactions = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Reservation> Reservations
        {
            get => _reservations;
            set
            {
                _reservations = value;
                OnPropertyChanged();
            }
        }

        // Объявляем элементы управления явно
        private ComboBox? transactionsDateRangeComboBox;
        private DataGrid? transactionsDataGrid;
        private TextBlock? totalSalesValueTextBlock;
        private TextBlock? transactionsCountValueTextBlock;
        private TextBlock? averageCheckValueTextBlock;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public AdminWindow(int userId)
        {
            InitializeComponent();
            _userId = userId;
            DataContext = this;

            // Инициализируем элементы управления
            transactionsDateRangeComboBox = TransactionsDateRangeComboBoxControl;
            transactionsDataGrid = TransactionsDataGridControl;
            totalSalesValueTextBlock = TotalSalesValueTextBlock;
            transactionsCountValueTextBlock = TransactionsCountValueTextBlock;
            averageCheckValueTextBlock = AverageCheckValueTextBlock;

            // Устанавливаем значение по умолчанию для комбобокса периода
            transactionsDateRangeComboBox.SelectedItem = transactionsDateRangeComboBox.Items[0];

            try
            {
                _context = new AutoStationContext();
                InitializeData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при инициализации: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void InitializeData()
        {
            if (_context == null) return;

            try
            {
                LoadUsers();
                LoadFuels();
                LoadColumns();
                LoadTransactions();
                LoadReservations();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadUsers()
        {
            try
            {
                var users = _context.Users
                    .AsNoTracking()
                    .ToList();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Users = new ObservableCollection<User>(users);
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке пользователей: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadFuels()
        {
            try
            {
                var fuels = _context.Fuels
                    .Include(f => f.Transactions)
                    .AsNoTracking()
                    .ToList();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Fuels = new ObservableCollection<Fuel>(fuels);
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке топлива: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadColumns()
        {
            try
            {
                var columns = _context.FuelColumns
                    .Include(fc => fc.FuelColumnFuels)
                    .ThenInclude(fcf => fcf.Fuel)
                    .AsNoTracking()
                    .ToList();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    FuelColumns = new ObservableCollection<FuelColumn>(columns);
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке колонок: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadTransactions()
        {
            try
            {
                var transactions = _context.Transactions
                    .Include(t => t.User)
                    .Include(t => t.Fuel)
                    .Include(t => t.FuelColumn)
                    .OrderByDescending(t => t.Date)
                    .AsNoTracking()
                    .ToList();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Transactions = new ObservableCollection<Transaction>(transactions);
                    UpdateStatistics(transactions);
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке транзакций: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadReservations()
        {
            try
            {
                var reservations = _context.Reservations
                    .Include(r => r.User)
                    .Include(r => r.FuelColumn)
                    .OrderByDescending(r => r.ReservationTime)
                    .AsNoTracking()
                    .ToList();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Reservations = new ObservableCollection<Reservation>(reservations);
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке бронирований: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateStatistics(IEnumerable<Transaction> transactions)
        {
            if (transactions == null)
                return;

            try
            {
                decimal totalSales = 0;
                int transactionsCount = 0;
                decimal averageCheck = 0;

                if (transactions.Any())
                {
                    totalSales = transactions.Sum(t => t.TotalPrice);
                    transactionsCount = transactions.Count();
                    averageCheck = totalSales / transactionsCount;
                }

                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (totalSalesValueTextBlock != null)
                        totalSalesValueTextBlock.Text = $"{totalSales:N2} ₽";
                    if (transactionsCountValueTextBlock != null)
                        transactionsCountValueTextBlock.Text = transactionsCount.ToString();
                    if (averageCheckValueTextBlock != null)
                        averageCheckValueTextBlock.Text = $"{averageCheck:N2} ₽";
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении статистики: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TransactionsDateRangeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (_context == null || transactionsDateRangeComboBox?.SelectedItem == null)
                    return;

                var comboBoxItem = (ComboBoxItem)transactionsDateRangeComboBox.SelectedItem;
                var selectedRange = comboBoxItem.Content.ToString();
                var startDate = DateTime.Now;

                switch (selectedRange)
                {
                    case "За сегодня":
                        startDate = DateTime.Today;
                        break;
                    case "За неделю":
                        startDate = DateTime.Today.AddDays(-7);
                        break;
                    case "За месяц":
                        startDate = DateTime.Today.AddMonths(-1);
                        break;
                    case "За все время":
                        startDate = DateTime.MinValue;
                        break;
                }

                var transactions = _context.Transactions
                    .Include(t => t.User)
                    .Include(t => t.Fuel)
                    .Include(t => t.FuelColumn)
                    .Where(t => t.Date >= startDate)
                    .OrderByDescending(t => t.Date)
                    .AsNoTracking()
                    .ToList();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Transactions = new ObservableCollection<Transaction>(transactions);
                    UpdateStatistics(transactions);
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при фильтрации транзакций: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ToggleAdminRights_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is User user && _context != null)
            {
                var result = MessageBox.Show(
                    $"Вы уверены, что хотите {(user.IsAdmin ? "отменить" : "предоставить")} права администратора для пользователя {user.Username}?",
                    "Подтверждение",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    user.IsAdmin = !user.IsAdmin;
                    _context.SaveChanges();
                    LoadUsers();
                }
            }
        }

        private async void AddFuelButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_context == null)
                {
                    _context = new AutoStationContext();
                }

                if (string.IsNullOrWhiteSpace(NewFuelNameTextBox.Text))
                {
                    MessageBox.Show("Введите название топлива", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!decimal.TryParse(FuelPriceTextBox.Text, out decimal price))
                {
                    MessageBox.Show("Введите корректную цену", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!decimal.TryParse(FuelVolumeTextBox.Text, out decimal volume))
                {
                    MessageBox.Show("Введите корректный объем", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var fuelName = NewFuelNameTextBox.Text.Trim();
                var existingFuel = await _context.Fuels.FirstOrDefaultAsync(f => f.Name == fuelName);

                if (existingFuel != null)
                {
                    var result = MessageBox.Show(
                        "Топливо с таким названием уже существует. Хотите обновить его параметры?",
                        "Подтверждение",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question
                    );

                    if (result == MessageBoxResult.Yes)
                    {
                        existingFuel.Volume += volume;
                        existingFuel.Price = price;
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    var fuel = new Fuel
                    {
                        Name = fuelName,
                        Price = price,
                        Volume = volume,
                        IsAvailable = true
                    };

                    _context.Fuels.Add(fuel);
                }

                await _context.SaveChangesAsync();
                LoadFuels();

                // Очищаем поля ввода
                NewFuelNameTextBox.Text = string.Empty;
                FuelPriceTextBox.Text = string.Empty;
                FuelVolumeTextBox.Text = string.Empty;

                MessageBox.Show("Топливо успешно добавлено/обновлено", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении топлива: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void AddColumnButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_context == null)
                {
                    _context = new AutoStationContext();
                }

                if (!int.TryParse(ColumnNumberTextBox.Text, out int columnNumber))
                {
                    MessageBox.Show("Введите корректный номер колонки", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var selectedFuels = FuelTypesForColumnListBox.SelectedItems.Cast<Fuel>().ToList();
                if (!selectedFuels.Any())
                {
                    MessageBox.Show("Выберите хотя бы один тип топлива для колонки", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var column = new FuelColumn
                {
                    Number = columnNumber,
                    IsAvailable = true
                };

                _context.FuelColumns.Add(column);
                await _context.SaveChangesAsync();

                // Добавляем связи с выбранными типами топлива
                foreach (var fuel in selectedFuels)
                {
                    var fuelColumnFuel = new FuelColumnFuel
                    {
                        FuelColumnId = column.Id,
                        FuelId = fuel.Id
                    };
                    _context.FuelColumnFuels.Add(fuelColumnFuel);
                }

                await _context.SaveChangesAsync();
                LoadColumns();

                ColumnNumberTextBox.Clear();
                FuelTypesForColumnListBox.SelectedItems.Clear();

                MessageBox.Show("Колонка успешно добавлена", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении колонки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void CancelReservationButton_Click(object sender, RoutedEventArgs e)
        {
            if (_context == null)
            {
                _context = new AutoStationContext();
            }

            if (ReservationsDataGrid.SelectedItem is not Reservation selectedReservation)
            {
                MessageBox.Show("Выберите бронирование для отмены", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                selectedReservation.IsActive = false;
                await _context.SaveChangesAsync();
                LoadReservations();
                MessageBox.Show("Бронирование успешно отменено", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при отмене бронирования: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void SaveFuelChangesButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_context == null)
                {
                    _context = new AutoStationContext();
                }

                await _context.SaveChangesAsync();
                LoadFuels();
                MessageBox.Show("Изменения сохранены", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении изменений: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            InitializeData();
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            Close();
        }

        private void ColumnsDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                var column = e.Row.Item as FuelColumn;
                if (column != null && _context != null)
                {
                    try
                    {
                        _context.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при сохранении изменений: {ex.Message}", 
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void EditColumnFuels_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var column = (FuelColumn)button.DataContext;

            if (column != null && _context != null)
            {
                var availableFuels = _context.Fuels.ToList();
                var editWindow = new EditColumnFuelsWindow(column, availableFuels);
                if (editWindow.ShowDialog() == true)
                {
                    var selectedFuelIds = editWindow.GetSelectedFuelIds();

                    // Удаляем старые связи
                    var existingLinks = _context.FuelColumnFuels.Where(cf => cf.FuelColumnId == column.Id);
                    _context.FuelColumnFuels.RemoveRange(existingLinks);

                    // Добавляем новые связи
                    foreach (var fuelId in selectedFuelIds)
                    {
                        _context.FuelColumnFuels.Add(new FuelColumnFuel
                        {
                            FuelColumnId = column.Id,
                            FuelId = fuelId
                        });
                    }

                    _context.SaveChanges();
                    LoadColumns(); // Обновляем отображение
                }
            }
        }

        private void DeleteColumn_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var column = (FuelColumn)button.DataContext;

            if (column != null && _context != null)
            {
                var result = MessageBox.Show(
                    $"Вы уверены, что хотите удалить колонку {column.Number}?",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        // Загружаем колонку со всеми связанными данными
                        var columnToDelete = _context.FuelColumns
                            .Include(c => c.FuelColumnFuels)
                            .Include(c => c.Transactions)
                            .Include(c => c.Reservations)
                            .FirstOrDefault(c => c.Id == column.Id);

                        if (columnToDelete != null)
                        {
                            // Удаляем все связанные резервации
                            foreach (var reservation in columnToDelete.Reservations.ToList())
                            {
                                _context.Reservations.Remove(reservation);
                            }

                            // Удаляем все связи с топливом
                            foreach (var fuelLink in columnToDelete.FuelColumnFuels.ToList())
                            {
                                _context.FuelColumnFuels.Remove(fuelLink);
                            }

                            // Удаляем саму колонку
                            _context.FuelColumns.Remove(columnToDelete);
                            _context.SaveChanges();

                            LoadColumns(); // Обновляем список колонок
                            MessageBox.Show("Колонка успешно удалена", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении колонки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void DeleteFuel_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var fuel = (Fuel)button.DataContext;

            if (_context != null)
            {
                var result = MessageBox.Show(
                    "Вы уверены, что хотите удалить это топливо? Это также удалит его из всех колонок.",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    var fuelToDelete = _context.Fuels
                        .Include(f => f.FuelColumnFuels)
                        .FirstOrDefault(f => f.Id == fuel.Id);

                    if (fuelToDelete != null)
                    {
                        // Удаляем связи с колонками
                        _context.FuelColumnFuels.RemoveRange(fuelToDelete.FuelColumnFuels);
                        // Удаляем само топливо
                        _context.Fuels.Remove(fuelToDelete);
                        _context.SaveChanges();
                        LoadFuels(); // Обновляем отображение топлива
                        LoadColumns(); // Обновляем отображение колонок
                    }
                }
            }
        }

        private void FuelsDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                var fuel = (Fuel)e.Row.Item;
                _context?.SaveChanges();
            }
        }

        private void UsersDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit && e.Column is DataGridCheckBoxColumn)
            {
                var user = e.Row.Item as User;
                if (user != null && _context != null)
                {
                    try
                    {
                        var dbUser = _context.Users.Find(user.Id);
                        if (dbUser != null)
                        {
                            dbUser.IsAdmin = user.IsAdmin;
                            _context.SaveChanges();
                            MessageBox.Show($"Права администратора для пользователя {user.Username} успешно обновлены", 
                                "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при обновлении прав администратора: {ex.Message}", 
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
    }
}
