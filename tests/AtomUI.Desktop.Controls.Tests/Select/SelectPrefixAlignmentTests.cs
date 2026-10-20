using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.SelectControl;

public class SelectPrefixAlignmentTests
{
    static SelectPrefixAlignmentTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void MultipleMode_Prefix_LeftInset_Should_Match_SingleMode()
    {
        var single = new Desktop.Controls.Select
        {
            Width             = 200,
            Mode              = SelectMode.Single,
            ContentLeftAddOn  = "User",
            OptionsSource     = [new SelectOption { Header = "Lucy", Content = "lucy" }],
            DefaultValues     = ["lucy"]
        };
        var multiple = new Desktop.Controls.Select
        {
            Width             = 200,
            Mode              = SelectMode.Multiple,
            ContentLeftAddOn  = "User",
            OptionsSource     = [new SelectOption { Header = "Lucy", Content = "lucy" }],
            DefaultValues     = ["lucy"]
        };

        var panel = new StackPanel
        {
            Width    = 240,
            Spacing  = 10,
            Children = { single, multiple }
        };

        ShowInWindow(panel, () =>
        {
            var singleInset   = GetPrefixLeftInset(single);
            var multipleInset = GetPrefixLeftInset(multiple);

            multiple.IsSelectionEmpty.ShouldBeFalse();
            multipleInset.ShouldBe(singleInset, 0.51,
                "多选模式下 prefix 的左内缩应与单选模式一致（对齐 Ant Design 规范）");
        });
    }

    [Fact]
    public void TagsMode_Prefix_LeftInset_Should_Match_SingleMode()
    {
        var single = new Desktop.Controls.Select
        {
            Width             = 200,
            Mode              = SelectMode.Single,
            ContentLeftAddOn  = "User",
            OptionsSource     = [new SelectOption { Header = "Lucy", Content = "lucy" }],
            DefaultValues     = ["lucy"]
        };
        var tags = new Desktop.Controls.Select
        {
            Width             = 200,
            Mode              = SelectMode.Tags,
            ContentLeftAddOn  = "User",
            OptionsSource     = [new SelectOption { Header = "Lucy", Content = "lucy" }],
            DefaultValues     = ["lucy"]
        };

        var panel = new StackPanel
        {
            Width    = 240,
            Spacing  = 10,
            Children = { single, tags }
        };

        ShowInWindow(panel, () =>
        {
            var singleInset = GetPrefixLeftInset(single);
            var tagsInset   = GetPrefixLeftInset(tags);

            tags.IsSelectionEmpty.ShouldBeFalse();
            tagsInset.ShouldBe(singleInset, 0.51,
                "Tags 模式下 prefix 的左内缩应与单选模式一致（对齐 Ant Design 规范）");
        });
    }

    private static double GetPrefixLeftInset(Desktop.Controls.Select select)
    {
        var frame = select.GetVisualDescendants()
                          .OfType<AddOnDecoratedBoxContentFrame>()
                          .Single(f => f.Name == "PART_ContentFrame");
        var prefix = select.GetVisualDescendants()
                           .OfType<AddOnContentPresenter>()
                           .Single(p => p.Name == "PART_ContentLeftAddOn");
        var prefixTopLeft = prefix.TranslatePoint(new Point(0, 0), frame)!.Value;
        return prefixTopLeft.X - frame.BorderThickness.Left;
    }

    private static void ShowInWindow(Control content, Action assertion)
    {
        var window = new AvaloniaWindow
        {
            Width   = 420,
            Height  = 320,
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
}
