namespace AtomUI.Theme.Definitions.Compiled;

// BCL-only value transport shared by Core, the isolated worker and the generator.
internal sealed class BuiltInThemeLiteralValue
{
    internal string Type = "";
    internal string Canonical = "";
    internal string Text = "";
    internal string Kind = "";
    internal double[] Numbers = Array.Empty<double>();
    internal long Integer;
    internal BuiltInThemeLiteralValue[] Items = Array.Empty<BuiltInThemeLiteralValue>();

    internal void Write(BinaryWriter writer)
    {
        writer.Write(Type); writer.Write(Canonical); writer.Write(Text); writer.Write(Kind); writer.Write(Integer);
        writer.Write(Numbers.Length);
        foreach (var number in Numbers) { writer.Write(number); }
        writer.Write(Items.Length);
        foreach (var item in Items) { item.Write(writer); }
    }

    internal static BuiltInThemeLiteralValue Read(BinaryReader reader, int depth = 0)
    {
        if (depth > 1) { throw new InvalidDataException("Invalid nested theme value."); }
        var value = new BuiltInThemeLiteralValue { Type = reader.ReadString(), Canonical = reader.ReadString(),
            Text = reader.ReadString(), Kind = reader.ReadString(), Integer = reader.ReadInt64() };
        var count = reader.ReadInt32();
        if (count < 0 || count > 16) { throw new InvalidDataException("Invalid theme value components."); }
        value.Numbers = new double[count];
        for (var i = 0; i < count; i++) { value.Numbers[i] = reader.ReadDouble(); }
        count = reader.ReadInt32();
        if (count < 0 || count > 65536) { throw new InvalidDataException("Invalid theme value items."); }
        value.Items = new BuiltInThemeLiteralValue[count];
        for (var i = 0; i < count; i++) { value.Items[i] = Read(reader, depth + 1); }
        return value;
    }
}
