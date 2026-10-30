using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using AtomUI.Controls.Primitives;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaButton = Avalonia.Controls.Button;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.TabControl;

public class TabOverflowSessionReviewTests
{
    static TabOverflowSessionReviewTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    // Reentrancy and popup teardown run in the shared viewer; exercise both owner bases.
    public static TheoryData<OwnerKind> BaseOwnerKinds =>
    [
        OwnerKind.TabControl,
        OwnerKind.TabStrip
    ];

    [Theory]
    [MemberData(nameof(BaseOwnerKinds))]
    public void Public_Overflow_Snapshot_Cannot_Be_Rewritten_Through_IList(OwnerKind kind)
    {
        WithOwner(kind, (owner, _, _) =>
        {
            var viewer = GetViewer(owner);
            ClickMore(viewer);
            var context = viewer.OverflowPopupContext.ShouldNotBeNull();
            var snapshot = context.Items;
            snapshot.Count.ShouldBeGreaterThan(1);
            var first = snapshot[0];
            var second = snapshot[1];

            if (snapshot is IList<TabOverflowItem> list)
            {
                Should.Throw<NotSupportedException>(() => list[0] = second);
            }

            snapshot[0].ShouldBeSameAs(first);
            snapshot[1].ShouldBeSameAs(second);
            var menu = viewer.OverflowPopupRoot.ShouldBeOfType<TabOverflowMenu>();
            menu.ContainerFromIndex(0).ShouldBeOfType<TabOverflowMenuItem>()
                .OverflowItem.ShouldBeSameAs(first);
            context.TryActivate(first).ShouldBeTrue();
        });
    }

    [Theory]
    [MemberData(nameof(BaseOwnerKinds))]
    public void Dismiss_During_Opening_Snapshot_Notification_Aborts_The_Open(OwnerKind kind)
    {
        WithOwner(kind, (owner, _, _) =>
        {
            var viewer = GetViewer(owner);
            var context = OpenThenClose(viewer);
            TabOverflowItem? invalidatedItem = null;
            OnNextSnapshot(context, item =>
            {
                invalidatedItem = item;
                context.Dismiss();
            });

            ClickMore(viewer);

            AssertClosedSession(viewer, context, invalidatedItem.ShouldNotBeNull());
            ClickMore(viewer);
            viewer.IsOverflowPopupOpen.ShouldBeTrue();
            viewer.OverflowPopupContext!.Items.ShouldNotBeEmpty();
        });
    }

    [Theory]
    [MemberData(nameof(BaseOwnerKinds))]
    public void Collection_Change_During_Opening_Snapshot_Notification_Invalidates_The_Open(OwnerKind kind)
    {
        WithOwner(kind, (owner, _, _) =>
        {
            var viewer = GetViewer(owner);
            var context = OpenThenClose(viewer);
            TabOverflowItem? invalidatedItem = null;
            OnNextSnapshot(context, item =>
            {
                invalidatedItem = item;
                owner.Items.Add(CreateItem(kind, "Added during snapshot publication"));
            });

            ClickMore(viewer);

            owner.ItemCount.ShouldBe(13);
            AssertClosedSession(viewer, context, invalidatedItem.ShouldNotBeNull());
            ClickMore(viewer);
            viewer.IsOverflowPopupOpen.ShouldBeTrue();
            viewer.OverflowPopupContext!.Items.ShouldContain(item =>
                Equals(item.Header, "Added during snapshot publication"));
        });
    }

    [Theory]
    [MemberData(nameof(BaseOwnerKinds))]
    public void ItemsSource_Replacement_During_Opening_Snapshot_Notification_Invalidates_The_Open(OwnerKind kind)
    {
        WithOwner(kind, (owner, _, window) =>
        {
            var originalItems = owner.Items.Cast<Control>().ToArray();
            owner.Items.Clear();
            owner.ItemsSource = new ObservableCollection<Control>(originalItems);
            Refresh(window);
            var replacement = new ObservableCollection<Control>(
                Enumerable.Range(0, 12).Select(index => CreateItem(kind, $"Replacement {index:00}")));
            var viewer = GetViewer(owner);
            var context = OpenThenClose(viewer);
            TabOverflowItem? invalidatedItem = null;
            OnNextSnapshot(context, item =>
            {
                invalidatedItem = item;
                owner.ItemsSource = replacement;
            });

            ClickMore(viewer);

            owner.ItemsSource.ShouldBeSameAs(replacement);
            AssertClosedSession(viewer, context, invalidatedItem.ShouldNotBeNull());
            ClickMore(viewer);
            viewer.IsOverflowPopupOpen.ShouldBeTrue();
            viewer.OverflowPopupContext!.Items.ShouldAllBe(item =>
                replacement.Any(value => ReferenceEquals(value, item.Item)));
        });
    }

    [Theory]
    [MemberData(nameof(BaseOwnerKinds))]
    public void Dismiss_During_Template_Build_Does_Not_Leave_An_Open_Popup(OwnerKind kind)
    {
        WithOwner(kind, (owner, _, _) =>
        {
            var template = new CallbackTemplate(context => context.Dismiss());
            SetPopupTemplate(owner, template);
            var viewer = GetViewer(owner);

            ClickMore(viewer);

            var context = template.Context.ShouldNotBeNull();
            AssertClosedSession(viewer, context, template.FirstItem.ShouldNotBeNull());
            ClickMore(viewer);
            viewer.IsOverflowPopupOpen.ShouldBeTrue();
            viewer.OverflowPopupContext!.Items.ShouldNotBeEmpty();
        });
    }

    [Theory]
    [MemberData(nameof(BaseOwnerKinds))]
    public void Detach_During_Template_Build_Releases_The_Aborted_Popup(OwnerKind kind)
    {
        WithOwner(kind, (owner, host, window) =>
        {
            var template = new CallbackTemplate(_ => host.Children.Remove(owner));
            SetPopupTemplate(owner, template);
            var viewer = GetViewer(owner);

            ClickMore(viewer);

            owner.IsAttachedToVisualTree().ShouldBeFalse();
            AssertClosedSession(viewer, template.Context.ShouldNotBeNull(), template.FirstItem.ShouldNotBeNull());
            viewer.OverflowOwner.ShouldBeNull();
            viewer.OverflowPopupContext.ShouldBeNull();
            viewer.OverflowPopup!.Child.ShouldBeNull();
            template.Root.ShouldNotBeNull().IsAttachedToVisualTree().ShouldBeFalse();

            host.Children.Add(owner);
            Refresh(window);
            ClickMore(GetViewer(owner));
            GetViewer(owner).IsOverflowPopupOpen.ShouldBeTrue();
        });
    }

    [Theory]
    [MemberData(nameof(BaseOwnerKinds))]
    public void Template_Replacement_During_Build_Releases_The_Aborted_Content(OwnerKind kind)
    {
        WithOwner(kind, (owner, _, _) =>
        {
            var replacement = new FuncDataTemplate<TabOverflowPopupContext>(
                (_, _) => new Border { Name = "ReplacementOverflowRoot" });
            var template = new CallbackTemplate(_ => SetPopupTemplate(owner, replacement));
            SetPopupTemplate(owner, template);
            var viewer = GetViewer(owner);

            ClickMore(viewer);

            var oldContext = template.Context.ShouldNotBeNull();
            AssertClosedSession(viewer, oldContext, template.FirstItem.ShouldNotBeNull());
            viewer.OverflowPopupContext.ShouldBeNull();
            viewer.OverflowPopup!.Child.ShouldBeNull();
            template.Root.ShouldNotBeNull().IsAttachedToVisualTree().ShouldBeFalse();

            ClickMore(viewer);
            viewer.IsOverflowPopupOpen.ShouldBeTrue();
            viewer.OverflowPopupContext.ShouldNotBeSameAs(oldContext);
            viewer.OverflowPopupRoot.ShouldNotBeNull().Name.ShouldBe("ReplacementOverflowRoot");
        });
    }

    [Theory]
    [MemberData(nameof(BaseOwnerKinds))]
    public void Collection_Change_Rejects_Reopening_During_The_Closing_Snapshot_Notification(OwnerKind kind)
    {
        WithOwner(kind, (owner, _, _) =>
        {
            var viewer = GetViewer(owner);
            ClickMore(viewer);
            var context = viewer.OverflowPopupContext.ShouldNotBeNull();
            var oldItem = context.Items[0];
            var callbackInvoked = false;
            var reopenedDuringClose = false;
            OnNextEmptySnapshot(context, () =>
            {
                callbackInvoked = true;
                viewer.OpenOverflowPopup();
                reopenedDuringClose = viewer.IsOverflowPopupOpen || viewer.OverflowPopup!.IsOpen;
            });

            owner.Items.Add(CreateItem(kind, "Added while popup is open"));
            Dispatcher.UIThread.RunJobs();

            callbackInvoked.ShouldBeTrue();
            reopenedDuringClose.ShouldBeFalse();
            AssertClosedSession(viewer, context, oldItem);
            ClickMore(viewer);
            viewer.IsOverflowPopupOpen.ShouldBeTrue();
            viewer.OverflowPopupContext!.Items.ShouldContain(item =>
                Equals(item.Header, "Added while popup is open"));
        });
    }

    [Theory]
    [MemberData(nameof(BaseOwnerKinds))]
    public void Template_Replacement_Rejects_Reopening_During_The_Closing_Snapshot_Notification(OwnerKind kind)
    {
        WithOwner(kind, (owner, _, _) =>
        {
            var viewer = GetViewer(owner);
            ClickMore(viewer);
            var context = viewer.OverflowPopupContext.ShouldNotBeNull();
            var oldItem = context.Items[0];
            var oldRoot = viewer.OverflowPopupRoot.ShouldNotBeNull();
            var callbackInvoked = false;
            var reopenedDuringClose = false;
            OnNextEmptySnapshot(context, () =>
            {
                callbackInvoked = true;
                viewer.OpenOverflowPopup();
                reopenedDuringClose = viewer.IsOverflowPopupOpen || viewer.OverflowPopup!.IsOpen;
            });

            SetPopupTemplate(owner, new FuncDataTemplate<TabOverflowPopupContext>(
                (_, _) => new Border { Name = "ReplacementAfterClosingRoot" }));
            Dispatcher.UIThread.RunJobs();

            callbackInvoked.ShouldBeTrue();
            reopenedDuringClose.ShouldBeFalse();
            AssertClosedSession(viewer, context, oldItem);
            viewer.OverflowPopupContext.ShouldBeNull();
            viewer.OverflowPopup!.Child.ShouldBeNull();
            oldRoot.IsAttachedToVisualTree().ShouldBeFalse();
            ClickMore(viewer);
            viewer.IsOverflowPopupOpen.ShouldBeTrue();
            viewer.OverflowPopupContext.ShouldNotBeSameAs(context);
            viewer.OverflowPopupRoot.ShouldNotBeNull().Name.ShouldBe("ReplacementAfterClosingRoot");
        });
    }

    [Theory]
    [MemberData(nameof(BaseOwnerKinds))]
    public void Owner_Detach_Rejects_Reopening_During_The_Closing_Snapshot_Notification(OwnerKind kind)
    {
        WithOwner(kind, (owner, host, window) =>
        {
            var viewer = GetViewer(owner);
            ClickMore(viewer);
            var context = viewer.OverflowPopupContext.ShouldNotBeNull();
            var oldItem = context.Items[0];
            var oldRoot = viewer.OverflowPopupRoot.ShouldNotBeNull();
            var callbackInvoked = false;
            var reopenedDuringClose = false;
            OnNextEmptySnapshot(context, () =>
            {
                callbackInvoked = true;
                viewer.OpenOverflowPopup();
                reopenedDuringClose = viewer.IsOverflowPopupOpen || viewer.OverflowPopup!.IsOpen;
            });

            host.Children.Remove(owner);
            Dispatcher.UIThread.RunJobs();

            callbackInvoked.ShouldBeTrue();
            reopenedDuringClose.ShouldBeFalse();
            AssertClosedSession(viewer, context, oldItem);
            viewer.OverflowOwner.ShouldBeNull();
            viewer.OverflowPopupContext.ShouldBeNull();
            viewer.OverflowPopup!.Child.ShouldBeNull();
            oldRoot.IsAttachedToVisualTree().ShouldBeFalse();
            host.Children.Add(owner);
            Refresh(window);
            ClickMore(GetViewer(owner));
            GetViewer(owner).IsOverflowPopupOpen.ShouldBeTrue();
        });
    }

    [Theory]
    [MemberData(nameof(BaseOwnerKinds))]
    public void Prepared_Template_Root_Inherits_The_Popup_Context_Across_Ordinary_Closes(OwnerKind kind)
    {
        WithOwner(kind, (owner, _, _) =>
        {
            var buildCount = 0;
            var child = new TextBlock();
            var root = new Border { Child = child };
            SetPopupTemplate(owner, new FuncDataTemplate<TabOverflowPopupContext>((_, _) =>
            {
                buildCount++;
                return root;
            }));
            var viewer = GetViewer(owner);

            ClickMore(viewer);

            var context = viewer.OverflowPopupContext.ShouldNotBeNull();
            root.DataContext.ShouldBeSameAs(context);
            child.DataContext.ShouldBeSameAs(context);
            context.Dismiss();
            Dispatcher.UIThread.RunJobs();
            ClickMore(viewer);
            viewer.OverflowPopupRoot.ShouldBeSameAs(root);
            root.DataContext.ShouldBeSameAs(context);
            child.DataContext.ShouldBeSameAs(context);
            buildCount.ShouldBe(1);
        });
    }

    [Theory]
    [MemberData(nameof(BaseOwnerKinds))]
    public void Prepared_Template_Preserves_An_Explicit_Root_DataContext_Across_Ordinary_Closes(OwnerKind kind)
    {
        WithOwner(kind, (owner, _, _) =>
        {
            var buildCount = 0;
            var applicationContext = new object();
            var child = new TextBlock();
            var root = new Border { DataContext = applicationContext, Child = child };
            SetPopupTemplate(owner, new FuncDataTemplate<TabOverflowPopupContext>((_, _) =>
            {
                buildCount++;
                return root;
            }));
            var viewer = GetViewer(owner);

            ClickMore(viewer);

            var context = viewer.OverflowPopupContext.ShouldNotBeNull();
            root.DataContext.ShouldBeSameAs(applicationContext);
            child.DataContext.ShouldBeSameAs(applicationContext);
            context.Dismiss();
            Dispatcher.UIThread.RunJobs();
            ClickMore(viewer);
            viewer.OverflowPopupRoot.ShouldBeSameAs(root);
            root.DataContext.ShouldBeSameAs(applicationContext);
            child.DataContext.ShouldBeSameAs(applicationContext);
            buildCount.ShouldBe(1);
        });
    }

    private static void OnNextEmptySnapshot(TabOverflowPopupContext context, Action action)
    {
        PropertyChangedEventHandler? handler = null;
        handler = (_, args) =>
        {
            if (args.PropertyName == nameof(TabOverflowPopupContext.Items) && context.Items.Count == 0)
            {
                context.PropertyChanged -= handler;
                action();
            }
        };
        context.PropertyChanged += handler;
    }

    private static void OnNextSnapshot(TabOverflowPopupContext context, Action<TabOverflowItem> action)
    {
        PropertyChangedEventHandler? handler = null;
        handler = (_, args) =>
        {
            if (args.PropertyName == nameof(TabOverflowPopupContext.Items) && context.Items.Count > 0)
            {
                context.PropertyChanged -= handler;
                action(context.Items[0]);
            }
        };
        context.PropertyChanged += handler;
    }

    private static TabOverflowPopupContext OpenThenClose(TabScrollViewer viewer)
    {
        ClickMore(viewer);
        viewer.IsOverflowPopupOpen.ShouldBeTrue();
        var context = viewer.OverflowPopupContext.ShouldNotBeNull();
        context.Dismiss();
        Dispatcher.UIThread.RunJobs();
        viewer.IsOverflowPopupOpen.ShouldBeFalse();
        context.Items.ShouldBeEmpty();
        return context;
    }

    private static void AssertClosedSession(TabScrollViewer viewer, TabOverflowPopupContext context,
        TabOverflowItem invalidatedItem)
    {
        viewer.IsOverflowPopupOpen.ShouldBeFalse();
        viewer.OverflowPopup.ShouldNotBeNull().IsOpen.ShouldBeFalse();
        context.Items.ShouldBeEmpty();
        context.SelectedItem.ShouldBeNull();
        context.TryActivate(invalidatedItem).ShouldBeFalse();
        context.TryClose(invalidatedItem).ShouldBeFalse();
    }

    private static void ClickMore(TabScrollViewer viewer)
    {
        var indicator = viewer.GetVisualDescendants().OfType<IconButton>()
                              .Single(control => control.Name == "PART_ScrollMenuIndicator");
        indicator.IsVisible.ShouldBeTrue();
        indicator.RaiseEvent(new RoutedEventArgs(AvaloniaButton.ClickEvent, indicator));
        Dispatcher.UIThread.RunJobs();
    }

    private static TabScrollViewer GetViewer(Control owner) =>
        owner.GetVisualDescendants().OfType<TabScrollViewer>().ShouldHaveSingleItem();

    private static void SetPopupTemplate(ItemsControl owner, IDataTemplate template)
    {
        if (owner is BaseTabControl tabs)
            tabs.OverflowPopupTemplate = template;
        else
            ((BaseTabStrip)owner).OverflowPopupTemplate = template;
    }

    private static void WithOwner(OwnerKind kind, Action<ItemsControl, Panel, AvaloniaWindow> assertion)
    {
        ItemsControl owner = kind switch
        {
            OwnerKind.TabControl => new AtomUI.Desktop.Controls.TabControl { IsMotionEnabled = false },
            OwnerKind.CardTabControl => new CardTabControl { IsMotionEnabled = false },
            OwnerKind.TabStrip => new AtomUI.Desktop.Controls.TabStrip { IsMotionEnabled = false },
            _ => new CardTabStrip { IsMotionEnabled = false }
        };
        owner.Width = 220;
        owner.Height = 240;
        for (var index = 0; index < 12; index++)
            owner.Items.Add(CreateItem(kind, $"Document {index:00}"));

        var host = new ScopeAwareOverlayLayerPanel { Width = 420, Height = 320 };
        host.Children.Add(owner);
        var layers = new VisualLayerManager { Child = host };
        var overlayProperty = typeof(VisualLayerManager).GetProperty(
            "EnablePopupOverlayLayer", BindingFlags.Instance | BindingFlags.NonPublic).ShouldNotBeNull();
        overlayProperty.SetValue(layers, true);
        var window = new AvaloniaWindow { Width = 420, Height = 320, Content = layers };
        try
        {
            window.Show();
            Refresh(window);
            assertion(owner, host, window);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    private static Control CreateItem(OwnerKind kind, string header) =>
        kind is OwnerKind.TabControl or OwnerKind.CardTabControl
            ? new TabItem { Header = header, Content = header, IsClosable = true }
            : new TabStripItem { Content = header, IsClosable = true };

    private static void Refresh(AvaloniaWindow window)
    {
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();
    }

    public enum OwnerKind
    {
        TabControl,
        CardTabControl,
        TabStrip,
        CardTabStrip
    }

    private sealed class CallbackTemplate(Action<TabOverflowPopupContext> callback) : IDataTemplate
    {
        public TabOverflowPopupContext? Context { get; private set; }
        public TabOverflowItem? FirstItem { get; private set; }
        public Control? Root { get; private set; }

        public bool Match(object? data) => data is TabOverflowPopupContext;

        public Control Build(object? param)
        {
            var context = param.ShouldBeOfType<TabOverflowPopupContext>();
            if (Context is null)
            {
                Context = context;
                FirstItem = context.Items.First();
                callback(context);
            }
            return Root = new Border { Name = "CallbackOverflowRoot" };
        }
    }
}
