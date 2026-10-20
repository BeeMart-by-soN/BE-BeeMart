using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace S1.Model
{
    public class TrTransaction
    {
        [Key]
        [MaxLength(36)]
        public string IdTransaction { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string InvoiceNo {get; set;} = string.Empty;

        public DateTime TransactionDate {get; set;}

        [Required]
        [MaxLength(5)]
        public string IdUser { get; set; } = string.Empty;

        [Required]
        public int IdPaymentMethod { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice {get;set;}

        // Default eksplisit true agar EF mengirim nilai pada INSERT (bukan mengandalkan
        // default constraint DB).
        public bool Stsrc { get; set; } = true;

        public DateTime Datein { get; set; }

        [Required]
        [MaxLength(36)]
        public string Userin { get; set; } = string.Empty;

        public DateTime? Dateup { get; set; }

        [MaxLength(36)]
        public string? Userup { get; set; }

        [ForeignKey(nameof(IdUser))]
        public MsUser User { get; set; } = null!;

        [ForeignKey(nameof(IdPaymentMethod))]
        public LtPaymentMethod PaymentMethod { get; set; } = null!;

        // Navigation: rincian item dari transaksi ini (1 — N, CASCADE delete)
        public ICollection<TrTransactionDetail> Details { get; set; } = new List<TrTransactionDetail>();
    }
}
