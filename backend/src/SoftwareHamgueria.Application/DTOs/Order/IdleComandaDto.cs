namespace SoftwareHamgueria.Application.DTOs.Order;

/// <summary>An open comanda with no new order for a while (spec 025): the kitchen decides whether to close it.</summary>
public record IdleComandaDto(Guid TableId, string TableName, string CustomerName, DateTime LastOrderAt, int Orders);
