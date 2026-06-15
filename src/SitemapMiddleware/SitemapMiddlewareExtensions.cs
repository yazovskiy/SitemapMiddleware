using Microsoft.AspNetCore.Builder;

namespace Yasl.Net.SiteMapMiddleware
{
    /// <summary>
    /// Registration helpers for <see cref="SitemapMiddleware"/>.
    /// </summary>
    public static class SitemapMiddlewareExtensions
    {
        /// <summary>
        /// Adds sitemap and optional robots.txt generation to the application pipeline.
        /// </summary>
        public static IApplicationBuilder UseSitemapMiddleware(
            this IApplicationBuilder app,
            string rootUrl,
            Action<SitemapMiddlewareOptions>? configure = null)
        {
            ArgumentNullException.ThrowIfNull(app);

            var options = new SitemapMiddlewareOptions
            {
                RootUrl = rootUrl
            };

            configure?.Invoke(options);
            return app.UseMiddleware<SitemapMiddleware>(options);
        }
    }
}
