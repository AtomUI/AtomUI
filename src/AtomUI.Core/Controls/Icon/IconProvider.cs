using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace AtomUI.Controls;

/// <summary>
/// Base class of markup extensions that create an <see cref="Icon" /> for a value of an icon kind enumeration.
/// </summary>
/// <typeparam name="TIconKind">
/// The enumeration that names the icons of one icon package.
/// </typeparam>
/// <remarks>
/// An icon package derives from it and maps each kind to the type that draws the icon through
/// <see cref="GetTypeForKind" />.
/// </remarks>
public abstract class IconProvider<TIconKind> : MarkupExtension
    where TIconKind : Enum
{
    private TIconKind? _kind;
    
    /// <summary>
    /// The kind of icon that <see cref="ProvideValue" /> creates.
    /// </summary>
    /// <remarks>
    /// Assigning a value different from the current one calls <see cref="NotifyKindChanged" />; assigning the same
    /// value again does nothing. <see cref="ProvideValue" /> asserts in debug builds that it is not
    /// <see langword="null" />.
    /// </remarks>
    public TIconKind? Kind
    {
        get => _kind;
        set
        {
            if (!EqualityComparer<TIconKind?>.Default.Equals(_kind, value))
            {
                _kind = value;
                NotifyKindChanged();
            }
        }
    }
    
    /// <summary>
    /// The brush that paints the outline of the created icon.
    /// When <see langword="null" />, the brush is not assigned and the created icon keeps the brush of its theme.
    /// </summary>
    public IBrush? StrokeBrush { get; set; }
    /// <summary>
    /// The brush that paints the filled shapes of the created icon.
    /// When <see langword="null" />, the brush is not assigned and the created icon keeps the brush of its theme.
    /// </summary>
    public IBrush? FillBrush { get; set; }
    /// <summary>
    /// The brush that paints the outline of the secondary layer of the created icon.
    /// Only a two-tone glyph has a secondary layer.
    /// When <see langword="null" />, the brush is not assigned and the created icon keeps the brush of its theme.
    /// </summary>
    public IBrush? SecondaryStrokeBrush { get; set; }
    /// <summary>
    /// The brush that paints the interior of the secondary layer of the created icon.
    /// Only a two-tone glyph has a secondary layer.
    /// When <see langword="null" />, the brush is not assigned and the created icon keeps the brush of its theme.
    /// </summary>
    public IBrush? SecondaryFillBrush { get; set; }

    /// <summary>
    /// The width of the created icon.
    /// When NaN, the width is not assigned and the created icon keeps the width of its theme, the IconSize token.
    /// </summary>
    public double Width { get; set; } = double.NaN;
    /// <summary>
    /// The height of the created icon.
    /// When NaN, the height is not assigned and the created icon keeps the height of its theme, the IconSize token.
    /// </summary>
    public double Height { get; set; } = double.NaN;
    /// <summary>
    /// The loading animation that the created icon plays.
    /// When <see langword="null" />, the created icon keeps the default <see cref="IconAnimation.None" /> and stays
    /// still.
    /// </summary>
    public IconAnimation? Animation { get; set; }

    public IconProvider()
    {
    }

    /// <summary>
    /// Creates a provider that produces the icon for <paramref name="kind" />.
    /// </summary>
    /// <param name="kind">
    /// The icon kind that <see cref="Kind" /> is set to.
    /// </param>
    public IconProvider(TIconKind kind)
    {
        Kind = kind;
    }
    
    /// <summary>
    /// Called by the <see cref="Kind" /> setter when the new value differs from the current one.
    /// </summary>
    /// <remarks>
    /// The base method does nothing, so an override need not call it.
    /// </remarks>
    protected virtual void NotifyKindChanged()
    {
    }

    /// <summary>
    /// Creates the icon for <see cref="Kind" /> and returns it as the value of the markup extension.
    /// </summary>
    /// <param name="serviceProvider">
    /// Not read.
    /// </param>
    /// <returns>
    /// The created icon. It carries the brushes, the size and the animation that are set on this provider.
    /// </returns>
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        Debug.Assert(Kind != null);
        var icon = GetIcon(Kind);

        if (Animation != null)
        {
            icon.SetCurrentValue(Icon.LoadingAnimationProperty, Animation);
        }
        
        if (StrokeBrush != null)
        {
            icon.StrokeBrush = StrokeBrush;
        }
        
        if (FillBrush != null)
        {
            icon.FillBrush = FillBrush;
        }
        
        if (SecondaryFillBrush != null)
        {
            icon.SecondaryFillBrush = SecondaryFillBrush;
        }
        
        if (SecondaryStrokeBrush != null)
        {
            icon.SecondaryStrokeBrush = SecondaryStrokeBrush;
        }
        
        if (!double.IsNaN(Width))
        {
            icon.Width = Width;
        }

        if (!double.IsNaN(Height))
        {
            icon.Height = Height;
        }

        return icon;
    }

    /// <summary>
    /// Returns the icon that represents <paramref name="kind" />, created by the creator cached for that kind.
    /// </summary>
    /// <param name="kind">
    /// The kind of icon to create.
    /// </param>
    /// <returns>
    /// The created icon.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the icon cannot be created; the original error is kept as the inner exception.
    /// </exception>
    /// <remarks>
    /// The creator for a kind is built once, with <see cref="CreateFactory" /> and <see cref="GetTypeForKind" />,
    /// and reused from then on. With the default creator every call creates a new icon instance.
    /// </remarks>
    protected virtual Icon GetIcon(TIconKind kind)
    {
        try
        {
            var enumType = typeof(TIconKind);
            var creator = IconProviderCache.GetOrAddCreator(
                enumType,
                kind,
                value => CreateFactory(GetTypeForKind((TIconKind)value)));
            
            var icon = creator();
            Debug.Assert(icon != null);
            return icon;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Create icon {kind} failed", ex);
        }
    }
    
    /// <summary>
    /// Returns the type of the icon that represents <paramref name="kind" />.
    /// </summary>
    /// <param name="kind">
    /// The kind whose icon type is requested.
    /// </param>
    /// <returns>
    /// A type with a public parameterless constructor.
    /// </returns>
    /// <remarks>
    /// <see cref="GetIcon" /> calls it the first time an icon of <paramref name="kind" /> is requested. The default
    /// <see cref="CreateFactory" /> looks the constructor up by reflection, so a type without one makes
    /// <see cref="GetIcon" /> throw.
    /// </remarks>
    [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
    protected abstract Type GetTypeForKind(TIconKind kind);
    
    /// <summary>
    /// Creates the delegate that produces the icons for a kind.
    /// </summary>
    /// <param name="type">
    /// The icon type that <see cref="GetTypeForKind" /> returned for the kind.
    /// </param>
    /// <returns>
    /// A delegate that returns a new instance of <paramref name="type" /> on every call.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <paramref name="type" /> has no public parameterless constructor.
    /// </exception>
    /// <remarks>
    /// <see cref="GetIcon" /> caches the delegate and asserts in debug builds that the icon it returns is not
    /// <see langword="null" />.
    /// </remarks>
    protected virtual Func<Icon> CreateFactory(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
        Type type)
    {
        var constructor = type.GetConstructor(Type.EmptyTypes);
        if (constructor == null)
        {
            throw new InvalidOperationException(
                $"No parameterless constructor found for {type.Name}");
        }

        return () => (Icon)constructor.Invoke(null);
    }
    
    /// <summary>
    /// Clears the icon creators cached for the <typeparamref name="TIconKind" /> enumeration of this provider.
    /// </summary>
    /// <remarks>
    /// After the call, the creator of each kind is built again the next time an icon of that kind is requested.
    /// </remarks>
    public static void ClearCache()
    {
        IconProviderCache.ClearCache(typeof(TIconKind));
    }
    
    /// <summary>
    /// Clears the icon creators cached for every icon kind enumeration.
    /// </summary>
    public static void ClearAllCache()
    {
        IconProviderCache.ClearAllCache();
    }
}
