using CoffeeLoyalty.Api.Entities;
using CoffeeLoyalty.Api.Enums;
using Microsoft.EntityFrameworkCore;

namespace CoffeeLoyalty.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<PointsTransaction> PointsTransactions => Set<PointsTransaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Enums stored as strings (readable in the DB, decoupled from ordinal values).
        modelBuilder.Entity<Employee>()
            .Property(e => e.Role)
            .HasConversion<string>()
            .HasMaxLength(20);

        modelBuilder.Entity<Order>()
            .Property(o => o.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        modelBuilder.Entity<PointsTransaction>()
            .Property(pt => pt.Type)
            .HasConversion<string>()
            .HasMaxLength(20);

        modelBuilder.Entity<Product>()
            .Property(p => p.UnitType)
            .HasConversion<string>()
            .HasMaxLength(20);

        modelBuilder.Entity<Product>()
            .Property(p => p.Category)
            .HasConversion<string>()
            .HasMaxLength(20);

        // Customer: phone is the identity (regular UK), Firebase UID is a filtered UK
        // (NULL until first app login; SQL Server treats NULL as a value otherwise).
        modelBuilder.Entity<Customer>(customer =>
        {
            customer.HasIndex(c => c.PhoneNumber).IsUnique();

            customer.HasIndex(c => c.FirebaseUid)
                .IsUnique()
                .HasFilter("[FirebaseUid] IS NOT NULL");

            customer.ToTable(t => t.HasCheckConstraint(
                "CK_Customer_Balance",
                "[PointsBalance] >= 0"));
        });

        // Employee: username is the login identity (regular UK).
        modelBuilder.Entity<Employee>()
            .HasIndex(e => e.Username).IsUnique();

        // OrderItem: returned quantity is bounded by the ordered quantity.
        modelBuilder.Entity<OrderItem>()
            .ToTable(t => t.HasCheckConstraint(
                "CK_OrderItem_Returned",
                "[ReturnedQuantity] >= 0 AND [ReturnedQuantity] <= [Quantity]"));

        // Order: points columns can never go negative.
        modelBuilder.Entity<Order>()
            .ToTable(t => t.HasCheckConstraint(
                "CK_Order_Points",
                "[PointsEarned] >= 0 AND [PointsRedeemed] >= 0"));

        // Restrict delete on every FK — entities are soft-deleted and kept for history,
        // so a physical delete must never cascade through orders / points movements.
        modelBuilder.Entity<Order>()
            .HasOne(o => o.Customer)
            .WithMany(c => c.Orders)
            .HasForeignKey(o => o.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Order>()
            .HasOne(o => o.Employee)
            .WithMany(e => e.Orders)
            .HasForeignKey(o => o.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Order)
            .WithMany(o => o.OrderItems)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Product)
            .WithMany(p => p.OrderItems)
            .HasForeignKey(oi => oi.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PointsTransaction>()
            .HasOne(pt => pt.Customer)
            .WithMany(c => c.PointsTransactions)
            .HasForeignKey(pt => pt.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        // OrderId is nullable in the column only so the one-time OpeningBalance import can exist;
        // the check constraint keeps "no sourceless points movement" (decision 12) true for every other type.
        modelBuilder.Entity<PointsTransaction>(pointsTransaction =>
        {
            pointsTransaction.HasOne(pt => pt.Order)
                .WithMany(o => o.PointsTransactions)
                .HasForeignKey(pt => pt.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            pointsTransaction.ToTable(t => t.HasCheckConstraint(
                "CK_PointsTransaction_Order",
                "[Type] = 'OpeningBalance' OR [OrderId] IS NOT NULL"));
        });

        // Earn / Redeem / RedeemReversal happen at most once per order.
        // Refund is excluded because partial returns are processed line by line;
        // NULL OrderId (OpeningBalance) is excluded because SQL Server treats NULLs as equal here,
        // which would let only one customer ever hold an opening balance.
        modelBuilder.Entity<PointsTransaction>()
            .HasIndex(pt => new { pt.OrderId, pt.Type })
            .IsUnique()
            .HasFilter("[Type] <> 'Refund' AND [OrderId] IS NOT NULL")
            .HasDatabaseName("UX_PointsTransaction_Order_Type");

        // TODO (Phase 6 — build with the opening-balance import, not before): add a filtered unique
        // index on CustomerId with filter "[Type] = 'OpeningBalance'", so the DB refuses a second
        // opening balance for a customer even if two concurrent import runs both pass the app-level
        // "already imported?" check. That check is a TOCTOU race; the index is the real guarantee.
        // See decisions.md #38.

        // Daily sales reports filter on the order date.
        modelBuilder.Entity<Order>()
            .HasIndex(o => o.CreatedAt);
    }
}
