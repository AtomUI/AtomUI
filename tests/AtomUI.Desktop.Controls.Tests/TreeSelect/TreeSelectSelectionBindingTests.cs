using System.Collections.ObjectModel;
using System.ComponentModel;
using AtomUI.Controls;
using AtomUI.Controls.Primitives;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.TreeSelectControl;

public class TreeSelectSelectionBindingTests
{
    static TreeSelectSelectionBindingTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void SelectedValueProperties_Are_TwoWay_And_DataValidation_Enabled()
    {
        var selectedItemMetadata = Desktop.Controls.TreeSelect.SelectedItemProperty.GetMetadata(
            typeof(Desktop.Controls.TreeSelect));
        var selectedItemsMetadata = Desktop.Controls.TreeSelect.SelectedItemsProperty.GetMetadata(
            typeof(Desktop.Controls.TreeSelect));

        selectedItemMetadata.DefaultBindingMode.ShouldBe(BindingMode.TwoWay);
        selectedItemMetadata.EnableDataValidation.ShouldBe(true);
        selectedItemsMetadata.DefaultBindingMode.ShouldBe(BindingMode.TwoWay);
        selectedItemsMetadata.EnableDataValidation.ShouldBe(true);
    }

    [Fact]
    public void SelectedItem_DefaultBindingMode_Updates_ViewModel()
    {
        var firstNode = new TreeItemNode
        {
            Header = "First",
            Value  = "first"
        };
        var secondNode = new TreeItemNode
        {
            Header = "Second",
            Value  = "second"
        };
        var viewModel = new TreeSelectBindingViewModel
        {
            SelectedItem = firstNode
        };
        var treeSelect = new Desktop.Controls.TreeSelect
        {
            Width                 = 240,
            IsDefaultExpandAll    = true,
            IsMotionEnabled       = false,
            ItemsSource           = [firstNode, secondNode],
            PlaceholderText       = "Please select"
        };
        treeSelect.Bind(
            Desktop.Controls.TreeSelect.SelectedItemProperty,
            new Binding(nameof(TreeSelectBindingViewModel.SelectedItem))
            {
                Source = viewModel
            });

        ShowInWindow(treeSelect, () =>
        {
            treeSelect.SelectedItem.ShouldBeSameAs(firstNode);

            treeSelect.SelectedItem = secondNode;
            Dispatcher.UIThread.RunJobs();

            viewModel.SelectedItem.ShouldBeSameAs(secondNode);
        });
    }

    [Fact]
    public void SelectedItems_DefaultBindingMode_Updates_ViewModel()
    {
        var firstNode = new TreeItemNode
        {
            Header = "First",
            Value  = "first"
        };
        var secondNode = new TreeItemNode
        {
            Header = "Second",
            Value  = "second"
        };
        var viewModel = new TreeSelectBindingViewModel
        {
            SelectedItems = new List<ITreeItemNode> { firstNode }
        };
        var treeSelect = new Desktop.Controls.TreeSelect
        {
            Width                 = 240,
            IsDefaultExpandAll    = true,
            IsMotionEnabled       = false,
            IsMultiple            = true,
            ItemsSource           = [firstNode, secondNode],
            PlaceholderText       = "Please select"
        };
        treeSelect.Bind(
            Desktop.Controls.TreeSelect.SelectedItemsProperty,
            new Binding(nameof(TreeSelectBindingViewModel.SelectedItems))
            {
                Source = viewModel
            });

        ShowInWindow(treeSelect, () =>
        {
            treeSelect.SelectedItems.ShouldBeSameAs(viewModel.SelectedItems);

            treeSelect.SelectedItems = new List<ITreeItemNode> { firstNode, secondNode };
            Dispatcher.UIThread.RunJobs();

            viewModel.SelectedItems.ShouldNotBeNull();
            viewModel.SelectedItems.ShouldBe([firstNode, secondNode]);
        });
    }

    [Fact]
    public void SelectedItems_ObservableCollection_Mutation_Refreshes_Result_And_TreeView_Selection()
    {
        var firstNode = new TreeItemNode
        {
            Header = "First",
            Value  = "first"
        };
        var secondNode = new TreeItemNode
        {
            Header = "Second",
            Value  = "second"
        };
        var viewModel = new TreeSelectBindingViewModel
        {
            SelectedItems = new ObservableCollection<ITreeItemNode>
            {
                firstNode
            }
        };
        var treeSelect = new Desktop.Controls.TreeSelect
        {
            Width                   = 240,
            IsDefaultExpandAll      = true,
            IsMotionEnabled         = false,
            IsMultiple              = true,
            IsAllowClear            = true,
            IsShowMaxCountIndicator = true,
            MaxCount                = 5,
            ItemsSource             = [firstNode, secondNode],
            PlaceholderText         = "Please select"
        };
        treeSelect.Bind(
            Desktop.Controls.TreeSelect.SelectedItemsProperty,
            new Binding(nameof(TreeSelectBindingViewModel.SelectedItems))
            {
                Source = viewModel,
                Mode   = BindingMode.TwoWay
            });

        ShowInWindow(treeSelect, () =>
        {
            FindSelectTag(treeSelect, "First").ShouldNotBeNull();
            treeSelect.SelectedCount.ShouldBe(1);
            var formValueChangedCount = 0;
            ((IFormItemAware)treeSelect).ValueChanged += (_, _) => formValueChangedCount++;

            treeSelect.IsDropDownOpen = true;
            Dispatcher.UIThread.RunJobs();

            var treeView = GetPopupTreeView(treeSelect);
            treeView.SelectedItems.Cast<ITreeItemNode>().ShouldContain(firstNode);

            viewModel.SelectedItems!.Add(secondNode);
            Dispatcher.UIThread.RunJobs();

            formValueChangedCount.ShouldBe(1);
            treeSelect.EffectiveSelectedItems.ShouldNotBeNull();
            treeSelect.EffectiveSelectedItems.ShouldBe([firstNode, secondNode]);
            treeSelect.SelectedCount.ShouldBe(2);
            FindSelectTag(treeSelect, "Second").ShouldNotBeNull();
            treeView.SelectedItems.Cast<ITreeItemNode>().ShouldContain(secondNode);

            treeView.ItemsSource = new List<ITreeItemNode> { firstNode, secondNode };
            Dispatcher.UIThread.RunJobs();
            formValueChangedCount.ShouldBe(1);
            treeView.SelectedItems.Cast<ITreeItemNode>().ShouldContain(firstNode);
            treeView.SelectedItems.Cast<ITreeItemNode>().ShouldContain(secondNode);

            viewModel.SelectedItems.Remove(firstNode);
            Dispatcher.UIThread.RunJobs();

            formValueChangedCount.ShouldBe(2);
            treeSelect.EffectiveSelectedItems.ShouldNotBeNull();
            treeSelect.EffectiveSelectedItems.ShouldBe([secondNode]);
            treeSelect.SelectedCount.ShouldBe(1);
            treeSelect.GetVisualDescendants()
                      .OfType<SelectTag>()
                      .ShouldNotContain(tag => tag.Text == "First");
            treeView.SelectedItems.Cast<ITreeItemNode>().ShouldNotContain(firstNode);

            viewModel.SelectedItems.Clear();
            Dispatcher.UIThread.RunJobs();

            formValueChangedCount.ShouldBe(3);
            treeSelect.EffectiveSelectedItems.ShouldNotBeNull();
            treeSelect.EffectiveSelectedItems.ShouldBeEmpty();
            treeSelect.SelectedCount.ShouldBe(0);
            treeSelect.IsSelectionEmpty.ShouldBeTrue();
            treeView.SelectedItems.Cast<ITreeItemNode>().ShouldBeEmpty();

            var oldEqualNode = new EqualByKeyNode { ItemKey = "equal", Header = "Equal", Value = "equal" };
            var source = new ObservableCollection<ITreeItemNode> { oldEqualNode };
            treeSelect.ItemsSource = source;
            treeSelect.SelectedItems = new List<ITreeItemNode> { oldEqualNode };
            Dispatcher.UIThread.RunJobs();
            formValueChangedCount = 0;

            var newEqualNode = new EqualByKeyNode { ItemKey = "equal", Header = "Equal", Value = "equal" };
            source[0] = newEqualNode;
            Dispatcher.UIThread.RunJobs();

            treeSelect.SelectedItems.ShouldNotBeNull();
            treeSelect.SelectedItems.Single().ShouldBeSameAs(newEqualNode);
            treeView.SelectedItems.Cast<ITreeItemNode>().Single().ShouldBeSameAs(newEqualNode);
            formValueChangedCount.ShouldBe(1);
        });
    }

    [Fact]
    public void SelectedItems_Mutation_Before_First_Popup_Open_Preserves_All_Selected_Items()
    {
        var firstNode = new TreeItemNode
        {
            Header = "First",
            Value  = "first"
        };
        var secondNode = new TreeItemNode
        {
            Header = "Second",
            Value  = "second"
        };
        var viewModel = new TreeSelectBindingViewModel
        {
            SelectedItems = new ObservableCollection<ITreeItemNode>()
        };
        var treeSelect = new Desktop.Controls.TreeSelect
        {
            Width              = 240,
            IsDefaultExpandAll = true,
            IsMotionEnabled    = false,
            IsMultiple         = true,
            IsAllowClear       = true,
            ItemsSource        = [firstNode, secondNode],
            PlaceholderText    = "Please select"
        };
        treeSelect.Bind(
            Desktop.Controls.TreeSelect.SelectedItemsProperty,
            new Binding(nameof(TreeSelectBindingViewModel.SelectedItems))
            {
                Source = viewModel,
                Mode   = BindingMode.TwoWay
            });

        ShowInWindow(treeSelect, () =>
        {
            viewModel.SelectedItems!.Add(firstNode);
            viewModel.SelectedItems.Add(secondNode);
            Dispatcher.UIThread.RunJobs();

            treeSelect.SelectedItems.ShouldBe([firstNode, secondNode]);
            FindSelectTag(treeSelect, "First").ShouldNotBeNull();
            FindSelectTag(treeSelect, "Second").ShouldNotBeNull();

            treeSelect.IsDropDownOpen = true;
            Dispatcher.UIThread.RunJobs();

            treeSelect.SelectedItems.ShouldBe([firstNode, secondNode]);
            FindSelectTag(treeSelect, "First").ShouldNotBeNull();
            FindSelectTag(treeSelect, "Second").ShouldNotBeNull();
            var treeView = GetPopupTreeView(treeSelect);
            treeView.SelectedItems.Cast<ITreeItemNode>().ShouldContain(firstNode);
            treeView.SelectedItems.Cast<ITreeItemNode>().ShouldContain(secondNode);
        });
    }

    [Fact]
    public void SelectedItems_ObservableCollection_Mutation_Refreshes_Checkable_TreeView()
    {
        var firstNode = new TreeItemNode
        {
            Header = "First",
            Value  = "first"
        };
        var secondNode = new TreeItemNode
        {
            Header = "Second",
            Value  = "second"
        };
        var viewModel = new TreeSelectBindingViewModel
        {
            SelectedItems = new ObservableCollection<ITreeItemNode>
            {
                firstNode
            }
        };
        var treeSelect = new Desktop.Controls.TreeSelect
        {
            Width              = 240,
            IsDefaultExpandAll = true,
            IsMotionEnabled    = false,
            IsTreeCheckable    = true,
            ItemsSource        = [firstNode, secondNode],
            PlaceholderText    = "Please select"
        };
        treeSelect.Bind(
            Desktop.Controls.TreeSelect.SelectedItemsProperty,
            new Binding(nameof(TreeSelectBindingViewModel.SelectedItems))
            {
                Source = viewModel,
                Mode   = BindingMode.TwoWay
            });

        ShowInWindow(treeSelect, () =>
        {
            treeSelect.IsDropDownOpen = true;
            Dispatcher.UIThread.RunJobs();

            var treeView = GetPopupTreeView(treeSelect);
            treeView.CheckedItems.Cast<ITreeItemNode>().ShouldContain(firstNode);

            viewModel.SelectedItems!.Add(secondNode);
            Dispatcher.UIThread.RunJobs();

            treeSelect.EffectiveSelectedItems.ShouldNotBeNull();
            treeSelect.EffectiveSelectedItems.ShouldBe([firstNode, secondNode]);
            FindSelectTag(treeSelect, "Second").ShouldNotBeNull();
            treeView.CheckedItems.Cast<ITreeItemNode>().ShouldContain(secondNode);

            viewModel.SelectedItems.Remove(firstNode);
            Dispatcher.UIThread.RunJobs();

            treeSelect.EffectiveSelectedItems.ShouldNotBeNull();
            treeSelect.EffectiveSelectedItems.ShouldBe([secondNode]);
            treeView.CheckedItems.Cast<ITreeItemNode>().ShouldNotContain(firstNode);

            viewModel.SelectedItems.Add(firstNode);
            Dispatcher.UIThread.RunJobs();
            var firstReplacement = new TreeItemNode { Header = "First replacement", Value = "first" };
            var secondReplacement = new TreeItemNode { Header = "Second replacement", Value = "second" };
            var formValueChangedCount = 0;
            ((IFormItemAware)treeSelect).ValueChanged += (_, _) => formValueChangedCount++;

            treeSelect.ItemsSource = new List<ITreeItemNode> { firstReplacement, secondReplacement };
            Dispatcher.UIThread.RunJobs();

            formValueChangedCount.ShouldBe(1);
            treeSelect.SelectedItems.ShouldNotBeNull();
            treeSelect.SelectedItems.ShouldContain(firstReplacement);
            treeSelect.SelectedItems.ShouldContain(secondReplacement);
            treeView.CheckedItems.Cast<ITreeItemNode>().ShouldContain(firstReplacement);
            treeView.CheckedItems.Cast<ITreeItemNode>().ShouldContain(secondReplacement);

            var oldEqualNode = new EqualByKeyNode { ItemKey = "equal", Header = "Equal", Value = "equal" };
            var source = new ObservableCollection<ITreeItemNode> { oldEqualNode };
            treeSelect.ItemsSource = source;
            treeSelect.SelectedItems = new List<ITreeItemNode> { oldEqualNode };
            Dispatcher.UIThread.RunJobs();
            formValueChangedCount = 0;

            var newEqualNode = new EqualByKeyNode { ItemKey = "equal", Header = "Equal", Value = "equal" };
            oldEqualNode.Equals(newEqualNode).ShouldBeTrue();
            source[0] = newEqualNode;
            Dispatcher.UIThread.RunJobs();

            treeSelect.SelectedItems.ShouldNotBeNull();
            treeSelect.SelectedItems.Count.ShouldBe(1);
            treeSelect.SelectedItems[0].ShouldBeSameAs(newEqualNode);
            treeView.CheckedItems.Cast<ITreeItemNode>().Single().ShouldBeSameAs(newEqualNode);
            formValueChangedCount.ShouldBe(1);
        });
    }

    [Fact]
    public void CheckedItems_Popup_Reopen_Continues_To_Sync_Cascading_Selection()
    {
        var firstChild = new TreeItemNode { Header = "First", Value = "first" };
        var secondChild = new TreeItemNode { Header = "Second", Value = "second" };
        var parentNode = new TreeItemNode
        {
            Header   = "Parent",
            Value    = "parent",
            Children = [firstChild, secondChild]
        };
        var viewModel = new TreeSelectBindingViewModel
        {
            SelectedItems = new ObservableCollection<ITreeItemNode>()
        };
        var treeSelect = new Desktop.Controls.TreeSelect
        {
            Width              = 240,
            IsDefaultExpandAll = true,
            IsMotionEnabled    = false,
            IsTreeCheckable    = true,
            ItemsSource        = [parentNode],
            PlaceholderText    = "Please select"
        };
        treeSelect.Bind(
            Desktop.Controls.TreeSelect.SelectedItemsProperty,
            new Binding(nameof(TreeSelectBindingViewModel.SelectedItems))
            {
                Source = viewModel,
                Mode   = BindingMode.TwoWay
            });

        ShowInWindow(treeSelect, () =>
        {
            treeSelect.IsDropDownOpen = true;
            Dispatcher.UIThread.RunJobs();

            var treeView = GetPopupTreeView(treeSelect);
            var parentContainer = treeView.ContainerFromItem(parentNode)
                                         .ShouldBeAssignableTo<Desktop.Controls.TreeViewItem>()!;
            parentContainer.ContainerFromItem(firstChild)
                           .ShouldBeAssignableTo<Desktop.Controls.TreeViewItem>();

            treeSelect.IsDropDownOpen = false;
            Dispatcher.UIThread.RunJobs();

            treeSelect.IsDropDownOpen = true;
            Dispatcher.UIThread.RunJobs();
            var reopenedParentContainer = treeView.ContainerFromItem(parentNode)
                                                   .ShouldBeAssignableTo<Desktop.Controls.TreeViewItem>()!;
            var secondContainer = reopenedParentContainer.ContainerFromItem(secondChild)
                                                       .ShouldBeAssignableTo<Desktop.Controls.TreeViewItem>();
            secondContainer!.SetCurrentValue(Desktop.Controls.TreeViewItem.IsCheckedProperty, true);
            Dispatcher.UIThread.RunJobs();

            treeView.CheckedItems.Cast<ITreeItemNode>().ShouldContain(secondChild);
            treeSelect.SelectedItems.ShouldNotBeNull();
            treeSelect.SelectedItems.ShouldContain(secondChild);
            viewModel.SelectedItems.ShouldNotBeNull();
            viewModel.SelectedItems.ShouldContain(secondChild);
        });
    }

    [Fact]
    public void BindableNode_Popup_Reopen_Continues_To_Sync_Node_State()
    {
        var node = new BindableTreeItemNode
        {
            Header    = "Node",
            Value     = "node",
            IsChecked = false
        };
        var treeSelect = new Desktop.Controls.TreeSelect
        {
            Width              = 240,
            IsDefaultExpandAll = true,
            IsMotionEnabled    = false,
            IsTreeCheckable    = true,
            ItemsSource        = [node],
            PlaceholderText    = "Please select"
        };

        ShowInWindow(treeSelect, () =>
        {
            treeSelect.IsDropDownOpen = true;
            Dispatcher.UIThread.RunJobs();

            var treeView = GetPopupTreeView(treeSelect);
            var container = treeView.ContainerFromItem(node)
                                     .ShouldBeAssignableTo<Desktop.Controls.TreeViewItem>()!;

            treeSelect.IsDropDownOpen = false;
            Dispatcher.UIThread.RunJobs();
            treeSelect.IsDropDownOpen = true;
            Dispatcher.UIThread.RunJobs();

            var reopenedContainer = treeView.ContainerFromItem(node)
                                              .ShouldBeAssignableTo<Desktop.Controls.TreeViewItem>()!;
            reopenedContainer.ShouldBeSameAs(container);
            reopenedContainer.SetCurrentValue(Desktop.Controls.TreeViewItem.IsCheckedProperty, true);
            Dispatcher.UIThread.RunJobs();

            node.IsChecked.ShouldBe(true);
        });
    }

    [Fact]
    public void BindableNode_With_Children_Popup_Reopen_Preserves_Child_Containers_And_Binding()
    {
        var child = new BindableTreeItemNode
        {
            Header = "Child",
            Value  = "child"
        };
        var parent = new BindableTreeItemNode
        {
            Header   = "Parent",
            Value    = "parent",
            Children = [child]
        };
        var treeSelect = new Desktop.Controls.TreeSelect
        {
            Width              = 240,
            IsDefaultExpandAll = true,
            IsMotionEnabled    = false,
            IsTreeCheckable    = true,
            ItemsSource        = [parent],
            PlaceholderText    = "Please select"
        };

        ShowInWindow(treeSelect, () =>
        {
            treeSelect.IsDropDownOpen = true;
            Dispatcher.UIThread.RunJobs();

            var treeView = GetPopupTreeView(treeSelect);
            var parentContainer = treeView.ContainerFromItem(parent)
                                         .ShouldBeAssignableTo<Desktop.Controls.TreeViewItem>()!;
            var childContainer = parentContainer.ContainerFromItem(child)
                                             .ShouldBeAssignableTo<Desktop.Controls.TreeViewItem>()!;

            treeSelect.IsDropDownOpen = false;
            Dispatcher.UIThread.RunJobs();
            treeSelect.IsDropDownOpen = true;
            Dispatcher.UIThread.RunJobs();

            var reopenedParentContainer = treeView.ContainerFromItem(parent)
                                                   .ShouldBeAssignableTo<Desktop.Controls.TreeViewItem>()!;
            var reopenedChildContainer = reopenedParentContainer.ContainerFromItem(child)
                                                       .ShouldBeAssignableTo<Desktop.Controls.TreeViewItem>()!;
            reopenedChildContainer.SetCurrentValue(Desktop.Controls.TreeViewItem.IsCheckedProperty, true);
            Dispatcher.UIThread.RunJobs();

            child.IsChecked.ShouldBe(true);
        });
    }

    private static void ShowInWindow(Control content, Action assertion)
    {
        var overlayPanel = new ScopeAwareOverlayLayerPanel
        {
            Width  = 420,
            Height = 320
        };
        overlayPanel.Children.Add(content);

        var visualLayerManager = new VisualLayerManager
        {
            Child = overlayPanel
        };
        EnablePopupOverlayLayer(visualLayerManager);

        var window = new AvaloniaWindow
        {
            Width   = 420,
            Height  = 320,
            Content = visualLayerManager
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            assertion();
        }
        finally
        {
            window.Close();
        }
    }

    private static Desktop.Controls.TreeView GetPopupTreeView(Desktop.Controls.TreeSelect treeSelect)
    {
        var field = typeof(Desktop.Controls.TreeSelect).GetField(
            "_treeView",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

        field.ShouldNotBeNull();
        var treeView = field.GetValue(treeSelect) as Desktop.Controls.TreeView;
        treeView.ShouldNotBeNull();
        return treeView!;
    }

    private static SelectTag? FindSelectTag(Visual root, string text)
    {
        return root.GetVisualDescendants()
                   .OfType<SelectTag>()
                   .FirstOrDefault(tag => tag.Text == text);
    }

    private static void EnablePopupOverlayLayer(VisualLayerManager visualLayerManager)
    {
        var property = typeof(VisualLayerManager).GetProperty(
            "EnablePopupOverlayLayer",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

        property.ShouldNotBeNull();
        property.SetValue(visualLayerManager, true);
    }

    private sealed class TreeSelectBindingViewModel : INotifyPropertyChanged
    {
        private ITreeItemNode? _selectedItem;
        private IList<ITreeItemNode>? _selectedItems;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ITreeItemNode? SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (ReferenceEquals(_selectedItem, value))
                {
                    return;
                }
                _selectedItem = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedItem)));
            }
        }

        public IList<ITreeItemNode>? SelectedItems
        {
            get => _selectedItems;
            set
            {
                if (ReferenceEquals(_selectedItems, value))
                {
                    return;
                }
                _selectedItems = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedItems)));
            }
        }
    }

    private sealed class EqualByKeyNode : ITreeItemNode
    {
        public EntityKey? ItemKey { get; init; }
        public ITreeNode<ITreeItemNode>? ParentNode { get; private set; }
        public object? Header { get; init; }
        public PathIcon? Icon => null;
        public bool IsEnabled { get; set; } = true;
        public bool? IsChecked { get; set; }
        public bool IsSelected { get; set; }
        public bool IsExpanded { get; set; }
        public bool IsIndicatorEnabled { get; set; } = true;
        public string? GroupName => null;
        public bool IsLeaf => true;
        public object? Value { get; set; }
        public IEnumerable<ITreeItemNode> Children => Array.Empty<ITreeItemNode>();

        public void UpdateParentNode(ITreeItemNode? parentNode)
        {
            ParentNode = parentNode;
        }

        public override bool Equals(object? obj)
        {
            return obj is EqualByKeyNode other && Equals(ItemKey, other.ItemKey);
        }

        public override int GetHashCode()
        {
            return ItemKey?.GetHashCode() ?? 0;
        }
    }
}
