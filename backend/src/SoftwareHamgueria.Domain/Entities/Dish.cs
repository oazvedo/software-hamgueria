namespace SoftwareHamgueria.Domain.Entities;

public class Dish
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public double Price { get; private set; }
    public Category Category { get; private set; }
    public bool IsActive { get; private set; }

    protected Dish() { }

    public Dish(string name, string description, double price, Category category)
    {
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
        Price = price;
        Category = category;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Update(string name, string description, double price, Category category)
    {
        Name = name;
        Description = description;
        Price = price;
        Category = category;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Disable() => IsActive = false;
    public void Enable() => IsActive = true;
}
