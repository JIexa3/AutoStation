using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AutoStation.Data;
using AutoStation.Models;

namespace AutoStation.Views
{
    /// <summary>
    /// Логика взаимодействия для PaymentWindow.xaml
    /// </summary>
    public partial class PaymentWindow : Window
    {
        private readonly AutoStationContext _context;
        private readonly User _user;
        private readonly Fuel _fuel;
        private readonly decimal _volume;
        private readonly int _columnId;
        private static readonly Regex NumberRegex = new(@"^[0-9]+$");

        public PaymentWindow(AutoStationContext context, User user, Fuel fuel, decimal volume, int columnId)
        {
            InitializeComponent();
            _context = context;
            _user = user;
            _fuel = fuel;
            _volume = volume;
            _columnId = columnId;

            CardNumberTextBox.TextChanged += CardNumberTextBox_TextChanged;
            ExpiryDateTextBox.TextChanged += ExpiryDateTextBox_TextChanged;
            CardNumberTextBox.PreviewTextInput += CardNumberTextBox_PreviewTextInput;
            ExpiryDateTextBox.PreviewTextInput += ExpiryDateTextBox_PreviewTextInput;
            CvvPasswordBox.PreviewTextInput += CvvPasswordBox_PreviewTextInput;
            CardNumberTextBox.PreviewKeyDown += TextBox_PreviewKeyDown;
            ExpiryDateTextBox.PreviewKeyDown += TextBox_PreviewKeyDown;
            CvvPasswordBox.PreviewKeyDown += TextBox_PreviewKeyDown;

            UpdateOrderInfo();
        }

        private void UpdateOrderInfo()
        {
            FuelTypeText.Text = $"Топливо: {_fuel.Name}";
            VolumeText.Text = $"Объем: {_volume:N2} л";
            TotalAmountText.Text = $"Сумма к оплате: {_volume * _fuel.Price:N2} ₽";
        }

        private void CardNumberTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                string text = textBox.Text.Replace(" ", "");
                string formatted = "";

                for (int i = 0; i < text.Length && i < 16; i++)
                {
                    if (i > 0 && i % 4 == 0)
                    {
                        formatted += " ";
                    }
                    formatted += text[i];
                }

                if (formatted != textBox.Text)
                {
                    int caretIndex = textBox.CaretIndex;
                    int newSpaces = formatted.Take(caretIndex).Count(c => c == ' ');
                    int oldSpaces = textBox.Text.Take(caretIndex).Count(c => c == ' ');
                    textBox.Text = formatted;
                    textBox.CaretIndex = Math.Min(caretIndex + (newSpaces - oldSpaces), formatted.Length);
                }
            }
        }

        private void ExpiryDateTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                string text = textBox.Text.Replace("/", "");
                if (text.Length > 0)
                {
                    string formatted = text.Length > 2 
                        ? text.Insert(2, "/") 
                        : text;

                    if (formatted != textBox.Text)
                    {
                        int caretIndex = textBox.CaretIndex;
                        textBox.Text = formatted;
                        textBox.CaretIndex = Math.Min(caretIndex + 1, formatted.Length);
                    }
                }
            }
        }

        private void CardNumberTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !NumberRegex.IsMatch(e.Text);
        }

        private void ExpiryDateTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !NumberRegex.IsMatch(e.Text);
        }

        private void CvvPasswordBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !NumberRegex.IsMatch(e.Text);
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true;
            }
        }

        private bool ValidateCardDetails()
        {
            // Validate card number (should be 16 digits)
            string cardNumber = CardNumberTextBox.Text.Replace(" ", "");
            if (cardNumber.Length != 16 || !cardNumber.All(char.IsDigit))
            {
                MessageBox.Show("Введите корректный номер карты (16 цифр)", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            // Validate expiry date (should be MM/YY format)
            string expiryDate = ExpiryDateTextBox.Text;
            if (!Regex.IsMatch(expiryDate, @"^(0[1-9]|1[0-2])/([0-9]{2})$"))
            {
                MessageBox.Show("Введите корректную дату в формате ММ/ГГ", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            // Validate CVV (should be 3 digits)
            string cvv = CvvPasswordBox.Password;
            if (cvv.Length != 3 || !cvv.All(char.IsDigit))
            {
                MessageBox.Show("Введите корректный CVV код (3 цифры)", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            // Validate expiry date is not in the past
            var parts = expiryDate.Split('/');
            int month = int.Parse(parts[0]);
            int year = 2000 + int.Parse(parts[1]);
            var cardExpiry = new DateTime(year, month, 1).AddMonths(1).AddDays(-1);
            
            if (cardExpiry < DateTime.Now)
            {
                MessageBox.Show("Срок действия карты истек", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }

        private void PayButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateCardDetails())
            {
                return;
            }

            // In a real application, here would be the actual payment processing
            // For now, we'll just simulate a successful payment
            DialogResult = true;
            Close();
        }
    }
}
