using AtomUI.Controls;
using AtomUI.Reflection;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

public abstract class AbstractStatistic : HeaderedContentControl, IThemeInheritanceSource
{
    #region 公共属性定义
    
    public static readonly StyledProperty<string> DecimalSeparatorProperty =
        AvaloniaProperty.Register<AbstractStatistic, string>(nameof(DecimalSeparator), ".");
    
    public static readonly StyledProperty<string> GroupSeparatorProperty =
        AvaloniaProperty.Register<AbstractStatistic, string>(nameof(GroupSeparator), ",");
    
    public static readonly StyledProperty<int> PrecisionProperty =
        AvaloniaProperty.Register<AbstractStatistic, int>(nameof(Precision), 0);
    
    public static readonly StyledProperty<bool> IsLoadingProperty =
        AvaloniaProperty.Register<AbstractStatistic, bool>(nameof(IsLoading));
    
    public static readonly StyledProperty<object?> ValuePrefixAddOnProperty =
        AvaloniaProperty.Register<AbstractStatistic, object?>(nameof(ValuePrefixAddOn));
    
    public static readonly StyledProperty<IDataTemplate?> ValuePrefixAddOnTemplateProperty =
        AvaloniaProperty.Register<AbstractStatistic, IDataTemplate?>(nameof(ValuePrefixAddOnTemplate));

    public static readonly StyledProperty<object?> ValueSuffixAddOnProperty =
        AvaloniaProperty.Register<AbstractStatistic, object?>(nameof(ValueSuffixAddOn));
    
    public static readonly StyledProperty<IDataTemplate?> ValueSuffixAddOnTemplateProperty =
        AvaloniaProperty.Register<AbstractStatistic, IDataTemplate?>(nameof(ValueSuffixAddOnTemplate));
    
    public static readonly StyledProperty<IBrush?> ContentForegroundProperty =
        AvaloniaProperty.Register<AbstractStatistic, IBrush?>(nameof(ContentForeground));
    
    public static readonly StyledProperty<double> ContentFontSizeProperty =
        AvaloniaProperty.Register<AbstractStatistic, double>(nameof(ContentFontSize));

    public static readonly StyledProperty<IReadOnlyList<double>?> StrokeDashArrayProperty =
        AvaloniaProperty.Register<AbstractStatistic, IReadOnlyList<double>?>(nameof(StrokeDashArray));
    
    public string DecimalSeparator
    {
        get => GetValue(DecimalSeparatorProperty);
        set => SetValue(DecimalSeparatorProperty, value);
    }
    
    public string GroupSeparator
    {
        get => GetValue(GroupSeparatorProperty);
        set => SetValue(GroupSeparatorProperty, value);
    }
    
    public int Precision
    {
        get => GetValue(PrecisionProperty);
        set => SetValue(PrecisionProperty, value);
    }
    
    public bool IsLoading
    {
        get => GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }
    
    [DependsOn(nameof(ValuePrefixAddOnTemplate))]
    public object? ValuePrefixAddOn
    {
        get => GetValue(ValuePrefixAddOnProperty);
        set => SetValue(ValuePrefixAddOnProperty, value);
    }
    
    public IDataTemplate? ValuePrefixAddOnTemplate
    {
        get => GetValue(ValuePrefixAddOnTemplateProperty);
        set => SetValue(ValuePrefixAddOnTemplateProperty, value);
    }

    [DependsOn(nameof(ValueSuffixAddOnTemplate))]
    public object? ValueSuffixAddOn
    {
        get => GetValue(ValueSuffixAddOnProperty);
        set => SetValue(ValueSuffixAddOnProperty, value);
    }
    
    public IDataTemplate? ValueSuffixAddOnTemplate
    {
        get => GetValue(ValueSuffixAddOnTemplateProperty);
        set => SetValue(ValueSuffixAddOnTemplateProperty, value);
    }
    
    public IBrush? ContentForeground
    {
        get => GetValue(ContentForegroundProperty);
        set => SetValue(ContentForegroundProperty, value);
    }

    public double ContentFontSize
    {
        get => GetValue(ContentFontSizeProperty);
        set => SetValue(ContentFontSizeProperty, value);
    }

    public IReadOnlyList<double>? StrokeDashArray
    {
        get => GetValue(StrokeDashArrayProperty);
        set => SetValue(StrokeDashArrayProperty, value);
    }

    #endregion

    private WeakReference<Control>? _pendingTemplateFocus;
    private int _templateGeneration;

    static AbstractStatistic()
    {
        HeaderProperty.Changed.AddClassHandler<AbstractStatistic>((x, e) => x.HandleChildChanged(e));
        ValuePrefixAddOnProperty.Changed.AddClassHandler<AbstractStatistic>((x, e) => x.HandleChildChanged(e));
        ValueSuffixAddOnProperty.Changed.AddClassHandler<AbstractStatistic>((x, e) => x.HandleChildChanged(e));
        AffectsMeasure<AbstractStatistic>(DecimalSeparatorProperty, GroupSeparatorProperty, PrecisionProperty);
    }
    
    public AbstractStatistic()
    {
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (change.Property == TemplateProperty)
        {
            var focused = TopLevel.GetTopLevel(this)?.FocusManager?.GetFocusedElement() as Control;
            if (focused is not null)
            {
                _pendingTemplateFocus = this.IsVisualAncestorOf(focused)
                    ? new WeakReference<Control>(focused)
                    : null;
            }
            ++_templateGeneration;
        }
        base.OnPropertyChanged(change);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        ++_templateGeneration;
        _pendingTemplateFocus = null;
        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        if (_pendingTemplateFocus is { } focus)
        {
            _pendingTemplateFocus = null;
            var generation = _templateGeneration;
            var weakOwner = new WeakReference<AbstractStatistic>(this);
            // Native header/value/add-on controls reattach after presenter creation;
            // a theme Setter cannot restore runtime focus or guard later user input.
            Dispatcher.UIThread.Post(() =>
            {
                if (weakOwner.TryGetTarget(out var owner) && owner._templateGeneration == generation &&
                    focus.TryGetTarget(out var control) && owner.IsVisualAncestorOf(control) &&
                    TopLevel.GetTopLevel(owner) is { } topLevel &&
                    ReferenceEquals(TopLevel.GetTopLevel(control), topLevel) &&
                    topLevel.FocusManager?.GetFocusedElement() is null)
                {
                    control.Focus();
                }
            }, DispatcherPriority.Loaded);
        }
    }
    
    #region 实现 IThemeInheritanceSource 接口

    AvaloniaObject? IThemeInheritanceSource.ThemeInheritanceParent => InheritanceParent;

    #endregion

    private void HandleChildChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.OldValue is ILogical oldChild)
        {
            LogicalChildren.Remove(oldChild);
            if (oldChild is Icon icon)
            {
                icon.SetTemplatedParent(null);
            }
        }

        if (e.NewValue is ILogical newChild)
        {
            LogicalChildren.Add(newChild);
            if (newChild is Icon icon)
            {
                icon.SetTemplatedParent(this);
            }
        }
    }
}
