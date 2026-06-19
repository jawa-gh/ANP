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
    // GET /api/invoices?status=Draft
    [HttpGet]
    public async Task<ActionResult<IEnumerable<InvoiceSummaryDto>>> GetAll(
        [FromQuery] InvoiceStatus? status
    )
    {
        var query = db.Invoices.Include(i => i.LineItems).AsQueryable();
        if (status is not null)
        {
            query = query.Where(i => i.Status == status);
        }

        var invoices = await query.OrderByDescending(i => i.IssueDate).ToListAsync();
        return invoices.Select(i => i.ToSummaryDto()).ToList();
    }

    // GET /api/invoices/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<InvoiceReadDto>> GetById(int id)
    {
        var invoice = await db
            .Invoices.Include(i => i.LineItems)
            .FirstOrDefaultAsync(i => i.Id == id);

        return invoice is null ? NotFound() : invoice.ToReadDto();
    }

    // POST /api/invoices
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

    // PUT /api/invoices/5
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

        // Replace the line-item collection wholesale — simplest correct strategy for a demo.
        invoice.LineItems.Clear();
        invoice.LineItems.AddRange(dto.LineItems.Select(l => l.ToEntity()));

        await db.SaveChangesAsync();
        return invoice.ToReadDto();
    }

    // DELETE /api/invoices/5
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

    private async Task<string> NextInvoiceNumber()
    {
        var year = DateTime.UtcNow.Year;
        var count = await db.Invoices.CountAsync();
        return $"INV-{year}-{count + 1:D4}";
    }
}
