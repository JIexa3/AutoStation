using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoStation.Models
{
    public class Reservation
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("User")]
        public int UserId { get; set; }
        public virtual User User { get; set; } = null!;

        [Required]
        [ForeignKey("FuelColumn")]
        public int FuelColumnId { get; set; }
        public virtual FuelColumn FuelColumn { get; set; } = null!;

        [Required]
        public DateTime ReservationTime { get; set; }
        
        [Required]
        public DateTime ExpirationTime { get; set; }
        
        [Required]
        public bool IsActive { get; set; } = true;
    }
}
