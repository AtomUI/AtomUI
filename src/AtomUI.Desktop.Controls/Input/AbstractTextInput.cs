using AtomUI.Controls;
using AtomUI.Icons.AntDesign;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Input;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

using SharedTextInput = AtomUI.Controls.AbstractTextInput;
using AvaloniaButton = Avalonia.Controls.Button;

/// <summary>
/// Shared text-input logic for AtomUI desktop text controls.
/// </summary>
public abstract class AbstractTextInput : SharedTextInput,
                                           IMotionAwareControl,
                                           ICustomizableSizeTypeAware,
                                           ICompactSpaceAware,
                                           IFormItemAware,
                                           IFormItemFeedbackAware,
                                           IInputControlStatusAware,
                                           IInputControlStyleVariantAware
{
    public static readonly StyledProperty<CustomizableSizeType> SizeTypeProperty =
        CustomizableSizeTypeControlProperty.SizeTypeProperty.AddOwner<AbstractTextInput>();

    public static readonly StyledProperty<InputControlStyleVariant> StyleVariantProperty =
        InputControlStyleVariantProperty.StyleVariantProperty.AddOwner<AbstractTextInput>();

    public static readonly StyledProperty<bool> IsCustomFontSizeProperty =
        AvaloniaProperty.Register<AbstractTextInput, bool>(nameof(IsCustomFontSize));

    public new static readonly StyledProperty<InputControlStatus> StatusProperty =
        SharedTextInput.StatusProperty.AddOwner<AbstractTextInput>();

    public new static readonly StyledProperty<bool> IsAllowClearProperty =
        SharedTextInput.IsAllowClearProperty.AddOwner<AbstractTextInput>();

    public new static readonly StyledProperty<bool> IsEnableRevealButtonProperty =
        SharedTextInput.IsEnableRevealButtonProperty.AddOwner<AbstractTextInput>();

    public new static readonly StyledProperty<bool> IsShowCountProperty =
        SharedTextInput.IsShowCountProperty.AddOwner<AbstractTextInput>();

    public new static readonly StyledProperty<PathIcon?> ClearIconProperty =
        SharedTextInput.ClearIconProperty.AddOwner<AbstractTextInput>();

    public new static readonly StyledProperty<bool> IsMotionEnabledProperty =
        SharedTextInput.IsMotionEnabledProperty.AddOwner<AbstractTextInput>();

    public CustomizableSizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }

    public InputControlStyleVariant StyleVariant
    {
        get => GetValue(StyleVariantProperty);
        set => SetValue(StyleVariantProperty, value);
    }

    public bool IsCustomFontSize
    {
        get => GetValue(IsCustomFontSizeProperty);
        set => SetValue(IsCustomFontSizeProperty, value);
    }

    public new InputControlStatus Status
    {
        get => base.Status;
        set => base.Status = value;
    }

    public new bool IsAllowClear
    {
        get => base.IsAllowClear;
        set => base.IsAllowClear = value;
    }

    public new bool IsEnableRevealButton
    {
        get => base.IsEnableRevealButton;
        set => base.IsEnableRevealButton = value;
    }

    public new bool IsShowCount
    {
        get => base.IsShowCount;
        set => base.IsShowCount = value;
    }

    public new PathIcon? ClearIcon
    {
        get => base.ClearIcon;
        set => base.ClearIcon = value;
    }

    public new bool IsMotionEnabled
    {
        get => base.IsMotionEnabled;
        set => base.IsMotionEnabled = value;
    }

    internal static readonly StyledProperty<SpaceItemPosition?> CompactSpaceItemPositionProperty =
        CompactSpaceAwareControlProperty.CompactSpaceItemPositionProperty.AddOwner<AbstractTextInput>();

    internal static readonly StyledProperty<Orientation> CompactSpaceOrientationProperty =
        CompactSpaceAwareControlProperty.CompactSpaceOrientationProperty.AddOwner<AbstractTextInput>();

    internal static readonly StyledProperty<bool> IsUsedInCompactSpaceProperty =
        CompactSpaceAwareControlProperty.IsUsedInCompactSpaceProperty.AddOwner<AbstractTextInput>();

    internal SpaceItemPosition? CompactSpaceItemPosition
    {
        get => GetValue(CompactSpaceItemPositionProperty);
        set => SetValue(CompactSpaceItemPositionProperty, value);
    }

    internal Orientation CompactSpaceOrientation
    {
        get => GetValue(CompactSpaceOrientationProperty);
        set => SetValue(CompactSpaceOrientationProperty, value);
    }

    internal bool IsUsedInCompactSpace
    {
        get => GetValue(IsUsedInCompactSpaceProperty);
        set => SetValue(IsUsedInCompactSpaceProperty, value);
    }

    private IDisposable? _textViewportSubscription;

    static AbstractTextInput()
    {
        AffectsArrange<AbstractTextInput>(CompactSpaceItemPositionProperty, CompactSpaceOrientationProperty);
    }

    protected AbstractTextInput()
    {
    }

    protected new TextPresenter? InputTextPresenter => base.InputTextPresenter;

    protected virtual AvaloniaProperty? InnerRightContentTemplatePropertyForBinding => null;

    protected override bool CanClearText => base.CanClearText;
    protected override bool AllowsClearForMultilineInput => base.AllowsClearForMultilineInput;
    protected override void NotifyClearButtonClicked() => base.NotifyClearButtonClicked();
    protected override void NotifySetFormValue(string? value) => base.NotifySetFormValue(value);
    protected override string? NotifyGetFormValue() => base.NotifyGetFormValue();
    protected override void NotifyClearFormValue() => base.NotifyClearFormValue();
    protected override void NotifyValidateStatus(FormValidateStatus status) => base.NotifyValidateStatus(status);
    protected override void NotifySetFeedBackControl(FormValidateFeedback? value) => base.NotifySetFeedBackControl(value);

    internal void NotifyTextViewportCreated(ScrollViewer scrollViewer) => SetupTextViewportMetrics(scrollViewer);

    protected override void OnInitialized()
    {
        base.OnInitialized();
        if (ClearIcon is null)
        {
            SetCurrentValue(ClearIconProperty, new CloseCircleFilled());
        }
    }

    protected override void OnTemplateChanged(AvaloniaPropertyChangedEventArgs change)
    {
        ReleaseDesktopTemplateResources();
        base.OnTemplateChanged(change);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        ReleaseDesktopTemplateResources();
        // Root BorderBrush / Background reach the input frame through the template's HostBorderBrush / HostBackground
        // bindings, which the frame theme projects over its state brushes (antd styles.root).
        SetupTextViewportMetrics(e.NameScope.Find<ScrollViewer>("PART_ScrollViewer") ?? e.NameScope.Find<ScrollViewer>("ScrollViewer"));
    }

    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e) => base.OnAttachedToLogicalTree(e);

    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e) => base.OnDetachedFromLogicalTree(e);

    protected override NavigationMethod? GetClearButtonFocusReturnMethod(AvaloniaButton? button) =>
        (button as InputClearIconButton)?.GetFocusReturnMethod();

    private void ReleaseDesktopTemplateResources()
    {
        _textViewportSubscription?.Dispose();
        _textViewportSubscription = null;
        TextViewportMetrics.SetViewportWidth(this, null);
    }

    protected virtual double GetBorderThicknessForCompactSpace()
    {
        return CompactSpaceOrientation == Orientation.Horizontal ? BorderThickness.Left : BorderThickness.Top;
    }

    void ICompactSpaceAware.NotifyPositionChange(SpaceItemPosition? position)
    {
        SetCurrentValue(IsUsedInCompactSpaceProperty, position is not null);
        SetCurrentValue(CompactSpaceItemPositionProperty, position);
    }

    void ICompactSpaceAware.NotifyOrientationChange(Orientation orientation)
    {
        SetCurrentValue(CompactSpaceOrientationProperty, orientation);
    }

    double ICompactSpaceAware.GetBorderThickness() => GetBorderThicknessForCompactSpace();

    private void SetupTextViewportMetrics(ScrollViewer? scrollViewer)
    {
        _textViewportSubscription?.Dispose();
        _textViewportSubscription = null;
        TextViewportMetrics.SetViewportWidth(this, null);
        if (scrollViewer is not null)
        {
            _textViewportSubscription = TextViewportMetrics.PublishViewportWidth(this, scrollViewer, InputTextPresenter);
        }
    }

}
