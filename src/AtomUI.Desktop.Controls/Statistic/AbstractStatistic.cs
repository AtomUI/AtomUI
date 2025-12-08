using AtomUI.Controls;
using AtomUI.Reflection;
using AtomUI.Theme;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Metadata;

namespace AtomUI.Desktop.Controls;

public abstract class AbstractStatistic : HeaderedContentControl, IControlSharedTokenResourcesHost
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

    #endregion

    #region 内部属性定义

    Control IControlSharedTokenResourcesHost.HostControl => this;
    string IControlSharedTokenResourcesHost.TokenId => StatisticToken.ID;

    #endregion
    
    static AbstractStatistic()
    {
        HeaderProperty.Changed.AddClassHandler<AbstractStatistic>((x, e) => x.HandleChildChanged(e));
        ValuePrefixAddOnProperty.Changed.AddClassHandler<AbstractStatistic>((x, e) => x.HandleChildChanged(e));
        ValueSuffixAddOnProperty.Changed.AddClassHandler<AbstractStatistic>((x, e) => x.HandleChildChanged(e));
        AffectsMeasure<AbstractStatistic>(DecimalSeparatorProperty, GroupSeparatorProperty, PrecisionProperty);
    }
    
    public AbstractStatistic()
    {
        this.RegisterResources();
    }
    
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