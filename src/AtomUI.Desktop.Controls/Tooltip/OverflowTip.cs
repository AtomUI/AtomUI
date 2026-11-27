using System.Reactive.Disposables;
using AtomUI.Media;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Presenters;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

using AvaloniaTextBlock = Avalonia.Controls.TextBlock;
using AvaloniaTextBox = Avalonia.Controls.TextBox;

/// <summary>
/// Shows a tool tip that repeats the full text of an element while that text is wider than the space it has.
/// </summary>
/// <remarks>
/// Set <see cref="IsEnabledProperty" /> on the element to watch. The element is checked when its bounds, font or
/// text change and when it is attached to the visual tree. While the text overflows, it is written to
/// <c>ToolTip.Tip</c> of the element, so the element shows it as its tool tip. That tool tip is removed when the
/// text fits again or when the element is invisible or detached. A tool tip that other code sets on the element
/// is kept and takes the place of this one.
/// </remarks>
public sealed class OverflowTip : AvaloniaObject
{
    #region 附加属性定义

    /// <summary>
    /// Identifies the IsEnabled attached property, which switches the overflow tip of an element on or off.
    /// </summary>
    /// <remarks>
    /// Set it on the element whose text may be clipped. The value is not inherited, so it is read from that element
    /// only. <see langword="true" /> starts the watch described in the class remarks. <see langword="false" />, the
    /// default, stops the watch and removes a tool tip written here.
    /// </remarks>
    public static readonly AttachedProperty<bool> IsEnabledProperty =
        AvaloniaProperty.RegisterAttached<OverflowTip, Control, bool>("IsEnabled");

    /// <summary>
    /// Identifies the Text attached property, which holds the text to measure and to show in the overflow tip.
    /// </summary>
    /// <remarks>
    /// Set it on the watched element. <see langword="null" />, the default, takes the text from the element itself:
    /// the <c>Text</c> of an <c>Avalonia.Controls.TextBlock</c> or an <c>Avalonia.Controls.TextBox</c>, or the
    /// <c>Content</c> of an <c>Avalonia.Controls.Presenters.ContentPresenter</c>. Other elements show no tip unless
    /// this property is set. The value is shown as its <c>ToString()</c> result, and an empty result shows no tip.
    /// </remarks>
    public static readonly AttachedProperty<object?> TextProperty =
        AvaloniaProperty.RegisterAttached<OverflowTip, Control, object?>("Text");

    /// <summary>
    /// Identifies the ShowDelay attached property, which sets how long to wait before the overflow tip appears.
    /// </summary>
    /// <remarks>
    /// Set it on the watched element, in milliseconds; the default is 1200. The value is written to
    /// <c>ToolTip.ShowDelay</c> of the element whenever the tip is applied here, so it replaces a delay set on the
    /// element through <c>ToolTip</c> directly.
    /// </remarks>
    public static readonly AttachedProperty<int> ShowDelayProperty =
        AvaloniaProperty.RegisterAttached<OverflowTip, Control, int>("ShowDelay", 1200);

    /// <summary>
    /// Identifies the Placement attached property, which sets where the overflow tip appears.
    /// </summary>
    /// <remarks>
    /// Set it on the watched element. The default is
    /// <see cref="Avalonia.Controls.PlacementMode.TopEdgeAlignedLeft" />. The value is written to
    /// <c>ToolTip.Placement</c> of the element whenever the tip is applied here, so it replaces a placement set on
    /// the element through <c>ToolTip</c> directly. The horizontal and vertical offsets of the tool tip are written
    /// at the same time to align the tip with <see cref="PlacementTargetProperty" />.
    /// </remarks>
    public static readonly AttachedProperty<PlacementMode> PlacementProperty =
        AvaloniaProperty.RegisterAttached<OverflowTip, Control, PlacementMode>("Placement", PlacementMode.TopEdgeAlignedLeft);

    /// <summary>
    /// Identifies the PlacementTarget attached property, which sets the control the overflow tip is aligned with.
    /// </summary>
    /// <remarks>
    /// Set it on the watched element; the default is <see langword="null" />. While it names another control, the tip
    /// is shifted so that it aligns with that control instead of the watched element. Use this when the watched
    /// element is only a part of a larger control. <see langword="null" /> or the watched element itself leaves the
    /// tip aligned with the watched element. The shift is written to <c>ToolTip.HorizontalOffset</c> and
    /// <c>ToolTip.VerticalOffset</c> of the element whenever the tip is applied here, so it replaces offsets set on
    /// the element through <c>ToolTip</c> directly.
    /// </remarks>
    public static readonly AttachedProperty<Control?> PlacementTargetProperty =
        AvaloniaProperty.RegisterAttached<OverflowTip, Control, Control?>("PlacementTarget");

    /// <summary>
    /// Reads whether the overflow tip is switched on for an element.
    /// </summary>
    /// <param name="element">
    /// The element that carries the value.
    /// </param>
    /// <returns>
    /// The value on the element, or <see langword="false" /> when the property is not set on it.
    /// </returns>
    public static bool GetIsEnabled(Control element)
    {
        return element.GetValue(IsEnabledProperty);
    }

    /// <summary>
    /// Switches the overflow tip for an element on or off.
    /// </summary>
    /// <param name="element">
    /// The element whose text may be clipped.
    /// </param>
    /// <param name="value">
    /// Whether the element is watched for clipped text.
    /// </param>
    public static void SetIsEnabled(Control element, bool value)
    {
        element.SetValue(IsEnabledProperty, value);
    }

    /// <summary>
    /// Reads the text that an element shows in its overflow tip.
    /// </summary>
    /// <param name="element">
    /// The element that carries the value.
    /// </param>
    /// <returns>
    /// The value on the element, or <see langword="null" /> when the property is not set on it.
    /// </returns>
    public static object? GetText(Control element)
    {
        return element.GetValue(TextProperty);
    }

    /// <summary>
    /// Sets the text that an element shows in its overflow tip.
    /// </summary>
    /// <param name="element">
    /// The element whose text may be clipped.
    /// </param>
    /// <param name="value">
    /// The text to show, or <see langword="null" /> to take the text from the element.
    /// </param>
    public static void SetText(Control element, object? value)
    {
        element.SetValue(TextProperty, value);
    }

    /// <summary>
    /// Reads how long an element waits before its overflow tip appears.
    /// </summary>
    /// <param name="element">
    /// The element that carries the value.
    /// </param>
    /// <returns>
    /// The value on the element in milliseconds, or 1200 when the property is not set on it.
    /// </returns>
    public static int GetShowDelay(Control element)
    {
        return element.GetValue(ShowDelayProperty);
    }

    /// <summary>
    /// Sets how long an element waits before its overflow tip appears.
    /// </summary>
    /// <param name="element">
    /// The element whose text may be clipped.
    /// </param>
    /// <param name="value">
    /// The wait in milliseconds.
    /// </param>
    public static void SetShowDelay(Control element, int value)
    {
        element.SetValue(ShowDelayProperty, value);
    }

    /// <summary>
    /// Reads where the overflow tip of an element appears.
    /// </summary>
    /// <param name="element">
    /// The element that carries the value.
    /// </param>
    /// <returns>
    /// The value on the element, or <see cref="Avalonia.Controls.PlacementMode.TopEdgeAlignedLeft" /> when the
    /// property is not set on it.
    /// </returns>
    public static PlacementMode GetPlacement(Control element)
    {
        return element.GetValue(PlacementProperty);
    }

    /// <summary>
    /// Sets where the overflow tip of an element appears.
    /// </summary>
    /// <param name="element">
    /// The element whose text may be clipped.
    /// </param>
    /// <param name="value">
    /// The placement, relative to the watched element. <see cref="PlacementTargetProperty" /> shifts the tip so
    /// that it aligns with another control.
    /// </param>
    public static void SetPlacement(Control element, PlacementMode value)
    {
        element.SetValue(PlacementProperty, value);
    }

    /// <summary>
    /// Reads the control that the overflow tip of an element is aligned with.
    /// </summary>
    /// <param name="element">
    /// The element that carries the value.
    /// </param>
    /// <returns>
    /// The value on the element, or <see langword="null" /> when the property is not set on it.
    /// </returns>
    public static Control? GetPlacementTarget(Control element)
    {
        return element.GetValue(PlacementTargetProperty);
    }

    /// <summary>
    /// Sets the control that the overflow tip of an element is aligned with.
    /// </summary>
    /// <param name="element">
    /// The element whose text may be clipped.
    /// </param>
    /// <param name="value">
    /// The control to align with, or <see langword="null" /> to use the element itself.
    /// </param>
    public static void SetPlacementTarget(Control element, Control? value)
    {
        element.SetValue(PlacementTargetProperty, value);
    }

    #endregion

    private static readonly AttachedProperty<OverflowTipState?> StateProperty =
        AvaloniaProperty.RegisterAttached<OverflowTip, Control, OverflowTipState?>("State");

    static OverflowTip()
    {
        IsEnabledProperty.Changed.Subscribe(HandleIsEnabledChanged);
        TextProperty.Changed.Subscribe(HandleOverflowTipPropertyChanged);
        ShowDelayProperty.Changed.Subscribe(HandleOverflowTipPropertyChanged);
        PlacementProperty.Changed.Subscribe(HandleOverflowTipPropertyChanged);
        PlacementTargetProperty.Changed.Subscribe(HandleOverflowTipPropertyChanged);
    }

    private OverflowTip()
    {
    }

    private static void HandleIsEnabledChanged(AvaloniaPropertyChangedEventArgs<bool> args)
    {
        if (args.Sender is not Control control)
        {
            return;
        }

        if (args.NewValue.Value)
        {
            GetOrCreateState(control).Update();
        }
        else
        {
            ReleaseState(control);
        }
    }

    private static void HandleOverflowTipPropertyChanged(AvaloniaPropertyChangedEventArgs args)
    {
        if (args.Sender is Control control)
        {
            GetState(control)?.Update();
        }
    }

    private static OverflowTipState GetOrCreateState(Control control)
    {
        var state = GetState(control);
        if (state is not null)
        {
            return state;
        }

        state = new OverflowTipState(control);
        control.SetValue(StateProperty, state);
        return state;
    }

    private static OverflowTipState? GetState(Control control)
    {
        return control.GetValue(StateProperty);
    }

    private static void ReleaseState(Control control)
    {
        var state = GetState(control);
        if (state is null)
        {
            return;
        }

        state.Dispose();
        control.ClearValue(StateProperty);
    }

    private sealed class OverflowTipState : IDisposable
    {
        private const double OverflowEpsilon = 0.5;

        private readonly Control _owner;
        private readonly CompositeDisposable _subscriptions = new();

        private bool _isApplyingTip;
        private bool _ownsTip;

        public OverflowTipState(Control owner)
        {
            _owner = owner;

            _owner.AttachedToVisualTree += HandleAttachedToVisualTree;
            _owner.DetachedFromVisualTree += HandleDetachedFromVisualTree;
            _subscriptions.Add(Disposable.Create(() =>
            {
                _owner.AttachedToVisualTree -= HandleAttachedToVisualTree;
                _owner.DetachedFromVisualTree -= HandleDetachedFromVisualTree;
            }));

            _subscriptions.Add(_owner.GetObservable(Visual.BoundsProperty).Subscribe(_ => Update()));
            _subscriptions.Add(_owner.GetObservable(TextElement.FontSizeProperty).Subscribe(_ => Update()));
            _subscriptions.Add(_owner.GetObservable(TextElement.FontFamilyProperty).Subscribe(_ => Update()));
            _subscriptions.Add(_owner.GetObservable(TextElement.FontStyleProperty).Subscribe(_ => Update()));
            _subscriptions.Add(_owner.GetObservable(TextElement.FontWeightProperty).Subscribe(_ => Update()));
            _subscriptions.Add(_owner.GetObservable(ToolTip.TipProperty).Subscribe(HandleToolTipChanged));
            SubscribeTargetTextChanges();
        }

        public void Dispose()
        {
            ClearOwnedTip();
            _subscriptions.Dispose();
        }

        public void Update()
        {
            if (!GetIsEnabled(_owner) || !_owner.IsAttachedToVisualTree() || !_owner.IsVisible)
            {
                ClearOwnedTip();
                return;
            }

            var text = ResolveText();
            if (string.IsNullOrEmpty(text) || HasUserTip())
            {
                ClearOwnedTip();
                return;
            }

            if (IsTextOverflowing(text))
            {
                ApplyOwnedTip(text);
            }
            else
            {
                ClearOwnedTip();
            }
        }

        private void SubscribeTargetTextChanges()
        {
            if (_owner is AvaloniaTextBlock textBlock)
            {
                _subscriptions.Add(textBlock.GetObservable(AvaloniaTextBlock.TextProperty).Subscribe(_ => Update()));
            }
            else if (_owner is AvaloniaTextBox textBox)
            {
                _subscriptions.Add(textBox.GetObservable(AvaloniaTextBox.TextProperty).Subscribe(_ => Update()));
                _subscriptions.Add(textBox.GetObservable(TextViewportMetrics.ViewportWidthProperty)
                                          .Subscribe(_ => Update()));
            }
            else if (_owner is ContentPresenter contentPresenter)
            {
                _subscriptions.Add(contentPresenter.GetObservable(ContentPresenter.ContentProperty).Subscribe(_ => Update()));
            }
        }

        private void HandleAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
        {
            Update();
        }

        private void HandleDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
        {
            ClearOwnedTip();
        }

        private void HandleToolTipChanged(object? value)
        {
            if (_isApplyingTip)
            {
                return;
            }

            _ownsTip = false;
        }

        private bool HasUserTip()
        {
            return ToolTip.GetTip(_owner) is not null && !_ownsTip;
        }

        private void ApplyOwnedTip(string text)
        {
            _isApplyingTip = true;
            try
            {
                ToolTip.SetTip(_owner, text);
                ToolTip.SetShowDelay(_owner, GetShowDelay(_owner));
                ToolTip.SetPlacement(_owner, GetPlacement(_owner));
                var placementOffset = CalculatePlacementOffset();
                ToolTip.SetHorizontalOffset(_owner, placementOffset.X);
                ToolTip.SetVerticalOffset(_owner, placementOffset.Y);
                _ownsTip = true;
            }
            finally
            {
                _isApplyingTip = false;
            }
        }

        private void ClearOwnedTip()
        {
            if (!_ownsTip)
            {
                return;
            }

            _isApplyingTip = true;
            try
            {
                ToolTip.SetTip(_owner, null);
                _ownsTip = false;
            }
            finally
            {
                _isApplyingTip = false;
            }
        }

        private Point CalculatePlacementOffset()
        {
            var placementTarget = GetPlacementTarget(_owner);
            if (placementTarget is null || placementTarget == _owner)
            {
                return default;
            }

            var ownerOrigin = _owner.TranslatePoint(default, placementTarget);
            if (ownerOrigin is null)
            {
                return default;
            }

            var placement    = GetPlacement(_owner);
            var ownerBounds  = _owner.Bounds;
            var targetBounds = placementTarget.Bounds;
            var offsetX      = 0.0d;
            var offsetY      = 0.0d;

            switch (placement)
            {
                case PlacementMode.Top:
                case PlacementMode.Bottom:
                    offsetX = targetBounds.Width / 2 - (ownerOrigin.Value.X + ownerBounds.Width / 2);
                    break;
                case PlacementMode.TopEdgeAlignedLeft:
                case PlacementMode.BottomEdgeAlignedLeft:
                    offsetX = -ownerOrigin.Value.X;
                    break;
                case PlacementMode.TopEdgeAlignedRight:
                case PlacementMode.BottomEdgeAlignedRight:
                    offsetX = targetBounds.Width - ownerOrigin.Value.X - ownerBounds.Width;
                    break;
                case PlacementMode.Left:
                case PlacementMode.Right:
                    offsetY = targetBounds.Height / 2 - (ownerOrigin.Value.Y + ownerBounds.Height / 2);
                    break;
                case PlacementMode.LeftEdgeAlignedTop:
                case PlacementMode.RightEdgeAlignedTop:
                    offsetY = -ownerOrigin.Value.Y;
                    break;
                case PlacementMode.LeftEdgeAlignedBottom:
                case PlacementMode.RightEdgeAlignedBottom:
                    offsetY = targetBounds.Height - ownerOrigin.Value.Y - ownerBounds.Height;
                    break;
            }

            return new Point(offsetX, offsetY);
        }

        private string? ResolveText()
        {
            var configuredText = GetText(_owner);
            if (configuredText is not null)
            {
                return configuredText.ToString();
            }

            return _owner switch
            {
                AvaloniaTextBlock textBlock => textBlock.Text,
                AvaloniaTextBox textBox     => textBox.Text,
                ContentPresenter presenter  => presenter.Content?.ToString(),
                _                           => null
            };
        }

        private bool IsTextOverflowing(string text)
        {
            var availableWidth = ResolveAvailableWidth();
            if (!IsUsableWidth(availableWidth))
            {
                return false;
            }

            // Shared parts bind FontSize to inherited theme inputs; while a part joins or leaves its owner the
            // inherited value is transiently the 0 default. Text cannot be measured then; the FontSize
            // subscription re-evaluates once the real size arrives.
            var fontSize = TextElement.GetFontSize(_owner);
            if (!IsUsableWidth(fontSize))
            {
                return false;
            }

            var textSize = TextUtils.CalculateTextSize(
                text,
                fontSize,
                TextElement.GetFontFamily(_owner),
                TextElement.GetFontStyle(_owner),
                TextElement.GetFontWeight(_owner));

            return textSize.Width > availableWidth + OverflowEpsilon;
        }

        private double ResolveAvailableWidth()
        {
            if (_owner is AvaloniaTextBox textBox)
            {
                var viewportWidth = TextViewportMetrics.GetViewportWidth(textBox);
                if (viewportWidth.HasValue)
                {
                    return viewportWidth.Value;
                }

                return textBox.Bounds.Width - textBox.Padding.Left - textBox.Padding.Right;
            }

            if (_owner is AvaloniaTextBlock textBlock)
            {
                return textBlock.Bounds.Width - textBlock.Padding.Left - textBlock.Padding.Right;
            }

            return _owner.Bounds.Width;
        }

        private static bool IsUsableWidth(double width)
        {
            return !double.IsNaN(width) && !double.IsInfinity(width) && width > 0;
        }
    }
}
