using System.Collections;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace AtomUI.Desktop.Controls.Primitives;

/// <summary>
/// Contract of a candidate list that a host control drives by keyboard. The list keeps the committed selection
/// (<see cref="SelectedItem" /> and <see cref="SelectedItems" />) apart from the highlighted candidate
/// (<see cref="CandidateSelectedItem" />) that a commit acts on.
/// </summary>
public interface ICandidateList
{
    /// <summary>
    /// Raised after the selection of the list changed.
    /// </summary>
    event EventHandler<SelectionChangedEventArgs>? SelectionChanged;
    /// <summary>
    /// Gets or sets the selected item. In single selection mode a commit stores the confirmed candidate here before
    /// <see cref="Commit" /> is raised.
    /// Setting an item selects it; setting <see langword="null" /> clears the selection.
    /// </summary>
    object? SelectedItem { get; set; }
    /// <summary>
    /// Gets or sets the selected items in multiple selection mode.
    /// Setting the property selects exactly the listed items; setting <see langword="null" /> clears the selection.
    /// </summary>
    IList? SelectedItems { get; set; }
    /// <summary>
    /// Gets or sets the candidate: the item the highlight currently points at, kept apart from the committed
    /// selection. Up and Down move it among the available items; Enter commits it in single selection mode and
    /// toggles its selection in multiple selection mode.
    /// Setting an item that is not in the list or not available clears the candidate. Setting
    /// <see langword="null" /> clears it as well.
    /// </summary>
    public object? CandidateSelectedItem { get; set; }
    /// <summary>
    /// Gets or sets the items offered as candidates.
    /// </summary>
    IEnumerable? ItemsSource { get; set; }
    /// <summary>
    /// Raised in single selection mode when the user confirms the current candidate with the Enter key or by
    /// activating an item. The confirmed item is already selected when the event is raised; the list resets its
    /// selection after the handlers returned.
    /// </summary>
    event EventHandler<RoutedEventArgs>? Commit;
    /// <summary>
    /// Raised when the user cancels with the Escape key. The list resets its selection and candidate after the
    /// handlers returned.
    /// </summary>
    event EventHandler<RoutedEventArgs>? Cancel;
    /// <summary>
    /// Lets the host operate the list from its own key handling: Enter commits the candidate, Up and Down move it
    /// and Escape cancels.
    /// </summary>
    /// <param name="e">
    /// The key event. <c>Handled</c> is set for Enter, Up, Down and Escape and left unchanged for other keys.
    /// </param>
    void HandleKeyDown(KeyEventArgs e);
}
