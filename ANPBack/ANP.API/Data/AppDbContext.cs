using ANP.API.Models;
using Microsoft.EntityFrameworkCore;

namespace ANP.API.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<LineItem> LineItems => Set<LineItem>();
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>().HasKey(p => p.Code);

        modelBuilder.Entity<Product>().Property(p => p.DefaultPrice).HasPrecision(18, 2);
        modelBuilder.Entity<Invoice>().Property(i => i.TaxRate).HasPrecision(5, 4);
        modelBuilder.Entity<LineItem>().Property(l => l.UnitPrice).HasPrecision(18, 2);

        modelBuilder
            .Entity<Invoice>()
            .HasMany(i => i.LineItems)
            .WithOne(l => l.Invoice!)
            .HasForeignKey(l => l.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Invoice>().Property(i => i.Status).HasConversion<string>();
    }
}
