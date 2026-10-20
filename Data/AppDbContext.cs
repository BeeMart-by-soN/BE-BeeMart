using Microsoft.EntityFrameworkCore;
using S1.Model;
using S1.Services;

namespace S1.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<LtPaymentMethod> LtPaymentMethod { get; set; } = null!;
    public DbSet<MsProduct> MsProduct { get; set; } = null!;
    public DbSet<MsUser> MsUser { get; set; } = null!;
    public DbSet<TrTransaction> TrTransaction { get; set; } = null!;
    public DbSet<TrTransactionDetail> TrTransactionDetail { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ---------- LtPaymentMethod ----------
        modelBuilder.Entity<LtPaymentMethod>(e =>
        {
            e.ToTable("LtPaymentMethod");
            e.HasKey(x => x.IdPaymentMethod);
            e.Property(x => x.IdPaymentMethod).UseIdentityColumn(1, 1);
            e.Property(x => x.MethodName).HasMaxLength(50).IsRequired();
            // Stsrc TIDAK memakai HasDefaultValue (lihat catatan pada MsUser).
            e.Property(x => x.Datein).HasColumnType("datetime2");
            e.Property(x => x.Userin).HasMaxLength(36).IsRequired();
            e.Property(x => x.Dateup).HasColumnType("datetime2");
            e.Property(x => x.Userup).HasMaxLength(36);
        });

        // ---------- MsUser ----------
        modelBuilder.Entity<MsUser>(e =>
        {
            e.ToTable("MsUser");
            e.HasKey(x => x.IdUser);
            e.Property(x => x.IdUser).HasMaxLength(5).IsRequired();
            e.Property(x => x.Username).HasMaxLength(50).IsRequired();
            // Unique index IX_MsUser_Username
            e.HasIndex(x => x.Username).IsUnique().HasDatabaseName("IX_MsUser_Username");
            e.Property(x => x.FullName).HasMaxLength(300).IsRequired();
            e.Property(x => x.PasswordHash).HasMaxLength(255).IsRequired();
            e.Property(x => x.Role).HasMaxLength(20).IsRequired();
            // CATATAN: jangan memakai HasDefaultValue di sini. Dengan default value,
            // EF menganggap nilai default (true) sebagai "tidak diisi" sehingga kolom
            // Stsrc dikeluarkan dari perintah INSERT dan gagal: "Cannot insert the value
            // NULL into column 'Stsrc'". Nilai Stsrc selalu diisi eksplisit dari entity.
            e.Property(x => x.Datein).HasColumnType("datetime2");
            e.Property(x => x.Userin).HasMaxLength(36).IsRequired();
            e.Property(x => x.Dateup).HasColumnType("datetime2");
            e.Property(x => x.Userup).HasMaxLength(36);
        });

        // ---------- MsProduct ----------
        modelBuilder.Entity<MsProduct>(e =>
        {
            e.ToTable("MsProduct");
            e.HasKey(x => x.IdProduct);
            e.Property(x => x.IdProduct).HasMaxLength(36).IsRequired();
            e.Property(x => x.Name).HasMaxLength(100).IsRequired();
            // Index IX_MsProduct_Name
            e.HasIndex(x => x.Name).HasDatabaseName("IX_MsProduct_Name");
            e.Property(x => x.Weight).HasColumnType("decimal(10,2)");
            e.Property(x => x.Stock).IsRequired();
            e.Property(x => x.Price).HasColumnType("decimal(18,2)").IsRequired();
            e.Property(x => x.Discount).HasColumnType("decimal(5,2)").HasDefaultValue(0m);
            // Stsrc TIDAK memakai HasDefaultValue (lihat catatan pada MsUser).
            e.Property(x => x.Datein).HasColumnType("datetime2");
            e.Property(x => x.Userin).HasMaxLength(36).IsRequired();
            e.Property(x => x.Dateup).HasColumnType("datetime2");
            e.Property(x => x.Userup).HasMaxLength(36);
        });

        // ---------- TrTransaction ----------
        modelBuilder.Entity<TrTransaction>(e =>
        {
            e.ToTable("TrTransaction");
            e.HasKey(x => x.IdTransaction);
            e.Property(x => x.IdTransaction).HasMaxLength(36).IsRequired();
            e.Property(x => x.InvoiceNo).HasMaxLength(20).IsRequired();
            // Unique index InvoiceNo
            e.HasIndex(x => x.InvoiceNo).IsUnique().HasDatabaseName("IX_TrTransaction_InvoiceNo");
            e.Property(x => x.TransactionDate).HasColumnType("datetime2");
            // Index TransactionDate
            e.HasIndex(x => x.TransactionDate).HasDatabaseName("IX_TrTransaction_TransactionDate");
            e.Property(x => x.IdUser).HasMaxLength(5).IsRequired();
            e.Property(x => x.TotalPrice).HasColumnType("decimal(18,2)");
            // Stsrc TIDAK memakai HasDefaultValue (lihat catatan pada MsUser).
            e.Property(x => x.Datein).HasColumnType("datetime2");
            e.Property(x => x.Userin).HasMaxLength(36).IsRequired();
            e.Property(x => x.Dateup).HasColumnType("datetime2");
            e.Property(x => x.Userup).HasMaxLength(36);

            // FK -> MsUser (NO ACTION)
            e.HasOne(x => x.User)
                .WithMany(u => u.Transactions)
                .HasForeignKey(x => x.IdUser)
                .OnDelete(DeleteBehavior.NoAction);

            // FK -> LtPaymentMethod (NO ACTION)
            e.HasOne(x => x.PaymentMethod)
                .WithMany(p => p.Transactions)
                .HasForeignKey(x => x.IdPaymentMethod)
                .OnDelete(DeleteBehavior.NoAction);
        });

        // ---------- TrTransactionDetail ----------
        modelBuilder.Entity<TrTransactionDetail>(e =>
        {
            e.ToTable("TrTransactionDetail");
            e.HasKey(x => x.IdTransactionDetail);
            e.Property(x => x.IdTransactionDetail).HasMaxLength(36).IsRequired();
            e.Property(x => x.IdTransaction).HasMaxLength(36).IsRequired();
            e.Property(x => x.IdProduct).HasMaxLength(36).IsRequired();
            e.Property(x => x.Qty).IsRequired();
            e.Property(x => x.Price).HasColumnType("decimal(18,2)");
            e.Property(x => x.Discount).HasColumnType("decimal(5,2)").HasDefaultValue(0m);
            e.Property(x => x.SubTotal).HasColumnType("decimal(18,2)");
            // Stsrc TIDAK memakai HasDefaultValue (lihat catatan pada MsUser).
            e.Property(x => x.Datein).HasColumnType("datetime2");
            e.Property(x => x.Userin).HasMaxLength(36).IsRequired();
            e.Property(x => x.Dateup).HasColumnType("datetime2");
            e.Property(x => x.Userup).HasMaxLength(36);

            // FK -> TrTransaction (CASCADE)
            e.HasOne(x => x.Transaction)
                .WithMany(t => t.Details)
                .HasForeignKey(x => x.IdTransaction)
                .OnDelete(DeleteBehavior.Cascade);

            // FK -> MsProduct (NO ACTION)
            e.HasOne(x => x.Product)
                .WithMany(p => p.TransactionDetails)
                .HasForeignKey(x => x.IdProduct)
                .OnDelete(DeleteBehavior.NoAction);
        });
    }

    /// <summary>
    /// Seeder: isi data awal (payment method + user demo) bila tabel masih kosong.
    /// Dipanggil saat startup. Password demo sudah di-hash dengan BCrypt (algoritma yang sama
    /// dengan yang dipakai AuthController) agar kompatibel dengan login.
    /// EnsureCreatedAsync membuat database + tabel bila belum ada (idempoten bila
    /// script SQL sudah dijalankan). Untuk seed produk lengkap, jalankan Database/BeeMart.sql.
    /// </summary>
    public async Task SeedAsync()
    {
        // Buat database + tabel bila belum ada (tidak menyentuh DB yang sudah ada).
        await Database.EnsureCreatedAsync();

        var now = WibTime.Now;

        // --- Payment methods ---
        if (!await LtPaymentMethod.AnyAsync())
        {
            await LtPaymentMethod.AddRangeAsync(
                new LtPaymentMethod { MethodName = "Cash",   Stsrc = true, Datein = now, Userin = "SYSTEM" },
                new LtPaymentMethod { MethodName = "Debit",  Stsrc = true, Datein = now, Userin = "SYSTEM" },
                new LtPaymentMethod { MethodName = "Credit", Stsrc = true, Datein = now, Userin = "SYSTEM" }
            );
        }

        // --- Demo users (password di-hash BCrypt workFactor 11) ---
        if (!await MsUser.AnyAsync())
        {
            await MsUser.AddRangeAsync(
                new MsUser
                {
                    IdUser = "10001",
                    Username = "admin",
                    FullName = "Administrator",
                    PasswordHash = "$2a$11$bTzzTbN/.EJzfqxpuqzsn.ibF4IusILOmmd3kDCdtgwqp8CBId2Kq", // admin123
                    Role = "Admin",
                    Stsrc = true,
                    Datein = now,
                    Userin = "SYSTEM"
                },
                new MsUser
                {
                    IdUser = "76985",
                    Username = "steven",
                    FullName = "Steven",
                    PasswordHash = "$2a$11$s0MQnW/ss2EVYKMNFoUPH..kzCybGX3MXnd2uJOrmdReUt.FqTXla", // steven123
                    Role = "Cashier",
                    Stsrc = true,
                    Datein = now,
                    Userin = "SYSTEM"
                },
                new MsUser
                {
                    IdUser = "66859",
                    Username = "cashier",
                    FullName = "Cashier Demo",
                    PasswordHash = "$2a$11$gmQl4rZfR5bkAcSqJWQhyOcr0OwOBbKMB3SuwhcXe6k6/nCbfI34C", // cashier123
                    Role = "Cashier",
                    Stsrc = true,
                    Datein = now,
                    Userin = "SYSTEM"
                }
            );
        }

        await SaveChangesAsync();
    }
}
