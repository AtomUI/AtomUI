namespace AtomUI;

/// <summary>
/// The names of pseudo-classes that AtomUI controls share, such as <c>:pressed</c> and <c>:selected</c>.
/// A control switches a class with <c>PseudoClasses.Set</c>, <c>Add</c> or <c>Remove</c> and declares it in its
/// <c>PseudoClasses</c> attribute. A theme selects the class by its literal name, as in <c>^:pressed</c>.
/// </summary>
public static class StdPseudoClass
{
    /// <summary>
    /// The <c>:disabled</c> pseudo-class.
    /// <c>RadioIndicator</c> reads it to skip the wave animation of a disabled indicator.
    /// No AtomUI type sets it through this constant.
    /// </summary>
    public const string Disabled = ":disabled";
    /// <summary>
    /// The <c>:focus</c> pseudo-class. <c>DataGridCell</c> sets it while the grid is focused and the cell is current.
    /// </summary>
    public const string Focus = ":focus";
    /// <summary>
    /// The <c>:focus-within</c> pseudo-class.
    /// <c>CompactSpace</c> reads it on the child of an item and treats the item as focused when either the class is
    /// present or the child's <c>IsFocused</c> is <see langword="true" />.
    /// No AtomUI type sets it through this constant.
    /// </summary>
    public const string FocusWithIn = ":focus-within";
    /// <summary>
    /// The <c>:pointerover</c> pseudo-class, for an element while the pointer is over it.
    /// No AtomUI type refers to this constant.
    /// </summary>
    public const string PointerOver = ":pointerover";
    /// <summary>
    /// The <c>:checked</c> pseudo-class, set by <c>CheckBoxIndicator</c>, <c>RadioIndicator</c>, <c>SplitButton</c>,
    /// <c>TreeViewItem</c> and <c>CascaderViewItem</c> while they are checked.
    /// </summary>
    public const string Checked = ":checked";
    /// <summary>
    /// The <c>:unchecked</c> pseudo-class, set by <c>CheckBoxIndicator</c> and <c>RadioIndicator</c> while they
    /// are unchecked. <c>RadioIndicator</c> also sets it while <c>IsChecked</c> is <see langword="null" />.
    /// </summary>
    public const string UnChecked = ":unchecked";
    /// <summary>
    /// The <c>:pressed</c> pseudo-class, set for example by <c>HyperLinkTextBlock</c> and <c>PaginationNavItem</c>
    /// while their <c>IsPressed</c> is <see langword="true" />.
    /// </summary>
    public const string Pressed = ":pressed";
    /// <summary>
    /// The <c>:selected</c> pseudo-class, set by DataGrid rows, cells and row headers while the row is selected,
    /// and by <c>TabOverflowMenuItem</c> while it is selected.
    /// </summary>
    public const string Selected = ":selected";
    /// <summary>
    /// The <c>:indeterminate</c> pseudo-class, set by <c>CheckBoxIndicator</c> while its state is indeterminate.
    /// </summary>
    public const string Indeterminate = ":indeterminate";
    /// <summary>
    /// The <c>:dragging</c> pseudo-class, set by <c>SplitterDragBar</c> and <c>SplitterHandle</c> while dragging.
    /// </summary>
    public const string Dragging = ":dragging";
    /// <summary>
    /// The <c>:drag-indicator</c> pseudo-class, added by <c>DataGridColumnHeader</c> to the drag indicator used
    /// while a column is dragged.
    /// </summary>
    public const string DragIndicator = ":drag-indicator";
    /// <summary>
    /// The <c>:empty</c> pseudo-class, set by <c>SizeTypeAwareIconPresenter</c> while its <c>Icon</c> is
    /// <see langword="null" />.
    /// </summary>
    public const string Empty = ":empty";
    /// <summary>
    /// The <c>:flyout-open</c> pseudo-class, set by <c>SplitButton</c> while its flyout is open.
    /// </summary>
    public const string FlyoutOpen = ":flyout-open";
    /// <summary>
    /// The <c>:expanded</c> pseudo-class, set by <c>CascaderViewItem</c> and <c>DataGridRowGroupHeader</c> while
    /// they are expanded.
    /// </summary>
    public const string Expanded = ":expanded";
    /// <summary>
    /// The <c>:open</c> pseudo-class, set by <c>NavMenuItem</c> while its sub-menu is open and by
    /// <c>FullScreenTitleBarHost</c> while it is open.
    /// </summary>
    public const string Open = ":open";
    /// <summary>
    /// The <c>:horizontal</c> pseudo-class, set by <c>Slider</c>, <c>SliderTrack</c> and <c>Steps</c> while their
    /// <c>Orientation</c> is horizontal.
    /// </summary>
    public const string Horizontal = ":horizontal";
    /// <summary>
    /// The <c>:vertical</c> pseudo-class, set by <c>Slider</c>, <c>SliderTrack</c> and <c>Steps</c> while their
    /// <c>Orientation</c> is vertical.
    /// </summary>
    public const string Vertical = ":vertical";
    /// <summary>
    /// The <c>:active</c> pseudo-class, set by <c>GradientColorSliderThumb</c> while <c>IsActivated</c> is
    /// <see langword="true" /> and by <c>WindowTitleBar</c> while the window is active.
    /// </summary>
    public const string Active = ":active";
    /// <summary>
    /// The <c>:inactive</c> pseudo-class, the counterpart of <see cref="Active" />.
    /// No AtomUI type refers to this constant.
    /// </summary>
    public const string InActive = ":inactive";
    /// <summary>
    /// The <c>:draggable</c> pseudo-class, set by <c>TreeView</c> while <c>IsDraggable</c> is <see langword="true" />.
    /// </summary>
    public const string Draggable = ":draggable";
    /// <summary>
    /// The <c>:current</c> pseudo-class, set by DataGrid cells, row headers and group headers while they are current.
    /// </summary>
    public const string Current = ":current";
    /// <summary>
    /// The <c>:edited</c> pseudo-class, set by <c>DataGridCell</c> while <c>IsEdited</c> is <see langword="true" />.
    /// </summary>
    public const string Edited = ":edited";
    /// <summary>
    /// The <c>:editing</c> pseudo-class, set by DataGrid rows and row headers while the row is being edited.
    /// </summary>
    public const string Editing = ":editing";
    /// <summary>
    /// The <c>:invalid</c> pseudo-class, set by DataGrid rows, cells and row headers while their <c>IsValid</c> is
    /// <see langword="false" />.
    /// </summary>
    public const string Invalid = ":invalid";
    /// <summary>
    /// The <c>:sort-ascending</c> pseudo-class, set by <c>DataGridColumnHeader</c> while the column is sorted in
    /// ascending order.
    /// </summary>
    public const string SortAscending = ":sort-ascending";
    /// <summary>
    /// The <c>:sort-descending</c> pseudo-class, set by <c>DataGridColumnHeader</c> while the column is sorted in
    /// descending order.
    /// </summary>
    public const string SortDescending = ":sort-descending";
    /// <summary>
    /// The <c>:minimized</c> pseudo-class, set by <c>WindowTitleBar</c> and <c>CaptionButtonGroup</c> while the host
    /// window is minimized.
    /// </summary>
    public const string Minimized = ":minimized";
    /// <summary>
    /// The <c>:normal</c> pseudo-class, set by <c>WindowTitleBar</c> and <c>CaptionButtonGroup</c> while the host
    /// window is neither minimized, maximized nor full screen.
    /// </summary>
    public const string Normal = ":normal";
    /// <summary>
    /// The <c>:maximized</c> pseudo-class, set by <c>WindowTitleBar</c> and <c>CaptionButtonGroup</c> while the host
    /// window is maximized.
    /// </summary>
    public const string Maximized = ":maximized";
    /// <summary>
    /// The <c>:fullscreen</c> pseudo-class, set by <c>WindowTitleBar</c> and <c>CaptionButtonGroup</c> while the host
    /// window is full screen.
    /// </summary>
    public const string Fullscreen = ":fullscreen";
    /// <summary>
    /// The <c>:error</c> pseudo-class, set by <c>NotificationCard</c> while its type is
    /// <c>NotificationType.Error</c>.
    /// </summary>
    public const string Error = ":error";
    /// <summary>
    /// The <c>:information</c> pseudo-class, set by <c>NotificationCard</c> while its type is
    /// <c>NotificationType.Information</c>.
    /// </summary>
    public const string Information = ":information";
    /// <summary>
    /// The <c>:success</c> pseudo-class, set by <c>NotificationCard</c> while its type is
    /// <c>NotificationType.Success</c>.
    /// </summary>
    public const string Success = ":success";
    /// <summary>
    /// The <c>:warning</c> pseudo-class, set by <c>NotificationCard</c> while its type is
    /// <c>NotificationType.Warning</c>, and by input controls while their status is <c>InputControlStatus.Warning</c>.
    /// </summary>
    public const string Warning = ":warning";
}