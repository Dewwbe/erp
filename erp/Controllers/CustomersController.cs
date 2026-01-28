using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniErp.Api.Data;
using MiniErp.Api.Models;

namespace MiniErp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CustomersController : ControllerBase
{
    private readonly Db _db;
    public CustomersController(Db db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<List<CustomerDto>>> GetAll()
    {
        await using var conn = _db.CreateConnection();
        await conn.OpenAsync();

        var list = new List<CustomerDto>();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT id, code, name, email, phone, address FROM customers ORDER BY id DESC";

        await using var r = await cmd.ExecuteReaderAsync();
        while (await r.ReadAsync())
        {
            list.Add(new CustomerDto(
                r.GetInt32("id"),
                r.GetString("code"),
                r.GetString("name"),
                r.IsDBNull(r.GetOrdinal("email")) ? null : r.GetString("email"),
                r.IsDBNull(r.GetOrdinal("phone")) ? null : r.GetString("phone"),
                r.IsDBNull(r.GetOrdinal("address")) ? null : r.GetString("address")
            ));
        }

        return Ok(list);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CustomerDto>> GetById(int id)
    {
        await using var conn = _db.CreateConnection();
        await conn.OpenAsync();

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT id, code, name, email, phone, address FROM customers WHERE id=@id";
        cmd.Parameters.AddWithValue("@id", id);

        await using var r = await cmd.ExecuteReaderAsync();
        if (!await r.ReadAsync()) return NotFound();

        return Ok(new CustomerDto(
            r.GetInt32("id"),
            r.GetString("code"),
            r.GetString("name"),
            r.IsDBNull(r.GetOrdinal("email")) ? null : r.GetString("email"),
            r.IsDBNull(r.GetOrdinal("phone")) ? null : r.GetString("phone"),
            r.IsDBNull(r.GetOrdinal("address")) ? null : r.GetString("address")
        ));
    }

    [HttpPost]
    public async Task<ActionResult> Create(CreateCustomerRequest req)
    {
        await using var conn = _db.CreateConnection();
        await conn.OpenAsync();

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = @"INSERT INTO customers (code, name, email, phone, address)
                            VALUES (@code, @name, @email, @phone, @address)";
        cmd.Parameters.AddWithValue("@code", req.Code.Trim());
        cmd.Parameters.AddWithValue("@name", req.Name.Trim());
        cmd.Parameters.AddWithValue("@email", (object?)req.Email ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@phone", (object?)req.Phone ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@address", (object?)req.Address ?? DBNull.Value);

        await cmd.ExecuteNonQueryAsync();
        return Created("", null);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> Update(int id, UpdateCustomerRequest req)
    {
        await using var conn = _db.CreateConnection();
        await conn.OpenAsync();

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = @"UPDATE customers
                            SET code=@code, name=@name, email=@email, phone=@phone, address=@address
                            WHERE id=@id";
        cmd.Parameters.AddWithValue("@id", id);
        cmd.Parameters.AddWithValue("@code", req.Code.Trim());
        cmd.Parameters.AddWithValue("@name", req.Name.Trim());
        cmd.Parameters.AddWithValue("@email", (object?)req.Email ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@phone", (object?)req.Phone ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@address", (object?)req.Address ?? DBNull.Value);

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
        cmd.CommandText = "DELETE FROM customers WHERE id=@id";
        cmd.Parameters.AddWithValue("@id", id);

        var rows = await cmd.ExecuteNonQueryAsync();
        if (rows == 0) return NotFound();
        return NoContent();
    }
}
