using Microsoft.Extensions.Configuration;
using SoftwareHamgueria.Application.Configuration;

namespace SoftwareHamgueria.UnitTests.Application;

public class RequiredSettingsTests
{
    private static readonly Dictionary<string, string?> Valid = new()
    {
        ["ConnectionStrings:Postgres"] = "Host=postgres;Database=hamgueria;Username=hamgueria;Password=Sup3rS3creta!",
        ["Auth:AdminUsername"] = "cozinha",
        ["Auth:AdminPassword"] = "senha-forte-12",
        ["Auth:JwtSecret"] = new string('k', 32),
        ["RabbitMq:Username"] = "hamgueria",
        ["RabbitMq:Password"] = "fila-secreta",
    };

    private static IReadOnlyList<string> Check(Action<Dictionary<string, string?>>? change = null)
    {
        var values = new Dictionary<string, string?>(Valid);
        change?.Invoke(values);
        return RequiredSettings.Check(new ConfigurationBuilder().AddInMemoryCollection(values).Build());
    }

    [Fact(DisplayName = "004/CA-05: configuração completa e forte passa (senha 12, chave 32)")]
    public void ValidConfiguration_HasNoProblems() => Assert.Empty(Check());

    [Theory(DisplayName = "004/CA-04: segredo ausente é citado pelo nome")]
    [InlineData("ConnectionStrings:Postgres")]
    [InlineData("Auth:AdminUsername")]
    [InlineData("Auth:AdminPassword")]
    [InlineData("Auth:JwtSecret")]
    [InlineData("RabbitMq:Username")]
    [InlineData("RabbitMq:Password")]
    public void MissingKey_IsNamed(string key)
    {
        var problems = Check(v => v.Remove(key));
        Assert.Contains(problems, p => p.Contains(key));
    }

    [Fact(DisplayName = "004/CA-04: segredo só com espaços conta como ausente")]
    public void BlankKey_IsMissing() =>
        Assert.Contains(Check(v => v["Auth:AdminPassword"] = "   "), p => p.Contains("Auth:AdminPassword"));

    [Fact(DisplayName = "004/CA-05: senha da cozinha de desenvolvimento é recusada")]
    public void DevPassword_IsRejected() =>
        Assert.Contains(Check(v => v["Auth:AdminPassword"] = "admin123"), p => p.Contains("Auth:AdminPassword"));

    [Fact(DisplayName = "004/CA-05: senha da cozinha com 11 caracteres é recusada")]
    public void ShortPassword_IsRejected() =>
        Assert.Contains(Check(v => v["Auth:AdminPassword"] = new string('p', 11)), p => p.Contains("Auth:AdminPassword"));

    [Fact(DisplayName = "004/CA-05: chave do login com 31 caracteres é recusada")]
    public void ShortJwtSecret_IsRejected() =>
        Assert.Contains(Check(v => v["Auth:JwtSecret"] = new string('k', 31)), p => p.Contains("Auth:JwtSecret"));

    [Theory(DisplayName = "004/CA-05: credencial padrão do RabbitMQ é recusada")]
    [InlineData("RabbitMq:Username")]
    [InlineData("RabbitMq:Password")]
    public void GuestRabbitMq_IsRejected(string key) =>
        Assert.Contains(Check(v => v[key] = "guest"), p => p.Contains(key));

    [Fact(DisplayName = "004/CA-04: mensagens não exibem os valores")]
    public void Problems_DoNotLeakValues()
    {
        const string leaked = "segredo-vazado";
        var problems = Check(v =>
        {
            v["Auth:AdminPassword"] = leaked[..5];
            v["Auth:JwtSecret"] = leaked;
            v.Remove("ConnectionStrings:Postgres");
        });
        Assert.Equal(3, problems.Count);
        Assert.DoesNotContain(problems, p => p.Contains("segre"));
    }
}
