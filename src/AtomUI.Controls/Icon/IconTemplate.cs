using Avalonia.Controls;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Metadata;
using Avalonia.Styling;

namespace AtomUI.Controls;

/// <summary>
/// An <see cref="IIconTemplate" /> whose icon is declared as the XAML content of the template and built by
/// <see cref="Build" />.
/// </summary>
[ControlTemplateScope]
public class IconTemplate : IIconTemplate
{
    /// <summary>
    /// The icon declared as the XAML content of this template.
    /// </summary>
    /// <value>
    /// Assigned in XAML as the content of the template and expected to be a <see cref="PathIcon" />.
    /// <see langword="null" /> makes <see cref="Build" /> return <see langword="null" />.
    /// </value>
    [Content]
    [TemplateContent]
    public object? Content { get; set; }
    /// <summary>
    /// Builds a new instance of the icon declared by <see cref="Content" /> on every call.
    /// </summary>
    /// <returns>
    /// The new icon, or <see langword="null" /> when <see cref="Content" /> is <see langword="null" />.
    /// </returns>
    /// <exception cref="InvalidCastException">
    /// <see cref="Content" /> declares an object that is not a <see cref="PathIcon" />.
    /// </exception>
    public PathIcon? Build() => (PathIcon?)TemplateContent.Load(Content)?.Result;
    object? ITemplate.Build() => Build();
}