using System;
using System.Collections.Generic;
using System.Text;

namespace CodeCapital.Services
{
    public class SitemapService
    {
        public string MadeBy { get; set; } = "Sitemap Generator";

        public string GetSiteMap(string websiteUrl, string websiteName, List<string> pages)
        {
            if (string.IsNullOrWhiteSpace(websiteUrl)) throw new ArgumentException(nameof(websiteUrl));

            if (pages.Count == 0) return string.Empty;

            var html = new StringBuilder();

            html.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            html.Append("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");
            foreach (var item in pages)
            {
                html.AppendFormat("<url>");
                html.Append($"<loc>{websiteUrl}{item.ToLower()}</loc>");
                html.AppendFormat("<changefreq>always</changefreq>");
                html.AppendFormat("<priority>1.0</priority>");
                html.AppendFormat("</url>");
            }
            html.AppendFormat($"<!-- Items: {pages.Count} -->");
            html.AppendFormat($"<!-- Generated for {websiteName} by {MadeBy} -->");
            html.Append("</urlset>");

            return html.ToString();
        }
    }
}
