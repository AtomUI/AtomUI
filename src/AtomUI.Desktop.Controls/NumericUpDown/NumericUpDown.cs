using System.Reactive.Disposables;
using System.Reactive.Linq;
using AtomUI.Controls;
using AtomUI.Data;
using AtomUI.Icons.AntDesign;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Metadata;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// The layout of the text area of a <see cref="NumericUpDown" />, in particular where the <c>-</c> and <c>+</c>
/// handles sit.
/// </summary>
public enum NumericUpDownMode
{
    /// <summary>
    /// Left-aligned text. The <c>-</c> and <c>+</c> handles stay hidden until the pointer is over the control and
    /// then slide in, pushing the content aside.
    /// </summary>
    Input,
    /// <summary>
    /// Centered text between the <c>-</c> and <c>+</c> handles, which stay in view and keep their own space. The
    /// layout is used only while <c>ShowButtonSpinner</c> is <see langword="true" />; otherwise the control uses the
    /// <see cref="Input" /> layout.
    /// </summary>
    Spinner
}

/// <summary>
/// The numeric up-down control: a text box in a frame with an optional clear button, add-on segments outside the
/// frame and content slots inside it, with <c>-</c> and <c>+</c> handles while <c>ShowButtonSpinner</c> is
/// <see langword="true" />. Parsing, range checks and validation come from <see cref="AbstractNumericUpDown" />.
/// </summary>
public partial class NumericUpDown : AbstractNumericUpDown,
                                     IMotionAwareControl,
                                     ICompactSpaceAware,
                                     IInputControlStyleVariantAware,
                                     ICustomizableSizeTypeAware
{
    #region 公共属性定义
    /// <summary>
    /// Identifies the <see cref="ClearIcon" /> property.
    /// </summary>
    public static readonly StyledProperty<PathIcon?> ClearIconProperty =
        AvaloniaProperty.Register<NumericUpDown, PathIcon?>(nameof(ClearIcon));

    /// <summary>
    /// Identifies the <see cref="LeftAddOn" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> LeftAddOnProperty =
        AddOnDecoratedBox.LeftAddOnProperty.AddOwner<NumericUpDown>();
    
    /// <summary>
    /// Identifies the <see cref="LeftAddOnTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> LeftAddOnTemplateProperty =
        AddOnDecoratedBox.LeftAddOnTemplateProperty.AddOwner<NumericUpDown>();

    /// <summary>
    /// Identifies the <see cref="RightAddOn" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> RightAddOnProperty =
        AddOnDecoratedBox.RightAddOnProperty.AddOwner<NumericUpDown>();
    
    /// <summary>
    /// Identifies the <see cref="RightAddOnTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> RightAddOnTemplateProperty =
        AddOnDecoratedBox.RightAddOnTemplateProperty.AddOwner<NumericUpDown>();
    
    /// <summary>
    /// Identifies the <see cref="InnerLeftContentTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> InnerLeftContentTemplateProperty =
        AvaloniaProperty.Register<NumericUpDown, IDataTemplate?>(nameof(InnerLeftContentTemplate));
    
    /// <summary>
    /// Identifies the <see cref="InnerRightContentTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> InnerRightContentTemplateProperty =
        AvaloniaProperty.Register<NumericUpDown, IDataTemplate?>(nameof(InnerRightContentTemplate));

    /// <summary>
    /// Identifies the <see cref="SizeType" /> property.
    /// </summary>
    public static readonly StyledProperty<CustomizableSizeType> SizeTypeProperty =
        CustomizableSizeTypeControlProperty.SizeTypeProperty.AddOwner<NumericUpDown>();

    /// <summary>
    /// Identifies the <see cref="StyleVariant" /> property.
    /// </summary>
    public static readonly StyledProperty<InputControlStyleVariant> StyleVariantProperty =
        InputControlStyleVariantProperty.StyleVariantProperty.AddOwner<NumericUpDown>();

    /// <summary>
    /// Identifies the <see cref="Status" /> property.
    /// </summary>
    public new static readonly StyledProperty<InputControlStatus> StatusProperty =
        AbstractNumericUpDown.StatusProperty.AddOwner<NumericUpDown>();

    /// <summary>
    /// Identifies the <see cref="IsAllowClear" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsAllowClearProperty =
        TextBox.IsAllowClearProperty.AddOwner<NumericUpDown>();

    /// <summary>
    /// Identifies the <see cref="IsMotionEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<NumericUpDown>();

    /// <summary>
    /// Identifies the <see cref="IsStringMode" /> property.
    /// </summary>
    public new static readonly StyledProperty<bool> IsStringModeProperty =
        AbstractNumericUpDown.IsStringModeProperty.AddOwner<NumericUpDown>();
    
    /// <summary>
    /// Identifies the <see cref="IsKeyboardEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsKeyboardEnabledProperty =
        AvaloniaProperty.Register<NumericUpDown, bool>(nameof(IsKeyboardEnabled), true);

    /// <summary>
    /// Identifies the <see cref="Mode" /> property.
    /// </summary>
    public static readonly StyledProperty<NumericUpDownMode> ModeProperty =
        AvaloniaProperty.Register<NumericUpDown, NumericUpDownMode>(nameof(Mode), NumericUpDownMode.Input);

    /// <summary>
    /// Identifies the <see cref="StringValue" /> property.
    /// </summary>
    public new static readonly StyledProperty<string?> StringValueProperty =
        AbstractNumericUpDown.StringValueProperty.AddOwner<NumericUpDown>();

    /// <summary>
    /// Icon of the clear button. When it is <see langword="null" /> when the control is initialized, a
    /// <see cref="CloseCircleFilled" /> icon is set as the current value in its place.
    /// </summary>
    public PathIcon? ClearIcon
    {
        get => GetValue(ClearIconProperty);
        set => SetValue(ClearIconProperty, value);
    }

    /// <summary>
    /// Content of the add-on segment attached before the input frame. It is presented with
    /// <see cref="LeftAddOnTemplate" /> when that is set.
    /// </summary>
    [DependsOn(nameof(LeftAddOnTemplate))]
    public object? LeftAddOn
    {
        get => GetValue(LeftAddOnProperty);
        set => SetValue(LeftAddOnProperty, value);
    }
    
    /// <summary>
    /// The data template used to present <see cref="LeftAddOn" />.
    /// </summary>
    public object? LeftAddOnTemplate
    {
        get => GetValue(LeftAddOnTemplateProperty);
        set => SetValue(LeftAddOnTemplateProperty, value);
    }

    /// <summary>
    /// Content of the add-on segment attached after the input frame. It is presented with
    /// <see cref="RightAddOnTemplate" /> when that is set.
    /// </summary>
    [DependsOn(nameof(RightAddOnTemplate))]
    public object? RightAddOn
    {
        get => GetValue(RightAddOnProperty);
        set => SetValue(RightAddOnProperty, value);
    }
    
    /// <summary>
    /// The data template used to present <see cref="RightAddOn" />.
    /// </summary>
    public object? RightAddOnTemplate
    {
        get => GetValue(RightAddOnTemplateProperty);
        set => SetValue(RightAddOnTemplateProperty, value);
    }
    
    /// <summary>
    /// The data template used to present the inner left content, a prefix inside the frame before the text; in the
    /// <see cref="NumericUpDownMode.Spinner" /> layout it sits between the two handles.
    /// </summary>
    public object? InnerLeftContentTemplate
    {
        get => GetValue(InnerLeftContentTemplateProperty);
        set => SetValue(InnerLeftContentTemplateProperty, value);
    }
    
    /// <summary>
    /// The data template used to present the inner right content, which sits inside the frame after the text and the
    /// clear button.
    /// </summary>
    public object? InnerRightContentTemplate
    {
        get => GetValue(InnerRightContentTemplateProperty);
        set => SetValue(InnerRightContentTemplateProperty, value);
    }

    /// <summary>
    /// Size preset of the control. It is passed to the spinner of the template, which sizes the frame from it, and it
    /// selects the font size of the text while <see cref="IsCustomFontSize" /> is <see langword="false" />.
    /// </summary>
    public CustomizableSizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }

    /// <summary>
    /// Style variant of the input frame. It is passed to the spinner of the template, which paints the frame in that
    /// variant.
    /// </summary>
    public InputControlStyleVariant StyleVariant
    {
        get => GetValue(StyleVariantProperty);
        set => SetValue(StyleVariantProperty, value);
    }

    /// <summary>
    /// Validation state painted on the input frame. The contract is that of
    /// <see cref="AbstractNumericUpDown.Status" />.
    /// </summary>
    public new InputControlStatus Status
    {
        get => GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }

    /// <summary>
    /// <see langword="true" /> shows a clear button inside the frame. It is visible only while the control is not
    /// read-only and its text is not empty; clicking it sets the value to <see langword="null" /> and moves focus
    /// back to the text box.
    /// </summary>
    public bool IsAllowClear
    {
        get => GetValue(IsAllowClearProperty);
        set => SetValue(IsAllowClearProperty, value);
    }
    
    /// <summary>
    /// Controls the motion of the template's parts. The value is passed to the spinner and to the clear button. The
    /// theme sets it from the <c>EnableMotion</c> token when no value is set on the control.
    /// </summary>
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    /// <summary>
    /// <see langword="true" /> keeps the text as entered in <see cref="StringValue" />, next to the parsed value. The
    /// contract is that of <see cref="AbstractNumericUpDown.IsStringMode" />.
    /// </summary>
    public new bool IsStringMode
    {
        get => GetValue(IsStringModeProperty);
        set => SetValue(IsStringModeProperty, value);
    }

    /// <summary>
    /// Controls whether the Up, Down, PageUp and PageDown keys change the value. With <see langword="false" /> those
    /// keys are marked handled on the control and on its text box; typing is not affected.
    /// </summary>
    public bool IsKeyboardEnabled
    {
        get => GetValue(IsKeyboardEnabledProperty);
        set => SetValue(IsKeyboardEnabledProperty, value);
    }

    /// <summary>
    /// Layout of the text area; <see cref="NumericUpDownMode.Input" /> when not set. The Spinner layout is used only
    /// while <c>ShowButtonSpinner</c> is <see langword="true" />.
    /// </summary>
    public NumericUpDownMode Mode
    {
        get => GetValue(ModeProperty);
        set => SetValue(ModeProperty, value);
    }

    /// <summary>
    /// The text as it was entered, used only while <see cref="IsStringMode" /> is <see langword="true" />. The
    /// contract is that of <see cref="AbstractNumericUpDown.StringValue" />.
    /// </summary>
    public new string? StringValue
    {
        get => GetValue(StringValueProperty);
        set => SetValue(StringValueProperty, value);
    }

    /// <summary>
    /// <see langword="true" /> keeps the font size set on the control or inherited by it, so that the size presets no
    /// longer change the text size.
    /// </summary>
    public bool IsCustomFontSize
    {
        get => GetValue(IsCustomFontSizeProperty);
        set => SetValue(IsCustomFontSizeProperty, value);
    }

    #endregion
    
    #region 内部属性定义

    internal static readonly StyledProperty<double> SpinnerHandleWidthProperty =
        ButtonSpinner.SpinnerHandleWidthProperty.AddOwner<NumericUpDown>();
    
    internal static readonly StyledProperty<bool> IsCustomFontSizeProperty =
        AvaloniaProperty.Register<NumericUpDown, bool>(nameof(IsCustomFontSize));
    
    internal static readonly DirectProperty<NumericUpDown, bool> IsEffectiveShowClearButtonProperty =
        AvaloniaProperty.RegisterDirect<NumericUpDown, bool>(nameof(IsEffectiveShowClearButton),
            o => o.IsEffectiveShowClearButton,
            (o, v) => o.IsEffectiveShowClearButton = v);
    
    internal static readonly StyledProperty<SpaceItemPosition?> CompactSpaceItemPositionProperty = 
        CompactSpaceAwareControlProperty.CompactSpaceItemPositionProperty.AddOwner<NumericUpDown>();
    
    internal static readonly StyledProperty<Orientation> CompactSpaceOrientationProperty = 
        CompactSpaceAwareControlProperty.CompactSpaceOrientationProperty.AddOwner<NumericUpDown>();
    
    internal static readonly StyledProperty<bool> IsUsedInCompactSpaceProperty = 
        CompactSpaceAwareControlProperty.IsUsedInCompactSpaceProperty.AddOwner<NumericUpDown>();

    /// <summary>
    /// Width of the spinner handle, in device-independent pixels. The theme takes it from the <c>HandleWidth</c> token
    /// of the control and passes it to the decorated input frames of the template, which reserve this width for the
    /// spinner handle next to the content.
    /// </summary>
    internal double SpinnerHandleWidth
    {
        get => GetValue(SpinnerHandleWidthProperty);
        set => SetValue(SpinnerHandleWidthProperty, value);
    }
    
    private bool _isEffectiveShowClearButton;

    internal bool IsEffectiveShowClearButton
    {
        get => _isEffectiveShowClearButton;
        set => SetAndRaise(IsEffectiveShowClearButtonProperty, ref _isEffectiveShowClearButton, value);
    }
    
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

    #endregion
    
    private IconButton? _clearButton;
    private TextBox? _textBoxPart;
    private CompositeDisposable? _templatePartBindings;
    private ButtonSpinner? _buttonSpinner;

    private IconButton? ClearButtonPart
    {
        get => _clearButton;
        set
        {
            if (ReferenceEquals(_clearButton, value))
            {
                return;
            }

            if (_clearButton is not null)
            {
                _clearButton.Click -= HandleClearButtonClicked;
            }

            _clearButton = value;

            if (_clearButton is not null)
            {
                _clearButton.Click += HandleClearButtonClicked;
            }
        }
    }

    private TextBox? TextBoxPart
    {
        get => _textBoxPart;
        set
        {
            if (ReferenceEquals(_textBoxPart, value))
            {
                return;
            }

            if (_textBoxPart is not null)
            {
                _textBoxPart.KeyDown -= HandleTextBoxKeyDown;
            }

            _textBoxPart = value;

            if (_textBoxPart is not null)
            {
                _textBoxPart.KeyDown += HandleTextBoxKeyDown;
            }
        }
    }

    public NumericUpDown()
    {
    }

    /// <summary>
    /// Returns <see langword="false" />, which keeps the text commit policy of <see cref="AbstractNumericUpDown" />
    /// off unless <see cref="AbstractNumericUpDown.Digits" /> is set or empty text is not allowed. Without the policy,
    /// text that cannot be parsed is left to the Avalonia base class: it is not committed and raises no error.
    /// </summary>
    protected override bool ReportsTextConversionErrors => false;

    /// <summary>
    /// Sets a <see cref="CloseCircleFilled" /> icon as the current value of <see cref="ClearIcon" /> when the control
    /// is initialized without one. An override must call this member for the default icon to be applied.
    /// </summary>
    protected override void OnInitialized()
    {
        base.OnInitialized();
        if (ClearIcon == null)
        {
            SetCurrentValue(ClearIconProperty, new CloseCircleFilled());
        }
    }

    /// <summary>
    /// Locates the clear button and the text box of the template and subscribes to their click and key down events.
    /// It detaches the text box from the relayed <see cref="KeyboardNavigation.TabIndexProperty" /> so that the tab
    /// order of the template holds, updates the visibility of the clear button and binds the template parts to
    /// this control once the spinner's own template is applied, which the bindings reach into. An override must call
    /// this member.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        ClearButtonPart = e.NameScope.Find<IconButton>("PART_ClearButton");
        TextBoxPart     = e.NameScope.Find<TextBox>("PART_TextBox");
        // Avalonia relays the host TabIndex to the text box with a local binding. The host is a local tab group that
        // carries that index itself; inside it the template order (input 0, then the clear button 1, which precedes
        // the input in the visual tree) must hold. A new local binding replaces the relay, and an empty one completes
        // at once, so the text box falls back to its template TabIndex.
        TextBoxPart?.Bind(KeyboardNavigation.TabIndexProperty, Observable.Empty<int>());
        ConfigureEffectiveShowClearButton();
        if (e.NameScope.Find<ButtonSpinner>("PART_Spinner") is { } buttonSpinner)
        {
            _buttonSpinner = buttonSpinner;
            // The frame node lives inside the spinner's own template; apply it now so the
            // template part bindings below can reach the decorated box.
            buttonSpinner.ApplyTemplate();
        }
        // The root BorderBrush is template-bound onto the spinner, which binds it into the frame's HostBorderBrush.
        SetupTemplatePartBindings(e);
    }

    /// <summary>
    /// Called when the clear button is clicked, before focus returns to the text box. The default implementation sets
    /// the value to <see langword="null" />; an override that should still clear it must call this member.
    /// </summary>
    protected virtual void NotifyClearButtonClicked()
    {
        Value = null;
    }

    /// <summary>
    /// Recomputes the visibility of the clear button after <c>IsReadOnly</c>, <c>Text</c> or
    /// <see cref="IsAllowClear" /> changes. An override must call this member.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IsReadOnlyProperty ||
            change.Property == TextProperty ||
            change.Property == IsAllowClearProperty)
        {
            ConfigureEffectiveShowClearButton();
        }

    }

    /// <inheritdoc />
    protected override void OnTextChanged(string? oldValue, string? newValue) => base.OnTextChanged(oldValue, newValue);

    /// <inheritdoc />
    protected override void OnValueChanged(decimal? oldValue, decimal? newValue) => base.OnValueChanged(oldValue, newValue);

    /// <summary>
    /// While <see cref="IsKeyboardEnabled" /> is <see langword="false" />, marks Up, Down, PageUp and PageDown as
    /// handled without calling the base member, so that those keys do not change the value. Every other key goes to
    /// the base member.
    /// </summary>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (!IsKeyboardEnabled)
        {
            switch (e.Key)
            {
                case Key.Up:
                case Key.Down:
                case Key.PageUp:
                case Key.PageDown:
                    e.Handled = true;
                    return;
            }
        }

        base.OnKeyDown(e);
    }

    #region 实现 CompactSpace 接口

    void ICompactSpaceAware.NotifyPositionChange(SpaceItemPosition? position)
    {
        var isUsedInCompactSpace = position != null;
        if (IsUsedInCompactSpace != isUsedInCompactSpace)
        {
            IsUsedInCompactSpace = isUsedInCompactSpace;
        }

        if (CompactSpaceItemPosition != position)
        {
            CompactSpaceItemPosition = position;
        }
    }

    void ICompactSpaceAware.NotifyOrientationChange(Orientation orientation)
    {
        if (CompactSpaceOrientation != orientation)
        {
            CompactSpaceOrientation = orientation;
        }
    }

    double ICompactSpaceAware.GetBorderThickness()
    {
        if (!IsUsedInCompactSpace)
        {
            return 0.0;
        }

        var addOnDecoratedBox = _buttonSpinner?.DecoratedBox;
        if (addOnDecoratedBox == null)
        {
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel != null)
            {
                topLevel.UpdateLayout();
                addOnDecoratedBox = _buttonSpinner?.DecoratedBox;
            }
        }

        if (addOnDecoratedBox == null || StyleVariant != InputControlStyleVariant.Outlined)
        {
            return 0.0;
        }

        // All four sides are equally wide.
        return addOnDecoratedBox.InnerBoxBorderThickness.Left;
    }

    #endregion

    /// <inheritdoc />
    protected override void NotifySetFormValue(decimal? value) => base.NotifySetFormValue(value);

    /// <inheritdoc />
    protected override decimal? NotifyGetFormValue() => base.NotifyGetFormValue();

    /// <inheritdoc />
    protected override void NotifyClearFormValue() => base.NotifyClearFormValue();

    /// <inheritdoc />
    protected override void NotifyValidateStatus(FormValidateStatus status) => base.NotifyValidateStatus(status);

    private void HandleClearButtonClicked(object? sender, RoutedEventArgs args)
    {
        // Focus returns to the input, as in Ant Design; the button hides once the value is gone.
        var focusReturnMethod = (sender as InputClearIconButton)?.GetFocusReturnMethod();
        NotifyClearButtonClicked();
        if (focusReturnMethod is { } navigationMethod)
        {
            _textBoxPart?.Focus(navigationMethod);
        }
    }

    private void SetupTemplatePartBindings(TemplateAppliedEventArgs e)
    {
        _templatePartBindings?.Dispose();
        _templatePartBindings = new CompositeDisposable();

        if (e.NameScope.Find<InputClearIconButton>("PART_ClearButton") is { } clearButton)
        {
            // Only the visibility relay stays in code: IsEffectiveShowClearButton is an internal
            // direct property, invisible to compiled template bindings. Icon and IsMotionEnabled
            // are template-bound in the ControlTheme.
            _templatePartBindings.Add(BindUtils.RelayBind(this, IsEffectiveShowClearButtonProperty, clearButton,
                Visual.IsVisibleProperty));
        }

        if (e.NameScope.Find<ContentPresenter>("PART_InnerLeftContentPresenter") is { } innerLeftContent)
        {
            // The presenter lives in a ButtonSpinner.InnerLeftContent property-element subtree, where
            // TemplatedParent never propagates, so template bindings stay dead there; relay in code.
            _templatePartBindings.Add(BindUtils.RelayBind(this, InnerLeftContentProperty, innerLeftContent,
                ContentPresenter.ContentProperty));
            _templatePartBindings.Add(BindUtils.RelayBind(this, InnerLeftContentTemplateProperty,
                innerLeftContent, ContentPresenter.ContentTemplateProperty));
        }

        // InnerRightContent / InnerRightContentTemplate and the text box IsCustomFontSize are
        // template-bound in the ControlTheme (the parts sit in direct template content where
        // TemplatedParent propagates), so no code relay is needed for them.

        if (e.NameScope.Find<NumericUpDownInputPanel>("PART_InputPanel") is { } inputPanel &&
            _buttonSpinner?.DecoratedBox is { } decoratedBox)
        {
            // The animated source is internal state of the spinner's nested decorated-box
            // template, outside this Content subtree's TemplateBinding scope. Keep this
            // binding scoped to the current template and consume the existing animation;
            // the panel must not start a second transition or translate the suffix again.
            _templatePartBindings.Add(inputPanel.Bind(NumericUpDownInputPanel.LeftInsetProperty,
                decoratedBox.GetObservable(ButtonSpinnerDecoratedBox.ContentLeftShiftProperty)));
            _templatePartBindings.Add(inputPanel.Bind(NumericUpDownInputPanel.RightInsetProperty,
                decoratedBox.GetObservable(ButtonSpinnerDecoratedBox.ContentRightShiftProperty)
                            .Select(static shift => Math.Max(0d, -shift))));
        }
    }

    private void HandleTextBoxKeyDown(object? sender, KeyEventArgs e)
    {
        if (!IsKeyboardEnabled)
        {
            switch (e.Key)
            {
                case Key.Up:
                case Key.Down:
                case Key.PageUp:
                case Key.PageDown:
                    e.Handled = true;
                    break;
            }
        }
    }

    private void ConfigureEffectiveShowClearButton()
    {
        if (!IsAllowClear)
        {
            SetCurrentValue(IsEffectiveShowClearButtonProperty, false);
            return;
        }
        
        SetCurrentValue(IsEffectiveShowClearButtonProperty, !IsReadOnly && !string.IsNullOrEmpty(Text));
    }
}
