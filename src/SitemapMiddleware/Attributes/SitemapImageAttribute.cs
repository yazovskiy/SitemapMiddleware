namespace Yasl.Net.SiteMapMiddleware
{
    /// <summary>
    /// Attribute to include an image in the sitemap.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class SitemapImageAttribute : Attribute
    {
        /// <summary>
        /// URL of the image.
        /// </summary>
        public string ImageUrl { get; set; } = string.Empty;

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