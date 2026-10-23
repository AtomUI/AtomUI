using Avalonia.Controls;
using Avalonia.Styling;
using Avalonia.Threading;
using AtomUI.Theme.Branding;

namespace AtomUI.Theme.Styling;

internal sealed class ThemeStyleAsset
{
    private readonly Func<Style> _factory;
    private readonly ThemeStyleCapabilities _capabilities;

    internal ThemeStyleAsset(
        string id,
        string revision,
        Type controlType,
        Func<Style> factory,
        ThemeStyleCapabilities capabilities = ThemeStyleCapabilities.QualifiedStyles)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(revision);
        ArgumentNullException.ThrowIfNull(controlType);
        ArgumentNullException.ThrowIfNull(factory);
        if (!typeof(Control).IsAssignableFrom(controlType) || controlType.ContainsGenericParameters)
        {
            throw new ArgumentException("A style asset requires a closed Control type.", nameof(controlType));
        }

        Id          = id;
        Revision    = revision;
        ControlType = controlType;
        _factory    = factory;
        _capabilities = capabilities;
    }

    internal string Id { get; }
    internal string Revision { get; }
    internal Type ControlType { get; }

    internal Style Create(ThemeStyleActivation activation)
    {
        Dispatcher.UIThread.VerifyAccess();
        ArgumentNullException.ThrowIfNull(activation);
        var style = _factory() ??
                    throw new InvalidOperationException($"Style asset '{Id}' returned no style.");
        var selector = style.Selector.OfType(ControlType)
                                     .PropertyEquals(ThemeStyleQualification.ActiveProperty, activation);
        ThemeStyleGraph.Prepare(style, selector, _capabilities);
        return style;
    }
}
