namespace FacturaElectronica.WebApi.Middleware;

public class TenantMiddleware(RequestDelegate next)
{
    private const string TenantHeader = "X-Tenant-Id";

    public async Task InvokeAsync(HttpContext context, TenantContext tenantContext)
    {
        // Skip tenant validation for health checks and swagger
        var path = context.Request.Path.Value?.ToLower() ?? string.Empty;
        if (path.StartsWith("/api/health") || path.StartsWith("/swagger") || path.StartsWith("/health"))
        {
            await next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue(TenantHeader, out var tenantIdValue) ||
            !Guid.TryParse(tenantIdValue, out var tenantId))
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new { error = $"Header '{TenantHeader}' is required and must be a valid GUID." });
            return;
        }

        tenantContext.TenantId = tenantId;
        await next(context);
    }
}
