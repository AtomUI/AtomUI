using AtomUI.Controls;
using AtomUI.Icons.AntDesign;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Input;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.LogicalTree;

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
    /// <summary>
    /// Identifies the <see cref="SizeType" /> property.
    /// </summary>
    public static readonly StyledProperty<CustomizableSizeType> SizeTypeProperty =
        CustomizableSizeTypeControlProperty.SizeTypeProperty.AddOwner<AbstractTextInput>();

    /// <summary>
    /// Identifies the <see cref="StyleVariant" /> property.
    /// </summary>
    public static readonly StyledProperty<InputControlStyleVariant> StyleVariantProperty =
        InputControlStyleVariantProperty.StyleVariantProperty.AddOwner<AbstractTextInput>();

    /// <summary>
    /// Identifies the <see cref="IsCustomFontSize" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsCustomFontSizeProperty =
        AvaloniaProperty.Register<AbstractTextInput, bool>(nameof(IsCustomFontSize));

    /// <summary>
    /// Identifies the <see cref="Status" /> property.
    /// </summary>
    public new static readonly StyledProperty<InputControlStatus> StatusProperty =
        SharedTextInput.StatusProperty.AddOwner<AbstractTextInput>();

    /// <summary>
    /// Identifies the <see cref="IsAllowClear" /> property.
    /// </summary>
    public new static readonly StyledProperty<bool> IsAllowClearProperty =
        SharedTextInput.IsAllowClearProperty.AddOwner<AbstractTextInput>();

    /// <summary>
    /// Identifies the <see cref="IsEnableRevealButton" /> property.
    /// </summary>
    public new static readonly StyledProperty<bool> IsEnableRevealButtonProperty =
        SharedTextInput.IsEnableRevealButtonProperty.AddOwner<AbstractTextInput>();

    /// <summary>
    /// Identifies the <see cref="IsShowCount" /> property.
    /// </summary>
    public new static readonly StyledProperty<bool> IsShowCountProperty =
        SharedTextInput.IsShowCountProperty.AddOwner<AbstractTextInput>();

    /// <summary>
    /// Identifies the <see cref="ClearIcon" /> property.
    /// </summary>
    public new static readonly StyledProperty<PathIcon?> ClearIconProperty =
        SharedTextInput.ClearIconProperty.AddOwner<AbstractTextInput>();

    /// <summary>
    /// Identifies the <see cref="IsMotionEnabled" /> property.
    /// </summary>
    public new static readonly StyledProperty<bool> IsMotionEnabledProperty =
        SharedTextInput.IsMotionEnabledProperty.AddOwner<AbstractTextInput>();

    /// <summary>
    /// Size preset of the input, <see cref="CustomizableSizeType.Middle" /> by default.
    /// <see cref="CustomizableSizeType.Large" />, <see cref="CustomizableSizeType.Middle" /> and
    /// <see cref="CustomizableSizeType.Small" /> make the theme take the content padding, the line height, the
    /// corner radius and the font size from the Tokens of that size.
    /// <see cref="CustomizableSizeType.Custom" /> sets no padding, so the control keeps the padding set on it, and
    /// uses the middle line height, corner radius and font size.
    /// </summary>
    public CustomizableSizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }

    /// <summary>
    /// Style variant of the input frame, <see cref="InputControlStyleVariant.Outlined" /> by default.
    /// <see cref="InputControlStyleVariant.Filled" /> fills the background without a visible border,
    /// <see cref="InputControlStyleVariant.Borderless" /> draws no border and
    /// <see cref="InputControlStyleVariant.Underlined" /> draws a border on the bottom edge only.
    /// </summary>
    public InputControlStyleVariant StyleVariant
    {
        get => GetValue(StyleVariantProperty);
        set => SetValue(StyleVariantProperty, value);
    }

    /// <summary>
    /// <see langword="true" /> keeps the font size that the control has locally or inherits, so the
    /// <see cref="SizeType" /> preset sets no font size. The default, <see langword="false" />, lets the preset set
    /// the font size.
    /// </summary>
    public bool IsCustomFontSize
    {
        get => GetValue(IsCustomFontSizeProperty);
        set => SetValue(IsCustomFontSizeProperty, value);
    }

    /// <summary>
    /// Validation state painted on the input frame: <see cref="InputControlStatus.Warning" /> or
    /// <see cref="InputControlStatus.Error" /> paints the frame in that state, and
    /// <see cref="InputControlStatus.Default" /> leaves it neutral. Data validation errors and an error reported by
    /// the form take precedence and force the error state; a warning reported by the form forces the warning state.
    /// </summary>
    public new InputControlStatus Status
    {
        get => base.Status;
        set => base.Status = value;
    }

    /// <summary>
    /// <see langword="true" /> lets a clear button appear in the input. The button is visible only while the text
    /// can be cleared: the input is not read-only, the text is not empty and, for multi-line input,
    /// <c>AllowsClearForMultilineInput</c> is <see langword="true" />. With <c>IsClearButtonOnlyWhenFocused</c> set,
    /// the button is visible only while the input has keyboard focus. Clicking it clears the text and raises
    /// <c>Cleared</c>.
    /// </summary>
    public new bool IsAllowClear
    {
        get => base.IsAllowClear;
        set => base.IsAllowClear = value;
    }

    /// <summary>
    /// <see langword="true" /> shows the reveal button of the theme, which toggles <c>RevealPassword</c> with its
    /// checked state. Themes without a reveal button ignore the value.
    /// </summary>
    public new bool IsEnableRevealButton
    {
        get => base.IsEnableRevealButton;
        set => base.IsEnableRevealButton = value;
    }

    /// <summary>
    /// <see langword="true" /> shows a counter with the length of the text and <c>MaxLength</c>, separated by a
    /// slash. The length is measured in the unit selected by <c>TextLengthMode</c>.
    /// </summary>
    public new bool IsShowCount
    {
        get => base.IsShowCount;
        set => base.IsShowCount = value;
    }

    /// <summary>
    /// Icon of the clear button. A <see langword="null" /> value is replaced with <see cref="CloseCircleFilled" />
    /// when the control is initialized and, while <see cref="IsAllowClear" /> is <see langword="true" />, also when
    /// <see cref="IsAllowClear" /> changes to <see langword="true" />.
    /// </summary>
    public new PathIcon? ClearIcon
    {
        get => base.ClearIcon;
        set => base.ClearIcon = value;
    }

    /// <summary>
    /// <see langword="true" /> enables the animated state transitions of the input frame, the clear button and the
    /// reveal button. When the value is not set, the theme supplies it from the <c>EnableMotion</c> token.
    /// </summary>
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

    /// <summary>
    /// The <c>PART_TextPresenter</c> part of the current template, or <see langword="null" /> while no such part is
    /// attached. It is the presenter of the base class, re-exposed for desktop text controls.
    /// </summary>
    protected new TextPresenter? InputTextPresenter => base.InputTextPresenter;

    /// <summary>
    /// The styled property that holds the template of the content placed after the text inside the input, or
    /// <see langword="null" /> when the control has no such property. The base implementation returns
    /// <see langword="null" />; <c>LineEdit</c> and <c>TextArea</c> override it.
    /// </summary>
    protected virtual AvaloniaProperty? InnerRightContentTemplatePropertyForBinding => null;

    /// <inheritdoc />
    protected override bool CanClearText => base.CanClearText;
    /// <inheritdoc />
    protected override bool AllowsClearForMultilineInput => base.AllowsClearForMultilineInput;
    /// <inheritdoc />
    protected override void NotifyClearButtonClicked() => base.NotifyClearButtonClicked();
    /// <inheritdoc />
    protected override void NotifySetFormValue(string? value) => base.NotifySetFormValue(value);
    /// <inheritdoc />
    protected override string? NotifyGetFormValue() => base.NotifyGetFormValue();
    /// <inheritdoc />
    protected override void NotifyClearFormValue() => base.NotifyClearFormValue();
    /// <inheritdoc />
    protected override void NotifyValidateStatus(FormValidateStatus status) => base.NotifyValidateStatus(status);
    /// <inheritdoc />
    protected override void NotifySetFeedBackControl(FormValidateFeedback? value) => base.NotifySetFeedBackControl(value);

    internal void NotifyTextViewportCreated(ScrollViewer scrollViewer) => SetupTextViewportMetrics(scrollViewer);

    /// <summary>
    /// Calls the base implementation and then, when <see cref="ClearIcon" /> is still <see langword="null" />, sets
    /// it to a <see cref="CloseCircleFilled" /> icon as the default. An override must call the base implementation
    /// to get the default icon.
    /// </summary>
    protected override void OnInitialized()
    {
        base.OnInitialized();
        if (ClearIcon is null)
        {
            SetCurrentValue(ClearIconProperty, new CloseCircleFilled());
        }
    }

    /// <summary>
    /// Stops publishing the viewport width of the previous template before the base implementation handles the
    /// change. An override must call the base implementation.
    /// </summary>
    protected override void OnTemplateChanged(AvaloniaPropertyChangedEventArgs change)
    {
        ReleaseDesktopTemplateResources();
        base.OnTemplateChanged(change);
    }

    /// <summary>
    /// Calls the base implementation, then discards the viewport width published so far and starts publishing the
    /// viewport width of the ScrollViewer that the template provides (<c>PART_ScrollViewer</c>, or
    /// <c>ScrollViewer</c>). An override must call the base implementation.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        ReleaseDesktopTemplateResources();
        // Root BorderBrush / Background reach the input frame through the template's HostBorderBrush / HostBackground
        // bindings, which the frame theme projects over its state brushes (antd styles.root).
        SetupTextViewportMetrics(e.NameScope.Find<ScrollViewer>("PART_ScrollViewer") ?? e.NameScope.Find<ScrollViewer>("ScrollViewer"));
    }

    /// <inheritdoc />
    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e) => base.OnAttachedToLogicalTree(e);

    /// <inheritdoc />
    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e) => base.OnDetachedFromLogicalTree(e);

    /// <summary>
    /// Returns how focus moves back to the input after its clear button was clicked, so that the focus does not drop
    /// when the button hides: the method reported by the <see cref="InputClearIconButton" />. It is
    /// <see langword="null" />, which leaves the focus where it is, when the button is of another type or does not
    /// hold the focus.
    /// </summary>
    protected override NavigationMethod? GetClearButtonFocusReturnMethod(AvaloniaButton? button) =>
        (button as InputClearIconButton)?.GetFocusReturnMethod();

    private void ReleaseDesktopTemplateResources()
    {
        _textViewportSubscription?.Dispose();
        _textViewportSubscription = null;
        TextViewportMetrics.SetViewportWidth(this, null);
    }

    /// <summary>
    /// Returns the border width that the control reports while it is an item of a compact <c>Space</c>, which reads
    /// it through <c>ICompactSpaceAware.GetBorderThickness</c>: the left width of <c>BorderThickness</c> while the
    /// space lays out horizontally and the top width while it lays out vertically. An override can return the width
    /// of another border without calling this implementation.
    /// </summary>
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
