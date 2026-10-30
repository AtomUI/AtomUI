using System.Reflection;
using System.Runtime.CompilerServices;
using AtomUI.Controls.Primitives;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaButton = Avalonia.Controls.Button;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.TabControl;

public class TabOverflowMenuReuseTests
{
    private const string ReuseBrushKey = "TabOverflowReuseBrush";

    static TabOverflowMenuReuseTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    public enum OwnerKind { TabControl, CardTabControl, TabStrip, CardTabStrip }

    // Item projection differs between the two owner bases; the cache itself is shared.
    public static TheoryData<OwnerKind> BaseOwnerKinds =>
    [OwnerKind.TabControl, OwnerKind.TabStrip];

    public static TheoryData<OwnerKind> SharedViewerOwner => [OwnerKind.TabControl];

    [Theory]
    [MemberData(nameof(BaseOwnerKinds))]
    public void Reopening_Reuses_Empty_Containers_And_Their_Templates_With_Current_State_And_Resources(OwnerKind kind)
    {
        WithOwner(kind, (owner, _, window) =>
        {
            var viewer = GetViewer(owner);
            var menu = Open(viewer, window);
            var firstContainers = GetContainers(menu);
            firstContainers.Length.ShouldBe(menu.ItemCount);
            firstContainers.Length.ShouldBeGreaterThan(5);
            var templateRoots = firstContainers.ToDictionary(item => item,
                item => item.GetVisualChildren().ShouldHaveSingleItem());
            var menuSize = menu.Bounds.Size;
            menu.Resources[ReuseBrushKey] = Brushes.Red;
            foreach (var item in firstContainers)
            {
                item.Bind(TemplatedControl.BackgroundProperty, new DynamicResourceExtension(ReuseBrushKey));
                item.Background.ShouldBeSameAs(Brushes.Red);
            }

            Close(viewer, window);
            menu.ItemCount.ShouldBe(0);
            menu.Resources[ReuseBrushKey] = Brushes.Blue;
            owner.SelectedIndex = owner.ItemCount - 1;
            SetItemState((Control)owner.Items[1]!, isEnabled: false, isClosable: true);
            SetItemState((Control)owner.Items[2]!, isEnabled: true, isClosable: false);
            DrainLayout(window);

            Open(viewer, window).ShouldBeSameAs(menu);
            var reopened = GetContainers(menu);
            reopened.Length.ShouldBe(firstContainers.Length);
            menu.Bounds.Width.ShouldBe(menuSize.Width, 0.5);
            menu.Bounds.Height.ShouldBe(menuSize.Height, 0.5);
            foreach (var item in reopened)
            {
                firstContainers.ShouldContain(item);
                item.GetVisualChildren().ShouldHaveSingleItem().ShouldBeSameAs(templateRoots[item]);
                item.Background.ShouldBeSameAs(Brushes.Blue);
                var snapshot = item.OverflowItem.ShouldNotBeNull();
                item.Header.ShouldBe(snapshot.Header);
                item.HeaderTemplate.ShouldBeSameAs(snapshot.HeaderTemplate);
                item.IsEnabled.ShouldBe(snapshot.IsEnabled);
                item.IsClosable.ShouldBe(snapshot.IsClosable);
                item.IsSelected.ShouldBe(snapshot.IsSelected);
            }
            reopened.Single(item => ReferenceEquals(item.OverflowItem!.Item, owner.Items[1])).IsEnabled.ShouldBeFalse();
            reopened.Single(item => ReferenceEquals(item.OverflowItem!.Item, owner.Items[2])).IsClosable.ShouldBeFalse();
            reopened.Count(item => item.IsSelected).ShouldBe(1);

            var firstEnabled = reopened.First(item => item.IsEnabled);
            var lastEnabled = reopened.Last(item => item.IsEnabled);
            firstEnabled.Focus().ShouldBeTrue();
            window.KeyPress(Key.End, RawInputModifiers.None, PhysicalKey.End, null);
            DrainLayout(window);
            window.FocusManager!.GetFocusedElement().ShouldBeSameAs(lastEnabled);
            window.KeyPress(Key.Home, RawInputModifiers.None, PhysicalKey.Home, null);
            DrainLayout(window);
            window.FocusManager.GetFocusedElement().ShouldBeSameAs(firstEnabled);
        });
    }

    [Theory]
    [MemberData(nameof(SharedViewerOwner))]
    public void The_Empty_Cache_Shrinks_To_The_Most_Recent_Snapshot(OwnerKind kind)
    {
        WithOwner(kind, (owner, _, window) =>
        {
            var result = OpenLargeThenSmall(owner, window);
            CollectGarbage();

            result.Menu.ItemCount.ShouldBe(0);
            result.ContainerReferences.Count(reference => reference.IsAlive).ShouldBe(result.LatestCount);
            result.LatestCount.ShouldBeLessThan(result.ContainerReferences.Length);
            GC.KeepAlive(result.Menu);
        });
    }

    [Theory]
    [MemberData(nameof(BaseOwnerKinds))]
    public void Closed_Cached_Containers_Release_Snapshots_Headers_Templates_And_Template_DataContexts(OwnerKind kind)
    {
        WithOwner(kind, (owner, _, window) =>
        {
            var result = OpenAndClearPayloads(owner, window);
            foreach (var container in result.Containers)
            {
                container.OverflowItem.ShouldBeNull();
                container.Header.ShouldBeNull();
                container.HeaderTemplate.ShouldBeNull();
                container.Content.ShouldBeNull();
                container.ContentTemplate.ShouldBeNull();
                container.DataContext.ShouldBeNull();
                var presenter = HeaderPresenter(container);
                presenter.Content.ShouldBeNull();
                presenter.ContentTemplate.ShouldBeNull();
                presenter.Child.ShouldBeNull();
                presenter.DataContext.ShouldBeNull();
            }

            // Avalonia 12.1.2 keeps the last rendered composition tree reachable from a
            // detached cached root: DetachFromCompositor clears DrawList but not
            // CompositionVisual.Children, and Border retains _borderVisual. The cached
            // menu therefore holds the previous frame's composition chain — including
            // removed header template children — until the next open rebuilds it.
            // Reopen once with fresh items, then every old payload must be collectible.
            for (var index = 0; index < 12; index++)
            {
                owner.Items.Add(CreateReplacementItem(kind, $"Replacement {index:00}"));
            }
            DrainLayout(window);
            var viewer = GetViewer(owner);
            Open(viewer, window);
            CommitPendingVisualChanges();
            Close(viewer, window);
            CommitPendingVisualChanges();

            CollectGarbage();
            result.Payloads.Where(reference => reference.Reference.IsAlive)
                  .Select(reference => reference.Name)
                  .ShouldBeEmpty(RetainedPayloadPath(result, owner, window));
            GC.KeepAlive(result.Containers);
            GC.KeepAlive(result.Menu);
        });
    }

    private static string RetainedPayloadPath(PayloadReferences result, Control owner, AvaloniaWindow window)
    {
        return result.Payloads.Any(reference => reference.Reference.IsAlive)
            ? DescribeRetainedHeaderPath(result, owner, window)
            : string.Empty;
    }

    private static Control CreateReplacementItem(OwnerKind kind, string header)
    {
        Control item = kind is OwnerKind.TabControl or OwnerKind.CardTabControl
            ? new AtomUI.Desktop.Controls.TabItem()
            : new TabStripItem();
        SetHeader(item, header, null);
        SetItemState(item, isEnabled: true, isClosable: false);
        return item;
    }

    [Theory]
    [MemberData(nameof(SharedViewerOwner))]
    public void Full_Teardown_Releases_The_Menu_And_Its_Empty_Container_Cache(OwnerKind kind)
    {
        WithOwner(kind, (owner, _, window) =>
        {
            var references = CreateTornDownReferences(owner, window);
            CollectGarbage();

            references.Where(reference => reference.Reference.IsAlive)
                      .Select(reference => reference.Name).ShouldBeEmpty();
            GetViewer(owner).OverflowPopupContext.ShouldBeNull();
            GetViewer(owner).OverflowPopupRoot.ShouldBeNull();
        });
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static CacheReferences OpenLargeThenSmall(SelectingItemsControl owner, AvaloniaWindow window)
    {
        var viewer = GetViewer(owner);
        var menu = Open(viewer, window);
        var references = GetContainers(menu).Select(item => new WeakReference(item)).ToArray();
        Close(viewer, window);
        while (owner.Items.Count > 4) owner.Items.RemoveAt(owner.Items.Count - 1);
        DrainLayout(window);
        Open(viewer, window).ShouldBeSameAs(menu);
        var latestCount = menu.ItemCount;
        latestCount.ShouldBeGreaterThan(0);
        Close(viewer, window);
        CommitPendingVisualChanges();
        return new CacheReferences(menu, references, latestCount);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static PayloadReferences OpenAndClearPayloads(SelectingItemsControl owner, AvaloniaWindow window)
    {
        var graph = new object();
        var template = new PayloadTemplate(graph);
        foreach (var item in owner.Items.Cast<Control>())
        {
            SetHeader(item, new HeaderPayload("Wide header with retained data", graph), template);
        }
        DrainLayout(window);
        var viewer = GetViewer(owner);
        var menu = Open(viewer, window);
        var containers = GetContainers(menu);
        var snapshot = viewer.OverflowPopupContext!.Items;
        var references = new List<NamedReference>
        {
            new("Header graph", new WeakReference(graph)),
            new("Header template", new WeakReference(template)),
            new("Snapshot array", new WeakReference(snapshot))
        };
        for (var index = 0; index < containers.Length; index++)
        {
            var container = containers[index];
            references.Add(new NamedReference($"Snapshot item {index}", new WeakReference(container.OverflowItem!)));
            references.Add(new NamedReference($"Header {index}", new WeakReference(container.Header!)));
            references.Add(new NamedReference($"Header template child {index}",
                new WeakReference(HeaderPresenter(container).Child.ShouldNotBeNull())));
        }
        Close(viewer, window);
        foreach (var item in owner.Items.Cast<Control>()) SetHeader(item, "Replacement header", null);
        owner.Items.Clear();
        DrainLayout(window);
        CommitPendingVisualChanges();
        return new PayloadReferences(menu, containers, references.ToArray());
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static NamedReference[] CreateTornDownReferences(SelectingItemsControl owner, AvaloniaWindow window)
    {
        var viewer = GetViewer(owner);
        var menu = Open(viewer, window);
        var containers = GetContainers(menu);
        var references = containers
            .Select((item, index) => new NamedReference($"Container {index}", new WeakReference(item))).ToList();
        references.Add(new NamedReference("Menu", new WeakReference(menu)));
        Close(viewer, window);
        viewer.CloseForLifecycle();
        DrainLayout(window);
        CommitPendingVisualChanges();
        return references.ToArray();
    }

    private static void CommitPendingVisualChanges()
    {
        // Match the headless input helpers: committing a frame can enqueue work
        // for another frame, including removal of detached composition visuals.
        for (var iteration = 0; iteration < 10; iteration++)
        {
            Dispatcher.UIThread.RunJobs();
            AvaloniaHeadlessPlatform.ForceRenderTimerTick();
            if (!Dispatcher.UIThread.HasJobsWithPriority(DispatcherPriority.SystemIdle))
            {
                return;
            }
        }
        throw new InvalidOperationException("Pending visual changes did not settle within ten frames.");
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static string DescribeRetainedHeaderPath(PayloadReferences result, Control owner, AvaloniaWindow window)
    {
        var target = result.Payloads.FirstOrDefault(reference =>
            reference.Reference.IsAlive && reference.Name.StartsWith("Header ", StringComparison.Ordinal))?.Reference.Target;
        if (target is null) return "No live header payload to describe.";

        var paths = new Dictionary<object, string>(ReferenceEqualityComparer.Instance)
        {
            [result.Menu] = "Menu",
            [owner] = "Owner",
            [window] = "Window"
        };
        var queue = new Queue<(object Value, int Depth)>();
        queue.Enqueue((result.Menu, 0));
        queue.Enqueue((owner, 0));
        queue.Enqueue((window, 0));
        for (var index = 0; index < result.Containers.Length; index++)
        {
            paths[result.Containers[index]] = $"Container {index}";
            queue.Enqueue((result.Containers[index], 0));
        }

        while (queue.TryDequeue(out var entry) && paths.Count < 600000)
        {
            if (entry.Depth >= 80) continue;
            foreach (var (name, child) in ReadReferences(entry.Value))
            {
                if (child is null || paths.ContainsKey(child)) continue;
                var type = child.GetType();
                if (type.IsPrimitive || type.IsEnum || child is string or Type or MemberInfo or Assembly or Module ||
                    type.FullName?.StartsWith("System.WeakReference", StringComparison.Ordinal) == true) continue;

                var path = paths[entry.Value] + $".{name}({type.Name})";
                if (ReferenceEquals(child, target)) return path;
                paths[child] = path;
                queue.Enqueue((child, entry.Depth + 1));
            }
        }
        return $"No strong path to the retained header child from Menu/Owner/Window/Containers; visited {paths.Count} objects.";

        static IEnumerable<(string Name, object? Value)> ReadReferences(object value)
        {
            if (value is Array array)
            {
                var index = 0;
                foreach (var item in array) yield return ($"[{index++}]", item);
                yield break;
            }
            for (var type = value.GetType(); type is not null; type = type.BaseType)
            {
                foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public |
                                                     BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                {
                    object? child;
                    try { child = field.GetValue(value); }
                    catch { continue; }
                    yield return (field.Name, child);
                }
            }
        }
    }

    private static SelectingItemsControl CreateOwner(OwnerKind kind)
    {
        SelectingItemsControl owner = kind switch
        {
            OwnerKind.TabControl => new AtomUI.Desktop.Controls.TabControl { IsMotionEnabled = false },
            OwnerKind.CardTabControl => new CardTabControl { IsMotionEnabled = false },
            OwnerKind.TabStrip => new AtomUI.Desktop.Controls.TabStrip { IsMotionEnabled = false },
            OwnerKind.CardTabStrip => new CardTabStrip { IsMotionEnabled = false },
            _ => throw new ArgumentOutOfRangeException(nameof(kind))
        };
        owner.Width = 100;
        owner.Height = 240;
        owner.SelectedIndex = 0;
        for (var index = 0; index < 12; index++)
        {
            Control item = kind is OwnerKind.TabControl or OwnerKind.CardTabControl
                ? new AtomUI.Desktop.Controls.TabItem()
                : new TabStripItem();
            SetHeader(item, $"Document {index:00} with a wide header", null);
            SetItemState(item, index != 9, index % 3 == 0);
            owner.Items.Add(item);
        }
        return owner;
    }

    private static void SetHeader(Control item, object header, IDataTemplate? template)
    {
        if (item is AtomUI.Desktop.Controls.TabItem tab)
        {
            tab.Header = header;
            tab.HeaderTemplate = template;
        }
        else
        {
            var strip = (TabStripItem)item;
            strip.Content = header;
            strip.ContentTemplate = template;
        }
    }

    private static void SetItemState(Control item, bool isEnabled, bool isClosable)
    {
        item.IsEnabled = isEnabled;
        if (item is AtomUI.Desktop.Controls.TabItem tab) tab.IsClosable = isClosable;
        else ((TabStripItem)item).IsClosable = isClosable;
    }

    private static TabScrollViewer GetViewer(Control owner) =>
        owner.GetVisualDescendants().OfType<TabScrollViewer>().ShouldHaveSingleItem();

    private static TabOverflowMenuItem[] GetContainers(TabOverflowMenu menu) =>
        Enumerable.Range(0, menu.ItemCount)
                  .Select(index => menu.ContainerFromIndex(index).ShouldBeOfType<TabOverflowMenuItem>()).ToArray();

    private static ContentPresenter HeaderPresenter(Control item) =>
        item.GetVisualDescendants().OfType<ContentPresenter>()
            .Single(presenter => presenter.Name == "ItemTextPresenter");

    private static TabOverflowMenu Open(TabScrollViewer viewer, AvaloniaWindow window)
    {
        var indicator = viewer.GetVisualDescendants().OfType<IconButton>()
                              .Single(button => button.Name == "PART_ScrollMenuIndicator");
        indicator.IsVisible.ShouldBeTrue();
        indicator.RaiseEvent(new RoutedEventArgs(AvaloniaButton.ClickEvent, indicator));
        DrainLayout(window);
        viewer.IsOverflowPopupOpen.ShouldBeTrue();
        var menu = viewer.OverflowPopupRoot.ShouldBeOfType<TabOverflowMenu>();
        foreach (var container in GetContainers(menu)) container.IsMotionEnabled = false;
        return menu;
    }

    private static void Close(TabScrollViewer viewer, AvaloniaWindow window)
    {
        viewer.CloseOverflowPopup();
        DrainLayout(window);
        viewer.IsOverflowPopupOpen.ShouldBeFalse();
        viewer.OverflowPopupContext!.Items.ShouldBeEmpty();
    }

    private static void WithOwner(OwnerKind kind,
        Action<SelectingItemsControl, ScopeAwareOverlayLayerPanel, AvaloniaWindow> assertion)
    {
        var owner = CreateOwner(kind);
        var host = new ScopeAwareOverlayLayerPanel();
        host.Children.Add(owner);
        var layers = new VisualLayerManager { Child = host };
        typeof(VisualLayerManager).GetProperty("EnablePopupOverlayLayer",
            BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(layers, true);
        var window = new AvaloniaWindow { Width = 480, Height = 360, Content = layers };
        try
        {
            window.Show();
            DrainLayout(window);
            assertion(owner, host, window);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    private static void DrainLayout(AvaloniaWindow window)
    {
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();
    }

    private static void CollectGarbage()
    {
        for (var iteration = 0; iteration < 3; iteration++)
        {
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);
            GC.WaitForPendingFinalizers();
        }
    }

    private sealed record HeaderPayload(string Text, object Graph);

    private sealed class PayloadTemplate(object graph) : IDataTemplate
    {
        public bool Match(object? data) => data is HeaderPayload;

        public Control Build(object? data) => new Border
        {
            Width = 180, Height = 24, DataContext = data, Tag = graph,
            Child = new TextBlock { Text = (data as HeaderPayload)?.Text }
        };
    }

    private sealed record CacheReferences(TabOverflowMenu Menu, WeakReference[] ContainerReferences, int LatestCount);
    private sealed record PayloadReferences(TabOverflowMenu Menu, TabOverflowMenuItem[] Containers, NamedReference[] Payloads);
    private sealed record NamedReference(string Name, WeakReference Reference);
}
