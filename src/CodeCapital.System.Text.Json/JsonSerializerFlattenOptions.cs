namespace CodeCapital.System.Text.Json;

public class JsonSerializerFlattenOptions
{
    public int MaxDepth { get; set; } = 1;
    public bool RemoveIntended { get; set; }

    /// <summary>
    /// The delimiter "." used to separate individual keys in a path.
    /// </summary>
    public string KeyDelimiter { get; set; } = ".";
}