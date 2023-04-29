namespace CodeCapital.System.Text.Json;

/// <summary>
/// JsonSerializerFlattenOptions provides configuration options for the JSON flattening process.
/// </summary>
public class JsonSerializerFlattenOptions
{
    /// <summary>
    /// Gets or sets the maximum depth to flatten nested JSON objects.
    /// Default value is 1.
    /// </summary>
    public int MaxDepth { get; set; } = 1;

    /// <summary>
    /// Gets or sets a value indicating whether to remove indentation from the flattened JSON data.
    /// Default value is false.
    /// </summary>
    public bool RemoveIntended { get; set; }

    /// <summary>
    /// Gets or sets the delimiter used to separate individual keys in a path.
    /// Default value is ".".
    /// </summary>
    public string KeyDelimiter { get; set; } = ".";

    /// <summary>
    /// Gets or sets an optional starting token, which indicates the starting point of the JSON document
    /// to be flattened. If not specified, the root element will be used.
    /// Default value is null.
    /// </summary>
    public string? StartToken { get; set; }
}