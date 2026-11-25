using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.VisualTree;

namespace AtomUI.Controls.Commons;

internal class DotBadgeIndicator : Control
{
    internal static readonly StyledProperty<IBrush?> BadgeDotColorProperty =
        AvaloniaProperty.Register<DotBadgeIndicator, IBrush?>(
            nameof(BadgeDotColor));

    internal static readonly StyledProperty<IBrush?> BadgeShadowColorProperty =
        AvaloniaProperty.Register<DotBadgeIndicator, IBrush?>(
            nameof(BadgeShadowColor));

    internal static readonly StyledProperty<double> BadgeShadowSizeProperty =
        AvaloniaProperty.Register<DotBadgeIndicator, double>(
            nameof(BadgeShadowSize));

    internal static readonly StyledProperty<BadgeShadowTokenResourceKeys?> ShadowTokenResourceKeysProperty =
        AvaloniaProperty.Register<DotBadgeIndicator, BadgeShadowTokenResourceKeys?>(nameof(ShadowTokenResourceKeys));

    internal IBrush? BadgeDotColor
    {
        get => GetValue(BadgeDotColorProperty);
        set => SetValue(BadgeDotColorProperty, value);
    }

    internal IBrush? BadgeShadowColor
    {
        get => GetValue(BadgeShadowColorProperty);
        set => SetValue(BadgeShadowColorProperty, value);
    }

    public double BadgeShadowSize
    {
        get => GetValue(BadgeShadowSizeProperty);
        set => SetValue(BadgeShadowSizeProperty, value);
    }

    internal BadgeShadowTokenResourceKeys? ShadowTokenResourceKeys
    {
        get => GetValue(ShadowTokenResourceKeysProperty);
        set => SetValue(ShadowTokenResourceKeysProperty, value);
    }

    private BoxShadows _boxShadows;
    private OwnerTokenSet? _shadowTokens;

    static DotBadgeIndicator()
    {
        AffectsRender<DotBadgeIndicator>(BadgeDotColorProperty, BadgeShadowSizeProperty, BadgeShadowColorProperty);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        UpdateShadowTokenSubscription();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _shadowTokens?.Dispose();
        _shadowTokens = null;
        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ShadowTokenResourceKeysProperty && this.IsAttachedToVisualTree())
        {
            UpdateShadowTokenSubscription();
        }
        else if (change.Property == BadgeShadowSizeProperty ||
            change.Property == BadgeShadowColorProperty)
        {
            BuildBoxShadow();
        }
    }

    public override void Render(DrawingContext context)
    {
        context.DrawRectangle(BadgeDotColor, null, new Rect(Bounds.Size), Bounds.Width / 2, Bounds.Height / 2,
            _boxShadows);
    }

    private void UpdateShadowTokenSubscription()
    {
        _shadowTokens?.Dispose();
        _shadowTokens = ShadowTokenResourceKeys?.Observe(this, BuildBoxShadow);
        BuildBoxShadow();
    }

    private void BuildBoxShadow()
    {
        if (BadgeShadowTokenResourceKeys.TryRead(this, _shadowTokens, BadgeShadowColorProperty, BadgeShadowSizeProperty, out var shadow) &&
            _boxShadows != shadow)
        {
            _boxShadows = shadow;
            InvalidateVisual();
        }
    }
}
