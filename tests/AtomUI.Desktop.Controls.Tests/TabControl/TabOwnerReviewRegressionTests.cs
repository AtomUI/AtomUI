using System.Reactive.Subjects;
using AtomUI.Icons.AntDesign;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Shouldly;
using Xunit;
using AtomTabControl = AtomUI.Desktop.Controls.TabControl;
using AtomTabItem = AtomUI.Desktop.Controls.TabItem;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.TabControl;

public class TabOwnerReviewRegressionTests
{
    static TabOwnerReviewRegressionTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    public enum OwnerKind
    {
        TabControl,
        CardTabControl,
        TabStrip,
        CardTabStrip
    }

    public enum CollectionMutation
    {
        InsertBefore,
        RemoveBefore,
        RemoveTarget,
        Clear,
        ReplaceSource,
        ReplaceTarget
    }

    public static TheoryData<OwnerKind> OwnerKinds =>
    [
        OwnerKind.TabControl,
        OwnerKind.CardTabControl,
        OwnerKind.TabStrip,
        OwnerKind.CardTabStrip
    ];

    // Callback and drag commit paths are implemented by the two owner bases.
    public static TheoryData<OwnerKind> BaseOwnerKinds =>
    [
        OwnerKind.TabControl,
        OwnerKind.TabStrip
    ];

    public static TheoryData<OwnerKind, CollectionMutation> ShiftCases => Cases(
        CollectionMutation.InsertBefore,
        CollectionMutation.RemoveBefore);

    public static TheoryData<OwnerKind, CollectionMutation> InvalidatedCloseCases => Cases(
        CollectionMutation.RemoveTarget,
        CollectionMutation.Clear,
        CollectionMutation.ReplaceSource);

    public static TheoryData<OwnerKind, CollectionMutation> ReorderMutationCases => Cases(
        CollectionMutation.InsertBefore,
        CollectionMutation.RemoveBefore,
        CollectionMutation.RemoveTarget,
        CollectionMutation.Clear,
        CollectionMutation.ReplaceSource);

    public static TheoryData<OwnerKind, CollectionMutation> NonNotifyingReorderCases => Cases(
        CollectionMutation.InsertBefore,
        CollectionMutation.ReplaceTarget);

    public static TheoryData<OwnerKind, bool> ReorderExceptionCases
    {
        get
        {
            var cases = new TheoryData<OwnerKind, bool>();
            foreach (var owner in BaseOwnerKinds)
            {
                cases.Add(owner, false);
                cases.Add(owner, true);
            }
            return cases;
        }
    }

    [Theory]
    [MemberData(nameof(ShiftCases))]
    public void Close_Follows_The_Logical_Target_When_Closing_Changes_Indexes(
        OwnerKind kind,
        CollectionMutation mutation)
    {
        var source = CreateSource();
        var owner = CreateOwner(kind, source);
        owner.SelectedItem = "Delta";
        var closed = new List<Control>();
        SubscribeClosed(owner, closed.Add);
        SubscribeClosing(owner, () => MutateSource(owner, source, mutation));

        ShowInWindow(owner, _ =>
        {
            var target = GetContainer(owner, 1);

            CloseTab(owner, target).ShouldBeTrue();

            string[] expected = mutation == CollectionMutation.InsertBefore
                ? ["Inserted", "Alpha", "Charlie", "Delta"]
                : ["Charlie", "Delta"];
            source.ShouldBe(expected);
            owner.SelectedItem.ShouldBe("Delta");
            closed.Count.ShouldBe(1);
            closed[0].ShouldBeSameAs(target);
        });
    }

    [Theory]
    [MemberData(nameof(InvalidatedCloseCases))]
    public void Close_Does_Not_Delete_A_Second_Item_After_Closing_Invalidates_The_Target(
        OwnerKind kind,
        CollectionMutation mutation)
    {
        var source = CreateSource();
        var owner = CreateOwner(kind, source);
        var closedCount = 0;
        SubscribeClosed(owner, _ => closedCount++);
        SubscribeClosing(owner, () => MutateSource(owner, source, mutation));

        ShowInWindow(owner, _ =>
        {
            var target = GetContainer(owner, 1);

            CloseTab(owner, target).ShouldBeFalse();

            AssertCallbackMutation(owner, source, mutation);
            closedCount.ShouldBe(0);
        });
    }

    [Theory]
    [MemberData(nameof(OwnerKinds))]
    public void Cancelled_Close_Preserves_The_Source_And_Selection(OwnerKind kind)
    {
        var source = CreateSource();
        var owner = CreateOwner(kind, source);
        owner.SelectedItem = "Bravo";
        var closingCount = 0;
        var closedCount = 0;
        SubscribeClosing(owner, () => closingCount++, cancel: true);
        SubscribeClosed(owner, _ => closedCount++);

        ShowInWindow(owner, _ =>
        {
            CloseTab(owner, GetContainer(owner, 1)).ShouldBeFalse();

            source.ShouldBe(["Alpha", "Bravo", "Charlie", "Delta"]);
            owner.SelectedItem.ShouldBe("Bravo");
            closingCount.ShouldBe(1);
            closedCount.ShouldBe(0);
        });
    }

    [Theory]
    [MemberData(nameof(BaseOwnerKinds))]
    public void Close_Rechecks_The_Target_After_SelectionChanged_Inserts_A_Preceding_Item(OwnerKind kind)
    {
        var source = CreateSource();
        var owner = CreateOwner(kind, source);
        owner.SelectedItem = "Bravo";
        var callbackRan = false;

        ShowInWindow(owner, _ =>
        {
            owner.SelectionChanged += (_, _) =>
            {
                if (!callbackRan && Equals(owner.SelectedItem, "Alpha"))
                {
                    callbackRan = true;
                    source.Insert(0, "Inserted");
                }
            };
            var target = GetContainer(owner, 1);

            CloseTab(owner, target).ShouldBeTrue();

            callbackRan.ShouldBeTrue();
            source.ShouldBe(["Inserted", "Alpha", "Charlie", "Delta"]);
            owner.SelectedItem.ShouldBe("Alpha");
        });
    }

    [Theory]
    [MemberData(nameof(BaseOwnerKinds))]
    public void Close_Preserves_An_Explicit_SelectionChanged_Choice_After_Inserting_An_Item(OwnerKind kind)
    {
        var source = CreateSource();
        var owner = CreateOwner(kind, source);
        owner.SelectedItem = "Bravo";
        var callbackRan = false;

        ShowInWindow(owner, _ =>
        {
            owner.SelectionChanged += (_, _) =>
            {
                if (!callbackRan && Equals(owner.SelectedItem, "Alpha"))
                {
                    callbackRan = true;
                    source.Insert(0, "Inserted");
                    owner.SelectedItem = "Delta";
                }
            };

            CloseTab(owner, GetContainer(owner, 1)).ShouldBeTrue();

            callbackRan.ShouldBeTrue();
            source.ShouldBe(["Inserted", "Alpha", "Charlie", "Delta"]);
            owner.SelectedItem.ShouldBe("Delta");
        });
    }

    [Theory]
    [MemberData(nameof(BaseOwnerKinds))]
    public void Close_Stops_When_SelectionChanged_Replaces_The_Source(OwnerKind kind)
    {
        var source = CreateSource();
        var replacement = new AvaloniaList<string> { "Replacement A", "Replacement B" };
        var owner = CreateOwner(kind, source);
        owner.SelectedItem = "Bravo";
        var callbackRan = false;
        var closedCount = 0;
        SubscribeClosed(owner, _ => closedCount++);

        ShowInWindow(owner, _ =>
        {
            owner.SelectionChanged += (_, _) =>
            {
                if (!callbackRan && Equals(owner.SelectedItem, "Alpha"))
                {
                    callbackRan = true;
                    owner.ItemsSource = replacement;
                }
            };
            var target = GetContainer(owner, 1);

            CloseTab(owner, target).ShouldBeFalse();

            callbackRan.ShouldBeTrue();
            owner.ItemsSource.ShouldBeSameAs(replacement);
            source.ShouldBe(["Alpha", "Bravo", "Charlie", "Delta"]);
            replacement.ShouldBe(["Replacement A", "Replacement B"]);
            closedCount.ShouldBe(0);
        });
    }

    [Theory]
    [MemberData(nameof(ReorderMutationCases))]
    public void Reorder_Cancels_Commit_When_Reordering_Changes_The_Source(
        OwnerKind kind,
        CollectionMutation mutation)
    {
        var source = CreateSource();
        var owner = CreateOwner(kind, source);
        owner.SelectedItem = "Delta";
        var reorderingCount = 0;
        var reorderedCount = 0;
        SubscribeReordering(owner, args =>
        {
            args.Item.ShouldBe("Bravo");
            reorderingCount++;
            MutateSource(owner, source, mutation);
        });
        SubscribeReordered(owner, _ => reorderedCount++);

        ShowInWindow(owner, window =>
        {
            var containers = Enumerable.Range(0, owner.ItemCount).Select(i => GetContainer(owner, i)).ToArray();
            var transforms = containers.Select(container => container.RenderTransform).ToArray();

            DragAfter(window, containers[1], containers[2]);

            reorderingCount.ShouldBe(1);
            reorderedCount.ShouldBe(0);
            AssertCallbackMutation(owner, source, mutation);
            for (var i = 0; i < containers.Length; i++)
            {
                containers[i].RenderTransform.ShouldBe(transforms[i]);
                IsDragging(containers[i]).ShouldBeFalse();
            }
        });
    }

    [Theory]
    [MemberData(nameof(NonNotifyingReorderCases))]
    public void Reorder_Cancels_Commit_When_A_List_Changes_Without_Collection_Notifications(
        OwnerKind kind,
        CollectionMutation mutation)
    {
        var source = new List<string> { "Alpha", "Bravo", "Charlie", "Delta" };
        var owner = CreateOwner(kind, source);
        owner.SelectedItem = "Delta";
        var reorderingCount = 0;
        var reorderedCount = 0;
        SubscribeReordering(owner, args =>
        {
            args.Item.ShouldBe("Bravo");
            reorderingCount++;
            MutateSource(owner, source, mutation);
        });
        SubscribeReordered(owner, _ => reorderedCount++);

        ShowInWindow(owner, window =>
        {
            var dragged = GetContainer(owner, 1);
            var originalTransform = dragged.RenderTransform;

            DragAfter(window, dragged, GetContainer(owner, 2));

            reorderingCount.ShouldBe(1);
            reorderedCount.ShouldBe(0);
            owner.ItemsSource.ShouldBeSameAs(source);
            AssertCallbackMutation(owner, source, mutation);
            dragged.RenderTransform.ShouldBe(originalTransform);
            IsDragging(dragged).ShouldBeFalse();
        });
    }

    [Theory]
    [MemberData(nameof(ReorderExceptionCases))]
    public void Reorder_Callback_Exception_Propagates_After_Cleaning_Up_The_Drag(
        OwnerKind kind,
        bool throwAfterCommit)
    {
        var source = CreateSource();
        var owner = CreateOwner(kind, source);
        owner.SelectedItem = "Bravo";
        var expectedException = new InvalidOperationException("Reorder callback failed.");
        IPointer? pointer = null;
        owner.AddHandler(InputElement.PointerPressedEvent, (_, args) => pointer = args.Pointer,
            RoutingStrategies.Tunnel, handledEventsToo: true);
        if (throwAfterCommit)
        {
            SubscribeReordered(owner, _ => throw expectedException);
        }
        else
        {
            SubscribeReordering(owner, _ => throw expectedException);
        }

        ShowInWindow(owner, window =>
        {
            var containers = Enumerable.Range(0, owner.ItemCount).Select(i => GetContainer(owner, i)).ToArray();
            var transforms = containers.Select(container => container.RenderTransform).ToArray();
            var zIndexes = containers.Select(container => container.ZIndex).ToArray();
            var releasePoint = BeginDragAfter(window, containers[1], containers[2]);
            IsDragging(containers[1]).ShouldBeTrue();
            var capturedPointer = pointer.ShouldNotBeNull();
            capturedPointer.Captured.ShouldBeSameAs(containers[1]);

            Should.Throw<InvalidOperationException>(() => window.MouseUp(releasePoint, MouseButton.Left))
                  .ShouldBeSameAs(expectedException);

            capturedPointer.Captured.ShouldBeNull();
            for (var i = 0; i < containers.Length; i++)
            {
                containers[i].RenderTransform.ShouldBe(transforms[i]);
                containers[i].ZIndex.ShouldBe(zIndexes[i]);
                IsDragging(containers[i]).ShouldBeFalse();
            }
            var deferred = owner is BaseTabControl tabControl
                ? tabControl.IsTabReorderIndicatorRefreshDeferred
                : ((BaseTabStrip)owner).IsTabReorderIndicatorRefreshDeferred;
            deferred.ShouldBeFalse();
            if (!throwAfterCommit)
            {
                source.ShouldBe(["Alpha", "Bravo", "Charlie", "Delta"]);
            }
        });
    }

    [Theory]
    [MemberData(nameof(BaseOwnerKinds))]
    public void Cancelled_Reorder_Preserves_Source_Selection_And_Removes_Preview(OwnerKind kind)
    {
        var source = CreateSource();
        var owner = CreateOwner(kind, source);
        owner.SelectedItem = "Delta";
        var reorderingCount = 0;
        var reorderedCount = 0;
        SubscribeReordering(owner, args =>
        {
            reorderingCount++;
            args.Cancel = true;
        });
        SubscribeReordered(owner, _ => reorderedCount++);

        ShowInWindow(owner, window =>
        {
            var target = GetContainer(owner, 1);
            var originalTransform = target.RenderTransform;

            DragAfter(window, target, GetContainer(owner, 2));

            source.ShouldBe(["Alpha", "Bravo", "Charlie", "Delta"]);
            owner.SelectedItem.ShouldBe("Delta");
            reorderingCount.ShouldBe(1);
            reorderedCount.ShouldBe(0);
            target.RenderTransform.ShouldBe(originalTransform);
            IsDragging(target).ShouldBeFalse();
        });
    }

    [Theory]
    [MemberData(nameof(OwnerKinds))]
    public void Generated_Containers_Track_The_Owner_Close_Defaults_And_Preserve_Local_Overrides(OwnerKind kind)
    {
        var owner = CreateOwner(kind, CreateSource());
        SetCloseDefaults(owner, false);

        ShowInWindow(owner, _ =>
        {
            var inherited = GetContainer(owner, 0);
            var overridden = GetContainer(owner, 1);
            SetItemCloseOverrides(overridden, false);
            GetItemIsClosable(inherited).ShouldBeFalse();
            GetItemAutoHide(inherited).ShouldBeFalse();

            SetCloseDefaults(owner, true);

            GetItemIsClosable(inherited).ShouldBeTrue();
            GetItemAutoHide(inherited).ShouldBeTrue();
            GetItemIsClosable(overridden).ShouldBeFalse();
            GetItemAutoHide(overridden).ShouldBeFalse();

            SetCloseDefaults(owner, false);

            GetItemIsClosable(inherited).ShouldBeFalse();
            GetItemAutoHide(inherited).ShouldBeFalse();
            GetItemIsClosable(overridden).ShouldBeFalse();
            GetItemAutoHide(overridden).ShouldBeFalse();
        });
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Default_Close_Icon_Preserves_A_Binding_That_Changes_From_Null_To_Custom(bool isTabStripItem)
    {
        Control item = isTabStripItem ? new TabStripItem() : new AtomTabItem();
        var property = isTabStripItem ? TabStripItem.CloseIconProperty : AtomTabItem.CloseIconProperty;
        using var icons = new BehaviorSubject<PathIcon?>(null);
        using var binding = item.Bind(property, icons);

        SetItemCloseOverrides(item, true);

        item.GetValue(property).ShouldBeOfType<CloseOutlined>();

        var firstCustomIcon = new CheckOutlined();
        icons.OnNext(firstCustomIcon);
        item.GetValue(property).ShouldBeSameAs(firstCustomIcon);

        icons.OnNext(null);
        item.GetValue(property).ShouldBeOfType<CloseOutlined>();

        var secondCustomIcon = new CloseOutlined();
        icons.OnNext(secondCustomIcon);
        item.GetValue(property).ShouldBeSameAs(secondCustomIcon);
    }

    private static TheoryData<OwnerKind, CollectionMutation> Cases(params CollectionMutation[] mutations)
    {
        var result = new TheoryData<OwnerKind, CollectionMutation>();
        foreach (var owner in BaseOwnerKinds)
        {
            foreach (var mutation in mutations)
            {
                result.Add(owner, mutation);
            }
        }
        return result;
    }

    private static AvaloniaList<string> CreateSource() => ["Alpha", "Bravo", "Charlie", "Delta"];

    private static SelectingItemsControl CreateOwner(OwnerKind kind, IEnumerable<string> source)
    {
        SelectingItemsControl owner = kind switch
        {
            OwnerKind.TabControl => new AtomTabControl { IsTabReorderEnabled = true },
            OwnerKind.CardTabControl => new CardTabControl { IsTabReorderEnabled = true },
            OwnerKind.TabStrip => new AtomUI.Desktop.Controls.TabStrip { IsTabReorderEnabled = true },
            OwnerKind.CardTabStrip => new CardTabStrip { IsTabReorderEnabled = true },
            _ => throw new ArgumentOutOfRangeException(nameof(kind))
        };
        owner.Width = 460;
        owner.ItemsSource = source;
        SetCloseDefaults(owner, true);
        return owner;
    }

    private static void MutateSource(ItemsControl owner, IList<string> source, CollectionMutation mutation)
    {
        switch (mutation)
        {
            case CollectionMutation.InsertBefore:
                source.Insert(0, "Inserted");
                break;
            case CollectionMutation.RemoveBefore:
                source.RemoveAt(0);
                break;
            case CollectionMutation.RemoveTarget:
                source.Remove("Bravo");
                break;
            case CollectionMutation.Clear:
                source.Clear();
                break;
            case CollectionMutation.ReplaceSource:
                owner.ItemsSource = new AvaloniaList<string> { "Replacement A", "Replacement B" };
                break;
            case CollectionMutation.ReplaceTarget:
                source[1] = "Replacement";
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(mutation));
        }
    }

    private static void AssertCallbackMutation(ItemsControl owner, IList<string> source, CollectionMutation mutation)
    {
        string[] expected = mutation switch
        {
            CollectionMutation.InsertBefore => ["Inserted", "Alpha", "Bravo", "Charlie", "Delta"],
            CollectionMutation.RemoveBefore => ["Bravo", "Charlie", "Delta"],
            CollectionMutation.RemoveTarget => ["Alpha", "Charlie", "Delta"],
            CollectionMutation.Clear => [],
            CollectionMutation.ReplaceSource => ["Alpha", "Bravo", "Charlie", "Delta"],
            CollectionMutation.ReplaceTarget => ["Alpha", "Replacement", "Charlie", "Delta"],
            _ => throw new ArgumentOutOfRangeException(nameof(mutation))
        };
        source.ShouldBe(expected);
        if (mutation == CollectionMutation.ReplaceSource)
        {
            owner.ItemsSource.ShouldNotBeSameAs(source);
            owner.ItemsSource!.Cast<string>().ShouldBe(["Replacement A", "Replacement B"]);
        }
    }

    private static void SubscribeClosing(SelectingItemsControl owner, Action callback, bool cancel = false)
    {
        if (owner is BaseTabControl tabControl)
        {
            tabControl.Closing += (_, args) => { callback(); args.Cancel = cancel; };
        }
        else if (owner is BaseTabStrip tabStrip)
        {
            tabStrip.Closing += (_, args) => { callback(); args.Cancel = cancel; };
        }
    }

    private static void SubscribeClosed(SelectingItemsControl owner, Action<Control> callback)
    {
        if (owner is BaseTabControl tabControl)
        {
            tabControl.Closed += (_, args) => callback(args.TabItem);
        }
        else if (owner is BaseTabStrip tabStrip)
        {
            tabStrip.Closed += (_, args) => callback(args.TabStripItem);
        }
    }

    private static void SubscribeReordering(SelectingItemsControl owner, Action<TabReorderingEventArgs> callback)
    {
        if (owner is BaseTabControl tabControl)
        {
            tabControl.TabReordering += (_, args) => callback(args);
        }
        else if (owner is BaseTabStrip tabStrip)
        {
            tabStrip.TabReordering += (_, args) => callback(args);
        }
    }

    private static void SubscribeReordered(SelectingItemsControl owner, Action<TabReorderedEventArgs> callback)
    {
        if (owner is BaseTabControl tabControl)
        {
            tabControl.TabReordered += (_, args) => callback(args);
        }
        else if (owner is BaseTabStrip tabStrip)
        {
            tabStrip.TabReordered += (_, args) => callback(args);
        }
    }

    private static bool CloseTab(SelectingItemsControl owner, Control container) => owner switch
    {
        BaseTabControl tabControl => tabControl.CloseTab((AtomTabItem)container),
        BaseTabStrip tabStrip => tabStrip.CloseTab((TabStripItem)container),
        _ => throw new ArgumentOutOfRangeException(nameof(owner))
    };

    private static void SetCloseDefaults(SelectingItemsControl owner, bool value)
    {
        if (owner is BaseTabControl tabControl)
        {
            tabControl.IsTabClosable = value;
            tabControl.IsTabAutoHideCloseButton = value;
        }
        else if (owner is BaseTabStrip tabStrip)
        {
            tabStrip.IsTabClosable = value;
            tabStrip.IsTabAutoHideCloseButton = value;
        }
    }

    private static void SetItemCloseOverrides(Control container, bool value)
    {
        if (container is AtomTabItem tabItem)
        {
            tabItem.IsClosable = value;
            tabItem.IsAutoHideCloseButton = value;
        }
        else if (container is TabStripItem tabStripItem)
        {
            tabStripItem.IsClosable = value;
            tabStripItem.IsAutoHideCloseButton = value;
        }
    }

    private static bool GetItemIsClosable(Control container) => container switch
    {
        AtomTabItem tabItem => tabItem.IsClosable,
        TabStripItem tabStripItem => tabStripItem.IsClosable,
        _ => throw new ArgumentOutOfRangeException(nameof(container))
    };

    private static bool GetItemAutoHide(Control container) => container switch
    {
        AtomTabItem tabItem => tabItem.IsAutoHideCloseButton,
        TabStripItem tabStripItem => tabStripItem.IsAutoHideCloseButton,
        _ => throw new ArgumentOutOfRangeException(nameof(container))
    };

    private static bool IsDragging(Control container) => container switch
    {
        AtomTabItem tabItem => tabItem.IsTabReorderDragging,
        TabStripItem tabStripItem => tabStripItem.IsTabReorderDragging,
        _ => throw new ArgumentOutOfRangeException(nameof(container))
    };

    private static Control GetContainer(ItemsControl owner, int index)
    {
        Dispatcher.UIThread.RunJobs();
        return owner.ContainerFromIndex(index).ShouldNotBeNull();
    }

    private static void DragAfter(AvaloniaWindow window, Control source, Control target)
    {
        var end = BeginDragAfter(window, source, target);
        window.MouseUp(end, MouseButton.Left);
        Dispatcher.UIThread.RunJobs();
    }

    private static Point BeginDragAfter(AvaloniaWindow window, Control source, Control target)
    {
        var start = source.TranslatePoint(new Point(source.Bounds.Width / 2, source.Bounds.Height / 2), window)!.Value;
        var end = target.TranslatePoint(new Point(target.Bounds.Width + 8, target.Bounds.Height / 2), window)!.Value;
        window.MouseMove(start);
        window.MouseDown(start, MouseButton.Left);
        Dispatcher.UIThread.RunJobs();
        window.MouseMove(end);
        Dispatcher.UIThread.RunJobs();
        return end;
    }

    private static void ShowInWindow(Control owner, Action<AvaloniaWindow> assertion)
    {
        var window = new AvaloniaWindow { Width = 520, Height = 320, Content = owner };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            assertion(window);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }
}
