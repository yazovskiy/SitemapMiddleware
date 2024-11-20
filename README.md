# SitemapMiddleware

SitemapMiddleware is a middleware component for generating and serving sitemaps in a web application. This project helps in improving the SEO of your website by providing an up-to-date sitemap.

## Features

- Automatically generates sitemaps for your web application.
- Easy integration with existing web frameworks.

## Installation

To install SitemapMiddleware, use the following command:

```sh
dotnet add package Yasl.Net.SitemapMiddleware
```

## Usage

To use this middleware, add it to the request pipeline in the Startup.cs file:
```csharp
public void Configure(IApplicationBuilder app, IHostingEnvironment env)
{
    app.UseMiddleware<SitemapMiddleware>("https://example.com");
    // other middlewares
}
```

## Contributing

Contributions are welcome! Please fork the repository and submit a pull request.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

## Contact

For any questions or suggestions, please open an issue on GitHub.
