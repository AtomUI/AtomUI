using AtomUI.Controls;
using AtomUI.Desktop.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;

namespace AtomUI.Toolkits.GalleryBase.Controls;

/// <summary>
/// Overlay that shows the content of one zoomed <see cref="ShowCaseItem" /> on a stage that covers the page
/// content, with a title, a description and a button that exits the zoom.
/// It is created and driven by <see cref="ShowCaseZoomOverlayHost" />. The overlay itself never closes: it
/// raises <see cref="CloseRequested" /> and leaves <see cref="IsOpen" /> to the host.
/// </summary>
public class ShowCaseZoomOverlay : TemplatedControl
{
    private const string CloseButtonPart = "PART_CloseButton";
    private const string StagePanelPart  = "PART_StagePanel";

    /// <summary>
    /// Identifies the <see cref="IsOpen" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsOpenProperty =
        AvaloniaProperty.Register<ShowCaseZoomOverlay, bool>(nameof(IsOpen));

    /// <summary>
    /// Identifies the <see cref="Title" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<ShowCaseZoomOverlay, string?>(nameof(Title));

    /// <summary>
    /// Identifies the <see cref="Description" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> DescriptionProperty =
        AvaloniaProperty.Register<ShowCaseZoomOverlay, string?>(nameof(Description));

    /// <summary>
    /// Identifies the <see cref="StageContent" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> StageContentProperty =
        AvaloniaProperty.Register<ShowCaseZoomOverlay, object?>(nameof(StageContent));

    /// <summary>
    /// Identifies the <see cref="IsMotionEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<ShowCaseZoomOverlay>();

    internal static readonly StyledProperty<ShowCaseItem?> SourceItemProperty =
        AvaloniaProperty.Register<ShowCaseZoomOverlay, ShowCaseItem?>(nameof(SourceItem));

    internal ShowCaseItem? SourceItem
    {
        get => GetValue(SourceItemProperty);
        set => SetValue(SourceItemProperty, value);
    }

    /// <summary>
    /// Raised when the user asks to leave the zoom: by activating the close button, or by pressing the Escape
    /// key while <see cref="IsOpen" /> is <see langword="true" />. The overlay does not change
    /// <see cref="IsOpen" /> itself, so the handler decides whether the overlay closes. An Escape key press
    /// that raises the event is marked as handled.
    /// </summary>
    public event EventHandler? CloseRequested;

    private IconButton? _closeButton;
    private ShowCaseZoomStagePanel? _stagePanel;

    static ShowCaseZoomOverlay()
    {
        FocusableProperty.OverrideDefaultValue<ShowCaseZoomOverlay>(true);
    }

    public ShowCaseZoomOverlay()
    {
        AddHandler(KeyDownEvent, HandleKeyDown,
            RoutingStrategies.Tunnel | RoutingStrategies.Bubble, handledEventsToo: true);
    }

    /// <summary>
    /// Whether the zoom is shown. <see langword="true" /> makes the root of the overlay visible and plays its
    /// open transition (fade and scale, 0.18 seconds); the keyboard focus is then moved to the overlay so that
    /// the Escape key works. <see langword="false" /> hides the root at once, without a transition.
    /// </summary>
    public bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    /// <summary>
    /// Title of the zoom, shown as the centered text of the separator line in the header, to the left of the
    /// close button. <see langword="null" /> hides the text; the line stays.
    /// </summary>
    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>
    /// Description of the zoom, shown as wrapped text at the bottom of the card. The text is hidden while the
    /// value is <see langword="null" /> or empty.
    /// </summary>
    public string? Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    /// <summary>
    /// Content shown on the stage of the overlay. Only a <see cref="Control" /> is placed on the stage; any
    /// other value leaves the stage empty. The control is added directly to the stage panel, which does not
    /// become its logical parent, so that the host can attach it logically to its original item.
    /// <see langword="null" /> removes the content from the stage.
    /// </summary>
    public object? StageContent
    {
        get => GetValue(StageContentProperty);
        set => SetValue(StageContentProperty, value);
    }

    /// <summary>
    /// Whether the open motion of the overlay is enabled. The value takes effect only when it changes: a change
    /// to <see langword="false" /> sets the <c>no-motion</c> class on the overlay, and the theme then removes
    /// the transitions of the root; a change to <see langword="true" /> clears the class. The registered default
    /// is <see langword="false" />, and it does not set the class, so an overlay on which the value has not
    /// changed yet keeps its open transition.
    /// </summary>
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    /// <summary>
    /// Called when the control template is applied. It detaches the handler of the previous close button and
    /// removes the stage content from the stage panel of the previous template, which still holds it as a
    /// visual child. It then resolves the new close button and stage panel, attaches the click handler and
    /// moves <see cref="StageContent" /> into the new stage panel.
    /// Overrides must call the base member; without it the close button does not work and the stage stays empty.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        if (_closeButton is not null)
        {
            _closeButton.Click -= HandleCloseButtonClick;
        }

        // The stage panel of the previous template still holds the visual parent of the content. Release the
        // content so that the new stage panel can take it over.
        _stagePanel?.Children.Clear();

        _closeButton = e.NameScope.Find<IconButton>(CloseButtonPart);
        _stagePanel  = e.NameScope.Find<ShowCaseZoomStagePanel>(StagePanelPart);
        if (_closeButton is not null)
        {
            _closeButton.Click += HandleCloseButtonClick;
        }

        SyncStageContentToPanel();
    }

    /// <summary>
    /// Called when a property of this control changes. After the base call it reacts to three properties:
    /// <see cref="IsOpen" /> turning <see langword="true" /> posts a request that moves the keyboard focus to
    /// the overlay, a change of <see cref="StageContent" /> moves the content into or out of the stage panel,
    /// and a change of <see cref="IsMotionEnabled" /> sets or clears the <c>no-motion</c> class.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsOpenProperty && IsOpen)
        {
            Dispatcher.UIThread.Post(() => Focus(), DispatcherPriority.Background);
        }
        else if (change.Property == StageContentProperty)
        {
            SyncStageContentToPanel();
        }
        else if (change.Property == IsMotionEnabledProperty)
        {
            Classes.Set("no-motion", !IsMotionEnabled);
        }
    }

    /// <summary>
    /// The stage content is added directly to a plain panel, which does not claim the logical child, instead of
    /// through a ContentPresenter. The host then calls <c>ISetLogicalParent.SetParent</c> to attach the content
    /// logically back to its original <see cref="ShowCaseItem" />, so that page-level styles, resources and
    /// the DataContext keep resolving. A ContentPresenter would claim the logical parent and conflict with that.
    /// </summary>
    private void SyncStageContentToPanel()
    {
        if (_stagePanel is null)
        {
            return;
        }

        _stagePanel.Children.Clear();
        if (StageContent is Control control)
        {
            _stagePanel.Children.Add(control);
        }
    }

    private void HandleCloseButtonClick(object? sender, RoutedEventArgs e)
    {
        RaiseCloseRequested();
    }

    private void HandleKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Handled)
        {
            return;
        }

        if (IsOpen && e.Key == Key.Escape)
        {
            e.Handled = true;
            RaiseCloseRequested();
        }
    }

    private void RaiseCloseRequested()
    {
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }
}
