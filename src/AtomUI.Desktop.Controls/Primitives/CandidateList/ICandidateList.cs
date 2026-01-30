using System.Collections;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace AtomUI.Desktop.Controls.Primitives;

public interface ICandidateList
{
    bool IsCandidateItemNavigationEnabled { get; set; }
    event EventHandler<SelectionChangedEventArgs>? SelectionChanged;
    object? SelectedItem { get; set; }
    IList? SelectedItems { get; set; }
    IEnumerable? ItemsSource { get; set; }
    event EventHandler<RoutedEventArgs>? Commit;
    event EventHandler<RoutedEventArgs>? Cancel;
    void HandleKeyDown(KeyEventArgs e);
}