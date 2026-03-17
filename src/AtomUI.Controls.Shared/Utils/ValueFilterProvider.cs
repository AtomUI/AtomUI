using Avalonia.Markup.Xaml;

namespace AtomUI.Controls.Utils;

public class ValueFilterProvider : MarkupExtension
{
    public ValueFilterMode Mode { get; set; }
    
    private Dictionary<ValueFilterMode, IValueFilter> _filters = new();
    
    public ValueFilterProvider()
    {}
    
    public ValueFilterProvider(ValueFilterMode mode)
    {
        Mode = mode;
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var mode = Mode == ValueFilterMode.None ? ValueFilterMode.Contains : Mode;
        if (_filters.TryGetValue(Mode, out var filter))
        {
            return filter;
        }

        var newFilter = ValueFilterFactory.BuildFilter(mode)!;
        _filters.Add(mode, newFilter);
        return newFilter;
    }
}