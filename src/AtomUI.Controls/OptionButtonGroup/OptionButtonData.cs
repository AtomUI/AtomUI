using Avalonia.Controls;

namespace AtomUI.Controls;

interface IOptionButtonData : IHeadered
{
    PathIcon? Icon { get; }
    bool IsEnabled { get; }
}

/// <summary>
/// Item data that describes one option of an
/// <see cref="AtomUI.Controls.Commons.AbstractOptionButtonGroup" /> when the options are generated from
/// <see cref="ItemsControl.ItemsSource" /> instead of being declared as option buttons.
/// </summary>
public class OptionButtonData : IOptionButtonData
{
    /// <summary>
    /// Content of the option, shown as its text. With no <c>ItemTemplate</c> on the group it becomes the
    /// content of the generated option button; with a template, the template receives this whole object.
    /// </summary>
    public object? Header { get; init; }
    /// <summary>
    /// Icon shown to the left of the option text, or <see langword="null" /> for a text-only option.
    /// </summary>
    public PathIcon? Icon { get; init; }
    /// <summary>
    /// Whether the generated option button is enabled; <see langword="true" /> by default.
    /// </summary>
    public bool IsEnabled { get; init; } = true;
}