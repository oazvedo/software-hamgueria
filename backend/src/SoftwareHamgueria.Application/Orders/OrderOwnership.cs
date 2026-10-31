using System.Security.Cryptography;
using System.Text;

namespace SoftwareHamgueria.Application.Orders;

/// <summary>
/// Who is acting on an order (ADR 0030): the kitchen (logged in as Admin) or someone holding the owner token that the
/// API returned once, when the order was created.
/// </summary>
public sealed record OrderActor(bool IsKitchen, string? OwnerToken)
{
    public static readonly OrderActor Kitchen = new(true, null);
    public static OrderActor Customer(string? token) => new(false, string.IsNullOrWhiteSpace(token) ? null : token.Trim());
}

/// <summary>Owner tokens: 256 random bits, base64url; only their SHA-256 is stored.</summary>
public static class OrderOwnerToken
{
    public static string New() => Base64Url(RandomNumberGenerator.GetBytes(32));

    public static string Hash(string token) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

    public static bool Matches(string? storedHash, string? token) =>
        storedHash is not null && token is not null &&
        CryptographicOperations.FixedTimeEquals(Encoding.ASCII.GetBytes(storedHash), Encoding.ASCII.GetBytes(Hash(token)));

    private static string Base64Url(byte[] bytes) => Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
}
