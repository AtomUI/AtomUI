using AtomUI.Controls.Commons;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

using IconControl = AtomUI.Controls.Icon;

public partial class Result : AbstractResult
{
    internal static readonly StyledProperty<double> StatusIconSizeProperty =
        AvaloniaProperty.Register<Result, double>(nameof(StatusIconSize), double.NaN);

    internal static readonly StyledProperty<IBrush?> StatusIconBrushProperty =
        AvaloniaProperty.Register<Result, IBrush?>(nameof(StatusIconBrush));

    internal double StatusIconSize
    {
        get => GetValue(StatusIconSizeProperty);
        set => SetValue(StatusIconSizeProperty, value);
    }

    internal IBrush? StatusIconBrush
    {
        get => GetValue(StatusIconBrushProperty);
        set => SetValue(StatusIconBrushProperty, value);
    }

    private ContentPresenter? _statusIconPresenter;

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
        if (change.Property == StatusIconSizeProperty ||
            change.Property == StatusIconBrushProperty)
        {
            UpdateStatusIcon();
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
        if (_statusIconPresenter?.Child is not Control child)
        {
            return;
        }

        var size = StatusIconSize;
        if (!double.IsNaN(size))
        {
            child.SetCurrentValue(WidthProperty, size);
            child.SetCurrentValue(HeightProperty, size);
        }

        var brush = StatusIconBrush;
        if (brush != null)
        {
            child.SetCurrentValue(ForegroundProperty, brush);
            if (child is IconControl icon)
            {
                icon.SetCurrentValue(IconControl.FillBrushProperty, brush);
                icon.SetCurrentValue(IconControl.StrokeBrushProperty, brush);
            }
        }
    }
}
