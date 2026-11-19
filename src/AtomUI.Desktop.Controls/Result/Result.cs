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

    private protected override double HeaderLineHeightRatio =>
        IsSet(RelativeHeaderLineHeightProperty) ? base.HeaderLineHeightRatio : _headerLineHeightRatio;

    private protected override double SubHeaderLineHeightRatio =>
        IsSet(RelativeSubHeaderLineHeightProperty) ? base.SubHeaderLineHeightRatio : _subHeaderLineHeightRatio;

    public Result()
    {
    }

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

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        UpdatePresentationTokenSubscription();
    }

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
        // Status size and colour are removable Template-priority values: they outrank the icon's own ControlTheme
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
