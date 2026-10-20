using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace S1.Model
{
    public class TrTransactionDetail
    {
        [Key]
        [MaxLength(36)]
        public string IdTransactionDetail { get; set; } = string.Empty;

        [Required]
        [MaxLength(36)]
        public string IdTransaction { get; set; } = string.Empty;

        [Required]
        [MaxLength(36)]
        public string IdProduct { get; set; } = string.Empty;

        public int Qty { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal Discount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SubTotal { get; set; }

        // Default eksplisit true agar EF mengirim nilai pada INSERT (bukan mengandalkan
        // default constraint DB).
        public bool Stsrc { get; set; } = true;

        public DateTime Datein { get; set; }

        [MaxLength(36)]
        public string Userin { get; set; } = string.Empty;

        public DateTime? Dateup { get; set; }

        [MaxLength(36)]
        public string? Userup { get; set; }

        // Navigation
        [ForeignKey(nameof(IdTransaction))]
        public TrTransaction Transaction { get; set; } = null!;

        [ForeignKey(nameof(IdProduct))]
        public MsProduct Product { get; set; } = null!;
    }
}
