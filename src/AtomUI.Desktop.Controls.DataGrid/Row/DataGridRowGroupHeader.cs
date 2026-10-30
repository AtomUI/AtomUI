// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Diagnostics;
using System.Reactive.Linq;
using AtomUI.Desktop.Controls.Utils;
using AtomUI.Generated.AtomUIDesktopControlsDataGrid;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Mixins;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[PseudoClasses(StdPseudoClass.Pressed, StdPseudoClass.Current, StdPseudoClass.Expanded)]
public class DataGridRowGroupHeader : TemplatedControl
{
    #region 公共属性定义

    public static readonly StyledProperty<bool> IsItemCountVisibleProperty =
        AvaloniaProperty.Register<DataGridRowGroupHeader, bool>(nameof(IsItemCountVisible));
    
    public static readonly StyledProperty<string> ItemCountFormatProperty =
        AvaloniaProperty.Register<DataGridRowGroupHeader, string>(nameof(ItemCountFormat));
    
    public static readonly StyledProperty<string?> PropertyNameProperty =
        AvaloniaProperty.Register<DataGridRowGroupHeader, string?>(nameof(PropertyName));
    
    public static readonly StyledProperty<bool> IsPropertyNameVisibleProperty =
        AvaloniaProperty.Register<DataGridRowGroupHeader, bool>(nameof(IsPropertyNameVisible));
    
    public static readonly StyledProperty<double> SublevelIndentProperty =
        AvaloniaProperty.Register<DataGridRowGroupHeader, double>(
            nameof(SublevelIndent),
            defaultValue: DataGrid.DefaultRowGroupSublevelIndent,
            validate: IsValidSublevelIndent);
    
    /// <summary>
    /// Gets or sets a value that indicates whether the item count is visible.
    /// </summary>
    public bool IsItemCountVisible
    {
        get => GetValue(IsItemCountVisibleProperty);
        set => SetValue(IsItemCountVisibleProperty, value);
    }
    
    /// <summary>
    /// Gets or sets a value that indicates number format of items count
    /// </summary>
    public string ItemCountFormat
    {
        get => GetValue(ItemCountFormatProperty);
        set => SetValue(ItemCountFormatProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the name of the property that this <see cref="T:AtomUI.Desktop.Controls.DataGrid" /> row is bound to.
    /// </summary>
    public string? PropertyName
    {
        get => GetValue(PropertyNameProperty);
        set => SetValue(PropertyNameProperty, value);
    }
    
    /// <summary>
    /// Gets or sets a value that indicates whether the property name is visible.
    /// </summary>
    public bool IsPropertyNameVisible
    {
        get => GetValue(IsPropertyNameVisibleProperty);
        set => SetValue(IsPropertyNameVisibleProperty, value);
    }
    
    /// <summary>
    /// Gets or sets a value that indicates the amount that the
    /// children of the <see cref="T:AtomUI.Desktop.Controls.RowGroupHeader" /> are indented.
    /// </summary>
    public double SublevelIndent
    {
        get => GetValue(SublevelIndentProperty);
        set => SetValue(SublevelIndentProperty, value);
    }
    
    #endregion

    #region 内部属性定义

    internal DataGridRowHeader? HeaderCell => _headerElement;

    private bool IsCurrent
    {
        get
        {
            Debug.Assert(OwningGrid != null);
            return DisplaySlot == OwningGrid.CurrentSlot;
        }
    }

    internal int DisplaySlot => SourceSlot;

    private bool IsMouseOver { get; set; }

    internal bool IsRecycled { get; set; }

    internal int Level { get; set; }

    internal DataGrid? OwningGrid { get; set; }

    internal DataGridGroupEntry? SourceGroup { get; set; }

    internal int SourceSlot { get; set; } = -1;

    internal double TotalIndent
    {
        set
        {
            _totalIndent = value;
            if (_indentSpacer != null)
            {
                _indentSpacer.Width = _totalIndent;
            }
        }
    }

    #endregion
    
    private bool _areIsCheckedHandlersSuspended;
    private ToggleButton? _expanderButton;
    private DataGridRowHeader? _headerElement;
    private Control? _indentSpacer;
    private TextBlock? _itemCountElement;
    private TextBlock? _propertyNameElement;
    private Panel? _rootElement;
    private double _totalIndent;
    private IDisposable? _expanderButtonSubscription;
    private Dictionary<Visual, RectangleGeometry>? _childClipGeometries;
    private Dictionary<Visual, TranslateTransform>? _childTranslateTransforms;
    
    private static bool IsValidSublevelIndent(double value)
    {
        return !double.IsNaN(value) && !double.IsInfinity(value) && value >= 0;
    }
    
    static DataGridRowGroupHeader()
    {
        SublevelIndentProperty.Changed.AddClassHandler<DataGridRowGroupHeader>((x,e) => x.HandleSublevelIndentChanged(e));
        PointerPressedEvent.AddClassHandler<DataGridRowGroupHeader>(
            (x, e) => x.HandlePointerPressed(e),
            handledEventsToo: true);
        PressedMixin.Attach<DataGridRowGroupHeader>();
        IsTabStopProperty.OverrideDefaultValue<DataGridRowGroupHeader>(false);
    }

    public DataGridRowGroupHeader()
    {
        Classes.Add(DataGridSemanticParts.BodyRowClass);
    }

    private void HandleSublevelIndentChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (OwningGrid != null)
        {
            OwningGrid.OnSublevelIndentUpdated(this, (double)(change.NewValue ?? 0));
        }
    }
    
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        ClearChildClipGeometries();
        ClearChildTranslateTransforms();
        _rootElement = e.NameScope.Find<Panel>(DataGridRowThemeConstants.FramePart) ??
                       e.NameScope.Find<Panel>(DataGridRowGroupHeaderThemeConstants.RootLayoutPart);

        _expanderButtonSubscription?.Dispose();
        _expanderButton = e.NameScope.Find<ToggleButton>(DataGridRowGroupHeaderThemeConstants.ExpanderButtonPart);
        if(_expanderButton != null)
        {
            EnsureExpanderButtonIsChecked();
            _expanderButtonSubscription =
                _expanderButton.GetObservable(ToggleButton.IsCheckedProperty)
                               .Skip(1)
                               .Subscribe(HandleExpanderButtonIsCheckedChanged);
        }

        _headerElement = e.NameScope.Find<DataGridRowHeader>(DataGridRowThemeConstants.RowHeaderPart) ??
                         e.NameScope.Find<DataGridRowHeader>(DataGridRowGroupHeaderThemeConstants.RowHeaderPart);
        if(_headerElement != null)
        {
            _headerElement.Owner = this;
            EnsureHeaderVisibility();
        }

        _indentSpacer = e.NameScope.Find<Control>(DataGridRowGroupHeaderThemeConstants.IndentSpacerPart);
        if(_indentSpacer != null)
        {
            _indentSpacer.Width = _totalIndent;
        }

        _itemCountElement    = e.NameScope.Find<TextBlock>(DataGridRowGroupHeaderThemeConstants.ItemCountElementPart);
        _propertyNameElement = e.NameScope.Find<TextBlock>(DataGridRowGroupHeaderThemeConstants.PropertyNameElementPart);
        UpdateTitleElements();
    }

    internal void ApplyHeaderStatus()
    {
        Debug.Assert(OwningGrid != null);
        if (_headerElement != null && OwningGrid.IsRowHeadersVisible)
        {
            _headerElement.UpdatePseudoClasses();
        }
    }

    internal void UpdatePseudoClasses()
    {
        PseudoClasses.Set(StdPseudoClass.Current, IsCurrent);

        PseudoClasses.Set(
            StdPseudoClass.Expanded,
            SourceGroup is { LeafCount: > 0 } group &&
            !(OwningGrid?.GroupExpansion.CollapsedGroups.Contains(group.Key) ?? false));
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        if (OwningGrid == null)
        {
            return base.ArrangeOverride(finalSize);
        }

        Size size = base.ArrangeOverride(finalSize);
        if (_rootElement != null)
        {
            if (OwningGrid.IsRowGroupHeadersFrozen)
            {
                ClearChildClipGeometries();
                ClearChildTranslateTransforms();
            }
            else
            {
                double frozenLeftEdge = 0;
                foreach (Control child in _rootElement.Children)
                {
                    if (DataGridFrozenGrid.GetIsFrozen(child) && child.IsVisible)
                    {
                        // Automatic layout rounding doesn't apply to transforms so we need to Round this
                        var transform = UpdateChildTranslateTransform(child, Math.Round(OwningGrid.HorizontalOffset));

                        double childLeftEdge = child.Translate(this, new Point(child.Bounds.Width, 0)).X - transform.X;
                        frozenLeftEdge = Math.Max(frozenLeftEdge, childLeftEdge + OwningGrid.HorizontalOffset);
                    }
                }
                // Clip the non-frozen elements so they don't overlap the frozen ones
                foreach (Control child in _rootElement.Children)
                {
                    if (!DataGridFrozenGrid.GetIsFrozen(child))
                    {
                        EnsureChildClip(child, frozenLeftEdge);
                    }
                }
            }
        }
        return size;
    }

    internal void ClearFrozenStates()
    {
        ClearChildTranslateTransforms();
    }

    private TranslateTransform UpdateChildTranslateTransform(Visual child, double x)
    {
        _childTranslateTransforms ??= new Dictionary<Visual, TranslateTransform>(_rootElement?.Children.Count ?? 0);
        if (!_childTranslateTransforms.TryGetValue(child, out var transform))
        {
            transform = new TranslateTransform();
            _childTranslateTransforms.Add(child, transform);
        }

        if (!transform.X.Equals(x))
        {
            transform.X = x;
        }

        if (!transform.Y.Equals(0))
        {
            transform.Y = 0;
        }

        if (!ReferenceEquals(child.RenderTransform, transform))
        {
            child.RenderTransform = transform;
        }

        return transform;
    }

    private void ClearChildTranslateTransforms()
    {
        if (_rootElement != null)
        {
            foreach (Control child in _rootElement.Children)
            {
                if (child.RenderTransform is not null)
                {
                    child.RenderTransform = null;
                }
            }
        }
        _childTranslateTransforms?.Clear();
    }

    //TODO TabStop
    private void HandlePointerPressed(PointerPressedEventArgs e)
    {
        if (OwningGrid == null)
        {
            return;
        }
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            if (OwningGrid.IsDoubleClickRecordsClickOnCall(this) && !e.Handled)
            {
                ToggleExpandCollapse(!IsExpanded, true);
                e.Handled = true;
            }
            else
            {
                if (!e.Handled && OwningGrid.IsTabStop)
                {
                    OwningGrid.Focus();
                }
                e.Handled = OwningGrid.UpdateStateOnMouseLeftButtonDown(e, OwningGrid.CurrentColumnIndex, DisplaySlot, allowEdit: false);
            }
        }
        else if (e.GetCurrentPoint(this).Properties.IsRightButtonPressed)
        {
            if (!e.Handled)
            {
                OwningGrid.Focus();
            }
            e.Handled = OwningGrid.UpdateStateOnMouseRightButtonDown(e, OwningGrid.CurrentColumnIndex, DisplaySlot, allowEdit: false);
        }

    }

    private void EnsureChildClip(Visual child, double frozenLeftEdge)
    {
        double childLeftEdge = child.Translate(this, new Point(0, 0)).X;
        if (frozenLeftEdge > childLeftEdge)
        {
            double xClip = Math.Round(frozenLeftEdge - childLeftEdge);
            var    clipRect = new Rect(xClip, 0,
                Math.Max(0, child.Bounds.Width - xClip),
                child.Bounds.Height);
            UpdateChildClipGeometry(child, clipRect);
        }
        else
        {
            ClearChildClipGeometry(child);
        }
    }

    private void UpdateChildClipGeometry(Visual child, Rect clipRect)
    {
        _childClipGeometries ??= new Dictionary<Visual, RectangleGeometry>(_rootElement?.Children.Count ?? 0);
        if (!_childClipGeometries.TryGetValue(child, out var clipGeometry))
        {
            clipGeometry = new RectangleGeometry
            {
                Rect = clipRect
            };
            _childClipGeometries.Add(child, clipGeometry);
        }
        else if (!clipGeometry.Rect.Equals(clipRect))
        {
            clipGeometry.Rect = clipRect;
            child.InvalidateVisual();
        }

        if (!ReferenceEquals(child.Clip, clipGeometry))
        {
            child.Clip = clipGeometry;
        }
    }

    private void ClearChildClipGeometry(Visual child)
    {
        if (child.Clip is not null)
        {
            child.Clip = null;
        }
    }

    private void ClearChildClipGeometries()
    {
        if (_rootElement != null)
        {
            foreach (Control child in _rootElement.Children)
            {
                ClearChildClipGeometry(child);
            }
        }
        _childClipGeometries?.Clear();
    }

    internal void EnsureExpanderButtonIsChecked()
    {
        if (_expanderButton != null && HasChildren)
        {
            SetIsCheckedNoCallBack(IsExpanded);
        }
    }

    internal void EnsureHeaderVisibility()
    {
        if (_headerElement != null && OwningGrid != null)
        {
            _headerElement.IsVisible = OwningGrid.IsRowHeadersVisible;
        }
    }

    private void HandleExpanderButtonIsCheckedChanged(bool? value)
    {
        if(!_areIsCheckedHandlersSuspended)
        {
            ToggleExpandCollapse(value ?? false, true);
        }
    }

    internal void LoadVisualsForDisplay()
    {
        EnsureExpanderButtonIsChecked();
        EnsureHeaderVisibility();
        EnsureGridLines();
        UpdatePseudoClasses();
        ApplyHeaderStatus();
    }

    internal void EnsureGridLines()
    {
        _headerElement?.EnsureGridLines();
    }

    protected override void OnPointerEntered(PointerEventArgs e)
    {
        if (IsEnabled)
        {
            IsMouseOver = true;
            UpdatePseudoClasses();
        }

        base.OnPointerEntered(e);
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        if (IsEnabled)
        {
            IsMouseOver = false;
            UpdatePseudoClasses();
        }

        base.OnPointerExited(e);
    }

    private void SetIsCheckedNoCallBack(bool value)
    {
        if (_expanderButton != null && _expanderButton.IsChecked != value)
        {
            _areIsCheckedHandlersSuspended = true;
            try
            {
                _expanderButton.IsChecked = value;
            }
            finally
            {
                _areIsCheckedHandlersSuspended = false;
            }
        }
    }

    internal void ToggleExpandCollapse(bool isVisible, bool setCurrent)
    {
        if (!HasChildren)
        {
            return;
        }
        if (SourceGroup is not null)
        {
            if (OwningGrid is not null && IsExpanded != isVisible)
            {
                if (isVisible)
                {
                    OwningGrid.ExpandGroup(SourceGroup.Key);
                }
                else
                {
                    OwningGrid.CollapseGroup(SourceGroup.Key);
                }
            }
        }
    }

    internal void UpdateTitleElements()
    {
        if (_propertyNameElement != null)
        {
            string txt;
            if (string.IsNullOrWhiteSpace(PropertyName))
            {
                txt = string.Empty;
            }
            else
            {
                txt = $"{PropertyName}:";
            }
            _propertyNameElement.Text = txt;
        }
        if (_itemCountElement != null && SourceGroup is not null)
        {
            string formatString;
            if (SourceGroup.LeafCount == 1)
            {
                formatString = string.IsNullOrEmpty(ItemCountFormat) ? "({0} Item)" : ItemCountFormat;
            }
            else
            {
                formatString = string.IsNullOrEmpty(ItemCountFormat) ? "({0} Items)" : ItemCountFormat;
            }
            _itemCountElement.Text = string.Format(formatString, SourceGroup.LeafCount);
        }
    }

    internal void DetachFromDataGrid()
    {
        OwningGrid = null;
        DataContext = null;
        SourceGroup = null;
        SourceSlot = -1;
        Level = 0;
        PropertyName = null;
        IsMouseOver = false;
        IsRecycled = true;
        PseudoClasses.Set(StdPseudoClass.Current, false);
        PseudoClasses.Set(StdPseudoClass.Expanded, false);
    }

    private bool HasChildren => SourceGroup?.LeafCount > 0;

    private bool IsExpanded => SourceGroup is not null &&
                               !(OwningGrid?.GroupExpansion.CollapsedGroups.Contains(SourceGroup.Key) ?? false);

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _expanderButtonSubscription?.Dispose();
        _expanderButtonSubscription = null;
        ClearChildClipGeometries();
        ClearChildTranslateTransforms();
    }
}
