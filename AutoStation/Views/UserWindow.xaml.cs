using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using AutoStation.Data;
using AutoStation.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoStation.Views
{
    public partial class UserWindow : Window
    {
        private readonly AutoStationContext _context;
        private readonly int _userId;
        private User _currentUser;
        private Fuel? _selectedFuel;
        private FuelColumn? _selectedColumn;
        private ObservableCollection<Transaction> _transactions;
        private ObservableCollection<Fuel> _fuels;
        private ObservableCollection<FuelColumn> _fuelColumns;
        private decimal _volume;
        private decimal _totalPrice;

        public UserWindow(int userId)
        {
            InitializeComponent();
            _context = new AutoStationContext();
            _userId = userId;

            // Инициализируем начальное состояние навигации
            RefuelContent.Visibility = Visibility.Visible;
            ProfileContent.Visibility = Visibility.Collapsed;
            HistoryContent.Visibility = Visibility.Collapsed;

            LoadData();
            InitializePaymentMethods();
        }

        private void InitializePaymentMethods()
        {
            PaymentMethodComboBox.SelectedIndex = 0;
        }

        private async void LoadData()
        {
            _currentUser = await _context.Users.FindAsync(_userId);
            if (_currentUser == null)
            {
                MessageBox.Show("Пользователь не найден", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
                return;
            }

            // Загружаем данные профиля
            FirstNameTextBox.Text = _currentUser.FirstName;
            LastNameTextBox.Text = _currentUser.LastName;
            PhoneTextBox.Text = _currentUser.Phone;
            EmailTextBox.Text = _currentUser.Email;

            // Загружаем все топливо
            _fuels = new ObservableCollection<Fuel>(await _context.Fuels.ToListAsync());

            // Загружаем колонки с их топливом
            _fuelColumns = new ObservableCollection<FuelColumn>(
                await _context.FuelColumns
                    .Include(fc => fc.FuelColumnFuels)
                    .ThenInclude(fcf => fcf.Fuel)
                    .ToListAsync());

            ColumnComboBox.ItemsSource = _fuelColumns;

            await UpdateFuelAvailability();

            // Загружаем транзакции в отдельной вкладке истории
            _transactions = new ObservableCollection<Transaction>(
                await _context.Transactions
                    .Include(t => t.Fuel)
                    .Include(t => t.FuelColumn)
                    .Where(t => t.UserId == _userId)
                    .OrderByDescending(t => t.Date)
                    .ToListAsync()
            );
            HistoryDataGrid.ItemsSource = _transactions;
        }

        private async Task UpdateFuelAvailability()
        {
            var columns = await _context.FuelColumns
                .Include(fc => fc.FuelColumnFuels)
                .ThenInclude(fcf => fcf.Fuel)
                .ToListAsync();

            var fuelAvailability = new List<FuelAvailabilityInfo>();
            foreach (var column in columns)
            {
                foreach (var fuelColumnFuel in column.FuelColumnFuels)
                {
                    fuelAvailability.Add(new FuelAvailabilityInfo
                    {
                        Column = column,
                        Fuel = fuelColumnFuel.Fuel
                    });
                }
            }

            // Обновляем данные в гриде
            FuelAvailabilityGrid.ItemsSource = fuelAvailability;
        }

        private class FuelAvailabilityInfo
        {
            public FuelColumn Column { get; set; }
            public Fuel Fuel { get; set; }
        }

        private void ColumnComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedColumn = ColumnComboBox.SelectedItem as FuelColumn;
            
            if (_selectedColumn != null)
            {
                // Получаем доступные типы топлива для выбранной колонки
                var availableFuels = _context.FuelColumnFuels
                    .Where(cf => cf.FuelColumnId == _selectedColumn.Id)
                    .Select(cf => cf.Fuel)
                    .ToList();

                // Обновляем источник данных для комбобокса с топливом
                FuelComboBox.ItemsSource = availableFuels;
                
                // Если текущее выбранное топливо недоступно в этой колонке, сбрасываем выбор
                if (_selectedFuel != null && !availableFuels.Any(f => f.Id == _selectedFuel.Id))
                {
                    FuelComboBox.SelectedItem = null;
                    _selectedFuel = null;
                    UpdateTotalPrice();
                }
            }
            else
            {
                // Если колонка не выбрана, очищаем список топлива
                FuelComboBox.ItemsSource = null;
                _selectedFuel = null;
                UpdateTotalPrice();
            }
        }

        private void FuelComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedFuel = FuelComboBox.SelectedItem as Fuel;
            UpdateTotalPrice();
        }

        private void VolumeTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (decimal.TryParse(VolumeTextBox.Text, out decimal volume))
            {
                _volume = volume;
                UpdateTotalPrice();
            }
            else
            {
                _volume = 0;
                UpdateTotalPrice();
            }
        }

        private void UpdateTotalPrice()
        {
            if (_selectedFuel != null)
            {
                var totalPrice = _volume * _selectedFuel.Price;
                TotalPriceTextBlock.Text = $"Итого к оплате: {totalPrice:N2} ₽";
                _totalPrice = totalPrice;
            }
            else
            {
                TotalPriceTextBlock.Text = "Итого к оплате: 0.00 ₽";
                _totalPrice = 0;
            }
        }

        private async void PayButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedFuel == null)
            {
                MessageBox.Show("Выберите тип топлива", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (_selectedColumn == null)
            {
                MessageBox.Show("Выберите колонку", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Проверяем, доступно ли выбранное топливо в этой колонке
            var isFuelAvailable = _context.FuelColumnFuels
                .Any(cf => cf.FuelColumnId == _selectedColumn.Id && cf.FuelId == _selectedFuel.Id);

            if (!isFuelAvailable)
            {
                MessageBox.Show($"Топливо {_selectedFuel.Name} недоступно в выбранной колонке", 
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_volume <= 0)
            {
                MessageBox.Show("Введите корректный объем топлива", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (_volume > _selectedFuel.Volume)
            {
                MessageBox.Show($"Недостаточно топлива. Доступно: {_selectedFuel.Volume:N2} л", 
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var isCardPayment = PaymentMethodComboBox.SelectedIndex == 1;
            if (isCardPayment)
            {
                var cardWindow = new PaymentWindow(_context, _currentUser, _selectedFuel, _volume, _selectedColumn.Id);
                if (cardWindow.ShowDialog() != true)
                {
                    return;
                }
            }
            else
            {
                // Для наличной оплаты показываем сообщение с суммой
                var totalAmount = _volume * _selectedFuel.Price;
                MessageBox.Show(
                    $"Пожалуйста, оплатите {totalAmount:N2} ₽ кассиру.\n\n" +
                    $"Топливо: {_selectedFuel.Name}\n" +
                    $"Объем: {_volume:N2} л\n" +
                    $"Цена за литр: {_selectedFuel.Price:N2} ₽",
                    "Оплата наличными",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            }

            var transaction = new Transaction
            {
                UserId = _userId,
                FuelId = _selectedFuel.Id,
                FuelColumnId = _selectedColumn.Id,
                Volume = _volume,
                UnitPrice = _selectedFuel.Price,
                TotalPrice = _totalPrice,
                Date = DateTime.Now,
                PaymentMethod = PaymentMethodComboBox.SelectedIndex == 1 ? "Картой" : "Наличными"
            };

            try
            {
                // Обновляем объем топлива
                _selectedFuel.Volume -= _volume;
                
                // Добавляем транзакцию
                _context.Transactions.Add(transaction);
                
                // Сохраняем изменения в базе данных
                await _context.SaveChangesAsync();

                // Обновляем отображение доступности топлива
                await UpdateFuelAvailability();

                MessageBox.Show("Транзакция успешно выполнена!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                ClearInputs();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при выполнении транзакции: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task UpdateTransactionsGrid()
        {
            // Загружаем актуальные транзакции с включением связанных данных
            var transactions = await _context.Transactions
                .Include(t => t.Fuel)
                .Include(t => t.FuelColumn)
                .Where(t => t.UserId == _userId)
                .OrderByDescending(t => t.Date)
                .ToListAsync();

            _transactions.Clear();
            foreach (var transaction in transactions)
            {
                _transactions.Add(transaction);
            }
            HistoryDataGrid.ItemsSource = _transactions;
        }

        private void SaveProfileButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _currentUser.FirstName = FirstNameTextBox.Text;
                _currentUser.LastName = LastNameTextBox.Text;
                _currentUser.Phone = PhoneTextBox.Text;

                _context.SaveChanges();
                MessageBox.Show("Профиль успешно обновлен", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении профиля: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            Close();
        }

        private void ClearInputs()
        {
            FuelComboBox.SelectedItem = null;
            ColumnComboBox.SelectedItem = null;
            VolumeTextBox.Text = "";
            PaymentMethodComboBox.SelectedIndex = 0;
        }

        private void ReservationButton_Click(object sender, RoutedEventArgs e)
        {
            var reservationWindow = new ReservationWindow(_context, _userId);
            reservationWindow.Owner = this;
            reservationWindow.ShowDialog();
        }

        private void NavigationListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (NavigationListBox?.SelectedItem == null || 
                RefuelContent == null || 
                ProfileContent == null || 
                HistoryContent == null) return;

            var selectedItem = NavigationListBox.SelectedItem as ListBoxItem;
            if (selectedItem == null) return;

            // Скрываем все контенты
            RefuelContent.Visibility = Visibility.Collapsed;
            ProfileContent.Visibility = Visibility.Collapsed;
            HistoryContent.Visibility = Visibility.Collapsed;

            // Показываем выбранный контент
            if (selectedItem.Name == "RefuelTab")
            {
                RefuelContent.Visibility = Visibility.Visible;
            }
            else if (selectedItem.Name == "ProfileTab")
            {
                ProfileContent.Visibility = Visibility.Visible;
            }
            else if (selectedItem.Name == "HistoryTab")
            {
                HistoryContent.Visibility = Visibility.Visible;
                LoadHistoryData();
            }
        }

        private async void LoadHistoryData()
        {
            if (HistoryDataGrid == null) return;

            try 
            {
                var history = await _context.Transactions
                    .Include(t => t.Fuel)
                    .Include(t => t.FuelColumn)
                    .Where(t => t.UserId == _userId)
                    .OrderByDescending(t => t.Date)
                    .Select(t => new
                    {
                        Date = t.Date,
                        Type = "Заправка",
                        Description = $"{t.Fuel.Name} - {t.Volume:N2}л (Колонка {t.FuelColumn.Number})",
                        Amount = t.TotalPrice
                    })
                    .ToListAsync();

                HistoryDataGrid.ItemsSource = history;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке истории: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void NewPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            var password = NewPasswordBox.Password;
            var requirements = new[]
            {
                (password.Length >= 6, "• Минимум 6 символов"),
                (password.Any(char.IsUpper), "• Хотя бы одна заглавная буква"),
                (password.Any(char.IsLower), "• Хотя бы одна строчная буква"),
                (password.Any(char.IsDigit), "• Хотя бы одна цифра")
            };

            var text = "Требования к паролю:\n";
            foreach (var (isMet, requirement) in requirements)
            {
                text += requirement + (isMet ? " ✓" : "") + "\n";
            }

            PasswordRequirementsTextBlock.Text = text;
            
            // Включаем кнопку смены пароля только если все требования выполнены
            ChangePasswordButton.IsEnabled = requirements.All(r => r.Item1) &&
                                          !string.IsNullOrEmpty(CurrentPasswordBox.Password) &&
                                          !string.IsNullOrEmpty(ConfirmPasswordBox.Password) &&
                                          NewPasswordBox.Password == ConfirmPasswordBox.Password;
        }

        private bool IsPasswordValid(string password)
        {
            return password.Length >= 6 &&
                   password.Any(char.IsUpper) &&
                   password.Any(char.IsLower) &&
                   password.Any(char.IsDigit);
        }

        private async void ChangePasswordButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Проверяем текущий пароль
                if (_currentUser.Password != CurrentPasswordBox.Password)
                {
                    MessageBox.Show("Неверный текущий пароль", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Проверяем совпадение паролей
                if (NewPasswordBox.Password != ConfirmPasswordBox.Password)
                {
                    MessageBox.Show("Новые пароли не совпадают", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Проверяем требования к паролю
                var password = NewPasswordBox.Password;
                if (!IsPasswordValid(password))
                {
                    MessageBox.Show("Новый пароль не соответствует требованиям", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Сохраняем новый пароль
                _currentUser.Password = password;
                await _context.SaveChangesAsync();

                MessageBox.Show("Пароль успешно изменен", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                // Очищаем поля
                CurrentPasswordBox.Clear();
                NewPasswordBox.Clear();
                ConfirmPasswordBox.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при смене пароля: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
