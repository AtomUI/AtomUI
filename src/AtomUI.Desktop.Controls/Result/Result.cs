using System.Reactive.Disposables;
using AtomUI.Controls.Commons;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
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
    private CompositeDisposable? _statusIconValues;

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
