// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Diagnostics;
using System.Globalization;
using AtomUI.Desktop.Controls.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace AtomUI.Desktop.Controls;

public abstract class DataGridBoundColumn : DataGridColumn
{
    private BindingBase? _binding;
    private BindingBase? _editingBinding;

    /// <summary>
    /// Gets or sets the binding that associates the column with a property in the data source.
    /// </summary>
    //TODO Binding
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

    //TODO Rename
    //TODO Validation
    protected sealed override Control GenerateEditingElement(DataGridCell cell, object dataItem, out BindingExpressionBase? editBinding)
    {
        Control element = GenerateEditingElementDirect(cell, dataItem);
        editBinding = null;

        // Edits are committed through the data source, so the editor only reads the bound value.
        if (Binding != null && (_editingBinding ??= CreateOneWayBinding(Binding)) is { } editingBinding)
        {
            Debug.Assert(BindingTarget != null);
            editBinding = element.Bind(BindingTarget, editingBinding);
        }

        return element;
    } 

    protected abstract Control GenerateEditingElementDirect(DataGridCell cell, object dataItem);

    protected AvaloniaProperty? BindingTarget { get; set; }

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
        return binding switch
        {
            CompiledBinding compiledBinding => new CompiledBinding
            {
                Path = compiledBinding.Path,
                Mode = BindingMode.OneWay,
                Converter = compiledBinding.Converter,
                ConverterCulture = compiledBinding.ConverterCulture,
                ConverterParameter = compiledBinding.ConverterParameter,
                FallbackValue = compiledBinding.FallbackValue,
                TargetNullValue = compiledBinding.TargetNullValue,
                StringFormat = compiledBinding.StringFormat,
                Source = compiledBinding.Source,
                Priority = compiledBinding.Priority,
                Delay = compiledBinding.Delay
            },
            ReflectionBinding reflectionBinding => new ReflectionBinding
            {
                Path = reflectionBinding.Path,
                Mode = BindingMode.OneWay,
                Converter = reflectionBinding.Converter,
                ConverterCulture = reflectionBinding.ConverterCulture,
                ConverterParameter = reflectionBinding.ConverterParameter,
                FallbackValue = reflectionBinding.FallbackValue,
                TargetNullValue = reflectionBinding.TargetNullValue,
                StringFormat = reflectionBinding.StringFormat,
                Source = reflectionBinding.Source,
                Priority = reflectionBinding.Priority,
                Delay = reflectionBinding.Delay,
                ElementName = reflectionBinding.ElementName,
                RelativeSource = reflectionBinding.RelativeSource,
                TypeResolver = reflectionBinding.TypeResolver
            },
            _ => null
        };
    }

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
