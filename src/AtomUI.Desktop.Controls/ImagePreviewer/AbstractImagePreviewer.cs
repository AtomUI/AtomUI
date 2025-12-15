using System.ComponentModel;
using System.Diagnostics;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using AtomUI.Controls;
using AtomUI.Data;
using AtomUI.Desktop.Controls.DialogPositioning;
using AtomUI.Theme;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace AtomUI.Desktop.Controls;

public class AbstractImagePreviewer : TemplatedControl, 
                                      IControlSharedTokenResourcesHost,
                                      IMotionAwareControl
{
    #region 公共属性定义
    public static readonly StyledProperty<IList<string>?> SourcesProperty =
        AvaloniaProperty.Register<AbstractImagePreviewer, IList<string>?>(nameof(Sources));
    
    public static readonly StyledProperty<string?> FallbackImageSrcProperty =
        AvaloniaProperty.Register<AbstractImagePreviewer, string?>(nameof(FallbackImageSrc));
    
    public static readonly StyledProperty<bool> IsOpenProperty =
        AvaloniaProperty.Register<AbstractImagePreviewer, bool>(nameof(IsOpen));
    
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<AbstractImagePreviewer>();
    
    public static readonly StyledProperty<double> CoverWidthProperty =
        AvaloniaProperty.Register<AbstractImagePreviewer, double>(nameof(CoverWidth), double.NaN);
    
    public static readonly StyledProperty<double> CoverHeightProperty =
        AvaloniaProperty.Register<AbstractImagePreviewer, double>(nameof(CoverHeight), double.NaN);
    
    public static readonly StyledProperty<int> CurrentIndexProperty =
        AvaloniaProperty.Register<AbstractImagePreviewer, int>(nameof(CurrentIndex), 0);
    
    public IList<string>? Sources
    {
        get => GetValue(SourcesProperty);
        set => SetValue(SourcesProperty, value);
    }
    
    public string? FallbackImageSrc
    {
        get => GetValue(FallbackImageSrcProperty);
        set => SetValue(FallbackImageSrcProperty, value);
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
    
    public double CoverWidth
    {
        get => GetValue(CoverWidthProperty);
        set => SetValue(CoverWidthProperty, value);
    }
    
    public double CoverHeight
    {
        get => GetValue(CoverHeightProperty);
        set => SetValue(CoverHeightProperty, value);
    }
    
    public int CurrentIndex
    {
        get => GetValue(CurrentIndexProperty);
        set => SetValue(CurrentIndexProperty, value);
    }
    
    #endregion

    #region 预览窗口相关属性设置

    public static readonly StyledProperty<bool> IsImageMovableProperty =
        AvaloniaProperty.Register<AbstractImagePreviewer, bool>(nameof(IsImageMovable), true);
    
    public static readonly StyledProperty<double> ImageScaleStepProperty =
        AvaloniaProperty.Register<AbstractImagePreviewer, double>(nameof(ImageScaleStep), 0.5);
    
    public static readonly StyledProperty<double> ImageMinScaleProperty =
        AvaloniaProperty.Register<AbstractImagePreviewer, double>(nameof(ImageMinScale), 1.0);
    
    public static readonly StyledProperty<double> ImageMaxScaleProperty =
        AvaloniaProperty.Register<AbstractImagePreviewer, double>(nameof(ImageMaxScale), 50.0);
    
    public static readonly StyledProperty<bool> IsDialogModalProperty =
        AvaloniaProperty.Register<AbstractImagePreviewer, bool>(nameof (IsDialogModal), false);
    
    public static readonly StyledProperty<DialogHorizontalAnchor> DialogHorizontalStartupLocationProperty =
        AvaloniaProperty.Register<AbstractImagePreviewer, DialogHorizontalAnchor>(nameof(DialogHorizontalStartupLocation), DialogHorizontalAnchor.Custom);
    
    public static readonly StyledProperty<DialogVerticalAnchor> DialogVerticalStartupLocationProperty =
        AvaloniaProperty.Register<AbstractImagePreviewer, DialogVerticalAnchor>(nameof(DialogVerticalStartupLocation), DialogVerticalAnchor.Custom);
    
    public static readonly StyledProperty<Dimension?> DialogHorizontalOffsetProperty =
        AvaloniaProperty.Register<AbstractImagePreviewer, Dimension?>(nameof(DialogHorizontalOffset));
    
    public static readonly StyledProperty<Dimension?> DialogVerticalOffsetProperty =
        AvaloniaProperty.Register<AbstractImagePreviewer, Dimension?>(nameof(DialogVerticalOffset));
    
    public static readonly StyledProperty<CustomDialogPlacementCallback?> CustomPopupPlacementCallbackProperty =
        AvaloniaProperty.Register<AbstractImagePreviewer, CustomDialogPlacementCallback?>(nameof(CustomDialogPlacementCallback));
    
    public static readonly StyledProperty<bool> IsDialogTopmostProperty =
        AvaloniaProperty.Register<AbstractImagePreviewer, bool>(nameof(IsDialogTopmost));

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
    
    internal static readonly DirectProperty<AbstractImagePreviewer, double> PreviewDialogOffsetXProperty =
        AvaloniaProperty.RegisterDirect<AbstractImagePreviewer, double>(
            nameof(PreviewDialogOffsetX),
            o => o.PreviewDialogOffsetX,
            (o, v) => o.PreviewDialogOffsetX = v);
    
    internal static readonly DirectProperty<AbstractImagePreviewer, double> PreviewDialogOffsetYProperty =
        AvaloniaProperty.RegisterDirect<AbstractImagePreviewer, double>(
            nameof(PreviewDialogOffsetY),
            o => o.PreviewDialogOffsetY,
            (o, v) => o.PreviewDialogOffsetY = v);
    
    internal static readonly DirectProperty<AbstractImagePreviewer, IList<PreviewImageSource>?> EffectiveSourcesProperty =
        AvaloniaProperty.RegisterDirect<AbstractImagePreviewer, IList<PreviewImageSource>?>(
            nameof(EffectiveSources),
            o => o.EffectiveSources,
            (o, v) => o.EffectiveSources = v);
    
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
    
    private IList<PreviewImageSource>? _effectiveSources;

    internal IList<PreviewImageSource>? EffectiveSources
    {
        get => _effectiveSources;
        set => SetAndRaise(EffectiveSourcesProperty, ref _effectiveSources, value);
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
    
    static AbstractImagePreviewer()
    {
        FocusableProperty.OverrideDefaultValue<AbstractImagePreviewer>(true);
        IsOpenProperty.Changed.AddClassHandler<AbstractImagePreviewer>((x, e) => x.HandleIsOpenChanged(e));
    }
    
    public AbstractImagePreviewer()
    {
        this.RegisterResources();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == SourcesProperty)
        {
            HandleSourceChanged();
        }
    }

    internal PreviewImageSource LoadImageSource(string filePath)
    {
        using var stream  = AssetsLoader.OpenStream(filePath);
        if (IsSvgImage(filePath))
        {
            return PreviewImageSource.CreateSvg(stream);
        }
        return PreviewImageSource.CreateBitmap(stream);
    }

    private void HandleSourceChanged()
    {
        if (Sources != null && Sources.Count > 0)
        {
            var effectiveSources = new List<PreviewImageSource>();
            foreach (var source in Sources)
            {
                try
                {
                    effectiveSources.Add(LoadImageSource(source));
                }
                catch (Exception)
                {
                    // TODO 这个错误直接抛出还是忽略
                }
            }
            SetCurrentValue(EffectiveSourcesProperty, effectiveSources);
        }
    }

    private bool IsSvgImage(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return false;
        }
            
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        return extension == ".svg";
    }
    
    protected override void OnLoaded(RoutedEventArgs args)
    {
        base.OnLoaded(args);
        if (EffectiveSources == null || EffectiveSources?.Count == 0)
        {
            if (FallbackImageSrc != null)
            {
                try
                {
                    var sources = new List<PreviewImageSource>();
                    sources.Add(LoadImageSource(FallbackImageSrc));
                    SetCurrentValue(EffectiveSourcesProperty, sources);
                }
                catch (Exception)
                {
                    // TODO 这个错误直接抛出还是忽略
                }
            }
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
        disposables.Add(BindUtils.RelayBind(this, EffectiveSourcesProperty, dialogHost, ImagePreviewerDialog.SourcesProperty));
        disposables.Add(BindUtils.RelayBind(this, IsMotionEnabledProperty, dialogHost, ImagePreviewerDialog.IsMotionEnabledProperty));
        disposables.Add(BindUtils.RelayBind(this, IsDialogModalProperty, dialogHost, ImagePreviewerDialog.IsModalProperty));
        disposables.Add(BindUtils.RelayBind(this, CurrentIndexProperty, dialogHost, ImagePreviewerDialog.CurrentIndexProperty));
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
        private readonly AbstractImagePreviewer _owner;

        public IgnoreIsOpenScope(AbstractImagePreviewer owner)
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