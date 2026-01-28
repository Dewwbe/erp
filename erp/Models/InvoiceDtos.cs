namespace MiniErp.Api.Models;

public record InvoiceLineDto(int Id, int ItemId, string ItemName, int Qty, decimal UnitPrice, decimal LineTotal);

public record InvoiceDto(
    int Id,
    string InvoiceNo,
    int CustomerId,
    string CustomerName,
    DateTime InvoiceDate,
    string Status,
    decimal Total,
    List<InvoiceLineDto> Lines
);

public record CreateInvoiceLineRequest(int ItemId, int Qty, decimal UnitPrice);
public record CreateInvoiceRequest(string InvoiceNo, int CustomerId, DateTime InvoiceDate, string Status, List<CreateInvoiceLineRequest> Lines);

public record UpdateInvoiceLineRequest(int ItemId, int Qty, decimal UnitPrice);
public record UpdateInvoiceRequest(string InvoiceNo, int CustomerId, DateTime InvoiceDate, string Status, List<UpdateInvoiceLineRequest> Lines);
