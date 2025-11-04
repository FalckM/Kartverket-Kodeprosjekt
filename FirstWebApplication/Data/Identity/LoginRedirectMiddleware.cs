using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using FirstWebApplication.Data.Identity;

namespace FirstWebApplication.Data
{
    public class LoginRedirectMiddleware
    {
        private readonly RequestDelegate _next;

        public LoginRedirectMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!context.User.Identity.IsAuthenticated &&
                !context.Request.Path.StartsWithSegments("/Identity/Account/Login") &&
                !context.Request.Path.StartsWithSegments("/Identity/Account/Register") &&
                !context.Request.Path.StartsWithSegments("/css") &&
                !context.Request.Path.StartsWithSegments("/js"))
            {
                context.Response.Redirect("/Identity/Account/Login");
                return;
            }

            await _next(context);
        }
    }

    // Extension metode for senere setup i program
    public static class LoginRedirectMiddlewareExtensions
    {
        public static IApplicationBuilder UseLoginRedirect(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<LoginRedirectMiddleware>();
        }
    }
}
