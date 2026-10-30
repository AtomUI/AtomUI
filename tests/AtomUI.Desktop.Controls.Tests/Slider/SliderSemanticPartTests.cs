using System.Xml.Linq;
using AtomUI.Theme;
using AtomUI.Theme.Schema;
using AtomUI.Theme.Styling;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomUISlider = AtomUI.Desktop.Controls.Slider;
using AtomUISliderMark = AtomUI.Desktop.Controls.SliderMark;
using AtomUISliderThumb = AtomUI.Desktop.Controls.SliderThumb;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Slider;

public class SliderSemanticPartTests
{
    private const string RailClass   = "semantic-rail";
    private const string TracksClass = "semantic-tracks";
    private const string TrackClass  = "semantic-track";
    private const string HandleClass = "semantic-handle";

    static SliderSemanticPartTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Registered_Descriptors_Expose_Only_The_Approved_Parts()
    {
        var registry = Application.Current.ShouldNotBeNull()
                                  .GetThemeManager().ShouldNotBeNull()
                                  .SemanticParts;

        registry.TryGetControl(typeof(AtomUISlider), out var descriptor).ShouldBeTrue();
        descriptor.ShouldNotBeNull();
        descriptor.Parts.Select(static part => part.Name)
                  .ShouldBe(["root", "handle", "rail", "track", "tracks"]);
        AssertRoot(descriptor.Parts.Single(static part => part.Name == "root"), typeof(AtomUISlider));
        AssertPart(descriptor.Parts.Single(static part => part.Name == "rail"),
            RailClass, typeof(Border), "/template/ .semantic-rail", SemanticPartCardinality.Single);
        AssertPart(descriptor.Parts.Single(static part => part.Name == "tracks"),
            TracksClass, typeof(Border), "/template/ .semantic-tracks", SemanticPartCardinality.Single);
        AssertPart(descriptor.Parts.Single(static part => part.Name == "track"),
            TrackClass, typeof(Border), "/template/ .semantic-track", SemanticPartCardinality.Multiple);
        AssertPart(descriptor.Parts.Single(static part => part.Name == "handle"),
            HandleClass, typeof(AtomUISliderThumb), "/template/ .semantic-handle", SemanticPartCardinality.Multiple);

        // Collaborator types do not publish their own semantic descriptors.
        registry.TryGetControl(typeof(AtomUI.Desktop.Controls.SliderTrack), out _).ShouldBeFalse();
        registry.TryGetControl(typeof(AtomUISliderThumb), out _).ShouldBeFalse();
    }

    [Fact]
    public void Templates_Declare_No_Static_Semantic_Markers()
    {
        AssertThemeMarkers(
            "src/AtomUI.Desktop.Controls/Slider/Themes/SliderTheme.axaml",
            Array.Empty<string>());
        AssertThemeMarkers(
            "src/AtomUI.Desktop.Controls/Slider/Themes/SliderTrackTheme.axaml",
            Array.Empty<string>());
        AssertThemeMarkers(
            "src/AtomUI.Desktop.Controls/Slider/Themes/SliderThumbTheme.axaml",
            Array.Empty<string>());
    }

    [Fact]
    public void Single_Mode_Produces_One_Marker_Per_Part()
    {
        var slider = new AtomUISlider
        {
            Minimum = 0,
            Maximum = 100,
            Value   = 30
        };

        ShowInWindow(slider, () =>
        {
            AssertMarkerCounts(slider, expectedRails: 1, expectedTracks: 1, expectedSegments: 1, expectedHandles: 1);
        });
    }

    [Fact]
    public void Range_Mode_Produces_Markers_Matching_Segment_And_Handle_Counts()
    {
        var slider = new AtomUISlider
        {
            Minimum     = 0,
            Maximum     = 100,
            IsRangeMode = true,
            RangeValues = [20, 30, 50]
        };

        ShowInWindow(slider, () =>
        {
            AssertMarkerCounts(slider, expectedRails: 1, expectedTracks: 1, expectedSegments: 2, expectedHandles: 3);
        });
    }

    [Fact]
    public void Range_Values_And_Mode_Changes_Sync_Runtime_Nodes()
    {
        var slider = new AtomUISlider
        {
            Minimum     = 0,
            Maximum     = 100,
            IsRangeMode = true,
            RangeValues = [20, 30, 50]
        };

        ShowInWindow(slider, () =>
        {
            AssertMarkerCounts(slider, expectedRails: 1, expectedTracks: 1, expectedSegments: 2, expectedHandles: 3);

            slider.RangeValues = [20, 50];
            Dispatcher.UIThread.RunJobs();
            AssertMarkerCounts(slider, expectedRails: 1, expectedTracks: 1, expectedSegments: 1, expectedHandles: 2);

            slider.IsRangeMode = false;
            slider.Value       = 40;
            Dispatcher.UIThread.RunJobs();
            AssertMarkerCounts(slider, expectedRails: 1, expectedTracks: 1, expectedSegments: 1, expectedHandles: 1);
        });
    }

    [Fact]
    public void IsIncluded_False_Hides_Tracks_And_Segments_But_Keeps_Markers()
    {
        var slider = new AtomUISlider
        {
            Minimum     = 0,
            Maximum     = 100,
            IsRangeMode = true,
            RangeValues = [20, 80],
            IsIncluded  = false
        };

        ShowInWindow(slider, () =>
        {
            GetSemanticElements(slider, RailClass).Single().IsVisible.ShouldBeTrue();
            GetSemanticElements(slider, TracksClass).Single().IsVisible.ShouldBeFalse();
            GetSemanticElements(slider, TrackClass).ShouldAllBe(static segment => !segment.IsVisible);
            GetSemanticElements(slider, HandleClass).ShouldAllBe(static handle => handle.IsVisible);
        });
    }

    [Fact]
    public void Single_Mode_Tracks_Span_Covers_Minimum_To_Value()
    {
        var slider = new AtomUISlider
        {
            Minimum = 0,
            Maximum = 100,
            Value   = 30
        };

        ShowInWindow(slider, () =>
        {
            var rail   = GetSemanticElements(slider, RailClass).Single();
            var tracks = GetSemanticElements(slider, TracksClass).Single();
            var track  = GetSemanticElements(slider, TrackClass).Single();

            rail.Bounds.Width.ShouldBeGreaterThan(0);
            // The tracks container and the single track segment both cover Minimum → Value.
            tracks.Bounds.Position.ShouldBe(track.Bounds.Position);
            tracks.Bounds.Size.ShouldBe(track.Bounds.Size);
            // Geometry spans the same rail: value ratio 30% of the rail length.
            (track.Bounds.Width / rail.Bounds.Width).ShouldBe(0.3, tolerance: 0.01);
            track.Bounds.Y.ShouldBe(rail.Bounds.Y);
            track.Bounds.Height.ShouldBe(rail.Bounds.Height);
        });
    }

    [Fact]
    public void TracksBrush_In_Single_Mode_Applies_To_The_Tracks_Element()
    {
        var tracksBrush = new SolidColorBrush(Colors.Purple);
        var slider = new AtomUISlider
        {
            Minimum     = 0,
            Maximum     = 100,
            Value       = 30,
            TracksBrush = tracksBrush
        };

        ShowInWindow(slider, () =>
        {
            GetSemanticElements(slider, TracksClass)
                .Single().ShouldBeOfType<Border>().Background.ShouldBe(tracksBrush);
        });
    }

    [Fact]
    public void Generated_Route_Styles_Apply_Real_Setters_Over_Default_Theme_Values()
    {
        var slider = new AtomUISlider
        {
            Minimum = 0,
            Maximum = 100,
            Value   = 30
        };
        var railBrush   = new SolidColorBrush(Colors.Orange);
        var tracksBrush = new SolidColorBrush(Colors.MediumSeaGreen);
        var trackBrush  = new SolidColorBrush(Colors.SteelBlue);
        var handleBrush = new SolidColorBrush(Colors.Purple);

        var ownerStyle = new Style(selector => selector.OfType<AtomUISlider>());
        ownerStyle.Children.Add(new SliderRailStyle
        {
            Setters = { new Setter(Border.BackgroundProperty, railBrush) }
        });
        ownerStyle.Children.Add(new SliderTracksStyle
        {
            Setters = { new Setter(Border.BackgroundProperty, tracksBrush) }
        });
        ownerStyle.Children.Add(new SliderTrackStyle
        {
            Setters = { new Setter(Border.BackgroundProperty, trackBrush) }
        });
        ownerStyle.Children.Add(new SliderHandleStyle
        {
            Setters = { new Setter(TemplatedControl.BorderBrushProperty, handleBrush) }
        });
        slider.Styles.Add(ownerStyle);

        ShowInWindow(slider, () =>
        {
            GetSemanticElements(slider, RailClass).Single().ShouldBeOfType<Border>().Background.ShouldBe(railBrush);
            GetSemanticElements(slider, TracksClass).Single().ShouldBeOfType<Border>().Background.ShouldBe(tracksBrush);
            GetSemanticElements(slider, TrackClass).Single().ShouldBeOfType<Border>().Background.ShouldBe(trackBrush);
            GetSemanticElements(slider, HandleClass)
                .OfType<AtomUISliderThumb>()
                .ShouldAllBe(thumb => thumb.BorderBrush == handleBrush);
        });
    }

    [Fact]
    public void Default_Theme_Values_Flow_Into_The_Runtime_Part_Elements()
    {
        var slider = new AtomUISlider
        {
            Minimum = 0,
            Maximum = 100,
            Value   = 30
        };

        ShowInWindow(slider, () =>
        {
            GetSemanticElements(slider, RailClass).Single().ShouldBeOfType<Border>().Background.ShouldNotBeNull();
            GetSemanticElements(slider, TrackClass).Single().ShouldBeOfType<Border>().Background.ShouldNotBeNull();
            // No TracksBrush by default: the container stays transparent.
            GetSemanticElements(slider, TracksClass).Single().ShouldBeOfType<Border>().Background.ShouldBeNull();
        });
    }

    [Fact]
    public void Marks_And_Labels_Carry_No_Semantic_Markers()
    {
        var slider = new AtomUISlider
        {
            Minimum = 0,
            Maximum = 100,
            Value   = 30,
            Marks   = [new AtomUISliderMark("25", 25), new AtomUISliderMark("75", 75)]
        };

        ShowInWindow(slider, () =>
        {
            var semanticNodes = slider.GetVisualDescendants()
                                      .Where(static visual =>
                                          visual.Classes.Any(static cls => cls.StartsWith("semantic-", StringComparison.Ordinal)))
                                      .ToArray();
            semanticNodes.Length.ShouldBe(4);
            AssertMarkerCounts(slider, expectedRails: 1, expectedTracks: 1, expectedSegments: 1, expectedHandles: 1);
        });
    }

    private static void AssertMarkerCounts(
        AtomUISlider slider,
        int expectedRails,
        int expectedTracks,
        int expectedSegments,
        int expectedHandles)
    {
        GetSemanticElements(slider, RailClass).Length.ShouldBe(expectedRails);
        GetSemanticElements(slider, TracksClass).Length.ShouldBe(expectedTracks);
        GetSemanticElements(slider, TrackClass).Length.ShouldBe(expectedSegments);
        GetSemanticElements(slider, HandleClass).Length.ShouldBe(expectedHandles);
    }

    private static Visual[] GetSemanticElements(AtomUISlider slider, string semanticClass)
    {
        return slider.GetVisualDescendants()
                     .Where(visual => visual.Classes.Contains(semanticClass))
                     .ToArray();
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

    private static void AssertRoot(SemanticPartDescriptor part, Type contractType)
    {
        part.Path.ShouldBe("root");
        part.SelectorClass.ShouldBeNull();
        part.ContractType.ShouldBe(contractType);
        part.Cardinality.ShouldBe(SemanticPartCardinality.Single);
        part.Customization.ShouldBe(SemanticPartCustomization.Root);
        part.CrossVisualRoot.ShouldBeFalse();
        part.RuntimeCreated.ShouldBeFalse();
    }

    private static void AssertPart(
        SemanticPartDescriptor part,
        string selectorClass,
        Type contractType,
        string selectorRoute,
        SemanticPartCardinality cardinality)
    {
        part.Path.ShouldBe(part.Name);
        part.SelectorClass.ShouldBe(selectorClass);
        part.SelectorRoute.ShouldBe(selectorRoute);
        part.ContractType.ShouldBe(contractType);
        part.Cardinality.ShouldBe(cardinality);
        part.Customization.ShouldBe(SemanticPartCustomization.Selector);
        part.CrossVisualRoot.ShouldBeFalse();
        part.RuntimeCreated.ShouldBeTrue();
        part.Since.ShouldBe("6.2.0");
    }

    private static void AssertThemeMarkers(string relativePath, string[] expectedMarkers)
    {
        var document = XDocument.Load(GetRepoFile(relativePath), LoadOptions.SetLineInfo);
        var literalSemanticMarkers = document.Descendants()
                                             .Attributes()
                                             .Where(static attribute =>
                                                 attribute.Name.LocalName == "Classes" &&
                                                 attribute.Value.Split(
                                                             (char[]?)null,
                                                             StringSplitOptions.RemoveEmptyEntries)
                                                         .Any(static value => value.StartsWith(
                                                             "semantic-",
                                                             StringComparison.Ordinal)))
                                             .ToArray();
        var classPropertyMarkers = document.Descendants()
                                           .SelectMany(static element => element.Attributes()
                                               .Where(static attribute => attribute.Name.LocalName.StartsWith(
                                                   "Classes.semantic-",
                                                   StringComparison.Ordinal))
                                               .Select(attribute => (Element: element, Attribute: attribute)))
                                           .ToArray();

        literalSemanticMarkers.ShouldBeEmpty();
        classPropertyMarkers.ShouldAllBe(static marker =>
            string.Equals(marker.Attribute.Value, "true", StringComparison.OrdinalIgnoreCase));
        classPropertyMarkers.Select(static marker =>
                                $"{marker.Attribute.Name.LocalName["Classes.".Length..]}:{marker.Element.Name.LocalName}")
                            .OrderBy(static value => value, StringComparer.Ordinal)
                            .ShouldBe(expectedMarkers);
    }

    private static string GetRepoFile(string relativePath)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, relativePath);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException($"Could not locate repository file '{relativePath}'.");
    }
}
