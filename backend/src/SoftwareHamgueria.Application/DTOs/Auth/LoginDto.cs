namespace SoftwareHamgueria.Application.DTOs.Auth;

public record LoginDto(string Username, string Password);
public record TokenResponseDto(string Token, DateTime ExpiresAt);
