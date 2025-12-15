using System.Diagnostics;
using System.Reactive.Disposables;
using AtomUI.Data;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

internal class PopupConfirmFlyout : Flyout
{
    internal WeakReference<PopupConfirm> PopupConfirmRef { get; set; }
    
    private CompositeDisposable? _presenterBindingDisposables;

    public PopupConfirmFlyout(PopupConfirm popupConfirm)
    {
        PopupConfirmRef = new WeakReference<PopupConfirm>(popupConfirm);
    }

    protected override Control CreatePresenter()
    {
        var presenter = base.CreatePresenter() as FlyoutPresenter;
        Debug.Assert(presenter != null);
        _presenterBindingDisposables?.Dispose();
        _presenterBindingDisposables = new CompositeDisposable(10);
        if (PopupConfirmRef.TryGetTarget(out var popupConfirm))
        {
            var popupConfirmContainer = new PopupConfirmContainer(popupConfirm);
            _presenterBindingDisposables.Add(BindUtils.RelayBind(popupConfirm, PopupConfirm.OkTextProperty, popupConfirmContainer,
                PopupConfirmContainer.OkTextProperty));
            _presenterBindingDisposables.Add(BindUtils.RelayBind(popupConfirm, PopupConfirm.CancelTextProperty, popupConfirmContainer,
                PopupConfirmContainer.CancelTextProperty));
            _presenterBindingDisposables.Add(BindUtils.RelayBind(popupConfirm, PopupConfirm.OkButtonTypeProperty, popupConfirmContainer,
                PopupConfirmContainer.OkButtonTypeProperty));
            _presenterBindingDisposables.Add(BindUtils.RelayBind(popupConfirm, PopupConfirm.IsShowCancelButtonProperty, popupConfirmContainer,
                PopupConfirmContainer.IsShowCancelButtonProperty));
            _presenterBindingDisposables.Add(BindUtils.RelayBind(popupConfirm, PopupConfirm.TitleProperty, popupConfirmContainer,
                PopupConfirmContainer.TitleProperty));
            _presenterBindingDisposables.Add(BindUtils.RelayBind(popupConfirm, PopupConfirm.ConfirmStatusProperty, popupConfirmContainer,
                PopupConfirmContainer.ConfirmStatusProperty));
            _presenterBindingDisposables.Add(BindUtils.RelayBind(popupConfirm, PopupConfirm.IconProperty, popupConfirmContainer,
                PopupConfirmContainer.IconProperty));
            _presenterBindingDisposables.Add(BindUtils.RelayBind(popupConfirm, PopupConfirm.ConfirmContentProperty, popupConfirmContainer,
                PopupConfirmContainer.ConfirmContentProperty));
            _presenterBindingDisposables.Add(BindUtils.RelayBind(popupConfirm, PopupConfirm.ConfirmContentTemplateProperty, popupConfirmContainer,
                PopupConfirmContainer.ConfirmContentTemplateProperty));
            
            presenter.Content = popupConfirmContainer;
        }
        return presenter;
    }
}