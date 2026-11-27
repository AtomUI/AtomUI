using System.Reactive.Disposables;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using ReactiveUI;

namespace AtomUI.Toolkits.GalleryBase.Controls;

/// <summary>
/// Base class of a showcase view whose logic lives in a view model. It keeps <c>DataContext</c> and
/// <see cref="ViewModel" /> in step, runs the blocks registered through <see cref="WhenActivated" /> while the
/// control is loaded, and activates the view model when it implements <see cref="IActivatableViewModel" />.
/// </summary>
/// <typeparam name="TViewModel">
/// The type of the view model held by <see cref="ViewModel" />.
/// </typeparam>
public class GalleryReactiveUserControl<TViewModel> : UserControl, IViewFor<TViewModel>
    where TViewModel : class
{
    /// <summary>
    /// Identifies the <see cref="ViewModel" /> property.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("AvaloniaProperty", "AVP1002",
        Justification = "Generic avalonia property is expected here.")]
    public static readonly StyledProperty<TViewModel?> ViewModelProperty =
        AvaloniaProperty.Register<GalleryReactiveUserControl<TViewModel>, TViewModel?>(nameof(ViewModel));

    private readonly List<ActivationRegistration> _activationRegistrations = [];
    private CompositeDisposable? _activationDisposables;
    private IDisposable? _viewModelActivationDisposable;
    private object? _activatedViewModel;
    private bool _isViewActivated;
    private bool _isUpdatingViewModelActivation;
    private bool _hasPendingViewModelActivationUpdate;

    /// <summary>
    /// The view model of the view.
    /// <c>DataContext</c> and this property follow each other as long as they hold the same instance: a change of
    /// either one sets the other to the new value. A <c>DataContext</c> change is followed only when its new value
    /// is <see langword="null" /> or an instance of <typeparamref name="TViewModel" />; a value of any other type
    /// leaves this property unchanged. From then on each property keeps its own value until they hold the same
    /// instance again.
    /// While the control is loaded, a change deactivates the previous view model and activates the new one when it
    /// implements <see cref="IActivatableViewModel" />.
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
    /// Registers a block that sets up the bindings and subscriptions of the view for one activation.
    /// </summary>
    /// <param name="activationBlock">
    /// The block to run. It receives the disposables of the current activation; they are disposed when the control
    /// is unloaded.
    /// </param>
    /// <returns>
    /// A disposable that removes the registration. While the control is activated, it also disposes what the block
    /// added to the disposables.
    /// </returns>
    /// <remarks>
    /// The block runs each time the control is loaded, and at once when the control is already activated. When the
    /// block throws during such an immediate run, the registration is removed and the exception propagates.
    /// </remarks>
    public IDisposable WhenActivated(Action<CompositeDisposable> activationBlock)
    {
        ArgumentNullException.ThrowIfNull(activationBlock);

        var registration = new ActivationRegistration(this, activationBlock);
        _activationRegistrations.Add(registration);
        try
        {
            if (_isViewActivated && _activationDisposables is not null)
            {
                registration.Activate(_activationDisposables);
            }
        }
        catch
        {
            registration.Dispose();
            throw;
        }

        return registration;
    }

    /// <summary>
    /// Keeps <c>DataContext</c> and <see cref="ViewModel" /> in step when either of them changes, and swaps the
    /// activated view model when <see cref="ViewModel" /> changes while the control is activated. The base method is
    /// called first.
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
    /// Activates the view when the control is loaded: it activates <see cref="ViewModel" /> when the view model
    /// implements <see cref="IActivatableViewModel" />, then runs every block registered through
    /// <see cref="WhenActivated" />. The base method is called first. When a block throws, the view is deactivated
    /// again before the exception propagates.
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
        _activationDisposables = new CompositeDisposable();
        try
        {
            foreach (var activationRegistration in _activationRegistrations.ToArray())
            {
                activationRegistration.Activate(_activationDisposables);
            }
        }
        catch
        {
            _isViewActivated = false;
            UpdateViewModelActivation();
            DisposeViewActivation();
            throw;
        }
    }

    /// <summary>
    /// Deactivates an activated view when the control is unloaded: it deactivates <see cref="ViewModel" /> and
    /// disposes what the activation blocks added. The registered blocks stay and run again on the next load. The
    /// base method is called last.
    /// </summary>
    protected override void OnUnloaded(RoutedEventArgs e)
    {
        if (_isViewActivated)
        {
            _isViewActivated = false;
            UpdateViewModelActivation();
            DisposeViewActivation();
        }

        base.OnUnloaded(e);
    }

    private void DisposeViewActivation()
    {
        var activationDisposables = _activationDisposables;
        _activationDisposables = null;
        activationDisposables?.Dispose();
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

    private sealed class ActivationRegistration : IDisposable
    {
        private readonly GalleryReactiveUserControl<TViewModel> _owner;
        private readonly Action<CompositeDisposable> _activationBlock;
        private CompositeDisposable? _activeDisposables;
        private CompositeDisposable? _viewDisposables;
        private bool _disposed;

        public ActivationRegistration(
            GalleryReactiveUserControl<TViewModel> owner,
            Action<CompositeDisposable> activationBlock)
        {
            _owner           = owner;
            _activationBlock = activationBlock;
        }

        public void Activate(CompositeDisposable viewDisposables)
        {
            if (_disposed)
            {
                return;
            }

            DisposeActiveDisposables();

            var activeDisposables = new CompositeDisposable();
            _activeDisposables = activeDisposables;
            _viewDisposables   = viewDisposables;
            viewDisposables.Add(activeDisposables);
            try
            {
                _activationBlock(activeDisposables);
            }
            catch
            {
                DisposeActiveDisposables();
                throw;
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _owner._activationRegistrations.Remove(this);
            DisposeActiveDisposables();
        }

        private void DisposeActiveDisposables()
        {
            var activeDisposables = _activeDisposables;
            var viewDisposables   = _viewDisposables;
            _activeDisposables = null;
            _viewDisposables   = null;

            if (activeDisposables is null)
            {
                return;
            }

            if (viewDisposables?.Remove(activeDisposables) != true)
            {
                activeDisposables.Dispose();
            }
        }
    }
}
