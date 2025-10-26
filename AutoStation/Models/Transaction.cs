using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoStation.Models
{
    public class Transaction
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("User")]
        public int UserId { get; set; }
        public virtual User User { get; set; } = null!;

        [Required]
        [ForeignKey("Fuel")]
        public int FuelId { get; set; }
        public virtual Fuel Fuel { get; set; } = null!;

        [Required]
        [ForeignKey("FuelColumn")]
        public int FuelColumnId { get; set; }
        public virtual FuelColumn FuelColumn { get; set; } = null!;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Volume { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        [MaxLength(50)]
        public string PaymentMethod { get; set; } = null!;
    }
}
