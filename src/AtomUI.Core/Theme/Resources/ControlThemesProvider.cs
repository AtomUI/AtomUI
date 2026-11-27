using Avalonia.Controls;
using Avalonia.Metadata;

namespace AtomUI.Theme.Resources;

/// <summary>
/// The XAML-ready implementation of <see cref="IControlThemesProvider" />: the child elements of the provider
/// are the entries of <see cref="ControlThemes" />.
/// </summary>
public class ControlThemesProvider : IControlThemesProvider
{
    private ResourceDictionary _resourceDictionary;
    
    /// <summary>
    /// The id of the theme package, an empty string until a derived provider assigns it through the protected
    /// setter.
    /// </summary>
    public string Id { get; protected set; } = string.Empty;

    public ControlThemesProvider()
    {
        _resourceDictionary = new ResourceDictionary();
    }
    
    /// <summary>
    /// The resource providers that hold the control themes. Child elements of the provider in XAML are added to
    /// this list.
    /// </summary>
    [Content]
    public IList<IResourceProvider> ControlThemes => _resourceDictionary.MergedDictionaries;
}
