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
                                 ISizeTypeAware,
                                 IFormItemAware,
                                 IFormItemFeedbackAware
{
    #region 公共属性定义

    public static readonly StyledProperty<Control?> ChildProperty =
        AvaloniaProperty.Register<FormItemDecorator, Control?>(nameof(Child));
    
    public static readonly StyledProperty<InputControlStyleVariant> StyleVariantProperty =
        InputControlStyleVariantProperty.StyleVariantProperty.AddOwner<FormItemDecorator>();

    public static readonly StyledProperty<InputControlStatus> StatusProperty =
        InputControlStatusProperty.StatusProperty.AddOwner<FormItemDecorator>();
    
    public static readonly StyledProperty<SizeType> SizeTypeProperty =
        SizeTypeControlProperty.SizeTypeProperty.AddOwner<FormItemDecorator>();
    
    public static readonly StyledProperty<bool> IsMotionEnabledProperty = 
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<FormItemDecorator>();
    
    public static readonly StyledProperty<object?> ExtraProperty =
        AvaloniaProperty.Register<FormItem, object?>(nameof(Extra));

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

    public SizeType SizeType
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

    private CompositeDisposable? _disposables;
    
    static FormItemDecorator()
    {
        AffectsMeasure<FormItemDecorator>(ChildProperty, PaddingProperty);
        ChildProperty.Changed.AddClassHandler<FormItemDecorator>((decorator, e) => decorator.HandleChildChanged(e));
    }

    private void HandleChildChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.OldValue is IFormItemAware oldFormItemAware)
        {
            _disposables?.Dispose();
            oldFormItemAware.ValueChanged -= HandleContentValueChanged;
        }
        if (e.NewValue is IFormItemAware newFormItemAware)
        {
            _disposables = new CompositeDisposable();
            newFormItemAware.ValueChanged += HandleContentValueChanged;
            if (e.NewValue is Control newChild)
            {
                if (newChild is ISizeTypeAware)
                {
                    _disposables.Add(BindUtils.RelayBind(this, SizeTypeProperty, newChild, SizeTypeProperty));
                }
                if (newChild is IMotionAwareControl)
                {
                    _disposables.Add(BindUtils.RelayBind(this, IsMotionEnabledProperty, newChild, IsMotionEnabledProperty));
                }
                if (newChild is IInputControlStyleVariantAware)
                {
                    _disposables.Add(BindUtils.RelayBind(this, StyleVariantProperty, newChild, StyleVariantProperty));
                }
            }
        }
        else
        {
            throw new Exception("Child must implement IFormItemAware interface.");
        }
    }

    private void HandleContentValueChanged(object? sender, EventArgs args)
    {
        _formValueChanged?.Invoke(this, EventArgs.Empty);
    }

    #region 内部属性定义

    internal static readonly StyledProperty<FormValidateFeedback?> FormFeedbackProperty = 
        AvaloniaProperty.Register<FormItemDecorator, FormValidateFeedback?>(nameof(FormFeedback));
    
    internal FormValidateFeedback? FormFeedback
    {
        get => GetValue(FormFeedbackProperty);
        set => SetValue(FormFeedbackProperty, value);
    }

    #endregion
    
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
        if (Child is IFormItemFeedbackAware formItemFeedbackAware)
        {
            formItemFeedbackAware.SetFeedbackControl(value);
        }
    }
    #endregion
}