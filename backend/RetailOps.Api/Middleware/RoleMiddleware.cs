using System.Security.Claims;

namespace RetailOps.Api.Middleware;

public class RoleMiddleware
{
    private readonly RequestDelegate _next;

    public RoleMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue("X-Role", out var role))
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "DemoUser"),
                new Claim(ClaimTypes.Role, role.ToString().ToLower())
            };

            var identity = new ClaimsIdentity(claims, "DemoAuth");
            context.User = new ClaimsPrincipal(identity);
        }

        await _next(context);
    }
}
