using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniErp.Api.Data;
using MiniErp.Api.Models;

namespace MiniErp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ItemsController : ControllerBase
{
    private readonly Db _db;
    public ItemsController(Db db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<List<ItemDto>>> GetAll()
    {
        await using var conn = _db.CreateConnection();
        await conn.OpenAsync();

        var list = new List<ItemDto>();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT id, sku, name, unit_price, qty_on_hand FROM items ORDER BY id DESC";

        await using var r = await cmd.ExecuteReaderAsync();
        while (await r.ReadAsync())
        {
            list.Add(new ItemDto(
                r.GetInt32("id"),
                r.GetString("sku"),
                r.GetString("name"),
                r.GetDecimal("unit_price"),
                r.GetInt32("qty_on_hand")
            ));
        }

        return Ok(list);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ItemDto>> GetById(int id)
    {
        await using var conn = _db.CreateConnection();
        await conn.OpenAsync();

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT id, sku, name, unit_price, qty_on_hand FROM items WHERE id=@id";
        cmd.Parameters.AddWithValue("@id", id);

        await using var r = await cmd.ExecuteReaderAsync();
        if (!await r.ReadAsync()) return NotFound();

        return Ok(new ItemDto(
            r.GetInt32("id"),
            r.GetString("sku"),
            r.GetString("name"),
            r.GetDecimal("unit_price"),
            r.GetInt32("qty_on_hand")
        ));
    }

    [HttpPost]
    public async Task<ActionResult> Create(CreateItemRequest req)
    {
        await using var conn = _db.CreateConnection();
        await conn.OpenAsync();

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = @"INSERT INTO items (sku, name, unit_price, qty_on_hand)
                            VALUES (@sku, @name, @price, @qty)";
        cmd.Parameters.AddWithValue("@sku", req.Sku.Trim());
        cmd.Parameters.AddWithValue("@name", req.Name.Trim());
        cmd.Parameters.AddWithValue("@price", req.UnitPrice);
        cmd.Parameters.AddWithValue("@qty", req.QtyOnHand);

        await cmd.ExecuteNonQueryAsync();
        return Created("", null);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> Update(int id, UpdateItemRequest req)
    {
        await using var conn = _db.CreateConnection();
        await conn.OpenAsync();

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = @"UPDATE items
                            SET sku=@sku, name=@name, unit_price=@price, qty_on_hand=@qty
                            WHERE id=@id";
        cmd.Parameters.AddWithValue("@id", id);
        cmd.Parameters.AddWithValue("@sku", req.Sku.Trim());
        cmd.Parameters.AddWithValue("@name", req.Name.Trim());
        cmd.Parameters.AddWithValue("@price", req.UnitPrice);
        cmd.Parameters.AddWithValue("@qty", req.QtyOnHand);

        var rows = await cmd.ExecuteNonQueryAsync();
        if (rows == 0) return NotFound();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        await using var conn = _db.CreateConnection();
        await conn.OpenAsync();

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM items WHERE id=@id";
        cmd.Parameters.AddWithValue("@id", id);

        var rows = await cmd.ExecuteNonQueryAsync();
        if (rows == 0) return NotFound();
        return NoContent();
    }
}
