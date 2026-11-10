using System.Reactive.Disposables;
using AtomUI.Controls;
using AtomUI.Data;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Controls;

namespace AtomUI.Toolkits.GalleryBase.Shell;

internal sealed class GalleryMediaBreakPointResolver : IDisposable
{
    private static readonly AttachedProperty<int> ScreenXXXLMinProperty =
        AvaloniaProperty.RegisterAttached<GalleryBrowserShellView, Control, int>("ScreenXXXLMin");
    private static readonly AttachedProperty<int> ScreenXXLMinProperty =
        AvaloniaProperty.RegisterAttached<GalleryBrowserShellView, Control, int>("ScreenXXLMin");
    private static readonly AttachedProperty<int> ScreenXLMinProperty =
        AvaloniaProperty.RegisterAttached<GalleryBrowserShellView, Control, int>("ScreenXLMin");
    private static readonly AttachedProperty<int> ScreenLGMinProperty =
        AvaloniaProperty.RegisterAttached<GalleryBrowserShellView, Control, int>("ScreenLGMin");
    private static readonly AttachedProperty<int> ScreenMDMinProperty =
        AvaloniaProperty.RegisterAttached<GalleryBrowserShellView, Control, int>("ScreenMDMin");
    private static readonly AttachedProperty<int> ScreenSMMinProperty =
        AvaloniaProperty.RegisterAttached<GalleryBrowserShellView, Control, int>("ScreenSMMin");

    private static readonly (AttachedProperty<int> Property, SharedTokenKind Token, MediaBreakPoint BreakPoint)[] s_thresholds =
    [
        (ScreenXXXLMinProperty, SharedTokenKind.ScreenXXXLMin, MediaBreakPoint.ExtraExtraExtraLarge),
        (ScreenXXLMinProperty, SharedTokenKind.ScreenXXLMin, MediaBreakPoint.ExtraExtraLarge),
        (ScreenXLMinProperty, SharedTokenKind.ScreenXLMin, MediaBreakPoint.ExtraLarge),
        (ScreenLGMinProperty, SharedTokenKind.ScreenLGMin, MediaBreakPoint.Large),
        (ScreenMDMinProperty, SharedTokenKind.ScreenMDMin, MediaBreakPoint.Medium),
        (ScreenSMMinProperty, SharedTokenKind.ScreenSMMin, MediaBreakPoint.Small)
    ];
    private readonly Control _host;
    private readonly Action _changed;
    private readonly CompositeDisposable _bindings = new();

    internal GalleryMediaBreakPointResolver(Control host, Action changed)
    {
        _host = host;
        _changed = changed;
        // These runtime-created shell hosts have no ControlTheme/template. Binding the
        // properties on the real host preserves its local scope and resource lifecycle.
        foreach (var threshold in s_thresholds)
        {
            _bindings.Add(TokenResourceBinder.CreateGlobalTokenBinding(host, threshold.Property, threshold.Token));
        }
        host.PropertyChanged += HandlePropertyChanged;
    }

    internal MediaBreakPoint Resolve(double width)
    {
        foreach (var threshold in s_thresholds)
        {
            if (width >= _host.GetValue(threshold.Property))
            {
                return threshold.BreakPoint;
            }
        }
        return MediaBreakPoint.ExtraSmall;
    }

    public void Dispose()
    {
        _host.PropertyChanged -= HandlePropertyChanged;
        _bindings.Dispose();
    }

    private void HandlePropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs change)
    {
        if (s_thresholds.Any(item => item.Property == change.Property))
        {
            _changed();
        }
    }

}
