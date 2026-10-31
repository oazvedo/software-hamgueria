namespace SoftwareHamgueria.Application.DTOs.Order;

/// <summary>Query of a customer's comanda: <c>GET /api/order/comanda?tableId&amp;customer</c>.</summary>
public record ComandaQuery(Guid TableId, string Customer);
