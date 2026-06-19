using ANP.API.Models;

namespace ANP.API.Dtos;

public record LineItemDto(string ProductCode, string Description, int Quantity, decimal UnitPrice);

/// <summary>Payload the Angular Signal Forms editor sends on create/update.</summary>
public record InvoiceWriteDto(
    string CustomerName,
    DateOnly IssueDate,
    InvoiceStatus Status,
    decimal TaxRate,
    string? Notes,
    List<LineItemDto> LineItems
);

/// <summary>Shape returned to the client, including server-computed totals.</summary>
public record InvoiceReadDto(
    int Id,
    string Number,
    string CustomerName,
    DateOnly IssueDate,
    InvoiceStatus Status,
    decimal TaxRate,
    string? Notes,
    List<LineItemDto> LineItems,
    decimal Subtotal,
    decimal TaxAmount,
    decimal Total
);

/// <summary>Summary row for the list page.</summary>
public record InvoiceSummaryDto(
    int Id,
    string Number,
    string CustomerName,
    DateOnly IssueDate,
    InvoiceStatus Status,
    decimal Total
);

public static class InvoiceMapping
{
    public static decimal Subtotal(this Invoice invoice) =>
        invoice.LineItems.Sum(l => l.Quantity * l.UnitPrice);

    public static InvoiceReadDto ToReadDto(this Invoice invoice)
    {
        var subtotal = invoice.Subtotal();
        var tax = decimal.Round(subtotal * invoice.TaxRate, 2);
        return new InvoiceReadDto(
            invoice.Id,
            invoice.Number,
            invoice.CustomerName,
            invoice.IssueDate,
            invoice.Status,
            invoice.TaxRate,
            invoice.Notes,
            invoice.LineItems.Select(l => l.ToDto()).ToList(),
            subtotal,
            tax,
            subtotal + tax
        );
    }

    public static InvoiceSummaryDto ToSummaryDto(this Invoice invoice)
    {
        var subtotal = invoice.Subtotal();
        var total = subtotal + decimal.Round(subtotal * invoice.TaxRate, 2);
        return new InvoiceSummaryDto(
            invoice.Id,
            invoice.Number,
            invoice.CustomerName,
            invoice.IssueDate,
            invoice.Status,
            total
        );
    }

    public static LineItemDto ToDto(this LineItem item) =>
        new(item.ProductCode, item.Description, item.Quantity, item.UnitPrice);

    public static LineItem ToEntity(this LineItemDto dto) =>
        new()
        {
            ProductCode = dto.ProductCode,
            Description = dto.Description,
            Quantity = dto.Quantity,
            UnitPrice = dto.UnitPrice,
        };
}
