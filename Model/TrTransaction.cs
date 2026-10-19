using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection.Metadata;

namespace S1.Model
{
    [Table("TrTransaction")]
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

        public bool Stsrc { get; set; }

        public DateTime Datein { get; set; }

        [Required]
        [MaxLength(36)]
        public string Userin { get; set; } = string.Empty;

        public DateTime? Dateup { get; set; }

        [MaxLength(36)]
        public string? Userup { get; set; }

        [ForeignKey(nameof(IdUser))]
        public User User {get;set;} = null!;

        [ForeignKey(nameof(IdPaymentMethod))]
        public LtPaymentMethod PaymentMethod {get;set;} = null!;
    }
}