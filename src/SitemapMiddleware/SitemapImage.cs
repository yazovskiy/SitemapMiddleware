namespace Yasl.Net.SiteMapMiddleware
{
    /// <summary>
    /// Represents an image in the sitemap.
    /// </summary>
    internal class SitemapImage
    {
        /// <summary>
        /// URL of the image.
        /// </summary>
        public string Location { get; set; } = string.Empty;

        /// <summary>
        /// Title of the image.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Caption of the image.
        /// </summary>
        public string Caption { get; set; } = string.Empty;

        /// <summary>
        /// License applied to the image.
        /// </summary>
        public string License { get; set; } = string.Empty;
    }
}