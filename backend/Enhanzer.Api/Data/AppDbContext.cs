using Enhanzer.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Enhanzer.Api.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<LocationDetail> LocationDetails => Set<LocationDetail>();
    public DbSet<PurchaseBill> PurchaseBills => Set<PurchaseBill>();
    public DbSet<PurchaseBillItem> PurchaseBillItems => Set<PurchaseBillItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LocationDetail>(entity =>
        {
            entity.ToTable("Location_Details");
            entity.HasKey(location => location.Id);
            entity.Property(location => location.LocationCode).HasColumnName("Location_Code").HasMaxLength(50).IsRequired();
            entity.Property(location => location.LocationName).HasColumnName("Location_Name").HasMaxLength(200).IsRequired();
            entity.HasIndex(location => location.LocationCode).IsUnique();
        });

        modelBuilder.Entity<PurchaseBill>(entity =>
        {
            entity.ToTable("Purchase_Bills");
            entity.HasKey(bill => bill.Id);
            entity.Property(bill => bill.CreatedAtUtc).HasColumnName("Created_At_Utc").IsRequired();
            entity.Property(bill => bill.CreatedByEmail).HasColumnName("Created_By_Email").HasMaxLength(256).IsRequired();
            entity.Property(bill => bill.TotalItems).HasColumnName("Total_Items").IsRequired();
            entity.Property(bill => bill.TotalQuantity).HasColumnName("Total_Quantity").HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(bill => bill.TotalCost).HasColumnName("Total_Cost").HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(bill => bill.TotalSelling).HasColumnName("Total_Selling").HasColumnType("decimal(18,2)").IsRequired();
            entity.HasMany(bill => bill.Items)
                .WithOne(item => item.PurchaseBill)
                .HasForeignKey(item => item.PurchaseBillId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PurchaseBillItem>(entity =>
        {
            entity.ToTable("Purchase_Bill_Items");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.PurchaseBillId).HasColumnName("Purchase_Bill_Id").IsRequired();
            entity.Property(item => item.Item).HasMaxLength(100).IsRequired();
            entity.Property(item => item.Batch).HasMaxLength(200).IsRequired();
            entity.Property(item => item.StandardCost).HasColumnName("Standard_Cost").HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(item => item.StandardPrice).HasColumnName("Standard_Price").HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(item => item.Quantity).HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(item => item.Discount).HasColumnType("decimal(5,2)").IsRequired();
            entity.Property(item => item.TotalCost).HasColumnName("Total_Cost").HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(item => item.TotalSelling).HasColumnName("Total_Selling").HasColumnType("decimal(18,2)").IsRequired();
        });
    }
}
