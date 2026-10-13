using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AtomUI.Theme.Build;

// Build-only transport. This model never reconstructs runtime XML objects.
internal sealed class CompiledThemeInput
{
    internal string SourceIdentity = "";
    internal string SourcePath = "";
    internal string ContentDigest = "";
    internal string Id = "";
    internal string Name = "";
    internal int Appearance;
    internal bool IsDefault;
    internal int[] Algorithms = Array.Empty<int>();
    internal ThemeLiteralInput[] Tokens = Array.Empty<ThemeLiteralInput>();
    internal ThemeControlInput[] Controls = Array.Empty<ThemeControlInput>();

    internal const string Extension = ".atomuithemeinput";
    private const int MaximumBytes = 8 * 1024 * 1024;
    private const int MaximumRecords = 65536;
    internal string Encode()
    {
        using var stream = new MemoryStream();
        using (var writer = new BinaryWriter(stream, new UTF8Encoding(false, true), true))
        {
            writer.Write(3); writer.Write("atomui-theme-reader-xsd-v1");
            writer.Write(SourceIdentity); writer.Write(SourcePath); writer.Write(ContentDigest);
            writer.Write(Id); writer.Write(Name); writer.Write(Appearance); writer.Write(IsDefault);
            WriteAlgorithms(writer, Algorithms); WriteTokens(writer, Tokens);
            writer.Write(Controls.Length);
            foreach (var control in Controls)
            {
                writer.Write(control.Catalog); writer.Write(control.Id); writer.Write(control.Mode);
                WriteAlgorithms(writer, control.Algorithms); WriteTokens(writer, control.Tokens);
                writer.Write(control.Line); writer.Write(control.Column);
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
        if (reader.ReadInt32() != 3 || reader.ReadString() != "atomui-theme-reader-xsd-v1") { throw new InvalidDataException("Unsupported compiled theme input version."); }
        var input = new CompiledThemeInput
        {
            SourceIdentity = reader.ReadString(), SourcePath = reader.ReadString(), ContentDigest = reader.ReadString(),
            Id = reader.ReadString(), Name = reader.ReadString(), Appearance = reader.ReadInt32(), IsDefault = reader.ReadBoolean(),
            Algorithms = ReadAlgorithms(reader), Tokens = ReadTokens(reader)
        };
        input.Controls = new ThemeControlInput[ReadCount(reader)];
        for (var i = 0; i < input.Controls.Length; i++)
        {
            input.Controls[i] = new ThemeControlInput
            {
                Catalog = reader.ReadString(), Id = reader.ReadString(), Mode = reader.ReadInt32(),
                Algorithms = ReadAlgorithms(reader), Tokens = ReadTokens(reader), Line = reader.ReadInt32(), Column = reader.ReadInt32()
            };
        }
        if (stream.Position != stream.Length || input.ContentDigest.Length != 64 || input.Appearance < 0 || input.Appearance > 1 || string.IsNullOrWhiteSpace(input.Id))
        { throw new InvalidDataException("Invalid compiled theme input."); }
        return input;
    }
    private static int ReadCount(BinaryReader reader)
    {
        var count = reader.ReadInt32();
        if (count < 0 || count > MaximumRecords) { throw new InvalidDataException("Compiled theme record budget exceeded."); }
        return count;
    }
    private static void WriteAlgorithms(BinaryWriter writer, int[] values)
    {
        writer.Write(values.Length); foreach (var value in values) { writer.Write(value); }
    }
    private static int[] ReadAlgorithms(BinaryReader reader)
    {
        var values = new int[ReadCount(reader)];
        for (var i = 0; i < values.Length; i++) { values[i] = reader.ReadInt32(); if (values[i] < 0 || values[i] > 2) { throw new InvalidDataException("Unknown algorithm."); } }
        return values;
    }
    private static void WriteTokens(BinaryWriter writer, ThemeLiteralInput[] tokens)
    {
        writer.Write(tokens.Length);
        foreach (var token in tokens) { writer.Write(token.Name); writer.Write(token.Value); writer.Write(token.Line); writer.Write(token.Column); writer.Write(token.Path); }
    }
    private static ThemeLiteralInput[] ReadTokens(BinaryReader reader)
    {
        var tokens = new ThemeLiteralInput[ReadCount(reader)];
        for (var i = 0; i < tokens.Length; i++) { tokens[i] = new ThemeLiteralInput { Name = reader.ReadString(), Value = reader.ReadString(), Line = reader.ReadInt32(), Column = reader.ReadInt32(), Path = reader.ReadString() }; }
        return tokens;
    }
}
internal sealed class ThemeLiteralInput
{
    internal string Name = "";
    internal string Value = "";
    internal int Line;
    internal int Column;
    internal string Path = "";
}
internal sealed class ThemeControlInput
{
    internal string Catalog = "";
    internal string Id = "";
    internal int Mode;
    internal int[] Algorithms = Array.Empty<int>();
    internal ThemeLiteralInput[] Tokens = Array.Empty<ThemeLiteralInput>();
    internal int Line;
    internal int Column;
}
