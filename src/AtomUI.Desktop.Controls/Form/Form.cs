using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.Threading;

namespace AtomUI.Desktop.Controls;

public class Form : AbstractForm,
                    ICustomizableSizeTypeAware,
                    IMotionAwareControl,
                    IForm,
                    IInputControlStyleVariantAware
{
    #region 公共属性定义
    public static readonly StyledProperty<bool> IsShowColonProperty =
        AvaloniaProperty.Register<Form, bool>(
            nameof(IsShowColon), true);

    public static readonly StyledProperty<BoxShadows?> BoxShadowProperty =
        AvaloniaProperty.Register<Form, BoxShadows?>(nameof(BoxShadow));
    
    public static readonly StyledProperty<InputControlStyleVariant> StyleVariantProperty =
        InputControlStyleVariantProperty.StyleVariantProperty.AddOwner<Form>();
    
    public static readonly StyledProperty<CustomizableSizeType> SizeTypeProperty =
        CustomizableSizeTypeControlProperty.SizeTypeProperty.AddOwner<Form>();
    
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<Form>();
    
    public new static readonly StyledProperty<FormRequiredMark> RequiredMarkProperty =
        AbstractForm.RequiredMarkProperty.AddOwner<Form>();
    
    public new static readonly StyledProperty<bool> IsValidateFeedbackEnabledProperty =
        AbstractForm.IsValidateFeedbackEnabledProperty.AddOwner<Form>();
    
    public static readonly StyledProperty<object?> SuccessFeedbackProperty =
        FormValidateFeedback.SuccessFeedbackProperty.AddOwner<Form>();
    
    public static readonly StyledProperty<IDataTemplate?> SuccessFeedbackTemplateProperty =
        FormValidateFeedback.SuccessFeedbackTemplateProperty.AddOwner<Form>();
    
    public static readonly StyledProperty<object?> WarningFeedbackProperty =
        FormValidateFeedback.WarningFeedbackProperty.AddOwner<Form>();
    
    public static readonly StyledProperty<IDataTemplate?> WarningFeedbackTemplateProperty =
        FormValidateFeedback.WarningFeedbackTemplateProperty.AddOwner<Form>();
    
    public static readonly StyledProperty<object?> ErrorFeedbackProperty =
        FormValidateFeedback.ErrorFeedbackProperty.AddOwner<Form>();
    
    public static readonly StyledProperty<IDataTemplate?> ErrorFeedbackTemplateProperty =
        FormValidateFeedback.ErrorFeedbackTemplateProperty.AddOwner<Form>();
    
    public static readonly StyledProperty<object?> ValidatingFeedbackProperty =
        FormValidateFeedback.ValidatingFeedbackProperty.AddOwner<Form>();
    
    public static readonly StyledProperty<IDataTemplate?> ValidatingFeedbackTemplateProperty =
        FormValidateFeedback.ValidatingFeedbackTemplateProperty.AddOwner<Form>();
    
    public static readonly StyledProperty<IDataTemplate?> FeedbackTemplateProperty =
        AvaloniaProperty.Register<Form, IDataTemplate?>(nameof(FeedbackTemplate));
    
    public static readonly StyledProperty<FormLabelAlign> LabelAlignProperty =
        AvaloniaProperty.Register<Form, FormLabelAlign>(nameof(LabelAlign), FormLabelAlign.Right);
    
    public static readonly StyledProperty<TextWrapping> LabelWrappingProperty =
        AvaloniaProperty.Register<Form, TextWrapping>(nameof(LabelWrapping), TextWrapping.NoWrap);
    
    public new static readonly StyledProperty<FormLayout> FormLayoutProperty =
        AbstractForm.FormLayoutProperty.AddOwner<Form>();
    
    public static readonly StyledProperty<object?> CustomRequireMarkProperty =
        AvaloniaProperty.Register<Form, object?>(nameof(CustomRequireMark));
    
    public static readonly StyledProperty<IDataTemplate?> CustomRequireMarkTemplateProperty =
        AvaloniaProperty.Register<Form, IDataTemplate?>(nameof(CustomRequireMarkTemplate));
    
    public static readonly StyledProperty<object?> CustomOptionalMarkProperty =
        AvaloniaProperty.Register<Form, object?>(nameof(CustomOptionalMark));
    
    public static readonly StyledProperty<IDataTemplate?> CustomOptionalMarkTemplateProperty =
        AvaloniaProperty.Register<Form, IDataTemplate?>(nameof(CustomOptionalMarkTemplate));
    
    public new static readonly StyledProperty<bool> IsScrollToFirstErrorEnabledProperty =
        AbstractForm.IsScrollToFirstErrorEnabledProperty.AddOwner<Form>();
    
    public new static readonly StyledProperty<FormValidateTrigger> ValidateTriggerProperty =
        AbstractForm.ValidateTriggerProperty.AddOwner<Form>();
    
    public static readonly StyledProperty<MediaBreakGridLength?> LabelColInfoProperty =
        AvaloniaProperty.Register<Form, MediaBreakGridLength?>(nameof(LabelColInfo));
    
    public static readonly StyledProperty<MediaBreakGridLength?> WrapperColInfoProperty =
        AvaloniaProperty.Register<Form, MediaBreakGridLength?>(nameof(WrapperColInfo));
    
    public new static readonly DirectProperty<Form, IFormValues?> ValuesProperty =
        AbstractForm.ValuesProperty.AddOwner<Form>(owner => owner.Values);
    
    public new static readonly DirectProperty<Form, IFormValues?> InitialValuesProperty =
        AbstractForm.InitialValuesProperty.AddOwner<Form>(owner => owner.InitialValues, (owner, value) => owner.InitialValues = value);
    
    public new static readonly DirectProperty<Form, bool?> IsFormValidProperty =
        AbstractForm.IsFormValidProperty.AddOwner<Form>(owner => owner.IsFormValid);
    
    public static readonly StyledProperty<IBrush?> ErrorMessageForegroundProperty =
        AvaloniaProperty.Register<Form, IBrush?>(nameof(ErrorMessageForeground));
    
    public static readonly StyledProperty<IBrush?> WarningMessageForegroundProperty =
        AvaloniaProperty.Register<Form, IBrush?>(nameof(WarningMessageForeground));
    
    public static readonly StyledProperty<int> MinItemCountProperty =
        AvaloniaProperty.Register<Form, int>(nameof(MinItemCount), 1);
    
    public static readonly StyledProperty<IIconTemplate?> ItemDeleteButtonIconProperty =
        AvaloniaProperty.Register<Form, IIconTemplate?>(nameof(ItemDeleteButtonIcon));
    
    public static readonly DirectProperty<Form, bool> IsShowItemDeleteButtonProperty =
        AvaloniaProperty.RegisterDirect<Form, bool>(
            nameof(IsShowItemDeleteButton),
            o => o.IsShowItemDeleteButton,
            (o, v) => o.IsShowItemDeleteButton = v);
    
    public static readonly DirectProperty<Form, bool> IsHideItemLabelProperty =
        AvaloniaProperty.RegisterDirect<Form, bool>(
            nameof(IsHideItemLabel),
            o => o.IsHideItemLabel,
            (o, v) => o.IsHideItemLabel = v);
    
    public bool IsShowColon
    {
        get => GetValue(IsShowColonProperty);
        set => SetValue(IsShowColonProperty, value);
    }

    public BoxShadows? BoxShadow
    {
        get => GetValue(BoxShadowProperty);
        set => SetValue(BoxShadowProperty, value);
    }
    
    public InputControlStyleVariant StyleVariant
    {
        get => GetValue(StyleVariantProperty);
        set => SetValue(StyleVariantProperty, value);
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
    
    public new FormRequiredMark RequiredMark
    {
        get => GetValue(RequiredMarkProperty);
        set => SetValue(RequiredMarkProperty, value);
    }
    
    public new bool IsValidateFeedbackEnabled
    {
        get => GetValue(IsValidateFeedbackEnabledProperty);
        set => SetValue(IsValidateFeedbackEnabledProperty, value);
    }
    
    [DependsOn(nameof(SuccessFeedbackTemplate))]
    public object? SuccessFeedback
    {
        get => GetValue(SuccessFeedbackProperty);
        set => SetValue(SuccessFeedbackProperty, value);
    }
    
    public IDataTemplate? SuccessFeedbackTemplate
    {
        get => GetValue(SuccessFeedbackTemplateProperty);
        set => SetValue(SuccessFeedbackTemplateProperty, value);
    }
    
    [DependsOn(nameof(WarningFeedbackTemplate))]
    public object? WarningFeedback
    {
        get => GetValue(WarningFeedbackProperty);
        set => SetValue(WarningFeedbackProperty, value);
    }
    
    public IDataTemplate? WarningFeedbackTemplate
    {
        get => GetValue(WarningFeedbackTemplateProperty);
        set => SetValue(WarningFeedbackTemplateProperty, value);
    }
    
    [DependsOn(nameof(ErrorFeedbackTemplate))]
    public object? ErrorFeedback
    {
        get => GetValue(ErrorFeedbackProperty);
        set => SetValue(ErrorFeedbackProperty, value);
    }

    public IDataTemplate? ErrorFeedbackTemplate
    {
        get => GetValue(ErrorFeedbackTemplateProperty);
        set => SetValue(ErrorFeedbackTemplateProperty, value);
    }
    
    [DependsOn(nameof(ValidatingFeedbackTemplate))]
    public object? ValidatingFeedback
    {
        get => GetValue(ValidatingFeedbackProperty);
        set => SetValue(ValidatingFeedbackProperty, value);
    }

    public IDataTemplate? ValidatingFeedbackTemplate
    {
        get => GetValue(ValidatingFeedbackTemplateProperty);
        set => SetValue(ValidatingFeedbackTemplateProperty, value);
    }
    
    public IDataTemplate? FeedbackTemplate
    {
        get => GetValue(FeedbackTemplateProperty);
        set => SetValue(FeedbackTemplateProperty, value);
    }
    
    public FormLabelAlign LabelAlign
    {
        get => GetValue(LabelAlignProperty);
        set => SetValue(LabelAlignProperty, value);
    }
    
    public TextWrapping LabelWrapping
    {
        get => GetValue(LabelWrappingProperty);
        set => SetValue(LabelWrappingProperty, value);
    }
    
    public new FormLayout FormLayout
    {
        get => GetValue(FormLayoutProperty);
        set => SetValue(FormLayoutProperty, value);
    }
    
    [DependsOn(nameof(CustomRequireMarkTemplate))]
    public object? CustomRequireMark
    {
        get => GetValue(CustomRequireMarkProperty);
        set => SetValue(CustomRequireMarkProperty, value);
    }
    
    public IDataTemplate? CustomRequireMarkTemplate
    {
        get => GetValue(CustomRequireMarkTemplateProperty);
        set => SetValue(CustomRequireMarkTemplateProperty, value);
    }
    
    [DependsOn(nameof(CustomOptionalMarkTemplate))]
    public object? CustomOptionalMark
    {
        get => GetValue(CustomOptionalMarkProperty);
        set => SetValue(CustomOptionalMarkProperty, value);
    }
    
    public IDataTemplate? CustomOptionalMarkTemplate
    {
        get => GetValue(CustomOptionalMarkTemplateProperty);
        set => SetValue(CustomOptionalMarkTemplateProperty, value);
    }
    
    public new bool IsScrollToFirstErrorEnabled
    {
        get => GetValue(IsScrollToFirstErrorEnabledProperty);
        set => SetValue(IsScrollToFirstErrorEnabledProperty, value);
    }
    
    public new FormValidateTrigger ValidateTrigger
    {
        get => GetValue(ValidateTriggerProperty);
        set => SetValue(ValidateTriggerProperty, value);
    }
    
    public MediaBreakGridLength? LabelColInfo
    {
        get => GetValue(LabelColInfoProperty);
        set => SetValue(LabelColInfoProperty, value);
    }
    
    public MediaBreakGridLength? WrapperColInfo
    {
        get => GetValue(WrapperColInfoProperty);
        set => SetValue(WrapperColInfoProperty, value);
    }
    
    private new IEnumerable? ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }
    
    private new IDataTemplate? ItemTemplate
    {
        get => GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }
    
    public new IFormValues? Values => base.Values;

    public new IFormValues? InitialValues
    {
        get => base.InitialValues;
        set => base.InitialValues = value;
    }

    public new bool? IsFormValid => base.IsFormValid;

    public IBrush? ErrorMessageForeground
    {
        get => GetValue(ErrorMessageForegroundProperty);
        set => SetValue(ErrorMessageForegroundProperty, value);
    }
    
    public IBrush? WarningMessageForeground
    {
        get => GetValue(WarningMessageForegroundProperty);
        set => SetValue(WarningMessageForegroundProperty, value);
    }
    
    /// <summary>
    /// 表单中最少的数据 Item 的数量，用于控制是否显示删除 Item 的按钮
    /// </summary>
    public int MinItemCount
    {
        get => GetValue(MinItemCountProperty);
        set => SetValue(MinItemCountProperty, value);
    }
    
    public IIconTemplate? ItemDeleteButtonIcon
    {
        get => GetValue(ItemDeleteButtonIconProperty);
        set => SetValue(ItemDeleteButtonIconProperty, value);
    }
    
    private bool _isShowItemDeleteButton;

    public bool IsShowItemDeleteButton
    {
        get => _isShowItemDeleteButton;
        set => SetAndRaise(IsShowItemDeleteButtonProperty, ref _isShowItemDeleteButton, value);
    }
    
    private bool _isHideItemLabel;

    public bool IsHideItemLabel
    {
        get => _isHideItemLabel;
        set => SetAndRaise(IsHideItemLabelProperty, ref _isHideItemLabel, value);
    }
    #endregion

    #region 公共事件定义

    public new event EventHandler<EventArgs>? AboutToValidate
    {
        add => base.AboutToValidate += value;
        remove => base.AboutToValidate -= value;
    }
    public new event EventHandler<FormValidatedEventArgs>? Validated
    {
        add => base.Validated += value;
        remove => base.Validated -= value;
    }
    public new event EventHandler<FormSubmittedEventArgs>? Submitted
    {
        add => base.Submitted += value;
        remove => base.Submitted -= value;
    }
    public new event EventHandler<FormItemValueChangedEventArgs>? ItemValueChanged
    {
        add => base.ItemValueChanged += value;
        remove => base.ItemValueChanged -= value;
    }
    public new event EventHandler? ResetCompleted
    {
        add => base.ResetCompleted += value;
        remove => base.ResetCompleted -= value;
    }
    
    #endregion
    
    #region 内部属性定义
    internal static readonly DirectProperty<Form, Orientation> FormLayoutOrientationProperty =
        AvaloniaProperty.RegisterDirect<Form, Orientation>(
            nameof(FormLayoutOrientation),
            o => o.FormLayoutOrientation);
    
    internal static readonly DirectProperty<Form, bool> IsEffectiveShowItemDeleteButtonProperty =
        AvaloniaProperty.RegisterDirect<Form, bool>(
            nameof(IsEffectiveShowItemDeleteButton),
            o => o.IsEffectiveShowItemDeleteButton,
            (o, v) => o.IsEffectiveShowItemDeleteButton = v);
    
    internal static readonly StyledProperty<double> FormLayoutSpacingProperty =
        AvaloniaProperty.Register<Form, double>(nameof(FormLayoutSpacing));
    
    private Orientation _formLayoutOrientation;

    internal Orientation FormLayoutOrientation
    {
        get => _formLayoutOrientation;
        set => SetAndRaise(FormLayoutOrientationProperty, ref _formLayoutOrientation, value);
    }
    
    private bool _isEffectiveShowItemDeleteButton;

    internal bool IsEffectiveShowItemDeleteButton
    {
        get => _isEffectiveShowItemDeleteButton;
        set => SetAndRaise(IsEffectiveShowItemDeleteButtonProperty, ref _isEffectiveShowItemDeleteButton, value);
    }
    
    internal double FormLayoutSpacing
    {
        get => GetValue(FormLayoutSpacingProperty);
        set => SetValue(FormLayoutSpacingProperty, value);
    }
    
    #endregion

    static Form()
    {
        AffectsMeasure<Form>(SizeTypeProperty);
        AffectsMeasure<Form>(FormLayoutOrientationProperty);
        SubmitButton.SubmitEvent.AddClassHandler<Form>((form, args) => form.HandleSubmitButtonClick(args));
        ResetButton.ResetEvent.AddClassHandler<Form>((form, args) => form.HandleResetButtonClick(args));
        FormItem.DeleteRequestEvent.AddClassHandler<Form>((form, args) => form.HandleFormItemDelete(args));
    }
    
    public Form()
    {
        LogicalChildren.CollectionChanged += HandleCollectionChanged;
        Items.CollectionChanged           += HandleItemsCollectionChanged;
    }

    private void HandleCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        ConfigureShowItemDeleteButton();
    }

    private void HandleItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        InvalidateMeasure();
    }
    
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        return new FormItem
        {
            OwnerForm = this
        };
    }

    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        return NeedsContainer<FormItem>(item, out recycleKey);
    }
    
    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);
        if (container is FormItem formItem)
        {
            formItem[!FormItem.SizeTypeProperty]                        = this[!SizeTypeProperty];
            formItem[!FormItem.IsMotionEnabledProperty]                 = this[!IsMotionEnabledProperty];
            formItem[!FormItem.ErrorMessageForegroundProperty]          = this[!ErrorMessageForegroundProperty];
            formItem[!FormItem.WarningMessageForegroundProperty]        = this[!WarningMessageForegroundProperty];
            formItem[!FormItem.IsShowColonProperty]                     = this[!IsShowColonProperty];
            formItem[!FormItem.RequiredMarkProperty]                    = this[!RequiredMarkProperty];
            formItem[!FormItem.LabelWrappingProperty]                   = this[!LabelWrappingProperty];
            formItem[!FormItem.StyleVariantProperty]                    = this[!StyleVariantProperty];
            formItem[!FormItem.CustomRequireMarkProperty]               = this[!CustomRequireMarkProperty];
            formItem[!FormItem.CustomRequireMarkTemplateProperty]       = this[!CustomRequireMarkTemplateProperty];
            formItem[!FormItem.CustomOptionalMarkProperty]              = this[!CustomOptionalMarkProperty];
            formItem[!FormItem.CustomOptionalMarkTemplateProperty]      = this[!CustomOptionalMarkTemplateProperty];
            formItem[!FormItem.LabelColInfoProperty]                    = this[!LabelColInfoProperty];
            formItem[!FormItem.WrapperColInfoProperty]                  = this[!WrapperColInfoProperty];
            formItem[!FormItem.FeedbackTemplateProperty]                = this[!FeedbackTemplateProperty];
            formItem[!FormItem.IsShowItemDeleteButtonProperty]          = this[!IsShowItemDeleteButtonProperty];
            formItem[!FormItem.IsEffectiveShowItemDeleteButtonProperty] = this[!IsEffectiveShowItemDeleteButtonProperty];
            formItem[!FormItem.ItemDeleteButtonIconTemplateProperty] = this[!ItemDeleteButtonIconProperty];
            formItem[!FormItem.FormLayoutProperty] = this[!FormLayoutProperty];
            formItem[!FormItem.IsHideItemLabelProperty] = this[!IsHideItemLabelProperty];
    
            PrepareFormItem(formItem, item, index);
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(container), "The container type is incorrect, it must be type FormItem.");
        }
    }

    protected virtual void PrepareFormItem(FormItem formItem, object? item, int index)
    {
    }
    
    public new void Validate() => base.Validate();

    public new Task<FormValidateResult> ValidateAsync(CancellationToken cancellationToken) => base.ValidateAsync(cancellationToken);

    public new void SetFormValues(IFormValues formValues) => base.SetFormValues(formValues);

    public new void Submit() => base.Submit();

    protected override void NotifySubmit(FormValues values) => base.NotifySubmit(values);

    public new void Reset() => base.Reset();

    protected override void NotifyReset() => base.NotifyReset();

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        SyncConfigToItems();
        ConfigureFormLayoutOrientation();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == LabelAlignProperty ||
            change.Property == FormLayoutProperty ||
            change.Property == ValidateTriggerProperty)
        {
            SyncConfigToItems();
        }

        if (change.Property == FormLayoutProperty)
        {
            ConfigureFormLayoutOrientation();
        }
        else if (change.Property == IsShowItemDeleteButtonProperty)
        {
            ConfigureShowItemDeleteButton();
        }
    }

    private void SyncConfigToItems()
    {
        foreach (var item in Items)
        {
            if (item is FormItem formItem)
            {
                SyncConfigToItem(formItem);
            }
        }
    }

    private void SyncConfigToItem(FormItem formItem)
    {
        formItem.SetValue(FormItem.LabelAlignProperty, LabelAlign, BindingPriority.Style);
        formItem.SetValue(FormItem.IsValidateFeedbackEnabledProperty, IsValidateFeedbackEnabled, BindingPriority.Style);
        formItem.SetValue(FormItem.ValidateTriggerProperty, ValidateTrigger, BindingPriority.Style);
        
        if (FormLayout == FormLayout.Horizontal || FormLayout == FormLayout.Inline)
        {
            formItem.SetValue(FormItem.LayoutProperty, FormItemLayout.Horizontal, BindingPriority.Style);
        }
        else
        {
            formItem.SetValue(FormItem.LayoutProperty, FormItemLayout.Vertical, BindingPriority.Style);
        }
    }

    private void ConfigureFormLayoutOrientation()
    {
        FormLayoutOrientation = (FormLayout == FormLayout.Horizontal || FormLayout == FormLayout.Vertical) ? Orientation.Vertical : Orientation.Horizontal;
    }

    private void HandleSubmitButtonClick(RoutedEventArgs args)
    {
        Submit();
        args.Handled = true;
    }

    private void HandleResetButtonClick(RoutedEventArgs args)
    {
        Reset();
        args.Handled = true;
    }

    protected override void OnLoaded(RoutedEventArgs e) => base.OnLoaded(e);

    public new void DeleteFormItem(IFormItem formItem) => base.DeleteFormItem(formItem);

    private void HandleFormItemDelete(RoutedEventArgs args)
    {
        if (args.Source is FormItem formItem)
        {
            DeleteFormItem(formItem);
        }
    }

    private void ConfigureShowItemDeleteButton()
    {
        var count = 0;
        foreach (var item in LogicalChildren)
        {
            if (item is FormItem formItem && formItem.IsValueItem)
            {
                count++;
            }
        }
        IsEffectiveShowItemDeleteButton = IsShowItemDeleteButton && count > MinItemCount;
    }
}
