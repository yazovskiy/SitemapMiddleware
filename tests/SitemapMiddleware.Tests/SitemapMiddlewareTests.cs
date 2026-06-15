using System.Text;
using Microsoft.AspNetCore.Http;
using Yasl.Net.SiteMapMiddleware;
using Xunit;

namespace SitemapMiddleware.Tests;

public class SitemapMiddlewareTests
{
    [Fact]
    public async Task SitemapXml_ReturnsAbsoluteHttpUrls()
    {
        var context = CreateContext("/sitemap.xml");
        var middleware = new Yasl.Net.SiteMapMiddleware.SitemapMiddleware(
            _ => throw new InvalidOperationException("Next middleware should not be called."),
            "https://example.com");

        await middleware.InvokeAsync(context);

        var body = await ReadBodyAsync(context);
        Assert.Equal("application/xml", context.Response.ContentType);
        Assert.Equal("public,max-age=3600", context.Response.Headers["Cache-Control"]);
        Assert.Contains("<loc>https://example.com</loc>", body);
        Assert.DoesNotContain("file://", body);
    }

    [Fact]
    public async Task RobotsTxt_IsNotIntercepted_WhenDisabled()
    {
        var context = CreateContext("/robots.txt");
        var nextCalled = false;
        var middleware = new Yasl.Net.SiteMapMiddleware.SitemapMiddleware(
            _ =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            },
            new SitemapMiddlewareOptions
            {
                RootUrl = "https://example.com"
            });

        await middleware.InvokeAsync(context);

        Assert.True(nextCalled);
        Assert.Equal(string.Empty, await ReadBodyAsync(context));
    }

    [Fact]
    public async Task RobotsTxt_ReturnsConfiguredDirectives_WhenEnabled()
    {
        var context = CreateContext("/robots.txt");
        var options = new SitemapMiddlewareOptions
        {
            RootUrl = "https://example.com",
            ServeRobotsTxt = true
        };
        options.RobotsTxtAdditionalLines.Add("Clean-param: etext");

        var middleware = new Yasl.Net.SiteMapMiddleware.SitemapMiddleware(
            _ => throw new InvalidOperationException("Next middleware should not be called."),
            options);

        await middleware.InvokeAsync(context);

        var body = await ReadBodyAsync(context);
        Assert.Equal("text/plain; charset=utf-8", context.Response.ContentType);
        Assert.Equal(
            """
            User-agent: *
            Clean-param: etext
            Sitemap: https://example.com/sitemap.xml
            """.ReplaceLineEndings("\n").TrimEnd(),
            body.ReplaceLineEndings("\n"));
        Assert.DoesNotContain("file://", body);
    }

    [Theory]
    [InlineData("")]
    [InlineData("file:///tmp/site")]
    public void Constructor_RejectsInvalidRootUrl(string rootUrl)
    {
        Assert.Throws<ArgumentException>(() => new Yasl.Net.SiteMapMiddleware.SitemapMiddleware(
            _ => Task.CompletedTask,
            rootUrl));
    }

    private static DefaultHttpContext CreateContext(string path)
    {
        var context = new DefaultHttpContext();
        context.Request.Path = path;
        context.Response.Body = new MemoryStream();
        return context;
    }

    private static async Task<string> ReadBodyAsync(HttpContext context)
    {
        context.Response.Body.Position = 0;
        using var reader = new StreamReader(context.Response.Body, Encoding.UTF8, leaveOpen: true);
        return await reader.ReadToEndAsync();
    }
}
