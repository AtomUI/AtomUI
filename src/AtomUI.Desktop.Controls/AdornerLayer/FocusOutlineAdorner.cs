using System.Globalization;
using AtomUI.Controls.Primitives;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme.Resources;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Styling;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Keyboard focus outline matching Ant Design <c>genFocusOutline</c>: a stroke of
/// <see cref="OutlineThickness"/> drawn <see cref="OutlineOffset"/> outside the adorned element and
/// following its corner radius.
/// </summary>
internal sealed class FocusOutlineAdorner : Control
{
    /// <summary>
    /// Set on the adorned element to draw the outline just inside its bounds instead of outside
    /// (Ant Design <c>genFocusOutline(token, -lineWidthFocus)</c>, used by card tabs).
    /// </summary>
    public static readonly AttachedProperty<bool> IsInsetProperty =
        AvaloniaProperty.RegisterAttached<FocusOutlineAdorner, Visual, bool>("IsInset");

    internal static readonly StyledProperty<bool> UseFocusOutlineTokenResourcesProperty =
        AvaloniaProperty.Register<FocusOutlineAdorner, bool>(nameof(UseFocusOutlineTokenResources));

    internal bool UseFocusOutlineTokenResources
    {
        get => GetValue(UseFocusOutlineTokenResourcesProperty);
        set => SetValue(UseFocusOutlineTokenResourcesProperty, value);
    }

    public IBrush? OutlineBrush => _outline.Brush;
    public double OutlineThickness => _outline.Thickness;
    public double OutlineOffset => _outline.Offset;

    public static bool GetIsInset(Visual element) => element.GetValue(IsInsetProperty);

    public static void SetIsInset(Visual element, bool value) => element.SetValue(IsInsetProperty, value);

    private Pen? _pen;
    private Pen? _borderPen;
    private CornerRadius _renderedCornerRadius;
    private OwnerTokenSet? _outlineTokens;
    private OwnerTokenSet? _adornerOverrides;
    private StyledElement? _outlineTokenSource;
    private (IBrush? Brush, double Thickness, double Offset, IBrush? BorderBrush, double BorderThickness) _outline;

    private static readonly object[] s_outlineKeys =
    [
        ControlTokenResourceKey.Global(AdornerLayerTokens.Identity, SharedTokenKind.ColorFocusBorder),
        ControlTokenResourceKey.Global(AdornerLayerTokens.Identity, SharedTokenKind.LineWidthFocus),
        AdornerLayerTokenKind.FocusOutlineOffset
    ];
    private static readonly object[] s_explicitOutlineKeys = s_outlineKeys.Select(ExplicitToken).ToArray();
    // The adorned element and, when it borrows its shape from its owner, the owner: shape changes that keep
    // the size (an expander header squaring its bottom corners) re-arrange nothing, so they are watched here.
    private AvaloniaObject? _shapeSource;
    private AvaloniaObject? _shapeOwner;

    private const double ClipEdgeTolerance = 0.5;

    static FocusOutlineAdorner()
    {
        AffectsRender<FocusOutlineAdorner>(AdornerLayer.AdornedElementProperty);
        IsHitTestVisibleProperty.OverrideDefaultValue<FocusOutlineAdorner>(false);
    }

    public FocusOutlineAdorner()
    {
        // The outline sits outside the adorned element, so AdornerLayer clipping, which starts at the element's
        // own bounds (TemplatedControl clips by default), would remove it; Render applies the ancestor clips.
        AdornerLayer.SetIsClipEnabled(this, false);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        SubscribeShapeSources();
        UpdateOutlineTokenSubscription();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _outlineTokens?.Dispose();
        _outlineTokens = null;
        _adornerOverrides?.Dispose();
        _adornerOverrides = null;
        _outlineTokenSource = null;
        _outline = default;
        _pen = null;
        _borderPen = null;
        base.OnDetachedFromVisualTree(e);
        UnsubscribeShapeSources();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == AdornerLayer.AdornedElementProperty)
        {
            UnsubscribeShapeSources();
            if (this.IsAttachedToVisualTree())
            {
                SubscribeShapeSources();
                UpdateOutlineTokenSubscription();
            }
        }
        else if (change.Property == ParentProperty && change.NewValue is null &&
                 this.GetVisualParent() is AdornerLayer)
        {
            // Removed from AdornerLayer.Children (Control.OnLostFocus does this): Panel drops the logical child
            // before the visual one. AdornerLayer releases its subscriptions on the adorned element and every
            // ancestor only when the adorned element changes while the adorner is still its visual child.
            AdornerLayer.SetAdornedElement(this, null);
        }
        else if (change.Property == UseFocusOutlineTokenResourcesProperty && this.IsAttachedToVisualTree())
        {
            UpdateOutlineTokenSubscription();
        }
    }

    private void UpdateOutlineTokenSubscription()
    {
        var adorned = AdornerLayer.GetAdornedElement(this);
        StyledElement? source = !UseFocusOutlineTokenResources || adorned is null
            ? null
            : adorned is Button { UsesThemeRecipe: true } button ? button : this;
        if (!ReferenceEquals(_outlineTokenSource, source))
        {
            _outlineTokens?.Dispose();
            _outlineTokens = null;
            _adornerOverrides?.Dispose();
            _adornerOverrides = null;
            _outlineTokenSource = source;
            if (source is Button buttonSource)
            {
                // The adorned control and the adorner layer are in separate resource scopes.
                // Read the button's scoped recipe and also observe explicit legacy adorner overrides.
                var owner = ButtonBehavior.OwnerOf(buttonSource);
                _outlineTokens = new OwnerTokenSet(source, 9, RefreshOutline);
                _outlineTokens.SetKind(0, SharedTokenKind.ColorFocusBorder);
                _outlineTokens.SetKind(1, SharedTokenKind.LineWidthFocus);
                _outlineTokens.SetResourceKey(2, s_explicitOutlineKeys[2]);
                _outlineTokens.SetKind(3, SharedTokenKind.ColorError);
                _outlineTokens.SetKind(4, SharedTokenKind.LineWidth);
                _outlineTokens.SetResourceKey(5, ExplicitToken(ControlTokenResourceKey.Global(owner, SharedTokenKind.ColorFocusBorder)));
                _outlineTokens.SetResourceKey(6, ExplicitToken(ControlTokenResourceKey.Global(owner, SharedTokenKind.LineWidthFocus)));
                _outlineTokens.SetResourceKey(7, s_explicitOutlineKeys[0]);
                _outlineTokens.SetResourceKey(8, s_explicitOutlineKeys[1]);

                _adornerOverrides = new OwnerTokenSet(this, s_explicitOutlineKeys.Length, RefreshOutline);
                for (var index = 0; index < s_explicitOutlineKeys.Length; ++index)
                {
                    _adornerOverrides.SetResourceKey(index, s_explicitOutlineKeys[index]);
                }
            }
            else if (source is not null)
            {
                _outlineTokens = new OwnerTokenSet(source, s_outlineKeys.Length, RefreshOutline);
                for (var index = 0; index < s_outlineKeys.Length; ++index)
                {
                    _outlineTokens.SetResourceKey(index, s_outlineKeys[index]);
                }
            }
        }
        RefreshOutline();
    }

    private void RefreshOutline()
    {
        var brush = ReadBrush(_outlineTokens?.GetValue(0), _outline.Brush);
        var thickness = ReadNumber(_outlineTokens?.GetValue(1), _outline.Thickness);
        var offsetValue = _outlineTokens?.GetValue(2);
        IBrush? borderBrush = null;
        double borderThickness = 0;
        if (_outlineTokenSource is Button button)
        {
            borderThickness = ReadNumber(_outlineTokens!.GetValue(4), _outline.BorderThickness);
            if (ReferenceEquals(offsetValue, AvaloniaProperty.UnsetValue))
            {
                offsetValue = _adornerOverrides!.GetValue(2);
            }
            if (TryGetOutlineOverride(6, 8, 1, out var thicknessOverride))
            {
                thickness = ReadNumber(thicknessOverride, _outline.Thickness);
            }
            if (TryGetOutlineOverride(5, 7, 0, out var brushOverride))
            {
                brush = borderBrush = ReadBrush(brushOverride, _outline.Brush);
            }
            else if (button is { EffectiveColor: ButtonColor.Danger, EffectiveVariant: ButtonVariant.Solid })
            {
                var error = ReadBrush(_outlineTokens.GetValue(3), _outline.BorderBrush);
                borderBrush = WithAlpha(error, 102);
                brush = WithAlpha(error, button.ActualThemeVariant == ThemeVariant.Dark ? (byte)102 : (byte)51);
            }
            else
            {
                borderBrush = WithAlpha(brush, byte.MaxValue);
            }
        }

        var offset = ReadNumber(offsetValue, _outline.Offset);
        var paint = (brush, thickness, offset, borderBrush, borderThickness);
        if (!_outline.Equals(paint))
        {
            _outline = paint;
            InvalidateVisual();
        }
    }

    private static object ExplicitToken(object key) =>
        new ThemeTokenResourceProvider.ExplicitTokenLookup(key, DefaultValue: AvaloniaProperty.UnsetValue);

    private bool TryGetOutlineOverride(int buttonSlot, int scopedAdornerSlot, int adornerSlot, out object? value)
    {
        value = _outlineTokens!.GetValue(buttonSlot);
        if (ReferenceEquals(value, AvaloniaProperty.UnsetValue))
        {
            value = _outlineTokens.GetValue(scopedAdornerSlot);
        }
        if (ReferenceEquals(value, AvaloniaProperty.UnsetValue))
        {
            value = _adornerOverrides!.GetValue(adornerSlot);
        }
        return !ReferenceEquals(value, AvaloniaProperty.UnsetValue);
    }

    private static IBrush? ReadBrush(object? value, IBrush? previous) => value switch
    {
        IBrush brush => brush,
        Color color => new ImmutableSolidColorBrush(color),
        string text when Color.TryParse(text, out var color) => new ImmutableSolidColorBrush(color),
        _ when ReferenceEquals(value, BindingOperations.DoNothing) => previous,
        _ => null
    };

    private static IBrush? WithAlpha(IBrush? brush, byte alpha) => brush is ISolidColorBrush solid
        ? new ImmutableSolidColorBrush(Color.FromArgb(alpha, solid.Color.R, solid.Color.G, solid.Color.B))
        : brush;

    private static double ReadNumber(object? value, double previous)
    {
        if (value is double number)
        {
            return number;
        }
        if (ReferenceEquals(value, BindingOperations.DoNothing))
        {
            return previous;
        }
        try
        {
            return value is IConvertible convertible ? convertible.ToDouble(CultureInfo.CurrentCulture) : 0;
        }
        catch (Exception error) when (error is FormatException or InvalidCastException or OverflowException)
        {
            return 0;
        }
    }

    public override void Render(DrawingContext context)
    {
        var adorned   = AdornerLayer.GetAdornedElement(this);
        if (adorned is null)
        {
            return;
        }

        var pen = GetPen(ref _pen, OutlineBrush, OutlineThickness);
        var borderPen = GetPen(ref _borderPen, _outline.BorderBrush, _outline.BorderThickness);
        if (pen is null && borderPen is null)
        {
            return;
        }

        // The pen is centered on the path, so the path runs half a stroke further out than the offset.
        var thickness    = pen?.Thickness ?? 0;
        var offset       = GetIsInset(adorned) ? -thickness : OutlineOffset;
        var inflate      = offset + thickness / 2;
        var cornerRadius = ResolveCornerRadius(adorned);
        var elementRect  = new Rect(Bounds.Size);
        var rect         = elementRect.Inflate(inflate);
        _renderedCornerRadius = cornerRadius;
        var clip = CalculateAncestorClip(adorned, elementRect, rect.Inflate(thickness / 2));
        if (clip is { Width: <= 0 } or { Height: <= 0 })
        {
            return;
        }

        using var clipState = clip is { } clipRect ? context.PushClip(clipRect) : default(DrawingContext.PushedState?);
        if (pen is not null)
        {
            DrawStroke(context, pen, rect, cornerRadius, inflate);
        }

        if (borderPen is not null)
        {
            var inset = -borderPen.Thickness / 2;
            DrawStroke(context, borderPen, elementRect.Inflate(inset), cornerRadius, inset);
        }
    }

    private static Pen? GetPen(ref Pen? cache, IBrush? brush, double thickness)
    {
        if (brush is null || thickness <= 0)
        {
            return null;
        }

        if (cache is null || !ReferenceEquals(cache.Brush, brush) || !MathUtils.AreClose(cache.Thickness, thickness))
        {
            cache = new Pen(brush, thickness);
        }

        return cache;
    }

    private static void DrawStroke(DrawingContext context, Pen pen, Rect rect, CornerRadius cornerRadius, double inflate)
    {
        if (rect.Width <= 0 || rect.Height <= 0)
        {
            return;
        }

        var maxRadius = Math.Min(rect.Width, rect.Height) / 2;
        context.DrawRectangle(null, pen, new RoundedRect(rect,
            Grow(cornerRadius.TopLeft, inflate, maxRadius),
            Grow(cornerRadius.TopRight, inflate, maxRadius),
            Grow(cornerRadius.BottomRight, inflate, maxRadius),
            Grow(cornerRadius.BottomLeft, inflate, maxRadius)));
    }

    // The clips of the adorned element's ancestors in its own coordinates (what AdornerLayer clipping applies,
    // minus the element's own clip), except that on a side where an ancestor does not cut the element the
    // outline may pass that ancestor's edge: wrappers that hug the element keep the outline, while a viewport
    // the element is scrolled out of hides it. Evaluated on each render, which AdornerLayer triggers whenever
    // the element or an ancestor moves or resizes (it re-arranges the adorner and replaces its transform).
    private static Rect? CalculateAncestorClip(Visual adorned, Rect element, Rect outline)
    {
        Rect? result = null;
        for (var ancestor = adorned.GetVisualParent(); ancestor is not null; ancestor = ancestor.GetVisualParent())
        {
            var geometry = ancestor.Clip;
            if (!ancestor.ClipToBounds && geometry is null)
            {
                continue;
            }

            var clip = ancestor.ClipToBounds ? new Rect(ancestor.Bounds.Size) : geometry!.Bounds;
            if (ancestor.ClipToBounds && geometry is not null)
            {
                clip = clip.Intersect(geometry.Bounds);
            }

            if (ancestor.TransformToVisual(adorned) is not { } transform)
            {
                continue;
            }

            clip = clip.TransformToAABB(transform);
            var left   = element.Left >= clip.Left - ClipEdgeTolerance ? Math.Min(clip.Left, outline.Left) : clip.Left;
            var top    = element.Top >= clip.Top - ClipEdgeTolerance ? Math.Min(clip.Top, outline.Top) : clip.Top;
            var right  = element.Right <= clip.Right + ClipEdgeTolerance ? Math.Max(clip.Right, outline.Right) : clip.Right;
            var bottom = element.Bottom <= clip.Bottom + ClipEdgeTolerance ? Math.Max(clip.Bottom, outline.Bottom) : clip.Bottom;
            if (right <= left || bottom <= top)
            {
                return default(Rect);
            }

            var sideClip = new Rect(left, top, right - left, bottom - top);
            var combined = result is { } current ? current.Intersect(sideClip) : sideClip;
            if (combined.Width <= 0 || combined.Height <= 0)
            {
                return default(Rect);
            }

            result = combined;
        }

        return result;
    }

    private void SubscribeShapeSources()
    {
        if (AdornerLayer.GetAdornedElement(this) is not { } adorned)
        {
            return;
        }

        _shapeSource                 =  adorned;
        _shapeSource.PropertyChanged += HandleShapeSourcePropertyChanged;
        if (adorned is Button button)
        {
            button.ActualThemeVariantChanged += HandleButtonThemeVariantChanged;
        }

        if (adorned is not IFocusOutlineShapeProvider &&
            adorned is Control { TemplatedParent: IFocusOutlineShapeProvider and AvaloniaObject owner })
        {
            _shapeOwner                 =  owner;
            _shapeOwner.PropertyChanged += HandleShapeSourcePropertyChanged;
        }
    }

    private void UnsubscribeShapeSources()
    {
        if (_shapeSource is not null)
        {
            if (_shapeSource is Button button)
            {
                button.ActualThemeVariantChanged -= HandleButtonThemeVariantChanged;
            }

            _shapeSource.PropertyChanged -= HandleShapeSourcePropertyChanged;
            _shapeSource                 =  null;
        }

        if (_shapeOwner is not null)
        {
            _shapeOwner.PropertyChanged -= HandleShapeSourcePropertyChanged;
            _shapeOwner                 =  null;
        }
    }

    private void HandleShapeSourcePropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == Button.UsesThemeRecipeProperty)
        {
            UpdateOutlineTokenSubscription();
        }
        else if (ReferenceEquals(sender, _outlineTokenSource) && sender is Button &&
                 (e.Property == Button.EffectiveColorProperty || e.Property == Button.EffectiveVariantProperty))
        {
            RefreshOutline();
        }

        if (AdornerLayer.GetAdornedElement(this) is { } adorned &&
            ResolveCornerRadius(adorned) != _renderedCornerRadius)
        {
            InvalidateVisual();
        }
    }

    private void HandleButtonThemeVariantChanged(object? sender, EventArgs e)
    {
        if (ReferenceEquals(sender, _outlineTokenSource))
        {
            RefreshOutline();
        }
    }

    // Shapes such as circle buttons report a radius larger than half their size; clamp so the outline
    // stays a circle instead of relying on the renderer. An inset outline shrinks the radius, never below 0.
    private static double Grow(double radius, double inflate, double maxRadius) =>
        radius > 0 ? Math.Clamp(radius + inflate, 0, maxRadius) : 0;

    private static CornerRadius ResolveCornerRadius(Visual? adorned)
    {
        return adorned switch
        {
            IFocusOutlineShapeProvider provider => provider.FocusOutlineCornerRadius,
            // A template focus target whose visible shape comes from its owner, such as a header clipped by
            // the owner's rounded frame.
            Control { TemplatedParent: IFocusOutlineShapeProvider provider } => provider.FocusOutlineCornerRadius,
            TemplatedControl templatedControl => templatedControl.CornerRadius,
            Border border => border.CornerRadius,
            DashedBorder dashedBorder => dashedBorder.CornerRadius,
            _ => default
        };
    }
}
