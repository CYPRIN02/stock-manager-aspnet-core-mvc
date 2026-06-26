using System.Diagnostics;

namespace StockManager.Web.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestPath = context.Request.Path.Value ?? "/";
        var userName = context.User.Identity?.IsAuthenticated == true
            ? context.User.Identity.Name ?? "Utilisateur authentifié"
            : "Anonyme";

        try
        {
            await _next(context);
            stopwatch.Stop();

            if (!IsStaticAsset(requestPath))
            {
                _logger.LogInformation(
                    "HTTP {Method} {Path} -> {StatusCode} en {ElapsedMilliseconds} ms | Utilisateur={UserName} | TraceId={TraceId}",
                    context.Request.Method,
                    requestPath,
                    context.Response.StatusCode,
                    stopwatch.ElapsedMilliseconds,
                    userName,
                    context.TraceIdentifier);
            }
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogError(
                ex,
                "Erreur non gérée pendant la requête HTTP {Method} {Path} après {ElapsedMilliseconds} ms | Utilisateur={UserName} | TraceId={TraceId}",
                context.Request.Method,
                requestPath,
                stopwatch.ElapsedMilliseconds,
                userName,
                context.TraceIdentifier);

            throw;
        }
    }

    private static bool IsStaticAsset(string path)
    {
        return path.StartsWith("/css", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("/js", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("/lib", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("/images", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("/favicon", StringComparison.OrdinalIgnoreCase)
            || path.EndsWith(".css", StringComparison.OrdinalIgnoreCase)
            || path.EndsWith(".js", StringComparison.OrdinalIgnoreCase)
            || path.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
            || path.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)
            || path.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase)
            || path.EndsWith(".gif", StringComparison.OrdinalIgnoreCase)
            || path.EndsWith(".svg", StringComparison.OrdinalIgnoreCase)
            || path.EndsWith(".ico", StringComparison.OrdinalIgnoreCase);
    }
}
