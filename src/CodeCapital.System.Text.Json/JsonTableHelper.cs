﻿namespace CodeCapital.System.Text.Json;

/// <summary>
/// JsonTableHelper is a utility class that provides methods to work with
/// flattened JSON data in a table-like structure.
/// </summary>
public static class JsonTableHelper
{
    /// <summary>
    /// Extracts unique column names from a list of flattened JSON objects.
    /// </summary>
    /// <param name="data">A list of dynamic objects representing the flattened JSON data.</param>
    /// <returns>A HashSet of unique column names.</returns>
    public static HashSet<string> ExtractColumnNames(List<dynamic> data)
    {
        return data.SelectMany(row =>
        {
            var dictionary = row as IDictionary<string, object>;

            return dictionary?.Keys ?? Enumerable.Empty<string>();

        }).ToHashSet();
    }

    /// <summary>
    /// Gets the value of a specific column in a flattened JSON object.
    /// </summary>
    /// <param name="row">A dynamic object representing a flattened JSON object.</param>
    /// <param name="columnName">The name of the column to retrieve the value from.</param>
    /// <returns>The value of the specified column or null if the column does not exist.</returns>
    public static object? GetValue(dynamic row, string columnName)
    {
        if (row is IDictionary<string, object> dictionary
            && dictionary.ContainsKey(columnName))
        {
            return dictionary[columnName];
        }

        return null;
    }

    /// <summary>
    /// Gets the value of a specific column in a flattened JSON object as a string.
    /// </summary>
    /// <param name="row">A dynamic object representing a flattened JSON object.</param>
    /// <param name="columnName">The name of the column to retrieve the value from.</param>
    /// <returns>The value of the specified column as a string or "null" if the column does not exist.</returns>

    public static string GetValueAsString(dynamic row, string columnName)
        => GetValue(row, columnName) ?? "null";
}