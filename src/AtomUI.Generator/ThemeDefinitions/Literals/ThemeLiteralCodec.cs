namespace AtomUI.Generator;

internal static class ThemeLiteralCodec
{
    internal const string Identity = "atomui-core-literal-v2";
    internal static string NormalizeType(string type)
    {
        type = type.Replace("global::", "");
        if (type.EndsWith("?", StringComparison.Ordinal)) { type = type.Substring(0, type.Length - 1); }
        return type;
    }
    internal static bool Supports(string type)
    {
        var nullable = type.EndsWith("?", StringComparison.Ordinal) || type.Replace("global::", "").StartsWith("System.Nullable<", StringComparison.Ordinal);
        type = NormalizeType(type);
        if (type.StartsWith("System.Nullable<", StringComparison.Ordinal)) { type = type.Substring(16, type.Length - 17); }
        if (nullable && type is not ("Avalonia.Media.Color" or "AtomUI.TextDecorationInfo" or "System.String" or
            "Avalonia.Media.FontFamily" or "Avalonia.Animation.Easings.Easing" or "Avalonia.Media.IBrush" or
            "Avalonia.Media.IImmutableBrush" or "Avalonia.Media.SolidColorBrush" or "Avalonia.Media.Immutable.ImmutableTransform"))
        {
            return false;
        }
        return type is "System.String" or "System.Int32" or "System.Double" or "System.Single" or "System.Boolean" or "System.TimeSpan" or
            "Avalonia.Media.FontFamily" or "Avalonia.Media.Color" or "Avalonia.Thickness" or "Avalonia.CornerRadius" or "Avalonia.Point" or "Avalonia.Size" or
            "AtomUI.TextDecorationInfo" or "Avalonia.Media.BoxShadow" or "Avalonia.Media.BoxShadows" or "AtomUI.Dimension" or "Avalonia.Animation.Easings.Easing" or "Avalonia.Media.IBrush" or "Avalonia.Media.IImmutableBrush" or
            "Avalonia.Media.SolidColorBrush" or "Avalonia.Media.Immutable.ImmutableTransform" or
            "Avalonia.Media.FontWeight" or "Avalonia.Media.PenLineCap" or "Avalonia.Media.PenLineJoin" or "AtomUI.LineStyle";
    }
}
