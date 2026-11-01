using FluentValidation;
using SoftwareHamgueria.Application.DTOs.Order;

namespace SoftwareHamgueria.Application.Validators.Order;

/// <summary>Item rules shared by creating and changing an order (spec 003, RN-01 and RN-05).</summary>
public static class OrderItemsRules
{
    public static void Validate<T>(List<OrderItemDto>? items, ValidationContext<T> context)
    {
        if (items is null || items.Count == 0)
        {
            context.AddFailure(nameof(CreateOrderDto.Items), "O pedido precisa de pelo menos um item.");
            return;
        }

        var totals = new Dictionary<Guid, int>();
        for (var i = 0; i < items.Count; i++)
        {
            var item = items[i];
            var n = i + 1;
            var path = $"{nameof(CreateOrderDto.Items)}[{i}]";

            if (item.DishId == Guid.Empty)
                context.AddFailure($"{path}.{nameof(OrderItemDto.DishId)}", $"Escolha o prato do item {n}.");

            if (item.Quantity is < Limits.ItemQuantityMin or > Limits.ItemQuantityMax)
            {
                context.AddFailure($"{path}.{nameof(OrderItemDto.Quantity)}",
                    $"A quantidade do item {n} vai de {Limits.ItemQuantityMin} a {Limits.ItemQuantityMax}.");
            }
            else if (item.DishId != Guid.Empty)
            {
                // Repeated dishes are merged into one item (001/RN-04), so the limit applies to the sum.
                totals[item.DishId] = totals.GetValueOrDefault(item.DishId) + item.Quantity;
                if (totals[item.DishId] > Limits.ItemQuantityMax)
                    context.AddFailure($"{path}.{nameof(OrderItemDto.Quantity)}",
                        $"O prato do item {n} passa de {Limits.ItemQuantityMax} unidades somando as repetições.");
            }

            if (item.Notes is { Length: > Limits.ItemNotes })
                context.AddFailure($"{path}.{nameof(OrderItemDto.Notes)}",
                    $"A observação do item {n} pode ter no máximo {Limits.ItemNotes} caracteres.");
        }
    }
}
