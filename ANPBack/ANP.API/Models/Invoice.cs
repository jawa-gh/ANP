
using Microsoft.EntityFrameworkCore;

namespace ANP.API.Models;

public enum InvoiceStatus
{
    Draft,
    Sent,
    Paid,
}

public class Invoice
{
    public int Id { get; set; }

    public required string Number { get; set; }

    public required string CustomerName { get; set; }

    public required DateOnly IssueDate { get; set; }

    public required InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;

    public required decimal TaxRate { get; set; }

    public required string? Notes { get; set; }

    public List<LineItem> LineItems { get; set; } = [];
}
