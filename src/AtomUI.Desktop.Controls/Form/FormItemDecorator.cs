using AtomUI.Theme.Resources;
using System.Reactive.Disposables;
using AtomUI.Controls;
using AtomUI.Data;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;

namespace AtomUI.Desktop.Controls;

public class FormItemDecorator : TemplatedControl,
                                 IInputControlStatusAware,
                                 IInputControlStyleVariantAware,
                                 IMotionAwareControl,
                                 ICustomizableSizeTypeAware,
                                 IFormItemAware,
                                 IFormItemFeedbackAware,
    IThemeInheritanceSource
{
    #region 公共属性定义

    public static readonly StyledProperty<Control?> ChildProperty =
        AvaloniaProperty.Register<FormItemDecorator, Control?>(nameof(Child));

    public static readonly StyledProperty<InputControlStyleVariant> StyleVariantProperty =
        InputControlStyleVariantProperty.StyleVariantProperty.AddOwner<FormItemDecorator>();

    public static readonly StyledProperty<InputControlStatus> StatusProperty =
        InputControlStatusProperty.StatusProperty.AddOwner<FormItemDecorator>();

    public static readonly StyledProperty<CustomizableSizeType> SizeTypeProperty =
        CustomizableSizeTypeControlProperty.SizeTypeProperty.AddOwner<FormItemDecorator>();

    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<FormItemDecorator>();

    public static readonly StyledProperty<object?> ExtraProperty =
        AvaloniaProperty.Register<FormItemDecorator, object?>(nameof(Extra));

    public static readonly StyledProperty<IDataTemplate?> ExtraTemplateProperty =
        AvaloniaProperty.Register<FormItemDecorator, IDataTemplate?>(nameof(ExtraTemplate));

    public static readonly StyledProperty<double> ItemSpacingProperty =
        AvaloniaProperty.Register<FormItemDecorator, double>(nameof(ItemSpacing));

    [Content]
    public Control? Child
    {
        get => GetValue(ChildProperty);
        set => SetValue(ChildProperty, value);
    }

    public InputControlStyleVariant StyleVariant
    {
        get => GetValue(StyleVariantProperty);
        set => SetValue(StyleVariantProperty, value);
    }

    public InputControlStatus Status
    {
        get => GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }

    public CustomizableSizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }

    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    [DependsOn(nameof(ExtraTemplate))]
    public object? Extra
    {
        get => GetValue(ExtraProperty);
        set => SetValue(ExtraProperty, value);
    }

    public IDataTemplate? ExtraTemplate
    {
        get => GetValue(ExtraTemplateProperty);
        set => SetValue(ExtraTemplateProperty, value);
    }

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

    private EventHandler? _formValueChanged;

    static FormItemDecorator()
    {
        AffectsMeasure<FormItemDecorator>(ChildProperty, PaddingProperty);
        ChildProperty.Changed.AddClassHandler<FormItemDecorator>((decorator, e) => decorator.HandleChildChanged(e));
    }



    #region 实现 既有接口

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

    protected virtual void NotifySetFormValue(object? value)
    {
        if (Child is IFormItemAware formItemAware)
        {
            formItemAware.SetFormValue(value);
        }
    }

    protected virtual object? NotifyGetFormValue()
    {
        if (Child is IFormItemAware formItemAware)
        {
            return formItemAware.GetFormValue();
        }
        return null;
    }

    protected virtual void NotifyClearFormValue()
    {
        if (Child is IFormItemAware formItemAware)
        {
            formItemAware.ClearFormValue();
        }
    }

    protected virtual void NotifyValidateStatus(FormValidateStatus status)
    {
        if (Child is IFormItemAware formItemAware)
        {
            formItemAware.NotifyValidateStatus(status);
        }
    }

    protected virtual void NotifySetFeedBackControl(FormValidateFeedback? value)
    {
        FormFeedback = value;
        if (Child is IFormItemFeedbackAware formItemFeedbackAware)
        {
            formItemFeedbackAware.SetFeedbackControl(value);
        }
    }

    #endregion

    #region 实现 IThemeInheritanceSource 接口

    // AXAML cannot read AvaloniaObject's protected, potentially non-logical inheritance parent.
    AvaloniaObject? IThemeInheritanceSource.ThemeInheritanceParent => InheritanceParent;

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

        if (child is Control childControl)
        {
            // User content is outside the ControlTheme template route. These
            // interface behavior relays retain their existing acquire/release lifetime.
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
