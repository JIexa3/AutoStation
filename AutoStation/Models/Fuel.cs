using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace AutoStation.Models
{
    public class Fuel : INotifyPropertyChanged
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string Name { get; set; } = null!;
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Volume { get; set; }
        
        public bool IsAvailable { get; set; }

        public virtual ICollection<FuelColumnFuel> FuelColumnFuels { get; set; } = new List<FuelColumnFuel>();
        public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

        [NotMapped]
        public bool IsSelected { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
