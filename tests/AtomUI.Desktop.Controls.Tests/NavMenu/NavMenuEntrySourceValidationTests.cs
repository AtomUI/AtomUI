using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;
using Avalonia.Controls;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.NavMenu;

public class NavMenuEntrySourceValidationTests
{
    static NavMenuEntrySourceValidationTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Direct_Items_Accept_All_Entry_Kinds()
    {
        var menu = new AtomUI.Desktop.Controls.NavMenu();

        menu.Items.Add(new NavMenuNode());
        menu.Items.Add(new NavMenuGroup());
        menu.Items.Add(new NavMenuDivider());

        menu.Items.Count.ShouldBe(3);
    }

    [Fact]
    public void ItemsSource_Replacement_Rejects_Invalid_Entries_With_Owner_Index_And_Type()
    {
        var menu = new AtomUI.Desktop.Controls.NavMenu();

        var exception = Should.Throw<InvalidOperationException>(() =>
            menu.ItemsSource = new object[] { new NavMenuNode(), "invalid" });

        exception.Message.ShouldContain(nameof(AtomUI.Desktop.Controls.NavMenu));
        exception.Message.ShouldContain("index 1");
        exception.Message.ShouldContain(nameof(String));
    }

    [Fact]
    public void ItemsSource_Add_And_Replace_Reject_Invalid_Entries()
    {
        var source = new ObservableCollection<object> { new NavMenuNode(), new NavMenuDivider() };
        var menu = new AtomUI.Desktop.Controls.NavMenu { ItemsSource = source };

        var addException = Should.Throw<InvalidOperationException>(() => source.Add(42));
        addException.Message.ShouldContain("index 2");
        addException.Message.ShouldContain(nameof(Int32));

        source.RemoveAt(2);
        var replaceException = Should.Throw<InvalidOperationException>(() => source[1] = new object());
        replaceException.Message.ShouldContain("index 1");
        replaceException.Message.ShouldContain(nameof(Object));
    }

    [Fact]
    public void ItemsSource_Reset_Rescans_The_Current_View()
    {
        var source = new ResettableEntrySource([new NavMenuNode()]);
        var menu = new AtomUI.Desktop.Controls.NavMenu { ItemsSource = source };

        var exception = Should.Throw<InvalidOperationException>(() =>
            source.ResetTo([new NavMenuDivider(), "invalid"]));

        exception.Message.ShouldContain("index 1");
        exception.Message.ShouldContain(nameof(String));
    }

    [Fact]
    public void Root_Entry_Source_Rejects_Duplicate_Stateful_Entry_Instances()
    {
        var node = new NavMenuNode();
        var group = new NavMenuGroup();
        var menu = new AtomUI.Desktop.Controls.NavMenu();

        var nodeException = Should.Throw<InvalidOperationException>(() =>
            menu.ItemsSource = new INavMenuEntry[] { node, node });
        var groupException = Should.Throw<InvalidOperationException>(() =>
            menu.ItemsSource = new INavMenuEntry[] { group, group });

        nodeException.Message.ShouldContain(nameof(NavMenuNode));
        groupException.Message.ShouldContain(nameof(NavMenuGroup));
    }

    [Fact]
    public void Root_Entry_Source_Rejects_A_Duplicate_Added_After_Initial_Attachment()
    {
        var node = new NavMenuNode();
        var source = new ObservableCollection<INavMenuEntry> { node };
        var menu = new AtomUI.Desktop.Controls.NavMenu { ItemsSource = source };

        var exception = Should.Throw<InvalidOperationException>(() => source.Add(node));

        exception.Message.ShouldContain(nameof(NavMenuNode));
        exception.Message.ShouldContain("indexes 0 and 1");
    }

    [Fact]
    public void Root_Entry_Source_Rejects_A_Shared_BuiltIn_Descendant_Exposed_By_Custom_Nodes()
    {
        var shared = new NavMenuNode();
        var first = new ObservableCustomNode();
        var second = new ObservableCustomNode();
        first.ChildrenSource.Add(shared);
        second.ChildrenSource.Add(shared);
        var menu = new AtomUI.Desktop.Controls.NavMenu();

        var exception = Should.Throw<InvalidOperationException>(() =>
            menu.ItemsSource = new INavMenuEntry[] { first, second });

        exception.Message.ShouldContain(nameof(NavMenuNode));
    }

    [Fact]
    public void Nested_Custom_Entry_Source_Releases_A_BuiltIn_Descendant_After_Removal()
    {
        var shared = new NavMenuNode();
        var first = new ObservableCustomNode();
        var second = new ObservableCustomNode();
        var menu = new AtomUI.Desktop.Controls.NavMenu
        {
            ItemsSource = new INavMenuEntry[] { first, second }
        };

        first.ChildrenSource.Add(shared);
        var exception = Should.Throw<InvalidOperationException>(() => second.ChildrenSource.Add(shared));
        exception.Message.ShouldContain(nameof(NavMenuNode));

        second.ChildrenSource.Remove(shared);
        first.ChildrenSource.Remove(shared);
        second.ChildrenSource.Add(shared);

        second.ChildrenSource.ShouldBe([shared]);
    }

    [Fact]
    public void Root_Entry_Source_Allows_A_Reused_NavMenuDivider_Instance()
    {
        var divider = new NavMenuDivider();
        var menu = new AtomUI.Desktop.Controls.NavMenu
        {
            ItemsSource = new INavMenuEntry[] { divider, divider }
        };

        menu.Items.Count.ShouldBe(2);
    }

    [Fact]
    public void Root_Entry_Can_Move_To_Another_NavMenu_After_Source_Removal()
    {
        var node = new NavMenuNode();
        var firstMenu = new AtomUI.Desktop.Controls.NavMenu
        {
            ItemsSource = new INavMenuEntry[] { node }
        };
        var secondMenu = new AtomUI.Desktop.Controls.NavMenu();

        Should.Throw<InvalidOperationException>(() =>
            secondMenu.ItemsSource = new INavMenuEntry[] { node });

        firstMenu.ItemsSource = null;
        secondMenu.ItemsSource = new INavMenuEntry[] { node };

        secondMenu.Items.ShouldBe([node]);
    }

    [Fact]
    public void Root_Entry_Ownership_Does_Not_Keep_The_NavMenu_Alive()
    {
        var node = new NavMenuNode();
        var menuReference = CreateAttachedMenuReference(node);

        ForceGarbageCollection();

        menuReference.IsAlive.ShouldBeFalse();
        var newOwner = new NavMenuNode();
        newOwner.Entries.Add(node);
        newOwner.Entries.ShouldBe([node]);
    }

    [Fact]
    public void Nested_Custom_Source_Subscription_Does_Not_Keep_The_NavMenu_Alive()
    {
        var customNode = new ObservableCustomNode();
        customNode.ChildrenSource.Add(new NavMenuNode());
        var menuReference = CreateAttachedMenuReference(customNode);

        ForceGarbageCollection();

        menuReference.IsAlive.ShouldBeFalse();
    }

    [Fact]
    public void Entry_Sources_Reject_Unsupported_Custom_Marker_Implementations()
    {
        var menu = new AtomUI.Desktop.Controls.NavMenu();

        var exception = Should.Throw<InvalidOperationException>(() =>
            menu.ItemsSource = new INavMenuEntry[] { new UnsupportedEntry() });

        exception.Message.ShouldContain(nameof(UnsupportedEntry));
        exception.Message.ShouldContain(nameof(INavMenuNode));
        exception.Message.ShouldContain(nameof(NavMenuGroup));
        exception.Message.ShouldContain(nameof(NavMenuDivider));

        var group = new NavMenuGroup();
        Should.Throw<InvalidOperationException>(() => group.Entries.Add(new UnsupportedEntry()));
        group.Entries.ShouldBeEmpty();
    }

    private sealed class ResettableEntrySource : ObservableCollection<object>
    {
        public ResettableEntrySource(IEnumerable<object> entries)
            : base(entries)
        {
        }

        public void ResetTo(IEnumerable<object> entries)
        {
            Items.Clear();
            foreach (var entry in entries)
            {
                Items.Add(entry);
            }

            OnPropertyChanged(new(nameof(Count)));
            OnPropertyChanged(new("Item[]"));
            OnCollectionChanged(new(NotifyCollectionChangedAction.Reset));
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference CreateAttachedMenuReference(INavMenuNode node)
    {
        var menu = new AtomUI.Desktop.Controls.NavMenu
        {
            ItemsSource = new INavMenuEntry[] { node }
        };
        return new WeakReference(menu);
    }

    private static void ForceGarbageCollection()
    {
        for (var i = 0; i < 3; i++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
    }

    private sealed class UnsupportedEntry : INavMenuEntry
    {
    }

    private sealed class ObservableCustomNode : INavMenuNode
    {
        public object? Header => null;
        public Avalonia.Controls.Templates.IDataTemplate? HeaderTemplate => null;
        public AtomUI.Controls.EntityKey? ItemKey => null;
        public PathIcon? Icon => null;
        public bool IsEnabled => true;
        public AtomUI.Controls.ITreeNode<INavMenuNode>? ParentNode { get; private set; }
        public ObservableCollection<INavMenuNode> ChildrenSource { get; } = [];
        public IEnumerable<INavMenuNode> Children => ChildrenSource;

        public void UpdateParentNode(INavMenuNode? parentNode)
        {
            ParentNode = parentNode;
        }
    }
}
