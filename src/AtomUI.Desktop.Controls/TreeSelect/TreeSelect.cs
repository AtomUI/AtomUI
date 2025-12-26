using AtomUI.Desktop.Controls.Primitives;
using AtomUI.Theme;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

public class TreeSelect : AbstractSelect,
                          IControlSharedTokenResourcesHost
{
    #region 公共属性定义

    public static readonly StyledProperty<string> TreeNodeFilterPropProperty =
        AvaloniaProperty.Register<TreeSelect, string>(
            nameof(TreeNodeFilterProp), "ItemKey");
    
    public static readonly StyledProperty<TreeSelectCheckedStrategy> ShowCheckedStrategyProperty =
        AvaloniaProperty.Register<TreeSelect, TreeSelectCheckedStrategy>(
            nameof(ShowCheckedStrategy), TreeSelectCheckedStrategy.ShowChild);
    
    public static readonly StyledProperty<bool> IsTreeCheckableProperty =
        AvaloniaProperty.Register<TreeSelect, bool>(
            nameof(IsTreeCheckable));
    
    public static readonly StyledProperty<bool> IsTreeDefaultExpandAllProperty =
        AvaloniaProperty.Register<TreeSelect, bool>(
            nameof(IsTreeDefaultExpandAll));
    
    public static readonly DirectProperty<TreeSelect, IList<TreeNodePath>?> TreeDefaultExpandedPathsProperty =
        AvaloniaProperty.RegisterDirect<TreeSelect, IList<TreeNodePath>?>(
            nameof(TreeDefaultExpandedPaths),
            o => o.TreeDefaultExpandedPaths,
            (o, v) => o.TreeDefaultExpandedPaths = v);

    public string TreeNodeFilterProp
    {
        get => GetValue(TreeNodeFilterPropProperty);
        set => SetValue(TreeNodeFilterPropProperty, value);
    }
    
    public TreeSelectCheckedStrategy ShowCheckedStrategy
    {
        get => GetValue(ShowCheckedStrategyProperty);
        set => SetValue(ShowCheckedStrategyProperty, value);
    }
    
    public bool IsTreeCheckable
    {
        get => GetValue(IsTreeCheckableProperty);
        set => SetValue(IsTreeCheckableProperty, value);
    }
    
    public bool IsTreeDefaultExpandAll
    {
        get => GetValue(IsTreeDefaultExpandAllProperty);
        set => SetValue(IsTreeDefaultExpandAllProperty, value);
    }
    
    private IList<TreeNodePath>? _treeDefaultExpandedPaths;
    
    public IList<TreeNodePath>? TreeDefaultExpandedPaths
    {
        get => _treeDefaultExpandedPaths;
        set => SetAndRaise(TreeDefaultExpandedPathsProperty, ref _treeDefaultExpandedPaths, value);
    }
    #endregion
    
    #region 内部属性定义

    Control IControlSharedTokenResourcesHost.HostControl => this;
    string IControlSharedTokenResourcesHost.TokenId => TreeSelectToken.ID;

    #endregion
    
    public TreeSelect()
    {
        this.RegisterResources();
    }
}