using AtomUI.Controls;
using AtomUI.Controls.Primitives;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using System.Reflection;
using System.Runtime.CompilerServices;
using Xunit;
using AvaloniaGrid = Avalonia.Controls.Grid;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Descriptions;

public class DescriptionsResponsiveLayoutTests
{
    static DescriptionsResponsiveLayoutTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Descriptions_Invalidates_Measure_When_Media_Breakpoint_Changes()
    {
        var host = new TestMediaBreakHost
        {
            Width  = 800,
            Height = 600
        };
        var descriptions = new AtomUI.Desktop.Controls.Descriptions
        {
            IsBordered = true,
            ColumnInfo = ResponsiveInt.Parse("xs: 1, md: 2, lg: 3")
        };
        AddDescriptionItems(descriptions);
        host.Children.Add(descriptions);

        var window = new AvaloniaWindow
        {
            Width   = 800,
            Height  = 600,
            Content = host
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            descriptions.IsMeasureValid.ShouldBeTrue();

            host.SetMediaBreakPoint(MediaBreakPoint.ExtraSmall);

            descriptions.IsMeasureValid.ShouldBeFalse();
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Descriptions_ColumnInfo_Uses_Mobile_First_Cascade()
    {
        var descriptions = new AtomUI.Desktop.Controls.Descriptions
        {
            ColumnInfo = ResponsiveInt.Parse("xs: 1, md: 3")
        };

        descriptions.ColumnInfo!.Value.Resolve(MediaBreakPoint.Large, 9).ShouldBe(3);
    }

    [Fact]
    public void DescriptionItem_Span_Uses_Mobile_First_Cascade()
    {
        var item = new DescriptionItem
        {
            Span = ResponsiveInt.Parse("xs: 1, md: 3")
        };

        item.Span.Resolve(MediaBreakPoint.ExtraExtraLarge, 1).ShouldBe(3);
    }

    [Fact]
    public void Descriptions_Rebuilds_Item_Visuals_When_Horizontal_Bordered_State_Changes()
    {
        var descriptions = new AtomUI.Desktop.Controls.Descriptions();
        descriptions.Items.Add(new DescriptionItem { Label = "Product", Content = "Cloud Database" });
        descriptions.Items.Add(new DescriptionItem { Label = "Billing", Content = "Prepaid" });

        ShowInWindow(descriptions, () =>
        {
            CountVisuals<DescriptionDefaultItem>(descriptions).ShouldBe(2);

            descriptions.IsBordered = true;
            Dispatcher.UIThread.RunJobs();

            CountVisuals<DescriptionDefaultItem>(descriptions).ShouldBe(0);
            CountVisuals<DescriptionBorderedItemLabel>(descriptions).ShouldBe(2);
            CountVisuals<DescriptionBorderedItemContent>(descriptions).ShouldBe(2);

            descriptions.IsBordered = false;
            Dispatcher.UIThread.RunJobs();

            CountVisuals<DescriptionDefaultItem>(descriptions).ShouldBe(2);
            CountVisuals<DescriptionBorderedItemLabel>(descriptions).ShouldBe(0);
            CountVisuals<DescriptionBorderedItemContent>(descriptions).ShouldBe(0);
        });
    }

    [Fact]
    public void Bordered_Descriptions_ContentFrame_Applies_Outer_Border()
    {
        var descriptions = new AtomUI.Desktop.Controls.Descriptions
        {
            IsBordered = true
        };
        descriptions.Items.Add(new DescriptionItem { Label = "Product", Content = "Cloud Database" });

        ShowInWindow(descriptions, () =>
        {
            var contentFrame = FindVisualByName<PixelAlignedBorder>(descriptions, "ContentFrame");
            contentFrame.ShouldNotBeNull();
            contentFrame!.BorderBrush.ShouldNotBeNull();
            contentFrame.BorderThickness.ShouldNotBe(new Thickness());
            contentFrame.CornerRadius.ShouldNotBe(new CornerRadius());
        });
    }

    [Fact]
    public void Vertical_Bordered_Default_Item_Separator_Is_A_Coloured_Line()
    {
        var descriptions = new AtomUI.Desktop.Controls.Descriptions
        {
            IsBordered = true,
            Layout     = Orientation.Vertical
        };
        descriptions.Items.Add(new DescriptionItem { Label = "Product", Content = "Cloud Database" });

        ShowInWindow(descriptions, () =>
        {
            var separator = FindVisualByName<PixelAlignedBorder>(descriptions, "Separator");
            separator.ShouldNotBeNull();
            separator!.Background.ShouldNotBeNull();
            separator.BorderThickness.ShouldBe(default);
            separator.Height.ShouldBe(1);
        });
    }

    [Fact]
    public void Descriptions_Layouts_Repeated_Content_Items_By_Position()
    {
        var descriptions = new AtomUI.Desktop.Controls.Descriptions
        {
            ColumnInfo = 1
        };
        descriptions.Items.Add(new DescriptionItem { Label = "Same", Content = "Same content" });
        descriptions.Items.Add(new DescriptionItem { Label = "Same", Content = "Same content" });

        ShowInWindow(descriptions, () =>
        {
            var generatedItems = descriptions.GetSelfAndVisualDescendants()
                                             .OfType<DescriptionDefaultItem>()
                                             .ToList();

            generatedItems.Count.ShouldBe(2);
            AvaloniaGrid.GetRow(generatedItems[0]).ShouldBe(0);
            AvaloniaGrid.GetRow(generatedItems[1]).ShouldBe(1);
        });
    }

    [Fact]
    public void DescriptionItem_Property_Changes_Update_Generated_Default_Item()
    {
        var item = new DescriptionItem
        {
            Label   = "Product",
            Content = "Cloud Database"
        };
        var descriptions = new AtomUI.Desktop.Controls.Descriptions();
        descriptions.Items.Add(item);

        ShowInWindow(descriptions, () =>
        {
            var generatedItem = descriptions.GetSelfAndVisualDescendants()
                                            .OfType<DescriptionDefaultItem>()
                                            .Single();

            item.Label   = "Billing";
            item.Content = "Prepaid";
            Dispatcher.UIThread.RunJobs();

            generatedItem.Header.ShouldBe("Billing");
            generatedItem.Content.ShouldBe("Prepaid");
        });
    }

    [Fact]
    public void DescriptionItem_Property_Changes_Update_Generated_Bordered_Items()
    {
        var item = new DescriptionItem
        {
            Label   = "Product",
            Content = "Cloud Database"
        };
        var descriptions = new AtomUI.Desktop.Controls.Descriptions
        {
            IsBordered = true
        };
        descriptions.Items.Add(item);

        ShowInWindow(descriptions, () =>
        {
            var label = descriptions.GetSelfAndVisualDescendants()
                                    .OfType<DescriptionBorderedItemLabel>()
                                    .Single();
            var content = descriptions.GetSelfAndVisualDescendants()
                                      .OfType<DescriptionBorderedItemContent>()
                                      .Single();

            item.Label   = "Billing";
            item.Content = "Prepaid";
            Dispatcher.UIThread.RunJobs();

            label.Content.ShouldBe("Billing");
            content.Content.ShouldBe("Prepaid");
        });
    }

    [Fact]
    public void DescriptionItem_Span_Change_Recalculates_Layout()
    {
        var firstItem = new DescriptionItem { Label = "A", Content = "A", Span = 1 };
        var secondItem = new DescriptionItem { Label = "B", Content = "B" };
        var descriptions = new AtomUI.Desktop.Controls.Descriptions
        {
            ColumnInfo = 2
        };
        descriptions.Items.Add(firstItem);
        descriptions.Items.Add(secondItem);

        ShowInWindow(descriptions, () =>
        {
            var generatedItems = descriptions.GetSelfAndVisualDescendants()
                                             .OfType<DescriptionDefaultItem>()
                                             .ToList();
            AvaloniaGrid.GetRow(generatedItems[1]).ShouldBe(0);

            firstItem.Span = 2;
            Dispatcher.UIThread.RunJobs();

            AvaloniaGrid.GetColumnSpan(generatedItems[0]).ShouldBe(2);
            AvaloniaGrid.GetRow(generatedItems[1]).ShouldBe(1);
        });
    }

    [Fact]
    public void DescriptionItem_Replacement_Detaches_Old_Item_Property_Subscriptions()
    {
        var oldItem = new DescriptionItem { Label = "Old", Content = "Old content" };
        var newItem = new DescriptionItem { Label = "New", Content = "New content" };
        var descriptions = new AtomUI.Desktop.Controls.Descriptions();
        descriptions.Items.Add(oldItem);

        ShowInWindow(descriptions, () =>
        {
            descriptions.Items = [newItem];
            Dispatcher.UIThread.RunJobs();

            oldItem.Label   = "Stale";
            oldItem.Content = "Stale content";
            Dispatcher.UIThread.RunJobs();

            var generatedItem = descriptions.GetSelfAndVisualDescendants()
                                            .OfType<DescriptionDefaultItem>()
                                            .Single();
            generatedItem.Header.ShouldBe("New");
            generatedItem.Content.ShouldBe("New content");
        });
    }

    [Fact]
    public void Dynamic_Resource_Content_Does_Not_Root_Removed_DescriptionItem()
    {
        var itemReference = CreateRemovedItemReference(CreateResourceKey());

        CollectGarbage();

        itemReference.IsAlive.ShouldBeFalse(
            "DescriptionItem dynamic resources must not keep a removed item alive through Application.ResourcesChanged");
    }

    [Fact]
    public void Dynamic_Resource_Content_Uses_Owner_Descriptions_Resources()
    {
        var resourceKey = CreateResourceKey();
        Application.Current!.Resources[resourceKey] = "Application content";

        var item = new DescriptionItem { Label = "Content" };
        BindDynamicResource(item, DescriptionItem.ContentProperty, resourceKey, Application.Current);

        var descriptions = new AtomUI.Desktop.Controls.Descriptions();
        descriptions.Resources[resourceKey] = "Descriptions content";
        descriptions.Items.Add(item);

        ShowInWindow(descriptions, () =>
        {
            item.Content.ShouldBe("Descriptions content");
        });
    }

    private static void AddDescriptionItems(AtomUI.Desktop.Controls.Descriptions descriptions)
    {
        descriptions.Items.Add(new DescriptionItem { Label = "Product", Content = "Cloud Database" });
        descriptions.Items.Add(new DescriptionItem { Label = "Billing Mode", Content = "Prepaid" });
        descriptions.Items.Add(new DescriptionItem { Label = "Automatic Renewal", Content = "YES" });
        descriptions.Items.Add(new DescriptionItem { Label = "Order Time", Content = "2018-04-24 18:00:00" });
        descriptions.Items.Add(new DescriptionItem { Label = "Usage Time", Content = "2019-04-24 18:00:00", Span = 2 });
        descriptions.Items.Add(new DescriptionItem { Label = "Status", Content = "Running", Span = 3 });
    }

    private static void ShowInWindow(Control content, Action assertion)
    {
        var window = new AvaloniaWindow
        {
            Width   = 800,
            Height  = 600,
            Content = content
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

    private static int CountVisuals<T>(Control root)
        where T : Control
    {
        return root.GetSelfAndVisualDescendants().OfType<T>().Count();
    }

    private static T? FindVisualByName<T>(Control root, string name)
        where T : Control
    {
        return root.GetSelfAndVisualDescendants()
                   .OfType<T>()
                   .FirstOrDefault(control => control.Name == name);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference CreateRemovedItemReference(string resourceKey)
    {
        Application.Current!.Resources[resourceKey] = "Application content";

        var item = new DescriptionItem { Label = "Content" };
        BindDynamicResource(item, DescriptionItem.ContentProperty, resourceKey, Application.Current);

        var descriptions = new AtomUI.Desktop.Controls.Descriptions();
        descriptions.Items.Add(item);

        var window = new AvaloniaWindow
        {
            Width   = 800,
            Height  = 600,
            Content = descriptions
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            item.Content.ShouldBe("Application content");
            descriptions.Items.Remove(item);
            Dispatcher.UIThread.RunJobs();
            return new WeakReference(item);
        }
        finally
        {
            window.Close();
        }
    }

    private static string CreateResourceKey()
    {
        return $"DescriptionsResponsiveLayoutTests.Content.{Guid.NewGuid():N}";
    }

    private static void BindDynamicResource(AvaloniaObject target, AvaloniaProperty property, object key, object? anchor)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var bindMethod = typeof(AvaloniaObject).GetMethod(
            "Bind",
            flags,
            binder: null,
            types: [typeof(AvaloniaProperty), typeof(BindingBase), typeof(object)],
            modifiers: null);

        bindMethod.ShouldNotBeNull();

        var extension = new DynamicResourceExtension(key);
        var anchorField = typeof(DynamicResourceExtension).GetField("_anchor", flags);
        anchorField.ShouldNotBeNull();
        anchorField.SetValue(extension, anchor);

        bindMethod.Invoke(target, [property, extension, anchor]);
    }

    private static void CollectGarbage()
    {
        for (var i = 0; i < 3; i++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        Dispatcher.UIThread.RunJobs();
    }

    private sealed class TestMediaBreakHost : Panel, IMediaBreakAwareControl
    {
        public MediaBreakPoint MediaBreakPoint { get; private set; } = MediaBreakPoint.Large;

        public event EventHandler<MediaBreakPointChangedEventArgs>? MediaBreakPointChanged;

        public void SetMediaBreakPoint(MediaBreakPoint mediaBreakPoint)
        {
            MediaBreakPoint = mediaBreakPoint;
            MediaBreakPointChanged?.Invoke(this, new MediaBreakPointChangedEventArgs(mediaBreakPoint));
        }
    }
}
