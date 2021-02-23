using System.Collections.Generic;
using System.Net;
using System.Text;

namespace CodeCapital.Services
{
    //https://mitchelsellers.com/blog/article/creating-an-rss-feed-in-asp-net-core-3-0
    public class RssService
    {
        private const string CustomUtcDateTimeFormat = "yyyy-MM-ddThh:mm:ssZ"; //"ddd, dd MMM yyyy HH:mm:ss 'UTC'";
        public string MadeBy { get; set; } = "RSS Generator";

        public string GetRssFeed(FeedItem feed, List<FeedItem> items)
        {
            if (items.Count == 0) return string.Empty;

            var html = new StringBuilder();

            html.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n\n");
            html.Append("<rss version=\"2.0\">\n");
            html.Append("<channel>\n");
            html.Append($"<title>{WebUtility.HtmlEncode(feed.Title)}</title>\n");
            html.Append($"<link>{feed.Link}</link>\n");
            html.Append($"<description>{feed.Description}</description>\n");
            foreach (var item in items)
            {
                html.AppendFormat("<item>\n");

                html.Append($"<title>{WebUtility.HtmlEncode(item.Title)}</title>\n");

                if (!string.IsNullOrWhiteSpace(item.Guid)) html.Append($"<guid>{item.Guid}</guid>\n");

                if (item.PubDate != null) html.Append($"<pubDate>{item.PubDate.Value.ToString(CustomUtcDateTimeFormat)}</pubDate>\n");

                html.Append($"<link>{feed.Link}{item.Link}</link>\n");

                html.Append($"<description>{WebUtility.HtmlEncode(item.Description)}</description>\n");

                if (!string.IsNullOrWhiteSpace(item.Author)) html.Append($"<author>{item.Author}</author>\n");

                html.AppendFormat("</item>\n");
            }
            html.AppendFormat($"<!-- Items: {items.Count} -->");
            html.AppendFormat($"<!-- Generated for {feed.Title} by {MadeBy} -->");
            html.Append("</channel>\n");
            html.Append("</rss>\n");
            return html.ToString();
        }

    }
}
