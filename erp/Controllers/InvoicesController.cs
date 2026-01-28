using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniErp.Api.Data;
using MiniErp.Api.Models;

namespace MiniErp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InvoicesController : ControllerBase
{
    private readonly Db _db;
    public InvoicesController(Db db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<List<object>>> GetAll()
    {
        await using var conn = _db.CreateConnection();
        await conn.OpenAsync();

        var list = new List<object>();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT i.id, i.invoice_no, i.customer_id, c.name AS customer_name, i.invoice_date, i.status, i.total
            FROM invoices i
            JOIN customers c ON c.id = i.customer_id
            ORDER BY i.id DESC";

        await using var r = await cmd.ExecuteReaderAsync();
        while (await r.ReadAsync())
        {
            list.Add(new
            {
                id = r.GetInt32("id"),
                invoiceNo = r.GetString("invoice_no"),
                customerId = r.GetInt32("customer_id"),
                customerName = r.GetString("customer_name"),
                invoiceDate = r.GetDateTime("invoice_date"),
                status = r.GetString("status"),
                total = r.GetDecimal("total"),
            });
        }

        return Ok(list);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<InvoiceDto>> GetById(int id)
    {
        await using var conn = _db.CreateConnection();
        await conn.OpenAsync();

        // Header
        string invoiceNo;
        int customerId;
        string customerName;
        DateTime invoiceDate;
        string status;
        decimal total;

        await using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = @"
                SELECT i.id, i.invoice_no, i.customer_id, c.name AS customer_name, i.invoice_date, i.status, i.total
                FROM invoices i
                JOIN customers c ON c.id = i.customer_id
                WHERE i.id = @id";
            cmd.Parameters.AddWithValue("@id", id);

            await using var r = await cmd.ExecuteReaderAsync();
            if (!await r.ReadAsync()) return NotFound();

            invoiceNo = r.GetString("invoice_no");
            customerId = r.GetInt32("customer_id");
            customerName = r.GetString("customer_name");
            invoiceDate = r.GetDateTime("invoice_date");
            status = r.GetString("status");
            total = r.GetDecimal("total");
        }

        // Lines
        var lines = new List<InvoiceLineDto>();
        await using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = @"
                SELECT l.id, l.item_id, it.name AS item_name, l.qty, l.unit_price, l.line_total
                FROM invoice_lines l
                JOIN items it ON it.id = l.item_id
                WHERE l.invoice_id = @id
                ORDER BY l.id";
            cmd.Parameters.AddWithValue("@id", id);

            await using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync())
            {
                lines.Add(new InvoiceLineDto(
                    r.GetInt32("id"),
                    r.GetInt32("item_id"),
                    r.GetString("item_name"),
                    r.GetInt32("qty"),
                    r.GetDecimal("unit_price"),
                    r.GetDecimal("line_total")
                ));
            }
        }

        return Ok(new InvoiceDto(id, invoiceNo, customerId, customerName, invoiceDate, status, total, lines));
    }

    [HttpPost]
    public async Task<ActionResult> Create(CreateInvoiceRequest req)
    {
        if (req.Lines == null || req.Lines.Count == 0)
            return BadRequest("Invoice must have at least 1 line.");

        await using var conn = _db.CreateConnection();
        await conn.OpenAsync();
        await using var tx = await conn.BeginTransactionAsync();

        try
        {
            // Calculate totals
            var computedTotal = req.Lines.Sum(l => l.Qty * l.UnitPrice);

            int invoiceId;
            await using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tx;
                cmd.CommandText = @"
                    INSERT INTO invoices (invoice_no, customer_id, invoice_date, status, total)
                    VALUES (@no, @cid, @dt, @st, @tot);
                    SELECT LAST_INSERT_ID();";
                cmd.Parameters.AddWithValue("@no", req.InvoiceNo.Trim());
                cmd.Parameters.AddWithValue("@cid", req.CustomerId);
                cmd.Parameters.AddWithValue("@dt", req.InvoiceDate);
                cmd.Parameters.AddWithValue("@st", req.Status.Trim());
                cmd.Parameters.AddWithValue("@tot", computedTotal);

                invoiceId = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            }

            foreach (var line in req.Lines)
            {
                var lineTotal = line.Qty * line.UnitPrice;
                await using var cmd = conn.CreateCommand();
                cmd.Transaction = tx;
                cmd.CommandText = @"
                    INSERT INTO invoice_lines (invoice_id, item_id, qty, unit_price, line_total)
                    VALUES (@iid, @item, @qty, @price, @lt)";
                cmd.Parameters.AddWithValue("@iid", invoiceId);
                cmd.Parameters.AddWithValue("@item", line.ItemId);
                cmd.Parameters.AddWithValue("@qty", line.Qty);
                cmd.Parameters.AddWithValue("@price", line.UnitPrice);
                cmd.Parameters.AddWithValue("@lt", lineTotal);

                await cmd.ExecuteNonQueryAsync();
            }

            await tx.CommitAsync();
            return Created($"/api/invoices/{invoiceId}", new { id = invoiceId });
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> Update(int id, UpdateInvoiceRequest req)
    {
        if (req.Lines == null || req.Lines.Count == 0)
            return BadRequest("Invoice must have at least 1 line.");

        await using var conn = _db.CreateConnection();
        await conn.OpenAsync();
        await using var tx = await conn.BeginTransactionAsync();

        try
        {
            // Ensure invoice exists
            await using (var existsCmd = conn.CreateCommand())
            {
                existsCmd.Transaction = tx;
                existsCmd.CommandText = "SELECT COUNT(1) FROM invoices WHERE id=@id";
                existsCmd.Parameters.AddWithValue("@id", id);
                var exists = Convert.ToInt32(await existsCmd.ExecuteScalarAsync());
                if (exists == 0) return NotFound();
            }

            var computedTotal = req.Lines.Sum(l => l.Qty * l.UnitPrice);

            // Update header
            await using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tx;
                cmd.CommandText = @"
                    UPDATE invoices
                    SET invoice_no=@no, customer_id=@cid, invoice_date=@dt, status=@st, total=@tot
                    WHERE id=@id";
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@no", req.InvoiceNo.Trim());
                cmd.Parameters.AddWithValue("@cid", req.CustomerId);
                cmd.Parameters.AddWithValue("@dt", req.InvoiceDate);
                cmd.Parameters.AddWithValue("@st", req.Status.Trim());
                cmd.Parameters.AddWithValue("@tot", computedTotal);

                await cmd.ExecuteNonQueryAsync();
            }

            // Replace lines (simple approach)
            await using (var del = conn.CreateCommand())
            {
                del.Transaction = tx;
                del.CommandText = "DELETE FROM invoice_lines WHERE invoice_id=@id";
                del.Parameters.AddWithValue("@id", id);
                await del.ExecuteNonQueryAsync();
            }

            foreach (var line in req.Lines)
            {
                var lineTotal = line.Qty * line.UnitPrice;
                await using var cmd = conn.CreateCommand();
                cmd.Transaction = tx;
                cmd.CommandText = @"
                    INSERT INTO invoice_lines (invoice_id, item_id, qty, unit_price, line_total)
                    VALUES (@iid, @item, @qty, @price, @lt)";
                cmd.Parameters.AddWithValue("@iid", id);
                cmd.Parameters.AddWithValue("@item", line.ItemId);
                cmd.Parameters.AddWithValue("@qty", line.Qty);
                cmd.Parameters.AddWithValue("@price", line.UnitPrice);
                cmd.Parameters.AddWithValue("@lt", lineTotal);
                await cmd.ExecuteNonQueryAsync();
            }

            await tx.CommitAsync();
            return NoContent();
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        await using var conn = _db.CreateConnection();
        await conn.OpenAsync();

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM invoices WHERE id=@id";
        cmd.Parameters.AddWithValue("@id", id);

        var rows = await cmd.ExecuteNonQueryAsync();
        if (rows == 0) return NotFound();

        // invoice_lines deleted automatically via ON DELETE CASCADE
        return NoContent();
    }
}
