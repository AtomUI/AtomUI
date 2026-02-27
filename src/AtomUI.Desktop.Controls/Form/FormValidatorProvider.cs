using System.Collections;
using Avalonia.Markup.Xaml;
using Avalonia.Metadata;

namespace AtomUI.Desktop.Controls;

public class FormValidatorProvider : MarkupExtension
{
    private readonly List<IFormValidator> _items = new();
    
    [Content]
    public IList Items => _items;
    
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return _items;
    }
}