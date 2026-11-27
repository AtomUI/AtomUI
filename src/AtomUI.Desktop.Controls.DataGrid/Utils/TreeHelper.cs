// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using Avalonia;
using Avalonia.Input;

namespace AtomUI.Desktop.Controls.Utils;

internal static class TreeHelper
{
    /// <summary>
    /// Returns whether the element or one of its descendants has keyboard focus.
    /// </summary>
    /// <param name="element">
    /// The element to check.
    /// </param>
    /// <returns>
    /// <see langword="true" /> when <paramref name="element" /> is an <see cref="InputElement" /> whose
    /// <see cref="InputElement.IsKeyboardFocusWithin" /> is set; <see langword="false" /> for any other element,
    /// including <see langword="null" />.
    /// </returns>
    internal static bool ContainsFocusedElement(this Visual? element)
    {
        return element is InputElement { IsKeyboardFocusWithin: true };
    }
}