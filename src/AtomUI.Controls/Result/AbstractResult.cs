using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;
using Avalonia.VisualTree;
using AtomUI.Icons.AntDesign;

namespace AtomUI.Controls.Commons;

using SvgControl = Avalonia.Svg.Svg;

public abstract class AbstractResult : ContentControl
{
    #region 公共属性定义
    public static readonly StyledProperty<object?> ExtraProperty =
        AvaloniaProperty.Register<AbstractResult, object?>(nameof(Extra));

    public static readonly StyledProperty<IDataTemplate?> ExtraTemplateProperty =
        AvaloniaProperty.Register<AbstractResult, IDataTemplate?>(nameof(ExtraTemplate));

    public static readonly StyledProperty<object?> IconProperty =
        AvaloniaProperty.Register<AbstractResult, object?>(nameof(Icon));

    public static readonly StyledProperty<ResultStatus> StatusProperty =
        AvaloniaProperty.Register<AbstractResult, ResultStatus>(nameof(Status));

    public static readonly StyledProperty<object?> HeaderProperty =
        AvaloniaProperty.Register<AbstractResult, object?>(nameof(Header));

    public static readonly StyledProperty<IDataTemplate?> HeaderTemplateProperty =
        AvaloniaProperty.Register<AbstractResult, IDataTemplate?>(nameof(HeaderTemplate));

    public static readonly StyledProperty<object?> SubHeaderProperty =
        AvaloniaProperty.Register<AbstractResult, object?>(nameof(SubHeader));

    public static readonly StyledProperty<IDataTemplate?> SubHeaderTemplateProperty =
        AvaloniaProperty.Register<AbstractResult, IDataTemplate?>(nameof(SubHeaderTemplate));

    public static readonly StyledProperty<double> HeaderFontSizeProperty =
        AvaloniaProperty.Register<AbstractResult, double>(nameof(HeaderFontSize));

    public static readonly StyledProperty<double> SubHeaderFontSizeProperty =
        AvaloniaProperty.Register<AbstractResult, double>(nameof(SubHeaderFontSize));

    public static readonly StyledProperty<IReadOnlyList<double>?> StrokeDashArrayProperty =
        AvaloniaProperty.Register<AbstractResult, IReadOnlyList<double>?>(nameof(StrokeDashArray));

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

    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public ResultStatus Status
    {
        get => GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }

    [DependsOn(nameof(HeaderTemplate))]
    public object? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public IDataTemplate? HeaderTemplate
    {
        get => GetValue(HeaderTemplateProperty);
        set => SetValue(HeaderTemplateProperty, value);
    }

    [DependsOn(nameof(SubHeaderTemplate))]
    public object? SubHeader
    {
        get => GetValue(SubHeaderProperty);
        set => SetValue(SubHeaderProperty, value);
    }

    public IDataTemplate? SubHeaderTemplate
    {
        get => GetValue(SubHeaderTemplateProperty);
        set => SetValue(SubHeaderTemplateProperty, value);
    }

    public double HeaderFontSize
    {
        get => GetValue(HeaderFontSizeProperty);
        set => SetValue(HeaderFontSizeProperty, value);
    }

    public double SubHeaderFontSize
    {
        get => GetValue(SubHeaderFontSizeProperty);
        set => SetValue(SubHeaderFontSizeProperty, value);
    }

    public IReadOnlyList<double>? StrokeDashArray
    {
        get => GetValue(StrokeDashArrayProperty);
        set => SetValue(StrokeDashArrayProperty, value);
    }

    #endregion

    #region 内部属性定义

    internal static readonly StyledProperty<double> RelativeHeaderLineHeightProperty =
        AvaloniaProperty.Register<AbstractResult, double>(nameof(RelativeHeaderLineHeight));

    internal static readonly StyledProperty<double> RelativeSubHeaderLineHeightProperty =
        AvaloniaProperty.Register<AbstractResult, double>(nameof(RelativeSubHeaderLineHeight));

    internal static readonly DirectProperty<AbstractResult, double> HeaderLineHeightProperty =
        AvaloniaProperty.RegisterDirect<AbstractResult, double>(
            nameof(HeaderLineHeight),
            o => o.HeaderLineHeight,
            (o, v) => o.HeaderLineHeight = v);

    internal static readonly DirectProperty<AbstractResult, double> SubHeaderLineHeightProperty =
        AvaloniaProperty.RegisterDirect<AbstractResult, double>(
            nameof(SubHeaderLineHeight),
            o => o.SubHeaderLineHeight,
            (o, v) => o.SubHeaderLineHeight = v);

    internal static readonly DirectProperty<AbstractResult, object?> StatusIconProperty =
        AvaloniaProperty.RegisterDirect<AbstractResult, object?>(
            nameof(StatusIcon),
            o => o.StatusIcon,
            (o, v) => o.StatusIcon = v);

    internal double RelativeHeaderLineHeight
    {
        get => GetValue(RelativeHeaderLineHeightProperty);
        set => SetValue(RelativeHeaderLineHeightProperty, value);
    }

    internal double RelativeSubHeaderLineHeight
    {
        get => GetValue(RelativeSubHeaderLineHeightProperty);
        set => SetValue(RelativeSubHeaderLineHeightProperty, value);
    }

    private double _headerLineHeight;

    internal double HeaderLineHeight
    {
        get => _headerLineHeight;
        set => SetAndRaise(HeaderLineHeightProperty, ref _headerLineHeight, value);
    }

    private double _subHeaderLineHeight;

    internal double SubHeaderLineHeight
    {
        get => _subHeaderLineHeight;
        set => SetAndRaise(SubHeaderLineHeightProperty, ref _subHeaderLineHeight, value);
    }

    private object? _statusIcon;

    internal object? StatusIcon
    {
        get => _statusIcon;
        set => SetAndRaise(StatusIconProperty, ref _statusIcon, value);
    }
    #endregion

    private SvgControl? _statusImage;

    static AbstractResult()
    {
        AffectsMeasure<AbstractResult>(StatusProperty);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (this.IsAttachedToVisualTree())
        {
            if (change.Property == StatusProperty ||
                change.Property == IconProperty)
            {
                ConfigureStatusImage();
            }
        }

        if (change.Property == HeaderFontSizeProperty ||
            change.Property == RelativeHeaderLineHeightProperty)
        {
            ConfigureHeaderLineHeight();
        }

        if (change.Property == SubHeaderFontSizeProperty ||
            change.Property == RelativeSubHeaderLineHeightProperty)
        {
            ConfigureSubHeaderLineHeight();
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _statusImage = e.NameScope.Find<SvgControl>("PART_ErrorCodeImage");
        ConfigureStatusImage();
        ConfigureHeaderLineHeight();
        ConfigureSubHeaderLineHeight();
    }

    /// <summary>
    /// Creates the icon shown for <paramref name="status"/> when <see cref="Icon"/> is not set.
    /// Returns a fresh instance per call, since icons are controls and can have only one parent.
    /// </summary>
    private protected virtual object? CreateDefaultStatusIcon(ResultStatus status)
    {
        return status switch
        {
            ResultStatus.Info    => new ExclamationCircleFilled(),
            ResultStatus.Success => new CheckCircleFilled(),
            ResultStatus.Warning => new WarningFilled(),
            ResultStatus.Error   => new CloseCircleFilled(),
            ResultStatus.Waiting => new ClockCircleFilled(),
            _                    => null
        };
    }

    private void ConfigureStatusImage()
    {
        if (Status is ResultStatus.Info or ResultStatus.Success or ResultStatus.Warning or ResultStatus.Error or ResultStatus.Waiting)
        {
            SetCurrentValue(StatusIconProperty, Icon ?? CreateDefaultStatusIcon(Status));
        }

        if (_statusImage != null)
        {
            if (Status == ResultStatus.ErrorCode403)
            {
                _statusImage.Source = ResultIndicator.UnauthorizedImageSource();
            }
            else if (Status == ResultStatus.ErrorCode404)
            {
                _statusImage.Source = ResultIndicator.NotFoundImageSource();
            }
            else if (Status == ResultStatus.ErrorCode500)
            {
                _statusImage.Source = ResultIndicator.ServerErrorImageSource();
            }
        }
    }

    private protected virtual double HeaderLineHeightRatio => RelativeHeaderLineHeight;

    private protected virtual double SubHeaderLineHeightRatio => RelativeSubHeaderLineHeight;

    private protected void ConfigureHeaderLineHeight()
    {
        SetCurrentValue(HeaderLineHeightProperty, HeaderLineHeightRatio * HeaderFontSize);
    }

    private protected void ConfigureSubHeaderLineHeight()
    {
        SetCurrentValue(SubHeaderLineHeightProperty, SubHeaderLineHeightRatio * SubHeaderFontSize);
    }
}
