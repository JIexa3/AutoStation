using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AutoStation.Data;
using AutoStation.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoStation.Views
{
    public partial class ReservationWindow : Window
    {
        private readonly AutoStationContext _context;
        private readonly int _userId;
        private ObservableCollection<Reservation> _userReservations;

        public ReservationWindow(AutoStationContext context, int userId)
        {
            InitializeComponent();
            _context = context;
            _userId = userId;

            LoadColumns();
            LoadTimeSlots();
            LoadUserReservations();

            // Устанавливаем минимальную дату как сегодня
            ReservationDatePicker.DisplayDateStart = DateTime.Today;
            ReservationDatePicker.DisplayDateEnd = DateTime.Today.AddMonths(1);
            ReservationDatePicker.SelectedDate = DateTime.Today;
        }

        private void LoadColumns()
        {
            var columns = _context.FuelColumns.ToList();
            ColumnComboBox.ItemsSource = columns;
        }

        private void LoadTimeSlots()
        {
            // Создаем временные слоты с 8:00 до 22:00 с интервалом в 15 минут
            var timeSlots = new List<DateTime>();
            var startTime = new DateTime(2000, 1, 1, 8, 0, 0);
            var endTime = new DateTime(2000, 1, 1, 22, 0, 0);

            while (startTime <= endTime)
            {
                timeSlots.Add(startTime);
                startTime = startTime.AddMinutes(15);
            }

            TimeComboBox.ItemsSource = timeSlots;
            TimeComboBox.SelectedValuePath = "TimeOfDay";
        }

        private void LoadUserReservations()
        {
            _userReservations = new ObservableCollection<Reservation>(
                _context.Reservations
                    .Include(r => r.FuelColumn)
                    .Where(r => r.UserId == _userId && r.ReservationTime >= DateTime.Today)
                    .OrderBy(r => r.ReservationTime)
                    .ToList()
            );
            ReservationsDataGrid.ItemsSource = _userReservations;
        }

        private void ReserveButton_Click(object sender, RoutedEventArgs e)
        {
            if (ColumnComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите колонку", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (ReservationDatePicker.SelectedDate == null)
            {
                MessageBox.Show("Выберите дату", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (TimeComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите время", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selectedColumn = (FuelColumn)ColumnComboBox.SelectedItem;
            var selectedDate = ReservationDatePicker.SelectedDate.Value;
            var selectedTime = ((DateTime)TimeComboBox.SelectedItem).TimeOfDay;
            
            var selectedDateTime = selectedDate.Date + selectedTime;

            // Проверяем, нет ли уже активного бронирования на это время
            var existingReservation = _context.Reservations
                .Any(r => r.FuelColumnId == selectedColumn.Id 
                    && r.ReservationTime <= selectedDateTime 
                    && r.ExpirationTime >= selectedDateTime);

            if (existingReservation)
            {
                MessageBox.Show("Эта колонка уже забронирована на выбранное время", 
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Проверяем количество бронирований пользователя на этот день
            var userReservationsCount = _context.Reservations
                .Count(r => r.UserId == _userId && 
                           r.ReservationTime.Date == selectedDate.Date);

            if (userReservationsCount >= 2)
            {
                MessageBox.Show("Вы не можете забронировать более 2 колонок на один день", 
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var reservation = new Reservation
            {
                UserId = _userId,
                FuelColumnId = selectedColumn.Id,
                ReservationTime = selectedDateTime,
                ExpirationTime = selectedDateTime.AddMinutes(15)
            };

            try
            {
                _context.Reservations.Add(reservation);
                _context.SaveChanges();

                // Обновляем список бронирований
                _userReservations.Add(reservation);
                MessageBox.Show("Колонка успешно забронирована на 15 минут!", "Успех", 
                    MessageBoxButton.OK, MessageBoxImage.Information);

                // Очищаем выбранные значения
                ColumnComboBox.SelectedItem = null;
                TimeComboBox.SelectedItem = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при бронировании: {ex.Message}", 
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelReservation_Click(object sender, RoutedEventArgs e)
        {
            var reservation = (Reservation)((FrameworkElement)sender).DataContext;

            if (MessageBox.Show("Вы уверены, что хотите отменить бронирование?", 
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    _context.Reservations.Remove(reservation);
                    _context.SaveChanges();
                    _userReservations.Remove(reservation);

                    MessageBox.Show("Бронирование успешно отменено", "Успех", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при отмене бронирования: {ex.Message}", 
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
