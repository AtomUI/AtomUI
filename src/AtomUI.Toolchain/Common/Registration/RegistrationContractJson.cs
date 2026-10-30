using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace AtomUI.Build.Tasks.Registration;

internal static class RegistrationContractJson
{
    internal static void ValidateShape(JsonElement value) => Validate(value, restrictScalars: true);

    internal static void ValidateUniqueFields(JsonElement value) => Validate(value, restrictScalars: false);

    private static void Validate(JsonElement value, bool restrictScalars)
    {
        if (value.ValueKind == JsonValueKind.Object)
        {
            var fields = new HashSet<string>(StringComparer.Ordinal);
            foreach (var field in value.EnumerateObject())
            {
                if (!fields.Add(field.Name))
                {
                    throw new InvalidDataException($"Duplicate registration JSON field '{field.Name}'.");
                }
                Validate(field.Value, restrictScalars);
            }
        }
        else if (value.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in value.EnumerateArray())
            {
                Validate(item, restrictScalars);
            }
        }
        else if (restrictScalars && value.ValueKind != JsonValueKind.String &&
                 !(value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out _)))
        {
            throw new InvalidDataException("Registration format 1 permits only objects, arrays, strings and int32 values.");
        }
    }

    internal static void Fields(JsonElement value, params string[] fields)
    {
        if (value.ValueKind != JsonValueKind.Object ||
            !value.EnumerateObject().Select(p => p.Name).Order(StringComparer.Ordinal).SequenceEqual(fields.Order(StringComparer.Ordinal)))
        {
            throw new InvalidDataException("Registration JSON has missing or unknown fields.");
        }
    }

    internal static string String(JsonElement value, string field)
    {
        var item = value.GetProperty(field);
        if (item.ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(item.GetString()))
        {
            throw new InvalidDataException($"Registration field '{field}' must be a non-empty string.");
        }
        return item.GetString()!;
    }

    internal static RegistrationTypeIdentity Type(JsonElement value)
    {
        Fields(value, "assembly", "metadataName");
        var assembly = String(value, "assembly");
        var name = String(value, "metadataName");
        try
        {
            var parsed = new AssemblyName(assembly);
            if (parsed.FullName != assembly || parsed.Version is null || parsed.CultureName is null || parsed.GetPublicKeyToken() is null)
            {
                throw new InvalidDataException("Registration type requires a canonical complete assembly identity.");
            }
        }
        catch (Exception error) when (error is ArgumentException or FileLoadException)
        {
            throw new InvalidDataException("Malformed registration defining assembly identity.", error);
        }
        if (name.Any(c => char.IsWhiteSpace(c) || c is '`' or '[' or ']' or '*' or '&' or ',' or '/' or '\\' or '\0') ||
            name.Split('+').Any(string.IsNullOrEmpty))
        {
            throw new InvalidDataException("Registration format 1 conditions and generated members require named non-generic types.");
        }
        return new(assembly, name);
    }

    internal static string Canonical(JsonElement value, string? omitField = null) => value.ValueKind switch
    {
        JsonValueKind.Object => "{" + string.Join(",", value.EnumerateObject().Where(p => p.Name != omitField)
            .OrderBy(p => p.Name, StringComparer.Ordinal).Select(p => Quote(p.Name) + ":" + Canonical(p.Value))) + "}",
        JsonValueKind.Array => "[" + string.Join(",", value.EnumerateArray().Select(p => Canonical(p))) + "]",
        JsonValueKind.String => Quote(value.GetString()!),
        JsonValueKind.Number => value.GetInt32().ToString(CultureInfo.InvariantCulture),
        _ => throw new InvalidDataException("Unsupported value in registration canonical JSON.")
    };

    internal static string Quote(string value)
    {
        var result = new StringBuilder("\"");
        foreach (var c in value)
        {
            if (c < 0x20)
            {
                result.Append("\\u").Append(((int)c).ToString("x4", CultureInfo.InvariantCulture));
            }
            else
            {
                if (c is '"' or '\\')
                {
                    result.Append('\\');
                }
                result.Append(c);
            }
        }
        return result.Append('"').ToString();
    }
}
