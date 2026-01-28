using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.IdentityModel.Tokens;
using MySqlConnector;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using MiniErp.Api.Models;

namespace MiniErp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _config;
    public AuthController(IConfiguration config) => _config = config;

    private MySqlConnection GetConn() => new(_config.GetConnectionString("MySql"));

    private static (string Hash, string Salt) HashPassword(string password)
    {
        byte[] saltBytes = RandomNumberGenerator.GetBytes(16);

        var hashBytes = KeyDerivation.Pbkdf2(
            password: password,
            salt: saltBytes,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 100_000,
            numBytesRequested: 32);

        return (Convert.ToBase64String(hashBytes), Convert.ToBase64String(saltBytes));
    }

    private static bool VerifyPassword(string password, string storedHashB64, string storedSaltB64)
    {
        byte[] saltBytes = Convert.FromBase64String(storedSaltB64);

        var hashBytes = KeyDerivation.Pbkdf2(
            password: password,
            salt: saltBytes,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 100_000,
            numBytesRequested: 32);

        var computedHashB64 = Convert.ToBase64String(hashBytes);

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(computedHashB64),
            Encoding.UTF8.GetBytes(storedHashB64));
    }

    private string CreateJwt(string email, string role, string fullName)
    {
        var jwt = _config.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, email),
            new(ClaimTypes.Role, string.IsNullOrWhiteSpace(role) ? "Clerk" : role),
            new("fullName", fullName ?? "")
        };

        var token = new JwtSecurityToken(
            issuer: jwt["Issuer"],
            audience: jwt["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(int.Parse(jwt["ExpiresMinutes"]!)),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest req)
    {
        var email = req.Email.Trim().ToLowerInvariant();

        await using var conn = GetConn();
        await conn.OpenAsync();

        // Check if email already exists
        await using (var check = conn.CreateCommand())
        {
            check.CommandText = "SELECT COUNT(1) FROM erp_users WHERE email = @email";
            check.Parameters.AddWithValue("@email", email);

            var exists = Convert.ToInt32(await check.ExecuteScalarAsync());
            if (exists > 0) return BadRequest("Email already registered.");
        }

        var (hash, salt) = HashPassword(req.Password);

        // Insert user
        await using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = @"INSERT INTO erp_users (email, password_hash, password_salt, full_name, role)
                                VALUES (@email, @hash, @salt, @fullName, 'Clerk')";
            cmd.Parameters.AddWithValue("@email", email);
            cmd.Parameters.AddWithValue("@hash", hash);
            cmd.Parameters.AddWithValue("@salt", salt);
            cmd.Parameters.AddWithValue("@fullName", req.FullName ?? "");
            await cmd.ExecuteNonQueryAsync();
        }

        var token = CreateJwt(email, "Clerk", req.FullName ?? "");
        return Ok(new AuthResponse(token, email, req.FullName ?? "", "Clerk"));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest req)
    {
        var email = req.Email.Trim().ToLowerInvariant();

        await using var conn = GetConn();
        await conn.OpenAsync();

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = @"SELECT email, full_name, role, password_hash, password_salt
                            FROM erp_users
                            WHERE email = @email";
        cmd.Parameters.AddWithValue("@email", email);

        await using var r = await cmd.ExecuteReaderAsync();
        if (!await r.ReadAsync()) return Unauthorized("Invalid email or password.");

        var dbHash = r.GetString("password_hash");
        var dbSalt = r.GetString("password_salt");

        if (!VerifyPassword(req.Password, dbHash, dbSalt))
            return Unauthorized("Invalid email or password.");

        var fullName = r.IsDBNull(r.GetOrdinal("full_name")) ? "" : r.GetString("full_name");
        var role = r.IsDBNull(r.GetOrdinal("role")) ? "Clerk" : r.GetString("role");

        var token = CreateJwt(email, role, fullName);
        return Ok(new AuthResponse(token, email, fullName, role));
    }
}
