using System.ComponentModel.DataAnnotations;

namespace S1.Model
{
    public class MsUser
    {
        [Key]
        [StringLength(5)]
        public string IdUser { get; set; } = string.Empty;

        [Required]
        [MinLength(3)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [MinLength(3)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = string.Empty;

        public bool Stsrc { get; set; } = true;

        public DateTime Datein { get; set; }

        [Required]
        [MaxLength(36)]
        public string Userin { get; set; } = string.Empty;

        public DateTime? Dateup { get; set; }

        [MaxLength(36)]
        public string? Userup { get; set; }

        // Selalu diisi true untuk data aktif. Nilai default eksplisit (bukan hanya
        // default constraint DB) agar Entity Framework benar-benar mengirim nilai
        // pada INSERT — tanpa ini INSERT gagal: "Cannot insert the value NULL into
        // column 'Stsrc'".
        public ICollection<TrTransaction> Transactions { get; set; } = new List<TrTransaction>();
    }
}
