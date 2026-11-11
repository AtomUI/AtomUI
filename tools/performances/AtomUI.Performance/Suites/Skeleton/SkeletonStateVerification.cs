using System.Reflection;
using AtomUI.Controls;
using AtomUI.Desktop.Controls;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;

namespace AtomUI.Performance;

internal static partial class Program
{
    private static bool RunSkeletonStateVerification()
    {
        var failures = new List<string>();
        VerifySkeletonContentLogicalChildren(failures);
        VerifySkeletonParagraphLineWidths(failures);
        VerifySkeletonParagraphLineRebuildCleanup(failures);
        VerifyStandaloneLineShimmerLifecycle(failures);
        VerifyCompositeSharesOneShimmer(failures);

        if (failures.Count == 0)
        {
            Console.WriteLine("Skeleton state verification passed.");
            return true;
        }

        Console.Error.WriteLine("Skeleton state verification failed:");
        foreach (var failure in failures)
        {
            Console.Error.WriteLine($"- {failure}");
        }
        return false;
    }

    private static void VerifySkeletonContentLogicalChildren(ICollection<string> failures)
    {
        _ = new Skeleton();
        _ = new Skeleton();
        var skeleton = new Skeleton();
        var first    = new StackPanel();
        var second   = new StackPanel();

        skeleton.Content = first;
        Expect(skeleton.GetLogicalChildren().Count() == 1 &&
               ReferenceEquals(skeleton.GetLogicalChildren().Single(), first),
            "Skeleton should add content as a single logical child even after multiple Skeleton instances are created.",
            failures);

        skeleton.Content = second;
        var logicalChildren = skeleton.GetLogicalChildren().ToList();
        Expect(logicalChildren.Count == 1 && ReferenceEquals(logicalChildren[0], second),
            "Skeleton should replace the old logical content child with the new one.",
            failures);

        skeleton.Content = null;
        Expect(!skeleton.GetLogicalChildren().Any(),
            "Skeleton should remove logical content when Content is cleared.",
            failures);
    }

    private static void VerifySkeletonParagraphLineWidths(ICollection<string> failures)
    {
        var paragraph = new SkeletonParagraph
        {
            Rows       = 3,
            LineWidths =
            [
                new Dimension(20, DimensionUnitType.Percentage),
                new Dimension(40, DimensionUnitType.Percentage),
                new Dimension(60, DimensionUnitType.Percentage)
            ]
        };

        using var realized = RealizeControl(paragraph);
        ExpectLineWidths(paragraph, [20, 40, 60], "initial paragraph line widths", failures);

        paragraph.LineWidths =
        [
            new Dimension(30, DimensionUnitType.Percentage),
            new Dimension(50, DimensionUnitType.Percentage),
            new Dimension(70, DimensionUnitType.Percentage)
        ];
        RefreshLayout(realized.Window);
        ExpectLineWidths(paragraph, [30, 50, 70], "updated paragraph line widths", failures);

        paragraph.LineWidths    = [new Dimension(25, DimensionUnitType.Percentage)];
        paragraph.LastLineWidth = new Dimension(65, DimensionUnitType.Percentage);
        RefreshLayout(realized.Window);
        ExpectLineWidths(paragraph, [25, 100, 65], "short paragraph line width list", failures);
    }

    private static void VerifySkeletonParagraphLineRebuildCleanup(ICollection<string> failures)
    {
        var paragraph = new SkeletonParagraph
        {
            Rows     = 2,
            IsActive = false
        };
        using var realized = RealizeControl(paragraph);
        var oldLine = GetSkeletonLines(paragraph).FirstOrDefault();
        Expect(oldLine != null,
            "SkeletonParagraph should create initial SkeletonLine children.",
            failures);

        paragraph.Rows = 3;
        RefreshLayout(realized.Window);
        paragraph.IsActive = true;
        RefreshLayout(realized.Window);

        Expect(oldLine?.GetVisualParent() == null,
            "Removed SkeletonLine should be detached from the paragraph visual tree.",
            failures);
        Expect(oldLine?.IsActive == false,
            "Removed SkeletonLine should no longer follow SkeletonParagraph IsActive changes.",
            failures);
        Expect(GetInternalProperty<int>(paragraph, "ShimmerBlockCount") == 3,
            "SkeletonParagraph should host exactly the shimmer blocks of its current rows after a rebuild.",
            failures);
        Expect(GetInternalProperty<bool>(paragraph, "IsShimmerRunning"),
            "Active standalone SkeletonParagraph should run its own shimmer.",
            failures);
    }

    private static void VerifyStandaloneLineShimmerLifecycle(ICollection<string> failures)
    {
        var line = new SkeletonLine();
        using var realized = RealizeControl(line);
        Expect(GetInternalProperty<bool>(line, "IsShimmerHost"),
            "Standalone SkeletonLine should be its own shimmer host.",
            failures);
        Expect(!GetInternalProperty<bool>(line, "IsShimmerRunning") &&
               GetInternalProperty<object>(line, "ShimmerVisual") == null,
            "Inactive SkeletonLine should not create a shimmer visual.",
            failures);

        line.IsActive = true;
        RefreshLayout(realized.Window);
        Expect(GetInternalProperty<bool>(line, "IsShimmerRunning"),
            "Activating SkeletonLine should start its shimmer.",
            failures);
        Expect(GetInternalProperty<int>(line, "ShimmerBlockCount") == 1,
            "SkeletonLine should register its content layer as its only shimmer block.",
            failures);

        line.MotionDuration = TimeSpan.Zero;
        RefreshLayout(realized.Window);
        Expect(!GetInternalProperty<bool>(line, "IsShimmerRunning"),
            "SkeletonLine with a zero MotionDuration should stop its shimmer.",
            failures);

        line.MotionDuration = TimeSpan.FromMilliseconds(1234);
        line.IsActive       = false;
        RefreshLayout(realized.Window);
        Expect(!GetInternalProperty<bool>(line, "IsShimmerRunning"),
            "Inactive SkeletonLine should stop its shimmer.",
            failures);
    }

    private static void VerifyCompositeSharesOneShimmer(ICollection<string> failures)
    {
        var skeleton = new Skeleton
        {
            IsActive     = true,
            IsShowAvatar = true
        };
        using var realized = RealizeControl(skeleton);
        Expect(GetInternalProperty<object>(skeleton, "Placeholder") == null,
            "Skeleton that is not loading should not build its placeholder.",
            failures);

        skeleton.IsLoading = true;
        RefreshLayout(realized.Window);
        var skeletons = skeleton.GetSelfAndVisualDescendants().OfType<AbstractSkeleton>().ToList();
        Expect(GetInternalProperty<object>(skeleton, "Placeholder") != null,
            "Loading Skeleton should build its placeholder.",
            failures);
        Expect(skeletons.Count(s => GetInternalProperty<bool>(s, "IsShimmerRunning")) == 1,
            "Loading Skeleton should run exactly one shimmer for all of its blocks.",
            failures);
        Expect(skeletons.Count(s => GetInternalProperty<bool>(s, "IsShimmerHost")) == 1,
            "Only the outer Skeleton should be a shimmer host.",
            failures);
        Expect(GetInternalProperty<int>(skeleton, "ShimmerBlockCount") == 4,
            "Skeleton with avatar, title and two rows should host four shimmer blocks.",
            failures);

        skeleton.IsLoading = false;
        RefreshLayout(realized.Window);
        Expect(GetInternalProperty<object>(skeleton, "Placeholder") == null &&
               !GetInternalProperty<bool>(skeleton, "IsShimmerRunning") &&
               GetInternalProperty<int>(skeleton, "ShimmerBlockCount") == 0,
            "Skeleton should drop its placeholder and stop its shimmer when loading ends.",
            failures);
    }

    // The shimmer state is internal to AtomUI.Controls; read it the way the other suites read private state.
    private static T GetInternalProperty<T>(object target, string propertyName)
    {
        var type = target.GetType();
        while (type is not null)
        {
            var property = type.GetProperty(propertyName,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly);
            if (property is not null)
            {
                var value = property.GetValue(target);
                if (value is null && default(T) is null)
                {
                    return default!;
                }
                if (value is T typed)
                {
                    return typed;
                }
                throw new InvalidOperationException(
                    $"{target.GetType().Name}.{propertyName} is {value?.GetType().Name ?? "null"}, expected {typeof(T).Name}.");
            }

            type = type.BaseType;
        }

        // A renamed or removed internal must fail the verification, not read as a default value.
        throw new InvalidOperationException($"{target.GetType().Name} has no property {propertyName}.");
    }

    private static void ExpectLineWidths(SkeletonParagraph paragraph,
                                         IReadOnlyList<double> expected,
                                         string label,
                                         ICollection<string> failures)
    {
        var lines = GetSkeletonLines(paragraph);
        Expect(lines.Count == expected.Count,
            $"{label}: expected {expected.Count} lines, actual {lines.Count}.",
            failures);

        for (var i = 0; i < Math.Min(lines.Count, expected.Count); i++)
        {
            Expect(lines[i].LineWidth.IsPercentage && Math.Abs(lines[i].LineWidth.Value - expected[i]) < 0.001,
                $"{label}: expected line {i} width {expected[i]}%, actual {lines[i].LineWidth}.",
                failures);
        }
    }

    private static List<SkeletonLine> GetSkeletonLines(Control root)
    {
        return root.GetSelfAndVisualDescendants()
                   .OfType<SkeletonLine>()
                   .Where(line => line.GetType() == typeof(SkeletonLine))
                   .ToList();
    }
}
