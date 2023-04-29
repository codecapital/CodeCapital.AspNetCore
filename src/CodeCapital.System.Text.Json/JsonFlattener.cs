using System.Text.Json;

namespace CodeCapital.System.Text.Json;

public class JsonFlattener
{
    private int _nestingLevel = 0;
    private string _currentPath = null!;
    private readonly Stack<string> _context = new();
    private JsonSerializerFlattenOptions _options = new();
    private IDictionary<string, object>? _flattenedObject;
    private readonly List<IDictionary<string, object>> _data = new();

    [Obsolete]
    public List<IDictionary<string, object>> Flatten(string json, JsonSerializerFlattenOptions? options = null)
    {
        Initialize(json, options);

        var jsonDocumentOptions = new JsonDocumentOptions
        {
            CommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
        };

        using var doc = JsonDocument.Parse(json, jsonDocumentOptions);

        ProcessRootElement(doc.RootElement);

        // There must be ToList to clone the list
        return _data.ToList();
    }

    public async Task<List<IDictionary<string, object>>> FlattenAsync(Stream jsonStream, JsonSerializerFlattenOptions? options = null)
    {
        Initialize(jsonStream, options);

        var jsonDocumentOptions = new JsonDocumentOptions
        {
            CommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
        };

        using var doc = await JsonDocument.ParseAsync(jsonStream, jsonDocumentOptions).ConfigureAwait(false);

        JsonElement startingElement;

        if (_options.StartToken == null)
        {
            startingElement = doc.RootElement;
        }
        else
        {
            if (doc.RootElement.ValueKind != JsonValueKind.Object)
            {
                throw new InvalidOperationException("The startToken parameter can only be used when the root element is an object.");
            }

            if (!doc.RootElement.TryGetProperty(_options.StartToken, out startingElement))
            {
                throw new ArgumentException($"The specified startToken '{_options.StartToken}' was not found in the JSON document.");
            }
        }

        ProcessRootElement(startingElement);

        // There must be ToList to clone the list
        return _data.ToList();
    }

    private void Initialize(string json, JsonSerializerFlattenOptions? options)
    {
        if (json == null)
        {
            throw new ArgumentNullException(nameof(json));
        }

        _options = options ?? new JsonSerializerFlattenOptions();

        if (_options.MaxDepth <= 0) throw new ArgumentException($"{_options.MaxDepth} must be more than 0");

        ResetFlattener();
    }

    private void Initialize(Stream jsonStream, JsonSerializerFlattenOptions? options)
    {
        if (jsonStream == null)
        {
            throw new ArgumentNullException(nameof(jsonStream));
        }

        _options = options ?? new JsonSerializerFlattenOptions();

        if (_options.MaxDepth <= 0) throw new ArgumentException($"{_options.MaxDepth} must be more than 0");

        ResetFlattener();
    }

    private void ProcessRootElement(JsonElement rootElement)
    {
        switch (rootElement.ValueKind)
        {
            case JsonValueKind.Object:
                VisitElement(rootElement);
                break;
            case JsonValueKind.Array:
                VisitValue(rootElement);
                break;
            default:
                throw new FormatException($"Unsupported JSON token '{rootElement.ValueKind}' was found.");
        }
    }

    private void ResetFlattener()
    {
        _data.Clear();
        _context.Clear();
        _flattenedObject = null;
        _currentPath = null!;
        _nestingLevel = 0;
    }

    private void VisitElement(JsonElement element)
    {
        foreach (var property in element.EnumerateObject())
        {
            EnterContext(property.Name);
            VisitValue(property.Value);
            ExitContext();
        }
    }

    private void VisitValue(JsonElement value)
    {
        IncreaseNesting();

        switch (value.ValueKind)
        {
            case JsonValueKind.Object:

                if (_nestingLevel > _options.MaxDepth + 1)
                    AddJsonValue(_currentPath, value);
                else
                    VisitElement(value);
                break;

            case JsonValueKind.Array:

                if (_flattenedObject == null)
                {
                    foreach (var arrayElement in value.EnumerateArray())
                    {
                        _flattenedObject = new Dictionary<string, object>();

                        VisitValue(arrayElement);

                        _data.Add(_flattenedObject);

                        _flattenedObject = null;
                    }
                }
                else
                {
                    AddJsonValue(_currentPath, value);
                }

                break;

            case JsonValueKind.Number:
            case JsonValueKind.String:
            case JsonValueKind.True:
            case JsonValueKind.False:

                AddJsonValue(_currentPath, value);

                break;

            case JsonValueKind.Null:

                AddNullValue(_currentPath);

                break;

            default:
                throw new FormatException($"Unsupported JSON token '{value.ValueKind}' was found.");
        }

        DecreaseNesting();
    }

    private void AddJsonValue(string key, JsonElement value)
    {
        if (_options.RemoveIntended && value.ValueKind is JsonValueKind.Object)
            AddKeyValue(key, RemoveIndentation(value));
        else if (value.ValueKind is JsonValueKind.Array)
            AddKeyValue(key, value.ToString() ?? "");
        else
            AddKeyValue(key, value.ToString() ?? "");

        void AddKeyValue(string keyItem, object valueItem)
            => _flattenedObject?.Add(keyItem, valueItem);

        static string RemoveIndentation(JsonElement valueItem)
            => valueItem.ToString()?.Replace("\t", "").Replace("\n", "") ?? string.Empty;
    }

    private void AddNullValue(string key)
        => _flattenedObject?.Add(key, "NULL");

    private void EnterContext(string context)
    {
        _context.Push(context);
        _currentPath = Combine(_context.Reverse());
    }

    private void ExitContext()
    {
        _context.Pop();
        _currentPath = Combine(_context.Reverse());
    }

    private string Combine(IEnumerable<string> pathSegments)
    {
        if (pathSegments == null)
        {
            throw new ArgumentNullException(nameof(pathSegments));
        }
        return string.Join(_options.KeyDelimiter, pathSegments);
    }

    private void IncreaseNesting() => _nestingLevel++;

    private void DecreaseNesting() => _nestingLevel--;
}