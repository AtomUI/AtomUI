using System.Reactive.Disposables;
using AtomUI.Controls.Commons;
using AtomUI.Data;

namespace AtomUI.Desktop.Controls;

public class BackTopFloatButtonHost : AbstractBackTopFloatButtonHost
{
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
