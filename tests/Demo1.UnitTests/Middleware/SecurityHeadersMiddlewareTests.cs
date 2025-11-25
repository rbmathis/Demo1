using Demo1.Middleware;
using Microsoft.AspNetCore.Http;

namespace Demo1.UnitTests.Middleware;

public class SecurityHeadersMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_AppendsExpectedHeaders_And_InvokesNext()
    {
        var invoked = false;
        RequestDelegate next = context =>
        {
            invoked = true;
            return Task.CompletedTask;
        };
        var middleware = new SecurityHeadersMiddleware(next);
        var context = new DefaultHttpContext();

        await middleware.InvokeAsync(context);

        Assert.True(invoked);
        var headers = context.Response.Headers;
        Assert.Equal("nosniff", headers["X-Content-Type-Options"].ToString());
        Assert.Equal("DENY", headers["X-Frame-Options"].ToString());
        Assert.Equal("1; mode=block", headers["X-XSS-Protection"].ToString());
        Assert.Equal("strict-origin-when-cross-origin", headers["Referrer-Policy"].ToString());
        Assert.Contains("default-src 'self'", headers["Content-Security-Policy"].ToString());
    }
}
