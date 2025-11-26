using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Gamesbakery.WebGUI.Middleware
{
    public class JwtCookieMiddleware
    {
        private readonly RequestDelegate next;
        private readonly IConfiguration configuration;

        public JwtCookieMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            this.next = next;
            this.configuration = configuration;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var publicPaths = new[]
            {
                "/api/v2/auth",
                "/swagger",
                "/swagger.json",
                "/health",
                "/Account/Login",
                "/User/Register",
            };
            if (publicPaths.Any(path => context.Request.Path.StartsWithSegments(path)))
            {
                await this.next(context);
                return;
            }

            var hasAuthHeader = context.Request.Headers.ContainsKey("Authorization");
            if (!hasAuthHeader)
            {
                var token = context.Request.Cookies["JwtToken"];
                if (!string.IsNullOrEmpty(token))
                {
                    context.Request.Headers.Append("Authorization", $"Bearer {token}");
                }
            }

            await this.next(context);
        }
    }
}
