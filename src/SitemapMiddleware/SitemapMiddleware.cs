using System.Reflection;
using System.Text;
using System.Xml.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Yasl.Net.SiteMapMiddleware
{
    /// <summary>
    /// Middleware to generate and serve a sitemap.xml file for application.
    /// </summary>
    /// <remarks>
    /// This middleware intercepts requests to "/sitemap.xml" and generates a sitemap based on the application's controllers and actions.
    /// It supports including images and videos in the sitemap.
    /// </remarks>
    /// <example>
    /// To use this middleware, add it to the request pipeline in the Startup.cs file:
    /// <code>
    /// public void Configure(IApplicationBuilder app, IHostingEnvironment env)
    /// {
    ///     app.UseMiddleware<SitemapMiddleware>("https://example.com");
    ///     // other middlewares
    /// }
    /// </code>
    /// </example>
    /// <param name="next">The next middleware in the request pipeline.</param>
    /// <param name="rootUrl">The root URL of the application.</param>
    public class SitemapMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly SitemapMiddlewareOptions _options;
        private readonly Uri _rootUri;
        private readonly string _rootUrl;
        private readonly string _sitemapPath;
        private readonly string _robotsPath;

        /// <summary>
        /// Initializes a new instance of the <see cref="SitemapMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next middleware in the request pipeline.</param>
        /// <param name="rootUrl">The root URL of the application.</param>
        /// 
        public SitemapMiddleware(RequestDelegate next, string rootUrl)
            : this(next, new SitemapMiddlewareOptions { RootUrl = rootUrl })
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SitemapMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next middleware in the request pipeline.</param>
        /// <param name="options">Options for sitemap and robots.txt generation.</param>
        public SitemapMiddleware(RequestDelegate next, SitemapMiddlewareOptions options)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _rootUri = _options.GetRootUri();
            _rootUrl = _rootUri.ToString().TrimEnd('/');
            _sitemapPath = SitemapMiddlewareOptions.NormalizeRequestPath(_options.SitemapPath, nameof(_options.SitemapPath));
            _robotsPath = SitemapMiddlewareOptions.NormalizeRequestPath(_options.RobotsPath, nameof(_options.RobotsPath));
        }

        /// <summary>
        /// Invokes the middleware to generate and serve the sitemap.xml file.
        /// </summary>
        /// <param name="context">Used for handling the HTTP request and response.</param>        
        /// <returns></returns>
        public async Task InvokeAsync(HttpContext context)
        {
            var requestPath = context.Request.Path.Value;
            if (string.Equals(requestPath, _sitemapPath, StringComparison.OrdinalIgnoreCase))
            {
                var urls = GenerateSitemapUrls();
                var sitemapXml = GenerateSitemapXml(urls);

                context.Response.ContentType = "application/xml";
                SetCacheHeaders(context);
                await context.Response.WriteAsync(sitemapXml);
                return;
            }

            if (_options.ServeRobotsTxt &&
                string.Equals(requestPath, _robotsPath, StringComparison.OrdinalIgnoreCase))
            {
                context.Response.ContentType = "text/plain; charset=utf-8";
                SetCacheHeaders(context);
                await context.Response.WriteAsync(GenerateRobotsTxt());
                return;
            }

            await _next(context);
        }

        private void SetCacheHeaders(HttpContext context)
        {
            if (_options.CacheMaxAgeSeconds > 0)
            {
                context.Response.Headers["Cache-Control"] = $"public,max-age={_options.CacheMaxAgeSeconds}";
            }
        }

        private string GenerateRobotsTxt()
        {
            var builder = new StringBuilder()
                .AppendLine("User-agent: *");

            foreach (var line in _options.RobotsTxtAdditionalLines)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    builder.AppendLine(line.Trim());
                }
            }

            return builder
                .Append("Sitemap: ")
                .Append(BuildAbsoluteUrl(_sitemapPath))
                .ToString();
        }

        private List<SitemapUrl> GenerateSitemapUrls()
        {
            var urls = new List<SitemapUrl>();
            urls.Add(new SitemapUrl
            {
                Location = _rootUrl,
                LastModified = DateTime.UtcNow,
                ChangeFrequency = "daily",
                Priority = 1.0
            });
            var assembly = Assembly.GetEntryAssembly();

            if (assembly == null)
                return urls;

            var controllers = assembly.GetTypes()
                .Where(type => typeof(Controller).IsAssignableFrom(type));

            foreach (var controller in controllers)
            {
                var controllerAttribute = controller.GetCustomAttribute<SitemapIncludeAttribute>();

                var methods = controller.GetMethods()
                    .Where(m => m.IsPublic && !m.IsSpecialName);

                foreach (var method in methods)
                {
                    var methodAttribute = method.GetCustomAttribute<SitemapIncludeAttribute>();
                    var imageAttributes = method.GetCustomAttributes<SitemapImageAttribute>().ToList();
                    var videoAttributes = method.GetCustomAttributes<SitemapVideoAttribute>().ToList();

                    if (methodAttribute != null || controllerAttribute != null)
                    {
                        var attr = methodAttribute ?? controllerAttribute;
                        var url = GenerateUrlFromMethod(controller, method);

                        urls.Add(new SitemapUrl
                        {
                            Location = BuildAbsoluteUrl(url),
                            LastModified = DateTime.UtcNow,
                            ChangeFrequency = attr?.ChangeFreq ?? "daily",
                            Priority = attr?.Priority ?? 0.5,
                            Images = imageAttributes.Select(img => new SitemapImage
                            {
                                Location = BuildAbsoluteUrl(img.ImageUrl),
                                Title = img.Title,
                                Caption = img.Caption,
                                License = img.License
                            }).ToList(),
                            Videos = videoAttributes.Select(video => new SitemapVideo
                            {
                                ThumbnailUrl = BuildAbsoluteUrl(video.ThumbnailUrl),
                                Title = video.Title,
                                Description = video.Description,
                                ContentUrl = video.ContentUrl,
                                PlayerUrl = video.PlayerUrl,
                                Duration = video.DurationSeconds,
                                ExpirationDate = video.ExpirationDate,
                                Rating = video.Rating,
                                ViewCount = video.ViewCount,
                                PublicationDate = video.PublicationDate,
                                Tags = video.Tags,
                                Category = video.Category,
                                FamilyFriendly = video.FamilyFriendly,
                                AllowEmbed = video.AllowEmbed,
                                Countries = video.Countries
                            }).ToList()
                        });
                    }
                }
            }

            return urls;
        }

        private string GenerateUrlFromMethod(Type controller, MethodInfo method)
        {
            var controllerName = controller.Name.Replace("Controller", "").ToLower();
            var actionName = method.Name.ToLower();
            return $"{controllerName}/{actionName}";
        }

        private string BuildAbsoluteUrl(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return _rootUrl;
            }

            var normalized = value.Trim();
            if (normalized.StartsWith("/", StringComparison.Ordinal))
            {
                return new Uri(_rootUri, normalized.TrimStart('/')).ToString();
            }

            if (Uri.TryCreate(normalized, UriKind.Absolute, out var absolute))
            {
                if (!string.Equals(absolute.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(absolute.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
                {
                    throw new ArgumentException("Sitemap URLs must use http or https.", nameof(value));
                }

                return absolute.ToString();
            }

            return new Uri(_rootUri, normalized).ToString();
        }

        private string GenerateSitemapXml(List<SitemapUrl> urls)
        {
            var xmlns = "http://www.sitemaps.org/schemas/sitemap/0.9";
            var imageXmlns = "http://www.google.com/schemas/sitemap-image/1.1";
            var videoXmlns = "http://www.google.com/schemas/sitemap-video/1.1";

            var doc = new XDocument(
                new XDeclaration("1.0", "utf-8", null),
                new XElement(XName.Get("urlset", xmlns),
                    new XAttribute(XNamespace.Xmlns + "image", imageXmlns),
                    new XAttribute(XNamespace.Xmlns + "video", videoXmlns),
                    urls.Select(url =>
                        new XElement(XName.Get("url", xmlns),
                            new XElement(XName.Get("loc", xmlns), url.Location),
                            new XElement(XName.Get("lastmod", xmlns), url.LastModified.ToString("yyyy-MM-dd")),
                            new XElement(XName.Get("changefreq", xmlns), url.ChangeFrequency),
                            new XElement(XName.Get("priority", xmlns), url.Priority.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture)),
                            url.Images.Select(img =>
                                new XElement(XName.Get("image", imageXmlns),
                                    new XElement(XName.Get("loc", imageXmlns), img.Location),
                                    img.Title != null ? new XElement(XName.Get("title", imageXmlns), img.Title) : null,
                                    img.Caption != null ? new XElement(XName.Get("caption", imageXmlns), img.Caption) : null,
                                    img.License != null ? new XElement(XName.Get("license", imageXmlns), img.License) : null
                                )
                            ),
                            url.Videos.Select(video =>
                                new XElement(XName.Get("video", videoXmlns),
                                    new XElement(XName.Get("thumbnail_loc", videoXmlns), video.ThumbnailUrl),
                                    new XElement(XName.Get("title", videoXmlns), video.Title),
                                    new XElement(XName.Get("description", videoXmlns), video.Description),
                                    video.ContentUrl != null ? new XElement(XName.Get("content_loc", videoXmlns), video.ContentUrl) : null,
                                    video.PlayerUrl != null ? new XElement(XName.Get("player_loc", videoXmlns), video.PlayerUrl) : null,
                                    video.Duration.HasValue ? new XElement(XName.Get("duration", videoXmlns), video.Duration) : null,
                                    video.ExpirationDate.HasValue ? new XElement(XName.Get("expiration_date", videoXmlns), video.ExpirationDate.Value.ToString("yyyy-MM-dd")) : null,
                                    video.Rating.HasValue ? new XElement(XName.Get("rating", videoXmlns), video.Rating) : null,
                                    video.ViewCount.HasValue ? new XElement(XName.Get("view_count", videoXmlns), video.ViewCount) : null,
                                    video.PublicationDate.HasValue ? new XElement(XName.Get("publication_date", videoXmlns), video.PublicationDate.Value.ToString("yyyy-MM-dd")) : null,
                                    video.Tags?.Select(tag => new XElement(XName.Get("tag", videoXmlns), tag)),
                                    video.Category != null ? new XElement(XName.Get("category", videoXmlns), video.Category) : null,
                                    video.FamilyFriendly.HasValue ? new XElement(XName.Get("family_friendly", videoXmlns), video.FamilyFriendly.Value.ToString().ToLower()) : null,
                                    video.AllowEmbed.HasValue ? new XElement(XName.Get("allow_embed", videoXmlns), video.AllowEmbed.Value.ToString().ToLower()) : null,
                                    video.Countries?.Select(country => new XElement(XName.Get("country", videoXmlns), country))
                                )
                            )
                        )
                    )
                )
            );

            return doc.ToString();
        }
    }
}
