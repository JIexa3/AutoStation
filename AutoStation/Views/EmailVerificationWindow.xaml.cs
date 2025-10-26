using System;
using System.Windows;
using AutoStation.Data;
using AutoStation.Models;
using AutoStation.Services;
using Microsoft.EntityFrameworkCore;

namespace AutoStation.Views
{
    public partial class EmailVerificationWindow : Window
    {
        private readonly AutoStationContext _context;
        private readonly int _userId;
        private readonly EmailService _emailService;

        public EmailVerificationWindow(int userId)
        {
            InitializeComponent();
            _context = new AutoStationContext();
            _userId = userId;
            _emailService = new EmailService();
            SendVerificationCode();
        }

        private async void SendVerificationCode()
        {
            try
            {
                var user = await _context.Users.FindAsync(_userId);
                if (user == null)
                {
                    MessageBox.Show("Пользователь не найден", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    Close();
                    return;
                }

                var verificationCode = GenerateVerificationCode();
                user.VerificationCode = verificationCode;
                user.VerificationCodeExpiry = DateTime.Now.AddHours(24);
                await _context.SaveChangesAsync();

                await _emailService.SendVerificationCodeAsync(user.Email, verificationCode);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка отправки кода: {ex.Message}", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GenerateVerificationCode()
        {
            return new Random().Next(100000, 999999).ToString();
        }

        private async void VerifyButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var code = VerificationCodeTextBox.Text.Trim();
                if (string.IsNullOrEmpty(code))
                {
                    MessageBox.Show("Введите код подтверждения", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var user = await _context.Users.FindAsync(_userId);
                if (user == null)
                {
                    MessageBox.Show("Пользователь не найден", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    Close();
                    return;
                }

                if (user.VerificationCode != code)
                {
                    MessageBox.Show("Неверный код подтверждения", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (user.VerificationCodeExpiry < DateTime.Now)
                {
                    MessageBox.Show("Срок действия кода истек", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                user.IsEmailVerified = true;
                await _context.SaveChangesAsync();

                MessageBox.Show("Email успешно подтвержден", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                var loginWindow = new LoginWindow();
                loginWindow.Show();
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при подтверждении: {ex.Message}", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ResendButton_Click(object sender, RoutedEventArgs e)
        {
            SendVerificationCode();
        }

        private void BackToLoginButton_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            Close();
        }
    }
}
