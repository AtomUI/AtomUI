using AtomUI.Controls;
using AtomUI.Controls.Data;
using AtomUI.Desktop.Controls;
using AtomUI.Toolkits.GalleryBase.Controls;
using Avalonia;
using Avalonia.VisualTree;
using Avalonia.Threading;
using Shouldly;
using Xunit;
using AvWindow = Avalonia.Controls.Window;

namespace AtomUI.Toolkits.GalleryBase.Tests.Controls;

public class SemanticPartTargetResolverTransferTests
{
    static SemanticPartTargetResolverTransferTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    private static IListItemData Item(string key)
    {
        return new ListItemData
        {
            ItemKey = key,
            Content = $"content{key}"
        };
    }

    private static (ListTransfer Transfer, AvWindow Window) ShowPopulatedListTransfer()
    {
        var transfer = new ListTransfer
        {
            Width           = 520,
            Height          = 320,
            IsMotionEnabled = false,
            ItemsSource = new IListItemData[]
            {
                Item("1"),
                Item("2"),
                Item("3")
            },
            TargetKeys = new List<EntityKey> { "3" }
        };
        var window = new AvWindow { Width = 640, Height = 460, Content = transfer };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();
        return (transfer, window);
    }

    [Theory]
    [InlineData("item", 3)]
    [InlineData("source.item", 2)]
    [InlineData("target.item", 1)]
    public void Item_Parts_Resolve_To_Transfer_List_Item_Containers(string partName, int expectedCount)
    {
        var (transfer, window) = ShowPopulatedListTransfer();
        var registry = Application.Current.ShouldNotBeNull()
                                  .GetThemeManager().ShouldNotBeNull()
                                  .SemanticParts;
        registry.TryGetControl(typeof(ListTransfer), out var descriptor).ShouldBeTrue();
        descriptor.ShouldNotBeNull();

        try
        {
            var part = descriptor.Parts.Single(candidate => candidate.Name == partName);
            var resolution = SemanticPartTargetResolver.Resolve(transfer, part, registry);

            resolution.TotalMatchCount.ShouldBe(expectedCount);
            if (partName is "item" or "source.item" or "target.item")
            {
                resolution.Targets.ShouldAllBe(static target => target is TransferListItem);
            }
        }
        finally
        {
            window.Close();
        }
    }

    [Theory]
    [InlineData("itemIcon", 3)]
    [InlineData("source.itemIcon", 2)]
    [InlineData("target.itemIcon", 1)]
    [InlineData("itemContent", 3)]
    [InlineData("source.itemContent", 2)]
    [InlineData("target.itemContent", 1)]
    public void Item_Content_Parts_Resolve_Inside_Item_Templates(string partName, int expectedCount)
    {
        var (transfer, window) = ShowPopulatedListTransfer();
        var registry = Application.Current.ShouldNotBeNull()
                                  .GetThemeManager().ShouldNotBeNull()
                                  .SemanticParts;
        registry.TryGetControl(typeof(ListTransfer), out var descriptor).ShouldBeTrue();
        descriptor.ShouldNotBeNull();

        try
        {
            var part = descriptor.Parts.Single(candidate => candidate.Name == partName);
            var resolution = SemanticPartTargetResolver.Resolve(transfer, part, registry);

            resolution.TotalMatchCount.ShouldBe(expectedCount);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void List_View_Item_Part_Resolves_To_Containers()
    {
        var listView = new AtomUI.Desktop.Controls.ListView
        {
            Width           = 420,
            Height          = 260,
            IsMotionEnabled = false,
            ItemsSource = new IListItemData[]
            {
                Item("1"),
                Item("2"),
                Item("3")
            }
        };
        var window = new AvWindow { Width = 520, Height = 360, Content = listView };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();

        var registry = Application.Current.ShouldNotBeNull()
                                  .GetThemeManager().ShouldNotBeNull()
                                  .SemanticParts;
        registry.TryGetControl(typeof(AtomUI.Desktop.Controls.ListView), out var descriptor).ShouldBeTrue();
        descriptor.ShouldNotBeNull();

        try
        {
            var part = descriptor.Parts.Single(candidate => candidate.Name == "item");
            var markers = listView.GetVisualDescendants()
                                  .Where(candidate => candidate.Classes.Contains("semantic-item"))
                                  .ToArray();
            var resolution = SemanticPartTargetResolver.Resolve(listView, part, registry);

            markers.Length.ShouldBe(3, $"markers={markers.Length}");
            resolution.TotalMatchCount.ShouldBe(3, $"resolved={resolution.TotalMatchCount} of markers={markers.Length}");
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Target_Item_Part_Resolves_For_The_Semantic_Preview_Configuration()
    {
        // 与 Gallery 语义预览同配置：20 项、TargetKeys 为 key 3 与 9（两个节点）
        var transfer = new ListTransfer
        {
            Width           = 520,
            Height          = 320,
            IsMotionEnabled = false,
            ItemsSource = new IListItemData[]
            {
                Item("1"), Item("2"), Item("3"), Item("4"), Item("5"),
                Item("6"), Item("7"), Item("8"), Item("9"), Item("10"),
                Item("11"), Item("12"), Item("13"), Item("14"), Item("15"),
                Item("16"), Item("17"), Item("18"), Item("19"), Item("20")
            },
            TargetKeys = new List<EntityKey> { new("3"), new("9") }
        };
        var window = new AvWindow { Width = 640, Height = 460, Content = transfer };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();

        var registry = Application.Current.ShouldNotBeNull()
                                  .GetThemeManager().ShouldNotBeNull()
                                  .SemanticParts;
        registry.TryGetControl(typeof(ListTransfer), out var descriptor).ShouldBeTrue();
        descriptor.ShouldNotBeNull();

        try
        {
            var sourcePart = descriptor.Parts.Single(candidate => candidate.Name == "source.item");
            var targetPart = descriptor.Parts.Single(candidate => candidate.Name == "target.item");

            // 列表虚拟化下仅可视区内的条目会实例化，源侧数量随窗口尺寸变化
            SemanticPartTargetResolver.Resolve(transfer, sourcePart, registry)
                                      .TotalMatchCount.ShouldBeGreaterThanOrEqualTo(1);
            var targetResolution = SemanticPartTargetResolver.Resolve(transfer, targetPart, registry);
            targetResolution.TotalMatchCount.ShouldBe(2);
            targetResolution.Targets.ShouldAllBe(static target => target is TransferListItem);
        }
        finally
        {
            window.Close();
        }
    }


    private static IList<ITreeItemNode> BuildTreeNodes()
    {
        var node010 = new TreeItemNode { ItemKey = "0-1-0", Header = "0-1-0" };
        var node011 = new TreeItemNode { ItemKey = "0-1-1", Header = "0-1-1" };
        var node01  = new TreeItemNode { ItemKey = "0-1", Header = "0-1" };
        node01.Children.Add(node010);
        node01.Children.Add(node011);
        return new List<ITreeItemNode>
        {
            new TreeItemNode { ItemKey = "0-0", Header = "0-0" },
            node01,
            new TreeItemNode { ItemKey = "0-2", Header = "0-2" },
            new TreeItemNode { ItemKey = "0-3", Header = "0-3" },
            new TreeItemNode { ItemKey = "0-4", Header = "0-4" }
        };
    }

    [Fact]
    public void Tree_Transfer_Item_Parts_Resolve_For_Target_Side()
    {
        var transfer = new TreeTransfer
        {
            Width           = 520,
            Height          = 380,
            IsMotionEnabled = false,
            SourceView      = new TransferTreeView { IsDefaultExpandAll = true },
            ItemsSource = BuildTreeNodes(),
            TargetKeys = new List<EntityKey> { new("0-1-0"), new("0-1-1") }
        };
        var window = new AvWindow { Width = 640, Height = 500, Content = transfer };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();

        var registry = Application.Current.ShouldNotBeNull()
                                  .GetThemeManager().ShouldNotBeNull()
                                  .SemanticParts;
        registry.TryGetControl(typeof(TreeTransfer), out var descriptor).ShouldBeTrue();
        descriptor.ShouldNotBeNull();

        try
        {
            var targetContainers = transfer.GetVisualDescendants()
                                           .OfType<TransferListItem>()
                                           .Where(candidate => candidate.Classes.Contains("semantic-target-item"))
                                           .ToArray();

            var targetItemPart = descriptor.Parts.Single(candidate => candidate.Name == "target.item");
            var targetResolution = SemanticPartTargetResolver.Resolve(transfer, targetItemPart, registry);

            targetContainers.Length.ShouldBe(2, $"targetContainers={targetContainers.Length}");
            targetResolution.TotalMatchCount.ShouldBe(2);
            targetResolution.Targets.ShouldAllBe(static target => target is TransferListItem);
        }
        finally
        {
            window.Close();
        }
    }
}
