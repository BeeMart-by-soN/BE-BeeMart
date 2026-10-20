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
