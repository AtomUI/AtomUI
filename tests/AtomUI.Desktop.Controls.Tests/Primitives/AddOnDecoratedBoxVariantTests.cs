using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomUI.Controls;
using AtomUIDesktopCascader = AtomUI.Desktop.Controls.Cascader;

namespace AtomUI.Desktop.Controls.Tests.Primitives;

public class AddOnDecoratedBoxVariantTests
{
    static AddOnDecoratedBoxVariantTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Filled_Variant_Paints_Background_Across_The_Transparent_Border()
    {
        var outlined = new AtomUIDesktopCascader { ContentLeftAddOn = "prefix" };
        var filled = new AtomUIDesktopCascader
        {
            StyleVariant = InputControlStyleVariant.Filled,
            ContentLeftAddOn = "prefix"
        };
        var stack = new StackPanel { Spacing = 8 };
        stack.Children.Add(outlined);
        stack.Children.Add(filled);

        var window = new Avalonia.Controls.Window { Width = 320, Height = 160, Content = stack };
        window.Show();
        stack.ApplyTemplate();
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();
        try
        {
            var outlinedFrame = FindContentFrame(outlined);
            var filledFrame = FindContentFrame(filled);

            // Filled 保留 1px 透明边框占位（与 Outlined 等高），背景必须铺满 border-box，
            // 否则可见填充比 Outlined 矮 2px。
            filledFrame.BackgroundSizing.ShouldBe(BackgroundSizing.OuterBorderEdge);
            outlinedFrame.BackgroundSizing.ShouldBe(BackgroundSizing.InnerBorderEdge);

            filled.Bounds.Height.ShouldBe(32);
            outlined.Bounds.Height.ShouldBe(32);
        }
        finally
        {
            window.Close();
        }
    }

    private static AddOnDecoratedBoxContentFrame FindContentFrame(AtomUIDesktopCascader cascader)
    {
        return cascader.GetVisualDescendants()
                       .OfType<AddOnDecoratedBoxContentFrame>()
                       .Single(frame => frame.Name == "PART_ContentFrame");
    }
}
