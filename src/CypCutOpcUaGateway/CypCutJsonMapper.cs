using System.Globalization;
using System.Text.Json;

namespace CypCutOpcUaGateway;

public static class CypCutJsonMapper
{
    public static IReadOnlyDictionary<string, object?> Map(string json)
    {
        using var document = JsonDocument.Parse(json);
        var values = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        foreach (var definition in ParameterCatalog.All)
        {
            if (TryFindCategory(document.RootElement, definition.Category, out var category) &&
                TryFindProperty(category, definition.Name, out var element))
            {
                values[definition.Key] = ConvertValue(element, definition.Kind);
            }
            else if (TryFindProperty(document.RootElement, definition.Name, out element))
            {
                values[definition.Key] = ConvertValue(element, definition.Kind);
            }
        }
        return values;
    }

    private static bool TryFindCategory(JsonElement root, string category, out JsonElement result)
    {
        if (category == "State") { result = root; return true; }
        return TryFindObject(root, category, out result);
    }

    private static bool TryFindObject(JsonElement element, string name, out JsonElement result)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                if (property.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && property.Value.ValueKind == JsonValueKind.Object)
                {
                    result = property.Value;
                    return true;
                }
                if (TryFindObject(property.Value, name, out result)) return true;
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray()) if (TryFindObject(item, name, out result)) return true;
        }
        result = default;
        return false;
    }

    private static bool TryFindProperty(JsonElement element, string name, out JsonElement result)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                if (property.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) { result = property.Value; return true; }
            }
            foreach (var property in element.EnumerateObject()) if (TryFindProperty(property.Value, name, out result)) return true;
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray()) if (TryFindProperty(item, name, out result)) return true;
        }
        result = default;
        return false;
    }

    private static object? ConvertValue(JsonElement element, ParameterValueKind kind)
    {
        if (element.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined) return null;
        return kind switch
        {
            ParameterValueKind.Boolean => element.ValueKind == JsonValueKind.True ||
                                          (element.ValueKind == JsonValueKind.String && bool.TryParse(element.GetString(), out var b) && b) ||
                                          (element.ValueKind == JsonValueKind.Number && element.TryGetInt64(out var bi) && bi != 0),
            ParameterValueKind.Integer => element.ValueKind == JsonValueKind.Number && element.TryGetInt64(out var i) ? i :
                                          long.TryParse(element.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out i) ? i : 0L,
            ParameterValueKind.Number => element.ValueKind == JsonValueKind.Number && element.TryGetDouble(out var n) ? n :
                                         double.TryParse(element.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out n) ? n : double.NaN,
            _ => element.ValueKind == JsonValueKind.String ? element.GetString() ?? string.Empty : element.ToString()
        };
    }
}
