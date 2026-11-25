using AtomUI.Theme.Definitions.Compiled;
using System.Text;

namespace AtomUI.Theme.Build;

// Build-only transport of one v2 theme document. This model never reconstructs runtime XML objects.
internal sealed class CompiledThemeInput
{
    internal string SourceIdentity = "";
    internal string SourcePath = "";
    internal string ContentDigest = "";
    internal string Id = "";
    internal string Name = "";
    internal string Language = "";
    /// <summary>The fixed style; an empty string leaves it open.</summary>
    internal string Style = "";
    /// <summary>The fixed appearance: -1 open, otherwise the numeric <c>ThemeAppearance</c>.</summary>
    internal int Appearance = -1;
    /// <summary>The fixed density; an empty string leaves it open.</summary>
    internal string Density = "";
    internal ThemeSectionInput Base = new();
    internal ThemeConditionInput[] Conditions = Array.Empty<ThemeConditionInput>();
    internal int Line;
    internal int Column;

    internal const string Extension = ".atomuithemeinput";
    // 8: theme definition format v2 (language, fixed axes, language tokens, condition sections, locked seeds).
    private const int FormatVersion = 8;
    private const int MaximumBytes = 8 * 1024 * 1024;
    private const int MaximumRecords = 65536;

    internal string Encode()
    {
        using var stream = new MemoryStream();
        using (var writer = new BinaryWriter(stream, new UTF8Encoding(false, true), true))
        {
            writer.Write(FormatVersion); writer.Write("atomui-theme-reader-xsd-v2");
            writer.Write(SourceIdentity); writer.Write(SourcePath); writer.Write(ContentDigest);
            writer.Write(Id); writer.Write(Name); writer.Write(Language); writer.Write(Style); writer.Write(Appearance);
            writer.Write(Density); writer.Write(Line); writer.Write(Column);
            Base.Write(writer);
            writer.Write(Conditions.Length);
            foreach (var condition in Conditions)
            {
                writer.Write(condition.Style); writer.Write(condition.Appearance); writer.Write(condition.Density);
                writer.Write(condition.Idiom); writer.Write(condition.Line); writer.Write(condition.Column);
                condition.Section.Write(writer);
            }
        }
        if (stream.Length > MaximumBytes) { throw new InvalidDataException("Compiled theme input exceeds its byte budget."); }
        return Convert.ToBase64String(stream.ToArray());
    }

    internal static CompiledThemeInput Decode(string text)
    {
        if (text.Length > MaximumBytes * 2) { throw new InvalidDataException("Compiled theme input exceeds its byte budget."); }
        var bytes = Convert.FromBase64String(text);
        if (bytes.Length > MaximumBytes) { throw new InvalidDataException("Compiled theme input exceeds its byte budget."); }
        using var stream = new MemoryStream(bytes, false);
        using var reader = new BinaryReader(stream, new UTF8Encoding(false, true));
        var version = reader.ReadInt32();
        if (version is > 0 and < FormatVersion) { throw new InvalidDataException("Compiled theme input format is out of date; rebuild the project."); }
        if (version != FormatVersion || reader.ReadString() != "atomui-theme-reader-xsd-v2") { throw new InvalidDataException("Unsupported compiled theme input version."); }
        var input = new CompiledThemeInput
        {
            SourceIdentity = reader.ReadString(), SourcePath = reader.ReadString(), ContentDigest = reader.ReadString(),
            Id = reader.ReadString(), Name = reader.ReadString(), Language = reader.ReadString(), Style = reader.ReadString(),
            Appearance = reader.ReadInt32(), Density = reader.ReadString(), Line = reader.ReadInt32(), Column = reader.ReadInt32()
        };
        input.Base = ThemeSectionInput.Read(reader);
        input.Conditions = new ThemeConditionInput[ReadCount(reader)];
        for (var i = 0; i < input.Conditions.Length; i++)
        {
            input.Conditions[i] = new ThemeConditionInput
            {
                Style = reader.ReadString(), Appearance = reader.ReadInt32(), Density = reader.ReadString(),
                Idiom = reader.ReadString(), Line = reader.ReadInt32(), Column = reader.ReadInt32(),
                Section = ThemeSectionInput.Read(reader)
            };
        }
        if (stream.Position != stream.Length || input.ContentDigest.Length != 64 || input.Appearance < -1 || input.Appearance > 1 ||
            !ThemeBuildIdentifier.IsValid(input.Language) || string.IsNullOrWhiteSpace(input.Id) ||
            (input.Style.Length != 0 && !ThemeBuildIdentifier.IsValid(input.Style)) ||
            (input.Density.Length != 0 && !ThemeBuildIdentifier.IsValid(input.Density)))
        { throw new InvalidDataException("Invalid compiled theme input."); }
        return input;
    }

    internal static int ReadCount(BinaryReader reader)
    {
        var count = reader.ReadInt32();
        if (count < 0 || count > MaximumRecords) { throw new InvalidDataException("Compiled theme record budget exceeded."); }
        return count;
    }
}

/// <summary>The values of one section: global tokens, language tokens and controls.</summary>
internal sealed class ThemeSectionInput
{
    internal ThemeLiteralInput[] Tokens = Array.Empty<ThemeLiteralInput>();
    internal ThemeLiteralInput[] LanguageTokens = Array.Empty<ThemeLiteralInput>();
    internal ThemeControlInput[] Controls = Array.Empty<ThemeControlInput>();

    internal void Write(BinaryWriter writer)
    {
        WriteTokens(writer, Tokens);
        WriteTokens(writer, LanguageTokens);
        writer.Write(Controls.Length);
        foreach (var control in Controls)
        {
            writer.Write(control.Catalog); writer.Write(control.Id); writer.Write(control.Appearance); writer.Write(control.Density);
            WriteTokens(writer, control.Tokens);
            writer.Write(control.Line); writer.Write(control.Column);
        }
    }

    internal static ThemeSectionInput Read(BinaryReader reader)
    {
        var section = new ThemeSectionInput { Tokens = ReadTokens(reader), LanguageTokens = ReadTokens(reader) };
        section.Controls = new ThemeControlInput[CompiledThemeInput.ReadCount(reader)];
        for (var i = 0; i < section.Controls.Length; i++)
        {
            section.Controls[i] = new ThemeControlInput
            {
                Catalog = reader.ReadString(), Id = reader.ReadString(), Appearance = reader.ReadInt32(), Density = reader.ReadString(),
                Tokens = ReadTokens(reader), Line = reader.ReadInt32(), Column = reader.ReadInt32()
            };
            if (section.Controls[i].Appearance < -1 || section.Controls[i].Appearance > 1) { throw new InvalidDataException("Invalid control appearance."); }
        }
        return section;
    }

    private static void WriteTokens(BinaryWriter writer, ThemeLiteralInput[] tokens)
    {
        writer.Write(tokens.Length);
        foreach (var token in tokens)
        {
            writer.Write(token.Name); writer.Write(token.Value); writer.Write(token.Locked); writer.Write(token.Line); writer.Write(token.Column);
            writer.Write(token.Path); writer.Write(token.Values.Length);
            foreach (var value in token.Values) { value.Write(writer); }
        }
    }

    private static ThemeLiteralInput[] ReadTokens(BinaryReader reader)
    {
        var tokens = new ThemeLiteralInput[CompiledThemeInput.ReadCount(reader)];
        for (var i = 0; i < tokens.Length; i++)
        {
            tokens[i] = new ThemeLiteralInput
            {
                Name = reader.ReadString(), Value = reader.ReadString(), Locked = reader.ReadBoolean(), Line = reader.ReadInt32(),
                Column = reader.ReadInt32(), Path = reader.ReadString()
            };
            var count = CompiledThemeInput.ReadCount(reader);
            if (count > 32) { throw new InvalidDataException("Too many literal capabilities."); }
            tokens[i].Values = new BuiltInThemeLiteralValue[count];
            for (var v = 0; v < count; v++) { tokens[i].Values[v] = BuiltInThemeLiteralValue.Read(reader); }
        }
        return tokens;
    }
}

internal sealed class ThemeConditionInput
{
    internal string Style = "";
    internal int Appearance = -1;
    internal string Density = "";
    internal string Idiom = "";
    internal ThemeSectionInput Section = new();
    internal int Line;
    internal int Column;
}

internal sealed class ThemeLiteralInput
{
    internal BuiltInThemeLiteralValue[] Values = Array.Empty<BuiltInThemeLiteralValue>();
    internal string Name = "";
    internal string Value = "";
    internal bool Locked;
    internal int Line;
    internal int Column;
    internal string Path = "";
}

internal sealed class ThemeControlInput
{
    internal string Catalog = "";
    internal string Id = "";
    /// <summary>-1 when the control does not fix its appearance.</summary>
    internal int Appearance = -1;
    /// <summary>Empty when the control does not fix its density.</summary>
    internal string Density = "";
    internal ThemeLiteralInput[] Tokens = Array.Empty<ThemeLiteralInput>();
    internal int Line;
    internal int Column;
}
