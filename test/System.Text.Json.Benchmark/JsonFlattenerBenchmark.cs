using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using CodeCapital.System.Text.Json;

namespace System.Text.Json.Benchmark;

[Config(typeof(Config))]
public class JsonFlattenerBenchmark
{
    private class Config : ManualConfig
    {
        public Config()
        {
            AddDiagnoser(BenchmarkDotNet.Diagnosers.MemoryDiagnoser.Default);
        }
    }

    private JsonFlattener? _flattener;
    private string? _json;
    private Stream? _jsonStream;

    [GlobalSetup]
    public void Setup()
    {
        _flattener = new JsonFlattener();
        _json = File.ReadAllText("data/test-json-file.json");
        _jsonStream = new MemoryStream(Encoding.UTF8.GetBytes(_json));
    }

    [Benchmark]
    public List<IDictionary<string, object>> FlattenAsDictionary()
    {
        if (_json is null || _flattener is null) return new List<IDictionary<string, object>>();

        return _flattener.Flatten(_json);
    }

    [Benchmark]
    public async Task<List<IDictionary<string, object>>> FlattenAsyncAsDictionary()
    {
        if (_jsonStream is null || _flattener is null) return new List<IDictionary<string, object>>();

        _jsonStream.Position = 0;

        return await _flattener.FlattenAsync(_jsonStream);
    }
}