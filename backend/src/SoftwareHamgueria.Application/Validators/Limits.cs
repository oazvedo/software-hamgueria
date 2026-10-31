namespace SoftwareHamgueria.Application.Validators;

/// <summary>Input limits shared by the validators and the EF configurations, so both always agree (ADR 0021).</summary>
public static class Limits
{
    public const int CustomerName = 60;
    public const int ItemNotes = 200;
    public const int ItemQuantityMin = 1;
    public const int ItemQuantityMax = 20;
    public const int DishName = 200;
    public const int DishDescription = 1000;
    public const int TableName = 100;
}
