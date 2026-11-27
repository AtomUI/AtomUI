using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;
using Avalonia.VisualTree;
using AtomUI.Icons.AntDesign;

namespace AtomUI.Controls.Commons;

using SvgControl = Avalonia.Svg.Svg;

/// <summary>
/// Base class of controls that report the outcome of an operation. A result stacks, in this order, a status
/// icon or illustration, the header (<see cref="Header" />), the sub-header (<see cref="SubHeader" />), the
/// extra content (<see cref="Extra" />) and the body (<see cref="ContentControl.Content" />).
/// </summary>
public abstract class AbstractResult : ContentControl
{
    #region 公共属性定义
    /// <summary>
    /// Identifies the <see cref="Extra" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> ExtraProperty =
        AvaloniaProperty.Register<AbstractResult, object?>(nameof(Extra));

    /// <summary>
    /// Identifies the <see cref="ExtraTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> ExtraTemplateProperty =
        AvaloniaProperty.Register<AbstractResult, IDataTemplate?>(nameof(ExtraTemplate));

    /// <summary>
    /// Identifies the <see cref="Icon" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> IconProperty =
        AvaloniaProperty.Register<AbstractResult, object?>(nameof(Icon));

    /// <summary>
    /// Identifies the <see cref="Status" /> property.
    /// </summary>
    public static readonly StyledProperty<ResultStatus> StatusProperty =
        AvaloniaProperty.Register<AbstractResult, ResultStatus>(nameof(Status));

    /// <summary>
    /// Identifies the <see cref="Header" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> HeaderProperty =
        AvaloniaProperty.Register<AbstractResult, object?>(nameof(Header));

    /// <summary>
    /// Identifies the <see cref="HeaderTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> HeaderTemplateProperty =
        AvaloniaProperty.Register<AbstractResult, IDataTemplate?>(nameof(HeaderTemplate));

    /// <summary>
    /// Identifies the <see cref="SubHeader" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> SubHeaderProperty =
        AvaloniaProperty.Register<AbstractResult, object?>(nameof(SubHeader));

    /// <summary>
    /// Identifies the <see cref="SubHeaderTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> SubHeaderTemplateProperty =
        AvaloniaProperty.Register<AbstractResult, IDataTemplate?>(nameof(SubHeaderTemplate));

    /// <summary>
    /// Identifies the <see cref="HeaderFontSize" /> property.
    /// </summary>
    public static readonly StyledProperty<double> HeaderFontSizeProperty =
        AvaloniaProperty.Register<AbstractResult, double>(nameof(HeaderFontSize));

    /// <summary>
    /// Identifies the <see cref="SubHeaderFontSize" /> property.
    /// </summary>
    public static readonly StyledProperty<double> SubHeaderFontSizeProperty =
        AvaloniaProperty.Register<AbstractResult, double>(nameof(SubHeaderFontSize));

    /// <summary>
    /// Identifies the <see cref="StrokeDashArray" /> property.
    /// </summary>
    public static readonly StyledProperty<IReadOnlyList<double>?> StrokeDashArrayProperty =
        AvaloniaProperty.Register<AbstractResult, IReadOnlyList<double>?>(nameof(StrokeDashArray));

    /// <summary>
    /// Content of the area under the sub-header, typically the actions. <see langword="null" />, the default, leaves
    /// the area empty.
    /// Without <see cref="ExtraTemplate" />, a string is shown as text and a control as is.
    /// </summary>
    [DependsOn(nameof(ExtraTemplate))]
    public object? Extra
    {
        get => GetValue(ExtraProperty);
        set => SetValue(ExtraProperty, value);
    }

    /// <summary>
    /// Template that presents <see cref="Extra" />. <see langword="null" />, the default, shows a string as text and
    /// a control as is.
    /// </summary>
    public IDataTemplate? ExtraTemplate
    {
        get => GetValue(ExtraTemplateProperty);
        set => SetValue(ExtraTemplateProperty, value);
    }

    /// <summary>
    /// Icon shown above the header in place of the built-in status icon. <see langword="null" />, the default, shows
    /// the built-in icon. <see cref="ResultStatus.ErrorCode403" />, <see cref="ResultStatus.ErrorCode404" /> and
    /// <see cref="ResultStatus.ErrorCode500" /> show an illustration instead, so this value does not apply to them.
    /// A control set here is shown as is, so the same instance cannot be used by two results.
    /// </summary>
    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>
    /// Outcome the result reports. It selects what is shown above the header: <see cref="ResultStatus.Info" />,
    /// <see cref="ResultStatus.Success" />, <see cref="ResultStatus.Warning" />, <see cref="ResultStatus.Error" />
    /// and <see cref="ResultStatus.Waiting" /> show the status icon, and <see cref="ResultStatus.ErrorCode403" />,
    /// <see cref="ResultStatus.ErrorCode404" /> and <see cref="ResultStatus.ErrorCode500" /> show a built-in
    /// illustration. The default is <see cref="ResultStatus.Info" />.
    /// </summary>
    public ResultStatus Status
    {
        get => GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }

    /// <summary>
    /// Title of the result, centered above the sub-header. <see langword="null" />, the default, hides the title.
    /// Without <see cref="HeaderTemplate" />, a string is shown as wrapping text and a control as is.
    /// </summary>
    [DependsOn(nameof(HeaderTemplate))]
    public object? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    /// <summary>
    /// Template that presents <see cref="Header" />. <see langword="null" />, the default, shows a string as
    /// wrapping text and a control as is.
    /// </summary>
    public IDataTemplate? HeaderTemplate
    {
        get => GetValue(HeaderTemplateProperty);
        set => SetValue(HeaderTemplateProperty, value);
    }

    /// <summary>
    /// Sub-header of the result: the secondary text under the title. <see langword="null" />, the default, hides it.
    /// Without <see cref="SubHeaderTemplate" />, a string is shown as wrapping text and a control as is.
    /// </summary>
    [DependsOn(nameof(SubHeaderTemplate))]
    public object? SubHeader
    {
        get => GetValue(SubHeaderProperty);
        set => SetValue(SubHeaderProperty, value);
    }

    /// <summary>
    /// Template that presents <see cref="SubHeader" />. <see langword="null" />, the default, shows a string as
    /// wrapping text and a control as is.
    /// </summary>
    public IDataTemplate? SubHeaderTemplate
    {
        get => GetValue(SubHeaderTemplateProperty);
        set => SetValue(SubHeaderTemplateProperty, value);
    }

    /// <summary>
    /// Font size of the header. The line height of the header is this size multiplied by the line-height ratio in
    /// effect, so it changes with the font size.
    /// The theme of the derived control supplies the value while it is not set here.
    /// </summary>
    public double HeaderFontSize
    {
        get => GetValue(HeaderFontSizeProperty);
        set => SetValue(HeaderFontSizeProperty, value);
    }

    /// <summary>
    /// Font size of the sub-header. The line height of the sub-header is this size multiplied by the line-height
    /// ratio in effect, so it changes with the font size.
    /// The theme of the derived control supplies the value while it is not set here.
    /// </summary>
    public double SubHeaderFontSize
    {
        get => GetValue(SubHeaderFontSizeProperty);
        set => SetValue(SubHeaderFontSizeProperty, value);
    }

    /// <summary>
    /// Dash pattern of the border around the result. <see langword="null" />, the default, and an empty list draw a
    /// solid border.
    /// </summary>
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

    /// <summary>
    /// The line height of the header text, in DIPs. The control calculates it as <see cref="HeaderFontSize" />
    /// multiplied by <see cref="HeaderLineHeightRatio" /> when the template is applied and when
    /// <see cref="HeaderFontSize" /> or <see cref="RelativeHeaderLineHeight" /> changes; the theme binds the line
    /// height of the header text to it. It is zero until it is first calculated.
    /// </summary>
    internal double HeaderLineHeight
    {
        get => _headerLineHeight;
        set => SetAndRaise(HeaderLineHeightProperty, ref _headerLineHeight, value);
    }

    private double _subHeaderLineHeight;

    /// <summary>
    /// The line height of the sub-header text, in DIPs. The control calculates it as
    /// <see cref="SubHeaderFontSize" /> multiplied by <see cref="SubHeaderLineHeightRatio" /> when the template is
    /// applied and when <see cref="SubHeaderFontSize" /> or <see cref="RelativeSubHeaderLineHeight" /> changes; the
    /// theme binds the line height of the sub-header text to it. It is zero until it is first calculated.
    /// </summary>
    internal double SubHeaderLineHeight
    {
        get => _subHeaderLineHeight;
        set => SetAndRaise(SubHeaderLineHeightProperty, ref _subHeaderLineHeight, value);
    }

    private object? _statusIcon;

    /// <summary>
    /// The status icon shown above the header; the theme presents it as content. The control writes it when the
    /// template is applied and when <see cref="Status" /> or <see cref="Icon" /> changes while the control is
    /// attached to a visual tree: <see cref="Icon" /> when it is set, otherwise a new instance of the built-in
    /// icon of <see cref="Status" />. A status that shows an illustration leaves the value unchanged.
    /// </summary>
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

    /// <summary>
    /// While the control is attached to a visual tree, updates the status icon when <see cref="Status" /> or
    /// <see cref="Icon" /> changes. Recalculates the line height of the header or of the sub-header when its font
    /// size or line-height ratio changes. Overrides must call the base member.
    /// </summary>
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

    /// <summary>
    /// When the control template is applied, looks up the template part <c>PART_ErrorCodeImage</c> and applies the
    /// status icon and both line heights. Overrides must call the base member.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _statusImage = e.NameScope.Find<SvgControl>("PART_ErrorCodeImage");
        ConfigureStatusImage();
        ConfigureHeaderLineHeight();
        ConfigureSubHeaderLineHeight();
    }

    /// <summary>
    /// Creates the icon shown for <paramref name="status" /> when <see cref="Icon" /> is not set. A new instance is
    /// created per call, because an icon is a control and can have only one parent.
    /// Returns <see langword="null" /> for <see cref="ResultStatus.ErrorCode403" />,
    /// <see cref="ResultStatus.ErrorCode404" /> and <see cref="ResultStatus.ErrorCode500" />, which show an
    /// illustration instead.
    /// This class calls it when <see cref="Status" /> or <see cref="Icon" /> changes while the control is attached to
    /// a visual tree, and when the template is applied. An override does not have to call the base member.
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

    /// <summary>
    /// Height of one header line as a multiple of <c>HeaderFontSize</c>. <c>ConfigureHeaderLineHeight</c> writes
    /// the product into <c>HeaderLineHeight</c> when <c>HeaderFontSize</c> or <c>RelativeHeaderLineHeight</c>
    /// changes and when the template is applied. The base member returns <c>RelativeHeaderLineHeight</c>; an
    /// override that returns a theme value should keep the base value while <c>RelativeHeaderLineHeight</c> is
    /// set explicitly.
    /// </summary>
    private protected virtual double HeaderLineHeightRatio => RelativeHeaderLineHeight;

    /// <summary>
    /// Height of one sub-header line as a multiple of <c>SubHeaderFontSize</c>.
    /// <c>ConfigureSubHeaderLineHeight</c> writes the product into <c>SubHeaderLineHeight</c> when
    /// <c>SubHeaderFontSize</c> or <c>RelativeSubHeaderLineHeight</c> changes and when the template is applied.
    /// The base member returns <c>RelativeSubHeaderLineHeight</c>; an override that returns a theme value should
    /// keep the base value while <c>RelativeSubHeaderLineHeight</c> is set explicitly.
    /// </summary>
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
