using Microsoft.Extensions.Configuration;

namespace SoftwareHamgueria.Application.Configuration;

/// <summary>
/// Production configuration rules (ADR 0024). Checked at startup outside Development; any problem stops the API.
/// Messages name the key and the rule, never the value.
/// </summary>
public static class RequiredSettings
{
    public const int MinJwtSecretLength = 32;
    public const int MinAdminPasswordLength = 12;
    public const string DevAdminPassword = "admin123";
    public const string DefaultRabbitMqCredential = "guest";

    public static IReadOnlyList<string> Check(IConfiguration configuration)
    {
        var problems = new List<string>();

        Required("ConnectionStrings:Postgres");
        Required("Auth:AdminUsername");

        var jwtSecret = Required("Auth:JwtSecret");
        if (jwtSecret is not null && jwtSecret.Length < MinJwtSecretLength)
            problems.Add($"Auth:JwtSecret precisa de pelo menos {MinJwtSecretLength} caracteres.");

        var adminPassword = Required("Auth:AdminPassword");
        if (adminPassword == DevAdminPassword)
            problems.Add("Auth:AdminPassword não pode ser a senha de desenvolvimento.");
        else if (adminPassword is not null && adminPassword.Length < MinAdminPasswordLength)
            problems.Add($"Auth:AdminPassword precisa de pelo menos {MinAdminPasswordLength} caracteres.");

        NotDefaultRabbitMq("RabbitMq:Username");
        NotDefaultRabbitMq("RabbitMq:Password");

        return problems;

        string? Required(string key)
        {
            var value = configuration[key];
            if (!string.IsNullOrWhiteSpace(value)) return value;
            problems.Add($"{key} é obrigatório.");
            return null;
        }

        void NotDefaultRabbitMq(string key)
        {
            if (Required(key) == DefaultRabbitMqCredential)
                problems.Add($"{key} não pode ser o padrão do RabbitMQ.");
        }
    }
}
