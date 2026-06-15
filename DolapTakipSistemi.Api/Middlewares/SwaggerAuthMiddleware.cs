using Microsoft.AspNetCore.Http;
using System;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization.Infrastructure;

public class SwaggerAuthMiddleware
{
    private readonly RequestDelegate _next;

    public SwaggerAuthMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments("/swagger"))
        {
            string authHeader = context.Request.Headers["Authorization"];

            if (authHeader != null && authHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var headerParameter = AuthenticationHeaderValue.Parse(authHeader).Parameter;
                    var credentials = Encoding.UTF8.GetString(Convert.FromBase64String(headerParameter)).Split(':');

                    var username = credentials[0];
                    var password = credentials[1];

                    if (username == "admin" && password == "admin1234")
                    {
                        await _next(context);
                        return;
                    }
                }
                catch
                {
                    ILogger<SwaggerAuthMiddleware> logger = context.RequestServices.GetRequiredService<ILogger<SwaggerAuthMiddleware>>();
                }
            }

            context.Response.Headers["WWW-Authenticate"] = "Basic realm=\"Swagger Security\"";
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
        }
        else
        {
            await _next(context);
        }
    }
}
