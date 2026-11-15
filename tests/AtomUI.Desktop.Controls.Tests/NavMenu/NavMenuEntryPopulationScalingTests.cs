using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using Avalonia.Controls;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.NavMenu;

/// <summary>
/// Guards the cost of populating NavMenu entries one at a time: appending built-in entries must
/// validate only the new entries instead of rescanning the whole entry scope on every addition,
/// while duplicate, owner-conflict and custom-source rules stay identical to the full reconcile.
/// </summary>
public class NavMenuEntryPopulationScalingTests
{
    static NavMenuEntryPopulationScalingTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Appending_Root_Source_Entries_One_At_A_Time_Reads_Each_Entry_A_Bounded_Number_Of_Times()
    {
        var smallReads = CountRootSourceReadsForAppends(200);
        var largeReads = CountRootSourceReadsForAppends(1600);

        // A full root reconcile per Add reads every existing entry, which is quadratic in the
        // number of appends (about N^2 / 2 reads). Incremental validation keeps it linear.
        largeReads.ShouldBeLessThanOrEqualTo(1600 * 4);
        ((double)largeReads / smallReads).ShouldBeLessThan(12d);
    }

    [Fact]
    public void Appending_Direct_Root_Items_One_At_A_Time_Scales_Near_Linearly()
    {
        AssertNearLinearScaling(count =>
        {
            var menu = new AtomUI.Desktop.Controls.NavMenu();
            for (var index = 0; index < count; index++)
            {
                menu.Items.Add(new NavMenuNode());
            }
        });
    }

    [Fact]
    public void Appending_Node_Entries_One_At_A_Time_Scales_Near_Linearly()
    {
        AssertNearLinearScaling(count =>
        {
            var owner = new NavMenuNode();
            for (var index = 0; index < count; index++)
            {
                owner.Entries.Add(new NavMenuNode());
            }
        });
    }

    [Fact]
    public void Appended_Root_Entries_Take_Structural_Ownership_And_Release_It_On_Removal()
    {
        var menu = new AtomUI.Desktop.Controls.NavMenu();
        var first = new NavMenuNode();
        var group = new NavMenuGroup();
        var divider = new NavMenuDivider();
        menu.Items.Add(first);
        menu.Items.Add(divider);
        menu.Items.Add(group);
        menu.Items.Add(divider);

        Should.Throw<InvalidOperationException>(() => new NavMenuNode().Entries.Add(first));
        Should.Throw<InvalidOperationException>(() => new NavMenuNode().Entries.Add(group));

        menu.Items.Remove(first);
        menu.Items.Remove(group);

        var newOwner = new NavMenuNode();
        newOwner.Entries.Add(first);
        newOwner.Entries.Add(group);
        newOwner.Entries.ShouldBe([first, group]);
    }

    [Fact]
    public void Appending_A_Duplicate_Root_Entry_Reports_Both_Indexes_And_Keeps_Failing_Until_Removed()
    {
        var menu = new AtomUI.Desktop.Controls.NavMenu();
        var nodes = Enumerable.Range(0, 5).Select(_ => new NavMenuNode()).ToArray();
        foreach (var node in nodes)
        {
            menu.Items.Add(node);
        }

        var duplicate = Should.Throw<InvalidOperationException>(() => menu.Items.Add(nodes[2]));
        duplicate.Message.ShouldContain("indexes 2 and 5");

        // The root notification is post-mutation, so the invalid duplicate stays in Items and the
        // next reconcile reports it again instead of silently accepting the following entry.
        var late = new NavMenuNode();
        var repeated = Should.Throw<InvalidOperationException>(() => menu.Items.Add(late));
        repeated.Message.ShouldContain("indexes 2 and 5");

        menu.Items.RemoveAt(5);
        menu.Items.Remove(late);
        var accepted = new NavMenuNode();
        menu.Items.Add(accepted);
        Should.Throw<InvalidOperationException>(() => new NavMenuNode().Entries.Add(accepted));
        new NavMenuNode().Entries.Add(late);
    }

    [Fact]
    public void Appending_A_Root_Entry_Owned_Elsewhere_Fails_Without_Taking_Ownership()
    {
        var foreignOwner = new NavMenuNode();
        var foreignChild = new NavMenuNode();
        foreignOwner.Entries.Add(foreignChild);
        var menu = new AtomUI.Desktop.Controls.NavMenu();
        menu.Items.Add(new NavMenuNode());

        var exception = Should.Throw<InvalidOperationException>(() => menu.Items.Add(foreignChild));
        exception.Message.ShouldContain("already attached");

        menu.Items.Remove(foreignChild);
        foreignOwner.Entries.Remove(foreignChild);
        menu.Items.Add(foreignChild);
        Should.Throw<InvalidOperationException>(() => foreignOwner.Entries.Add(foreignChild));
    }

    [Fact]
    public void Appending_A_Root_Entry_Shared_With_A_Custom_Node_Descendant_Is_Rejected()
    {
        var shared = new NavMenuNode();
        var customNode = new ListCustomNode();
        customNode.ChildrenSource.Add(shared);
        var menu = new AtomUI.Desktop.Controls.NavMenu();
        menu.Items.Add(customNode);

        var exception = Should.Throw<InvalidOperationException>(() => menu.Items.Add(shared));

        exception.Message.ShouldContain(nameof(ListCustomNode));
        exception.Message.ShouldContain(nameof(NavMenuNode));
    }

    [Fact]
    public void Appending_After_A_Silent_Custom_Source_Change_Still_Rescans_The_Root_Scope()
    {
        var shared = new NavMenuNode();
        var customNode = new ListCustomNode();
        var menu = new AtomUI.Desktop.Controls.NavMenu();
        menu.Items.Add(new NavMenuNode());
        menu.Items.Add(customNode);

        // The plain List does not notify; only the next root reconcile can observe it.
        customNode.ChildrenSource.Add(shared);
        var exception = Should.Throw<InvalidOperationException>(() => menu.Items.Add(shared));

        exception.Message.ShouldContain(nameof(NavMenuNode));
    }

    [Fact]
    public void Appending_Resumes_Incremental_Validation_After_A_Custom_Node_Leaves_The_Scope()
    {
        var shared = new NavMenuNode();
        var customNode = new ListCustomNode();
        customNode.ChildrenSource.Add(shared);
        var menu = new AtomUI.Desktop.Controls.NavMenu();
        menu.Items.Add(customNode);
        Should.Throw<InvalidOperationException>(() => new NavMenuNode().Entries.Add(shared));

        menu.Items.Remove(customNode);
        new NavMenuNode().Entries.Add(new NavMenuNode());
        var appended = new NavMenuNode();
        menu.Items.Add(appended);
        menu.Items.Add(new NavMenuDivider());

        var duplicate = Should.Throw<InvalidOperationException>(() => menu.Items.Add(appended));
        duplicate.Message.ShouldContain("indexes 0 and 2");
    }

    [Fact]
    public void Appending_Root_Source_Entries_Keeps_The_Source_Validation_Contract()
    {
        var source = new ObservableCollection<object> { new NavMenuNode() };
        var menu = new AtomUI.Desktop.Controls.NavMenu { ItemsSource = source };
        var appended = new NavMenuNode();
        source.Add(appended);
        source.Add(new NavMenuDivider());

        var invalid = Should.Throw<InvalidOperationException>(() => source.Add("invalid"));
        invalid.Message.ShouldContain("index 3");
        invalid.Message.ShouldContain(nameof(String));
        source.RemoveAt(3);

        var duplicate = Should.Throw<InvalidOperationException>(() => source.Add(appended));
        duplicate.Message.ShouldContain("indexes 1 and 3");
        source.RemoveAt(3);

        menu.ItemsSource = null;
        new NavMenuNode().Entries.Add(appended);
    }

    [Fact]
    public void Appending_Node_Entries_Keeps_The_Atomic_Collection_Contract()
    {
        var owner = new NavMenuNode();
        var child = new NavMenuNode();
        owner.Entries.Add(child);
        owner.Entries.Add(new NavMenuDivider());

        Should.Throw<InvalidOperationException>(() => owner.Entries.Add(child));
        Should.Throw<InvalidOperationException>(() => owner.Entries.Add(owner));
        owner.Entries.Count.ShouldBe(2);

        var appended = new NavMenuNode();
        owner.Entries.Add(appended);
        appended.ParentNode.ShouldBeSameAs(owner);
        Should.Throw<InvalidOperationException>(() => new NavMenuNode().Entries.Add(appended));

        owner.Entries.Remove(appended);
        new NavMenuNode().Entries.Add(appended);
    }

    private static int CountRootSourceReadsForAppends(int count)
    {
        var source = new CountingEntrySource();
        _ = new AtomUI.Desktop.Controls.NavMenu { ItemsSource = source };
        source.ResetReads();
        for (var index = 0; index < count; index++)
        {
            source.Add(new NavMenuNode());
        }

        return source.Reads;
    }

    private static void AssertNearLinearScaling(Action<int> populate)
    {
        const int smallCount = 500;
        const int largeCount = 4000;
        populate(smallCount);
        populate(largeCount);

        var small = MedianTicks(() => populate(smallCount));
        var large = MedianTicks(() => populate(largeCount));

        // Eight times the entries: linear population stays near 8x, a full reconcile per
        // addition grows toward 64x.
        ((double)large / Math.Max(1, small)).ShouldBeLessThan(24d);
    }

    private static long MedianTicks(Action action)
    {
        var samples = new long[5];
        for (var index = 0; index < samples.Length; index++)
        {
            var stopwatch = Stopwatch.StartNew();
            action();
            samples[index] = stopwatch.ElapsedTicks;
        }

        Array.Sort(samples);
        return samples[samples.Length / 2];
    }

    private sealed class CountingEntrySource : IList, INotifyCollectionChanged, INotifyPropertyChanged
    {
        private readonly List<object?> _items = [];

        public event NotifyCollectionChangedEventHandler? CollectionChanged;
        public event PropertyChangedEventHandler? PropertyChanged;

        public int Reads { get; private set; }
        public int Count => _items.Count;
        public bool IsFixedSize => false;
        public bool IsReadOnly => false;
        public bool IsSynchronized => false;
        public object SyncRoot => this;

        public object? this[int index]
        {
            get
            {
                Reads++;
                return _items[index];
            }
            set => throw new NotSupportedException();
        }

        public void ResetReads()
        {
            Reads = 0;
        }

        public int Add(object? value)
        {
            _items.Add(value);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
            CollectionChanged?.Invoke(this,
                new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Add,
                    value,
                    _items.Count - 1));
            return _items.Count - 1;
        }

        public IEnumerator GetEnumerator()
        {
            foreach (var item in _items)
            {
                Reads++;
                yield return item;
            }
        }

        public void Clear() => throw new NotSupportedException();
        public bool Contains(object? value) => _items.Contains(value);
        public int IndexOf(object? value) => _items.IndexOf(value);
        public void Insert(int index, object? value) => throw new NotSupportedException();
        public void Remove(object? value) => throw new NotSupportedException();
        public void RemoveAt(int index) => throw new NotSupportedException();
        public void CopyTo(Array array, int index) => throw new NotSupportedException();
    }

    private sealed class ListCustomNode : INavMenuNode
    {
        public object? Header => null;
        public Avalonia.Controls.Templates.IDataTemplate? HeaderTemplate => null;
        public AtomUI.Controls.EntityKey? ItemKey => null;
        public PathIcon? Icon => null;
        public bool IsEnabled => true;
        public AtomUI.Controls.ITreeNode<INavMenuNode>? ParentNode { get; private set; }
        public List<INavMenuNode> ChildrenSource { get; } = [];
        public IEnumerable<INavMenuNode> Children => ChildrenSource;

        public void UpdateParentNode(INavMenuNode? parentNode)
        {
            ParentNode = parentNode;
        }
    }
}
