using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using AtomUI.Controls;
using AtomUI.Controls.Primitives;
using AtomUI.Controls.Utils;
using AtomUI.Generated.AtomUIDesktopControls;
using AtomUI.Animations;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Metadata;

namespace AtomUI.Desktop.Controls;

using AvaloniaTreeView = Avalonia.Controls.TreeView;

/// <summary>
/// The region of a node row that is painted with the hover and the selection backgrounds.
/// <see cref="TreeView.NodeHoverMode" /> selects it.
/// </summary>
public enum TreeItemHoverMode
{
    /// <summary>
    /// The label of the node. The background covers the header content only and is as wide as that content.
    /// </summary>
    Default,
    /// <summary>
    /// The header of the node. The background covers the header content, stretched to the width available to it.
    /// </summary>
    Block,
    /// <summary>
    /// The whole row. The hover background is painted on the header across the full width of the row.
    /// </summary>
    WholeLine
}

/// <summary>
/// What the filter does with the nodes of the tree: which it highlights, shows in bold, expands or hides.
/// The values are flags and can be combined.
/// </summary>
[Flags]
public enum TreeFilterStrategy
{
    /// <summary>
    /// Counts the matches only: nodes are not highlighted, shown in bold or hidden, and the expanded state from
    /// before the filter is restored.
    /// </summary>
    None = 0,
    /// <summary>
    /// Highlights each occurrence of the filter value in the header of every matching node. It takes precedence
    /// over <see cref="HighlightedWhole" />.
    /// </summary>
    HighlightedMatch = 0x01,
    /// <summary>
    /// Highlights the whole header of every matching node, unless <see cref="HighlightedMatch" /> is also set.
    /// </summary>
    HighlightedWhole = 0x02,
    /// <summary>
    /// Shows the header of every matching node in bold.
    /// </summary>
    BoldedMatch = 0x04,
    /// <summary>
    /// Leaves the tree expanded after the filter pass instead of restoring the expanded state from before it,
    /// so that the path to every match is expanded.
    /// </summary>
    ExpandPath = 0x08,
    /// <summary>
    /// Hides every node that does not match and has no matching descendant. The nodes above a match stay visible.
    /// </summary>
    HideUnMatched = 0x10,
    /// <summary>
    /// <see cref="HighlightedMatch" />, <see cref="BoldedMatch" />, <see cref="ExpandPath" /> and
    /// <see cref="HideUnMatched" />: shows only the matching nodes and the nodes above them, highlighted, in bold
    /// and expanded.
    /// </summary>
    MatchedOnly = HighlightedMatch | BoldedMatch | ExpandPath | HideUnMatched,
    /// <summary>
    /// <see cref="HighlightedMatch" />, <see cref="BoldedMatch" /> and <see cref="ExpandPath" />: shows every
    /// node, expanded, with the matching nodes highlighted and in bold.
    /// </summary>
    FullTree = HighlightedMatch | BoldedMatch | ExpandPath,
    /// <summary>
    /// Every option except <see cref="HighlightedWhole" />; the same value as <see cref="MatchedOnly" /> and the
    /// default of <see cref="TreeView.FilterStrategy" />.
    /// </summary>
    All = MatchedOnly
}

/// <summary>
/// A tree of hierarchical nodes, each shown by a <see cref="TreeViewItem" /> container. It supports selecting
/// several nodes, check boxes, child nodes loaded on demand, filtering and dragging nodes. As a form item its
/// value is the selection.
/// </summary>
[PseudoClasses(StdPseudoClass.Draggable)]
public partial class TreeView : AvaloniaTreeView, 
                                IMotionAwareControl,
                                IFormItemAware
{
    #region 公共属性定义
    /// <summary>
    /// Identifies the <see cref="SelectedItems" /> property.
    /// </summary>
    public static readonly new DirectProperty<TreeView, IList> SelectedItemsProperty =
        AvaloniaTreeView.SelectedItemsProperty.AddOwner<TreeView>(
            o => o.SelectedItems,
            (o, v) => o.SelectedItems = v);

    /// <summary>
    /// Identifies the <see cref="IsAutoExpandParent" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsAutoExpandParentProperty =
        AvaloniaProperty.Register<TreeView, bool>(nameof(IsAutoExpandParent), true);
    
    /// <summary>
    /// Identifies the <see cref="IsDraggable" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsDraggableProperty =
        AvaloniaProperty.Register<TreeView, bool>(nameof(IsDraggable));

    /// <summary>
    /// Identifies the <see cref="IsShowIcon" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsShowIconProperty =
        AvaloniaProperty.Register<TreeView, bool>(nameof(IsShowIcon));

    /// <summary>
    /// Identifies the <see cref="IsShowLine" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsShowLineProperty =
        AvaloniaProperty.Register<TreeView, bool>(nameof(IsShowLine));
    
    /// <summary>
    /// Identifies the <see cref="IsDefaultExpandAll" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsDefaultExpandAllProperty =
        AvaloniaProperty.Register<TreeView, bool>(
            nameof(IsDefaultExpandAll));

    /// <summary>
    /// Identifies the <see cref="NodeHoverMode" /> property.
    /// </summary>
    public static readonly StyledProperty<TreeItemHoverMode> NodeHoverModeProperty =
        AvaloniaProperty.Register<TreeView, TreeItemHoverMode>(nameof(NodeHoverMode), TreeItemHoverMode.Default);
    
    /// <summary>
    /// Identifies the <see cref="SwitcherExpandIcon" /> property.
    /// </summary>
    public static readonly StyledProperty<IconTemplate?> SwitcherExpandIconProperty =
        AvaloniaProperty.Register<TreeView, IconTemplate?>(nameof(SwitcherExpandIcon));

    /// <summary>
    /// Identifies the <see cref="SwitcherCollapseIcon" /> property.
    /// </summary>
    public static readonly StyledProperty<IconTemplate?> SwitcherCollapseIconProperty =
        AvaloniaProperty.Register<TreeView, IconTemplate?>(nameof(SwitcherCollapseIcon));

    /// <summary>
    /// Identifies the <see cref="SwitcherRotationIcon" /> property.
    /// </summary>
    public static readonly StyledProperty<IconTemplate?> SwitcherRotationIconProperty =
        AvaloniaProperty.Register<TreeView, IconTemplate?>(nameof(SwitcherRotationIcon));

    /// <summary>
    /// Identifies the <see cref="SwitcherLoadingIcon" /> property.
    /// </summary>
    public static readonly StyledProperty<IconTemplate?> SwitcherLoadingIconProperty =
        AvaloniaProperty.Register<TreeView, IconTemplate?>(nameof(SwitcherLoadingIcon));

    /// <summary>
    /// Identifies the <see cref="SwitcherLeafIcon" /> property.
    /// </summary>
    public static readonly StyledProperty<IconTemplate?> SwitcherLeafIconProperty =
        AvaloniaProperty.Register<TreeView, IconTemplate?>(nameof(SwitcherLeafIcon));

    /// <summary>
    /// Identifies the <see cref="IsShowLeafIcon" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsShowLeafIconProperty =
        AvaloniaProperty.Register<TreeView, bool>(nameof(IsShowLeafIcon));
    
    /// <summary>
    /// Identifies the <see cref="IsSwitcherRotation" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsSwitcherRotationProperty = 
        AvaloniaProperty.Register<TreeView, bool>(nameof(IsSwitcherRotation), true);
    
    /// <summary>
    /// Identifies the <see cref="IsSelectable" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsSelectableProperty =
        AvaloniaProperty.Register<TreeView, bool>(nameof(IsSelectable), true);

    /// <summary>
    /// Identifies the <see cref="IsSelectOnRightClick" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsSelectOnRightClickProperty =
        AvaloniaProperty.Register<TreeView, bool>(nameof(IsSelectOnRightClick), true);

    /// <summary>
    /// Identifies the <see cref="IsCheckStrictly" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsCheckStrictlyProperty = 
        AvaloniaProperty.Register<TreeView, bool>(nameof(IsCheckStrictly), false);

    /// <summary>
    /// Identifies the <see cref="IsMotionEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<TreeView>();
    
    /// <summary>
    /// Identifies the <see cref="OpenMotion" /> property.
    /// </summary>
    public static readonly StyledProperty<AbstractMotion?> OpenMotionProperty =
        AvaloniaProperty.Register<TreeView, AbstractMotion?>(nameof(OpenMotion));
        
    /// <summary>
    /// Identifies the <see cref="CloseMotion" /> property.
    /// </summary>
    public static readonly StyledProperty<AbstractMotion?> CloseMotionProperty =
        AvaloniaProperty.Register<TreeView, AbstractMotion?>(nameof(CloseMotion));
    
    /// <summary>
    /// Identifies the <see cref="ToggleType" /> property.
    /// </summary>
    public static readonly StyledProperty<ItemToggleType> ToggleTypeProperty =
        AvaloniaProperty.Register<TreeView, ItemToggleType>(nameof(ToggleType), ItemToggleType.None);
    
    /// <summary>
    /// Identifies the <see cref="DefaultCheckedPaths" /> property.
    /// </summary>
    public static readonly DirectProperty<TreeView, IList<TreeNodePath>?> DefaultCheckedPathsProperty =
        AvaloniaProperty.RegisterDirect<TreeView, IList<TreeNodePath>?>(
            nameof(DefaultCheckedPaths),
            o => o.DefaultCheckedPaths,
            (o, v) => o.DefaultCheckedPaths = v);
    
    /// <summary>
    /// Identifies the <see cref="DefaultSelectedPaths" /> property.
    /// </summary>
    public static readonly DirectProperty<TreeView, IList<TreeNodePath>?> DefaultSelectedPathsProperty =
        AvaloniaProperty.RegisterDirect<TreeView, IList<TreeNodePath>?>(
            nameof(DefaultSelectedPaths),
            o => o.DefaultSelectedPaths,
            (o, v) => o.DefaultSelectedPaths = v);
    
    /// <summary>
    /// Identifies the <see cref="DefaultExpandedPaths" /> property.
    /// </summary>
    public static readonly DirectProperty<TreeView, IList<TreeNodePath>?> DefaultExpandedPathsProperty =
        AvaloniaProperty.RegisterDirect<TreeView, IList<TreeNodePath>?>(
            nameof(DefaultExpandedPaths),
            o => o.DefaultExpandedPaths,
            (o, v) => o.DefaultExpandedPaths = v);
    
    /// <summary>
    /// Identifies the <see cref="DataLoader" /> property.
    /// </summary>
    public static readonly StyledProperty<ITreeItemNodeLoader?> DataLoaderProperty =
        AvaloniaProperty.Register<TreeView, ITreeItemNodeLoader?>(nameof(DataLoader));
    
    /// <summary>
    /// Identifies the <see cref="Filter" /> property.
    /// </summary>
    public static readonly StyledProperty<IValueFilter?> FilterProperty =
        AvaloniaProperty.Register<TreeView, IValueFilter?>(nameof(Filter));

    /// <summary>
    /// Identifies the <see cref="FilterValue" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> FilterValueProperty =
        AvaloniaProperty.Register<TreeView, object?>(nameof(FilterValue));

    /// <summary>
    /// Identifies the <see cref="FilterValueSelector" /> property.
    /// </summary>
    public static readonly StyledProperty<DefaultFilterValueSelector?> FilterValueSelectorProperty =
        AvaloniaProperty.Register<TreeView, DefaultFilterValueSelector?>(nameof(FilterValueSelector));
    
    /// <summary>
    /// Identifies the <see cref="FilterStrategy" /> property.
    /// </summary>
    public static readonly StyledProperty<TreeFilterStrategy> FilterStrategyProperty =
        AvaloniaProperty.Register<TreeView, TreeFilterStrategy>(nameof(FilterStrategy), TreeFilterStrategy.All);
    
    /// <summary>
    /// Identifies the <see cref="FilterResultCount" /> property.
    /// </summary>
    public static readonly DirectProperty<TreeView, int> FilterResultCountProperty =
        AvaloniaProperty.RegisterDirect<TreeView, int>(nameof(FilterResultCount),
            o => o.FilterResultCount);
    
    /// <summary>
    /// Identifies the <see cref="FilterHighlightForeground" /> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> FilterHighlightForegroundProperty =
        AvaloniaProperty.Register<TreeView, IBrush?>(nameof(FilterHighlightForeground));
    
    /// <summary>
    /// Identifies the <see cref="EmptyIndicator" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> EmptyIndicatorProperty =
        AvaloniaProperty.Register<TreeView, object?>(nameof(EmptyIndicator));
    
    /// <summary>
    /// Identifies the <see cref="EmptyIndicatorTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> EmptyIndicatorTemplateProperty =
        AvaloniaProperty.Register<TreeView, IDataTemplate?>(nameof(EmptyIndicatorTemplate));
    
    /// <summary>
    /// Identifies the <see cref="IsShowEmptyIndicator" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsShowEmptyIndicatorProperty =
        AvaloniaProperty.Register<TreeView, bool>(nameof(IsShowEmptyIndicator), true);
    
    /// <summary>
    /// Identifies the <see cref="EmptyIndicatorPadding" /> property.
    /// </summary>
    public static readonly StyledProperty<Thickness> EmptyIndicatorPaddingProperty =
        AvaloniaProperty.Register<TreeView, Thickness>(nameof(EmptyIndicatorPadding));
    
    /// <summary>
    /// Gets or sets the collection of selected nodes. Assigning a collection replaces the selection with its
    /// entries; assigning <see langword="null" /> is accepted and clears the selection. While
    /// <c>SelectionMode.Multiple</c> is selected, this collection is the value of the form item.
    /// </summary>
    [AllowNull]
    public new IList SelectedItems
    {
        get => base.SelectedItems;
        set
        {
            var oldValue = base.SelectedItems;
            SyncingSelectedItems = true;
            _syncingSelectedItemsTarget = value;
            try
            {
                base.SelectedItems = value;
            }
            finally
            {
                _syncingSelectedItemsTarget = null;
                SyncingSelectedItems = false;
            }
            RaisePropertyChanged(SelectedItemsProperty, oldValue, base.SelectedItems);
        }
    }

    /// <summary>
    /// Expands the nodes above a node once it has expanded, so that it is not hidden inside a collapsed parent.
    /// </summary>
    public bool IsAutoExpandParent
    {
        get => GetValue(IsAutoExpandParentProperty);
        set => SetValue(IsAutoExpandParentProperty, value);
    }

    /// <summary>
    /// Lets the user drag a node with the left mouse button onto or beside another node of this tree. The drag
    /// reports its progress through the <see cref="ItemDragStarted" />, <see cref="ItemDragEnter" />,
    /// <see cref="ItemDragOver" />, <see cref="ItemDragLeave" />, <see cref="ItemDropped" /> and
    /// <see cref="ItemDragCompleted" /> events.
    /// </summary>
    public bool IsDraggable
    {
        get => GetValue(IsDraggableProperty);
        set => SetValue(IsDraggableProperty, value);
    }
    
    /// <summary>
    /// Shows the icon of each node in front of its header.
    /// </summary>
    public bool IsShowIcon
    {
        get => GetValue(IsShowIconProperty);
        set => SetValue(IsShowIconProperty, value);
    }

    /// <summary>
    /// Shows the guide lines that connect the indentation levels of the nodes.
    /// </summary>
    public bool IsShowLine
    {
        get => GetValue(IsShowLineProperty);
        set => SetValue(IsShowLineProperty, value);
    }
    
    /// <summary>
    /// Expands every node when the tree is loaded and again when a new items source has been shown, instead of
    /// showing only the root nodes. It takes precedence over <see cref="DefaultExpandedPaths" />.
    /// </summary>
    public bool IsDefaultExpandAll
    {
        get => GetValue(IsDefaultExpandAllProperty);
        set => SetValue(IsDefaultExpandAllProperty, value);
    }

    /// <summary>
    /// Selects the region of a node row that is painted with the hover and the selection backgrounds. The default,
    /// <see cref="TreeItemHoverMode.Default" />, paints only the label of the node.
    /// </summary>
    public TreeItemHoverMode NodeHoverMode
    {
        get => GetValue(NodeHoverModeProperty);
        set => SetValue(NodeHoverModeProperty, value);
    }
    
    /// <summary>
    /// The icon that the switcher of an expanded node shows, in place of the icon of the theme. The value is
    /// applied to every node of the tree.
    /// </summary>
    public IconTemplate? SwitcherExpandIcon
    {
        get => GetValue(SwitcherExpandIconProperty);
        set => SetValue(SwitcherExpandIconProperty, value);
    }

    /// <summary>
    /// The icon that the switcher of a collapsed node shows, in place of the icon of the theme. The value is
    /// applied to every node of the tree.
    /// </summary>
    public IconTemplate? SwitcherCollapseIcon
    {
        get => GetValue(SwitcherCollapseIconProperty);
        set => SetValue(SwitcherCollapseIconProperty, value);
    }

    /// <summary>
    /// The icon that the switcher of a node with children shows while <see cref="IsSwitcherRotation" /> is
    /// <see langword="true" />. It is turned by 90 degrees while the node is expanded. <see langword="null" />,
    /// the default, uses the built-in caret icon.
    /// </summary>
    public IconTemplate? SwitcherRotationIcon
    {
        get => GetValue(SwitcherRotationIconProperty);
        set => SetValue(SwitcherRotationIconProperty, value);
    }

    /// <summary>
    /// The icon that the switcher shows while the children of the node are being loaded through
    /// <see cref="DataLoader" />.
    /// </summary>
    public IconTemplate? SwitcherLoadingIcon
    {
        get => GetValue(SwitcherLoadingIconProperty);
        set => SetValue(SwitcherLoadingIconProperty, value);
    }

    /// <summary>
    /// The icon that the switcher of a leaf node shows while <see cref="IsShowLeafIcon" /> is
    /// <see langword="true" />.
    /// </summary>
    public IconTemplate? SwitcherLeafIcon
    {
        get => GetValue(SwitcherLeafIconProperty);
        set => SetValue(SwitcherLeafIconProperty, value);
    }

    /// <summary>
    /// Shows <see cref="SwitcherLeafIcon" /> in front of a leaf node. While <see langword="false" />, guide lines
    /// are drawn in the place of the leaf icon.
    /// </summary>
    public bool IsShowLeafIcon
    {
        get => GetValue(IsShowLeafIconProperty);
        set => SetValue(IsShowLeafIconProperty, value);
    }
    
    /// <summary>
    /// Uses <see cref="SwitcherRotationIcon" /> as the single icon of the switcher of a node with children, which
    /// turns when the node expands, instead of switching between <see cref="SwitcherExpandIcon" /> and
    /// <see cref="SwitcherCollapseIcon" />.
    /// </summary>
    public bool IsSwitcherRotation
    {
        get => GetValue(IsSwitcherRotationProperty);
        set => SetValue(IsSwitcherRotationProperty, value);
    }
    
    /// <summary>
    /// Lets the user select a node by clicking it or with the keyboard. While <see langword="false" />, clicking
    /// a node does not change the selection, and setting it to <see langword="false" /> clears the current
    /// selection. A node can still opt out on its own while this is <see langword="true" />.
    /// </summary>
    public bool IsSelectable
    {
        get => GetValue(IsSelectableProperty);
        set => SetValue(IsSelectableProperty, value);
    }

    /// <summary>
    /// Selects the node under the pointer when it is right-clicked, before its context menu opens. While
    /// <see langword="false" />, a right-click keeps the current selection.
    /// </summary>
    public bool IsSelectOnRightClick
    {
        get => GetValue(IsSelectOnRightClickProperty);
        set => SetValue(IsSelectOnRightClickProperty, value);
    }

    /// <summary>
    /// Checks every node independently. While <see langword="false" />, the default, and <see cref="ToggleType" />
    /// is <see cref="ItemToggleType.CheckBox" />, checking a node checks its whole subtree, and the check state of
    /// its ancestors is updated from their children.
    /// </summary>
    public bool IsCheckStrictly
    {
        get => GetValue(IsCheckStrictlyProperty);
        set => SetValue(IsCheckStrictlyProperty, value);
    }

    /// <summary>
    /// Plays the open and close motion when a node expands or collapses, and the turn of the switcher icon. The
    /// theme sets it from the <c>EnableMotion</c> token; a value set on the tree overrides the token.
    /// </summary>
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    /// <summary>
    /// The motion that plays on the children of a node when it expands. <see langword="null" />, the default,
    /// uses the built-in open motion.
    /// </summary>
    public AbstractMotion? OpenMotion
    {
        get => GetValue(OpenMotionProperty);
        set => SetValue(OpenMotionProperty, value);
    }
    
    /// <summary>
    /// The motion that plays on the children of a node when it collapses. <see langword="null" />, the default,
    /// uses the built-in close motion.
    /// </summary>
    public AbstractMotion? CloseMotion
    {
        get => GetValue(CloseMotionProperty);
        set => SetValue(CloseMotionProperty, value);
    }
    
    /// <summary>
    /// The control that is shown in front of a node to check it. <see cref="ItemToggleType.CheckBox" /> shows a
    /// check box on every checkable node, <see cref="ItemToggleType.Radio" /> a radio button on the leaf nodes
    /// only, and <see cref="ItemToggleType.None" />, the default, none of them.
    /// </summary>
    public ItemToggleType ToggleType
    {
        get => GetValue(ToggleTypeProperty);
        set => SetValue(ToggleTypeProperty, value);
    }
    
    private IList<TreeNodePath>? _defaultCheckedPaths;
    
    /// <summary>
    /// The nodes that are checked by default, as <see cref="TreeNodePath" /> values. They are applied after the
    /// first layout of the tree, and applied again when the tree is loaded while a path did not resolve to a
    /// node or the property was set to another list.
    /// </summary>
    public IList<TreeNodePath>? DefaultCheckedPaths
    {
        get => _defaultCheckedPaths;
        set => SetAndRaise(DefaultCheckedPathsProperty, ref _defaultCheckedPaths, value);
    }
    
    private IList<TreeNodePath>? _defaultSelectedPaths;
    
    /// <summary>
    /// The nodes that are selected by default, as <see cref="TreeNodePath" /> values. They are applied after the
    /// first layout of the tree and whenever this property is set. On the first layout the nodes are selected
    /// only while nothing is selected yet and every path resolves to a node.
    /// </summary>
    public IList<TreeNodePath>? DefaultSelectedPaths
    {
        get => _defaultSelectedPaths;
        set => SetAndRaise(DefaultSelectedPathsProperty, ref _defaultSelectedPaths, value);
    }
    
    private IList<TreeNodePath>? _defaultExpandedPaths;
    
    /// <summary>
    /// The nodes that are expanded by default, as <see cref="TreeNodePath" /> values: every node on a path is
    /// expanded. It is ignored while <see cref="IsDefaultExpandAll" /> is <see langword="true" />.
    /// </summary>
    public IList<TreeNodePath>? DefaultExpandedPaths
    {
        get => _defaultExpandedPaths;
        set => SetAndRaise(DefaultExpandedPathsProperty, ref _defaultExpandedPaths, value);
    }
    
    /// <summary>
    /// Loads the children of a node on demand when the node is expanded. It requires
    /// <see cref="ItemsControl.ItemsSource" /> to be set: a load that starts without an items source throws an
    /// <see cref="InvalidOperationException" />. Only nodes whose item implements <see cref="ITreeItemNode" />
    /// are loaded. <see cref="TreeItemLoaded" /> reports the result of a load; on success the children are
    /// appended and the node is expanded. A load can take up to <see cref="AsyncLoadTimeout" />.
    /// </summary>
    public ITreeItemNodeLoader? DataLoader
    {
        get => GetValue(DataLoaderProperty);
        set => SetValue(DataLoaderProperty, value);
    }
    
    /// <summary>
    /// Decides whether a node matches <see cref="FilterValue" />. It receives the value that
    /// <see cref="FilterValueSelector" /> returns for the node and the filter value. While it is
    /// <see langword="null" />, the tree is not filtered. When it is still <see langword="null" /> when the tree
    /// is initialized, it is set to a <c>ValueFilterMode.Contains</c> filter.
    /// </summary>
    public IValueFilter? Filter
    {
        get => GetValue(FilterProperty);
        set => SetValue(FilterProperty, value);
    }
    
    /// <summary>
    /// The value that the nodes are matched against while <see cref="Filter" /> is set. Changing it runs the
    /// filter again. <see langword="null" />, or a blank string, shows every node; a value set before the tree
    /// is loaded is applied when the tree is loaded.
    /// </summary>
    public object? FilterValue
    {
        get => GetValue(FilterValueProperty);
        set => SetValue(FilterValueProperty, value);
    }

    /// <summary>
    /// Selects the value of a node that <see cref="Filter" /> compares with <see cref="FilterValue" />.
    /// <see langword="null" />, the default, uses the header text of the node: the header of a
    /// <see cref="TreeViewItem" /> converted to a string.
    /// </summary>
    public DefaultFilterValueSelector? FilterValueSelector
    {
        get => GetValue(FilterValueSelectorProperty);
        set => SetValue(FilterValueSelectorProperty, value);
    }

    /// <summary>
    /// What the filter does with the nodes it matches, as a combination of <see cref="TreeFilterStrategy" />
    /// flags. The default, <see cref="TreeFilterStrategy.All" />, shows only the matching nodes with the nodes
    /// above them, highlighted, bold and expanded.
    /// </summary>
    public TreeFilterStrategy FilterStrategy
    {
        get => GetValue(FilterStrategyProperty);
        set => SetValue(FilterStrategyProperty, value);
    }

    private int _filterResultCount;
    
    /// <summary>
    /// The number of nodes that matched the last filter pass; <c>0</c> while no filter is applied. It is updated
    /// by <see cref="FilterTreeNode" /> and drives the empty indicator while the tree is filtered.
    /// </summary>
    public int FilterResultCount
    {
        get => _filterResultCount;
        private set => SetAndRaise(FilterResultCountProperty, ref _filterResultCount, value);
    }
    
    /// <summary>
    /// The content shown in place of an empty tree, presented by <see cref="EmptyIndicatorTemplate" />. The tree
    /// is empty while <see cref="IsShowEmptyIndicator" /> is <see langword="true" /> and it has no nodes or the
    /// filter matches none. The built-in indicator shows only while this property and
    /// <see cref="EmptyIndicatorTemplate" /> are both <see langword="null" />.
    /// </summary>
    [DependsOn(nameof(EmptyIndicatorTemplate))]
    public object? EmptyIndicator
    {
        get => GetValue(EmptyIndicatorProperty);
        set => SetValue(EmptyIndicatorProperty, value);
    }

    /// <summary>
    /// The template that presents <see cref="EmptyIndicator" /> in the place of an empty tree. Setting it, even
    /// without content, replaces the built-in empty indicator.
    /// </summary>
    public IDataTemplate? EmptyIndicatorTemplate
    {
        get => GetValue(EmptyIndicatorTemplateProperty);
        set => SetValue(EmptyIndicatorTemplateProperty, value);
    }
    
    /// <summary>
    /// Shows the empty indicator while the tree has no nodes or the filter matches none. While
    /// <see langword="false" />, an empty tree shows nothing.
    /// </summary>
    public bool IsShowEmptyIndicator
    {
        get => GetValue(IsShowEmptyIndicatorProperty);
        set => SetValue(IsShowEmptyIndicatorProperty, value);
    }
    
    /// <summary>
    /// Insets the empty indicator, built-in or custom, from the edge of the tree. The theme sets it from the
    /// <c>PaddingXL</c> token.
    /// </summary>
    public Thickness EmptyIndicatorPadding
    {
        get => GetValue(EmptyIndicatorPaddingProperty);
        set => SetValue(EmptyIndicatorPaddingProperty, value);
    }
    
    /// <summary>
    /// The foreground of the header text that matches <see cref="FilterValue" /> while
    /// <see cref="FilterStrategy" /> includes a highlight flag. The theme binds it to the
    /// <c>FilterHighlightBrush</c> token of the node header; <see langword="null" /> leaves the text in the
    /// normal foreground.
    /// </summary>
    public IBrush? FilterHighlightForeground
    {
        get => GetValue(FilterHighlightForegroundProperty);
        set => SetValue(FilterHighlightForegroundProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the items of the checked nodes. A read-only collection is rejected with a
    /// <see cref="NotSupportedException" />. Changes made to the assigned collection afterwards update the check
    /// state of the nodes. Assigning <see langword="null" /> is accepted and replaces the checked items with an
    /// empty list.
    /// </summary>
    [AllowNull]
    public IList CheckedItems
    {
        get
        {
            if (_checkedItems == null)
            {
                _checkedItems = new AvaloniaList<object>();
                SubscribeToCheckedItems();
            }

            return _checkedItems;
        }
        set
        {
            if (value?.IsReadOnly == true)
            {
                throw new NotSupportedException(
                    "Cannot use a fixed size or read-only collection as CheckedItems.");
            }

            UnsubscribeFromCheckedItems();
            _checkedItems = value ?? new AvaloniaList<object>();
            SubscribeToCheckedItems();
            RecordExplicitCheckedUpdateDuringReplay(true);
        }
    }

    #endregion

    #region 公共事件定义
    
    /// <summary>
    /// Raised after the items in <see cref="CheckedItems" /> were changed by an update of the check state, with
    /// the items that the update removed and added. It is not raised while an items source replay is in progress.
    /// </summary>
    public event EventHandler<TreeViewCheckedItemsChangedEventArgs>? CheckedItemsChanged;
    /// <summary>
    /// Raised after <see cref="DataLoader" /> finished loading the children of a node, with the result of the
    /// load, whether it succeeded, timed out, was canceled or failed. It is raised before successful children
    /// are appended and the node is expanded.
    /// </summary>
    public event EventHandler<TreeViewItemLoadedEventArgs>? TreeItemLoaded;
    /// <summary>
    /// Raised when a press on a node with the left mouse button moves past the drag threshold and the drag
    /// starts. The node is already flagged as dragging; the event cannot cancel the drag.
    /// </summary>
    public event EventHandler<TreeViewDragStartedEventArgs>? ItemDragStarted;
    /// <summary>
    /// Raised when the drag of a node ends, whether or not the node was dropped. The tree and the node are no
    /// longer flagged as dragging, and <see cref="ItemDropped" /> has already been raised for a performed drop.
    /// </summary>
    public event EventHandler<TreeViewDragCompletedEventArgs>? ItemDragCompleted;
    /// <summary>
    /// Raised while a node is dragged and the node under the pointer changes to another node of this tree. It
    /// follows <see cref="ItemDragLeave" /> for the node the pointer left, when there was one. The new node is
    /// already flagged as drag-over.
    /// </summary>
    public event EventHandler<TreeViewDragEnterEventArgs>? ItemDragEnter;
    /// <summary>
    /// Raised while a node is dragged and the pointer leaves the node it was over. That node is no longer
    /// flagged as drag-over. Ending the drag over a node does not raise this event.
    /// </summary>
    public event EventHandler<TreeViewDragLeaveEventArgs>? ItemDragLeave;
    /// <summary>
    /// Raised while a node is dragged and the pointer moves within the node it is already over. The move that
    /// brings the pointer onto a node raises <see cref="ItemDragEnter" /> instead.
    /// </summary>
    public event EventHandler<TreeViewDragOverEventArgs>? ItemDragOver;
    /// <summary>
    /// Raised after the dragged node was inserted at its drop target. It is not raised when the target is the
    /// dragged node or one of its descendants, nor when the tree is filled from an items source and the data did
    /// not accept the move. <see cref="ItemDragCompleted" /> follows.
    /// </summary>
    public event EventHandler<TreeViewDroppedEventArgs>? ItemDropped;
    /// <summary>
    /// Raised after a node of this tree was expanded.
    /// </summary>
    public event EventHandler<TreeItemExpandedEventArgs>? ItemExpanded;
    /// <summary>
    /// Raised after a node of this tree was collapsed.
    /// </summary>
    public event EventHandler<TreeItemCollapsedEventArgs>? ItemCollapsed;
    /// <summary>
    /// Raised after a node of this tree was clicked and <see cref="NotifyTreeItemClicked" /> was called for it.
    /// </summary>
    public event EventHandler<TreeItemClickedEventArgs>? ItemClicked;
    /// <summary>
    /// Raised when a context menu was requested for a node of this tree. The tree has then already closed the
    /// context menus that were open and, while <see cref="IsSelectOnRightClick" /> is <see langword="true" />,
    /// selected the node.
    /// </summary>
    public event EventHandler<TreeItemContextMenuEventArgs>? ItemContextMenuRequest;
    
    #endregion

    #region 内部属性定义
    
    internal static readonly StyledProperty<TimeSpan> MotionDurationProperty =
        MotionAwareControlProperty.MotionDurationProperty.AddOwner<TreeView>();
    
    internal static readonly DirectProperty<TreeView, bool> IsEffectiveEmptyVisibleProperty =
        AvaloniaProperty.RegisterDirect<TreeView, bool>(
            nameof(IsEffectiveEmptyVisible),
            o => o.IsEffectiveEmptyVisible,
            (o, v) => o.IsEffectiveEmptyVisible = v);

    internal static readonly DirectProperty<TreeView, bool> IsDefaultEmptyIndicatorVisibleProperty =
        AvaloniaProperty.RegisterDirect<TreeView, bool>(
            nameof(IsDefaultEmptyIndicatorVisible),
            o => o.IsDefaultEmptyIndicatorVisible,
            (o, v) => o.IsDefaultEmptyIndicatorVisible = v);

    internal static readonly StyledProperty<bool> IsPopupPinnedOpenProperty =
        Flyout.IsPopupPinnedOpenProperty.AddOwner<TreeView>();
    
    internal TimeSpan MotionDuration
    {
        get => GetValue(MotionDurationProperty);
        set => SetValue(MotionDurationProperty, value);
    }
    
    private bool _isEffectiveEmptyVisible = false;
    internal bool IsEffectiveEmptyVisible
    {
        get => _isEffectiveEmptyVisible;
        set => SetAndRaise(IsEffectiveEmptyVisibleProperty, ref _isEffectiveEmptyVisible, value);
    }

    private bool _isDefaultEmptyIndicatorVisible;
    internal bool IsDefaultEmptyIndicatorVisible
    {
        get => _isDefaultEmptyIndicatorVisible;
        set => SetAndRaise(IsDefaultEmptyIndicatorVisibleProperty, ref _isDefaultEmptyIndicatorVisible, value);
    }

    internal bool IsPopupPinnedOpen
    {
        get => GetValue(IsPopupPinnedOpenProperty);
        set => SetCurrentValue(IsPopupPinnedOpenProperty, value);
    }
    
    /// <summary>
    /// Gets the handler that implements the pointer and keyboard interaction of this tree when explicitly supplied.
    /// It is attached when the tree is attached to the visual tree and detached when the tree is detached.
    /// </summary>
    protected internal ITreeViewInteractionHandler? InteractionHandler { get; }
    
    #endregion
    
    private static readonly IList Empty = Array.Empty<object>();
    private IList? _checkedItems;
    private IList? _syncingSelectedItemsTarget;
    private readonly TreeDataController _treeDataController;
    private bool _isTreeDataControllerAttached;
    internal bool SyncingSelectedItems;
    internal bool SyncingCheckedItems;
    
    internal bool IsExpandAllProcess { get; set; }
    internal bool IsCollapseAllProcess { get; set; }

    static TreeView()
    {
        ConfigureDragAndDrop();
        TreeViewItem.ExpandedEvent.AddClassHandler<TreeView>((treeView, args) => treeView.HandleTreeItemExpanded(args));
        TreeViewItem.CollapsedEvent.AddClassHandler<TreeView>((treeView, args) => treeView.HandleTreeItemCollapsed(args));
        TreeViewItem.ContextMenuRequestEvent.AddClassHandler<TreeView>((treeView, args) => treeView.HandleTreeItemContextMenuRequest(args));
        TreeViewItem.ClickEvent.AddClassHandler<TreeView>((treeView, args) => treeView.HandleTreeItemClicked(args));
        RequestBringIntoViewEvent.AddClassHandler<TreeView>((treeView, args) => treeView.HandleDescendantRequestBringIntoView(args));
        Avalonia.Controls.Primitives.SelectingItemsControl.SelectionChangedEvent.AddClassHandler<TreeView>(
            (treeView, args) => treeView.HandleSelectionChangedDuringKeyboardRangeSync(args));
        Avalonia.Controls.Primitives.SelectingItemsControl.SelectionChangedEvent.AddClassHandler<TreeView>(
            (treeView, args) => treeView.HandleSelectionChangedDuringItemsSourceReplay(args));
        ConfigureFilter();

        SelectedItemProperty.Changed.AddClassHandler<TreeView>((treeView, args) =>
        {
            if (!treeView.IsItemsSourceReplayPending)
            {
                treeView.NotifyFormValueChanged(args.NewValue);
            }
        });
        SelectedItemsProperty.Changed.AddClassHandler<TreeView>((treeView, args) =>
        {
            if (!treeView.IsItemsSourceReplayPending)
            {
                treeView.NotifyFormValueChanged(args.NewValue);
            }
        });
    }

    public TreeView()
    {
        _treeDataController = new TreeDataController(this);
        Items.CollectionChanged += HandleCollectionChanged;
    }

    /// <summary>
    /// Creates a tree whose interaction is implemented by <paramref name="interactionHandler" />. Derived
    /// trees use this constructor to supply their own handler; the parameterless constructor resolves the active
    /// design language behavior instead.
    /// </summary>
    /// <param name="interactionHandler">
    /// The handler that receives the pointer and keyboard interaction of the tree. It may not be
    /// <see langword="null" />.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="interactionHandler" /> is <see langword="null" />.
    /// </exception>
    protected TreeView(ITreeViewInteractionHandler interactionHandler) : this()
    {
        InteractionHandler = interactionHandler ?? throw new ArgumentNullException(nameof(interactionHandler));
    }

    internal bool ShouldPreserveSelectedContainerDuringSelectedItemsSync(TreeViewItem treeViewItem)
    {
        if (!SyncingSelectedItems ||
            _syncingSelectedItemsTarget is null)
        {
            return false;
        }

        var item = TreeItemFromContainer(treeViewItem);
        return item != null && _syncingSelectedItemsTarget.Contains(item);
    }

    private void HandleDescendantRequestBringIntoView(RequestBringIntoViewEventArgs args)
    {
        if (!ReferenceEquals(args.Source, this))
        {
            args.Handled = true;
        }
    }

    /// <summary>
    /// Sets <see cref="Filter" /> to a <c>ValueFilterMode.Contains</c> filter and
    /// <see cref="FilterValueSelector" /> to the default selector when they are still <see langword="null" />, and
    /// updates the empty indicator. The base method is called first.
    /// </summary>
    protected override void OnInitialized()
    {
        base.OnInitialized();
        Filter ??= ValueFilterFactory.BuildFilter(ValueFilterMode.Contains);
        FilterValueSelector ??= DefaultTreeFilterValueSelector;
        ConfigureEmptyIndicator();
    }

    internal static readonly DefaultFilterValueSelector DefaultTreeFilterValueSelector = value =>
    {
        if (value is TreeViewItem treeViewItem)
        {
            if (treeViewItem.Header is ITreeItemNode treeItemData)
            {
                return treeItemData.Header?.ToString();
            }
            if (treeViewItem.Header is string header)
            {
                return header;
            }
            return treeViewItem.Header?.ToString();
        }
        return null;
    };
    
    private void HandleCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        ConfigureEmptyIndicator();
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Remove:
            case NotifyCollectionChangedAction.Replace:
                var oldItems = e.OldItems!;
                for (var i = 0; i < oldItems.Count; i++)
                {
                    RemoveCheckedItemOfRemovedNode(oldItems[i]);
                }
                break;
            case NotifyCollectionChangedAction.Reset:
                ClearCheckedItemsOfResetRoots();
                break;
        }
    }
    
    /// <summary>
    /// Expands every node of the tree.
    /// </summary>
    /// <param name="motionEnabled">
    /// Overrides <see cref="IsMotionEnabled" /> for this call: <see langword="true" /> plays the open motion,
    /// <see langword="false" /> expands without motion and <see langword="null" /> keeps the current value.
    /// </param>
    public void ExpandAll(bool? motionEnabled = null)
    {
        var originMotionEnabled = IsMotionEnabled;
        try
        {
            IsExpandAllProcess = true;
            if (motionEnabled.HasValue)
            {
                SetCurrentValue(IsMotionEnabledProperty, motionEnabled.Value);
            }

            foreach (var item in Items)
            {
                if (item != null)
                {
                    if (TreeContainerFromItem(item) is TreeViewItem treeItem)
                    {
                        ExpandSubTree(treeItem);
                    }
                }
            }
        }
        finally
        {
            IsExpandAllProcess = false;
            if (motionEnabled.HasValue)
            {
                SetCurrentValue(IsMotionEnabledProperty, originMotionEnabled);
            }
        }
    }

    /// <summary>
    /// Collapses every node of the tree.
    /// </summary>
    /// <param name="motionEnabled">
    /// Overrides <see cref="IsMotionEnabled" /> for this call: <see langword="true" /> plays the close motion,
    /// <see langword="false" /> collapses without motion and <see langword="null" /> keeps the current value.
    /// </param>
    public void CollapseAll(bool? motionEnabled = null)
    {
        var originMotionEnabled = IsMotionEnabled;
        try
        {
            IsCollapseAllProcess = true;
            if (motionEnabled.HasValue)
            {
                SetCurrentValue(IsMotionEnabledProperty, motionEnabled.Value);
            }

            for (var i = 0; i < ItemCount; i++)
            {
                if (ContainerFromIndex(i) is TreeViewItem treeItem)
                {
                    CollapseSubTree(treeItem);
                }
            }
        }
        finally
        {
            IsCollapseAllProcess = false;
            if (motionEnabled.HasValue)
            {
                SetCurrentValue(IsMotionEnabledProperty, originMotionEnabled);
            }
        }
    }

    /// <summary>
    /// Checks the node with all of its descendants, when the node can be checked. Unless
    /// <see cref="IsCheckStrictly" /> is <see langword="true" />, the check state of its ancestors is then updated
    /// from their children.
    /// </summary>
    /// <param name="viewItem">
    /// The container of the node to check.
    /// </param>
    public void CheckedSubTree(TreeViewItem viewItem)
    {
        ApplyCheckedSubTree(viewItem);
    }

    /// <summary>
    /// Decides whether a node whose container is realized is checked when a recursive check reaches it, for example
    /// from <see cref="CheckedSubTree" />. The base implementation always returns <see langword="true" />, so an
    /// override does not need to call it.
    /// </summary>
    /// <param name="treeViewItem">
    /// The container of the node that the recursive check is about to check.
    /// </param>
    /// <returns>
    /// <see langword="true" /> to check the node, <see langword="false" /> to leave its check state unchanged.
    /// </returns>
    protected virtual bool RecursiveCheckNodePredicate(TreeViewItem treeViewItem)
    {
        return true;
    }
    
    /// <summary>
    /// Decides whether a node whose container is realized is unchecked when a recursive uncheck reaches it, for
    /// example from <see cref="UnCheckedSubTree" />. The base implementation always returns <see langword="true" />,
    /// so an override does not need to call it.
    /// </summary>
    /// <param name="treeViewItem">
    /// The container of the node that the recursive uncheck is about to uncheck.
    /// </param>
    /// <returns>
    /// <see langword="true" /> to uncheck the node, <see langword="false" /> to leave its check state unchanged.
    /// </returns>
    protected virtual bool RecursiveUnCheckNodePredicate(TreeViewItem treeViewItem)
    {
        return true;
    }

    /// <summary>
    /// Unchecks the node with all of its descendants, when the node can be checked. Unless
    /// <see cref="IsCheckStrictly" /> is <see langword="true" />, the check state of its ancestors is then updated
    /// from their children.
    /// </summary>
    /// <param name="viewItem">
    /// The container of the node to uncheck.
    /// </param>
    public void UnCheckedSubTree(TreeViewItem viewItem)
    {
        ApplyUnCheckedSubTree(viewItem);
    }

    /// <summary>
    /// Unchecks the node with all of its descendants and updates its ancestors like
    /// <see cref="UnCheckedSubTree" /> does, and reports the result for the subtree.
    /// </summary>
    /// <param name="treeViewItem">
    /// The container of the node to uncheck.
    /// </param>
    /// <returns>
    /// The items of the subtree that are not checked after the update.
    /// </returns>
    public ISet<object> DoUnCheckedSubTree(TreeViewItem treeViewItem)
    {
        return CollectUnCheckedSubTreeItems(treeViewItem);
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(StdPseudoClass.Draggable, IsDraggable);
    }

    /// <summary>
    /// Creates a <see cref="TreeViewItem" /> as the container for an item of the tree.
    /// </summary>
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        var treeViewItem = new TreeViewItem();
        treeViewItem.Classes.Add(TreeViewSemanticParts.ItemClass);
        return treeViewItem;
    }

    /// <summary>
    /// Decides whether an item needs a container: an item that is already a <see cref="TreeViewItem" /> is its own
    /// container and needs none, any other item does.
    /// </summary>
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        return NeedsContainer<TreeViewItem>(item, out recycleKey);
    }

    /// <summary>
    /// Prepares a <see cref="TreeViewItem" /> container for its item: makes this tree its owner, forwards the
    /// header template, the switcher icons and the tree-wide node settings to it, calls
    /// <see cref="PrepareTreeViewItem" /> and then reports the container as prepared. The base method is called
    /// first.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="container" /> is not a <see cref="TreeViewItem" />.
    /// </exception>
    protected override void ContainerForItemPreparedOverride(Control container, object? item, int index)
    {
        base.ContainerForItemPreparedOverride(container, item, index);
        if (container is TreeViewItem treeViewItem)
        {
            treeViewItem.Classes.Add(TreeViewSemanticParts.ItemClass);
            treeViewItem.OwnerTreeView = this;
            
            if (item != null && item is not Visual && item is ITreeItemNode treeViewItemData)
            {
                treeViewItem.PrepareTreeItemNodeData(treeViewItemData, this);
            }
            
            if (ItemTemplate != null)
            {
                treeViewItem[!TreeViewItem.HeaderTemplateProperty] = this[!ItemTemplateProperty];
            }
            
            SetTreeViewItemIcon(treeViewItem, TreeViewItem.SwitcherExpandIconProperty, SwitcherExpandIcon);
            SetTreeViewItemIcon(treeViewItem, TreeViewItem.SwitcherCollapseIconProperty, SwitcherCollapseIcon);
            SetTreeViewItemIcon(treeViewItem, TreeViewItem.SwitcherRotationIconProperty, SwitcherRotationIcon);
            SetTreeViewItemIcon(treeViewItem, TreeViewItem.SwitcherLoadingIconProperty, SwitcherLoadingIcon);
            SetTreeViewItemIcon(treeViewItem, TreeViewItem.SwitcherLeafIconProperty, SwitcherLeafIcon);
            
            treeViewItem[!TreeViewItem.FilterStrategyProperty]           = this[!FilterStrategyProperty];
            treeViewItem[!TreeViewItem.IsMotionEnabledProperty]           = this[!IsMotionEnabledProperty];
            treeViewItem[!TreeViewItem.NodeHoverModeProperty]             = this[!NodeHoverModeProperty];
            treeViewItem[!TreeViewItem.IsShowLineProperty]                = this[!IsShowLineProperty];
            treeViewItem[!TreeViewItem.IsShowIconProperty]                = this[!IsShowIconProperty];
            treeViewItem[!TreeViewItem.IsShowLeafIconProperty]            = this[!IsShowLeafIconProperty];
            treeViewItem[!TreeViewItem.IsSwitcherRotationProperty]        = this[!IsSwitcherRotationProperty];
            treeViewItem[!TreeViewItem.ToggleTypeProperty]                = this[!ToggleTypeProperty];
            treeViewItem[!TreeViewItem.IsSelectableProperty]              = this[!IsSelectableProperty];
            treeViewItem[!TreeViewItem.FilterHighlightForegroundProperty] = this[!FilterHighlightForegroundProperty];
            treeViewItem[!TreeViewItem.HasTreeItemDataLoaderProperty]     = this[!HasTreeItemDataLoaderProperty];
            treeViewItem[!TreeViewItem.IsAutoExpandParentProperty]        = this[!IsAutoExpandParentProperty];
            
            PrepareTreeViewItem(treeViewItem, item, index);
            NotifyTreeViewItemPrepared(treeViewItem, item);
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(container), "The container type is incorrect, it must be type TreeItem.");
        }
    }

    private void SetTreeViewItemIcon(TreeViewItem treeViewItem, AvaloniaProperty iconProperty, IIconTemplate? iconTemplate)
    {
        if (iconTemplate == null)
        {
            treeViewItem.SetValue(iconProperty, null);
        }
        else
        {
            treeViewItem.SetValue(iconProperty, iconTemplate.Build());
        }
    }
    
    /// <summary>
    /// Called when a <see cref="TreeViewItem" /> container has been prepared for its item and has received the
    /// settings of the tree, before the tree is told that the container is prepared. The base method does nothing,
    /// so an override does not need to call it.
    /// </summary>
    /// <param name="treeViewItem">
    /// The container that presents the item.
    /// </param>
    /// <param name="item">
    /// The item that the container presents.
    /// </param>
    /// <param name="index">
    /// The index of the item among the items of the tree.
    /// </param>
    protected virtual void PrepareTreeViewItem(TreeViewItem treeViewItem, object? item, int index)
    {
    }

    /// <summary>
    /// Prepares a container for its item while the container lifecycle of the node is open, so that the node does
    /// not treat the changes of the prepare as changes made from outside. The base method is called for every
    /// container.
    /// </summary>
    // Avalonia's TreeViewItem forwards the prepare of nested containers here, so this covers every level.
    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        if (container is TreeViewItem treeViewItem)
        {
            using (treeViewItem.BeginContainerLifecycle())
            {
                base.PrepareContainerForItemOverride(container, item, index);
            }
            return;
        }

        base.PrepareContainerForItemOverride(container, item, index);
    }

    /// <summary>
    /// Clears a <see cref="TreeViewItem" /> container that is recycled or released: drops the node data it was
    /// prepared with, and releases the binding that a header template created for its children. The base method
    /// is called for every container.
    /// </summary>
    protected override void ClearContainerForItemOverride(Control container)
    {
        if (container is TreeViewItem treeViewItem)
        {
            using (treeViewItem.BeginContainerLifecycle())
            {
                var shouldReleaseTreeDataTemplateBinding = treeViewItem.Header is BindableTreeItemNode;
                treeViewItem.ClearPreparedTreeItemNodeData();
                base.ClearContainerForItemOverride(container);
                if (shouldReleaseTreeDataTemplateBinding)
                {
                    // HeaderedItemsControl keeps TreeDataTemplate children binding in an internal disposable.
                    // Preparing once with a null item lets Avalonia release that binding before recycling.
                    base.PrepareContainerForItemOverride(container, null, -1);
                    base.ClearContainerForItemOverride(container);
                }
            }
            return;
        }

        base.ClearContainerForItemOverride(container);
    }

    /// <summary>
    /// Calls the base method, then connects the tree to its data controller and its interaction handler, starts
    /// observing the selected items, updates the <c>:draggable</c> pseudo-class and, when default selected or
    /// checked paths are set, schedules them to be applied after the first layout.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _isTreeDataControllerAttached = true;
        _treeDataController.SetRootSource(ItemsSource);
        AttachInteraction();
        if (InteractionHandler is null)
        {
            PointerPressed += HandleTreePointerPressed;
            PointerReleased += HandleTreePointerReleased;
        }
        ObserveSelectedItems();
        UpdatePseudoClasses();
        if (DefaultSelectedPaths is { Count: > 0 } || DefaultCheckedPaths is { Count: > 0 })
        {
            LayoutUpdated += ReplayInitialNodeState;
        }
    }
    
    /// <summary>
    /// Ends a drag in progress and cancels the pending node state replays, calls the base method, and then
    /// detaches the interaction handler, stops observing the selected items, cancels every pending asynchronous
    /// node load and disconnects the tree from its data controller.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        FinishDrag();
        ClearInitialNodeReplay();
        base.OnDetachedFromVisualTree(e);
        ClearKeyboardActiveItem();
        CancelItemsSourceReplay();
        if (InteractionHandler is null)
        {
            PointerPressed -= HandleTreePointerPressed;
            PointerReleased -= HandleTreePointerReleased;
        }
        DetachInteraction();
        StopObservingSelectedItems();
        
        // Cancel every pending asynchronous node load.
        _asyncLoadCoordinator.CancelAll();
        _treeDataController.ClearRootSource();
        _isTreeDataControllerAttached = false;
    }

    /// <summary>
    /// Calls the base method, then applies the default node state that is still pending, runs the filter and
    /// expands every node when <see cref="IsDefaultExpandAll" /> is <see langword="true" />.
    /// </summary>
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        ReplayLoadedState();
    }

    /// <summary>
    /// Calls the base method and then reacts to the change. A change of <see cref="ItemsControl.ItemsSource" />
    /// opens an items source replay around both calls; when one of them throws, the replay is canceled and the
    /// exception is rethrown.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        var isItemsSourceChange = change.Property == ItemsSourceProperty;
        var replayRevision = isItemsSourceChange ? BeginItemsSourceReplay() : 0;
        if (isItemsSourceChange)
        {
            _itemsSourceReplayInternalDepth++;
        }
        try
        {
            base.OnPropertyChanged(change);
            HandlePropertyChangedCore(change, replayRevision);
        }
        catch
        {
            if (isItemsSourceChange)
            {
                CancelItemsSourceReplay();
            }
            throw;
        }
        finally
        {
            if (isItemsSourceChange)
            {
                _itemsSourceReplayInternalDepth--;
            }
        }
    }

    internal ITreeItemNode? ResolveTreeItemNode(TreeViewItem treeViewItem)
    {
        return TreeItemFromContainer(treeViewItem) as ITreeItemNode ??
               treeViewItem.Header as ITreeItemNode ??
               treeViewItem;
    }

    internal bool ShouldPreserveRemovedTreeItem(object? item)
    {
        return item is ITreeItemNode node &&
               _treeDataController.IsMovingNode(node);
    }

    /// <summary>
    /// Applies the current <see cref="SwitcherRotationIcon" /> to the realized top-level node containers. Nodes
    /// further down receive it when their containers are prepared.
    /// </summary>
    protected void HandleSwitcherRotationIconChanged()
    {
        for (var i = 0; i < ItemCount; i++)
        {
            var container = ContainerFromIndex(i);
            if (container is TreeViewItem treeViewItem)
            {
                SetTreeViewItemIcon(treeViewItem, TreeViewItem.SwitcherRotationIconProperty, SwitcherRotationIcon);
            }
        }
    }
    
    /// <summary>
    /// Applies the current <see cref="SwitcherExpandIcon" /> to the realized top-level node containers. Nodes
    /// further down receive it when their containers are prepared.
    /// </summary>
    protected void HandleSwitcherExpandIconChanged()
    {
        for (var i = 0; i < ItemCount; i++)
        {
            var container = ContainerFromIndex(i);
            if (container is TreeViewItem treeViewItem)
            {
                SetTreeViewItemIcon(treeViewItem, TreeViewItem.SwitcherExpandIconProperty, SwitcherExpandIcon);
            }
        }
    }
    
    /// <summary>
    /// Applies the current <see cref="SwitcherCollapseIcon" /> to the realized top-level node containers. Nodes
    /// further down receive it when their containers are prepared.
    /// </summary>
    protected void HandleSwitcherCollapseIconChanged()
    {
        for (var i = 0; i < ItemCount; i++)
        {
            var container = ContainerFromIndex(i);
            if (container is TreeViewItem treeViewItem)
            {
                SetTreeViewItemIcon(treeViewItem, TreeViewItem.SwitcherCollapseIconProperty, SwitcherCollapseIcon);
            }
        }
    }
    
    /// <summary>
    /// Applies the current <see cref="SwitcherLoadingIcon" /> to the realized top-level node containers. Nodes
    /// further down receive it when their containers are prepared.
    /// </summary>
    protected void HandleSwitcherLoadingIconChanged()
    {
        for (var i = 0; i < ItemCount; i++)
        {
            var container = ContainerFromIndex(i);
            if (container is TreeViewItem treeViewItem)
            {
                SetTreeViewItemIcon(treeViewItem, TreeViewItem.SwitcherLoadingIconProperty, SwitcherLoadingIcon);
            }
        }
    }
    
    /// <summary>
    /// Applies the current <see cref="SwitcherLeafIcon" /> to the realized top-level node containers. Nodes
    /// further down receive it when their containers are prepared.
    /// </summary>
    protected void HandleSwitcherLeafIconChanged()
    {
        for (var i = 0; i < ItemCount; i++)
        {
            var container = ContainerFromIndex(i);
            if (container is TreeViewItem treeViewItem)
            {
                SetTreeViewItemIcon(treeViewItem, TreeViewItem.SwitcherLeafIconProperty, SwitcherLeafIcon);
            }
        }
    }

    private void HandleTreeItemExpanded(RoutedEventArgs args)
    {
        if (args.Source is TreeViewItem item)
        {
            ItemExpanded?.Invoke(this, new TreeItemExpandedEventArgs(item));
        }
    }
    
    private void HandleTreeItemCollapsed(RoutedEventArgs args)
    {
        if (args.Source is TreeViewItem item)
        {
            ItemCollapsed?.Invoke(this, new TreeItemCollapsedEventArgs(item));
        }
    }

    private void HandleTreeItemContextMenuRequest(RoutedEventArgs args)
    {
        if (args.Source is TreeViewItem item)
        {
            ItemContextMenuRequest?.Invoke(this, new TreeItemContextMenuEventArgs(item));
        }
    }
    
    private void HandleTreeItemClicked(RoutedEventArgs args)
    {
        if (args.Source is TreeViewItem item)
        {
            NotifyTreeItemClicked(item);
            ItemClicked?.Invoke(this, new TreeItemClickedEventArgs(item));
        }
    }

    /// <summary>
    /// Called for a node of this tree that was clicked, before <see cref="ItemClicked" /> is raised. The base
    /// method does nothing, so an override does not need to call it.
    /// </summary>
    /// <param name="viewItem">
    /// The container of the node that was clicked.
    /// </param>
    protected virtual void NotifyTreeItemClicked(TreeViewItem viewItem)
    {
    }
    
    /// <summary>
    /// Recomputes whether the tree is to be shown as empty and updates the states that drive the empty indicator.
    /// The tree counts as empty while <see cref="IsShowEmptyIndicator" /> is <see langword="true" /> and either
    /// the filter mode shows no match or the tree has no items; the built-in indicator shows only while
    /// <see cref="EmptyIndicator" /> and <see cref="EmptyIndicatorTemplate" /> are both <see langword="null" />.
    /// </summary>
    protected virtual void ConfigureEmptyIndicator()
    {
        var isEmpty = false;
        if (IsFilterMode)
        {
            isEmpty = FilterResultCount == 0;
        }
        else
        {
            if (ItemsSource != null)
            {
                var enumerator = ItemsSource.GetEnumerator();
                isEmpty = !enumerator.MoveNext();
                if (enumerator is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
            else
            {
                isEmpty = Items.Count == 0;
            }
        }
        IsEffectiveEmptyVisible        = IsShowEmptyIndicator && isEmpty;
        IsDefaultEmptyIndicatorVisible = IsEffectiveEmptyVisible && EmptyIndicator is null && EmptyIndicatorTemplate is null;
    }
    
    /// <summary>
    /// Handles a press on a node: notifies the node of the press, updates the selection from the event while
    /// <see cref="IsSelectable" /> is <see langword="true" />, and, for a left button press while
    /// <see cref="IsDraggable" /> is <see langword="true" />, records the press position and blocks gesture
    /// recognition so that a drag can start. The base method is not called.
    /// </summary>
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        var container = GetTreeViewItemCore(e.Source as Control);
        if (container is not null && e.Source is Visual source)
        {
            var point = e.GetCurrentPoint(source);
            if (point.Properties.IsLeftButtonPressed || point.Properties.IsRightButtonPressed)
            {
                NotifyTreeViewItemPointerPressed(container);
                if (IsSelectable)
                {
                    UpdateSelectionFromEvent(container, e);
                }
            }

            if (point.Properties.IsLeftButtonPressed && IsDraggable)
            {
                _lastPoint = e.GetPosition(this);
                e.PreventGestureRecognition();
            }
        }
    }

    /// <summary>
    /// Updates the selection from an input event that hit <paramref name="container" /> as the base method does,
    /// except for a right button event while <see cref="IsSelectOnRightClick" /> is <see langword="false" />: that
    /// event is ignored.
    /// </summary>
    /// <param name="container">
    /// The node container that the event hit.
    /// </param>
    /// <param name="eventArgs">
    /// The input event that the selection is updated from.
    /// </param>
    /// <returns>
    /// The result of the base method, or <see langword="false" /> when the event was ignored.
    /// </returns>
    public override bool UpdateSelectionFromEvent(Control container, RoutedEventArgs eventArgs)
    {
        if (!IsSelectOnRightClick &&
            eventArgs is PointerEventArgs pointerEvent &&
            pointerEvent.GetCurrentPoint(container).Properties.PointerUpdateKind is
                PointerUpdateKind.RightButtonPressed or PointerUpdateKind.RightButtonReleased)
        {
            return false;
        }

        return base.UpdateSelectionFromEvent(container, eventArgs);
    }

    #region 实现 FormItem 接口
    
    private EventHandler? _formValueChanged;
    event EventHandler? IFormItemAware.ValueChanged
    {
        add => _formValueChanged += value;
        remove => _formValueChanged -= value;
    }

    void IFormItemAware.SetFormValue(object? value) => NotifySetFormValue(value);

    object? IFormItemAware.GetFormValue() => NotifyGetFormValue();
    void IFormItemAware.ClearFormValue() => NotifyClearFormValue();
    void IFormItemAware.NotifyValidateStatus(FormValidateStatus status) => NotifyValidateStatus(status);
    
    /// <summary>
    /// Called after the single selected item or <see cref="SelectedItems" /> changed, outside of an items source
    /// replay. The base method raises the value changed notification of the form item.
    /// </summary>
    /// <param name="value">
    /// The new value of the changed selection property; the base method ignores it.
    /// </param>
    protected virtual void NotifyFormValueChanged(object? value)
    {
        _formValueChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Applies a value set by a form item to the selection. While <c>SelectionMode.Multiple</c> is selected,
    /// <paramref name="value" /> is assigned to <see cref="SelectedItems" /> when it is an <see cref="IList" />,
    /// and clears the selection otherwise; in the other modes it becomes the single selected item.
    /// </summary>
    /// <param name="value">
    /// The form value: a list of nodes for multiple selection, or one node.
    /// </param>
    protected virtual void NotifySetFormValue(object? value)
    {
        if ((SelectionMode & SelectionMode.Multiple) == SelectionMode.Multiple)
        {
            SelectedItems = value as IList;
        }
        else
        {
            SelectedItem = value;
        }
    }

    /// <summary>
    /// Returns the selection as the value of the form item: <see cref="SelectedItems" /> while
    /// <c>SelectionMode.Multiple</c> is selected, otherwise the single selected item.
    /// </summary>
    /// <returns>
    /// The selected nodes, as a list or as a single item.
    /// </returns>
    protected virtual object? NotifyGetFormValue()
    {
        if ((SelectionMode & SelectionMode.Multiple) == SelectionMode.Multiple)
        {
            return SelectedItems;
        }
        return SelectedItem;
    }

    /// <summary>
    /// Clears the selection for a form item: <see cref="SelectedItems" /> is set to <see langword="null" />
    /// while <c>SelectionMode.Multiple</c> is selected, otherwise the single selected item is set to
    /// <see langword="null" />.
    /// </summary>
    protected virtual void NotifyClearFormValue()
    {
        if ((SelectionMode & SelectionMode.Multiple) == SelectionMode.Multiple)
        {
            SelectedItems = null;
        }
        else
        {
            SelectedItem = null;
        }
    }

    /// <summary>
    /// Called when a form item reports its validation status. The base method does nothing, so an override does
    /// not need to call it.
    /// </summary>
    /// <param name="status">
    /// The validation status of the form item that owns this tree.
    /// </param>
    protected virtual void NotifyValidateStatus(FormValidateStatus status)
    {
    }
    #endregion

    private void HandlePropertyChangedCore(AvaloniaPropertyChangedEventArgs change, int replayRevision)
    {
        if (change.Property == DefaultSelectedPathsProperty)
        {
            ConfigureDefaultSelectedPaths();
        }
        else if (change.Property == SwitcherRotationIconProperty)
        {
            HandleSwitcherRotationIconChanged();
        }
        else if (change.Property == SwitcherExpandIconProperty)
        {
            HandleSwitcherExpandIconChanged();
        }
        else if (change.Property == SwitcherCollapseIconProperty)
        {
            HandleSwitcherCollapseIconChanged();
        }
        else if (change.Property == SwitcherLoadingIconProperty)
        {
            HandleSwitcherLoadingIconChanged();
        }
        else if (change.Property == SwitcherLeafIconProperty)
        {
            HandleSwitcherLeafIconChanged();
        }
        else if (change.Property == DataLoaderProperty)
        {
            HasTreeItemDataLoader = DataLoader != null;
        }
        else if (change.Property == FilterProperty ||
                 change.Property == FilterStrategyProperty ||
                 change.Property == ItemsSourceProperty ||
                 change.Property == FilterValueProperty)
        {
            FilterTreeNode();
        }

        if (change.Property == IsShowEmptyIndicatorProperty ||
            change.Property == ItemsSourceProperty ||
            change.Property == FilterResultCountProperty ||
            change.Property == IsFilterModeProperty ||
            change.Property == EmptyIndicatorProperty ||
            change.Property == EmptyIndicatorTemplateProperty)
        {
            ConfigureEmptyIndicator();
        }
        else if (change.Property == IsSelectableProperty)
        {
            if (!IsSelectable)
            {
                SetCurrentValue(SelectedItemProperty, null);
                SelectedItems.Clear();
            }
        }

        if (change.Property == ItemsSourceProperty)
        {
            if (_isTreeDataControllerAttached)
            {
                _treeDataController.SetRootSource(ItemsSource);
            }
            Dispatcher.Post(() => CompleteItemsSourceReplay(replayRevision));
        }
        else if (change.Property == SelectedItemProperty || change.Property == SelectedItemsProperty)
        {
            RecordExplicitSelectionUpdateDuringReplay(true);
        }

        if (change.Property == SelectedItemsProperty && _isTreeDataControllerAttached)
        {
            ObserveSelectedItems();
        }

    }
}
