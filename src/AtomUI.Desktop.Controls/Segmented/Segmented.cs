using AtomUI.Controls.Commons;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Generated.AtomUIDesktopControls;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Desktop segmented control. It creates <see cref="SegmentedItem" /> containers for its items and supplies the
/// paint of the selection thumb: under the desktop theme, a thumb brush or shadow that is not set comes from the
/// <c>SegmentedToken.ItemSelectedBg</c> and the tertiary box shadow token resources.
/// </summary>
public partial class Segmented : AbstractSegmented
{
    /// <summary>
    /// Identifies the <see cref="UseSelectedThumbTokenResources" /> property.
    /// </summary>
    internal static readonly StyledProperty<bool> UseSelectedThumbTokenResourcesProperty =
        AvaloniaProperty.Register<Segmented, bool>(nameof(UseSelectedThumbTokenResources));

    private static readonly object s_thumbBrushKey = SegmentedTokenKind.ItemSelectedBg;
    private static readonly object s_thumbShadowsKey =
        ControlTokenResourceKey.Global(SegmentedTokens.Identity, SharedTokenKind.BoxShadowsTertiary);

    private OwnerTokenSet? _thumbTokens;
    private (IBrush? Brush, BoxShadows Shadows) _resourcePaint;

    /// <summary>
    /// When <see langword="true" /> and the control is attached, <see cref="ThumbPaint" /> takes the brush and the
    /// shadow from the <c>SegmentedTokenKind.ItemSelectedBg</c> and tertiary box shadow token resources whenever
    /// <c>SelectedThumbBg</c> or <c>SelectedThumbBoxShadows</c> is not set. The desktop theme
    /// sets it to <see langword="true" />; the default is <see langword="false" />.
    /// </summary>
    internal bool UseSelectedThumbTokenResources
    {
        get => GetValue(UseSelectedThumbTokenResourcesProperty);
        set => SetValue(UseSelectedThumbTokenResourcesProperty, value);
    }

    /// <summary>
    /// The brush and the shadow of the selection thumb: <c>SelectedThumbBg</c> or <c>SelectedThumbBoxShadows</c>
    /// when they are set, and otherwise the values read from the token resources while
    /// <see cref="UseSelectedThumbTokenResources" /> is <see langword="true" />, or no brush and no shadow.
    /// </summary>
    private protected override (IBrush? Brush, BoxShadows Shadows) ThumbPaint =>
        (IsSet(SelectedThumbBgProperty) ? SelectedThumbBg : _resourcePaint.Brush,
         IsSet(SelectedThumbBoxShadowsProperty) ? SelectedThumbBoxShadows : _resourcePaint.Shadows);

    public Segmented()
    {
    }

    /// <summary>
    /// Creates the token subscription of the selection thumb when <see cref="UseSelectedThumbTokenResources" /> is
    /// on. The base method runs first; an override must call it first to keep the thumb paint tracking the tokens.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        UpdateThumbTokenSubscription();
    }

    /// <summary>
    /// Disposes the token subscription of the selection thumb, then calls the base method. The subscription is
    /// created again on the next attach.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _thumbTokens?.Dispose();
        _thumbTokens = null;
        base.OnDetachedFromVisualTree(e);
    }

    /// <summary>
    /// Calls the base method, then synchronizes the token subscription of the selection thumb with
    /// <see cref="UseSelectedThumbTokenResources" /> while the control is attached. A change made while it is
    /// detached takes effect on the next attach.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == UseSelectedThumbTokenResourcesProperty && this.IsAttachedToVisualTree())
        {
            UpdateThumbTokenSubscription();
        }
    }

    private void UpdateThumbTokenSubscription()
    {
        if (UseSelectedThumbTokenResources && _thumbTokens is null)
        {
            _thumbTokens = new OwnerTokenSet(this, 2, RefreshThumbPaint);
            _thumbTokens.SetResourceKey(0, s_thumbBrushKey);
            _thumbTokens.SetResourceKey(1, s_thumbShadowsKey);
        }
        else if (!UseSelectedThumbTokenResources && _thumbTokens is not null)
        {
            _thumbTokens.Dispose();
            _thumbTokens = null;
        }
        RefreshThumbPaint();
    }

    private void RefreshThumbPaint()
    {
        var previous = ThumbPaint;
        var brush = _thumbTokens?.GetValue(0) switch
        {
            IBrush value => value,
            Color color => new ImmutableSolidColorBrush(color),
            string text when Color.TryParse(text, out var color) => new ImmutableSolidColorBrush(color),
            var value when ReferenceEquals(value, BindingOperations.DoNothing) => _resourcePaint.Brush,
            _ => null
        };
        var shadows = ReadThumbShadows(_thumbTokens?.GetValue(1));
        _resourcePaint = (brush, shadows);
        if (!previous.Equals(ThumbPaint))
        {
            InvalidateVisual();
        }
    }

    private BoxShadows ReadThumbShadows(object? value)
    {
        if (value is BoxShadows shadows)
        {
            return shadows;
        }
        if (ReferenceEquals(value, BindingOperations.DoNothing))
        {
            return _resourcePaint.Shadows;
        }
        if (value is string text)
        {
            try
            {
                return BoxShadows.Parse(text);
            }
            catch (FormatException)
            {
                return default;
            }
        }
        return default;
    }

    /// <summary>
    /// Creates a <see cref="SegmentedItem" /> as the container for an item and adds the item semantic part class to
    /// it. <paramref name="item" />, <paramref name="index" /> and <paramref name="recycleKey" /> are not read.
    /// </summary>
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        var segmentedItem = new SegmentedItem();
        segmentedItem.Classes.Add(SegmentedSemanticParts.ItemClass);
        return segmentedItem;
    }

    /// <summary>
    /// Calls the base method, then adds the item semantic part class to a <see cref="SegmentedItem" /> container.
    /// This also reaches containers that <see cref="CreateContainerForItemOverride" /> did not create, such as
    /// items declared in XAML. Containers of other types are not changed here.
    /// </summary>
    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);
        if (container is SegmentedItem segmentedItem)
        {
            segmentedItem.Classes.Add(SegmentedSemanticParts.ItemClass);
        }
    }
}
