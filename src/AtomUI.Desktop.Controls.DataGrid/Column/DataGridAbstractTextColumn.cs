using System.ComponentModel;
using AtomUI.Desktop.Controls.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

public abstract class DataGridAbstractTextColumn :  DataGridBoundColumn
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the FontFamily dependency property.
    /// </summary>
    public static readonly AttachedProperty<FontFamily> FontFamilyProperty =
        TextElement.FontFamilyProperty.AddOwner<DataGridAbstractTextColumn>();
    
    /// <summary>
    /// Identifies the FontSize dependency property.
    /// </summary>
    public static readonly AttachedProperty<double> FontSizeProperty =
        TextElement.FontSizeProperty.AddOwner<DataGridAbstractTextColumn>();
    
    /// <summary>
    /// Identifies the FontStyle dependency property.
    /// </summary>
    public static readonly AttachedProperty<FontStyle> FontStyleProperty =
        TextElement.FontStyleProperty.AddOwner<DataGridAbstractTextColumn>();
    
    /// <summary>
    /// Identifies the FontWeight dependency property.
    /// </summary>
    public static readonly AttachedProperty<FontWeight> FontWeightProperty =
        TextElement.FontWeightProperty.AddOwner<DataGridAbstractTextColumn>();
    
    /// <summary>
    /// Identifies the FontStretch dependency property.
    /// </summary>
    public static readonly AttachedProperty<FontStretch> FontStretchProperty =
        TextElement.FontStretchProperty.AddOwner<DataGridAbstractTextColumn>();
    
    /// <summary>
    /// Identifies the Foreground dependency property.
    /// </summary>
    public static readonly AttachedProperty<IBrush?> ForegroundProperty =
        TextElement.ForegroundProperty.AddOwner<DataGridAbstractTextColumn>();
    
    /// <summary>
    /// Gets or sets the font name.
    /// </summary>
    public FontFamily FontFamily
    {
        get => GetValue(FontFamilyProperty);
        set => SetValue(FontFamilyProperty, value);
    }

    /// <summary>
    /// Gets or sets the font size.
    /// </summary>
    // Use DefaultValue here so undo in the Designer will set this to NaN
    [DefaultValue(double.NaN)]
    public double FontSize
    {
        get => GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the font style.
    /// </summary>
    public FontStyle FontStyle
    {
        get => GetValue(FontStyleProperty);
        set => SetValue(FontStyleProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the font weight or thickness.
    /// </summary>
    public FontWeight FontWeight
    {
        get => GetValue(FontWeightProperty);
        set => SetValue(FontWeightProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the font weight or thickness.
    /// </summary>
    public FontStretch FontStretch
    {
        get => GetValue(FontStretchProperty);
        set => SetValue(FontStretchProperty, value);
    }
    
    /// <summary>
    /// Gets or sets a brush that describes the foreground of the column cells.
    /// </summary>
    public IBrush? Foreground
    {
        get => GetValue(ForegroundProperty);
        set => SetValue(ForegroundProperty, value);
    }
    #endregion
    
    /// <summary>
    /// Gets a read-only <see cref="T:AtomUI.Desktop.Controls.TextBlock" /> element that is bound to the column's <see cref="P:AtomUI.Desktop.Controls.DataGridBoundColumn.Binding" /> property value.
    /// </summary>
    /// <param name="cell">The cell that will contain the generated element.</param>
    /// <param name="dataItem">The data item represented by the row that contains the intended cell.</param>
    /// <returns>A new, read-only <see cref="T:AtomUI.Desktop.Controls.TextBlock" /> element that is bound to the column's <see cref="P:AtomUI.Desktop.Controls.DataGridBoundColumn.Binding" /> property value.</returns>
    protected override Control GenerateElement(DataGridCell cell, object dataItem)
    {
        var textBlockElement = new TextBlock
        {
            Name = "CellTextBlock"
        };

        SyncProperties(textBlockElement);

        if (Binding != null)
        {
            textBlockElement.Bind(TextBlock.TextProperty, Binding);
        }
        return textBlockElement;
    }
    
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == FontFamilyProperty
            || change.Property == FontSizeProperty
            || change.Property == FontStyleProperty
            || change.Property == FontWeightProperty
            || change.Property == ForegroundProperty)
        {
            NotifyPropertyChanged(change.Property.Name);
        }
    }

    /// <summary>
    /// Called by the DataGrid control when this column asks for its elements to be
    /// updated, because a property changed.
    /// </summary>
    protected internal override void RefreshCellContent(Control element, string propertyName)
    {
        if (element == null)
        {
            throw new ArgumentNullException(nameof(element));
        }

        if (element is AvaloniaObject content)
        {
            if (propertyName == nameof(FontFamily))
            {
                DataGridHelper.SyncColumnProperty(this, content, FontFamilyProperty);
            }
            else if (propertyName == nameof(FontSize))
            {
                DataGridHelper.SyncColumnProperty(this, content, FontSizeProperty);
            }
            else if (propertyName == nameof(FontStyle))
            {
                DataGridHelper.SyncColumnProperty(this, content, FontStyleProperty);
            }
            else if (propertyName == nameof(FontWeight))
            {
                DataGridHelper.SyncColumnProperty(this, content, FontWeightProperty);
            }
            else if (propertyName == nameof(Foreground))
            {
                DataGridHelper.SyncColumnProperty(this, content, ForegroundProperty);
            }
        }
        else
        {
            throw DataGridError.DataGrid.ValueIsNotAnInstanceOf("element", typeof(AvaloniaObject));
        }
    }

    protected void SyncProperties(AvaloniaObject content)
    {
        DataGridHelper.SyncColumnProperty(this, content, FontFamilyProperty);
        DataGridHelper.SyncColumnProperty(this, content, FontSizeProperty);
        DataGridHelper.SyncColumnProperty(this, content, FontStyleProperty);
        DataGridHelper.SyncColumnProperty(this, content, FontWeightProperty);
        DataGridHelper.SyncColumnProperty(this, content, ForegroundProperty);
    }
}