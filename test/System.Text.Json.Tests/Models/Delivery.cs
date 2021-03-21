namespace System.Text.Json.Tests.Models
{
    public class Delivery
    {
        public string Type { get; set; } = "Post";

        public int[] Numbers { get; set; } = new[] { 1, 2, 3 };

        public Delivery(string? type)
        {
            if (type != null) Type = type;
        }
    }
}