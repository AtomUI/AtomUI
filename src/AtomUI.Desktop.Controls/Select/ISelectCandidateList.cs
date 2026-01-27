using System.Collections;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace AtomUI.Desktop.Controls;

public interface ISelectCandidateList
{
    object? SelectedItem { get; set; }
    IEnumerable? ItemsSource { get; set; }
    event EventHandler<RoutedEventArgs>? Commit;
    event EventHandler<RoutedEventArgs>? Cancel;
    void HandleKeyDown(KeyEventArgs e);
}