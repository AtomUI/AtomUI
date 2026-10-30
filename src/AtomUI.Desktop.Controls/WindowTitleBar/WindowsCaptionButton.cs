using System.Diagnostics;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;

namespace AtomUI.Desktop.Controls;

[global::System.Runtime.Versioning.UnsupportedOSPlatform("browser")]
internal class WindowsCaptionButton : CaptionButton
{
    internal static readonly StyledProperty<WindowState> HostWindowStateProperty =
        WindowTitleBar.HostWindowStateProperty.AddOwner<WindowsCaptionButton>();

    internal static readonly DirectProperty<WindowsCaptionButton, bool> IsCloseButtonProperty =
        AvaloniaProperty.RegisterDirect<WindowsCaptionButton, bool>(nameof(IsCloseButton),
            o => o.IsCloseButton,
            (o, v) => o.IsCloseButton = v);

    internal static readonly DirectProperty<WindowsCaptionButton, bool> IsPointerOverSuppressedProperty =
        AvaloniaProperty.RegisterDirect<WindowsCaptionButton, bool>(nameof(IsPointerOverSuppressed),
            o => o.IsPointerOverSuppressed,
            (o, v) => o.IsPointerOverSuppressed = v);

    private bool _isCloseButton;
    private bool _isPointerOverSuppressed;

    internal WindowState HostWindowState
    {
        get => GetValue(HostWindowStateProperty);
        set => SetValue(HostWindowStateProperty, value);
    }

    internal bool IsCloseButton
    {
        get => _isCloseButton;
        set => SetAndRaise(IsCloseButtonProperty, ref _isCloseButton, value);
    }

    internal bool IsPointerOverSuppressed
    {
        get => _isPointerOverSuppressed;
        private set => SetAndRaise(IsPointerOverSuppressedProperty, ref _isPointerOverSuppressed, value);
    }

    static WindowsCaptionButton()
    {
        HostWindowStateProperty.Changed.AddClassHandler<WindowsCaptionButton>((button, _) =>
            button.InvalidatePointerOverVisualState());
    }

    internal void InvalidatePointerOverVisualState()
    {
        IsPointerOverSuppressed = IsPointerOver;
    }

    protected override void OnPointerEntered(PointerEventArgs e)
    {
        IsPointerOverSuppressed = false;
        base.OnPointerEntered(e);
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        IsPointerOverSuppressed = false;
        base.OnPointerMoved(e);
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        IsPointerOverSuppressed = false;
        base.OnPointerExited(e);
    }
    
    protected override void UpdateEffectiveCornerRadius(Size size)
    {
        Debug.Assert(MathUtils.AreClose(DesiredSize.Width, DesiredSize.Height));
        EffectiveCornerRadius = new CornerRadius(0);
    }
    
    protected override Size MeasureOverride(Size availableSize)
    {
        var measureConstraint = WindowsCaptionButtonLayout.NormalizeMeasureConstraint(availableSize);
        var measuredSize = base.MeasureOverride(measureConstraint);
        return WindowsCaptionButtonLayout.ResolveDesiredSize(measureConstraint, measuredSize);
    }
}
