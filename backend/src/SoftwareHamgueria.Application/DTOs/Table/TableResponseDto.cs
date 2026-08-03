namespace SoftwareHamgueria.Application.DTOs.Table;

public record TableResponseDto(Guid Id, string TableName, bool IsActive, DateTime CreatedAt, DateTime UpdatedAt);
