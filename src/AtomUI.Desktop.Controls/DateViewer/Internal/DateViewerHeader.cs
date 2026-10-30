using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;

namespace AtomUI.Desktop.Controls.Internal.DateViewer;

internal sealed class DateViewerHeader : TemplatedControl
{
    public static readonly StyledProperty<DateViewerHeaderContext?> ContextProperty =
        AvaloniaProperty.Register<DateViewerHeader, DateViewerHeaderContext?>(nameof(Context));
    public static readonly StyledProperty<IDataTemplate?> HeaderTemplateProperty =
        AtomUI.Desktop.Controls.DateViewer.HeaderTemplateProperty.AddOwner<DateViewerHeader>();
    public static readonly DirectProperty<DateViewerHeader, DateViewerPanelKind> ParentPanelKindProperty =
        AvaloniaProperty.RegisterDirect<DateViewerHeader, DateViewerPanelKind>(nameof(ParentPanelKind), control => control.ParentPanelKind);
    public DateViewerHeaderContext? Context { get => GetValue(ContextProperty); set => SetValue(ContextProperty, value); }
    public IDataTemplate? HeaderTemplate { get => GetValue(HeaderTemplateProperty); set => SetValue(HeaderTemplateProperty, value); }
    public DateViewerPanelKind ParentPanelKind => Context?.PanelKind == DateViewerPanelKind.Date ? DateViewerPanelKind.Month : DateViewerPanelKind.Year;

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ContextProperty)
        {
            RaisePropertyChanged(ParentPanelKindProperty, default, ParentPanelKind);
        }
    }
}
