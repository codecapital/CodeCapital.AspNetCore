namespace CodeCapital.System.Text.Json
{
    public class JsonSerializerFlattenOptions
    {
        public int MaxDepth { get; set; } = 1;
        public bool RemoveIntended { get; set; }
    }
}