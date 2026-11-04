using System.Reactive.Linq;
using AtomUI.Theme;
using Avalonia;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

internal sealed class DataGridFilterThemeScope : IDisposable
{
    private readonly Control _presenter;
    private readonly IDisposable _contextSubscription;
    private ThemeContextResourceBridge? _bridge;

    internal DataGridFilterThemeScope(Control presenter)
    {
        _presenter = presenter;
        _contextSubscription = presenter.GetObservable(ThemeScope.ContextProperty).Subscribe(UpdateContext);
    }

    public void Dispose()
    {
        _contextSubscription.Dispose();
        ClearBridge();
    }

    private void UpdateContext(ThemeContext? context)
    {
        if (context is not null && ReferenceEquals(_bridge?.OwnerContext, context))
        {
            return;
        }
        ClearBridge();
        if (context is not null)
        {
            // Runtime filter popup roots have an inheritance parent without membership
            // in its logical children. ControlTheme can bind values, but cannot forward
            // the missing resource publication. Keep that bridge within the open root.
            _bridge = new ThemeContextResourceBridge(context);
            _presenter.Resources.MergedDictionaries.Add(_bridge);
        }
    }

    private void ClearBridge()
    {
        if (_bridge is { } bridge)
        {
            _bridge = null;
            _presenter.Resources.MergedDictionaries.Remove(bridge);
            bridge.Dispose();
        }
    }
}
