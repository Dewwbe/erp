namespace MiniErp.Api.Models;

public record CustomerDto(int Id, string Code, string Name, string? Email, string? Phone, string? Address);
public record CreateCustomerRequest(string Code, string Name, string? Email, string? Phone, string? Address);
public record UpdateCustomerRequest(string Code, string Name, string? Email, string? Phone, string? Address);
