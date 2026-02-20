namespace IncidentFlow.API.Services;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpiryMinutes { get; set; } = 60;
    public string CookieName { get; set; } = "incidentflow_access";
    public string CsrfCookieName { get; set; } = "incidentflow_csrf";
    public string CsrfHeaderName { get; set; } = "X-CSRF-TOKEN";
}
