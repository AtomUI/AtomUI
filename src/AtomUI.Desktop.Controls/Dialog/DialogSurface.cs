using System.Collections.Specialized;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Rendering.Composition;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

internal sealed class DialogSurfaceCloseRequestedEventArgs : EventArgs
{
    internal object? Result { get; }
    internal DialogCloseReason Reason { get; }
    internal DialogButton SourceButton { get; }

    internal DialogSurfaceCloseRequestedEventArgs(
        object? result,
        DialogCloseReason reason,
        DialogButton sourceButton)
    {
        Result = result;
        Reason = reason;
        SourceButton = sourceButton;
    }
}

internal sealed class DialogSurface : ContentControl, IDisposable, IControlThemeGlobalOwnerSource, IThemeInheritanceSource
{
    #region 内部属性定义

    internal static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<DialogSurface, string?>(nameof(Title));

    internal static readonly StyledProperty<PathIcon?> TitleIconProperty =
        AvaloniaProperty.Register<DialogSurface, PathIcon?>(nameof(TitleIcon));

    internal static readonly StyledProperty<bool> IsHeaderVisibleProperty =
        AvaloniaProperty.Register<DialogSurface, bool>(nameof(IsHeaderVisible), true);

    internal static readonly StyledProperty<bool> IsClosableProperty =
        Dialog.IsClosableProperty.AddOwner<DialogSurface>();

    internal static readonly StyledProperty<bool> IsMaximizableProperty =
        Dialog.IsMaximizableProperty.AddOwner<DialogSurface>();

    internal static readonly StyledProperty<bool> IsResizableProperty =
        Dialog.IsResizableProperty.AddOwner<DialogSurface>();

    internal static readonly StyledProperty<bool> IsMotionEnabledProperty =
        Dialog.IsMotionEnabledProperty.AddOwner<DialogSurface>();

    internal static readonly StyledProperty<TimeSpan> MotionDurationProperty =
        AvaloniaProperty.Register<DialogSurface, TimeSpan>(nameof(MotionDuration));

    internal static readonly StyledProperty<bool> IsLoadingProperty =
        Dialog.IsLoadingProperty.AddOwner<DialogSurface>();

    internal static readonly StyledProperty<bool> IsConfirmLoadingProperty =
        Dialog.IsConfirmLoadingProperty.AddOwner<DialogSurface>();

    internal static readonly StyledProperty<bool> IsFooterVisibleProperty =
        Dialog.IsFooterVisibleProperty.AddOwner<DialogSurface>();

    internal static readonly StyledProperty<DialogStandardButtons> StandardButtonsProperty =
        Dialog.StandardButtonsProperty.AddOwner<DialogSurface>();

    internal static readonly StyledProperty<DialogStandardButton> DefaultStandardButtonProperty =
        Dialog.DefaultStandardButtonProperty.AddOwner<DialogSurface>();

    internal static readonly StyledProperty<DialogStandardButton> EscapeStandardButtonProperty =
        Dialog.EscapeStandardButtonProperty.AddOwner<DialogSurface>();

    internal static readonly DirectProperty<DialogSurface, bool> IsEffectiveFooterVisibleProperty =
        AvaloniaProperty.RegisterDirect<DialogSurface, bool>(
            nameof(IsEffectiveFooterVisible),
            surface => surface.IsEffectiveFooterVisible);

    internal static readonly DirectProperty<DialogSurface, bool> IsDialogMaximizedProperty =
        AvaloniaProperty.RegisterDirect<DialogSurface, bool>(
            nameof(IsDialogMaximized),
            surface => surface.IsDialogMaximized);

    internal static readonly StyledProperty<double> ContentViewportMinWidthProperty =
        AvaloniaProperty.Register<DialogSurface, double>(nameof(ContentViewportMinWidth));

    internal static readonly StyledProperty<double> ContentViewportMinHeightProperty =
        AvaloniaProperty.Register<DialogSurface, double>(nameof(ContentViewportMinHeight));

    internal string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    internal PathIcon? TitleIcon
    {
        get => GetValue(TitleIconProperty);
        set => SetValue(TitleIconProperty, value);
    }

    internal bool IsHeaderVisible
    {
        get => GetValue(IsHeaderVisibleProperty);
        set => SetValue(IsHeaderVisibleProperty, value);
    }

    internal bool IsClosable
    {
        get => GetValue(IsClosableProperty);
        set => SetValue(IsClosableProperty, value);
    }

    internal bool IsMaximizable
    {
        get => GetValue(IsMaximizableProperty);
        set => SetValue(IsMaximizableProperty, value);
    }

    internal bool IsResizable
    {
        get => GetValue(IsResizableProperty);
        set => SetValue(IsResizableProperty, value);
    }

    internal bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    internal TimeSpan MotionDuration
    {
        get => GetValue(MotionDurationProperty);
        set => SetValue(MotionDurationProperty, value);
    }

    internal bool IsLoading
    {
        get => GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    internal bool IsConfirmLoading
    {
        get => GetValue(IsConfirmLoadingProperty);
        set => SetValue(IsConfirmLoadingProperty, value);
    }

    internal bool IsFooterVisible
    {
        get => GetValue(IsFooterVisibleProperty);
        set => SetValue(IsFooterVisibleProperty, value);
    }

    internal DialogStandardButtons StandardButtons
    {
        get => GetValue(StandardButtonsProperty);
        set => SetValue(StandardButtonsProperty, value);
    }

    internal DialogStandardButton DefaultStandardButton
    {
        get => GetValue(DefaultStandardButtonProperty);
        set => SetValue(DefaultStandardButtonProperty, value);
    }

    internal DialogStandardButton EscapeStandardButton
    {
        get => GetValue(EscapeStandardButtonProperty);
        set => SetValue(EscapeStandardButtonProperty, value);
    }

    private bool _isEffectiveFooterVisible;

    internal bool IsEffectiveFooterVisible
    {
        get => _isEffectiveFooterVisible;
        private set => SetAndRaise(IsEffectiveFooterVisibleProperty, ref _isEffectiveFooterVisible, value);
    }

    private bool _isDialogMaximized;

    internal bool IsDialogMaximized
    {
        get => _isDialogMaximized;
        set => SetAndRaise(IsDialogMaximizedProperty, ref _isDialogMaximized, value);
    }

    internal double ContentViewportMinWidth
    {
        get => GetValue(ContentViewportMinWidthProperty);
        set => SetValue(ContentViewportMinWidthProperty, value);
    }

    internal double ContentViewportMinHeight
    {
        get => GetValue(ContentViewportMinHeightProperty);
        set => SetValue(ContentViewportMinHeightProperty, value);
    }

    #endregion

    private readonly Dialog _dialog;
    private readonly CompositeDisposable _bindings = new();
    private CompositeDisposable? _confirmLoadingBindings;
    private CompositeDisposable? _structuralButtonBindings;
    private IReadOnlyList<DialogButton> _synchronizedButtons = Array.Empty<DialogButton>();
    private Border? _contentFrame;
    private Border? _footerFrame;
    private bool _isStructuralMinimumDirty;
    private Size? _lastStructuralMinimum;

    internal DialogButtonBox? ButtonBox { get; private set; }
    internal OverlayDialogHeader? Header { get; private set; }
    internal OverlayDialogResizer? Resizer { get; private set; }
    internal Control? SurfaceContentLayer { get; private set; }

    #region 内部事件定义

    internal event EventHandler<DialogSurfaceCloseRequestedEventArgs>? CloseRequested;
    internal event EventHandler? HostCloseRequested;
    internal event EventHandler? MaximizeRequested;
    internal event EventHandler? RestoreRequested;
    internal event EventHandler<OverlayDialogResizeEventArgs>? ResizeStarted;
    internal event EventHandler<OverlayDialogResizeEventArgs>? ResizeRequested;
    internal event EventHandler<OverlayDialogResizeEventArgs>? ResizeCompleted;
    internal event EventHandler<PointerPressedEventArgs>? HeaderPointerPressed;
    internal event EventHandler<PointerEventArgs>? HeaderPointerMoved;
    internal event EventHandler<PointerReleasedEventArgs>? HeaderPointerReleased;
    internal event EventHandler? StructuralMinimumChanged;

    #endregion

    static DialogSurface()
    {
        AffectsMeasure<DialogSurface>(ContentViewportMinWidthProperty, ContentViewportMinHeightProperty);
    }

    internal DialogSurface(Dialog dialog)
    {
        _dialog = dialog;
        Focusable = true;
        _bindings.Add(Bind(
            ContentProperty,
            dialog.GetObservable(Dialog.ContentProperty).Select(_ => dialog.GetSurfaceContent())));
        _bindings.Add(Bind(
            ContentTemplateProperty,
            dialog.GetObservable(Dialog.ContentTemplateProperty).Select(_ => dialog.GetSurfaceContentTemplate())));
        _bindings.Add(Bind(TitleProperty, dialog.GetObservable(Dialog.TitleProperty)));
        _bindings.Add(Bind(TitleIconProperty, dialog.GetObservable(Dialog.TitleIconProperty)));
        _bindings.Add(Bind(IsClosableProperty, dialog.GetObservable(Dialog.IsClosableProperty)));
        _bindings.Add(Bind(IsMaximizableProperty, dialog.GetObservable(Dialog.IsMaximizableProperty)));
        _bindings.Add(Bind(IsResizableProperty, dialog.GetObservable(Dialog.IsResizableProperty)));
        _bindings.Add(Bind(IsMotionEnabledProperty, dialog.GetObservable(Dialog.IsMotionEnabledProperty)));
        _bindings.Add(Bind(IsLoadingProperty, dialog.GetObservable(Dialog.IsLoadingProperty)));
        _bindings.Add(Bind(IsConfirmLoadingProperty, dialog.GetObservable(Dialog.IsConfirmLoadingProperty)));
        _bindings.Add(Bind(IsFooterVisibleProperty, dialog.GetObservable(Dialog.IsFooterVisibleProperty)));
        _bindings.Add(Bind(StandardButtonsProperty, dialog.GetObservable(Dialog.StandardButtonsProperty)));
        _bindings.Add(Bind(DefaultStandardButtonProperty, dialog.GetObservable(Dialog.DefaultStandardButtonProperty)));
        _bindings.Add(Bind(EscapeStandardButtonProperty, dialog.GetObservable(Dialog.EscapeStandardButtonProperty)));
        _bindings.Add(this.GetObservable(IsLoadingProperty).Subscribe(_ => UpdateEffectiveFooterVisibility()));
        _bindings.Add(this.GetObservable(IsFooterVisibleProperty).Subscribe(_ => UpdateEffectiveFooterVisibility()));
        _bindings.Add(this.GetObservable(StandardButtonsProperty).Subscribe(_ => UpdateEffectiveFooterVisibility()));
        dialog.CustomButtons.CollectionChanged += HandleCustomButtonsChanged;
        _bindings.Add(Disposable.Create(() => dialog.CustomButtons.CollectionChanged -= HandleCustomButtonsChanged));
        UpdateEffectiveFooterVisibility();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == TitleProperty ||
            change.Property == TitleIconProperty ||
            change.Property == IsHeaderVisibleProperty ||
            change.Property == IsClosableProperty ||
            change.Property == IsMaximizableProperty ||
            change.Property == IsEffectiveFooterVisibleProperty ||
            change.Property == ContentViewportMinWidthProperty ||
            change.Property == ContentViewportMinHeightProperty)
        {
            InvalidateStructuralMinimum();
        }
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var measuredSize = base.MeasureOverride(availableSize);
        if (_isStructuralMinimumDirty)
        {
            _isStructuralMinimumDirty = false;
            var structuralMinimum = MeasureStructuralMinimum();
            if (_lastStructuralMinimum != structuralMinimum)
            {
                _lastStructuralMinimum = structuralMinimum;
                StructuralMinimumChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        return measuredSize;
    }

    protected override void OnMeasureInvalidated()
    {
        _isStructuralMinimumDirty = true;
        base.OnMeasureInvalidated();
    }


    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        ReleaseButtonBox();
        ReleaseHeader();
        ReleaseResizer();
        ReleaseStructuralFrames();

        ButtonBox = e.NameScope.Find<DialogButtonBox>("PART_ButtonBox");
        Header = e.NameScope.Find<OverlayDialogHeader>("PART_Header");
        Resizer = e.NameScope.Find<OverlayDialogResizer>("PART_Resizer");
        SurfaceContentLayer = e.NameScope.Find<Control>("PART_SurfaceContentLayer");
        _contentFrame = e.NameScope.Find<Border>("ContentFrame");
        _footerFrame = e.NameScope.Find<Border>("FooterFrame");
        if (ButtonBox is not null)
        {
            ButtonBox.Clicked += HandleButtonClicked;
            ButtonBox.EffectiveButtonsChanged += HandleEffectiveButtonsChanged;
            SynchronizeCustomButtons();
        }

        if (Header is not null)
        {
            Header.CloseRequest += HandleHeaderCloseRequest;
            Header.MaximizeRequest += HandleHeaderMaximizeRequest;
            Header.NormalizeRequest += HandleHeaderNormalizeRequest;
            Header.PointerPressed += HandleHeaderPointerPressed;
            Header.PointerMoved += HandleHeaderPointerMoved;
            Header.PointerReleased += HandleHeaderPointerReleased;
        }
        if (Resizer is not null)
        {
            Resizer.AboutToResize += HandleResizeStarted;
            Resizer.ResizeRequest += HandleResizeRequest;
            Resizer.ResizeCompleted += HandleResizeCompleted;
        }

        InvalidateStructuralMinimum();
    }

    #region 内部协作 API

    internal Size MeasureStructuralMinimum()
    {
        var headerSize = default(Size);
        if (IsHeaderVisible && Header is { IsVisible: true } header)
        {
            header.Measure(Size.Infinity);
            headerSize = header.DesiredSize;
        }

        var footerSize = default(Size);
        if (IsEffectiveFooterVisible)
        {
            var footer = (Control?)_footerFrame ?? ButtonBox;
            if (footer is { IsVisible: true })
            {
                footer.Measure(Size.Infinity);
                footerSize = footer.DesiredSize;
            }
        }

        var contentPadding = _contentFrame?.Padding ?? default;
        var contentBorder = _contentFrame?.BorderThickness ?? default;
        var contentWidth = NormalizeViewportMinimum(ContentViewportMinWidth) +
                           contentPadding.Left + contentPadding.Right +
                           contentBorder.Left + contentBorder.Right;
        var contentHeight = NormalizeViewportMinimum(ContentViewportMinHeight) +
                            contentPadding.Top + contentPadding.Bottom +
                            contentBorder.Top + contentBorder.Bottom;

        return new Size(
            Math.Max(contentWidth, Math.Max(headerSize.Width, footerSize.Width)),
            contentHeight + headerSize.Height + footerSize.Height);
    }

    #endregion

    #region 内部协作 API

    internal bool TryInvokeStandardButton(Key key)
    {
        var standardButton = key switch
        {
            Key.Enter => DefaultStandardButton,
            Key.Escape => ResolveEscapeStandardButton(),
            _ => DialogStandardButton.NoButton
        };
        if (standardButton == DialogStandardButton.NoButton)
        {
            return false;
        }

        var button = _synchronizedButtons.FirstOrDefault(candidate =>
            candidate.StandardButtonType == standardButton && candidate.IsEffectivelyEnabled);
        if (button is null)
        {
            return false;
        }

        HandleButtonClicked(this, new DialogButtonClickedEventArgs(button));
        return true;
    }

    #endregion

    #region 内部协作 API

    internal void DisconnectCompositionChildren()
    {
        foreach (var visual in this.GetVisualDescendants().Prepend(this))
        {
            if (ElementComposition.GetElementVisual(visual) is CompositionContainerVisual compositionVisual)
            {
                compositionVisual.Children.RemoveAll();
            }
        }
    }

    #endregion

    public void Dispose()
    {
        ReleaseButtonBox();
        ReleaseHeader();
        ReleaseResizer();
        ReleaseStructuralFrames();
        SurfaceContentLayer = null;
        _bindings.Dispose();
    }

    #region 实现 IControlThemeGlobalOwnerSource 接口

    Control? IControlThemeGlobalOwnerSource.ThemeGlobalOwner => _dialog;

    #endregion

    #region 实现 IThemeInheritanceSource 接口

    // Preserve the actual physical host typography when no owner Global is set.
    AvaloniaObject? IThemeInheritanceSource.ThemeInheritanceParent => InheritanceParent;

    #endregion

    private void HandleCustomButtonsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        SynchronizeCustomButtons();
        UpdateEffectiveFooterVisibility();
    }

    private void SynchronizeCustomButtons()
    {
        if (ButtonBox is null)
        {
            return;
        }

        ButtonBox.CustomButtons.Clear();
        ButtonBox.CustomButtons.AddRange(_dialog.CustomButtons);
    }

    private void HandleButtonClicked(object? sender, DialogButtonClickedEventArgs e)
    {
        if (!_dialog.NotifySurfaceButtonClicked(e.SourceButton))
        {
            return;
        }

        if (e.SourceButton.Role is DialogButtonRole.AcceptRole or
            DialogButtonRole.YesRole or
            DialogButtonRole.ApplyRole or
            DialogButtonRole.ResetRole)
        {
            CloseRequested?.Invoke(this,
                new DialogSurfaceCloseRequestedEventArgs(
                    DialogCode.Accepted,
                    DialogCloseReason.Accepted,
                    e.SourceButton));
        }
        else if (e.SourceButton.Role is DialogButtonRole.RejectRole or DialogButtonRole.NoRole)
        {
            CloseRequested?.Invoke(this,
                new DialogSurfaceCloseRequestedEventArgs(
                    DialogCode.Rejected,
                    DialogCloseReason.Rejected,
                    e.SourceButton));
        }
    }

    private void HandleHeaderCloseRequest(object? sender, EventArgs e)
    {
        HostCloseRequested?.Invoke(this, EventArgs.Empty);
    }

    private void HandleHeaderMaximizeRequest(object? sender, EventArgs e)
    {
        MaximizeRequested?.Invoke(this, EventArgs.Empty);
    }

    private void HandleHeaderNormalizeRequest(object? sender, EventArgs e)
    {
        RestoreRequested?.Invoke(this, EventArgs.Empty);
    }

    private void HandleResizeRequest(object? sender, OverlayDialogResizeEventArgs e)
    {
        ResizeRequested?.Invoke(this, e);
    }

    private void HandleResizeStarted(object? sender, OverlayDialogResizeEventArgs e)
    {
        ResizeStarted?.Invoke(this, e);
    }

    private void HandleResizeCompleted(object? sender, OverlayDialogResizeEventArgs e)
    {
        ResizeCompleted?.Invoke(this, e);
    }

    private void HandleHeaderPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        HeaderPointerPressed?.Invoke(this, e);
    }

    private void HandleHeaderPointerMoved(object? sender, PointerEventArgs e)
    {
        HeaderPointerMoved?.Invoke(this, e);
    }

    private void HandleHeaderPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        HeaderPointerReleased?.Invoke(this, e);
    }

    private void HandleEffectiveButtonsChanged(object? sender, EventArgs e)
    {
        if (ButtonBox is null)
        {
            return;
        }

        _synchronizedButtons = ButtonBox.EffectiveButtons.ToArray();
        _confirmLoadingBindings?.Dispose();
        _confirmLoadingBindings = new CompositeDisposable(_synchronizedButtons.Count);
        _structuralButtonBindings?.Dispose();
        _structuralButtonBindings = new CompositeDisposable(_synchronizedButtons.Count * 2);
        foreach (var button in _synchronizedButtons)
        {
            if (button.Role is DialogButtonRole.AcceptRole or DialogButtonRole.YesRole or DialogButtonRole.ApplyRole)
            {
                _confirmLoadingBindings.Add(
                    button.Bind(Button.IsLoadingProperty, this.GetObservable(IsConfirmLoadingProperty)));
            }

            _structuralButtonBindings.Add(
                button.GetObservable(Button.ContentProperty).Subscribe(_ => InvalidateStructuralMinimum()));
            _structuralButtonBindings.Add(
                button.GetObservable(Visual.IsVisibleProperty).Subscribe(_ => InvalidateStructuralMinimum()));
        }

        _dialog.ConfigureSurfaceButtons(_synchronizedButtons);
        InvalidateStructuralMinimum();
    }

    private static double NormalizeViewportMinimum(double value)
    {
        return double.IsFinite(value) ? Math.Max(0, value) : 0;
    }

    private void InvalidateStructuralMinimum()
    {
        _isStructuralMinimumDirty = true;
        InvalidateMeasure();
    }

    private DialogStandardButton ResolveEscapeStandardButton()
    {
        if (_dialog.IsSet(Dialog.EscapeStandardButtonProperty))
        {
            return EscapeStandardButton;
        }

        return StandardButtons.HasFlag(DialogStandardButton.Cancel)
            ? DialogStandardButton.Cancel
            : DialogStandardButton.NoButton;
    }

    private void UpdateEffectiveFooterVisibility()
    {
        IsEffectiveFooterVisible = IsFooterVisible &&
                                   !IsLoading &&
                                   (StandardButtons.Count > 0 || _dialog.CustomButtons.Count > 0);
    }

    private void ReleaseButtonBox()
    {
        _dialog.ReleaseSurfaceButtons(_synchronizedButtons);
        _confirmLoadingBindings?.Dispose();
        _confirmLoadingBindings = null;
        _structuralButtonBindings?.Dispose();
        _structuralButtonBindings = null;
        _synchronizedButtons = Array.Empty<DialogButton>();
        if (ButtonBox is null)
        {
            return;
        }

        ButtonBox.Clicked -= HandleButtonClicked;
        ButtonBox.EffectiveButtonsChanged -= HandleEffectiveButtonsChanged;
        ButtonBox.CustomButtons.Clear();
        ButtonBox.ReleaseButtons();
        ButtonBox = null;
    }

    private void ReleaseHeader()
    {
        if (Header is null)
        {
            return;
        }

        Header.CloseRequest -= HandleHeaderCloseRequest;
        Header.MaximizeRequest -= HandleHeaderMaximizeRequest;
        Header.NormalizeRequest -= HandleHeaderNormalizeRequest;
        Header.PointerPressed -= HandleHeaderPointerPressed;
        Header.PointerMoved -= HandleHeaderPointerMoved;
        Header.PointerReleased -= HandleHeaderPointerReleased;
        Header = null;
    }

    private void ReleaseResizer()
    {
        if (Resizer is null)
        {
            return;
        }

        Resizer.AboutToResize -= HandleResizeStarted;
        Resizer.ResizeRequest -= HandleResizeRequest;
        Resizer.ResizeCompleted -= HandleResizeCompleted;
        Resizer = null;
    }

    private void ReleaseStructuralFrames()
    {
        _footerFrame = null;
        _contentFrame = null;
    }
}
