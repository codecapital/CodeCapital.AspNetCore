using System;

namespace CodeCapital.Services
{
    public class FeedItem
    {
        public FeedItem(string title, string description, string link)
        {
            Title = title;
            Description = description;
            Link = link;
        }

        public string Title { get; set; }
        public string Guid { get; set; } = "";
        public string Description { get; set; }
        public string Author { get; set; } = "";
        public DateTime? PubDate { get; set; }
        public string Link { get; set; }
    }
}
