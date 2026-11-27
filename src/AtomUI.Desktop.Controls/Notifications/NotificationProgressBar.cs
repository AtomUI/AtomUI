using System.Globalization;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme.Resources;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

internal class NotificationProgressBar : TemplatedControl
{
    #region 公共属性定义

    public static readonly StyledProperty<TimeSpan> ExpirationProperty =
        AvaloniaProperty.Register<NotificationProgressBar, TimeSpan>(nameof(Expiration));

    public static readonly StyledProperty<TimeSpan> CurrentExpirationProperty =
        AvaloniaProperty.Register<NotificationProgressBar, TimeSpan>(nameof(CurrentExpiration));

    public TimeSpan Expiration
    {
        get => GetValue(ExpirationProperty);
        set => SetValue(ExpirationProperty, value);
    }

    public TimeSpan CurrentExpiration
    {
        get => GetValue(CurrentExpirationProperty);
        set => SetValue(CurrentExpirationProperty, value);
    }

    #endregion

    #region 内部属性定义

    internal static readonly StyledProperty<bool> UseProgressBarTokenResourcesProperty =
        AvaloniaProperty.Register<NotificationProgressBar, bool>(nameof(UseProgressBarTokenResources));

    internal double ProgressIndicatorThickness { get; private set; }

    internal bool UseProgressBarTokenResources
    {
        get => GetValue(UseProgressBarTokenResourcesProperty);
        set => SetValue(UseProgressBarTokenResourcesProperty, value);
    }

    internal IBrush? ProgressIndicatorBrush { get; private set; }
    internal IBrush? ProgressTrackBrush { get; private set; }

    #endregion

    private static readonly object[] s_progressPaintKeys =
    [
        NotificationCardTokenKind.NotificationProgressHeight,
        NotificationCardTokenKind.NotificationProgressBg,
        NotificationCardTokenKind.NotificationProgressTrackBg
    ];

    private OwnerTokenSet? _paintTokens;
    
    static NotificationProgressBar()
    {
        AffectsRender<NotificationProgressBar>(ExpirationProperty, CurrentExpirationProperty);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        UpdatePaintTokenSubscription();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _paintTokens?.Dispose();
        _paintTokens = null;
        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == UseProgressBarTokenResourcesProperty && this.IsAttachedToVisualTree())
        {
            UpdatePaintTokenSubscription();
        }
    }

    private void UpdatePaintTokenSubscription()
    {
        if (UseProgressBarTokenResources && _paintTokens is null)
        {
            _paintTokens = new OwnerTokenSet(this, s_progressPaintKeys.Length, RefreshPaint);
            for (var index = 0; index < s_progressPaintKeys.Length; ++index)
            {
                _paintTokens.SetResourceKey(index, s_progressPaintKeys[index]);
            }
        }
        else if (!UseProgressBarTokenResources && _paintTokens is not null)
        {
            _paintTokens.Dispose();
            _paintTokens = null;
        }
        RefreshPaint();
    }

    private void RefreshPaint()
    {
        var thickness = ReadThickness(_paintTokens?.GetValue(0));
        var indicator = ReadBrush(_paintTokens?.GetValue(1), ProgressIndicatorBrush);
        var track = ReadBrush(_paintTokens?.GetValue(2), ProgressTrackBrush);
        var thicknessChanged = !ProgressIndicatorThickness.Equals(thickness);
        var brushChanged = !Equals(ProgressIndicatorBrush, indicator) || !Equals(ProgressTrackBrush, track);
        ProgressIndicatorThickness = thickness;
        ProgressIndicatorBrush = indicator;
        ProgressTrackBrush = track;

        if (thicknessChanged)
        {
            InvalidateMeasure();
        }
        if (thicknessChanged || brushChanged)
        {
            InvalidateVisual();
        }
    }

    private static IBrush? ReadBrush(object? value, IBrush? previous) => value switch
    {
        IBrush brush => brush,
        Color color => new ImmutableSolidColorBrush(color),
        string text when Color.TryParse(text, out var color) => new ImmutableSolidColorBrush(color),
        _ when ReferenceEquals(value, BindingOperations.DoNothing) => previous,
        _ => null
    };

    private double ReadThickness(object? value)
    {
        if (value is double thickness)
        {
            return thickness;
        }
        if (ReferenceEquals(value, BindingOperations.DoNothing))
        {
            return ProgressIndicatorThickness;
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

    protected override Size MeasureOverride(Size availableSize)
    {
        return new Size(availableSize.Width, ProgressIndicatorThickness);
    }

    public override void Render(DrawingContext context)
    {
        var offsetY = Bounds.Height - ProgressIndicatorThickness;
        var trackRect = new Rect(new Point(0, offsetY), new Size(Bounds.Width, ProgressIndicatorThickness));
        // Paint the track across the full width first, then the progress segment on top of it.
        context.FillRectangle(ProgressTrackBrush!, trackRect);

        var indicatorWidth = 0d;
        var total          = Expiration.TotalMilliseconds;
        if (MathUtils.GreaterThan(total, 0))
        {
            indicatorWidth = CurrentExpiration.TotalMilliseconds / total * Bounds.Width;
        }

        var indicatorRect = new Rect(new Point(0, offsetY), new Size(indicatorWidth, ProgressIndicatorThickness));
        context.FillRectangle(ProgressIndicatorBrush!, indicatorRect);
    }
}
