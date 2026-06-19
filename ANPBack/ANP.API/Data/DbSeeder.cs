using ANP.API.Models;
using Microsoft.EntityFrameworkCore;

namespace ANP.API.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (!await db.Products.AnyAsync())
        {
            db.Products.AddRange(
                new Product
                {
                    Code = "ACME-001",
                    Name = "Widget",
                    DefaultPrice = 19.99m,
                },
                new Product
                {
                    Code = "ACME-002",
                    Name = "Gadget",
                    DefaultPrice = 49.50m,
                },
                new Product
                {
                    Code = "ACME-003",
                    Name = "Sprocket",
                    DefaultPrice = 8.75m,
                },
                new Product
                {
                    Code = "SVC-HOUR",
                    Name = "Consulting (hour)",
                    DefaultPrice = 120.00m,
                }
            );
        }

        if (!await db.Invoices.AnyAsync())
        {
            db.Invoices.Add(
                new Invoice
                {
                    Number = "INV-2026-0001",
                    CustomerName = "Globex Corporation",
                    IssueDate = new DateOnly(2026, 6, 1),
                    Status = InvoiceStatus.Sent,
                    TaxRate = 0.20m,
                    Notes = "Net 30.",
                    LineItems =
                    [
                        new LineItem
                        {
                            ProductCode = "ACME-001",
                            Description = "Widget",
                            Quantity = 10,
                            UnitPrice = 19.99m,
                        },
                        new LineItem
                        {
                            ProductCode = "SVC-HOUR",
                            Description = "Consulting (hour)",
                            Quantity = 4,
                            UnitPrice = 120.00m,
                        },
                    ],
                }
            );
        }

        await db.SaveChangesAsync();
    }
}
