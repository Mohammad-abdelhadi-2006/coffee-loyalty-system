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

        // One order per Idempotency-Key (decision 40). Filtered, because the header is optional
        // and SQL Server treats NULLs as equal in a unique index — without the filter only one
        // order in the whole system could ever be created without a key.
        modelBuilder.Entity<Order>()
            .HasIndex(o => o.IdempotencyKey)
            .IsUnique()
            .HasFilter("[IdempotencyKey] IS NOT NULL")
            .HasDatabaseName("UX_Order_IdempotencyKey");

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

        // At most one opening balance per customer, ever (decisions.md #38, whose TODO this
        // discharges — built now because the paper-card import has arrived and this is the run
        // it guards). An importer that checks "already imported?" and then inserts is a TOCTOU
        // race: two runs can both read "no" before either writes. This index is what actually
        // refuses the second row, so a re-run doubles nobody's balance.
        // Both indexes on CustomerId are spelled out, and both are needed.
        //
        // The plain one is what the foreign key would have got by convention — but the
        // convention only supplies it while nothing else covers the column, and the filtered
        // index below counts as covering it. Declaring the filtered one alone therefore
        // *removes* the plain index, and the customer's own ledger
        // (GET /api/customers/me/transactions) reads by CustomerId with no type filter, so it
        // would fall back to a scan. Naming both keeps them as two separate indexes.
        modelBuilder.Entity<PointsTransaction>()
            .HasIndex(pt => pt.CustomerId, "IX_PointsTransactions_CustomerId");

        modelBuilder.Entity<PointsTransaction>()
            .HasIndex(pt => pt.CustomerId, "UX_PointsTransaction_Customer_OpeningBalance")
            .IsUnique()
            .HasFilter("[Type] = 'OpeningBalance'");

        // Daily sales reports filter on the order date.
        modelBuilder.Entity<Order>()
            .HasIndex(o => o.CreatedAt);
    }
}
