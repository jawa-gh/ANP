using ANP.API.Data;
using ANP.API.Dtos;
using ANP.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ANP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InvoicesController(AppDbContext db) : ControllerBase
{
    /// <summary>
    /// Gets a list of all invoices, optionally filtered by status.
    /// </summary>
    /// <param name="status">[Optional] The status to filter by.</param>
    /// <example>GET /api/invoices?status=Pending</example>
    /// <returns>A list of invoice summaries.</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<InvoiceSummaryDto>>> GetAll([FromQuery] InvoiceStatus? status)
    {
        var query = db.Invoices.Include(i => i.LineItems).AsNoTracking().AsQueryable();
        if (status is not null)
        {
            query = query.Where(i => i.Status == status);
        }

        var invoices = await query.OrderByDescending(i => i.IssueDate).ToListAsync();
        return invoices.Select(i => i.ToSummaryDto()).ToList();
    }

    /// <summary>
    /// Gets a single invoice by ID.
    /// </summary>
    /// <example>GET /api/invoices/5</example>
    /// <param name="id">The ID of the invoice to retrieve.</param>
    /// <returns>The invoice data or a not-found response.</returns>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<InvoiceReadDto>> GetById(int id)
    {
        var invoice = await db
            .Invoices.Include(i => i.LineItems)
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == id);

        return invoice is null ? NotFound() : invoice.ToReadDto();
    }

    /// <summary>
    /// Creates a new invoice.
    /// </summary>
    /// <example>POST /api/invoices</example>
    /// <param name="dto">The data for the new invoice.</param>
    /// <returns>The created invoice data.</returns>
    [HttpPost]
    public async Task<ActionResult<InvoiceReadDto>> Create(InvoiceWriteDto dto)
    {
        var invoice = new Invoice
        {
            Number = await NextInvoiceNumber(),
            CustomerName = dto.CustomerName,
            IssueDate = dto.IssueDate,
            Status = dto.Status,
            TaxRate = dto.TaxRate,
            Notes = dto.Notes,
            LineItems = dto.LineItems.Select(l => l.ToEntity()).ToList(),
        };

        db.Invoices.Add(invoice);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = invoice.Id }, invoice.ToReadDto());
    }

    /// <summary>
    /// Updates an existing invoice.
    /// </summary>
    /// <example>PUT /api/invoices/5</example>
    /// <param name="id">The ID of the invoice to update.</param>
    /// <param name="dto">The updated invoice data.</param>
    /// <returns>The updated invoice data or a not-found response.</returns>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<InvoiceReadDto>> Update(int id, InvoiceWriteDto dto)
    {
        var invoice = await db
            .Invoices.Include(i => i.LineItems)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (invoice is null)
        {
            return NotFound();
        }

        invoice.CustomerName = dto.CustomerName;
        invoice.IssueDate = dto.IssueDate;
        invoice.Status = dto.Status;
        invoice.TaxRate = dto.TaxRate;
        invoice.Notes = dto.Notes;

        invoice.LineItems.Clear();
        invoice.LineItems.AddRange(dto.LineItems.Select(l => l.ToEntity()));

        await db.SaveChangesAsync();
        return invoice.ToReadDto();
    }

    /// <summary>
    /// Deletes an existing invoice.
    /// </summary>
    /// <example>DELETE /api/invoices/5</example>
    /// <param name="id">The ID of the invoice to delete.</param>
    /// <returns>A no-content response or a not-found response.</returns>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var invoice = await db.Invoices.FindAsync(id);
        if (invoice is null)
        {
            return NotFound();
        }

        db.Invoices.Remove(invoice);
        await db.SaveChangesAsync();
        return NoContent();
    }

    #region Private Helpers

    private async Task<string> NextInvoiceNumber()
    {
        var year = DateTime.UtcNow.Year;
        var count = await db.Invoices.CountAsync();
        return $"INV-{year}-{count + 1:D4}";
    }

    #endregion Private Helpers
}
