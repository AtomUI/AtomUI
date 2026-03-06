using System.Diagnostics;
using System.Reactive.Disposables;
using AtomUI.Controls;
using AtomUI.Data;
using AtomUI.Desktop.Controls.Themes;
using AtomUI.Icons.AntDesign;
using AtomUI.Theme;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.Threading;

namespace AtomUI.Desktop.Controls;

public enum FormItemLayout
{
    Horizontal,
    Vertical
}

public class FormItem : TemplatedControl,
                        IControlSharedTokenResourcesHost,
                        IFormItem
{
    #region 公共属性定义
    
    public static readonly StyledProperty<object?> ExtraProperty =
        AvaloniaProperty.Register<FormItem, object?>(nameof(Extra));

    public static readonly StyledProperty<IDataTemplate?> ExtraTemplateProperty =
        AvaloniaProperty.Register<FormItem, IDataTemplate?>(nameof(ExtraTemplate));
    
    public static readonly StyledProperty<string?> HelpProperty =
        AvaloniaProperty.Register<FormItem, string?>(nameof(Help));
    
    public static readonly StyledProperty<bool> IsValidateFeedbackEnabledProperty =
        Form.IsValidateFeedbackEnabledProperty.AddOwner<FormItem>();
    
    public static readonly StyledProperty<object?> InitialValueProperty =
        AvaloniaProperty.Register<FormItem, object?>(nameof(InitialValue));
    
    public static readonly StyledProperty<string?> LabelTextProperty =
        AvaloniaProperty.Register<FormItem, string?>(nameof(LabelText));
    
    public static readonly StyledProperty<FormLabelAlign> LabelAlignProperty =
        Form.LabelAlignProperty.AddOwner<FormItem>();
    
    public static readonly StyledProperty<TextWrapping> LabelWrappingProperty =
        Form.LabelWrappingProperty.AddOwner<FormItem>();
    
    public static readonly StyledProperty<MediaBreakGridLength?> LabelColInfoProperty =
        Form.LabelColInfoProperty.AddOwner<FormItem>();
    
    public static readonly StyledProperty<MediaBreakGridLength?> WrapperColInfoProperty =
        Form.WrapperColInfoProperty.AddOwner<FormItem>();
    
    public static readonly StyledProperty<string?> FieldNameProperty =
        AvaloniaProperty.Register<FormItem, string?>(nameof(FieldName));
    
    public static readonly StyledProperty<bool> IsRequiredProperty =
        AvaloniaProperty.Register<FormItem, bool>(nameof(IsRequired));
    
    public static readonly StyledProperty<IList<IFormValidator>?> ValidatorsProperty =
        AvaloniaProperty.Register<FormItem, IList<IFormValidator>?>(nameof(Validators));
    
    public static readonly StyledProperty<string?> TooltipProperty =
        AvaloniaProperty.Register<FormItem, string?>(nameof(Tooltip));
    
    public static readonly StyledProperty<PathIcon?> TooltipIconProperty =
        AvaloniaProperty.Register<FormItem, PathIcon?>(nameof(TooltipIcon));
    
    public static readonly StyledProperty<FormValidateTrigger> ValidateTriggerProperty =
        Form.ValidateTriggerProperty.AddOwner<FormItem>();
    
    public static readonly StyledProperty<TimeSpan> ValidateDebounceProperty =
        AvaloniaProperty.Register<FormItem, TimeSpan>(nameof(ValidateDebounce), TimeSpan.Zero);
    
    public static readonly StyledProperty<FormValidateStrategy> ValidateStrategyProperty =
        AvaloniaProperty.Register<FormItem, FormValidateStrategy>(nameof(ValidateStrategy), FormValidateStrategy.StopWhenFirstFailed);
    
    public static readonly DirectProperty<FormItem, FormValidateStatus> ValidateStatusProperty =
        AvaloniaProperty.RegisterDirect<FormItem, FormValidateStatus>(
            nameof(ValidateStatus),
            o => o.ValidateStatus);
    
    public static readonly DirectProperty<FormItem, FormValidateResult> ValidateResultProperty =
        AvaloniaProperty.RegisterDirect<FormItem, FormValidateResult>(
            nameof(ValidateResult),
            o => o.ValidateResult);
    
    public static readonly StyledProperty<FormItemLayout> LayoutProperty =
        AvaloniaProperty.Register<FormItem, FormItemLayout>(nameof(Layout), FormItemLayout.Horizontal);
    
    public static readonly StyledProperty<double> ChildrenSpacingProperty =
        AvaloniaProperty.Register<FormItem, double>(nameof(ChildrenSpacing));
    
    public static readonly StyledProperty<Control?> ContentProperty =
        AvaloniaProperty.Register<FormItem, Control?>(nameof(Content));
    
    public static readonly StyledProperty<bool> IsValidateContentTypeProperty =
        AvaloniaProperty.Register<FormItem, bool>(nameof(IsValidateContentType));
    
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
    
    public string? Help
    {
        get => GetValue(HelpProperty);
        set => SetValue(HelpProperty, value);
    }
    
    public bool IsValidateFeedbackEnabled
    {
        get => GetValue(IsValidateFeedbackEnabledProperty);
        set => SetValue(IsValidateFeedbackEnabledProperty, value);
    }
    
    public object? InitialValue
    {
        get => GetValue(InitialValueProperty);
        set => SetValue(InitialValueProperty, value);
    }
    
    public string? LabelText
    {
        get => GetValue(LabelTextProperty);
        set => SetValue(LabelTextProperty, value);
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

    public string? FieldName
    {
        get => GetValue(FieldNameProperty);
        set => SetValue(FieldNameProperty, value);
    }
    
    public bool IsRequired
    {
        get => GetValue(IsRequiredProperty);
        set => SetValue(IsRequiredProperty, value);
    }
    
    public IList<IFormValidator>? Validators
    {
        get => GetValue(ValidatorsProperty);
        set => SetValue(ValidatorsProperty, value);
    }
    
    public string? Tooltip
    {
        get => GetValue(TooltipProperty);
        set => SetValue(TooltipProperty, value);
    }
    
    public PathIcon? TooltipIcon
    {
        get => GetValue(TooltipIconProperty);
        set => SetValue(TooltipIconProperty, value);
    }
    
    public FormValidateTrigger ValidateTrigger
    {
        get => GetValue(ValidateTriggerProperty);
        set => SetValue(ValidateTriggerProperty, value);
    }
    
    public TimeSpan ValidateDebounce
    {
        get => GetValue(ValidateDebounceProperty);
        set => SetValue(ValidateDebounceProperty, value);
    }
    
    public FormValidateStrategy ValidateStrategy
    {
        get => GetValue(ValidateStrategyProperty);
        set => SetValue(ValidateStrategyProperty, value);
    }
    
    private FormValidateStatus _validateStatus = FormValidateStatus.Default;

    public FormValidateStatus ValidateStatus
    {
        get => _validateStatus;
        set => SetAndRaise(ValidateStatusProperty, ref _validateStatus, value);
    }
    
    private FormValidateResult _validateResult = FormValidateResult.Success;

    public FormValidateResult ValidateResult
    {
        get => _validateResult;
        private set => SetAndRaise(ValidateResultProperty, ref _validateResult, value);
    }
    
    public FormItemLayout Layout
    {
        get => GetValue(LayoutProperty);
        set => SetValue(LayoutProperty, value);
    }
    
    public double ChildrenSpacing
    {
        get => GetValue(ChildrenSpacingProperty);
        set => SetValue(ChildrenSpacingProperty, value);
    }
    
    [Content]
    public Control? Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }
    
    public bool IsValidateContentType
    {
        get => GetValue(IsValidateContentTypeProperty);
        set => SetValue(IsValidateContentTypeProperty, value);
    }
    #endregion

    #region 公共事件定义

    public static readonly RoutedEvent<FormItemValidateChangedEventArgs> ValidateChangedEvent =
        RoutedEvent.Register<Button, FormItemValidateChangedEventArgs>(nameof(ValidateChanged), RoutingStrategies.Bubble | RoutingStrategies.Tunnel);
    
    public event EventHandler<FormItemValidateChangedEventArgs>? ValidateChanged
    {
        add => AddHandler(ValidateChangedEvent, value);
        remove => RemoveHandler(ValidateChangedEvent, value);
    }

    #endregion

    #region 内部属性定义
    internal static readonly StyledProperty<bool> IsShowColonProperty =
        Form.IsShowColonProperty.AddOwner<FormItem>();

    internal static readonly StyledProperty<SizeType> SizeTypeProperty =
        SizeTypeControlProperty.SizeTypeProperty.AddOwner<FormItem>();
    
    internal static readonly StyledProperty<InputControlStyleVariant> StyleVariantProperty =
        InputControlStyleVariantProperty.StyleVariantProperty.AddOwner<FormItem>();
    
    internal static readonly DirectProperty<FormItem, bool> IsColonVisibleProperty =
        AvaloniaProperty.RegisterDirect<FormItem, bool>(
            nameof(IsColonVisible),
            o => o.IsColonVisible,
            (o, v) => o.IsColonVisible = v);
    
    internal static readonly DirectProperty<FormItem, bool> IsValueItemProperty =
        AvaloniaProperty.RegisterDirect<FormItem, bool>(
            nameof(IsValueItem),
            o => o.IsValueItem);
    
    internal static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<FormItem>();
    
    internal static readonly StyledProperty<FormRequiredMark> RequiredMarkProperty =
        Form.RequiredMarkProperty.AddOwner<FormItem>();
    
    internal static readonly DirectProperty<FormItem, IList<string>?> ValidateErrorMessagesProperty =
        AvaloniaProperty.RegisterDirect<FormItem, IList<string>?>(
            nameof(ValidateErrorMessages),
            o => o.ValidateErrorMessages,
            (o, v) => o.ValidateErrorMessages = v);
    
    internal static readonly DirectProperty<FormItem, IList<string>?> ValidateWarningMessagesProperty =
        AvaloniaProperty.RegisterDirect<FormItem, IList<string>?>(
            nameof(ValidateWarningMessages),
            o => o.ValidateWarningMessages,
            (o, v) => o.ValidateWarningMessages = v);
    
    internal static readonly StyledProperty<object?> CustomRequireMarkProperty =
        Form.CustomRequireMarkProperty.AddOwner<FormItem>();
    
    internal static readonly StyledProperty<IDataTemplate?> CustomRequireMarkTemplateProperty =
        Form.CustomRequireMarkTemplateProperty.AddOwner<FormItem>();
    
    internal static readonly StyledProperty<object?> CustomOptionalMarkProperty =
        Form.CustomOptionalMarkProperty.AddOwner<FormItem>();
    
    internal static readonly StyledProperty<IDataTemplate?> CustomOptionalMarkTemplateProperty =
        Form.CustomOptionalMarkTemplateProperty.AddOwner<FormItem>();
    
    internal static readonly DirectProperty<FormItem, bool> HasCustomRequireMarkProperty =
        AvaloniaProperty.RegisterDirect<FormItem, bool>(
            nameof(HasCustomRequireMark),
            o => o.HasCustomRequireMark,
            (o, v) => o.HasCustomRequireMark = v);
    
    internal static readonly DirectProperty<FormItem, bool> HasCustomOptionalMarkProperty =
        AvaloniaProperty.RegisterDirect<FormItem, bool>(
            nameof(HasCustomOptionalMark),
            o => o.HasCustomOptionalMark,
            (o, v) => o.HasCustomOptionalMark = v);
    
    internal static readonly DirectProperty<FormItem, double> LabelMaxWidthProperty =
        AvaloniaProperty.RegisterDirect<FormItem, double>(
            nameof(LabelMaxWidth),
            o => o.LabelMaxWidth,
            (o, v) => o.LabelMaxWidth = v);
    
    internal static readonly DirectProperty<FormItem, double> ContentPresenterMaxWidthProperty =
        AvaloniaProperty.RegisterDirect<FormItem, double>(
            nameof(ContentPresenterMaxWidth),
            o => o.ContentPresenterMaxWidth,
            (o, v) => o.ContentPresenterMaxWidth = v);
    
    internal static readonly DirectProperty<FormItem, bool> HasErrorOrWarningMsgProperty =
        AvaloniaProperty.RegisterDirect<FormItem, bool>(
            nameof(HasErrorOrWarningMsg),
            o => o.HasErrorOrWarningMsg,
            (o, v) => o.HasErrorOrWarningMsg = v);
    
    internal static readonly DirectProperty<FormItem, InlineCollection?> ErrorMessageInlinesProperty =
        AvaloniaProperty.RegisterDirect<FormItem, InlineCollection?>(
            nameof(ErrorMessageInlines), t => t.ErrorMessageInlines, 
            (t, v) => t.ErrorMessageInlines = v);
    
    internal static readonly StyledProperty<IBrush?> ErrorMessageForegroundProperty =
        Form.ErrorMessageForegroundProperty.AddOwner<FormItem>();
    
    internal static readonly StyledProperty<IBrush?> WarningMessageForegroundProperty =
        Form.WarningMessageForegroundProperty.AddOwner<FormItem>();
    
    internal static readonly StyledProperty<IDataTemplate?> FeedbackTemplateProperty =
        Form.FeedbackTemplateProperty.AddOwner<FormItem>();
    
    internal static readonly DirectProperty<FormItem, bool> IsShowItemDeleteButtonProperty =
        AvaloniaProperty.RegisterDirect<FormItem, bool>(
            nameof(IsShowItemDeleteButton),
            o => o.IsShowItemDeleteButton,
            (o, v) => o.IsShowItemDeleteButton = v);
    
    internal static readonly DirectProperty<FormItem, bool> IsEffectiveShowItemDeleteButtonProperty =
        AvaloniaProperty.RegisterDirect<FormItem, bool>(
            nameof(IsEffectiveShowItemDeleteButton),
            o => o.IsEffectiveShowItemDeleteButton,
            (o, v) => o.IsEffectiveShowItemDeleteButton = v);
    
    internal static readonly DirectProperty<FormItem, PathIcon?> ItemDeleteButtonIconProperty =
        AvaloniaProperty.RegisterDirect<FormItem, PathIcon?>(
            nameof(ItemDeleteButtonIcon),
            o => o.ItemDeleteButtonIcon,
            (o, v) => o.ItemDeleteButtonIcon = v);
    
    internal static readonly DirectProperty<FormItem, IIconTemplate?> ItemDeleteButtonIconTemplateProperty =
        AvaloniaProperty.RegisterDirect<FormItem, IIconTemplate?>(
            nameof(ItemDeleteButtonIconTemplate),
            o => o.ItemDeleteButtonIconTemplate,
            (o, v) => o.ItemDeleteButtonIconTemplate = v);
    
    internal bool IsShowColon
    {
        get => GetValue(IsShowColonProperty);
        set => SetValue(IsShowColonProperty, value);
    }
    
    internal SizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }
    
    internal InputControlStyleVariant StyleVariant
    {
        get => GetValue(StyleVariantProperty);
        set => SetValue(StyleVariantProperty, value);
    }
    
    internal bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    private bool _isColonVisible;

    internal bool IsColonVisible
    {
        get => _isColonVisible;
        set => SetAndRaise(IsColonVisibleProperty, ref _isColonVisible, value);
    }
    
    private bool _isValueItem;
    
    // 如果这个 FormItem 设置了 Label 和 FieldName 那么就是一个 ValueItem
    internal bool IsValueItem
    {
        get => _isValueItem;
        set => SetAndRaise(IsValueItemProperty, ref _isValueItem, value);
    }
    
    internal FormRequiredMark RequiredMark
    {
        get => GetValue(RequiredMarkProperty);
        set => SetValue(RequiredMarkProperty, value);
    }
    
    private IList<string>? _validateErrorMessages;

    internal IList<string>? ValidateErrorMessages
    {
        get => _validateErrorMessages;
        set => SetAndRaise(ValidateErrorMessagesProperty, ref _validateErrorMessages, value);
    }
    
    private IList<string>? _validateWarningMessages;

    internal IList<string>? ValidateWarningMessages
    {
        get => _validateWarningMessages;
        set => SetAndRaise(ValidateWarningMessagesProperty, ref _validateWarningMessages, value);
    }
    
    [DependsOn(nameof(CustomRequireMarkTemplate))]
    internal object? CustomRequireMark
    {
        get => GetValue(CustomRequireMarkProperty);
        set => SetValue(CustomRequireMarkProperty, value);
    }
    
    internal IDataTemplate? CustomRequireMarkTemplate
    {
        get => GetValue(CustomRequireMarkTemplateProperty);
        set => SetValue(CustomRequireMarkTemplateProperty, value);
    }
    
    [DependsOn(nameof(CustomOptionalMarkTemplate))]
    internal object? CustomOptionalMark
    {
        get => GetValue(CustomOptionalMarkProperty);
        set => SetValue(CustomOptionalMarkProperty, value);
    }
    
    internal IDataTemplate? CustomOptionalMarkTemplate
    {
        get => GetValue(CustomOptionalMarkTemplateProperty);
        set => SetValue(CustomOptionalMarkTemplateProperty, value);
    }
    
    private bool _hasCustomRequireMark;
    
    internal bool HasCustomRequireMark
    {
        get => _hasCustomRequireMark;
        set => SetAndRaise(HasCustomRequireMarkProperty, ref _hasCustomRequireMark, value);
    }
    
    private bool _hasCustomOptionalMark;
    
    internal bool HasCustomOptionalMark
    {
        get => _hasCustomOptionalMark;
        set => SetAndRaise(HasCustomOptionalMarkProperty, ref _hasCustomOptionalMark, value);
    }
    
    private double _labelMaxWidth = double.PositiveInfinity;
    
    internal double LabelMaxWidth
    {
        get => _labelMaxWidth;
        set => SetAndRaise(LabelMaxWidthProperty, ref _labelMaxWidth, value);
    }
    
    private double _contentPresenterMaxWidth = double.PositiveInfinity;
    
    internal double ContentPresenterMaxWidth
    {
        get => _contentPresenterMaxWidth;
        set => SetAndRaise(ContentPresenterMaxWidthProperty, ref _contentPresenterMaxWidth, value);
    }

    private bool _hasErrorOrWarningMsg;
    
    internal bool HasErrorOrWarningMsg
    {
        get => _hasErrorOrWarningMsg;
        set => SetAndRaise(HasErrorOrWarningMsgProperty, ref _hasErrorOrWarningMsg, value);
    }
    
    private InlineCollection? _errorMessageInlines;
    
    internal InlineCollection? ErrorMessageInlines
    {
        get => _errorMessageInlines;
        set => SetAndRaise(ErrorMessageInlinesProperty, ref _errorMessageInlines, value);
    }
    
    internal IBrush? ErrorMessageForeground
    {
        get => GetValue(ErrorMessageForegroundProperty);
        set => SetValue(ErrorMessageForegroundProperty, value);
    }
    
    internal IBrush? WarningMessageForeground
    {
        get => GetValue(WarningMessageForegroundProperty);
        set => SetValue(WarningMessageForegroundProperty, value);
    }
    
    internal IDataTemplate? FeedbackTemplate
    {
        get => GetValue(FeedbackTemplateProperty);
        set => SetValue(FeedbackTemplateProperty, value);
    }
    
    private bool _isShowItemDeleteButton;

    internal bool IsShowItemDeleteButton
    {
        get => _isShowItemDeleteButton;
        set => SetAndRaise(IsShowItemDeleteButtonProperty, ref _isShowItemDeleteButton, value);
    }
    
    private bool _isEffectiveShowItemDeleteButton;

    internal bool IsEffectiveShowItemDeleteButton
    {
        get => _isEffectiveShowItemDeleteButton;
        set => SetAndRaise(IsEffectiveShowItemDeleteButtonProperty, ref _isEffectiveShowItemDeleteButton, value);
    }
    
    private PathIcon? _itemDeleteButtonIcon;

    internal PathIcon? ItemDeleteButtonIcon
    {
        get => _itemDeleteButtonIcon;
        set => SetAndRaise(ItemDeleteButtonIconProperty, ref _itemDeleteButtonIcon, value);
    }
    
    private IIconTemplate? _itemDeleteButtonIconTemplate;

    internal IIconTemplate? ItemDeleteButtonIconTemplate
    {
        get => _itemDeleteButtonIconTemplate;
        set => SetAndRaise(ItemDeleteButtonIconTemplateProperty, ref _itemDeleteButtonIconTemplate, value);
    }
    
    Control IControlSharedTokenResourcesHost.HostControl => this;
    string IControlSharedTokenResourcesHost.TokenId => FormToken.ID;
    #endregion

    #region 内部事件定义

    internal static readonly RoutedEvent<RoutedEventArgs> ValueChangedEvent =
        RoutedEvent.Register<FormItem, RoutedEventArgs>(nameof(ValueChanged), RoutingStrategies.Bubble);
    
    internal static readonly RoutedEvent<RoutedEventArgs> DeleteRequestEvent =
        RoutedEvent.Register<FormItem, RoutedEventArgs>(nameof(DeleteRequest), RoutingStrategies.Bubble);
    
    internal event EventHandler<RoutedEventArgs>? ValueChanged
    {
        add => AddHandler(ValueChangedEvent, value);
        remove => RemoveHandler(ValueChangedEvent, value);
    }
    
    internal event EventHandler<RoutedEventArgs>? DeleteRequest
    {
        add => AddHandler(DeleteRequestEvent, value);
        remove => RemoveHandler(DeleteRequestEvent, value);
    }

    #endregion
    
    protected override Type StyleKeyOverride => typeof(FormItem);

    private Grid? _bodyLayout;
    private Panel? _contentLayout;
    private Panel? _labelLayout;
    private MediaBreakPoint? _breakPoint;
    private CompositeDisposable? _disposables;
    private FormValidateFeedback? _feedback;
    private IDisposable? _feedbackDisposable;
    private CancellationTokenSource? _validationTokenSource;
    internal Form? OwnerForm;
    
    static FormItem()
    {
        AffectsMeasure<FormItem>(SizeTypeProperty);
        IconButton.ClickEvent.AddClassHandler<FormItem>((formItem, args) => formItem.HandleDeleteButtonClicked(args));
    }
    
    public FormItem()
    {
        this.RegisterResources();
        LayoutUpdated += HandleLayoutUpdated;
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        if (TooltipIcon == null)
        {
            SetCurrentValue(TooltipIconProperty, new QuestionCircleOutlined());
        }
    }

    private void HandleDeleteButtonClicked(RoutedEventArgs args)
    {
        if (args.Source is ItemDeleteButton)
        {
            RaiseEvent(new RoutedEventArgs(DeleteRequestEvent, this));
            args.Handled = true;
        }
    }

    protected virtual void NotifyContentAdded(AvaloniaPropertyChangedEventArgs change)
    {
        if (change.OldValue is IFormItemAware oldFormItemAware)
        {
            oldFormItemAware.ValueChanged -= HandleContentValueChanged;
        }
        
        if (change.NewValue is IFormItemAware newFormItemAware)
        {
            newFormItemAware.ValueChanged += HandleContentValueChanged;
        }
        else if (IsValidateContentType)
        {
            throw new Exception($"Form item content: {change.NewValue?.GetType().FullName} not implement IFormItemAware interface");
        }
        
        _disposables?.Dispose();
        _disposables = new CompositeDisposable(2);
        if (Content is ISizeTypeAware)
        {
            _disposables.Add(BindUtils.RelayBind(this, SizeTypeProperty, Content, SizeTypeProperty));
        }

        if (Content is IInputControlStyleVariantAware)
        {
            _disposables.Add(BindUtils.RelayBind(this, StyleVariantProperty, Content, StyleVariantProperty));
        }
        
        if (change.NewValue is IFormItemFeedbackAware newFormItemFeedbackAware)
        {
            BuildFeedback(false);
            newFormItemFeedbackAware.SetFeedbackControl(_feedback);
        }
    }

    protected override void OnLostFocus(RoutedEventArgs e)
    {
        base.OnLostFocus(e);
        Debug.Assert(OwnerForm != null);
        if (!OwnerForm.IsResetting && ValidateTrigger == FormValidateTrigger.OnBlur)
        {
            ValidateValueDefer();
        }
    }

    private void HandleContentValueChanged(object? sender, EventArgs e)
    {
        Debug.Assert(OwnerForm != null);
        if (!OwnerForm.IsResetting && ValidateTrigger == FormValidateTrigger.OnChanged)
        {
            ValidateValueDefer();
        }
        RaiseEvent(new RoutedEventArgs(ValueChangedEvent, this));
    }

    private void ValidateValueDefer()
    {
        _validationTokenSource?.Cancel();
        _validationTokenSource = new CancellationTokenSource();
        var cancellationToken = _validationTokenSource.Token;
        if (ValidateDebounce != TimeSpan.Zero)
        {
            DispatcherTimer.RunOnce(() =>
            {
                Dispatcher.UIThread.InvokeAsync(() => ValidateValueAsync(cancellationToken));
            }, ValidateDebounce);
        }
        else
        {
            Dispatcher.UIThread.InvokeAsync(() => ValidateValueAsync(cancellationToken));
        }
    }

    public async Task ValidateValueAsync(CancellationToken cancellationToken)
    {
        if (Content == null || Validators == null || Validators.Count == 0)
        {
            return;
        }
        ValidateStatus = FormValidateStatus.Validating;
        var formItemAware     = Content as IFormItemAware;
        Debug.Assert(formItemAware != null);
        var value           = formItemAware.GetFormValue();
        var warningMessages = new List<string>();
        var hasWarning      = false;
        var hasError        = false;
        ValidateErrorMessages   = null;
        ValidateWarningMessages = null;
        if (ValidateStrategy == FormValidateStrategy.Parallel || ValidateStrategy == FormValidateStrategy.Sequential)
        {
            var tasks           = new Dictionary<Task<FormValidateResult>, IFormValidator>();
            var errorMessages = new List<string>();
            if (ValidateStrategy == FormValidateStrategy.Parallel)
            {
                foreach (var validator in Validators)
                {
                    var task = validator.ValidateAsync(FieldName ?? string.Empty, value, cancellationToken);
                    tasks.Add(task, validator);
                }
                await Task.WhenAll(tasks.Keys);
                foreach (var task in tasks.Keys)
                {
                    var result    = await task;
                    var validator = tasks[task];
                    if (result == FormValidateResult.Error)
                    {
                        hasError = true;
                        errorMessages.Add(validator.Message ?? string.Empty);
                    }
                    else if (result == FormValidateResult.Warning)
                    {
                        hasWarning = true;
                        warningMessages.Add(validator.Message ?? string.Empty);
                    }
                }
            }
            else
            {
                foreach (var validator in Validators)
                {
                    var result = await validator.ValidateAsync(FieldName ?? string.Empty, value, cancellationToken);
                    if (result == FormValidateResult.Error)
                    {
                        hasError = true;
                        errorMessages.Add(validator.Message ?? string.Empty);
                    }
                    else if (result == FormValidateResult.Warning)
                    {
                        hasWarning = true;
                        warningMessages.Add(validator.Message ?? string.Empty);
                    }
                }
            }
            
            if (hasWarning)
            {
                ValidateWarningMessages = warningMessages;
                ValidateStatus          = FormValidateStatus.Warning;
            }
            
            if (hasError)
            {
                ValidateErrorMessages = errorMessages;
                ValidateStatus = FormValidateStatus.Error;
            }
            
            if (!hasError && !hasWarning)
            {
                ValidateStatus          = FormValidateStatus.Success;
            }
        }
        else
        {
            foreach (var validator in Validators)
            {
                var result = await validator.ValidateAsync(FieldName ?? string.Empty, value, cancellationToken);
                if (result == FormValidateResult.Error)
                {
                    hasError = true;
                    ValidateStatus = FormValidateStatus.Error;
                    ValidateErrorMessages = new List<string>
                    {
                        validator.Message ?? string.Empty
                    };
                    break;
                }
                if (result == FormValidateResult.Warning)
                {
                    hasWarning = true;
                    warningMessages.Add(validator.Message ?? string.Empty);
                }
            }
            
            if  (hasWarning)
            {
                ValidateWarningMessages = warningMessages;
                if (!hasError)
                {
                    ValidateStatus = FormValidateStatus.Warning;
                }
            }
            
            if (!hasError && !hasWarning)
            {
                ValidateStatus          = FormValidateStatus.Success;
            }
        }

        if (ValidateStatus == FormValidateStatus.Error)
        {
            ValidateResult = FormValidateResult.Error;
        }
        else if (ValidateStatus == FormValidateStatus.Warning)
        {
            ValidateResult = FormValidateResult.Warning;
        }
        else
        {
            ValidateResult = FormValidateResult.Success;
        }

        formItemAware.NotifyValidateStatus(ValidateStatus);
        HasErrorOrWarningMsg = ValidateErrorMessages?.Count > 0 || ValidateWarningMessages?.Count > 0;
        BuildErrorMessageInlines();
        RaiseEvent(new FormItemValidateChangedEventArgs(ValidateStatus)
        {
            RoutedEvent = ValidateChangedEvent,
            Source = this,
        });
    }

    private void BuildErrorMessageInlines()
    {
        if (ValidateResult == FormValidateResult.Success)
        {
            ErrorMessageInlines = null;
        }
        else
        {
            var inlines = new InlineCollection();
            if (ValidateErrorMessages != null)
            {
                for (var i = 0; i < ValidateErrorMessages.Count; i++)
                {
                    var message = ValidateErrorMessages[i];
                    inlines.Add(new Run(message)
                    {
                        Foreground = ErrorMessageForeground,
                    });
                    if (i != ValidateErrorMessages.Count - 1)
                    {
                        inlines.Add(new LineBreak());
                    }
                }
            }

            if (ValidateWarningMessages != null)
            {
                for (int i = 0; i < ValidateWarningMessages.Count; i++)
                {
                    if (inlines.Count > 0 && inlines.Last() is not LineBreak)
                    {
                        inlines.Add(new LineBreak());
                    }
                    var message = ValidateWarningMessages[i];
                    inlines.Add(new Run(message)
                    {
                        Foreground = WarningMessageForeground,
                    });
         
                    if (i != ValidateWarningMessages.Count - 1)
                    {
                        inlines.Add(new LineBreak());
                    }
                }
            }
            ErrorMessageInlines = inlines;
        }
    }

    public object? GetItemValue()
    {
        if (Content is IFormItemAware formItemAware)
        {
            return formItemAware.GetFormValue();
        }
        return null;
    }

    public void SetItemValue(object? value)
    {
        if (Content is IFormItemAware formItemAware)
        {
            formItemAware.SetFormValue(value);
        }
    }

    public void ResetItemValue()
    {
        if (Content is IFormItemAware formItemAware)
        { 
            formItemAware.ClearFormValue();
            formItemAware.NotifyValidateStatus(FormValidateStatus.Default);
            ValidateStatus = FormValidateStatus.Default;
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsShowColonProperty ||
            change.Property == LabelTextProperty ||
            change.Property == LayoutProperty)
        {
            ConfigureLabelColonVisible();
        }
        
        if (change.Property == LayoutProperty ||
            change.Property == LabelColInfoProperty ||
            change.Property == WrapperColInfoProperty)
        {
            ConfigureLayout();
        }

        if (change.Property == ContentProperty)
        {
            NotifyContentAdded(change);
        }
        else if (change.Property == FieldNameProperty ||
                 change.Property == LabelTextProperty)
        {
            IsValueItem = !string.IsNullOrWhiteSpace(FieldName) && !string.IsNullOrWhiteSpace(LabelText);
        }

        if (change.Property == CustomRequireMarkProperty)
        {
            HasCustomRequireMark = CustomRequireMark != null;
        }
        else if (change.Property == CustomOptionalMarkProperty)
        {
            HasCustomOptionalMark = CustomOptionalMark != null;
        }
        else if (change.Property == ErrorMessageForegroundProperty ||
                 change.Property == WarningMessageForegroundProperty)
        {
            BuildErrorMessageInlines();
        }

        if (change.Property == FeedbackTemplateProperty ||
            change.Property == IsValidateFeedbackEnabledProperty)
        {
            HandleFeedbackTemplateChanged();
        }
        else if (change.Property == ItemDeleteButtonIconTemplateProperty)
        {
            if (ItemDeleteButtonIconTemplate == null)
            {
                ItemDeleteButtonIcon = null;
            }
            else
            {
                ItemDeleteButtonIcon = ItemDeleteButtonIconTemplate.Build();
            }
        }
    }

    private Control? BuildFeedback(bool force = false)
    {
        if (OwnerForm == null)
        {
            return null;
        }
        if (_feedback == null || force)
        {
            _feedback = FeedbackTemplate?.Build(OwnerForm) as FormValidateFeedback;
            if (_feedback != null)
            {
                _feedback.DataContext = OwnerForm;
                _feedbackDisposable = BindUtils.RelayBind(this, ValidateStatusProperty, _feedback, FormValidateFeedback.ValidateStatusProperty);
            }
        }
        return _feedback;
    }
    
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _bodyLayout     = e.NameScope.Find<Grid>(FormItemThemeConstants.BodyLayoutPart);
        _labelLayout    = e.NameScope.Find<Panel>(FormItemThemeConstants.LabelLayoutPart);
        _contentLayout = e.NameScope.Find<Panel>(FormItemThemeConstants.ContentLayoutPart);
        ConfigureLayout();
        Debug.Assert(OwnerForm != null);
        if (IsValidateFeedbackEnabled)
        {
            if (Content is IFormItemFeedbackAware feedbackAware)
            {
                BuildFeedback(false);
                feedbackAware.SetFeedbackControl(_feedback);
            }
        }
    }

    private void HandleLayoutUpdated(object? sender, EventArgs e)
    {
        if (_contentLayout != null)
        {
            ContentPresenterMaxWidth = _contentLayout.Bounds.Width;
        }

        if (_labelLayout != null)
        {
            if (Layout == FormItemLayout.Horizontal)
            {
                LabelMaxWidth = _labelLayout.Bounds.Width;
            }
            else
            {
                LabelMaxWidth = double.PositiveInfinity;
            }
        }
    }
    
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (TopLevel.GetTopLevel(this) is Window window)
        {
            window.MediaBreakPointChanged += HandleMediaBreakChanged;
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        if (TopLevel.GetTopLevel(this) is Window window)
        {
            window.MediaBreakPointChanged -= HandleMediaBreakChanged;
        }
    }
    
    private void HandleMediaBreakChanged(object? sender, MediaBreakPointChangedEventArgs args)
    {
        _breakPoint = args.MediaBreakPoint;
        if (_breakPoint != null)
        {
            if (Layout == FormItemLayout.Horizontal)
            {
                ConfigureLayout();
            }
        }
    }

    private void ConfigureLabelColonVisible()
    {
        IsColonVisible = !string.IsNullOrWhiteSpace(LabelText) && IsShowColon && Layout == FormItemLayout.Horizontal;
    }

    private void ConfigureLayout()
    {
        if (_bodyLayout == null || _labelLayout == null || _contentLayout == null)
        {
            return;
        }
        if (Layout == FormItemLayout.Horizontal)
        {
            _bodyLayout.RowDefinitions.Clear();
            _bodyLayout.ColumnDefinitions.Clear();
            
            _bodyLayout.ColumnDefinitions.Add(new ColumnDefinition(GetGridLengthForMediaBreak(_breakPoint ?? MediaBreakPoint.Large, 
                LabelColInfo ?? new MediaBreakGridLength(new GridLength(1, GridUnitType.Star)))));
            _bodyLayout.ColumnDefinitions.Add(new ColumnDefinition(GetGridLengthForMediaBreak(_breakPoint ?? MediaBreakPoint.Large,
                WrapperColInfo ?? new MediaBreakGridLength(new GridLength(3, GridUnitType.Star)))));
            _bodyLayout.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            Grid.SetRow(_labelLayout, 0);
            Grid.SetRow(_contentLayout, 0);
            Grid.SetColumn(_labelLayout, 0);
            Grid.SetColumn(_contentLayout, 1);
        }
        else if (Layout == FormItemLayout.Vertical)
        {
            _bodyLayout.RowDefinitions.Clear();
            _bodyLayout.ColumnDefinitions.Clear();
            _bodyLayout.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            _bodyLayout.RowDefinitions.Add(new RowDefinition(GridLength.Star));
            _bodyLayout.RowDefinitions.Add(new RowDefinition(GridLength.Star));
            Grid.SetRow(_labelLayout, 0);
            Grid.SetRow(_contentLayout, 1);
            Grid.SetColumn(_labelLayout, 0);
            Grid.SetColumn(_contentLayout, 0);
        }
    }
    
    private GridLength GetGridLengthForMediaBreak(MediaBreakPoint breakPoint, MediaBreakGridLength info)
    {
        var gridLength = GridLength.Auto;
        if (breakPoint == MediaBreakPoint.ExtraSmall)
        {
            gridLength = info.ExtraSmall;
        }
        else if (breakPoint == MediaBreakPoint.Small)
        {
            gridLength = info.Small;
        }
        else if (breakPoint == MediaBreakPoint.Medium)
        {
            gridLength = info.Medium;
        }
        else if (breakPoint == MediaBreakPoint.Large)
        {
            gridLength = info.Large;
        }
        else if (breakPoint == MediaBreakPoint.ExtraLarge)
        {
            gridLength = info.ExtraLarge;
        }
        else if (breakPoint == MediaBreakPoint.ExtraExtraLarge)
        {
            gridLength = info.ExtraExtraLarge;
        }

        return gridLength;
    }
    
    internal virtual bool IsSkipValidate()
    {
        return !IsVisible;
    }

    protected internal virtual void NotifyFormItemChanged(IFormItem formItem)
    {
    }

    private void HandleFeedbackTemplateChanged()
    {
        _feedback = null;
        _feedbackDisposable?.Dispose();
        _feedbackDisposable = null;
        if (IsValidateFeedbackEnabled)
        {
            if (Content is IFormItemFeedbackAware itemFeedbackAware)
            {
                BuildFeedback(true);
                itemFeedbackAware.SetFeedbackControl(_feedback);
            }
        }
    }
}

public class FormActionsItem : FormItem
{
    private new bool IsValidateContentType
    {
        get => GetValue(IsValidateContentTypeProperty);
        set => SetValue(IsValidateContentTypeProperty, value);
    }
    
    protected override void OnInitialized()
    {
        base.OnInitialized();
        IsValidateContentType = false;
        IsValueItem           = false;
    }
}