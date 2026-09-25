using AtomUI.Controls;
using AtomUI.Desktop.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;

namespace AtomUI.Toolkits.GalleryBase.Controls;

public class ShowCaseZoomOverlay : TemplatedControl
{
    private const string CloseButtonPart = "PART_CloseButton";
    private const string StagePanelPart  = "PART_StagePanel";

    public static readonly StyledProperty<bool> IsOpenProperty =
        AvaloniaProperty.Register<ShowCaseZoomOverlay, bool>(nameof(IsOpen));

    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<ShowCaseZoomOverlay, string?>(nameof(Title));

    public static readonly StyledProperty<string?> DescriptionProperty =
        AvaloniaProperty.Register<ShowCaseZoomOverlay, string?>(nameof(Description));

    public static readonly StyledProperty<object?> StageContentProperty =
        AvaloniaProperty.Register<ShowCaseZoomOverlay, object?>(nameof(StageContent));

    public static readonly StyledProperty<double> StageMeasureWidthProperty =
        AvaloniaProperty.Register<ShowCaseZoomOverlay, double>(nameof(StageMeasureWidth), double.NaN);

    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<ShowCaseZoomOverlay>();

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

    public bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string? Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public object? StageContent
    {
        get => GetValue(StageContentProperty);
        set => SetValue(StageContentProperty, value);
    }

    /// <summary>
    /// 内容在原 ShowCaseItem 卡片内的布局宽度。设置后舞台以该宽度测量内容，
    /// 保证放大态与卡片内布局（换行、间距）完全一致；未设置时以舞台宽度测量。
    /// </summary>
    public double StageMeasureWidth
    {
        get => GetValue(StageMeasureWidthProperty);
        set => SetValue(StageMeasureWidthProperty, value);
    }

    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        if (_closeButton is not null)
        {
            _closeButton.Click -= HandleCloseButtonClick;
        }

        _closeButton = e.NameScope.Find<IconButton>(CloseButtonPart);
        _stagePanel  = e.NameScope.Find<ShowCaseZoomStagePanel>(StagePanelPart);
        if (_closeButton is not null)
        {
            _closeButton.Click += HandleCloseButtonClick;
        }

        SyncStageContentToPanel();
    }

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
    /// 舞台内容直接挂到普通 Panel（Panel 不认领逻辑子级），而不是经过
    /// ContentPresenter——宿主随后用 ISetLogicalParent.SetParent 把内容逻辑
    /// 挂回原 ShowCaseItem，保持页面级 Styles/Resources/DataContext 解析；
    /// ContentPresenter 会认领逻辑父级，与该机制冲突。
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
