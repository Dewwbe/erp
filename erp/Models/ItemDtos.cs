namespace MiniErp.Api.Models;

public record ItemDto(int Id, string Sku, string Name, decimal UnitPrice, int QtyOnHand);
public record CreateItemRequest(string Sku, string Name, decimal UnitPrice, int QtyOnHand);
public record UpdateItemRequest(string Sku, string Name, decimal UnitPrice, int QtyOnHand);
