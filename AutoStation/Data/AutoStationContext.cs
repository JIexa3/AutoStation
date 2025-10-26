using System;
using AutoStation.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoStation.Data
{
    public class AutoStationContext : DbContext
    {
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Fuel> Fuels { get; set; } = null!;
        public DbSet<FuelColumn> FuelColumns { get; set; } = null!;
        public DbSet<Transaction> Transactions { get; set; } = null!;
        public DbSet<Reservation> Reservations { get; set; } = null!;
        public DbSet<FuelColumnFuel> FuelColumnFuels { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(@"Data Source=HOME-PC;Initial Catalog=AutoStation;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False");
            }
        }

        public void EnsureDatabaseCreated()
        {
            try
            {
                Database.EnsureCreated();
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при создании базы данных: {ex.Message}", ex);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Password).IsRequired().HasMaxLength(100);
                entity.Property(e => e.IsAdmin).IsRequired().HasDefaultValue(false);
                entity.Property(e => e.IsEmailVerified).IsRequired().HasDefaultValue(false);
                entity.Property(e => e.VerificationCode).IsRequired().HasMaxLength(50);
                entity.Property(e => e.VerificationCodeExpiry).IsRequired();

                entity.HasMany(e => e.Transactions)
                    .WithOne(e => e.User)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(e => e.Reservations)
                    .WithOne(e => e.User)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Fuel>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Price).IsRequired().HasColumnType("decimal(18,2)");
                entity.Property(e => e.Volume).IsRequired().HasColumnType("decimal(18,2)");
                entity.Property(e => e.IsAvailable).IsRequired().HasDefaultValue(true);

                entity.HasMany(e => e.FuelColumnFuels)
                    .WithOne(e => e.Fuel)
                    .HasForeignKey(e => e.FuelId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(e => e.Transactions)
                    .WithOne(e => e.Fuel)
                    .HasForeignKey(e => e.FuelId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<FuelColumn>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Number).IsRequired();
                entity.Property(e => e.IsAvailable).IsRequired().HasDefaultValue(true);

                entity.HasMany(e => e.FuelColumnFuels)
                    .WithOne(e => e.FuelColumn)
                    .HasForeignKey(e => e.FuelColumnId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(e => e.Transactions)
                    .WithOne(e => e.FuelColumn)
                    .HasForeignKey(e => e.FuelColumnId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(e => e.Reservations)
                    .WithOne(e => e.FuelColumn)
                    .HasForeignKey(e => e.FuelColumnId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<FuelColumnFuel>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.FuelColumnId, e.FuelId }).IsUnique();
            });

            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Volume).IsRequired().HasColumnType("decimal(18,2)");
                entity.Property(e => e.UnitPrice).IsRequired().HasColumnType("decimal(18,2)");
                entity.Property(e => e.TotalPrice).IsRequired().HasColumnType("decimal(18,2)");
                entity.Property(e => e.Date).IsRequired();
                entity.Property(e => e.PaymentMethod).IsRequired().HasMaxLength(50);
            });

            modelBuilder.Entity<Reservation>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ReservationTime).IsRequired();
                entity.Property(e => e.ExpirationTime).IsRequired();
                entity.Property(e => e.IsActive).IsRequired().HasDefaultValue(true);
            });

            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Username = "admin",
                    Email = "admin@autostation.com",
                    Password = "admin123",
                    IsAdmin = true,
                    IsEmailVerified = true,
                    VerificationCode = "123456",
                    VerificationCodeExpiry = DateTime.Now.AddYears(1)
                }
            );

            modelBuilder.Entity<Fuel>().HasData(
                new Fuel { Id = 1, Name = "АИ-92", Price = 45.50M, Volume = 1000, IsAvailable = true },
                new Fuel { Id = 2, Name = "АИ-95", Price = 48.30M, Volume = 1000, IsAvailable = true },
                new Fuel { Id = 3, Name = "АИ-98", Price = 51.20M, Volume = 1000, IsAvailable = true },
                new Fuel { Id = 4, Name = "ДТ", Price = 49.90M, Volume = 1000, IsAvailable = true }
            );

            modelBuilder.Entity<FuelColumn>().HasData(
                new FuelColumn { Id = 1, Number = 1, IsAvailable = true },
                new FuelColumn { Id = 2, Number = 2, IsAvailable = true },
                new FuelColumn { Id = 3, Number = 3, IsAvailable = true },
                new FuelColumn { Id = 4, Number = 4, IsAvailable = true }
            );

            modelBuilder.Entity<FuelColumnFuel>().HasData(
                new FuelColumnFuel { Id = 1, FuelColumnId = 1, FuelId = 1 },
                new FuelColumnFuel { Id = 2, FuelColumnId = 2, FuelId = 2 },
                new FuelColumnFuel { Id = 3, FuelColumnId = 3, FuelId = 3 },
                new FuelColumnFuel { Id = 4, FuelColumnId = 4, FuelId = 4 }
            );
        }
    }
}
