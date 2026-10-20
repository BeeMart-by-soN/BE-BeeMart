using Microsoft.EntityFrameworkCore;
using S1.Model; 

namespace S1.Data
{
    public class AppDbContext: DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<LtPaymentMethod> LtPaymentMethod { get; set;}
        public DbSet<MsProduct> MsProduct { get; set; }
        public DbSet<MsUser> MsUser { get; set; }
        public DbSet<TrTransaction> TrTransaction { get; set; }
        public DbSet<TrTransactionDetail> TrTransactionDetail { get; set; }
    }
}