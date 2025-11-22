using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Gamesbakery.WebGUI.Middleware
{
    public class JwtCookieMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;

        public JwtCookieMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var publicPaths = new[] {
                "/api/v2/auth",
                "/swagger",
                "/swagger.json",
                "/health",
                "/Account/Login",
                "/User/Register"
            };
            if (publicPaths.Any(path => context.Request.Path.StartsWithSegments(path)))
            {
                await _next(context);
                return;
            }

            var hasAuthHeader = context.Request.Headers.ContainsKey("Authorization");
            if (!hasAuthHeader)
            {
                var token = context.Request.Cookies["JwtToken"];
                if (!string.IsNullOrEmpty(token))
                {
                    context.Request.Headers.Add("Authorization", $"Bearer {token}");
                }
            }

            await _next(context);
        }
    }
}