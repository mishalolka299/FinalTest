using FinalTest.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace FinalTest.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Car> Cars => Set<Car>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Sale> Sales => Set<Sale>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Car>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Make).HasMaxLength(100).IsRequired();
            entity.Property(c => c.Model).HasMaxLength(100).IsRequired();
            entity.Property(c => c.Color).HasMaxLength(50).IsRequired();
            entity.Property(c => c.VIN).HasMaxLength(17).IsRequired();
            entity.HasIndex(c => c.VIN).IsUnique();
            entity.Property(c => c.Price);
            entity.Property(c => c.Mileage);
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(c => c.LastName).HasMaxLength(100).IsRequired();
            entity.Property(c => c.Email).HasMaxLength(150).IsRequired();
            entity.Property(c => c.Phone).HasMaxLength(20).IsRequired();
            entity.Property(c => c.DriversLicense).HasMaxLength(50).IsRequired();
        });

        modelBuilder.Entity<Sale>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.SalePrice);

            entity.HasOne(s => s.Car)
                .WithMany(c => c.Sales)
                .HasForeignKey(s => s.CarId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(s => s.Customer)
                .WithMany(c => c.Sales)
                .HasForeignKey(s => s.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        base.OnModelCreating(modelBuilder);
    }
}
