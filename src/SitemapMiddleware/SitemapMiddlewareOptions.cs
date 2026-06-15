namespace Yasl.Net.SiteMapMiddleware
{
    /// <summary>
    /// Options used by <see cref="SitemapMiddleware"/>.
    /// </summary>
    public sealed class SitemapMiddlewareOptions
    {
        /// <summary>
        /// Absolute root URL of the application.
        /// </summary>
        public string RootUrl { get; set; } = string.Empty;

        /// <summary>
        /// Request path used to serve the sitemap document.
        /// </summary>
        public string SitemapPath { get; set; } = "/sitemap.xml";

        /// <summary>
        /// Enables robots.txt generation.
        /// </summary>
        public bool ServeRobotsTxt { get; set; }

        /// <summary>
        /// Request path used to serve robots.txt.
        /// </summary>
        public string RobotsPath { get; set; } = "/robots.txt";

        /// <summary>
        /// Additional robots.txt directives emitted after User-agent.
        /// </summary>
        public IList<string> RobotsTxtAdditionalLines { get; } = new List<string>();

        /// <summary>
        /// Public cache duration for generated metadata responses.
        /// </summary>
        public int CacheMaxAgeSeconds { get; set; } = 3600;

        internal Uri GetRootUri()
        {
            var normalized = (RootUrl ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(normalized))
            {
                throw new ArgumentException("RootUrl must be configured.", nameof(RootUrl));
            }

            normalized = normalized.TrimEnd('/') + "/";
            if (!Uri.TryCreate(normalized, UriKind.Absolute, out var uri))
            {
                throw new ArgumentException("RootUrl must be an absolute URI.", nameof(RootUrl));
            }

            if (!string.Equals(uri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("RootUrl must use http or https.", nameof(RootUrl));
            }

            return uri;
        }

        internal static string NormalizeRequestPath(string path, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException($"{parameterName} must be configured.", parameterName);
            }

            var trimmed = path.Trim();
            return trimmed.StartsWith("/", StringComparison.Ordinal) ? trimmed : "/" + trimmed;
        }
    }
}
