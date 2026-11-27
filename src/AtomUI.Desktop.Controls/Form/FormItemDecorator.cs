using System.Reactive.Disposables;
using AtomUI.Controls;
using AtomUI.Data;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;
using AtomUI.Theme;

namespace AtomUI.Desktop.Controls;

// Token-owner transparent part: the decorator has no Own Tokens and its theme reads the Form TokenResource.
/// <summary>
/// Wraps the single input control given as <see cref="Child" />, so that a form item can use it as its value
/// control, and shows optional <see cref="Extra" /> content next to it.
/// Form value and validation calls are forwarded to the child, and the value changes of the child are reported to
/// the form item.
/// <see cref="SizeType" />, <see cref="IsMotionEnabled" /> and <see cref="StyleVariant" /> are relayed to the child.
/// </summary>
[TokenOwnerTransparent]
public class FormItemDecorator : TemplatedControl,
                                 IInputControlStatusAware,
                                 IInputControlStyleVariantAware,
                                 IMotionAwareControl,
                                 ICustomizableSizeTypeAware,
                                 IFormItemAware,
                                 IFormItemFeedbackAware
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="Child" /> property.
    /// </summary>
    public static readonly StyledProperty<Control?> ChildProperty =
        AvaloniaProperty.Register<FormItemDecorator, Control?>(nameof(Child));
    
    /// <summary>
    /// Identifies the <see cref="StyleVariant" /> property.
    /// </summary>
    public static readonly StyledProperty<InputControlStyleVariant> StyleVariantProperty =
        InputControlStyleVariantProperty.StyleVariantProperty.AddOwner<FormItemDecorator>();

    /// <summary>
    /// Identifies the <see cref="Status" /> property.
    /// </summary>
    public static readonly StyledProperty<InputControlStatus> StatusProperty =
        InputControlStatusProperty.StatusProperty.AddOwner<FormItemDecorator>();
    
    /// <summary>
    /// Identifies the <see cref="SizeType" /> property.
    /// </summary>
    public static readonly StyledProperty<CustomizableSizeType> SizeTypeProperty =
        CustomizableSizeTypeControlProperty.SizeTypeProperty.AddOwner<FormItemDecorator>();
    
    /// <summary>
    /// Identifies the <see cref="IsMotionEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMotionEnabledProperty = 
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<FormItemDecorator>();
    
    /// <summary>
    /// Identifies the <see cref="Extra" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> ExtraProperty =
        AvaloniaProperty.Register<FormItemDecorator, object?>(nameof(Extra));

    /// <summary>
    /// Identifies the <see cref="ExtraTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> ExtraTemplateProperty =
        AvaloniaProperty.Register<FormItemDecorator, IDataTemplate?>(nameof(ExtraTemplate));
    
    /// <summary>
    /// Identifies the <see cref="ItemSpacing" /> property.
    /// </summary>
    public static readonly StyledProperty<double> ItemSpacingProperty =
        AvaloniaProperty.Register<FormItemDecorator, double>(nameof(ItemSpacing));
    
    /// <summary>
    /// The input control that the decorator wraps and that holds the form value. It is the XAML content property.
    /// </summary>
    /// <exception cref="System.Exception">
    /// The assigned value is <see langword="null" /> or does not implement <see cref="IFormItemAware" />.
    /// </exception>
    [Content]
    public Control? Child
    {
        get => GetValue(ChildProperty);
        set => SetValue(ChildProperty, value);
    }
    
    /// <summary>
    /// The frame style that <see cref="Child" /> is drawn in. The value is relayed to the child when it implements
    /// <see cref="IInputControlStyleVariantAware" />. The default theme of the decorator does not vary with it.
    /// </summary>
    public InputControlStyleVariant StyleVariant
    {
        get => GetValue(StyleVariantProperty);
        set => SetValue(StyleVariantProperty, value);
    }

    /// <summary>
    /// The validation state of the input control that contains the decorator.
    /// The decorator does not relay the value to <see cref="Child" />, and its default theme has no style that
    /// depends on it.
    /// </summary>
    public InputControlStatus Status
    {
        get => GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }

    /// <summary>
    /// The size that <see cref="Child" /> is drawn in. The default theme of the decorator does not vary with it.
    /// The value is relayed to the child: unchanged to a child that implements
    /// <see cref="ICustomizableSizeTypeAware" />, and mapped to the legacy <c>SizeType</c> of a child that implements
    /// <c>ISizeTypeAware</c>, where <see cref="CustomizableSizeType.Custom" /> becomes the middle size.
    /// </summary>
    public CustomizableSizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }
    
    /// <summary>
    /// Whether the motion of <see cref="Child" /> plays. The value is relayed to the child when it implements
    /// <see cref="IMotionAwareControl" />. The decorator itself has no motion.
    /// </summary>
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    /// <summary>
    /// The content shown next to <see cref="Child" />, vertically centered. Nothing is shown while it is
    /// <see langword="null" />.
    /// </summary>
    [DependsOn(nameof(ExtraTemplate))]
    public object? Extra
    {
        get => GetValue(ExtraProperty);
        set => SetValue(ExtraProperty, value);
    }
    
    /// <summary>
    /// The template that builds the visual of <see cref="Extra" />.
    /// While it is <see langword="null" />, an <see cref="Extra" /> that is a control is shown as it is; any other
    /// value uses the data template registered for its type.
    /// </summary>
    public IDataTemplate? ExtraTemplate
    {
        get => GetValue(ExtraTemplateProperty);
        set => SetValue(ExtraTemplateProperty, value);
    }
    
    /// <summary>
    /// The gap in DIPs between <see cref="Child" /> and the <see cref="Extra" /> content in the horizontal stack of
    /// the default theme. An unset value falls back to the <c>SpacingXS</c> token of the form.
    /// </summary>
    public double ItemSpacing
    {
        get => GetValue(ItemSpacingProperty);
        set => SetValue(ItemSpacingProperty, value);
    }
    #endregion

    #region 内部属性定义

    internal static readonly StyledProperty<FormValidateFeedback?> FormFeedbackProperty = 
        AvaloniaProperty.Register<FormItemDecorator, FormValidateFeedback?>(nameof(FormFeedback));
    
    internal FormValidateFeedback? FormFeedback
    {
        get => GetValue(FormFeedbackProperty);
        set => SetValue(FormFeedbackProperty, value);
    }

    #endregion

    private CompositeDisposable? _disposables;

    static FormItemDecorator()
    {
        AffectsMeasure<FormItemDecorator>(ChildProperty, PaddingProperty);
        ChildProperty.Changed.AddClassHandler<FormItemDecorator>((decorator, e) => decorator.HandleChildChanged(e));
    }
    
    #region 实现 FormItem 接口
    private EventHandler? _formValueChanged;
    event EventHandler? IFormItemAware.ValueChanged
    {
        add => _formValueChanged += value;
        remove => _formValueChanged -= value;
    }

    void IFormItemAware.SetFormValue(object? value) => NotifySetFormValue(value);

    object? IFormItemAware.GetFormValue() => NotifyGetFormValue();
    void IFormItemAware.ClearFormValue() => NotifyClearFormValue();
    void IFormItemAware.NotifyValidateStatus(FormValidateStatus status) => NotifyValidateStatus(status);
    void IFormItemFeedbackAware.SetFeedbackControl(FormValidateFeedback? value) => NotifySetFeedBackControl(value);

    /// <summary>
    /// Called when the form item writes a value that does not come from the user, and forwards it to
    /// <see cref="Child" />.
    /// </summary>
    /// <param name="value">
    /// The new value, of the type that the child returns as the form value.
    /// </param>
    /// <remarks>
    /// An override calls the base method to pass the value on to the child.
    /// </remarks>
    protected virtual void NotifySetFormValue(object? value)
    {
        if (Child is IFormItemAware formItemAware)
        {
            formItemAware.SetFormValue(value);
        }
    }

    /// <summary>
    /// Called when the form item reads the value of the decorator.
    /// Returns the value reported by <see cref="Child" />.
    /// </summary>
    /// <returns>
    /// The value of the child, or <see langword="null" /> while no child is set.
    /// </returns>
    /// <remarks>
    /// An override calls the base method to read the child.
    /// </remarks>
    protected virtual object? NotifyGetFormValue()
    {
        if (Child is IFormItemAware formItemAware)
        {
            return formItemAware.GetFormValue();
        }
        return null;
    }

    /// <summary>
    /// Called when the form item resets the decorator and has no initial value to restore. Forwards the call to
    /// <see cref="Child" />.
    /// </summary>
    /// <remarks>
    /// An override calls the base method to pass the call on to the child.
    /// </remarks>
    protected virtual void NotifyClearFormValue()
    {
        if (Child is IFormItemAware formItemAware)
        {
            formItemAware.ClearFormValue();
        }
    }

    /// <summary>
    /// Called whenever the form item updates its validation state. Tells <see cref="Child" /> the new status.
    /// </summary>
    /// <param name="status">
    /// The validation status of the form item. It is also <see cref="FormValidateStatus.Validating" /> while a
    /// validation runs.
    /// </param>
    /// <remarks>
    /// An override calls the base method to pass the status on to the child.
    /// </remarks>
    protected virtual void NotifyValidateStatus(FormValidateStatus status)
    {
        if (Child is IFormItemAware formItemAware)
        {
            formItemAware.NotifyValidateStatus(status);
        }
    }

    /// <summary>
    /// Called when the form item hands over the control that shows its validation messages. The control is kept in
    /// <c>FormFeedback</c>, so that a child assigned later receives it too, and is passed to <see cref="Child" />
    /// when the child implements <see cref="IFormItemFeedbackAware" />.
    /// </summary>
    /// <param name="value">
    /// The feedback control, or <see langword="null" /> when the form item provides none.
    /// </param>
    /// <remarks>
    /// An override calls the base method to pass the control on to the child.
    /// </remarks>
    protected virtual void NotifySetFeedBackControl(FormValidateFeedback? value)
    {
        FormFeedback = value;
        if (Child is IFormItemFeedbackAware formItemFeedbackAware)
        {
            formItemFeedbackAware.SetFeedbackControl(value);
        }
    }
    #endregion

    private void HandleChildChanged(AvaloniaPropertyChangedEventArgs e)
    {
        ReleaseChild(e.OldValue);
        AttachChild(e.NewValue);
    }

    private void AttachChild(object? child)
    {
        if (child is not IFormItemAware formItemAware)
        {
            throw new Exception("Child must implement IFormItemAware interface.");
        }

        _disposables = new CompositeDisposable();
        formItemAware.ValueChanged += HandleContentValueChanged;

        // Child is runtime user content outside the decorator's template. ControlTheme cannot reach interface-selected
        // child properties; the child-change lifecycle disposes the previous bindings.
        if (child is Control childControl)
        {
            _disposables.Add(FormSizeTypeBindingHelper.RelaySizeType(this, SizeTypeProperty, childControl));
            if (childControl is IMotionAwareControl)
            {
                _disposables.Add(BindUtils.RelayBind(this, IsMotionEnabledProperty, childControl, IsMotionEnabledProperty));
            }
            if (childControl is IInputControlStyleVariantAware)
            {
                _disposables.Add(BindUtils.RelayBind(this, StyleVariantProperty, childControl, StyleVariantProperty));
            }
        }

        if (child is IFormItemFeedbackAware feedbackAware)
        {
            feedbackAware.SetFeedbackControl(FormFeedback);
        }
    }

    private void ReleaseChild(object? child)
    {
        _disposables?.Dispose();
        _disposables = null;

        if (child is IFormItemAware formItemAware)
        {
            formItemAware.ValueChanged -= HandleContentValueChanged;
        }
        if (child is IFormItemFeedbackAware feedbackAware)
        {
            feedbackAware.SetFeedbackControl(null);
        }
    }

    private void HandleContentValueChanged(object? sender, EventArgs args)
    {
        _formValueChanged?.Invoke(this, EventArgs.Empty);
    }
}
