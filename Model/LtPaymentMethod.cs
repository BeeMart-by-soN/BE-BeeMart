using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace S1.Model
{
    public class LtPaymentMethod
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdPaymentMethod { get; set; }

        [Required]
        [MaxLength(50)]
        public string MethodName { get; set; } = string.Empty;

        // Default eksplisit true agar EF mengirim nilai pada INSERT (bukan mengandalkan
        // default constraint DB). Tanpa ini INSERT dengan Stsrc NULL akan gagal.
        public bool Stsrc { get; set; } = true;

        public DateTime Datein { get; set; }

        [Required]
        [MaxLength(36)]
        public string Userin { get; set; } = string.Empty;

        public DateTime? Dateup { get; set; }

        [MaxLength(36)]
        public string? Userup { get; set; }

        // Navigation: transaksi yang memakai metode ini (1 — N)
        public ICollection<TrTransaction> Transactions { get; set; } = new List<TrTransaction>();
    }
}
