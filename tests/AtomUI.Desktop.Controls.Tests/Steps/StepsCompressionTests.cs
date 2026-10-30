using AtomUI.Desktop.Controls.DesignTokens;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Layout;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomUISizeType = AtomUI.SizeType;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Steps;

public class StepsCompressionTests
{
    static StepsCompressionTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Horizontal_Narrow_Container_Compresses_Items_To_The_Available_Width()
    {
        var steps = CreateNarrowSteps(320);

        ShowInWindow(steps, () =>
        {
            var items         = steps.Items.Cast<Desktop.Controls.StepsItem>().ToArray();
            var bounds        = items.Select(static item => item.Bounds).ToArray();
            var shareWidths   = items.Select(static item => item.DesiredSize.Width).ToArray();
            var naturalWidths = items.Select(MeasureNaturalWidth).ToArray();

            // The share algorithm is exact: measured item widths fill the container.
            shareWidths.Sum().ShouldBe(320d, 0.01);

            // Bounds go through Avalonia layout rounding (positions to nearest,
            // sizes up), so the arranged right edge may drift a couple of pixels.
            bounds[^1].Right.ShouldBe(320d, 1.75);

            for (var index = 0; index < items.Length; ++index)
            {
                bounds[index].Width.ShouldBeLessThan(naturalWidths[index]);
            }
        });
    }

    [Fact]
    public void Horizontal_Narrow_Container_Wraps_Title_And_Content_Text_Instead_Of_Clipping()
    {
        var steps = CreateNarrowSteps(320);

        ShowInWindow(steps, () =>
        {
            foreach (var item in steps.Items.Cast<Desktop.Controls.StepsItem>())
            {
                AssertWrappedToMultipleLines(FindControl(item, "HeaderPresenter"), 0.5);
                AssertWrappedToMultipleLines(FindControl(item, "ContentPresenter"), 0.6);
            }
        });
    }

    [Fact]
    public void Horizontal_Extremely_Narrow_Container_Stops_Items_At_The_Icon_Floor()
    {
        var iconSize = GetThemeResource<double>(StepsTokenKind.IconContainerSize);
        iconSize.ShouldBeGreaterThan(0);
        var steps = CreateUnevenSteps(iconSize * 3);

        ShowInWindow(steps, () =>
        {
            var widths = steps.Items.Cast<Desktop.Controls.StepsItem>()
                                .Select(static item => item.Bounds.Width)
                                .ToArray();

            widths.ShouldAllBe(width => Math.Abs(width - iconSize) < 0.01);
            widths.Sum().ShouldBe(iconSize * 3, 0.01);
        });
    }

    [Fact]
    public void Horizontal_Compressed_Items_Never_Render_Rails_Past_Their_Bounds()
    {
        var steps = CreateNarrowSteps(320);

        ShowInWindow(steps, () =>
        {
            foreach (var item in steps.Items.Cast<Desktop.Controls.StepsItem>().Where(static item => !item.IsLast))
            {
                var connector = FindControl(item, "Connector");
                connector.IsVisible.ShouldBeTrue();

                // The rail either collapses to zero width or stays inside the item.
                (connector.Bounds.Width == 0 || connector.Bounds.Right <= item.Bounds.Right + 0.01)
                    .ShouldBeTrue();
            }
        });
    }

    [Fact]
    public void Horizontal_Compressed_Items_Wrap_Content_When_Arranged_Narrower_Than_The_Measured_Text()
    {
        // Regression: the horizontal-title item measured its body at the full share
        // width but arranged it at share minus the icon area, so a share just above
        // the text's natural width produced a single-line layout that was then
        // clipped instead of wrapped. Every compressed layout must wrap the text,
        // never clip it.
        var steps = CreateUniformShortSteps();

        ShowInWindow(steps, () =>
        {
            var presenter = FindControl(
                steps.Items[0].ShouldBeOfType<Desktop.Controls.StepsItem>(),
                "ContentPresenter");
            presenter.Measure(Size.Infinity);
            var text = presenter.GetVisualDescendants().OfType<Avalonia.Controls.TextBlock>().First();
            // The fixture text must stay on one line when unconstrained, so the
            // assertion below measures an actual wrap.
            text.TextLayout!.TextLines.Count.ShouldBe(1);
            var singleLineWidth  = text.DesiredSize.Width;
            var singleLineHeight = text.DesiredSize.Height;

            steps.Width = double.NaN;
            steps.Measure(Size.Infinity);
            var naturalWidth = steps.DesiredSize.Width;

            foreach (var factor in new[]
                     {
                         0.95, 0.9, 0.85, 0.8, 0.75, 0.7, 0.65, 0.6, 0.55, 0.5
                     })
            {
                steps.Width = naturalWidth * factor;
                Relayout();

                foreach (var item in steps.Items.Cast<Desktop.Controls.StepsItem>())
                {
                    var bounds = FindControl(item, "ContentPresenter").Bounds;
                    if (bounds.Width < singleLineWidth - 0.5)
                    {
                        // Arranging below the natural single-line width must wrap the
                        // text into extra lines instead of clipping it to one line.
                        bounds.Height.ShouldBeGreaterThan(singleLineHeight + 0.5);
                    }
                }
            }
        });
    }

    [Fact]
    public void Horizontal_Compressed_Items_Wrap_SubHeader_Onto_Its_Own_Line_Instead_Of_Clipping()
    {
        // Regression: the heading row placed the SubHeader beside the Header but
        // measured both at the full body width, so a body narrower than
        // Header+SubHeader arranged the subheader at the leftover width and clipped
        // it ("00:0"). The subheader must flow onto its own line below the header
        // whenever the body cannot host both side by side, and it must never be
        // clipped while the body is wide enough to host it alone.
        var steps = CreateUniformLongContentSteps();

        ShowInWindow(steps, () =>
        {
            var firstItem = steps.Items[0].ShouldBeOfType<Desktop.Controls.StepsItem>();
            var headerPresenter = FindControl(firstItem, "HeaderPresenter");
            var subHeaderPresenter = FindControl(firstItem, "SubHeaderPresenter");

            headerPresenter.Measure(Size.Infinity);
            var headerSingleLineWidth = headerPresenter.DesiredSize.Width;
            // The presenter bounds exclude its left margin, so the no-clip baseline
            // is the inner text's natural width, not the presenter's measured size.
            var subHeaderText = subHeaderPresenter.GetVisualDescendants()
                                                  .OfType<Avalonia.Controls.TextBlock>()
                                                  .First();
            subHeaderText.Measure(Size.Infinity);
            subHeaderText.TextLayout!.TextLines.Count.ShouldBe(1);
            var subHeaderTextSingleLineWidth = subHeaderText.DesiredSize.Width;
            subHeaderPresenter.Measure(Size.Infinity);
            var subHeaderSingleLineWidth = subHeaderPresenter.DesiredSize.Width;
            var descriptionMaxWidth = GetThemeResource<double>(StepsTokenKind.DescriptionMaxWidth);

            steps.Width = double.NaN;
            steps.Measure(Size.Infinity);
            var naturalWidth = steps.DesiredSize.Width;

            var observedStackedWrap = false;
            foreach (var factor in new[]
                     {
                         0.95, 0.9, 0.85, 0.8, 0.75, 0.7, 0.65, 0.6, 0.55, 0.5, 0.45,
                         0.4, 0.35, 0.3, 0.25, 0.2, 0.18, 0.16, 0.15, 0.14, 0.13, 0.12,
                         0.11, 0.1, 0.09, 0.08, 0.07, 0.06, 0.05
                     })
            {
                steps.Width = naturalWidth * factor;
                Relayout();

                foreach (var item in steps.Items.Cast<Desktop.Controls.StepsItem>())
                {
                    var headerBounds = FindControl(item, "HeaderPresenter").Bounds;
                    var itemSubHeaderPresenter = FindControl(item, "SubHeaderPresenter");
                    var subHeaderBounds = itemSubHeaderPresenter.Bounds;
                    var itemSubHeaderText = itemSubHeaderPresenter.GetVisualDescendants()
                                                                 .OfType<Avalonia.Controls.TextBlock>()
                                                                 .First();
                    // The long content wraps under compression, so the content
                    // presenter width equals the body width while it stays below the
                    // DescriptionMaxWidth cap.
                    var bodyWidth = FindControl(item, "ContentPresenter").Bounds.Width;

                    if (bodyWidth >= subHeaderTextSingleLineWidth - 0.5)
                    {
                        // The body can host the subheader on its own line, so the
                        // subheader must never be clipped: a single-line layout keeps
                        // its full natural width, a wrapped layout keeps its full
                        // wrapped height.
                        if (itemSubHeaderText.TextLayout!.TextLines.Count == 1)
                        {
                            subHeaderBounds.Width.ShouldBeGreaterThanOrEqualTo(subHeaderTextSingleLineWidth - 0.5);
                        }
                        else
                        {
                            subHeaderBounds.Height.ShouldBeGreaterThanOrEqualTo(itemSubHeaderText.DesiredSize.Height - 0.5);
                        }

                        if (bodyWidth < descriptionMaxWidth - 0.5 &&
                            bodyWidth < headerSingleLineWidth + subHeaderSingleLineWidth - 0.5)
                        {
                            // Not enough room beside the header: the subheader wraps
                            // onto its own line below it.
                            subHeaderBounds.Y.ShouldBeGreaterThanOrEqualTo(headerBounds.Bottom - 0.5);
                            observedStackedWrap = true;
                        }
                    }
                }
            }

            observedStackedWrap.ShouldBeTrue(
                "the sweep must include at least one width where the subheader wraps onto its own line");
        });
    }

    private static Desktop.Controls.Steps CreateNarrowSteps(double width)
    {
        var steps = new Desktop.Controls.Steps
        {
            Width          = width,
            Current        = 0,
            Type           = Desktop.Controls.StepsType.Default,
            Orientation    = Orientation.Horizontal,
            TitlePlacement = Orientation.Horizontal,
            SizeType       = AtomUISizeType.Middle
        };
        steps.Items.Add(new Desktop.Controls.StepsItem
        {
            Header    = "Step one of an onboarding flow with a deliberately long title",
            SubHeader = "00:00",
            Content   = "A content description that is long enough to wrap across several lines when the container is narrow."
        });
        steps.Items.Add(new Desktop.Controls.StepsItem
        {
            Header    = "Step two of the same flow with another deliberately long title",
            SubHeader = "00:01",
            Content   = "Another content description with a very similar length so all items compress at the same pace."
        });
        steps.Items.Add(new Desktop.Controls.StepsItem
        {
            Header    = "Step three with a title kept at a comparable natural length",
            SubHeader = "00:02",
            Content   = "A third description of a matching length to keep the shared compression symmetric."
        });
        return steps;
    }

    private static Desktop.Controls.Steps CreateUnevenSteps(double width)
    {
        var steps = new Desktop.Controls.Steps
        {
            Width          = width,
            Current        = 0,
            Type           = Desktop.Controls.StepsType.Default,
            Orientation    = Orientation.Horizontal,
            TitlePlacement = Orientation.Horizontal,
            SizeType       = AtomUISizeType.Middle
        };
        steps.Items.Add(new Desktop.Controls.StepsItem
        {
            Header    = "Step 1",
            SubHeader = "00:00",
            Content   = "A concise description."
        });
        steps.Items.Add(new Desktop.Controls.StepsItem
        {
            Header    = "Step 2 of a very long onboarding flow that needs far more space than this extremely narrow container offers",
            SubHeader = "00:01",
            Content   = "This is a content description that is deliberately long enough to wrap across many lines when the available width shrinks far below the natural text width."
        });
        steps.Items.Add(new Desktop.Controls.StepsItem
        {
            Header    = "Step 3 with a moderately long title text",
            SubHeader = "00:02",
            Content   = "A medium length description that will still wrap when the container compresses."
        });
        return steps;
    }

    private static Desktop.Controls.Steps CreateUniformShortSteps()
    {
        var steps = new Desktop.Controls.Steps
        {
            Width          = 320,
            Current        = 0,
            Type           = Desktop.Controls.StepsType.Default,
            Orientation    = Orientation.Horizontal,
            TitlePlacement = Orientation.Horizontal,
            SizeType       = AtomUISizeType.Middle
        };
        for (var index = 1; index <= 3; ++index)
        {
            steps.Items.Add(new Desktop.Controls.StepsItem
            {
                Header    = $"Step {index}",
                SubHeader = $"00:0{index - 1}",
                Content   = "Note."
            });
        }
        return steps;
    }

    private static Desktop.Controls.Steps CreateUniformLongContentSteps()
    {
        var steps = new Desktop.Controls.Steps
        {
            Width          = 320,
            Current        = 0,
            Type           = Desktop.Controls.StepsType.Default,
            Orientation    = Orientation.Horizontal,
            TitlePlacement = Orientation.Horizontal,
            SizeType       = AtomUISizeType.Middle
        };
        for (var index = 1; index <= 3; ++index)
        {
            steps.Items.Add(new Desktop.Controls.StepsItem
            {
                Header    = $"Step {index}",
                SubHeader = $"00:0{index - 1}",
                Content   = "This is a content description that is long enough to wrap."
            });
        }
        return steps;
    }

    private static double MeasureNaturalWidth(Control control)
    {
        control.Measure(Size.Infinity);
        return control.DesiredSize.Width;
    }

    private static void AssertWrappedToMultipleLines(Control presenter, double maxWidthFactor)
    {
        var bounds = presenter.Bounds;
        var text = presenter.GetVisualDescendants().OfType<Avalonia.Controls.TextBlock>().First();
        var wrappedLines = text.TextLayout!.TextLines.Count;
        text.Measure(Size.Infinity);
        var singleLine = text.DesiredSize;

        // The presenter must be arranged at the compressed item width and wrap the
        // text into additional lines, rather than staying at its natural size.
        wrappedLines.ShouldBeGreaterThan(1);
        bounds.Height.ShouldBeGreaterThan(singleLine.Height);
        bounds.Width.ShouldBeLessThan(singleLine.Width * maxWidthFactor);
    }

    private static Control FindControl(Desktop.Controls.StepsItem item, string name)
    {
        return item.GetVisualDescendants().OfType<Control>().Single(control => control.Name == name);
    }

    private static T GetThemeResource<T>(object key)
    {
        var application = Application.Current.ShouldNotBeNull();
        application.TryGetResource(key, application.ActualThemeVariant, out var value).ShouldBeTrue();
        value.ShouldBeAssignableTo<T>();
        return (T)value!;
    }

    private static void Relayout()
    {
        Dispatcher.UIThread.RunJobs();
        AvaloniaHeadlessPlatform.ForceRenderTimerTick(1);
        Dispatcher.UIThread.RunJobs();
    }

    private static void ShowInWindow(Control content, Action assertion)
    {
        // Keep the window tall enough that compressed multi-line headings never hit
        // the vertical clamp: TextBlock re-lays its text at the arranged rect, so a
        // height-clamped item would clip its wrapped lines back to one.
        var window = new AvaloniaWindow
        {
            Width   = 900,
            Height  = 2000,
            Content = content
        };

        try
        {
            window.Show();
            Relayout();
            assertion();
        }
        finally
        {
            window.Close();
        }
    }
}
