using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Data;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Reads a Tab Token from the instance whose control theme supplied the resource scope.
/// </summary>
internal sealed class TabTokenResourceBindingExtension(string name) : MarkupExtension
{
    // Generated items panels and shared part templates have different templated parents.
    // Inherit the resource source from the concrete Tab theme over the same path as its parts.
    public static readonly AttachedProperty<StyledElement?> OwnerProperty =
        AvaloniaProperty.RegisterAttached<StyledElement, StyledElement?>(
            "Owner", typeof(TabTokenResourceBindingExtension), inherits: true);

    public static StyledElement? GetOwner(StyledElement element) => element.GetValue(OwnerProperty);
    public static void SetOwner(StyledElement element, StyledElement? owner) => element.SetValue(OwnerProperty, owner);

    // Reuse one resource listener per active key on a host across all its parts. The observable
    // stops listening when the last part unbinds, and the cache does not keep the host alive.
    private static readonly ConditionalWeakTable<StyledElement, Dictionary<object, TokenResourceObservable>> s_observables = new();
    private static readonly ClrPropertyInfo s_resourceProperty = new("TabTokenResource", null, null, typeof(object));

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var resource = GetResource(name);
        var path = new CompiledBindingPathBuilder()
                   .Self()
                   .Property(s_resourceProperty, (reference, _) => new ResourceAccessor(reference, resource))
                   .Build();
        return new CompiledBinding(path) { Mode = BindingMode.OneWay };
    }

    private static Resource GetResource(string name) => name switch
    {
        "InkBarColor" => new(
            TabControlTokenKind.InkBarColor, TabStripTokenKind.InkBarColor,
            null, null, null),
        "HorizontalItemGutter" => new(
            TabControlTokenKind.HorizontalItemGutter, TabStripTokenKind.HorizontalItemGutter,
            null, null, 0d),
        "VerticalItemGutter" => new(
            TabControlTokenKind.VerticalItemGutter, TabStripTokenKind.VerticalItemGutter,
            null, null, 0d),
        "CardGutter" => new(
            null, null,
            CardTabControlTokenKind.CardGutter, CardTabStripTokenKind.CardGutter, 0d),
        "AddTabButtonMarginHorizontal" => new(
            null, null,
            CardTabControlTokenKind.AddTabButtonMarginHorizontal, CardTabStripTokenKind.AddTabButtonMarginHorizontal, default(Thickness)),
        "AddTabButtonMarginVertical" => new(
            null, null,
            CardTabControlTokenKind.AddTabButtonMarginVertical, CardTabStripTokenKind.AddTabButtonMarginVertical, default(Thickness)),
        "TitleFontSize" => new(
            TabControlTokenKind.TitleFontSize, TabStripTokenKind.TitleFontSize,
            CardTabControlTokenKind.TitleFontSize, CardTabStripTokenKind.TitleFontSize, 0d,
            Small: new(
                TabControlTokenKind.TitleFontSizeSM, TabStripTokenKind.TitleFontSizeSM,
                CardTabControlTokenKind.TitleFontSizeSM, CardTabStripTokenKind.TitleFontSizeSM, 0d),
            Large: new(
                TabControlTokenKind.TitleFontSizeLG, TabStripTokenKind.TitleFontSizeLG,
                CardTabControlTokenKind.TitleFontSizeLG, CardTabStripTokenKind.TitleFontSizeLG, 0d)),
        "TitleFontWeight" => new(
            TabControlTokenKind.TitleFontWeight, TabStripTokenKind.TitleFontWeight,
            CardTabControlTokenKind.TitleFontWeight, CardTabStripTokenKind.TitleFontWeight, FontWeight.Normal),
        "HorizontalItemMargin" => new(
            TabControlTokenKind.HorizontalItemMargin, TabStripTokenKind.HorizontalItemMargin,
            CardTabControlTokenKind.HorizontalItemMargin, CardTabStripTokenKind.HorizontalItemMargin, default(Thickness)),
        "ItemColor" => new(
            TabControlTokenKind.ItemColor, TabStripTokenKind.ItemColor,
            CardTabControlTokenKind.ItemColor, CardTabStripTokenKind.ItemColor, null),
        "ItemHoverColor" => new(
            TabControlTokenKind.ItemHoverColor, TabStripTokenKind.ItemHoverColor,
            CardTabControlTokenKind.ItemHoverColor, CardTabStripTokenKind.ItemHoverColor, null),
        "ItemSelectedColor" => new(
            TabControlTokenKind.ItemSelectedColor, TabStripTokenKind.ItemSelectedColor,
            CardTabControlTokenKind.ItemSelectedColor, CardTabStripTokenKind.ItemSelectedColor, null),
        "ItemIconMargin" => new(
            TabControlTokenKind.ItemIconMargin, TabStripTokenKind.ItemIconMargin,
            CardTabControlTokenKind.ItemIconMargin, CardTabStripTokenKind.ItemIconMargin, default(Thickness)),
        "CloseIconMargin" => new(
            TabControlTokenKind.CloseIconMargin, TabStripTokenKind.CloseIconMargin,
            CardTabControlTokenKind.CloseIconMargin, CardTabStripTokenKind.CloseIconMargin, default(Thickness)),
        "MenuIndicatorPaddingHorizontal" => new(
            TabControlTokenKind.MenuIndicatorPaddingHorizontal, TabStripTokenKind.MenuIndicatorPaddingHorizontal,
            CardTabControlTokenKind.MenuIndicatorPaddingHorizontal, CardTabStripTokenKind.MenuIndicatorPaddingHorizontal, default(Thickness)),
        "MenuIndicatorPaddingVertical" => new(
            TabControlTokenKind.MenuIndicatorPaddingVertical, TabStripTokenKind.MenuIndicatorPaddingVertical,
            CardTabControlTokenKind.MenuIndicatorPaddingVertical, CardTabStripTokenKind.MenuIndicatorPaddingVertical, default(Thickness)),
        "BoxShadowTabsOverflowLeft" => new(
            TabControlTokenKind.BoxShadowTabsOverflowLeft, TabStripTokenKind.BoxShadowTabsOverflowLeft,
            CardTabControlTokenKind.BoxShadowTabsOverflowLeft, CardTabStripTokenKind.BoxShadowTabsOverflowLeft, default(BoxShadows)),
        "BoxShadowTabsOverflowRight" => new(
            TabControlTokenKind.BoxShadowTabsOverflowRight, TabStripTokenKind.BoxShadowTabsOverflowRight,
            CardTabControlTokenKind.BoxShadowTabsOverflowRight, CardTabStripTokenKind.BoxShadowTabsOverflowRight, default(BoxShadows)),
        "BoxShadowTabsOverflowTop" => new(
            TabControlTokenKind.BoxShadowTabsOverflowTop, TabStripTokenKind.BoxShadowTabsOverflowTop,
            CardTabControlTokenKind.BoxShadowTabsOverflowTop, CardTabStripTokenKind.BoxShadowTabsOverflowTop, default(BoxShadows)),
        "BoxShadowTabsOverflowBottom" => new(
            TabControlTokenKind.BoxShadowTabsOverflowBottom, TabStripTokenKind.BoxShadowTabsOverflowBottom,
            CardTabControlTokenKind.BoxShadowTabsOverflowBottom, CardTabStripTokenKind.BoxShadowTabsOverflowBottom, default(BoxShadows)),
        "HorizontalItemPadding" => new(
            TabControlTokenKind.HorizontalItemPadding, TabStripTokenKind.HorizontalItemPadding,
            null, null, default(Thickness),
            Small: new(
                TabControlTokenKind.HorizontalItemPaddingSM, TabStripTokenKind.HorizontalItemPaddingSM,
                null, null, default(Thickness)),
            Large: new(
                TabControlTokenKind.HorizontalItemPaddingLG, TabStripTokenKind.HorizontalItemPaddingLG,
                null, null, default(Thickness))),
        "CardPadding" => new(
            null, null,
            CardTabControlTokenKind.CardPadding, CardTabStripTokenKind.CardPadding, default(Thickness),
            Small: new(
                null, null,
                CardTabControlTokenKind.CardPaddingSM, CardTabStripTokenKind.CardPaddingSM, default(Thickness)),
            Large: new(
                null, null,
                CardTabControlTokenKind.CardPaddingLG, CardTabStripTokenKind.CardPaddingLG, default(Thickness))),
        "CardBg" => new(
            null, null,
            CardTabControlTokenKind.CardBg, CardTabStripTokenKind.CardBg, null),
        "VerticalItemPadding" => new(
            null, null,
            CardTabControlTokenKind.VerticalItemPadding, CardTabStripTokenKind.VerticalItemPadding, default(Thickness)),
        _ => throw new ArgumentException($"Unknown Tab Token: {name}", nameof(name))
    };

    private sealed record Resource(
        object? ControlKey, object? StripKey, object? CardControlKey, object? CardStripKey, object? Fallback,
        Resource? Small = null, Resource? Large = null)
    {
        internal object? GetKey(StyledElement owner) => owner switch
        {
            TabControl => ControlKey,
            TabStrip => StripKey,
            CardTabControl => CardControlKey,
            CardTabStrip => CardStripKey,
            _ => null
        };

        internal Resource ForSize(SizeType size) => size switch
        {
            SizeType.Small => Small ?? this,
            SizeType.Large => Large ?? this,
            _ => this
        };
    }

    private static IObservable<object?> Observe(StyledElement? owner, Resource resource)
    {
        if (owner is null)
        {
            return Observable.Return(resource.Fallback);
        }
        if (resource.Small is null)
        {
            return ObserveKey(owner, resource);
        }

        var sizes = owner switch
        {
            BaseTabControl tabs => tabs.GetObservable(BaseTabControl.SizeTypeProperty),
            BaseTabStrip strip => strip.GetObservable(BaseTabStrip.SizeTypeProperty),
            _ => Observable.Return(SizeType.Middle)
        };
        return sizes.Select(size => ObserveKey(owner, resource.ForSize(size))).Switch();
    }

    private static IObservable<object?> ObserveKey(StyledElement owner, Resource resource)
    {
        if (resource.GetKey(owner) is not { } key)
        {
            return Observable.Return(resource.Fallback);
        }

        var observables = s_observables.GetOrCreateValue(owner);
        if (!observables.TryGetValue(key, out var source))
        {
            source = new TokenResourceObservable(owner, key, null);
            observables.Add(key, source);
        }
        return source.Select(value => ReferenceEquals(value, AvaloniaProperty.UnsetValue) ? resource.Fallback : value);
    }

    private sealed class ResourceAccessor(
        WeakReference<object?> reference, Resource resource) : IPropertyAccessor
    {
        private SingleAssignmentDisposable? _subscription;

        public Type PropertyType => typeof(object);
        public object? Value { get; private set; } = AvaloniaProperty.UnsetValue;

        public bool SetValue(object? value, BindingPriority priority) => false;

        public void Subscribe(Action<object?> listener)
        {
            var subscription = new SingleAssignmentDisposable();
            _subscription = subscription;
            var values = reference.TryGetTarget(out var source) && source is StyledElement target
                ? target.GetObservable(OwnerProperty).Select(owner => Observe(owner, resource)).Switch()
                : Observable.Return(resource.Fallback);
            subscription.Disposable = values.DistinctUntilChanged().Subscribe(value =>
            {
                Value = value;
                listener(value);
            });
        }

        public void Unsubscribe()
        {
            _subscription?.Dispose();
            _subscription = null;
        }

        public void Dispose() => Unsubscribe();
    }
}
