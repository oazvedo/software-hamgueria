using SoftwareHamgueria.API.Errors;
using SoftwareHamgueria.Application.Audit;

namespace SoftwareHamgueria.API.Audit;

/// <summary>
/// The audit author and screen of the current request (ADR 0027). The kitchen is the authenticated user; a customer is
/// the comanda name the frontend sends in <c>X-Author-Name</c> (URI-encoded: header values must be ASCII). Both headers
/// are informative only and never used for authorization.
/// </summary>
public sealed class HttpAuditContext(IHttpContextAccessor accessor) : IAuditContext
{
    public const string AuthorHeader = "X-Author-Name";
    public const string FrontendUrlHeader = "X-Frontend-Url";
    private const int MaxAuthor = 60;
    private const int MaxUrl = 500;

    private HttpContext? Http => accessor.HttpContext;

    public string? AuthorName
    {
        get
        {
            if (Http is null) return IAuditContext.SystemAuthor;
            if (Http.User.Identity?.IsAuthenticated == true && !string.IsNullOrWhiteSpace(Http.User.Identity.Name))
                return Http.User.Identity.Name;
            return Clean(Decode(Http.Request.Headers[AuthorHeader].ToString()), MaxAuthor);
        }
    }

    public string? FrontendUrl
    {
        get
        {
            if (Http is null) return null;
            var url = Http.Request.Headers[FrontendUrlHeader].ToString();
            if (string.IsNullOrWhiteSpace(url)) url = Http.Request.Headers.Referer.ToString();
            return Uri.TryCreate(url, UriKind.Absolute, out var uri) && uri.Scheme is "http" or "https"
                ? Clean(uri.ToString(), MaxUrl)
                : null;
        }
    }

    public string? RequestId => Http is null ? null : GlobalExceptionHandler.TraceId(Http);

    private static string Decode(string value)
    {
        try { return Uri.UnescapeDataString(value); }
        catch (UriFormatException) { return value; }
    }

    private static string? Clean(string value, int max)
    {
        var trimmed = new string(value.Where(c => !char.IsControl(c)).ToArray()).Trim();
        return trimmed.Length == 0 ? null : trimmed[..Math.Min(trimmed.Length, max)];
    }
}
