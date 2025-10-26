using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoStation.Models
{
    public class FuelColumnFuel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("FuelColumn")]
        public int FuelColumnId { get; set; }
        public virtual FuelColumn FuelColumn { get; set; } = null!;

        [Required]
        [ForeignKey("Fuel")]
        public int FuelId { get; set; }
        public virtual Fuel Fuel { get; set; } = null!;
    }
}
