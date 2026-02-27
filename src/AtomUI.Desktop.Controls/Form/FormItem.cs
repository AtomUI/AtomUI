using System.Diagnostics;
using System.Reactive.Disposables;
using System.Text;
using AtomUI.Controls;
using AtomUI.Desktop.Controls.Themes;
using AtomUI.Theme;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.Threading;

namespace AtomUI.Desktop.Controls;

public enum FormValidateStrategy
{
    StopWhenFirstFailed,
    Sequential,
    Parallel
}

public enum FormValidateStatus
{
    Default,
    Success,
    Warning,
    Error,
    Validating
}

public enum FormItemLayout
{
    Horizontal,
    Vertical
}

public class FormItem : TemplatedControl,
                        IControlSharedTokenResourcesHost
{
    #region 公共属性定义
    
    public static readonly StyledProperty<object?> ExtraProperty =
        AvaloniaProperty.Register<FormItem, object?>(nameof(Extra));

    public static readonly StyledProperty<IDataTemplate?> ExtraTemplateProperty =
        AvaloniaProperty.Register<FormItem, IDataTemplate?>(nameof(ExtraTemplate));
    
    public static readonly StyledProperty<string?> HelpProperty =
        AvaloniaProperty.Register<FormItem, string?>(nameof(Help));
    
    public static readonly StyledProperty<bool> HasFeedbackProperty =
        AvaloniaProperty.Register<FormItem, bool>(nameof(HasFeedback));
    
    public static readonly StyledProperty<IconTemplate?> ErrorFeedbackIconProperty =
        Form.ErrorFeedbackIconProperty.AddOwner<FormItem>();
    
    public static readonly StyledProperty<IconTemplate?> WarningFeedbackIconProperty =
        Form.WarningFeedbackIconProperty.AddOwner<FormItem>();
    
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
    
    public static readonly StyledProperty<TimeSpan?> ValidateDebounceProperty =
        AvaloniaProperty.Register<FormItem, TimeSpan?>(nameof(ValidateDebounce));
    
    public static readonly StyledProperty<FormValidateStrategy> ValidateStrategyProperty =
        AvaloniaProperty.Register<FormItem, FormValidateStrategy>(nameof(ValidateStrategy), FormValidateStrategy.Sequential);
    
    public static readonly StyledProperty<FormValidateStatus> ValidateStatusProperty =
        AvaloniaProperty.Register<FormItem, FormValidateStatus>(nameof(ValidateStatus), FormValidateStatus.Default);
    
    public static readonly StyledProperty<FormItemLayout> LayoutProperty =
        AvaloniaProperty.Register<FormItem, FormItemLayout>(nameof(Layout), FormItemLayout.Horizontal);
    
    public static readonly StyledProperty<double> ChildrenSpacingProperty =
        AvaloniaProperty.Register<FormItem, double>(nameof(ChildrenSpacing));
    
    public static readonly StyledProperty<Control?> ContentProperty =
        AvaloniaProperty.Register<FormItem, Control?>(nameof(Content));
    
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
    
    public bool HasFeedback
    {
        get => GetValue(HasFeedbackProperty);
        set => SetValue(HasFeedbackProperty, value);
    }
    
    public IconTemplate? ErrorFeedbackIcon
    {
        get => GetValue(ErrorFeedbackIconProperty);
        set => SetValue(ErrorFeedbackIconProperty, value);
    }
    
    public IconTemplate? WarningFeedbackIcon
    {
        get => GetValue(WarningFeedbackIconProperty);
        set => SetValue(WarningFeedbackIconProperty, value);
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
    
    public TimeSpan? ValidateDebounce
    {
        get => GetValue(ValidateDebounceProperty);
        set => SetValue(ValidateDebounceProperty, value);
    }
    
    public FormValidateStrategy ValidateStrategy
    {
        get => GetValue(ValidateStrategyProperty);
        set => SetValue(ValidateStrategyProperty, value);
    }
    
    public FormValidateStatus ValidateStatus
    {
        get => GetValue(ValidateStatusProperty);
        set => SetValue(ValidateStatusProperty, value);
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
    #endregion

    #region 内部属性定义
    internal static readonly StyledProperty<bool> IsShowColonProperty =
        Form.IsShowColonProperty.AddOwner<FormItem>();

    internal static readonly StyledProperty<SizeType> SizeTypeProperty =
        SizeTypeControlProperty.SizeTypeProperty.AddOwner<FormItem>();
    
    internal static readonly StyledProperty<AddOnDecoratedVariant> StyleVariantProperty =
        AddOnDecoratedBox.StyleVariantProperty.AddOwner<FormItem>();
    
    internal static readonly DirectProperty<FormItem, bool> IsColonVisibleProperty =
        AvaloniaProperty.RegisterDirect<FormItem, bool>(
            nameof(IsColonVisible),
            o => o.IsColonVisible,
            (o, v) => o.IsColonVisible = v);
    
    internal static readonly DirectProperty<FormItem, bool> IsRequireMarkVisibleProperty =
        AvaloniaProperty.RegisterDirect<FormItem, bool>(
            nameof(IsRequireMarkVisible),
            o => o.IsRequireMarkVisible,
            (o, v) => o.IsRequireMarkVisible = v);
    
    internal static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<FormItem>();
    
    internal static readonly StyledProperty<FormRequiredMark> RequiredMarkProperty =
        Form.RequiredMarkProperty.AddOwner<FormItem>();
    
    internal static readonly StyledProperty<FormValidateTrigger> ValidateTriggerProperty =
        Form.ValidateTriggerProperty.AddOwner<FormItem>();
    
    internal static readonly DirectProperty<FormItem, string?> ValidateMsgProperty =
        AvaloniaProperty.RegisterDirect<FormItem, string?>(
            nameof(ValidateMsg),
            o => o.ValidateMsg,
            (o, v) => o.ValidateMsg = v);
    
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
    
    internal AddOnDecoratedVariant StyleVariant
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
    
    private bool _isRequireMarkVisible;

    internal bool IsRequireMarkVisible
    {
        get => _isRequireMarkVisible;
        set => SetAndRaise(IsRequireMarkVisibleProperty, ref _isRequireMarkVisible, value);
    }
    
    internal FormRequiredMark RequiredMark
    {
        get => GetValue(RequiredMarkProperty);
        set => SetValue(RequiredMarkProperty, value);
    }
    
    internal FormValidateTrigger ValidateTrigger
    {
        get => GetValue(ValidateTriggerProperty);
        set => SetValue(ValidateTriggerProperty, value);
    }
    
    private string? _validateMsg;

    internal string? ValidateMsg
    {
        get => _validateMsg;
        set => SetAndRaise(ValidateMsgProperty, ref _validateMsg, value);
    }
    
    Control IControlSharedTokenResourcesHost.HostControl => this;
    string IControlSharedTokenResourcesHost.TokenId => FormToken.ID;
    #endregion

    private Grid? _rootLayout;
    private Panel? _childrenLayout;
    private Panel? _labelLayout;
    private MediaBreakPoint? _breakPoint;
    
    static FormItem()
    {
        AffectsMeasure<FormItem>(SizeTypeProperty);
    }
    
    public FormItem()
    {
        this.RegisterResources();
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
    }

    private void HandleContentValueChanged(object? sender, EventArgs e)
    {
        if (ValidateTrigger == FormValidateTrigger.OnChanged)
        {
            Dispatcher.UIThread.InvokeAsync(async () => ValidateValueAsync());
        }
    }

    private async Task ValidateValueAsync()
    {
        if (Content == null || Validators == null || Validators.Count == 0)
        {
            return;
        }
        ValidateStatus = FormValidateStatus.Validating;
        var formItemAware     = Content as IFormItemAware;
        Debug.Assert(formItemAware != null);
        var value             = formItemAware.GetFormValue();
        var warningMsgBuilder = new StringBuilder();
        var hasWarning        = false;
        
        if (ValidateStrategy == FormValidateStrategy.Parallel || ValidateStrategy == FormValidateStrategy.Sequential)
        {
            var hasError        = false;
            var tasks           = new Dictionary<Task<FormValidateResult>, IFormValidator>();
            var errorMsgBuilder = new StringBuilder();
            if (ValidateStrategy == FormValidateStrategy.Parallel)
            {
                foreach (var validator in Validators)
                {
                    var task = validator.ValidateAsync(FieldName ?? string.Empty, value);
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
                        errorMsgBuilder.Append(validator.Message);
                    }
                    else if (result == FormValidateResult.Warning)
                    {
                        hasWarning = true;
                        warningMsgBuilder.Append(validator.Message);
                    }
                }
            }
            else
            {
                foreach (var validator in Validators)
                {
                    var result = await validator.ValidateAsync(FieldName ?? string.Empty, value);
                    if (result == FormValidateResult.Error)
                    {
                        hasError = true;
                        errorMsgBuilder.Append(validator.Message);
                    }
                    else if (result == FormValidateResult.Warning)
                    {
                        hasWarning = true;
                        warningMsgBuilder.Append(validator.Message);
                    }
                }
            }
            
            if (hasError)
            {
                ValidateMsg = errorMsgBuilder.ToString();
                ValidateStatus = FormValidateStatus.Error;
            }
            else if (hasWarning)
            {
                ValidateMsg = warningMsgBuilder.ToString();
                ValidateStatus = FormValidateStatus.Warning;
            }
            else
            {
                ValidateMsg    = null;
                ValidateStatus = FormValidateStatus.Success;
            }
        }
        else
        {
            foreach (var validator in Validators)
            {
                var result = await validator.ValidateAsync(FieldName ?? string.Empty, value);
                if (result == FormValidateResult.Error)
                {
                    ValidateStatus = FormValidateStatus.Error;
                    ValidateMsg    = validator.Message;
                    return;
                }
                if (result == FormValidateResult.Warning)
                {
                    hasWarning = true;
                    warningMsgBuilder.Append(validator.Message);
                }
            }
            
            if  (hasWarning)
            {
                ValidateMsg = warningMsgBuilder.ToString();
                ValidateStatus = FormValidateStatus.Warning;
            }
            else
            {
                ValidateMsg    = null;
                ValidateStatus = FormValidateStatus.Success;
            }
        }
        formItemAware.NotifyValidateStatus(ValidateStatus);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsShowColonProperty ||
            change.Property == LabelTextProperty)
        {
            ConfigureLabelColonVisible();
        }
        else if (change.Property == LayoutProperty)
        {
            ConfigureLayout();
        }
        else if (change.Property == RequiredMarkProperty ||
                 change.Property == IsRequiredProperty)
        {
            ConfigureRequireMarkVisible();
        }

        if (change.Property == ContentProperty)
        {
            NotifyContentAdded(change);
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _rootLayout        = e.NameScope.Find<Grid>(FormItemThemeConstants.RootLayoutPart);
        _labelLayout       = e.NameScope.Find<Panel>(FormItemThemeConstants.LabelLayoutPart);
        _childrenLayout    = e.NameScope.Find<Panel>(FormItemThemeConstants.ChildrenLayoutPart);
        ConfigureLayout();
        ConfigureRequireMarkVisible();
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
        IsColonVisible = !string.IsNullOrWhiteSpace(LabelText) && IsShowColon;
    }

    private void ConfigureLayout()
    {
        if (_rootLayout == null || _labelLayout == null || _childrenLayout == null)
        {
            return;
        }
        if (Layout == FormItemLayout.Horizontal)
        {
            _rootLayout.ColumnDefinitions.Clear();
            var columnDefinitions = new ColumnDefinitions();
            columnDefinitions.Add(new ColumnDefinition(GetGridLengthForMediaBreak(_breakPoint ?? MediaBreakPoint.Large, 
                LabelColInfo ?? new MediaBreakGridLength(new GridLength(1, GridUnitType.Star)))));
            columnDefinitions.Add(new ColumnDefinition(GetGridLengthForMediaBreak(_breakPoint ?? MediaBreakPoint.Large,
                WrapperColInfo ?? new MediaBreakGridLength(new GridLength(3, GridUnitType.Star)))));
            _rootLayout.ColumnDefinitions = columnDefinitions;
            
            Grid.SetColumn(_labelLayout, 0);
            Grid.SetColumn(_childrenLayout, 1);
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

    private void ConfigureRequireMarkVisible()
    {
        if (RequiredMark == FormRequiredMark.Default)
        {
            IsRequireMarkVisible = IsRequired;
        }
        else
        {
            IsRequireMarkVisible = false;
        }
    }
}