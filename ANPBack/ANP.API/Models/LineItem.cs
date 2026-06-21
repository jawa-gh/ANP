
namespace ANP.API.Models;

public class LineItem
{
    public int Id { get; set; }

    public required string ProductCode { get; set; }
    
    public required string Description { get; set; }
    
    public required int Quantity { get; set; }
    
    public required decimal UnitPrice { get; set; }

    public int InvoiceId { get; set; }
    public Invoice? Invoice { get; set; }
}
