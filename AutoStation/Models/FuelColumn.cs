using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AutoStation.Models
{
    public class FuelColumn
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int Number { get; set; }
        
        public bool IsAvailable { get; set; } = true;

        public virtual ICollection<FuelColumnFuel> FuelColumnFuels { get; set; } = new List<FuelColumnFuel>();
        public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
        public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}
