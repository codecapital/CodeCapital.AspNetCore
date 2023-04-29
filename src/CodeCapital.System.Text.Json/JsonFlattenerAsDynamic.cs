using System.Dynamic;
using System.Text.Json;

namespace CodeCapital.System.Text.Json;

public class JsonFlattenerAsDynamic
{
    private int _nestingLevel = 0;
    private string _currentPath = null!;
    private readonly Stack<string> _context = new();
    private JsonSerializerFlattenOptions _options = new();
    private ExpandoObject? _expandoObject;
    private readonly List<dynamic> _data = new();

    [Obsolete]
    public List<dynamic> Flatten(string json, JsonSerializerFlattenOptions? options = null)
    {
        if (json == null)
        {
            throw new ArgumentNullException(nameof(json));
        }

        _options = options ?? new JsonSerializerFlattenOptions();

        if (_options.MaxDepth <= 0) throw new ArgumentException($"{_options.MaxDepth} must be more than 0");

        var jsonDocumentOptions = new JsonDocumentOptions
        {
            CommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
        };

        ResetFlattener();

        using var doc = JsonDocument.Parse(json, jsonDocumentOptions);

        switch (doc.RootElement.ValueKind)
        {
            case JsonValueKind.Object:
                VisitElement(doc.RootElement);
                break;
            case JsonValueKind.Array:
                VisitValue(doc.RootElement);
                break;
            default:
                throw new FormatException($"Unsupported JSON token '{doc.RootElement.ValueKind}' was found.");
        }

        // There must be ToList to clone the list
        return _data.ToList();
    }

    public async Task<List<dynamic>> FlattenAsync(Stream jsonStream, JsonSerializerFlattenOptions? options = null)
    {
        if (jsonStream == null)
        {
            throw new ArgumentNullException(nameof(jsonStream));
        }

        _options = options ?? new JsonSerializerFlattenOptions();

        if (_options.MaxDepth <= 0) throw new ArgumentException($"{_options.MaxDepth} must be more than 0");

        var jsonDocumentOptions = new JsonDocumentOptions
        {
            CommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
        };

        ResetFlattener();

        using var doc = await JsonDocument.ParseAsync(jsonStream, jsonDocumentOptions).ConfigureAwait(false);

        switch (doc.RootElement.ValueKind)
        {
            case JsonValueKind.Object:
                VisitElement(doc.RootElement);
                break;
            case JsonValueKind.Array:
                VisitValue(doc.RootElement);
                break;
            default:
                throw new FormatException($"Unsupported JSON token '{doc.RootElement.ValueKind}' was found.");
        }

        // There must be ToList to clone the list
        return _data.ToList();
    }

    private void ResetFlattener()
    {
        _data.Clear();
        _context.Clear();
        _expandoObject = null;
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

                if (_expandoObject == null)
                {
                    foreach (var arrayElement in value.EnumerateArray())
                    {
                        CreateExpando();
                        VisitValue(arrayElement);
                        ProcessExpando();
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

    private void CreateExpando() => _expandoObject = new ExpandoObject();

    private void ProcessExpando()
    {
        if (_expandoObject == null) return;

        _data.Add(_expandoObject);

        _expandoObject = null;
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
            => (_expandoObject as IDictionary<string, object>)?.Add(keyItem, valueItem);

        static string RemoveIndentation(JsonElement valueItem)
            => valueItem.ToString()?.Replace("\t", "").Replace("\n", "") ?? string.Empty;
    }

    private void AddNullValue(string key)
        => (_expandoObject as IDictionary<string, object>)?.Add(key, "NULL");

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