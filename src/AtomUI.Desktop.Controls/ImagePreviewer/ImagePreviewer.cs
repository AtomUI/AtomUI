using System.ComponentModel;
using System.Diagnostics;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using AtomUI.Controls;
using AtomUI.Data;
using AtomUI.Desktop.Controls.DialogPositioning;
using AtomUI.Theme;
using AtomUI.Theme.Styling;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

public class ImagePreviewer : TemplatedControl, 
                              IControlSharedTokenResourcesHost,
                              IMotionAwareControl
{
    #region 公共属性定义
    public static readonly StyledProperty<IList<IImage>?> SourcesProperty =
        AvaloniaProperty.Register<ImagePreviewer, IList<IImage>?>(nameof(Sources));

    public static readonly StyledProperty<IImage?> CoverImageSrcProperty =
        AvaloniaProperty.Register<ImagePreviewer, IImage?>(nameof(CoverImageSrc));
    
    public static readonly StyledProperty<bool> IsOpenProperty =
        AvaloniaProperty.Register<ImagePreviewer, bool>(nameof(IsOpen));
    
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<ImagePreviewer>();
    
    public IList<IImage>? Sources
    {
        get => GetValue(SourcesProperty);
        set => SetValue(SourcesProperty, value);
    }

    public IImage? CoverImageSrc
    {
        get => GetValue(CoverImageSrcProperty);
        set => SetValue(CoverImageSrcProperty, value);
    }
    
    public bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }
    
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    #endregion

    #region 预览窗口相关属性设置

    public static readonly StyledProperty<bool> IsImageMovableProperty =
        AvaloniaProperty.Register<ImagePreviewer, bool>(nameof(IsImageMovable));
    
    public static readonly StyledProperty<double> ImageScaleStepProperty =
        AvaloniaProperty.Register<ImagePreviewer, double>(nameof(ImageScaleStep), 0.5);
    
    public static readonly StyledProperty<double> ImageMinScaleProperty =
        AvaloniaProperty.Register<ImagePreviewer, double>(nameof(ImageMinScale), 1.0);
    
    public static readonly StyledProperty<double> ImageMaxScaleProperty =
        AvaloniaProperty.Register<ImagePreviewer, double>(nameof(ImageMaxScale), 50.0);
    
    public static readonly StyledProperty<bool> IsDialogModalProperty =
        AvaloniaProperty.Register<ImagePreviewer, bool>(nameof (IsDialogModal), true);
    
    public static readonly StyledProperty<DialogHorizontalAnchor> DialogHorizontalStartupLocationProperty =
        AvaloniaProperty.Register<ImagePreviewer, DialogHorizontalAnchor>(nameof(DialogHorizontalStartupLocation), DialogHorizontalAnchor.Custom);
    
    public static readonly StyledProperty<DialogVerticalAnchor> DialogVerticalStartupLocationProperty =
        AvaloniaProperty.Register<ImagePreviewer, DialogVerticalAnchor>(nameof(DialogVerticalStartupLocation), DialogVerticalAnchor.Custom);
    
    public static readonly StyledProperty<Dimension?> DialogHorizontalOffsetProperty =
        AvaloniaProperty.Register<ImagePreviewer, Dimension?>(nameof(DialogHorizontalOffset));
    
    public static readonly StyledProperty<Dimension?> DialogVerticalOffsetProperty =
        AvaloniaProperty.Register<ImagePreviewer, Dimension?>(nameof(DialogVerticalOffset));
    
    public static readonly StyledProperty<CustomDialogPlacementCallback?> CustomPopupPlacementCallbackProperty =
        AvaloniaProperty.Register<ImagePreviewer, CustomDialogPlacementCallback?>(nameof(CustomDialogPlacementCallback));
    
    public static readonly StyledProperty<bool> IsDialogTopmostProperty =
        AvaloniaProperty.Register<ImagePreviewer, bool>(nameof(IsDialogTopmost));

    public bool IsImageMovable
    {
        get => GetValue(IsImageMovableProperty);
        set => SetValue(IsImageMovableProperty, value);
    }
    
    public double ImageScaleStep
    {
        get => GetValue(ImageScaleStepProperty);
        set => SetValue(ImageScaleStepProperty, value);
    }
    
    public double ImageMinScale
    {
        get => GetValue(ImageMinScaleProperty);
        set => SetValue(ImageMinScaleProperty, value);
    }
    
    public double ImageMaxScale
    {
        get => GetValue(ImageMaxScaleProperty);
        set => SetValue(ImageMaxScaleProperty, value);
    }
    
    public bool IsDialogModal
    {
        get => GetValue(IsDialogModalProperty);
        set => SetValue(IsDialogModalProperty, value);
    }
    
    public DialogHorizontalAnchor DialogHorizontalStartupLocation
    {
        get => GetValue(DialogHorizontalStartupLocationProperty);
        set => SetValue(DialogHorizontalStartupLocationProperty, value);
    }
    
    public DialogVerticalAnchor DialogVerticalStartupLocation
    {
        get => GetValue(DialogVerticalStartupLocationProperty);
        set => SetValue(DialogVerticalStartupLocationProperty, value);
    }
    
    public Dimension? DialogHorizontalOffset
    {
        get => GetValue(DialogHorizontalOffsetProperty);
        set => SetValue(DialogHorizontalOffsetProperty, value);
    }
    
    public Dimension? DialogVerticalOffset
    {
        get => GetValue(DialogVerticalOffsetProperty);
        set => SetValue(DialogVerticalOffsetProperty, value);
    }
    
    public CustomDialogPlacementCallback? CustomDialogPlacementCallback
    {
        get => GetValue(CustomPopupPlacementCallbackProperty);
        set => SetValue(CustomPopupPlacementCallbackProperty, value);
    }
    
    public bool IsDialogTopmost
    {
        get => GetValue(IsDialogTopmostProperty);
        set => SetValue(IsDialogTopmostProperty, value);
    }
    
    #endregion

    #region 公共事件定义

    public event EventHandler? DialogClosed;
    public event EventHandler? DialogOpened;
    public event EventHandler<CancelEventArgs>? DialogClosing;

    #endregion
    
    #region 内部属性定义
    
    internal static readonly StyledProperty<double> MaskOpacityProperty =
        AvaloniaProperty.Register<ImagePreviewer, double>(nameof(MaskOpacity), 0.0);
    
    internal static readonly DirectProperty<ImagePreviewer, double> PreviewDialogOffsetXProperty =
        AvaloniaProperty.RegisterDirect<ImagePreviewer, double>(
            nameof(PreviewDialogOffsetX),
            o => o.PreviewDialogOffsetX,
            (o, v) => o.PreviewDialogOffsetX = v);
    
    internal static readonly DirectProperty<ImagePreviewer, double> PreviewDialogOffsetYProperty =
        AvaloniaProperty.RegisterDirect<ImagePreviewer, double>(
            nameof(PreviewDialogOffsetY),
            o => o.PreviewDialogOffsetY,
            (o, v) => o.PreviewDialogOffsetY = v);
    
    internal double MaskOpacity
    {
        get => GetValue(MaskOpacityProperty);
        set => SetValue(MaskOpacityProperty, value);
    }
    
    private double _previewDialogOffsetX;

    internal double PreviewDialogOffsetX
    {
        get => _previewDialogOffsetX;
        set => SetAndRaise(PreviewDialogOffsetXProperty, ref _previewDialogOffsetX, value);
    }
    
    private double _previewDialogOffsetY;

    internal double PreviewDialogOffsetY
    {
        get => _previewDialogOffsetY;
        set => SetAndRaise(PreviewDialogOffsetYProperty, ref _previewDialogOffsetY, value);
    }

    Control IMotionAwareControl.PropertyBindTarget => this;
    Control IControlSharedTokenResourcesHost.HostControl => this;
    string IControlSharedTokenResourcesHost.TokenId => ImagePreviewerToken.ID;
    
    #endregion
    
    private bool _ignoreIsOpenChanged;
    private DialogOpenState? _openState;
    private bool _startupLocationCalculated;
    private IDisposable? _modalSubscription;
    private bool _dialogOpening;
    private bool _dialogClosing;
    
    static ImagePreviewer()
    {
        FocusableProperty.OverrideDefaultValue<ImagePreviewer>(true);
        IsOpenProperty.Changed.AddClassHandler<ImagePreviewer>((x, e) => x.HandleIsOpenChanged(e));
        AffectsRender<ImagePreviewer>(CoverImageSrcProperty);
    }
    
    public ImagePreviewer()
    {
        this.RegisterResources();
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        if (CoverImageSrc == null && Sources?.Count > 0)
        {
            SetCurrentValue(CoverImageSrcProperty, Sources.First());
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == SourcesProperty)
        {
            if (Sources?.Count > 0)
            {
                if (CoverImageSrc == null)
                {
                    SetCurrentValue(CoverImageSrcProperty, Sources.First());
                }
            }
        }
        if (IsLoaded)
        {
            if (change.Property == IsMotionEnabledProperty)
            {
                ConfigureTransitions(true);
            }
        }
    }
    
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        ConfigureTransitions(false);
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        Transitions = null;
    }

    private void ConfigureTransitions(bool force)
    {
        if (IsMotionEnabled)
        {
            if (force || Transitions == null)
            {
                Transitions = [
                    TransitionUtils.CreateTransition<DoubleTransition>(MaskOpacityProperty, SharedTokenKey.MotionDurationSlow)
                ];
            }
        }
        else
        {
            Transitions = null;
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        OpenDialog();
    }

    #region 预览窗口相关方法

    private void HandleIsOpenChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (!_ignoreIsOpenChanged)
        {
            if (e.GetNewValue<bool>())
            {
                OpenDialog();
            }
            else
            {
                CloseDialog();
            }
        }
    }
    
    public void OpenDialog()
    {
        if (_openState != null || _dialogOpening)
        {
            return;
        }
    
        _dialogOpening = true;
        var placementTarget = this;
        Debug.Assert(placementTarget != null);
        var topLevel = TopLevel.GetTopLevel(placementTarget);
        Debug.Assert(topLevel != null);
        CompositeDisposable relayBindingDisposables = new CompositeDisposable();

        var previewDialog = new ImagePreviewerDialog(topLevel, this);
        RelayDialogHostBindings(relayBindingDisposables, previewDialog);
        
        Debug.Assert(previewDialog != null);
        var handlerCleanup = new CompositeDisposable(8);
        previewDialog.Topmost = IsDialogTopmost;
        ((ISetLogicalParent)previewDialog).SetParent(this);

        previewDialog
            .Bind(
                ThemeVariantScope.ActualThemeVariantProperty,
                this.GetBindingObservable(ThemeVariantScope.ActualThemeVariantProperty))
            .DisposeWith(handlerCleanup);
        UpdateHostPosition(previewDialog, placementTarget);
        
        SubscribeToEventHandler<ImagePreviewerDialog, EventHandler<TemplateAppliedEventArgs>>(previewDialog, RootTemplateApplied,
            (x, handler) => x.TemplateApplied += handler,
            (x, handler) => x.TemplateApplied -= handler).DisposeWith(handlerCleanup);
        
        SubscribeToEventHandler<Control, EventHandler<VisualTreeAttachmentEventArgs>>(placementTarget, TargetDetached,
            (x, handler) => x.DetachedFromVisualTree += handler,
            (x, handler) => x.DetachedFromVisualTree -= handler).DisposeWith(handlerCleanup);
        if (topLevel is Window window)
        {
            SubscribeToEventHandler<Window, EventHandler>(window, ParentClosed,
                (x, handler) => x.Closed += handler,
                (x, handler) => x.Closed -= handler).DisposeWith(handlerCleanup);
        } 
        
        var cleanupPopup = Disposable.Create((previewDialog, handlerCleanup), state =>
        {
            previewDialog.Close(() =>
            {
                state.handlerCleanup.Dispose();
                ((ISetLogicalParent)state.previewDialog).SetParent(null);
                relayBindingDisposables.Dispose();
                _startupLocationCalculated = false;
                _dialogClosing             = false;
                if (DataContext is IDialogAwareDataContext dialogAwareDataContext)
                {
                    dialogAwareDataContext.NotifyClosed();
                }
                DialogClosed?.Invoke(this, EventArgs.Empty);
            });
            previewDialog.Dispose();
        });
        
        _openState = new DialogOpenState(topLevel, previewDialog, cleanupPopup);
    
        previewDialog.Focus();
        if (IsDialogModal)
        {
            if (topLevel is Window windowTopLevel)
            {
                previewDialog.ShowDialog(windowTopLevel);
            }
        }
        else
        {
            previewDialog.Show();
        }
        
        if (IsDialogModal)
        {
            var tcs = new TaskCompletionSource<object?>();
            var disposables = new CompositeDisposable(
            [
                Observable.FromEventPattern(
                              x => DialogClosed += x,
                              x => DialogClosed -= x)
                          .Take(1)
                          .Subscribe(_ =>
                          {
                              _modalSubscription?.Dispose();
                          }),
                Disposable.Create(() =>
                {
                    _modalSubscription = null;
                    tcs.SetResult(null);
                })
            ]);
    
            _modalSubscription = disposables;
        }
        
        using (BeginIgnoringIsOpen())
        {
            SetCurrentValue(IsOpenProperty, true);
        }
        DialogOpened?.Invoke(this, EventArgs.Empty);
        _dialogOpening = false;
    }
    
    protected virtual void CloseDialog()
    {
        if (_dialogClosing)
        {
            return;
        }
        
        var closingArgs = new CancelEventArgs();
        DialogClosing?.Invoke(this, closingArgs);
        if (closingArgs.Cancel)
        {
            return;
        }
        
        if (_openState is null)
        {
            using (BeginIgnoringIsOpen())
            {
                SetCurrentValue(IsOpenProperty, false);
            }

            return;
        }
        
        _openState.Dispose();
        _openState = null;
        
        _modalSubscription?.Dispose();
        _modalSubscription = null;
        using (BeginIgnoringIsOpen())
        {
            SetCurrentValue(IsOpenProperty, false);
        }
    }
    
    private protected virtual void RelayDialogHostBindings(CompositeDisposable disposables, ImagePreviewerDialog dialogHost)
    {
        disposables.Add(BindUtils.RelayBind(this, IsImageMovableProperty, dialogHost, ImagePreviewerDialog.IsImageMovableProperty));
        disposables.Add(BindUtils.RelayBind(this, ImageScaleStepProperty, dialogHost, ImagePreviewerDialog.ScaleStepProperty));
        disposables.Add(BindUtils.RelayBind(this, ImageMinScaleProperty, dialogHost, ImagePreviewerDialog.MinScaleProperty));
        disposables.Add(BindUtils.RelayBind(this, ImageMaxScaleProperty, dialogHost, ImagePreviewerDialog.MaxScaleProperty));
        disposables.Add(BindUtils.RelayBind(this, SourcesProperty, dialogHost, ImagePreviewerDialog.SourcesProperty));
        disposables.Add(BindUtils.RelayBind(this, IsMotionEnabledProperty, dialogHost, ImagePreviewerDialog.IsMotionEnabledProperty));
        disposables.Add(BindUtils.RelayBind(this, IsDialogModalProperty, dialogHost, ImagePreviewerDialog.IsModalProperty));
    }
    
    private void UpdateHostPosition(IDialogHost dialogHost, Control placementTarget)
    {
        dialogHost.ConfigurePosition(new DialogPositionRequest(
            placementTarget,
            PreviewDialogOffsetX,
            PreviewDialogOffsetY,
            new Rect(default, placementTarget.Bounds.Size),
            CustomDialogPlacementCallback));
    }
    
    private void RootTemplateApplied(object? sender, TemplateAppliedEventArgs e)
    {
        if (_openState is null)
        {
            return;
        }

        var popupHost = _openState.DialogHost;
        popupHost.TemplateApplied -= RootTemplateApplied;
        _openState.SetPresenterSubscription(null);

        // If the Popup appears in a control template, then the child controls
        // that appear in the popup host need to have their TemplatedParent
        // properties set.
        if (TemplatedParent != null && popupHost.Presenter is Control presenter)
        {
            presenter.ApplyTemplate();

            var presenterSubscription = presenter.GetObservable(ContentPresenter.ChildProperty)
                                                 .Subscribe(SetTemplatedParentAndApplyChildTemplates);

            _openState.SetPresenterSubscription(presenterSubscription);
        }
    }
    
    private void SetTemplatedParentAndApplyChildTemplates(Control? control)
    {
        if (control != null)
        {
            TemplatedControlUtils.ApplyTemplatedParent(control, TemplatedParent);
        }
    }
    
    private void TargetDetached(object? sender, VisualTreeAttachmentEventArgs e)
    {
        CloseDialog();
    }
    
    private static IDisposable SubscribeToEventHandler<T, TEventHandler>(T target, TEventHandler handler, Action<T, TEventHandler> subscribe, Action<T, TEventHandler> unsubscribe)
    {
        subscribe(target, handler);

        return Disposable.Create((unsubscribe, target, handler), state => state.unsubscribe(state.target, state.handler));
    }
    
    private void ParentClosed(object? sender, EventArgs e)
    {
        CloseDialog();
    }
    
    internal void NotifyDialogHostMeasured(Size size, Rect bounds)
    {
        if (!_startupLocationCalculated)
        {
            var boundSize = bounds.Size;
            if (DialogHorizontalStartupLocation != DialogHorizontalAnchor.Custom)
            {
                if (DialogHorizontalStartupLocation == DialogHorizontalAnchor.Left)
                {
                    SetCurrentValue(PreviewDialogOffsetXProperty, 0);
                }
                else if (DialogHorizontalStartupLocation == DialogHorizontalAnchor.Right)
                {
                    SetCurrentValue(PreviewDialogOffsetXProperty, boundSize.Width - size.Width);
                }
                else if (DialogHorizontalStartupLocation == DialogHorizontalAnchor.Center)
                {
                    SetCurrentValue(PreviewDialogOffsetXProperty, (boundSize.Width - size.Width) / 2);
                }
            }
            else
            {
                if (DialogHorizontalOffset != null)
                {
                    SetCurrentValue(PreviewDialogOffsetXProperty, DialogHorizontalOffset.Value.Resolve(boundSize.Width));
                }
            }
        
            if (DialogVerticalStartupLocation != DialogVerticalAnchor.Custom)
            {
                if (DialogVerticalStartupLocation == DialogVerticalAnchor.Top)
                {
                    SetCurrentValue(PreviewDialogOffsetYProperty, 0);
                }
                else if (DialogVerticalStartupLocation == DialogVerticalAnchor.Bottom)
                {
                    SetCurrentValue(PreviewDialogOffsetYProperty, boundSize.Height - size.Height);
                }
                else if (DialogVerticalStartupLocation == DialogVerticalAnchor.Center)
                {
                    SetCurrentValue(PreviewDialogOffsetYProperty, (boundSize.Height - size.Height) / 2);
                }
            }
            else
            {
                if (DialogVerticalOffset != null)
                {
                    SetCurrentValue(PreviewDialogOffsetYProperty, DialogVerticalOffset.Value.Resolve(boundSize.Height));
                }
            }

            _startupLocationCalculated = true;
        }
    }
     
    internal void NotifyDialogHostCloseRequest()
    {
        CloseDialog();
    }
    
    private IgnoreIsOpenScope BeginIgnoringIsOpen()
    {
        return new IgnoreIsOpenScope(this);
    }
    
    private readonly struct IgnoreIsOpenScope : IDisposable
    {
        private readonly ImagePreviewer _owner;

        public IgnoreIsOpenScope(ImagePreviewer owner)
        {
            _owner                      = owner;
            _owner._ignoreIsOpenChanged = true;
        }

        public void Dispose()
        {
            _owner._ignoreIsOpenChanged = false;
        }
    }
    
    private class DialogOpenState : IDisposable
    {
        private readonly IDisposable _cleanup;
        private IDisposable? _presenterCleanup;
        
        public DialogOpenState(TopLevel topLevel, ImagePreviewerDialog previewDialog, IDisposable cleanup)
        {
            TopLevel   = topLevel;
            DialogHost = previewDialog;
            _cleanup   = cleanup;
        }
        
        public ImagePreviewerDialog DialogHost { get; }
        public TopLevel TopLevel { get; }
        
        public void SetPresenterSubscription(IDisposable? presenterCleanup)
        {
            _presenterCleanup?.Dispose();
            _presenterCleanup = presenterCleanup;
        }
        
        public void Dispose()
        {
            _presenterCleanup?.Dispose();
            _cleanup.Dispose();
        }
    }

    #endregion
}