using System.ComponentModel;
using System.Globalization;

namespace AtomUI.Desktop.Controls;

public enum SplitterCollapsibleIconDisplayMode
{
    Hover,
    Auto = Hover,
    Always,
    Hidden,
    True = Always,
    False = Hidden
}

[TypeConverter(typeof(SplitterPanelCollapsibleConverter))]
public class SplitterPanelCollapsible
{
    public bool IsEnabled { get; init; } = true;
    public SplitterCollapsibleIconDisplayMode ShowCollapsibleIcon { get; init; } =
        SplitterCollapsibleIconDisplayMode.Hover;

    public static SplitterPanelCollapsible Parse(string input)
    {
        var trimmed = input.Trim();
        if (trimmed.Length == 0)
        {
            throw new FormatException("Splitter panel collapsible value cannot be empty.");
        }

        if (bool.TryParse(trimmed, out var boolValue))
        {
            return new SplitterPanelCollapsible { IsEnabled = boolValue };
        }

        if (trimmed.Equals("none", StringComparison.OrdinalIgnoreCase))
        {
            return new SplitterPanelCollapsible
            {
                ShowCollapsibleIcon = SplitterCollapsibleIconDisplayMode.Hidden
            };
        }

        if (Enum.TryParse<SplitterCollapsibleIconDisplayMode>(trimmed, true, out var mode))
        {
            return new SplitterPanelCollapsible
            {
                ShowCollapsibleIcon = mode
            };
        }

        throw new FormatException($"Invalid splitter panel collapsible value: '{input}'.");
    }
}

public class SplitterPanelCollapsibleConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
    {
        return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
    }

    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        if (value is string str)
        {
            return SplitterPanelCollapsible.Parse(str);
        }

        return base.ConvertFrom(context, culture, value);
    }
}
