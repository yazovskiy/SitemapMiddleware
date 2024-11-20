namespace Yasl.Net.SiteMapMiddleware
{
    /// <summary>
    /// Attribute to include a video in the sitemap.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class SitemapVideoAttribute : Attribute
    {
        /// <summary>
        /// URL of the video.
        /// </summary>
        public string ThumbnailUrl { get; set; } = string.Empty;

        /// <summary>
        /// Title of the video.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Description of the video.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Content URL of the video.
        /// </summary>
        public string ContentUrl { get; set; } = string.Empty;

        /// <summary>
        /// Player URL of the video.
        /// </summary>
        public string PlayerUrl { get; set; } = string.Empty;

        /// <summary>
        /// Duration of the video in seconds.
        /// </summary>
        public int? DurationSeconds { get; set; }

        /// <summary>
        /// Expiration date of the video.
        /// </summary>
        public DateTime? ExpirationDate { get; set; }

        /// <summary>
        /// Rating of the video.
        /// </summary>
        public double? Rating { get; set; }

        /// <summary>
        /// View count of the video.
        /// </summary>
        public int? ViewCount { get; set; }

        /// <summary>
        /// Publication date of the video.
        /// </summary>
        public DateTime? PublicationDate { get; set; }

        /// <summary>
        /// Tags of the video.
        /// </summary>
        public string[] Tags { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Category of the video.
        /// </summary>
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// Family friendly status of the video.
        /// </summary>
        public bool? FamilyFriendly { get; set; }

        /// <summary>
        /// Allow embedding status of the video.
        /// </summary>
        public bool? AllowEmbed { get; set; }

        /// <summary>
        /// Countries where the video is allowed.
        /// </summary>
        public string[] Countries { get; set; } = Array.Empty<string>();
    }
}