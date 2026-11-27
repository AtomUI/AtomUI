using Avalonia;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Styling;

namespace AtomUI.Localization;

/// <summary>
/// Base class of the generated markup extensions that read a language resource in XAML by the value of a
/// language resource enum. The generator derives one class per language catalog from it.
/// </summary>
/// <typeparam name="TResourceKind">
/// The language resource enum type of the catalog.
/// </typeparam>
public abstract class LanguageResourceExtension<TResourceKind> : MarkupExtension
    where TResourceKind : struct, Enum
{
    protected LanguageResourceExtension()
    {
    }

    /// <summary>
    /// Creates an extension that reads the language resource of <paramref name="kind" />.
    /// </summary>
    /// <param name="kind">
    /// The language resource value to read.
    /// </param>
    protected LanguageResourceExtension(TResourceKind kind)
    {
        Kind = kind;
    }

    /// <summary>
    /// The language resource value to read. It is <see langword="null" /> until it is assigned, and
    /// <see cref="ProvideValue" /> throws while it is.
    /// </summary>
    public TResourceKind? Kind { get; set; }

    /// <summary>
    /// Returns a <see cref="DynamicResourceExtension" /> when the target property belongs to an
    /// <see cref="AvaloniaObject" /> or to a style <see cref="SetterBase" />, so that the value follows language
    /// changes. For any other target it returns the resource value, resolved once, which does not follow later
    /// language changes. A resource whose value is <see langword="null" /> yields an empty string.
    /// </summary>
    /// <param name="serviceProvider">
    /// The service provider of the XAML loader. It supplies the target of the extension.
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// <see cref="Kind" /> is <see langword="null" />. For a target that takes a resolved value, also when there
    /// is no current Application or the language resource is not registered.
    /// </exception>
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        if (Kind is not { } kind)
        {
            throw new InvalidOperationException("A language resource key is required.");
        }

        var provideTarget = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;
        if (!ShouldUseStaticResourceValue(provideTarget?.TargetObject))
        {
            return new DynamicResourceExtension(kind);
        }

        var application = Application.Current ?? throw new InvalidOperationException(
            "A current Application is required to resolve a static language resource.");
        if (application.TryGetResource(kind, application.ActualThemeVariant, out var value))
        {
            return value ?? string.Empty;
        }

        throw new InvalidOperationException(
            $"Language resource '{typeof(TResourceKind).FullName}.{kind}' is not registered.");
    }

    private static bool ShouldUseStaticResourceValue(object? targetObject)
    {
        return targetObject is not null &&
               targetObject is not AvaloniaObject &&
               targetObject is not SetterBase;
    }
}
