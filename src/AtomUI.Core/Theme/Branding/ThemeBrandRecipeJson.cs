using System.Text;
using System.Text.Json;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Schema;

namespace AtomUI.Theme.Branding;

internal static class ThemeBrandRecipeJson
{
    private const int MaxBytes = 1024 * 1024;
    private const int MaxTokens = 32768;

    internal static ThemeBrandRecipeReadResult Read(string json)
    {
        ArgumentNullException.ThrowIfNull(json);
        try
        {
            if (json.Length > MaxBytes || Encoding.UTF8.GetByteCount(json) > MaxBytes)
            {
                throw new FormatException("The recipe exceeds the one MiB input limit.");
            }
            using var document = JsonDocument.Parse(json, new JsonDocumentOptions { MaxDepth = 16 });
            var root = document.RootElement;
            ValidateStrings(root);
            Fields(root, "version", "kind", "id", "revision", "themeId", "requires", "config", "assets");
            if (!root.GetProperty("version").TryGetInt32(out var version) || version != 1)
            {
                throw new FormatException("Only brand recipe version 1 is supported.");
            }
            var kind = Text(root, "kind") switch
            {
                "complete" => ThemeBrandRecipeKind.Complete,
                "token-only" => ThemeBrandRecipeKind.TokenOnly,
                _ => throw new FormatException("Unknown recipe kind.")
            };
            var requires = root.GetProperty("requires");
            Fields(requires, "schema", "definition", "program", "capabilities");
            var capabilities = ThemeStyleCapabilitiesFromJson(requires.GetProperty("capabilities"));
            var program = requires.GetProperty("program");
            var requirements = new ThemeBrandRequirements(Text(requires, "schema"), Text(requires, "definition"),
                program.ValueKind == JsonValueKind.Null ? null : program.GetString(), capabilities);
            var config = root.GetProperty("config");
            Fields(config, "inherit", "algorithms", "tokens", "controls");
            var tokenCount = 0;
            var tokens = Tokens(config.GetProperty("tokens"), ref tokenCount);
            var controls = new Dictionary<ControlTokenIdentity, ControlThemeConfig>();
            foreach (var control in Items(config.GetProperty("controls"), 2048))
            {
                Fields(control, "catalog", "id", "algorithm", "algorithms", "tokens");
                var mode = Text(control, "algorithm") switch
                {
                    "Unspecified" => ControlAlgorithmMode.Unspecified,
                    "Disabled" => ControlAlgorithmMode.Disabled,
                    "Global" => ControlAlgorithmMode.Global,
                    "Custom" => ControlAlgorithmMode.Custom,
                    _ => throw new FormatException("Unknown control algorithm mode.")
                };
                controls.Add(new ControlTokenIdentity(Text(control, "catalog"), Text(control, "id")),
                    new ControlThemeConfig(mode, Algorithms(control.GetProperty("algorithms")),
                        Tokens(control.GetProperty("tokens"), ref tokenCount)));
            }
            var assets = new List<ThemeStyleAssetReference>();
            foreach (var asset in Items(root.GetProperty("assets"), 256))
            {
                Fields(asset, "id", "revision");
                assets.Add(new ThemeStyleAssetReference(Text(asset, "id"), Text(asset, "revision")));
            }
            return new ThemeBrandRecipeReadResult(new ThemeBrandRecipe(Text(root, "id"), Text(root, "revision"),
                Text(root, "themeId"), new ThemeConfig(config.GetProperty("inherit").GetBoolean(),
                    Algorithms(config.GetProperty("algorithms")), tokens, controls), assets, requirements, kind), []);
        }
        catch (Exception exception) when (exception is JsonException or FormatException or ArgumentException or
                                          InvalidOperationException or KeyNotFoundException or OverflowException)
        {
            return new ThemeBrandRecipeReadResult(null,
                [new ThemeDiagnostic("ATMTHM7201", ThemeDiagnosticSeverity.Error, "BrandRecipe", "$", exception.Message)]);
        }
    }

    internal static string Write(ThemeBrandRecipe recipe)
    {
        ArgumentNullException.ThrowIfNull(recipe);
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            writer.WriteStartObject();
            writer.WriteNumber("version", recipe.Version);
            writer.WriteString("kind", recipe.Kind == ThemeBrandRecipeKind.Complete ? "complete" : "token-only");
            writer.WriteString("id", recipe.Id);
            writer.WriteString("revision", recipe.Revision);
            writer.WriteString("themeId", recipe.ThemeId);
            writer.WriteStartObject("requires");
            writer.WriteString("schema", recipe.Requirements.SchemaRevision);
            writer.WriteString("definition", recipe.Requirements.DefinitionDigest);
            writer.WriteString("program", recipe.Requirements.ProgramDigest);
            writer.WriteStartArray("capabilities");
            foreach (var capability in CapabilityNames)
            {
                if ((recipe.Requirements.Capabilities & capability.Value) != 0)
                {
                    writer.WriteStringValue(capability.Key);
                }
            }
            writer.WriteEndArray();
            writer.WriteEndObject();
            WriteConfig(writer, recipe.Config);
            writer.WriteStartArray("assets");
            foreach (var asset in recipe.Assets)
            {
                writer.WriteStartObject();
                writer.WriteString("id", asset.Id);
                writer.WriteString("revision", asset.Revision);
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
            writer.WriteEndObject();
        }
        var text = Encoding.UTF8.GetString(stream.ToArray());
        // The writer cannot produce a document that its bounded reader would reject.
        var validation = Read(text);
        if (!validation.Success)
        {
            throw new ArgumentException(validation.Diagnostics[0].Message, nameof(recipe));
        }
        return text;
    }

    private static void WriteConfig(Utf8JsonWriter writer, ThemeConfig config)
    {
        writer.WriteStartObject("config");
        writer.WriteBoolean("inherit", config.Inherit);
        WriteAlgorithms(writer, config.Algorithms);
        WriteTokens(writer, config.Tokens);
        writer.WriteStartArray("controls");
        foreach (var entry in config.Controls.OrderBy(static item => item.Key.Catalog, StringComparer.Ordinal)
                     .ThenBy(static item => item.Key.Id, StringComparer.Ordinal))
        {
            writer.WriteStartObject();
            writer.WriteString("catalog", entry.Key.Catalog);
            writer.WriteString("id", entry.Key.Id);
            writer.WriteString("algorithm", entry.Value.Algorithm.ToString());
            WriteAlgorithms(writer, entry.Value.Algorithms);
            WriteTokens(writer, entry.Value.Tokens);
            writer.WriteEndObject();
        }
        writer.WriteEndArray();
        writer.WriteEndObject();
    }

    private static void WriteAlgorithms(Utf8JsonWriter writer, IReadOnlyList<ThemeAlgorithm>? algorithms)
    {
        if (algorithms is null)
        {
            writer.WriteNull("algorithms");
            return;
        }
        writer.WriteStartArray("algorithms");
        foreach (var algorithm in algorithms)
        {
            writer.WriteStringValue(algorithm.ToString());
        }
        writer.WriteEndArray();
    }

    private static void WriteTokens(Utf8JsonWriter writer, IReadOnlyDictionary<string, string> tokens)
    {
        writer.WriteStartObject("tokens");
        foreach (var token in tokens.OrderBy(static item => item.Key, StringComparer.Ordinal))
        {
            writer.WriteString(token.Key, token.Value);
        }
        writer.WriteEndObject();
    }

    private static ThemeAlgorithm[]? Algorithms(JsonElement value)
    {
        if (value.ValueKind == JsonValueKind.Null)
        {
            return null;
        }
        return Items(value, 3).Select(static item => item.GetString() switch
        {
            "Default" => ThemeAlgorithm.Default,
            "Dark" => ThemeAlgorithm.Dark,
            "Compact" => ThemeAlgorithm.Compact,
            _ => throw new FormatException("Unknown theme algorithm.")
        }).ToArray();
    }

    private static Dictionary<string, string> Tokens(JsonElement element, ref int count)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            throw new FormatException("Tokens must be an object.");
        }
        var tokens = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var item in element.EnumerateObject())
        {
            if (++count > MaxTokens)
            {
                throw new FormatException("The recipe exceeds the Token entry limit.");
            }
            tokens.Add(item.Name, item.Value.GetString() ?? throw new FormatException("Token values must be strings."));
        }
        return tokens;
    }

    private static string Text(JsonElement value, string name) => value.GetProperty(name).GetString() ??
        throw new FormatException($"'{name}' must be a string.");

    private static IEnumerable<JsonElement> Items(JsonElement value, int limit)
    {
        if (value.ValueKind != JsonValueKind.Array || value.GetArrayLength() > limit)
        {
            throw new FormatException($"An array with at most {limit} items is required.");
        }
        return value.EnumerateArray();
    }

    private static void Fields(JsonElement value, params string[] names)
    {
        if (value.ValueKind != JsonValueKind.Object)
        {
            throw new FormatException("A recipe object is required.");
        }
        var remaining = names.ToHashSet(StringComparer.Ordinal);
        foreach (var property in value.EnumerateObject())
        {
            if (!remaining.Remove(property.Name))
            {
                throw new FormatException($"Unknown or repeated member '{property.Name}'.");
            }
        }
        if (remaining.Count != 0)
        {
            throw new FormatException($"Missing member '{remaining.Order(StringComparer.Ordinal).First()}'.");
        }
    }

    private static void ValidateStrings(JsonElement value)
    {
        if (value.ValueKind == JsonValueKind.String && value.GetString()!.Length > 65536)
        {
            throw new FormatException("A recipe string exceeds the length limit.");
        }
        if (value.ValueKind == JsonValueKind.Array)
        {
            foreach (var child in value.EnumerateArray())
            {
                ValidateStrings(child);
            }
        }
        else if (value.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in value.EnumerateObject())
            {
                if (property.Name.Length > 65536)
                {
                    throw new FormatException("A recipe member name exceeds the length limit.");
                }
                ValidateStrings(property.Value);
            }
        }
    }

    private static readonly KeyValuePair<string, ThemeStyleCapabilities>[] CapabilityNames =
    [
        new("QualifiedStyles", ThemeStyleCapabilities.QualifiedStyles),
        new("CompiledTemplates", ThemeStyleCapabilities.CompiledTemplates),
        new("RegisteredStaticAssets", ThemeStyleCapabilities.RegisteredStaticAssets),
        new("ScopedResources", ThemeStyleCapabilities.ScopedResources)
    ];

    private static ThemeStyleCapabilities ThemeStyleCapabilitiesFromJson(JsonElement element)
    {
        var result = (ThemeStyleCapabilities)0;
        foreach (var item in Items(element, CapabilityNames.Length))
        {
            var entry = CapabilityNames.FirstOrDefault(pair => pair.Key == item.GetString());
            if (entry.Key is null || (result & entry.Value) != 0)
            {
                throw new FormatException("Unknown or repeated style capability.");
            }
            result |= entry.Value;
        }
        return result;
    }
}
