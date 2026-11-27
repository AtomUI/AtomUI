// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using Avalonia;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls.Utils;

/// <summary>
/// A grid used as the root layout of the row and row group header templates. It provides the IsFrozen attached
/// property, which marks the direct children that the row group header keeps in place while the grid scrolls
/// horizontally.
/// </summary>
public class DataGridFrozenGrid : Grid
{
    /// <summary>
    /// Identifies the IsFrozen attached property, set in XAML as <c>DataGridFrozenGrid.IsFrozen</c> or with
    /// <see cref="SetIsFrozen" />. The value is carried by a direct child of the grid, is not inherited and
    /// defaults to <see langword="false" />; the row group header reads it for the children of its root grid.
    /// </summary>
    public static readonly StyledProperty<bool> IsFrozenProperty =
        AvaloniaProperty.RegisterAttached<DataGridFrozenGrid, Control, bool>("IsFrozen");

    /// <summary>
    /// Gets whether the element is marked as frozen. The row group header reads the mark for the direct children
    /// of its root grid: a frozen, visible child stays in place while the grid scrolls horizontally, and the other
    /// children are clipped where they would overlap the frozen ones.
    /// </summary>
    /// <param name="element">
    /// The element to read the value from.
    /// </param>
    /// <returns>
    /// <see langword="true" /> if the element is marked as frozen; otherwise, <see langword="false" />.
    /// </returns>
    public static bool GetIsFrozen(Control element)
    {
        return element.GetValue(IsFrozenProperty);
    }

    /// <summary>
    /// Marks or unmarks the element as frozen.
    /// </summary>
    /// <param name="element">
    /// The element to mark.
    /// </param>
    /// <param name="value">
    /// <see langword="true" /> to keep the element in place while the grid scrolls horizontally; otherwise,
    /// <see langword="false" />.
    /// </param>
    public static void SetIsFrozen(Control element, bool value)
    {
        element.SetValue(IsFrozenProperty, value);
    }
}