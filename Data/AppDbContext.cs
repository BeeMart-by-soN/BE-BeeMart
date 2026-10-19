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
        public DbSet<Product> Product { get; set; }
        public DbSet<User> User { get; set; }
        public DbSet<TrTransaction> Transaction { get; set; }
        public DbSet<TransactionDetails> TransactionDetail { get; set; }
    }
}