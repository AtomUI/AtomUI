// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using AtomUI.Desktop.Controls.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// The base class of columns whose cells read their value from a binding to a property of the data item.
/// When a cell is edited, the editor that the derived column creates is bound to <see cref="Binding" />.
/// </summary>
public abstract class DataGridBoundColumn : DataGridColumn
{
    #region 公共属性定义

    /// <summary>
    /// Gets or sets the binding that associates the column with a property in the data source.
    /// </summary>
    /// <remarks>
    /// A binding in <see cref="BindingMode.OneWayToSource" /> mode is rejected with an
    /// <see cref="InvalidOperationException" />. A binding in the default mode becomes two-way when it has a
    /// path, and a binding without a converter and without a string format uses <c>DataGridValueConverter.Instance</c>.
    /// Assigning a different binding while a row is being edited commits that edit first, or cancels it when
    /// the commit fails, then applies the new binding to the existing rows and drops the cached editing element.
    /// </remarks>
    // TODO Binding: verify that this member handles every binding option that a caller can set and complete it
    // where it is missing. Remove this TODO when that is done.
    [AssignBinding]
    public virtual BindingBase? Binding
    {
        get => _binding;
        set
        {
            if (_binding != value)
            {
                if (OwningGrid != null && !OwningGrid.CommitEdit(DataGridEditingUnit.Row, exitEditingMode: true))
                {
                    // Edited value couldn't be committed, so we force a CancelEdit
                    OwningGrid.CancelEdit(DataGridEditingUnit.Row, raiseEvents: false);
                }

                _binding = value; 
                _editingBinding = null;

                if (_binding != null)
                {
                    if(_binding is { } binding)
                    {
                        if (GetBindingMode(binding) == BindingMode.OneWayToSource)
                        {
                            throw new InvalidOperationException("DataGridColumn doesn't support BindingMode.OneWayToSource. Use BindingMode.TwoWay instead.");
                        }

                        var path = GetBindingPath(binding);
                        if (!string.IsNullOrEmpty(path) && GetBindingMode(binding) == BindingMode.Default)
                        {
                            SetBindingMode(binding, BindingMode.TwoWay);
                        } 

                        if (GetBindingConverter(binding) == null && string.IsNullOrEmpty(GetBindingStringFormat(binding)))
                        {
                            SetBindingConverter(binding, DataGridValueConverter.Instance);
                        }
                    }

                    // Apply the new Binding to existing rows in the DataGrid
                    if (OwningGrid != null)
                    {
                        OwningGrid.HandleColumnBindingChanged(this);
                    }
                } 

                RemoveEditingElement();
            }
        }
    } 

    /// <summary>
    /// The binding that will be used to get or set cell content for the clipboard.
    /// If the base ClipboardContentBinding is not explicitly set, this will return the value of Binding.
    /// </summary>
    public override BindingBase? ClipboardContentBinding
    {
        get => base.ClipboardContentBinding ?? Binding;
        set => base.ClipboardContentBinding = value;
    }

    #endregion

    /// <summary>
    /// Creates the editing element of a cell through <see cref="GenerateEditingElementDirect" /> and binds
    /// <see cref="BindingTarget" /> on it to a one-way copy of <see cref="Binding" />. The copy is created
    /// once per column and reused for every cell.
    /// </summary>
    /// <param name="cell">
    /// The cell that will contain the editing element.
    /// </param>
    /// <param name="dataItem">
    /// The data item of the row that owns the cell.
    /// </param>
    /// <param name="editBinding">
    /// The binding expression that connects the element to the column's binding, or <see langword="null" />
    /// when the column has no <see cref="Binding" />.
    /// </param>
    /// <returns>
    /// The editing element of the cell.
    /// </returns>
    // TODO Rename: verify that the name of this member still describes what it does and rename it where it does
    // not. Remove this TODO when that is done.
    // TODO Validation: verify that this member handles validation of the value that the editor produces and
    // complete it where it is missing. Remove this TODO when that is done.
    protected sealed override Control GenerateEditingElement(DataGridCell cell, object dataItem, out BindingExpressionBase? editBinding)
    {
        Control element = GenerateEditingElementDirect(cell, dataItem);
        editBinding = null;

        // Editors and their binding targets are created per cell at runtime, outside a ControlTheme.
        // Edits are committed through the data source, so the editor only reads the bound value.
        if (Binding != null && (_editingBinding ??= CreateOneWayBinding(Binding)) is { } editingBinding)
        {
            Debug.Assert(BindingTarget != null);
            editBinding = element.Bind(BindingTarget, editingBinding);
        }

        return element;
    } 

    /// <summary>
    /// When overridden in a derived class, creates the editing element of a cell without binding it.
    /// <see cref="GenerateEditingElement" /> binds <see cref="BindingTarget" /> on the returned element.
    /// </summary>
    /// <param name="cell">
    /// The cell that will contain the editing element.
    /// </param>
    /// <param name="dataItem">
    /// The data item of the row that owns the cell.
    /// </param>
    /// <returns>
    /// The editing element of the cell.
    /// </returns>
    protected abstract Control GenerateEditingElementDirect(DataGridCell cell, object dataItem);

    /// <summary>
    /// The property of the editing element that is bound to <see cref="Binding" /> and from which the edited
    /// value is read. A derived column assigns it in its constructor.
    /// </summary>
    protected AvaloniaProperty? BindingTarget { get; set; }

    private BindingBase? _binding;
    private BindingBase? _editingBinding;

    internal void SetHeaderFromBinding()
    {
        if (OwningGrid != null && OwningGrid.RangeDataAccess.DataType != null
                               && Header == null && Binding is { } binding)
        {
            var path = GetBindingPath(binding);
            if (!string.IsNullOrWhiteSpace(path))
            {
                var header = OwningGrid.RangeDataAccess.GetDisplayName(path);
                if (header != null)
                {
                    Header = header;
                }
            }
        }
    }

    /// <summary>
    /// Whether the editor value can be read back and converted without writing to the data item.
    /// </summary>
    internal bool CanEditThroughSource =>
        BindingTarget != null && Binding is CompiledBinding or ReflectionBinding;

    /// <summary>
    /// Reads the editor value and converts it to the field type with the converter the display binding uses.
    /// </summary>
    internal bool TryGetEditedValue(Control editingElement, Type valueType, out object? value, out Exception? error)
    {
        Debug.Assert(BindingTarget != null && Binding != null);
        var editorValue = editingElement.GetValue(BindingTarget);
        var (converter, parameter, culture) = GetBindingConversion(Binding);
        try
        {
            value = (converter ?? DataGridValueConverter.Instance).ConvertBack(
                editorValue,
                valueType,
                parameter,
                culture ?? CultureInfo.CurrentCulture);
        }
        catch (Exception exception)
        {
            value = null;
            error = exception;
            return false;
        }

        if (value is BindingNotification { Error: { } notificationError })
        {
            value = null;
            error = notificationError;
            return false;
        }
        if (value == AvaloniaProperty.UnsetValue || value == BindingOperations.DoNothing)
        {
            value = null;
            error = new InvalidCastException($"The edited value cannot be converted to '{valueType.Name}'.");
            return false;
        }
        if (value is null && valueType.IsValueType && Nullable.GetUnderlyingType(valueType) is null)
        {
            error = new ArgumentNullException(nameof(value), "A value is required.");
            return false;
        }
        error = null;
        return true;
    }

    internal string? GetBindingPath()
    {
        return Binding is null
            ? null
            : GetBindingPath(Binding);
    }

    private static BindingMode GetBindingMode(BindingBase binding)
    {
        return binding switch
        {
            ReflectionBinding reflectionBinding => reflectionBinding.Mode,
            CompiledBinding compiledBinding => compiledBinding.Mode,
            _ => BindingMode.Default
        };
    }

    private static void SetBindingMode(BindingBase binding, BindingMode mode)
    {
        switch (binding)
        {
            case ReflectionBinding reflectionBinding:
                reflectionBinding.Mode = mode;
                break;
            case CompiledBinding compiledBinding:
                compiledBinding.Mode = mode;
                break;
        }
    }

    private static BindingBase? CreateOneWayBinding(BindingBase binding)
    {
        if (binding is not (CompiledBinding or ReflectionBinding))
        {
            return null;
        }

        // Preserve the caller's complete definition, including weak XAML anchors/name scopes.
        // Copying an existing definition does not introduce a ReflectionBinding constructor
        // into compiled-binding applications, and never mutates the caller's binding mode.
        var editingBinding = (BindingBase)CloneBindingDefinition(binding);
        SetBindingMode(editingBinding, BindingMode.OneWay);
        return editingBinding;
    }

    // System.Object's shallow-copy intrinsic is statically resolved by .NET 8/10 NativeAOT;
    // it copies an existing runtime instance without member discovery or binding constructors.
    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "MemberwiseClone")]
    private static extern object CloneBindingDefinition(object source);

    private static (IValueConverter? Converter, object? Parameter, CultureInfo? Culture) GetBindingConversion(
        BindingBase binding)
    {
        return binding switch
        {
            ReflectionBinding reflectionBinding => (
                reflectionBinding.Converter,
                reflectionBinding.ConverterParameter,
                reflectionBinding.ConverterCulture),
            CompiledBinding compiledBinding => (
                compiledBinding.Converter,
                compiledBinding.ConverterParameter,
                compiledBinding.ConverterCulture),
            _ => (null, null, null)
        };
    }

    private static string? GetBindingPath(BindingBase binding)
    {
        return binding switch
        {
            ReflectionBinding reflectionBinding => reflectionBinding.Path,
            CompiledBinding compiledBinding => compiledBinding.Path?.ToString(),
            _ => null
        };
    }

    private static object? GetBindingConverter(BindingBase binding)
    {
        return binding switch
        {
            ReflectionBinding reflectionBinding => reflectionBinding.Converter,
            CompiledBinding compiledBinding => compiledBinding.Converter,
            _ => null
        };
    }

    private static void SetBindingConverter(BindingBase binding, DataGridValueConverter converter)
    {
        switch (binding)
        {
            case ReflectionBinding reflectionBinding:
                reflectionBinding.Converter = converter;
                break;
            case CompiledBinding compiledBinding:
                compiledBinding.Converter = converter;
                break;
        }
    }

    private static string? GetBindingStringFormat(BindingBase binding)
    {
        return binding switch
        {
            ReflectionBinding reflectionBinding => reflectionBinding.StringFormat,
            CompiledBinding compiledBinding => compiledBinding.StringFormat,
            _ => null
        };
    }
}
