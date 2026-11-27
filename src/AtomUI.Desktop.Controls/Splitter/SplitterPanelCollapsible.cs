using System.ComponentModel;
using System.Globalization;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// When a splitter bar shows the icon that collapses or expands the panel next to it.
/// </summary>
public enum SplitterCollapsibleIconDisplayMode
{
    /// <summary>
    /// Shows the icon only while the pointer is over the splitter bar.
    /// </summary>
    Hover,
    /// <summary>
    /// Same as <see cref="Hover" />.
    /// </summary>
    Auto = Hover,
    /// <summary>
    /// Shows the icon regardless of the pointer position.
    /// </summary>
    Always,
    /// <summary>
    /// Never shows the icon.
    /// </summary>
    Hidden,
    /// <summary>
    /// Same as <see cref="Always" />.
    /// </summary>
    True = Always,
    /// <summary>
    /// Same as <see cref="Hidden" />.
    /// </summary>
    False = Hidden
}

/// <summary>
/// The collapsible settings of one panel of a <see cref="Splitter" />, carried by the attached
/// <see cref="Splitter.CollapsibleProperty" /> property that is set on the panel. In XAML the attribute text is
/// parsed by <see cref="Parse" />.
/// </summary>
[TypeConverter(typeof(SplitterPanelCollapsibleConverter))]
public class SplitterPanelCollapsible
{
    /// <summary>
    /// Whether the panel can be collapsed from the splitter bars next to it and expanded again. The default is
    /// <see langword="true" />.
    /// </summary>
    public bool IsEnabled { get; init; } = true;
    /// <summary>
    /// When the splitter bars next to the panel show the icon that collapses or expands it. The default is
    /// <see cref="SplitterCollapsibleIconDisplayMode.Hover" />.
    /// </summary>
    public SplitterCollapsibleIconDisplayMode ShowCollapsibleIcon { get; init; } =
        SplitterCollapsibleIconDisplayMode.Hover;

    /// <summary>
    /// Parses the text of the attached <see cref="Splitter.CollapsibleProperty" /> property, ignoring case and
    /// surrounding white space. <c>true</c> and <c>false</c> set <see cref="IsEnabled" />; <c>none</c> sets
    /// <see cref="ShowCollapsibleIcon" /> to <see cref="SplitterCollapsibleIconDisplayMode.Hidden" />; the name of a
    /// member of <see cref="SplitterCollapsibleIconDisplayMode" /> sets <see cref="ShowCollapsibleIcon" /> to it.
    /// </summary>
    /// <exception cref="FormatException">
    /// <paramref name="input" /> is empty or none of these texts.
    /// </exception>
    public static SplitterPanelCollapsible Parse(string input)
    {
        var trimmed = input.AsSpan().Trim();
        if (trimmed.Length == 0)
        {
            throw new FormatException("Splitter panel collapsible value cannot be empty.");
        }

        if (bool.TryParse(trimmed, out var boolValue))
        {
            return new SplitterPanelCollapsible { IsEnabled = boolValue };
        }

        if (trimmed.Equals("none".AsSpan(), StringComparison.OrdinalIgnoreCase))
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

/// <summary>
/// Converts the text written for the attached <see cref="Splitter.CollapsibleProperty" /> property into a
/// <see cref="SplitterPanelCollapsible" />.
/// </summary>
public class SplitterPanelCollapsibleConverter : TypeConverter
{
    /// <summary>
    /// Returns whether a value of <paramref name="sourceType" /> can be converted with <see cref="ConvertFrom" />.
    /// Only strings are converted here; any other type is left to the base converter.
    /// </summary>
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
    {
        return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
    }

    /// <summary>
    /// Converts a string with <see cref="SplitterPanelCollapsible.Parse" /> and leaves every other value to the base
    /// converter.
    /// </summary>
    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        if (value is string str)
        {
            return SplitterPanelCollapsible.Parse(str);
        }

        return base.ConvertFrom(context, culture, value);
    }
}
