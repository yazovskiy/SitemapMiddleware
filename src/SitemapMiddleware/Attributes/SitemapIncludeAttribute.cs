namespace Yasl.Net.SiteMapMiddleware
{
    /// <summary>
    /// Attribute to include a controller or action in the sitemap.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class SitemapIncludeAttribute : Attribute
    {
        /// <summary>
        /// Priority of the page in the sitemap.
        /// </summary>
        public double Priority { get; set; } = 0.8;

        /// <summary>
        /// How frequently the page is likely to change.
        /// </summary>
        public string ChangeFreq { get; set; } = "daily";
    }
}
