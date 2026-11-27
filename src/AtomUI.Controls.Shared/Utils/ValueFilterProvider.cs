using Avalonia.Markup.Xaml;

namespace AtomUI.Controls.Utils;

/// <summary>
/// XAML markup extension that supplies a built-in <see cref="IValueFilter" />, for example
/// <c>Filter="{atom:ValueFilterProvider Contains}"</c>.
/// </summary>
public class ValueFilterProvider : MarkupExtension
{
    /// <summary>
    /// Gets or sets the built-in matching rule of the filter that is provided. The default is
    /// <see cref="ValueFilterMode.None" />.
    /// </summary>
    /// <remarks>
    /// <see cref="ValueFilterMode.Custom" /> cannot be used, because the provider creates built-in filters only.
    /// </remarks>
    public ValueFilterMode Mode { get; set; }
    
    private Dictionary<ValueFilterMode, IValueFilter> _filters = new();
    
    public ValueFilterProvider()
    {}
    
    /// <summary>
    /// Initializes a provider that supplies the filter of the given matching rule.
    /// </summary>
    /// <param name="mode">
    /// The built-in matching rule of the filter.
    /// </param>
    public ValueFilterProvider(ValueFilterMode mode)
    {
        Mode = mode;
    }

    /// <summary>
    /// Provides the built-in filter selected by <see cref="Mode" />.
    /// </summary>
    /// <returns>
    /// The <see cref="IValueFilter" /> created by <see cref="ValueFilterFactory.BuildFilter" /> and cached in the
    /// provider. <see cref="ValueFilterMode.None" /> gives the <see cref="ValueFilterMode.Contains" /> filter.
    /// </returns>
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