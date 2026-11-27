using System.Reactive.Disposables;
using AtomUI.Controls.Commons;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Host that shows a single <see cref="FloatButton" /> in the scoped overlay layer of the nearest layer host.
/// The button is created at run time and follows the properties set on the host.
/// </summary>
public class FloatButtonHost : AbstractFloatButtonHost
{
    /// <summary>
    /// Creates the <see cref="FloatButton" /> that the host shows in the scoped overlay layer and binds the properties
    /// of the host to it. It is called when the host is attached to a visual tree and has no button yet.
    /// </summary>
    /// <param name="disposables">
    /// Receives the bindings between the host and the button. It is disposed when the host is detached.
    /// </param>
    protected override AbstractFloatButton CreateFloatButton(CompositeDisposable disposables)
    {
        var floatButton = new FloatButton();
        BindFloatButtonProperties(floatButton, disposables);
        return floatButton;
    }
}
