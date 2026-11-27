using System.Globalization;
using System.Reactive.Disposables;
using AtomUI.Controls;
using AtomUI.Controls.Commons;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

using IconControl = AtomUI.Controls.Icon;

/// <summary>
/// Result control for desktop layouts. While it is attached and <c>UseResultPresentationTokenResources</c> is
/// <see langword="true" />, which the desktop theme sets, it takes the line height ratios of the header and the
/// sub-header and the size and color of the status icon from the theme tokens. A line height ratio set
/// explicitly through <c>RelativeHeaderLineHeight</c> or <c>RelativeSubHeaderLineHeight</c> is kept instead of
/// the token value.
/// </summary>
public partial class Result : AbstractResult
{
    private const int HeaderRatioIndex = 0;
    private const int SubHeaderRatioIndex = 1;
    private const int IconSizeIndex = 2;
    private const int InfoColorIndex = 3;
    private const int SuccessColorIndex = 4;
    private const int WarningColorIndex = 5;
    private const int ErrorColorIndex = 6;

    private static readonly object[] s_styleTokenKeys =
    [
        ControlTokenResourceKey.Global(ResultTokens.Identity, SharedTokenKind.RelativeLineHeightHeading3),
        ControlTokenResourceKey.Global(ResultTokens.Identity, SharedTokenKind.RelativeLineHeight),
        ResultTokenKind.StatusIconSize,
        ResultTokenKind.ResultInfoIconColor,
        ResultTokenKind.ResultSuccessIconColor,
        ResultTokenKind.ResultWarningIconColor,
        ResultTokenKind.ResultErrorIconColor
    ];

    internal static readonly StyledProperty<bool> UseResultPresentationTokenResourcesProperty =
        AvaloniaProperty.Register<Result, bool>(nameof(UseResultPresentationTokenResources));

    internal bool UseResultPresentationTokenResources
    {
        get => GetValue(UseResultPresentationTokenResourcesProperty);
        set => SetValue(UseResultPresentationTokenResourcesProperty, value);
    }

    internal double StatusIconSize { get; private set; } = double.NaN;
    internal IBrush? StatusIconBrush { get; private set; }

    private ContentPresenter? _statusIconPresenter;
    private CompositeDisposable? _statusIconValues;
    private OwnerTokenSet? _styleTokens;
    private double _headerLineHeightRatio;
    private double _subHeaderLineHeightRatio;

    /// <summary>
    /// Height of one header line as a multiple of the header font size: the ratio last read from the theme
    /// Tokens, or the base value while <c>RelativeHeaderLineHeight</c> is set explicitly.
    /// </summary>
    private protected override double HeaderLineHeightRatio =>
        IsSet(RelativeHeaderLineHeightProperty) ? base.HeaderLineHeightRatio : _headerLineHeightRatio;

    /// <summary>
    /// Height of one sub-header line as a multiple of the sub-header font size: the ratio last read from the theme
    /// Tokens, or the base value while <c>RelativeSubHeaderLineHeight</c> is set explicitly.
    /// </summary>
    private protected override double SubHeaderLineHeightRatio =>
        IsSet(RelativeSubHeaderLineHeightProperty) ? base.SubHeaderLineHeightRatio : _subHeaderLineHeightRatio;

    public Result()
    {
    }

    /// <summary>
    /// Called when the control template is applied. It releases the handler and the size and color values that
    /// were held for the status icon of the previous template, then observes the <c>PART_StatusIconPresenter</c>
    /// of the new template and applies the current status icon size and color to its icon when the presenter has
    /// one. Overrides must call the base implementation.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (_statusIconPresenter != null)
        {
            _statusIconPresenter.PropertyChanged -= HandleStatusIconPresenterPropertyChanged;
        }

        _statusIconValues?.Dispose();
        _statusIconValues = null;

        _statusIconPresenter = e.NameScope.Find<ContentPresenter>("PART_StatusIconPresenter");

        if (_statusIconPresenter != null)
        {
            _statusIconPresenter.PropertyChanged += HandleStatusIconPresenterPropertyChanged;
            UpdateStatusIcon();
        }
    }

    /// <summary>
    /// Called when a property of this control changes. When <c>UseResultPresentationTokenResources</c> changes
    /// while the control is attached, the subscription to the theme tokens is created or released. When
    /// <see cref="AbstractResult.Status" /> changes while that subscription exists, the theme tokens are read
    /// again, which applies the status icon color of the new status. Overrides must call the base implementation.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == UseResultPresentationTokenResourcesProperty && this.IsAttachedToVisualTree())
        {
            UpdatePresentationTokenSubscription();
        }
        else if (change.Property == StatusProperty && _styleTokens is not null)
        {
            RefreshPresentationDefaults();
        }
    }

    /// <summary>
    /// Called when the control is attached to the visual tree. It creates the subscription to the theme tokens
    /// when <c>UseResultPresentationTokenResources</c> is set and then reads the line heights and the status icon
    /// size and color. Overrides must call the base implementation.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        UpdatePresentationTokenSubscription();
    }

    /// <summary>
    /// Called when the control is detached from the visual tree. It disposes the subscription to the theme tokens
    /// before it calls the base implementation. The line heights and the status icon keep the values that were
    /// read last. Overrides must call the base implementation.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _styleTokens?.Dispose();
        _styleTokens = null;
        base.OnDetachedFromVisualTree(e);
    }

    private void UpdatePresentationTokenSubscription()
    {
        if (UseResultPresentationTokenResources && _styleTokens is null)
        {
            _styleTokens = new OwnerTokenSet(this, s_styleTokenKeys.Length, RefreshPresentationDefaults);
            for (var index = 0; index < s_styleTokenKeys.Length; ++index)
            {
                _styleTokens.SetResourceKey(index, s_styleTokenKeys[index]);
            }
        }
        else if (!UseResultPresentationTokenResources && _styleTokens is not null)
        {
            _styleTokens.Dispose();
            _styleTokens = null;
        }
        RefreshPresentationDefaults();
    }

    private void RefreshPresentationDefaults()
    {
        var header = ReadNumber(_styleTokens?.GetValue(HeaderRatioIndex), _headerLineHeightRatio, 0);
        var subHeader = ReadNumber(_styleTokens?.GetValue(SubHeaderRatioIndex), _subHeaderLineHeightRatio, 0);
        var size = ReadNumber(_styleTokens?.GetValue(IconSizeIndex), StatusIconSize, double.NaN);
        var brush = ReadStatusBrush();
        var headerChanged = !_headerLineHeightRatio.Equals(header);
        var subHeaderChanged = !_subHeaderLineHeightRatio.Equals(subHeader);
        var iconChanged = !StatusIconSize.Equals(size) || !Equals(StatusIconBrush, brush);

        _headerLineHeightRatio = header;
        _subHeaderLineHeightRatio = subHeader;
        StatusIconSize = size;
        StatusIconBrush = brush;

        if (headerChanged)
        {
            ConfigureHeaderLineHeight();
        }
        if (subHeaderChanged)
        {
            ConfigureSubHeaderLineHeight();
        }
        if (iconChanged)
        {
            UpdateStatusIcon();
        }
    }

    private IBrush? ReadStatusBrush()
    {
        var index = Status switch
        {
            ResultStatus.Info => InfoColorIndex,
            ResultStatus.Success or ResultStatus.Waiting => SuccessColorIndex,
            ResultStatus.Warning => WarningColorIndex,
            ResultStatus.Error => ErrorColorIndex,
            _ => -1
        };
        if (index < 0)
        {
            return null;
        }

        return _styleTokens?.GetValue(index) switch
        {
            IBrush brush => brush,
            Color color => new ImmutableSolidColorBrush(color),
            string text when Color.TryParse(text, out var color) => new ImmutableSolidColorBrush(color),
            _ => null
        };
    }

    private static double ReadNumber(object? value, double previous, double fallback)
    {
        if (value is double number)
        {
            return number;
        }
        if (ReferenceEquals(value, BindingOperations.DoNothing))
        {
            return previous;
        }
        try
        {
            return value is IConvertible convertible ? convertible.ToDouble(CultureInfo.CurrentCulture) : fallback;
        }
        catch (Exception error) when (error is FormatException or InvalidCastException or OverflowException)
        {
            return fallback;
        }
    }

    private void HandleStatusIconPresenterPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == ContentPresenter.ChildProperty)
        {
            UpdateStatusIcon();
        }
    }

    private void UpdateStatusIcon()
    {
        // Status size and color are removable Template-priority values: they outrank the icon's own ControlTheme
        // setters, so a publish that re-resolves the icon tokens cannot repaint the status icon, while user local
        // values and style triggers on the icon keep winning. Reapplying disposes exactly the values applied here.
        _statusIconValues?.Dispose();
        _statusIconValues = null;

        if (_statusIconPresenter?.Child is not Control child)
        {
            return;
        }

        var values = new CompositeDisposable();
        var size   = StatusIconSize;
        if (!double.IsNaN(size))
        {
            AddValue(values, child.SetValue(WidthProperty, size, BindingPriority.Template));
            AddValue(values, child.SetValue(HeightProperty, size, BindingPriority.Template));
        }

        var brush = StatusIconBrush;
        if (brush != null)
        {
            AddValue(values, child.SetValue(ForegroundProperty, brush, BindingPriority.Template));
            if (child is IconControl icon)
            {
                AddValue(values, icon.SetValue(IconControl.FillBrushProperty, brush, BindingPriority.Template));
                AddValue(values, icon.SetValue(IconControl.StrokeBrushProperty, brush, BindingPriority.Template));
            }
        }

        _statusIconValues = values;
    }

    private static void AddValue(CompositeDisposable values, IDisposable? applied)
    {
        if (applied is not null)
        {
            values.Add(applied);
        }
    }
}
