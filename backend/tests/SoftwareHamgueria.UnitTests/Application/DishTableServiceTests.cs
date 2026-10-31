using FluentValidation;
using NSubstitute;
using SoftwareHamgueria.Application.DTOs.Dish;
using SoftwareHamgueria.Application.DTOs.Table;
using SoftwareHamgueria.Application.Exceptions;
using SoftwareHamgueria.Application.Repositories;
using SoftwareHamgueria.Application.Services;
using SoftwareHamgueria.Application.Validators.Dish;
using SoftwareHamgueria.Application.Validators.Table;
using SoftwareHamgueria.Domain.Entities;

namespace SoftwareHamgueria.UnitTests.Application;

public class DishTableServiceTests
{
    private readonly IDishRepository _dishes = Substitute.For<IDishRepository>();
    private readonly ITableRepository _tables = Substitute.For<ITableRepository>();
    private readonly DishService _dishService;
    private readonly TableService _tableService;

    public DishTableServiceTests()
    {
        _dishService = new DishService(_dishes, new CreateDishDtoValidator(), new UpdateDishDtoValidator());
        _tableService = new TableService(_tables, new CreateTableDtoValidator(), new UpdateTableDtoValidator());
    }

    [Fact(DisplayName = "003/CA-07: prato inválido é recusado e o cardápio não muda")]
    public async Task CreateDish_Invalid_IsNotSaved()
    {
        var ex = await Assert.ThrowsAsync<ValidationException>(() =>
            _dishService.CreateAsync(new CreateDishDto("", "x", -1, Category.Drinks)));

        Assert.Equal(2, ex.Errors.Count());
        await _dishes.DidNotReceive().AddAsync(Arg.Any<Dish>());
    }

    [Fact(DisplayName = "003/CA-07: alterar prato com preço zero é recusado sem gravar")]
    public async Task UpdateDish_Invalid_IsNotSaved()
    {
        await Assert.ThrowsAsync<ValidationException>(() =>
            _dishService.UpdateAsync(Guid.NewGuid(), new UpdateDishDto("Burger", "x", 0, Category.Drinks)));

        await _dishes.DidNotReceive().UpdateAsync(Arg.Any<Dish>());
    }

    [Fact(DisplayName = "003/CA-07: prato válido é gravado com o nome aparado")]
    public async Task CreateDish_Valid_IsSavedTrimmed()
    {
        var result = await _dishService.CreateAsync(new CreateDishDto("  Burger  ", "x", 10, Category.Hamburgers));

        Assert.Equal("Burger", result.Name);
        await _dishes.Received().AddAsync(Arg.Is<Dish>(d => d.Name == "Burger"));
    }

    [Fact(DisplayName = "003/CA-09: desativar prato inexistente devolve não encontrado em português")]
    public async Task DisableDish_Unknown_IsNotFound()
    {
        var ex = await Assert.ThrowsAsync<NotFoundException>(() => _dishService.DisableAsync(Guid.NewGuid()));

        Assert.Equal("Prato não encontrado.", ex.Message);
    }

    [Fact(DisplayName = "003/CA-08: mesa inválida é recusada e as mesas não mudam")]
    public async Task CreateTable_Invalid_IsNotSaved()
    {
        await Assert.ThrowsAsync<ValidationException>(() => _tableService.CreateAsync(new CreateTableDto(" ")));

        await _tables.DidNotReceive().AddAsync(Arg.Any<Table>());
    }

    [Fact(DisplayName = "003/CA-08: mesa válida é gravada com o nome aparado")]
    public async Task CreateTable_Valid_IsSavedTrimmed()
    {
        var result = await _tableService.CreateAsync(new CreateTableDto("  Mesa 9  "));

        Assert.Equal("Mesa 9", result.TableName);
    }

    [Fact(DisplayName = "003/CA-08: alterar mesa inexistente devolve não encontrado em português")]
    public async Task UpdateTable_Unknown_IsNotFound()
    {
        var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
            _tableService.UpdateAsync(Guid.NewGuid(), new UpdateTableDto("Mesa 9")));

        Assert.Equal("Mesa não encontrada.", ex.Message);
    }
}
