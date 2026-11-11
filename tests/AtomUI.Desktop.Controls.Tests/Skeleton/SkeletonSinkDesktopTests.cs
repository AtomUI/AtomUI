using AtomUI.Controls;
using AtomUI.Controls.Commons;
using AtomUI.Desktop.Controls.DesignTokens;
using Avalonia;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomUISkeleton = AtomUI.Desktop.Controls.Skeleton;

namespace AtomUI.Desktop.Controls.Tests.Skeleton;

public class SkeletonSinkDesktopTests
{
    static SkeletonSinkDesktopTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    private static Dimension Percent(double value)
    {
        return new Dimension(value, DimensionUnitType.Percentage);
    }

    [Fact]
    public void Desktop_types_derive_from_shared_abstractions()
    {
        typeof(AtomUISkeleton).BaseType.ShouldBe(typeof(AbstractCompositeSkeleton));
        typeof(SkeletonLine).BaseType.ShouldBe(typeof(AbstractSkeletonLine));
        typeof(SkeletonTitle).BaseType.ShouldBe(typeof(SkeletonLine));
        typeof(SkeletonParagraph).BaseType.ShouldBe(typeof(AbstractSkeletonParagraph));
        typeof(AbstractCompositeSkeleton).BaseType.ShouldBe(typeof(AbstractSkeleton));
        typeof(AbstractSkeletonLine).BaseType.ShouldBe(typeof(AbstractSkeleton));
        typeof(AbstractSkeletonParagraph).BaseType.ShouldBe(typeof(AbstractSkeleton));
        typeof(SkeletonAvatar).BaseType.ShouldBe(typeof(AbstractSkeleton));
        typeof(SkeletonElement).BaseType.ShouldBe(typeof(AbstractSkeleton));
        typeof(AbstractSkeleton).Namespace.ShouldBe("AtomUI.Controls");
    }

    [Fact]
    public void Desktop_defaults_unchanged()
    {
        var skeleton = new AtomUISkeleton();
        skeleton.ParagraphRows.ShouldBe(2);
        skeleton.TitleWidth.ShouldBe(Percent(50));
        skeleton.ParagraphLastLineWidth.ShouldBe(Percent(61));
        skeleton.IsLoading.ShouldBeFalse();
        skeleton.IsShowAvatar.ShouldBeFalse();
        skeleton.IsShowTitle.ShouldBeTrue();
        skeleton.IsShowParagraph.ShouldBeTrue();
        skeleton.AvatarShape.ShouldBe(AvatarShape.Circle);
        skeleton.AvatarSizeType.ShouldBe(CustomizableSizeType.Middle);
        double.IsNaN(skeleton.AvatarSize).ShouldBeTrue();

        new SkeletonTitle().LineWidth.ShouldBe(Percent(50));
        new SkeletonLine().LineWidth.ShouldBe(Percent(100));
        new SkeletonParagraph().Rows.ShouldBe(2);
    }

    [Fact]
    public void Desktop_token_keys_unchanged()
    {
        var expected = new[]
        {
            "AvatarMarginRight",
            "BlockRadius",
            "GradientFromColor",
            "GradientToColor",
            "ImageContainerMaxSize",
            "ImageContainerSize",
            "ImageSize",
            "LoadingBackgroundEnd",
            "LoadingBackgroundMiddle",
            "LoadingBackgroundStart",
            "LoadingMotionDuration",
            "ParagraphLineHeight",
            "ParagraphLineRoundCornerRadius",
            "ParagraphMarginTop",
            "TitleHeight"
        };

        Enum.GetNames<SkeletonTokenKind>().OrderBy(name => name, StringComparer.Ordinal)
            .ShouldBe(expected);
    }

    [Fact]
    public void Standalone_paragraph_last_row_is_full_width()
    {
        var paragraph = new SkeletonParagraph { Rows = 3 };
        var window    = new Avalonia.Controls.Window { Width = 400, Height = 300, Content = paragraph };
        try
        {
            window.Show();
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            var lines = paragraph.GetVisualDescendants().OfType<SkeletonLine>().ToList();
            lines.Count.ShouldBe(3);
            lines[2].LineWidth.ShouldBe(Percent(100));
            lines[2].Bounds.Width.ShouldBeGreaterThan(0);
            lines[2].Bounds.Width.ShouldBe(lines[0].Bounds.Width, 0.5);
        }
        finally
        {
            window.Close();
        }
    }
}
