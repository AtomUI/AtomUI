using System.Reactive.Disposables;
using AtomUI.Media;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Presenters;
using Avalonia.Media;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

using AvaloniaTextBlock = Avalonia.Controls.TextBlock;
using AvaloniaTextBox = Avalonia.Controls.TextBox;

public sealed class OverflowTip : AvaloniaObject
{
    #region 公共属性定义

    public static readonly AttachedProperty<bool> IsEnabledProperty =
        AvaloniaProperty.RegisterAttached<OverflowTip, Control, bool>("IsEnabled");

    public static readonly AttachedProperty<object?> TextProperty =
        AvaloniaProperty.RegisterAttached<OverflowTip, Control, object?>("Text");

    public static readonly AttachedProperty<int> ShowDelayProperty =
        AvaloniaProperty.RegisterAttached<OverflowTip, Control, int>("ShowDelay", 1200);

    public static readonly AttachedProperty<PlacementMode> PlacementProperty =
        AvaloniaProperty.RegisterAttached<OverflowTip, Control, PlacementMode>("Placement", PlacementMode.TopEdgeAlignedLeft);

    public static readonly AttachedProperty<Control?> PlacementTargetProperty =
        AvaloniaProperty.RegisterAttached<OverflowTip, Control, Control?>("PlacementTarget");

    public static bool GetIsEnabled(Control element)
    {
        return element.GetValue(IsEnabledProperty);
    }

    public static void SetIsEnabled(Control element, bool value)
    {
        element.SetValue(IsEnabledProperty, value);
    }

    public static object? GetText(Control element)
    {
        return element.GetValue(TextProperty);
    }

    public static void SetText(Control element, object? value)
    {
        element.SetValue(TextProperty, value);
    }

    public static int GetShowDelay(Control element)
    {
        return element.GetValue(ShowDelayProperty);
    }

    public static void SetShowDelay(Control element, int value)
    {
        element.SetValue(ShowDelayProperty, value);
    }

    public static PlacementMode GetPlacement(Control element)
    {
        return element.GetValue(PlacementProperty);
    }

    public static void SetPlacement(Control element, PlacementMode value)
    {
        element.SetValue(PlacementProperty, value);
    }

    public static Control? GetPlacementTarget(Control element)
    {
        return element.GetValue(PlacementTargetProperty);
    }

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
        private readonly CompositeDisposable _viewportSubscriptions = new();

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
            if (_owner.IsAttachedToVisualTree())
            {
                SubscribeViewportChanges();
            }
        }

        public void Dispose()
        {
            ClearOwnedTip();
            _viewportSubscriptions.Dispose();
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
            SubscribeViewportChanges();
            Update();
        }

        private void HandleDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
        {
            _viewportSubscriptions.Clear();
            ClearOwnedTip();
        }

        private void SubscribeViewportChanges()
        {
            _viewportSubscriptions.Clear();
            foreach (var visual in _owner.GetSelfAndVisualAncestors())
            {
                visual.PropertyChanged += HandleViewportPropertyChanged;
                _viewportSubscriptions.Add(Disposable.Create(() => visual.PropertyChanged -= HandleViewportPropertyChanged));
                if (visual.RenderTransform is Transform transform)
                {
                    transform.Changed += HandleViewportGeometryChanged;
                    _viewportSubscriptions.Add(Disposable.Create(() => transform.Changed -= HandleViewportGeometryChanged));
                }
                if (visual.Clip is { } clip)
                {
                    clip.Changed += HandleViewportGeometryChanged;
                    _viewportSubscriptions.Add(Disposable.Create(() => clip.Changed -= HandleViewportGeometryChanged));
                }
            }
        }

        private void HandleViewportPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs args)
        {
            if (args.Property == Visual.RenderTransformProperty || args.Property == Visual.ClipProperty)
            {
                SubscribeViewportChanges();
                Update();
            }
            else if (args.Property == Visual.BoundsProperty || args.Property == Visual.ClipToBoundsProperty ||
                     args.Property == Visual.RenderTransformOriginProperty || args.Property == Visual.IsVisibleProperty)
            {
                Update();
            }
        }

        private void HandleViewportGeometryChanged(object? sender, EventArgs args)
        {
            Update();
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

            var textSize = TextUtils.CalculateTextSize(
                text,
                TextElement.GetFontSize(_owner),
                TextElement.GetFontFamily(_owner),
                TextElement.GetFontStyle(_owner),
                TextElement.GetFontWeight(_owner));

            return textSize.Width > availableWidth + OverflowEpsilon;
        }

        private double ResolveAvailableWidth()
        {
            var width = ResolveTextViewportWidth();
            if (!IsUsableWidth(width))
            {
                return width;
            }

            var paddingLeft = _owner switch
            {
                AvaloniaTextBox textBox => textBox.Padding.Left,
                AvaloniaTextBlock textBlock => textBlock.Padding.Left,
                _ => 0
            };
            var visible = new Rect(paddingLeft, 0, width, _owner.Bounds.Height);
            foreach (var visual in _owner.GetSelfAndVisualAncestors())
            {
                if (!visual.IsVisible)
                {
                    return 0;
                }

                Rect? clip = visual.ClipToBounds ? new Rect(visual.Bounds.Size) : null;
                if (visual.Clip is { } geometry)
                {
                    clip = clip?.Intersect(geometry.Bounds) ?? geometry.Bounds;
                }
                if (clip.HasValue)
                {
                    if (visual.TransformToVisual(_owner) is not { } transform)
                    {
                        return double.NaN;
                    }
                    visible = visible.Intersect(clip.Value.TransformToAABB(transform));
                }
            }

            return visible.Width;
        }

        private double ResolveTextViewportWidth()
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
