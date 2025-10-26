using System;
using System.Linq;
using System.Windows;
using AutoStation.Data;
using AutoStation.Models;
using AutoStation.Services;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace AutoStation.Views
{
    public partial class RegisterWindow : Window
    {
        private readonly AutoStationContext _context;
        private readonly EmailService _emailService;
        private static readonly Regex EmailRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

        public RegisterWindow()
        {
            InitializeComponent();
            _context = new AutoStationContext();
            _emailService = new EmailService();
        }

        private async void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var username = UsernameTextBox.Text.Trim();
                var email = EmailTextBox.Text.Trim();
                var password = PasswordBox.Password;
                var confirmPassword = ConfirmPasswordBox.Password;

                if (string.IsNullOrWhiteSpace(username) || 
                    string.IsNullOrWhiteSpace(email) || 
                    string.IsNullOrWhiteSpace(password))
                {
                    MessageBox.Show("Все поля должны быть заполнены", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (!EmailRegex.IsMatch(email))
                {
                    MessageBox.Show("Введите корректный email адрес", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (password != confirmPassword)
                {
                    MessageBox.Show("Пароли не совпадают", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (await _context.Users.AnyAsync(u => u.Username == username))
                {
                    MessageBox.Show("Пользователь с таким именем уже существует", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (await _context.Users.AnyAsync(u => u.Email == email))
                {
                    MessageBox.Show("Пользователь с таким email уже существует", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var verificationCode = GenerateVerificationCode();
                var user = new User
                {
                    Username = username,
                    Email = email,
                    Password = password,
                    IsAdmin = false,
                    IsEmailVerified = false,
                    VerificationCode = verificationCode,
                    VerificationCodeExpiry = DateTime.Now.AddDays(1)
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                await _emailService.SendVerificationCodeAsync(email, verificationCode);

                var verificationWindow = new EmailVerificationWindow(user.Id);
                verificationWindow.Show();
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при регистрации: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GenerateVerificationCode()
        {
            return new Random().Next(100000, 999999).ToString();
        }

        private void BackToLoginButton_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            Close();
        }
    }
}
