using Avalonia;
using Avalonia.Interactivity;
using ReactiveUI;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// A ReactiveUI <see cref="Window" /> that implements the <see cref="IViewFor" /> interface and will
/// activate your ViewModel automatically if the view model implements <see cref="IActivatableViewModel" />. When
/// the DataContext property changes, this class will update the ViewModel property with the new DataContext value,
/// and vice versa.
/// </summary>
/// <typeparam name="TViewModel">
/// ViewModel type.
/// </typeparam>
public class ReactiveWindow<TViewModel> : Window, IViewFor<TViewModel> where TViewModel : class
{
    /// <summary>
    /// Identifies the <see cref="ViewModel" /> property.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("AvaloniaProperty", "AVP1002",
        Justification = "Generic avalonia property is expected here.")]
    public static readonly StyledProperty<TViewModel?> ViewModelProperty = 
        AvaloniaProperty.Register<ReactiveWindow<TViewModel>, TViewModel?>(nameof(ViewModel));

    private IDisposable? _viewModelActivationDisposable;
    private object? _activatedViewModel;
    private bool _isViewActivated;
    private bool _isUpdatingViewModelActivation;
    private bool _hasPendingViewModelActivationUpdate;

    /// <summary>
    /// The ViewModel.
    /// </summary>
    public TViewModel? ViewModel
    {
        get => GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    object? IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = (TViewModel?)value;
    }

    /// <summary>
    /// Calls the base method first, then keeps <see cref="ViewModel" /> and <c>DataContext</c> equal in either
    /// direction while the two were equal before the change, and re-runs the activation when <see cref="ViewModel" />
    /// is replaced while the view is activated. A <c>DataContext</c> that is neither <see langword="null" /> nor a
    /// <typeparamref name="TViewModel" /> is not copied. Derived overrides must call the base method.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == DataContextProperty)
        {
            if (ReferenceEquals(change.OldValue, ViewModel)
                && change.NewValue is null or TViewModel)
            {
                SetCurrentValue(ViewModelProperty, change.NewValue);
            }
        }
        else if (change.Property == ViewModelProperty)
        {
            if (ReferenceEquals(change.OldValue, DataContext))
            {
                SetCurrentValue(DataContextProperty, change.NewValue);
            }

            if (_isViewActivated && !ReferenceEquals(change.OldValue, change.NewValue))
            {
                UpdateViewModelActivation();
            }
        }
    }

    /// <summary>
    /// Calls the base method first, then marks the view as activated and activates <see cref="ViewModel" /> when it
    /// implements <see cref="IActivatableViewModel" />. A call while the view is already activated changes nothing.
    /// Derived overrides must call the base method, otherwise the view model is never activated.
    /// </summary>
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        if (_isViewActivated)
        {
            return;
        }

        _isViewActivated = true;
        UpdateViewModelActivation();
    }

    /// <summary>
    /// Deactivates the activated <see cref="ViewModel" /> and then calls the base method; when the view is not
    /// activated it only calls the base method. Derived overrides must call the base method, otherwise the view model
    /// stays activated.
    /// </summary>
    protected override void OnUnloaded(RoutedEventArgs e)
    {
        if (!_isViewActivated)
        {
            base.OnUnloaded(e);
            return;
        }

        _isViewActivated = false;
        UpdateViewModelActivation();

        base.OnUnloaded(e);
    }

    private void UpdateViewModelActivation()
    {
        if (_isUpdatingViewModelActivation)
        {
            _hasPendingViewModelActivationUpdate = true;
            return;
        }

        _isUpdatingViewModelActivation = true;
        try
        {
            do
            {
                _hasPendingViewModelActivationUpdate = false;
                var targetViewModel = _isViewActivated ? ViewModel : null;
                if (ReferenceEquals(_activatedViewModel, targetViewModel))
                {
                    continue;
                }

                DeactivateViewModel();

                targetViewModel = _isViewActivated ? ViewModel : null;
                if (targetViewModel is IActivatableViewModel activatableViewModel)
                {
                    var activationDisposable = activatableViewModel.Activator.Activate();
                    _activatedViewModel = targetViewModel;
                    _viewModelActivationDisposable = activationDisposable;
                }
            } while (_hasPendingViewModelActivationUpdate);
        }
        finally
        {
            _isUpdatingViewModelActivation = false;
        }
    }

    private void DeactivateViewModel()
    {
        var activationDisposable = _viewModelActivationDisposable;
        _viewModelActivationDisposable = null;
        _activatedViewModel = null;
        activationDisposable?.Dispose();
    }
}
