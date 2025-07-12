using Microsoft.EntityFrameworkCore;

namespace Order.Api.Infrastructure
{
    public class OrderDbContext : DbContext
    {
        public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options) { }

        public DbSet<Models.Order> Orders => Set<Models.Order>();
        public DbSet<Models.OrderItem> OrderItems => Set<Models.OrderItem>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Models.Order>()
            .HasMany(o => o.Items)
            .WithOne()
            .HasForeignKey("OrderId")
            .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
