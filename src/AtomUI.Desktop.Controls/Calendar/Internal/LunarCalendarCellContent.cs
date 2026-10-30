using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace AtomUI.Desktop.Controls.Internal.Calendar;

/// <summary>Lunar text presentation only. All interaction belongs to DateViewerCell.</summary>
internal sealed class LunarCalendarCellContent : TemplatedControl
{
    public static readonly StyledProperty<LunarCalendarCellContext?> ContextProperty =
        AvaloniaProperty.Register<LunarCalendarCellContent, LunarCalendarCellContext?>(nameof(Context));
    public static readonly StyledProperty<DateViewerPresentation> PresentationProperty =
        AtomUI.Desktop.Controls.DateViewer.PresentationProperty.AddOwner<LunarCalendarCellContent>();
    public static readonly StyledProperty<bool> HighlightWeekendsProperty =
        LunarCalendar.HighlightWeekendsProperty.AddOwner<LunarCalendarCellContent>();
    public static readonly DirectProperty<LunarCalendarCellContent, bool> ShowMarkerProperty =
        AvaloniaProperty.RegisterDirect<LunarCalendarCellContent, bool>(nameof(ShowMarker), content => content.ShowMarker);

    public LunarCalendarCellContext? Context { get => GetValue(ContextProperty); set => SetValue(ContextProperty, value); }
    public DateViewerPresentation Presentation { get => GetValue(PresentationProperty); set => SetValue(PresentationProperty, value); }
    public bool HighlightWeekends { get => GetValue(HighlightWeekendsProperty); set => SetValue(HighlightWeekendsProperty, value); }
    public bool ShowMarker => Context is { IsHoliday: true } or { IsAdjustedWorkday: true };

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ContextProperty || change.Property == HighlightWeekendsProperty)
        {
            PseudoClasses.Set(":weekend", HighlightWeekends && Context?.IsWeekend == true);
            PseudoClasses.Set(":holiday", Context?.IsHoliday == true);
            PseudoClasses.Set(":workday", Context?.IsAdjustedWorkday == true);
            PseudoClasses.Set(":selected", Context?.IsSelected == true);
            PseudoClasses.Set(":disabled", Context?.IsDisabled == true);
            PseudoClasses.Set(":outside", Context?.IsInView == false);
            RaisePropertyChanged(ShowMarkerProperty, default, ShowMarker);
        }
    }
}
