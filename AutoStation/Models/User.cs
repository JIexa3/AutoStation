using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AutoStation.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Username { get; set; } = null!;

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string Password { get; set; } = null!;

        [MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [MaxLength(50)]
        public string LastName { get; set; } = string.Empty;

        [MaxLength(20)]
        public string Phone { get; set; } = string.Empty;

        public bool IsAdmin { get; set; }
        
        public bool IsEmailVerified { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string VerificationCode { get; set; } = null!;
        
        [Required]
        public DateTime VerificationCodeExpiry { get; set; }

        public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
        public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}
