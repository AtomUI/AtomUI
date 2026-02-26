using System.Collections.Specialized;
using System.Reactive.Disposables;
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

using ControlList = Avalonia.Controls.Controls;

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
    public static readonly StyledProperty<bool> IsShowColonProperty =
        Form.IsShowColonProperty.AddOwner<FormItem>();
    
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
    
    public static readonly StyledProperty<IList<IFormValidator>?> ValidatorProperty =
        AvaloniaProperty.Register<FormItem, IList<IFormValidator>?>(nameof(Validator));
    
    public static readonly StyledProperty<string?> TooltipProperty =
        AvaloniaProperty.Register<FormItem, string?>(nameof(Tooltip));
    
    public static readonly StyledProperty<FormValidateTrigger> ValidateTriggerProperty =
        Form.ValidateTriggerProperty.AddOwner<FormItem>();
    
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
    
    public bool IsShowColon
    {
        get => GetValue(IsShowColonProperty);
        set => SetValue(IsShowColonProperty, value);
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
    
    public IList<IFormValidator>? Validator
    {
        get => GetValue(ValidatorProperty);
        set => SetValue(ValidatorProperty, value);
    }
    
    public string? Tooltip
    {
        get => GetValue(TooltipProperty);
        set => SetValue(TooltipProperty, value);
    }
    
    public FormValidateTrigger ValidateTrigger
    {
        get => GetValue(ValidateTriggerProperty);
        set => SetValue(ValidateTriggerProperty, value);
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
    public ControlList Children { get; } = new();
    #endregion

    #region 内部属性定义

    internal static readonly StyledProperty<SizeType> SizeTypeProperty =
        SizeTypeControlProperty.SizeTypeProperty.AddOwner<FormItem>();
    
    internal static readonly StyledProperty<AddOnDecoratedVariant> StyleVariantProperty =
        AddOnDecoratedBox.StyleVariantProperty.AddOwner<FormItem>();
    
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
    
    Control IControlSharedTokenResourcesHost.HostControl => this;
    string IControlSharedTokenResourcesHost.TokenId => FormToken.ID;
    private readonly Dictionary<object, CompositeDisposable> _itemsBindingDisposables = new();
    #endregion

    private Grid? _rootLayout;
    private StackPanel? _childrenLayout;
    private Control? _label;
    
    static FormItem()
    {
        AffectsMeasure<FormItem>(SizeTypeProperty);
    }
    
    public FormItem()
    {
        this.RegisterResources();
        LogicalChildren.CollectionChanged += HandleCollectionChanged;
        Children.CollectionChanged        += HandleChildrenChanged;
    }
    
    private void HandleCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems != null)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems.Count > 0)
            {
                foreach (var item in e.OldItems)
                {
                    if (item != null)
                    {
                        if (_itemsBindingDisposables.TryGetValue(item, out var disposable))
                        {
                            disposable.Dispose();
                            _itemsBindingDisposables.Remove(item);
                        }
                    }
                }
            }
        }
    }
    
    protected virtual void HandleChildrenChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                var newChildren = e.NewItems!.OfType<FormItem>().ToList();
                _childrenLayout?.Children.InsertRange(e.NewStartingIndex, newChildren);
                foreach (var child in newChildren)
                {
                    NotifyChildrenAdded(child);
                }
                break;

            case NotifyCollectionChangedAction.Move:
                _childrenLayout?.Children.MoveRange(e.OldStartingIndex, e.OldItems!.Count, e.NewStartingIndex);
                break;

            case NotifyCollectionChangedAction.Remove:
                _childrenLayout?.Children.RemoveAll(e.OldItems!.OfType<Control>().ToList());
                break;

            case NotifyCollectionChangedAction.Replace:
                for (var i = 0; i < e.OldItems!.Count; ++i)
                {
                    var index = i + e.OldStartingIndex;
                    var child = (Control)e.NewItems![i]!;
                    _childrenLayout?.Children[index] = child;
                }
                break;

            case NotifyCollectionChangedAction.Reset:
                throw new NotSupportedException();
        }
    }

    protected virtual void NotifyChildrenAdded(Control child)
    {
        
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _rootLayout     = e.NameScope.Find<Grid>(FormItemThemeConstants.RootLayoutPart);
        _childrenLayout = e.NameScope.Find<StackPanel>(FormItemThemeConstants.ChildrenLayoutPart);
        _label          = e.NameScope.Find<Control>(FormItemThemeConstants.LabelPart);
        
        foreach (var child in Children)
        {
            if (child != null)
            {
                _childrenLayout?.Children.Add(child);
                NotifyChildrenAdded(child);
            }
        }
    }
}