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
    public string Number { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public DateOnly IssueDate { get; set; }
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;

    /// <summary>Tax rate applied to the subtotal, e.g. 0.20 for 20%.</summary>
    public decimal TaxRate { get; set; }

    public string? Notes { get; set; }

    public List<LineItem> LineItems { get; set; } = [];
}
