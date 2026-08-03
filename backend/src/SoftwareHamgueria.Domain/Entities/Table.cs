namespace SoftwareHamgueria.Domain.Entities;

public class Table
{
    public Guid Id { get; private set; }
    public string TableName { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public bool IsActive { get; private set; }

    protected Table() { }

    public Table(string tableName)
    {
        Id = Guid.NewGuid();
        TableName = tableName;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Update(string tableName)
    {
        TableName = tableName;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Disable() => IsActive = false;
    public void Enable() => IsActive = true;
}
