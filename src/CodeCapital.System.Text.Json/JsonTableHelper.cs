namespace CodeCapital.System.Text.Json;

/// <summary>
/// JsonTableHelper is a utility class that provides methods to work with
/// flattened JSON data in a table-like structure.
/// </summary>
public static class JsonTableHelper
{
    /// <summary>
    /// Extracts the column names from the flattened JSON data.
    /// </summary>
    /// <param name="data">A list of dictionaries representing flattened JSON data.</param>
    /// <returns>A HashSet of unique column names found in the data.</returns>
    public static HashSet<string> ExtractColumnNames(List<IDictionary<string, object>> data)
        => data.SelectMany(row => row?.Keys ?? Enumerable.Empty<string>()).ToHashSet();

    /// <summary>
    /// Gets the value of the specified column name in the given row.
    /// </summary>
    /// <param name="row">A dictionary representing a row of flattened JSON data.</param>
    /// <param name="columnName">The name of the column to retrieve the value for.</param>
    /// <returns>The value of the specified column in the row, or null if the column does not exist in the row.</returns>
    public static object? GetValue(IDictionary<string, object> row, string columnName)
    {
        if (row.ContainsKey(columnName))
        {
            return row[columnName];
        }

        return null;
    }

    /// <summary>
    /// Gets the value of the specified column name in the given row as a string.
    /// </summary>
    /// <param name="row">A dictionary representing a row of flattened JSON data.</param>
    /// <param name="columnName">The name of the column to retrieve the value for.</param>
    /// <returns>The value of the specified column in the row as a string, or "null" if the column does not exist in the row.</returns>
    public static string GetValueAsString(IDictionary<string, object> row, string columnName)
        => GetValue(row, columnName)?.ToString() ?? "null";
}