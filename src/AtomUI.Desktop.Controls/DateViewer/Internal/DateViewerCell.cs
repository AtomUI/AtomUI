using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.VisualTree;
using AtomUI.Controls;
using DateViewerControl = AtomUI.Desktop.Controls.DateViewer;
using AtomUI.Generated.AtomUIDesktopControls;

namespace AtomUI.Desktop.Controls.Internal.DateViewer;

/// <summary>Common interaction container; templates replace content, never selection ownership.</summary>
internal class DateViewerCell : TemplatedControl
{
    public static readonly DirectProperty<DateViewerCell, string> DisplayTextProperty =
        AvaloniaProperty.RegisterDirect<DateViewerCell, string>(nameof(DisplayText), cell => cell.DisplayText);
    public static readonly DirectProperty<DateViewerCell, DateViewerCellContext?> ContextProperty =
        AvaloniaProperty.RegisterDirect<DateViewerCell, DateViewerCellContext?>(nameof(Context), cell => cell.Context);
    public static readonly StyledProperty<IDataTemplate?> CellTemplateProperty =
        DateViewerControl.CellTemplateProperty.AddOwner<DateViewerCell>();
    public static readonly StyledProperty<IDataTemplate?> FullCellTemplateProperty =
        DateViewerControl.FullCellTemplateProperty.AddOwner<DateViewerCell>();
    public static readonly StyledProperty<DateViewerPresentation> PresentationProperty =
        DateViewerControl.PresentationProperty.AddOwner<DateViewerCell>();
    public static readonly StyledProperty<IDataTemplate?> DefaultFullCellTemplateProperty =
        DateViewerControl.DefaultFullCellTemplateProperty.AddOwner<DateViewerCell>();
    public static readonly DirectProperty<DateViewerCell, IDataTemplate?> EffectiveFullCellTemplateProperty =
        AvaloniaProperty.RegisterDirect<DateViewerCell, IDataTemplate?>(nameof(EffectiveFullCellTemplate), cell => cell.EffectiveFullCellTemplate);
    internal static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<DateViewerCell>();

    public string DisplayText => _model?.DisplayText ?? string.Empty;
    public DateViewerCellModel? Model => _model;
    public DateViewerCellContext? Context => _context;
    public IDataTemplate? CellTemplate { get => GetValue(CellTemplateProperty); set => SetValue(CellTemplateProperty, value); }
    public IDataTemplate? FullCellTemplate { get => GetValue(FullCellTemplateProperty); set => SetValue(FullCellTemplateProperty, value); }
    public DateViewerPresentation Presentation { get => GetValue(PresentationProperty); set => SetValue(PresentationProperty, value); }
    public IDataTemplate? DefaultFullCellTemplate { get => GetValue(DefaultFullCellTemplateProperty); set => SetValue(DefaultFullCellTemplateProperty, value); }
    public IDataTemplate? EffectiveFullCellTemplate => FullCellTemplate ?? (CellTemplate is null ? DefaultFullCellTemplate : null);
    internal bool IsMotionEnabled { get => GetValue(IsMotionEnabledProperty); set => SetValue(IsMotionEnabledProperty, value); }
    internal DatePanelSession? Session => _session;
    internal long ContentRevision { get; private set; }

    private DateViewerCellModel? _model;
    private DatePanelSession? _session;
    private DateViewerCellContext? _context;
    private bool _pressed;

    public DateViewerCell()
    {
        Focusable = true;
        Classes.Add(DateViewerSemanticParts.CellClass);
    }

    protected override Avalonia.Automation.Peers.AutomationPeer OnCreateAutomationPeer() => new DateViewerCellAutomationPeer(this);

    public void Bind(DatePanelSession session, DateViewerCellModel model)
    {
        var oldText = DisplayText;
        _session = session;
        _model = model;
        ContentRevision = session.ContentRevision;
        var previousContext = _context;
        _context = model.Value.HasValue && model.Kind != DateViewerCellType.Week && (CellTemplate is not null || EffectiveFullCellTemplate is not null)
            ? session.CreateContext(model) : null;
        RaisePropertyChanged(ContextProperty, previousContext, _context);
        SetCurrentValue(IsEnabledProperty, !model.IsDisabled);
        SetCurrentValue(FocusableProperty, model.IsFocusable);
        RaisePropertyChanged(DisplayTextProperty, oldText, DisplayText);
        UpdateState();
    }

    internal void Unbind()
    {
        var oldText = DisplayText;
        SetPressed(false);
        _session = null;
        ContentRevision = 0;
        _model = null;
        var previousContext = _context;
        _context = null;
        RaisePropertyChanged(ContextProperty, previousContext, null);
        CellTemplate = null;
        FullCellTemplate = null;
        DefaultFullCellTemplate = null;
        Theme = null;
        SetCurrentValue(IsEnabledProperty, true);
        SetCurrentValue(FocusableProperty, false);
        RaisePropertyChanged(DisplayTextProperty, oldText, string.Empty);
        UpdateState();
    }

    public void Activate()
    {
        if (IsEffectivelyEnabled && _session is { } session && _model is { Value: { } value, IsDisabled: false } model)
        {
            session.Apply(new DatePanelAction.Activate(value, model.Kind));
        }
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (e.Handled || !IsEffectivelyEnabled || IsNestedInput(e.Source) || !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            return;
        }

        SetPressed(true);
        e.Handled = true;
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (!_pressed || e.InitialPressMouseButton != MouseButton.Left)
        {
            return;
        }

        SetPressed(false);
        e.Handled = true;
        if (this.GetVisualsAt(e.GetPosition(this)).Any(visual => visual == this || this.IsVisualAncestorOf(visual)))
        {
            Activate();
        }
    }

    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        base.OnPointerCaptureLost(e);
        SetPressed(false);
    }

    protected override void OnLostFocus(FocusChangedEventArgs e)
    {
        base.OnLostFocus(e);
        SetPressed(false);
    }

    protected override void OnGotFocus(FocusChangedEventArgs e)
    {
        base.OnGotFocus(e);
        if (ReferenceEquals(e.Source, this) && _model is { Value: { } value })
        {
            _session?.Apply(new DatePanelAction.Focus(value));
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Handled || !IsEffectivelyEnabled || IsNestedInput(e.Source))
        {
            return;
        }

        if (e.Key == Key.Enter)
        {
            Activate();
            e.Handled = true;
        }
        else if (e.Key == Key.Space && IsFocused)
        {
            SetPressed(true);
            e.Handled = true;
        }
    }

    protected override void OnKeyUp(KeyEventArgs e)
    {
        base.OnKeyUp(e);
        if (e.Key == Key.Space && IsFocused && _pressed && !IsNestedInput(e.Source))
        {
            SetPressed(false);
            Activate();
            e.Handled = true;
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsEnabledProperty && !IsEnabled)
        {
            SetPressed(false);
        }

        if (change.Property == CellTemplateProperty || change.Property == FullCellTemplateProperty || change.Property == DefaultFullCellTemplateProperty || change.Property == PresentationProperty)
        {
            PseudoClasses.Set(":cell-template", CellTemplate is not null);
            PseudoClasses.Set(":full-template", EffectiveFullCellTemplate is not null);
            PseudoClasses.Set(":content", Presentation == DateViewerPresentation.Content);
            RaisePropertyChanged(EffectiveFullCellTemplateProperty, null, EffectiveFullCellTemplate);
        }
    }

    internal bool IsNestedInput(object? source)
    {
        for (var visual = source as Visual; visual is not null && !ReferenceEquals(visual, this); visual = visual.GetVisualParent())
        {
            if (visual is InputElement { Focusable: true })
            {
                return true;
            }
        }
        return false;
    }

    private void SetPressed(bool pressed)
    {
        _pressed = pressed;
        PseudoClasses.Set(":pressed", pressed);
    }

    private void UpdateState()
    {
        var model = _model;
        PseudoClasses.Set(":week-mode", _session?.Input is { PanelKind: DateViewerPanelKind.Date, SelectionUnit: DateViewerSelectionUnit.Week });
        PseudoClasses.Set(":hovered", model?.IsHovered == true);
        PseudoClasses.Set(":last-week-day", model is not null && _session is not null &&
            model.Column == _session.Models[0].Columns - 1);
        // One visual-endpoint state keeps selected-to-preview transitions atomic while
        // the detailed selection and range pseudo classes remain available semantically.
        PseudoClasses.Set(":visual-endpoint", model?.IsVisualEndpoint == true);
        PseudoClasses.Set(":date", model?.Kind == DateViewerCellType.Date);
        PseudoClasses.Set(":week", model?.Kind == DateViewerCellType.Week);
        PseudoClasses.Set(":month", model?.Kind == DateViewerCellType.Month);
        PseudoClasses.Set(":quarter", model?.Kind == DateViewerCellType.Quarter);
        PseudoClasses.Set(":year", model?.Kind == DateViewerCellType.Year);
        PseudoClasses.Set(":today", model?.IsToday == true);
        PseudoClasses.Set(":selected", model?.IsSelected == true);
        PseudoClasses.Set(":outside", model?.IsInView == false);
        PseudoClasses.Set(":disabled", model?.IsDisabled == true);
        PseudoClasses.Set(":focused", model?.IsFocused == true);
        PseudoClasses.Set(":navigation-current", model?.IsNavigationCurrent == true);
        PseudoClasses.Set(":range-start", model?.IsRangeStart == true);
        PseudoClasses.Set(":range-end", model?.IsRangeEnd == true);
        PseudoClasses.Set(":range-middle", model?.IsRangeMiddle == true);
        PseudoClasses.Set(":range-preview-start", model?.IsRangePreviewStart == true);
        PseudoClasses.Set(":range-preview-end", model?.IsRangePreviewEnd == true);
        PseudoClasses.Set(":range-preview-middle", model?.IsRangePreviewMiddle == true);
        PseudoClasses.Set(":has-preview", model?.HasRangePreview == true);
        PseudoClasses.Set(":week-selection-start", model?.IsWeekSelectionStart == true);
        PseudoClasses.Set(":week-selection-middle", model?.IsWeekSelectionMiddle == true);
        PseudoClasses.Set(":week-selection-end", model?.IsWeekSelectionEnd == true);
    }
}
