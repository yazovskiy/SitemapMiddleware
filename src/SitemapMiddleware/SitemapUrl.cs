namespace Yasl.Net.SiteMapMiddleware
{
    /// <summary>
    /// Represents a URL in the sitemap.
    /// </summary>
    internal class SitemapUrl
    {
        /// <summary>
        /// URL of the page.
        /// </summary>
        public string Location { get; set; } = string.Empty;

        /// <summary>
        /// Last modified date of the page.
        /// </summary>
        public DateTime LastModified { get; set; }

        /// <summary>
        /// How frequently the page is likely to change.
        /// </summary>
        public string ChangeFrequency { get; set; } = string.Empty;

        /// <summary>
        /// Priority of the page in the sitemap.
        /// </summary>
        public double Priority { get; set; }

        /// <summary>
        /// Images of the page.
        /// </summary>
        public List<SitemapImage> Images { get; set; } = new List<SitemapImage>();

        /// <summary>
        /// Videos of the page.
        /// </summary>
        public List<SitemapVideo> Videos { get; set; } = new List<SitemapVideo>();
    }
}