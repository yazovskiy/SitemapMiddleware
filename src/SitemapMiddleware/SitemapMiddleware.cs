using System.Reflection;
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
        private readonly string _rootUrl;

        /// <summary>
        /// Initializes a new instance of the <see cref="SitemapMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next middleware in the request pipeline.</param>
        /// <param name="rootUrl">The root URL of the application.</param>
        /// 
        public SitemapMiddleware(RequestDelegate next, string rootUrl)
        {
            _next = next;
            _rootUrl = rootUrl.TrimEnd('/');
        }

        /// <summary>
        /// Invokes the middleware to generate and serve the sitemap.xml file.
        /// </summary>
        /// <param name="context">Used for handling the HTTP request and response.</param>        
        /// <returns></returns>
        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path.Value?.ToLower() == "/sitemap.xml")
            {
                var urls = GenerateSitemapUrls();
                var sitemapXml = GenerateSitemapXml(urls);

                context.Response.ContentType = "application/xml";
                await context.Response.WriteAsync(sitemapXml);
                return;
            }

            await _next(context);
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
            var assembly = Assembly.GetExecutingAssembly();

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
                            Location = $"{_rootUrl}/{url}",
                            LastModified = DateTime.UtcNow,
                            ChangeFrequency = attr?.ChangeFreq ?? "daily",
                            Priority = attr?.Priority ?? 0.5,
                            Images = imageAttributes.Select(img => new SitemapImage
                            {
                                Location = img.ImageUrl.StartsWith("http") ? img.ImageUrl : $"{_rootUrl}/{img.ImageUrl.TrimStart('/')}",
                                Title = img.Title,
                                Caption = img.Caption,
                                License = img.License
                            }).ToList(),
                            Videos = videoAttributes.Select(video => new SitemapVideo
                            {
                                ThumbnailUrl = video.ThumbnailUrl.StartsWith("http") ? video.ThumbnailUrl : $"{_rootUrl}/{video.ThumbnailUrl.TrimStart('/')}",
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
                            // Изображения
                            url.Images.Select(img =>
                                new XElement(XName.Get("image", imageXmlns),
                                    new XElement(XName.Get("loc", imageXmlns), img.Location),
                                    img.Title != null ? new XElement(XName.Get("title", imageXmlns), img.Title) : null,
                                    img.Caption != null ? new XElement(XName.Get("caption", imageXmlns), img.Caption) : null,
                                    img.License != null ? new XElement(XName.Get("license", imageXmlns), img.License) : null
                                )
                            ),
                            // Видео
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