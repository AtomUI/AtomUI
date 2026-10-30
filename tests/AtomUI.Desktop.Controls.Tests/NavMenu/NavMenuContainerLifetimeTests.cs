using System.Reflection;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Data;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Threading;
using Shouldly;
using Xunit;
using MenuControl = AtomUI.Desktop.Controls.NavMenu;

namespace AtomUI.Desktop.Controls.Tests.NavMenu;

public class NavMenuContainerLifetimeTests
{
    static NavMenuContainerLifetimeTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Detached_Menu_Is_Collectible_When_Its_Entry_Is_Retained(bool retainEntry, bool useGroup)
    {
        var (menu, entry) = CreateDetachedGraph(retainEntry, useGroup);
        for (var i = 0; i < 4; i++)
        {
            Dispatcher.UIThread.RunJobs();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        try
        {
            menu.IsAlive.ShouldBeFalse("retaining menu data must not retain a detached menu through its resource host");
        }
        finally
        {
            GC.KeepAlive(entry);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Resource_Host_Follows_Detach_And_Reattach_Without_Losing_Updates(bool useGroup)
    {
        var key = $"NavMenuContainerLifetimeTests.{Guid.NewGuid():N}";
        Application.Current!.Resources[key] = "Application";
        var entry = CreateEntry(useGroup, key);
        var menu = CreateMenu(entry);
        menu.Resources[key] = "Menu";
        var window = new Avalonia.Controls.Window { Width = 360, Height = 300, Content = menu };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            ReadHeader(entry).ShouldBe("Menu");

            for (var i = 0; i < 3; i++)
            {
                window.Content = null;
                Dispatcher.UIThread.RunJobs();
                ReadHeader(entry).ShouldBe("Application");
                menu.Resources[key] = $"Menu {i}";
                ReadHeader(entry).ShouldBe("Application");

                window.Content = menu;
                Dispatcher.UIThread.RunJobs();
                ReadHeader(entry).ShouldBe($"Menu {i}");
                menu.Resources[key] = "Updated";
                ReadHeader(entry).ShouldBe("Updated");
            }
        }
        finally
        {
            window.Content = null;
            window.Close();
            Dispatcher.UIThread.RunJobs();
            Application.Current.Resources.Remove(key);
        }
    }

    [Fact]
    public void Resource_Reattachment_Callback_Can_Remove_The_Node_Without_Restoring_Old_Bindings()
    {
        var key = $"NavMenuContainerLifetimeTests.{Guid.NewGuid():N}";
        Application.Current!.Resources[key] = "Application";
        var node = (NavMenuNode)CreateEntry(false, key);
        var menu = CreateMenu(node);
        menu.Resources[key] = "Menu";
        var window = new Avalonia.Controls.Window { Width = 360, Height = 300, Content = menu };
        var callbackCount = 0;
        void RemoveOnResourceUpdate(object? sender, AvaloniaPropertyChangedEventArgs change)
        {
            if (change.Property == NavMenuNode.HeaderProperty && Equals(node.Header, "Menu"))
            {
                callbackCount++;
                menu.Items.Clear();
            }
        }
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            var container = menu.ContainerFromItem(node).ShouldBeOfType<NavMenuItem>();
            window.Content = null;
            Dispatcher.UIThread.RunJobs();
            node.PropertyChanged += RemoveOnResourceUpdate;
            window.Content = menu;
            Dispatcher.UIThread.RunJobs();

            callbackCount.ShouldBe(1);
            menu.Items.Count.ShouldBe(0);
            container.OwnerMenu.ShouldBeNull();
            container.Header.ShouldBeNull();
            container.NodeHeader.ShouldBeNull();
            node.Header = "Detached update";
            container.NodeHeader.ShouldBeNull();
        }
        finally
        {
            node.PropertyChanged -= RemoveOnResourceUpdate;
            window.Content = null;
            window.Close();
            Dispatcher.UIThread.RunJobs();
            Application.Current.Resources.Remove(key);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Reattached_Menu_Projects_Latest_Entry_Values_And_Preserves_Selection(bool useGroup)
    {
        var node = new NavMenuNode { Header = "Initial", ItemKey = "initial" };
        var group = new NavMenuGroup { Header = "Initial group" };
        group.Entries.Add(node);
        if (!useGroup)
        {
            group.Entries.Remove(node);
        }
        var menu = CreateMenu(useGroup ? group : node);
        menu.SelectedItem = node;
        var window = new Avalonia.Controls.Window { Width = 360, Height = 300, Content = menu };
        var template = new FuncDataTemplate<object?>((_, _) => new Avalonia.Controls.TextBlock { Text = "Updated" });
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            for (var i = 0; i < 3; i++)
            {
                window.Content = null;
                Dispatcher.UIThread.RunJobs();
                node.Header = $"Node {i}";
                node.ItemKey = $"key-{i}";
                node.HeaderTemplate = i == 1 ? null : template;
                node.Tooltip = $"Tooltip {i}";
                node.IsEnabled = i != 1;
                group.Header = $"Group {i}";
                group.HeaderTemplate = node.HeaderTemplate;

                window.Content = menu;
                Dispatcher.UIThread.RunJobs();
                var item = menu.FindRealizedMenuItem(node).ShouldNotBeNull();
                item.NodeHeader.ShouldBe(node.Header);
                item.ItemKey.ShouldBe(node.ItemKey);
                item.HeaderTemplate.ShouldBeSameAs(node.HeaderTemplate ?? menu.ItemTemplate);
                item.Tooltip.ShouldBe(node.Tooltip);
                item.IsEnabled.ShouldBe(node.IsEnabled);
                menu.SelectedItem.ShouldBeSameAs(node);
                item.IsSelected.ShouldBeTrue();
                if (useGroup)
                {
                    var groupItem = menu.ContainerFromItem(group).ShouldBeOfType<NavMenuGroupItem>();
                    groupItem.Header.ShouldBe(group.Header);
                    groupItem.HeaderTemplate.ShouldBeSameAs(group.HeaderTemplate);
                }
            }
        }
        finally
        {
            window.Content = null;
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (WeakReference Menu, INavMenuEntry? Entry) CreateDetachedGraph(bool retainEntry, bool useGroup)
    {
        var key = $"NavMenuContainerLifetimeTests.{Guid.NewGuid():N}";
        Application.Current!.Resources[key] = "Application";
        var entry = CreateEntry(useGroup, key);
        var menu = CreateMenu(entry);
        menu.Resources[key] = "Menu";
        var window = new Avalonia.Controls.Window { Width = 360, Height = 300, Content = menu };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            menu.ContainerFromItem(entry).ShouldNotBeNull();
            ReadHeader(entry).ShouldBe("Menu");
            window.Content = null;
            Dispatcher.UIThread.RunJobs();
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
            Application.Current.Resources.Remove(key);
        }

        return (new WeakReference(menu), retainEntry ? entry : null);
    }

    private static MenuControl CreateMenu(INavMenuEntry entry)
    {
        var menu = new MenuControl { Mode = NavMenuMode.Inline, IsMotionEnabled = false };
        menu.Items.Add(entry);
        return menu;
    }

    private static INavMenuEntry CreateEntry(bool useGroup, string key)
    {
        AvaloniaObject entry = useGroup ? new NavMenuGroup() : new NavMenuNode();
        var property = useGroup ? NavMenuGroup.HeaderProperty : (AvaloniaProperty)NavMenuNode.HeaderProperty;
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var bind = typeof(AvaloniaObject).GetMethod("Bind", flags, null,
            [typeof(AvaloniaProperty), typeof(BindingBase), typeof(object)], null).ShouldNotBeNull();
        var extension = new DynamicResourceExtension(key);
        typeof(DynamicResourceExtension).GetField("_anchor", flags).ShouldNotBeNull()
            .SetValue(extension, Application.Current);
        bind.Invoke(entry, [property, extension, Application.Current]);
        return (INavMenuEntry)entry;
    }

    private static object? ReadHeader(INavMenuEntry entry)
    {
        return entry switch
        {
            NavMenuGroup group => group.Header,
            INavMenuNode node => node.Header,
            _ => throw new ArgumentOutOfRangeException(nameof(entry))
        };
    }
}
