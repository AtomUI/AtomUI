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

public partial class Segmented : AbstractSegmented
{
    internal static readonly StyledProperty<bool> UseSelectedThumbTokenResourcesProperty =
        AvaloniaProperty.Register<Segmented, bool>(nameof(UseSelectedThumbTokenResources));

    private static readonly object s_thumbBrushKey = SegmentedTokenKind.ItemSelectedBg;
    private static readonly object s_thumbShadowsKey =
        ControlTokenResourceKey.Global(SegmentedTokens.Identity, SharedTokenKind.BoxShadowsTertiary);

    private OwnerTokenSet? _thumbTokens;
    private (IBrush? Brush, BoxShadows Shadows) _resourcePaint;

    internal bool UseSelectedThumbTokenResources
    {
        get => GetValue(UseSelectedThumbTokenResourcesProperty);
        set => SetValue(UseSelectedThumbTokenResourcesProperty, value);
    }

    private protected override (IBrush? Brush, BoxShadows Shadows) ThumbPaint =>
        (IsSet(SelectedThumbBgProperty) ? SelectedThumbBg : _resourcePaint.Brush,
         IsSet(SelectedThumbBoxShadowsProperty) ? SelectedThumbBoxShadows : _resourcePaint.Shadows);

    public Segmented()
    {
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        UpdateThumbTokenSubscription();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _thumbTokens?.Dispose();
        _thumbTokens = null;
        base.OnDetachedFromVisualTree(e);
    }

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

    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        var segmentedItem = new SegmentedItem();
        segmentedItem.Classes.Add(SegmentedSemanticParts.ItemClass);
        return segmentedItem;
    }

    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);
        if (container is SegmentedItem segmentedItem)
        {
            segmentedItem.Classes.Add(SegmentedSemanticParts.ItemClass);
        }
    }
}
