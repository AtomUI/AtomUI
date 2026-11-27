using System.Reactive.Disposables;
using AtomUI.Controls.Commons;
using AtomUI.Data;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// The desktop host that shows a <see cref="BackTopFloatButton" /> in the scoped overlay layer of the nearest
/// layer host. The button is created when the host is attached to a visual tree, and the properties set on the
/// host are forwarded to it.
/// </summary>
public class BackTopFloatButtonHost : AbstractBackTopFloatButtonHost
{
    /// <summary>
    /// Creates a <see cref="BackTopFloatButton" /> and binds the properties of the host to it: those that
    /// <c>BindFloatButtonProperties</c> forwards, plus <c>ToTopDuration</c>,
    /// <c>Target</c>, <c>VisibilityHeight</c>, <c>MotionDuration</c> and
    /// <c>IsShowProgress</c>. The bindings are added to <paramref name="disposables" />.
    /// </summary>
    protected override AbstractFloatButton CreateFloatButton(CompositeDisposable disposables)
    {
        var floatButton = new BackTopFloatButton();

        BindFloatButtonProperties(floatButton, disposables);
        disposables.Add(BindUtils.RelayBind(this, ToTopDurationProperty, floatButton, BackTopFloatButton.ToTopDurationProperty));
        disposables.Add(BindUtils.RelayBind(this, TargetProperty, floatButton, BackTopFloatButton.TargetProperty));
        disposables.Add(BindUtils.RelayBind(this, VisibilityHeightProperty, floatButton, BackTopFloatButton.VisibilityHeightProperty));
        disposables.Add(BindUtils.RelayBind(this, MotionDurationProperty, floatButton, BackTopFloatButton.MotionDurationProperty));
        disposables.Add(BindUtils.RelayBind(this, IsShowProgressProperty, floatButton, BackTopFloatButton.IsShowProgressProperty));
        
        return floatButton;
    }
}
