using BenchmarkDotNet.Running;
using System.Text.Json.Benchmark;

var summary = BenchmarkRunner.Run<JsonFlattenerBenchmark>();