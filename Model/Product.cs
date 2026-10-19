using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace S1.Model
{
    [Table("MsProduct")]
    public class Product
    {
        [Key]
        // [DatabaseGenerated]
        [Column("IdProduct")]
        public string ProductID {get; set;} = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Name {get; set;} = string.Empty;
        [Column(TypeName = "decimal(10,2)")]
        public decimal Weight {get; set;}

        [Required]
        public int Stock {get; set;}
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price {get; set;}

        [Column(TypeName = "decimal(5,2)")]
        public decimal Discount {get; set;}
        public bool Stsrc { get; set; }

        public DateTime Datein { get; set; }

        [Required]
        [MaxLength(36)]
        public string Userin { get; set; } = string.Empty;

        public DateTime? Dateup { get; set; }

        [MaxLength(36)]
        public string? Userup { get; set; }
    }
}