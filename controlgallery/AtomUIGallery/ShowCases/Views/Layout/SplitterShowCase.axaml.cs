using System;
using AtomUI.Desktop.Controls;
using AtomUIGallery.ShowCases.ViewModels;
using Avalonia.Interactivity;
using ReactiveUI;
using ReactiveUI.Avalonia;

namespace AtomUIGallery.ShowCases.Views;

public partial class SplitterShowCase : ReactiveUserControl<SplitterViewModel>
{
    public SplitterShowCase()
    {
        this.WhenActivated(_ => { });
        InitializeComponent();
        UpdateShowCollapsibleIconMode(SplitterCollapsibleIconDisplayMode.Always);
    }

    private void HandleShowCollapsibleIconChanged(object? sender, RoutedEventArgs e)
    {
        if (sender is not RadioButton { IsChecked: true } radioButton)
        {
            return;
        }

        var mode = ParseShowMode(radioButton.Tag);
        if (mode == null)
        {
            return;
        }

        UpdateShowCollapsibleIconMode(mode.Value);
    }

    private SplitterCollapsibleIconDisplayMode? ParseShowMode(object? tag)
    {
        if (tag is SplitterCollapsibleIconDisplayMode mode)
        {
            return mode;
        }

        if (tag is string text &&
            Enum.TryParse<SplitterCollapsibleIconDisplayMode>(text, true, out var parsed))
        {
            return parsed;
        }

        return null;
    }

    private void UpdateShowCollapsibleIconMode(SplitterCollapsibleIconDisplayMode mode)
    {
        ApplyShowMode(ShowCollapsiblePanelFirst, mode);
        ApplyShowMode(ShowCollapsiblePanelSecond, mode);
        ApplyShowMode(ShowCollapsiblePanelThird, mode);
    }

    private static void ApplyShowMode(SplitterPanel panel, SplitterCollapsibleIconDisplayMode mode)
    {
        panel.Collapsible = new SplitterPanelCollapsible
        {
            IsEnabled = true,
            ShowCollapsibleIcon = mode
        };
    }
}
