using Avalonia.Controls;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Metadata;
using Avalonia.Styling;

namespace AtomUI.Controls;

[ControlTemplateScope]
public class IconTemplate : IIconTemplate
{
    [Content]
    [TemplateContent]
    public object? Content { get; set; }
    public PathIcon? Build() => (PathIcon?)TemplateContent.Load(Content)?.Result;
    object? ITemplate.Build() => Build();
}