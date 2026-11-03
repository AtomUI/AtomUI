using Avalonia.Controls;
using System.Reactive.Disposables;
using AtomUI.Data;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using AtomUI.Controls.Commons;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

using IconControl = AtomUI.Controls.Icon;

public partial class Result : AbstractResult
{
    #region 内部属性定义

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

    #endregion

    private ContentPresenter? _statusIconPresenter;
    private CompositeDisposable? _glyphBindings;

    public Result()
    {
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        ReleaseGlyphBindings();
        if (_statusIconPresenter is not null)
        {
            _statusIconPresenter.PropertyChanged -= HandlePresenterPropertyChanged;
        }
        base.OnApplyTemplate(e);
        _statusIconPresenter = e.NameScope.Find<ContentPresenter>("PART_StatusIconPresenter");
        ObservePresenter();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        ObservePresenter();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (_statusIconPresenter is not null)
        {
            _statusIconPresenter.PropertyChanged -= HandlePresenterPropertyChanged;
        }
        ReleaseGlyphBindings();
        base.OnDetachedFromVisualTree(e);
    }

    private void ObservePresenter()
    {
        if (_statusIconPresenter is not null)
        {
            _statusIconPresenter.PropertyChanged -= HandlePresenterPropertyChanged;
            _statusIconPresenter.PropertyChanged += HandlePresenterPropertyChanged;
        }
        ConfigureGlyphBindings();
    }

    private void HandlePresenterPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == ContentPresenter.ChildProperty)
        {
            ConfigureGlyphBindings();
        }
    }

    private void ConfigureGlyphBindings()
    {
        ReleaseGlyphBindings();
        if (_statusIconPresenter?.Child is not IconControl glyph)
        {
            return;
        }
        // The runtime Content has no TemplatedParent. A real selector probe
        // proved this ControlTheme's template styles are not applied to it,
        // despite its logical Presenter parent. Typed projections therefore live
        // with this membership at Template priority: above the Icon's base theme,
        // below native glyph values, and disposable on replacement/detach.
        _glyphBindings = new CompositeDisposable
        {
            BindUtils.RelayBind(this, StatusIconSizeProperty, glyph, WidthProperty, BindingMode.OneWay, BindingPriority.Template),
            BindUtils.RelayBind(this, StatusIconSizeProperty, glyph, HeightProperty, BindingMode.OneWay, BindingPriority.Template),
            BindUtils.RelayBind(this, StatusIconBrushProperty, glyph, ForegroundProperty, BindingMode.OneWay, BindingPriority.Template),
            BindUtils.RelayBind(this, StatusIconBrushProperty, glyph, IconControl.FillBrushProperty, BindingMode.OneWay, BindingPriority.Template),
            BindUtils.RelayBind(this, StatusIconBrushProperty, glyph, IconControl.StrokeBrushProperty, BindingMode.OneWay, BindingPriority.Template)
        };
    }

    private void ReleaseGlyphBindings()
    {
        _glyphBindings?.Dispose();
        _glyphBindings = null;
    }
}
