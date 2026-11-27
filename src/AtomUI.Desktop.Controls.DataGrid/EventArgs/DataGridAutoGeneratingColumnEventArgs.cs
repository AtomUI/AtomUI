// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.ComponentModel;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Carries the data for the <see cref="DataGrid.AutoGeneratingColumn" /> event, which the grid raises once
/// for each property of the data source before it adds the column it generated for that property. Cancelling
/// the event, or leaving <see cref="Column" /> null, skips the column.
/// </summary>
public class DataGridAutoGeneratingColumnEventArgs : CancelEventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="T:AtomUI.Desktop.Controls.DataGridAutoGeneratingColumnEventArgs" /> class.
    /// </summary>
    /// <param name="propertyName">
    /// The name of the property bound to the generated column.
    /// </param>
    /// <param name="propertyType">
    /// The <see cref="T:System.Type" /> of the property bound to the generated column.
    /// </param>
    /// <param name="column">
    /// The generated column.
    /// </param>
    public DataGridAutoGeneratingColumnEventArgs(string propertyName, Type propertyType, DataGridColumn? column)
    {
        Column       = column;
        PropertyName = propertyName;
        PropertyType = propertyType;
    }

    /// <summary>
    /// Gets or sets the column that the grid adds for the property. A handler can replace the generated column
    /// with its own, which is then marked as auto-generated; when left null, no column is added.
    /// </summary>
    public DataGridColumn? Column
    {
        get;
        set;
    }

    /// <summary>
    /// Gets the name of the property bound to the generated column.
    /// </summary>
    public string PropertyName
    {
        get;
        private set;
    }

    /// <summary>
    /// Gets the <see cref="T:System.Type" /> of the property bound to the generated column.
    /// </summary>
    public Type PropertyType
    {
        get;
        private set;
    }
}