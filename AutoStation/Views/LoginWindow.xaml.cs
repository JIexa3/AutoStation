using System.Linq;
using System.Windows;
using AutoStation.Data;
using AutoStation.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoStation.Views
{
    public partial class LoginWindow : Window
    {
        private readonly AutoStationContext _context;

        public LoginWindow()
        {
            InitializeComponent();
            _context = new AutoStationContext();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            var username = UsernameTextBox.Text;
            var password = PasswordBox.Password;

            var user = _context.Users.FirstOrDefault(u => u.Username == username && u.Password == password);

            if (user == null)
            {
                MessageBox.Show("Неверное имя пользователя или пароль", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!user.IsEmailVerified)
            {
                var verificationWindow = new EmailVerificationWindow(user.Id);
                verificationWindow.Show();
                Close();
                return;
            }

            if (user.IsAdmin)
            {
                var adminWindow = new AdminWindow(user.Id);
                adminWindow.Show();
            }
            else
            {
                var userWindow = new UserWindow(user.Id);
                userWindow.Show();
            }

            Close();
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            var registrationWindow = new RegisterWindow();
            registrationWindow.Show();
            Close();
        }
    }
}
