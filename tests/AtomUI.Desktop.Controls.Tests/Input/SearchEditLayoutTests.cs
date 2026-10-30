using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomUISearchEdit = AtomUI.Desktop.Controls.SearchEdit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Input;

public class SearchEditLayoutTests
{
    static SearchEditLayoutTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Search_Button_Is_A_Public_Button_With_A_Replaceable_Semantic_Part_Theme()
    {
        var customTheme = new ControlTheme(typeof(global::AtomUI.Desktop.Controls.Button));
        var searchEdit = new AtomUISearchEdit
        {
            Width = 360,
            SearchButtonText = "Search",
            SearchButtonTheme = customTheme
        };

        ShowInWindow(searchEdit, () =>
        {
            var searchButton = FindTemplatePart<global::AtomUI.Desktop.Controls.Button>(
                searchEdit,
                "PART_RightAddOn");

            searchButton.GetType().ShouldBe(typeof(global::AtomUI.Desktop.Controls.Button));
            searchButton.Theme.ShouldBeSameAs(customTheme);
        });
    }

    [Fact]
    public void Search_Button_Theme_Contract_Has_No_Internal_Control_Or_Borrowed_LineEdit_Identity()
    {
        var searchEditSource = ReadRepoFile("src/AtomUI.Desktop.Controls/Input/SearchEdit.cs");
        var decoratedBoxTheme = ReadRepoFile(
            "src/AtomUI.Desktop.Controls/Input/Themes/SearchEditDecoratedBoxTheme.axaml");
        var searchButtonTheme = ReadRepoFile(
            "src/AtomUI.Desktop.Controls/Input/Themes/SearchButtonTheme.axaml");

        searchEditSource.ShouldContain("StyledProperty<ControlTheme?> SearchButtonThemeProperty");
        searchEditSource.ShouldContain("public ControlTheme? SearchButtonTheme");
        decoratedBoxTheme.ShouldContain("<atom:Button");
        decoratedBoxTheme.ShouldContain("Theme=\"{TemplateBinding SearchButtonTheme}\"");
        searchButtonTheme.ShouldContain("TargetType=\"{x:Type atom:Button}\"");
        searchButtonTheme.ShouldContain("ButtonTokenResource");
        searchButtonTheme.ShouldContain("SearchEditTokenResource");
        searchButtonTheme.ShouldNotContain("LineEditTokenResource");
    }

    [Fact]
    public void Custom_Size_Search_Button_Frame_Matches_Input_Frame_Height()
    {
        var searchEdit = new AtomUISearchEdit
        {
            Width      = 360,
            Height     = 38,
            FontSize   = 15,
            SizeType   = CustomizableSizeType.Custom,
            SearchButtonText = "Search"
        };

        ShowInWindow(searchEdit, () =>
        {
            var contentFrame = FindTemplatePart<global::AtomUI.Desktop.Controls.AddOnDecoratedBoxContentFrame>(
                searchEdit,
                "PART_ContentFrame");
            var searchButton = FindTemplatePart<global::AtomUI.Desktop.Controls.Button>(searchEdit, "PART_RightAddOn");
            var buttonFrame  = FindTemplatePart<Control>(searchButton, "Frame");

            contentFrame.Bounds.Height.ShouldBe(38, 0.5);
            buttonFrame.Bounds.Height.ShouldBe(contentFrame.Bounds.Height, 0.5);
        });
    }

    [Fact]
    public void Built_In_Size_Search_Button_Frame_Matches_Input_Frame_Height()
    {
        var sizeTypes = new[]
        {
            CustomizableSizeType.Large,
            CustomizableSizeType.Middle,
            CustomizableSizeType.Small
        };

        foreach (var sizeType in sizeTypes)
        {
            var searchEdit = new AtomUISearchEdit
            {
                Width            = 360,
                SizeType         = sizeType,
                SearchButtonText = "Search"
            };

            ShowInWindow(searchEdit, () =>
            {
                var contentFrame = FindTemplatePart<global::AtomUI.Desktop.Controls.AddOnDecoratedBoxContentFrame>(
                    searchEdit,
                    "PART_ContentFrame");
                var searchButton = FindTemplatePart<global::AtomUI.Desktop.Controls.Button>(searchEdit, "PART_RightAddOn");
                var buttonFrame  = FindTemplatePart<Control>(searchButton, "Frame");

                contentFrame.Bounds.Height.ShouldBeGreaterThan(0);
                searchButton.Bounds.Height.ShouldBe(contentFrame.Bounds.Height, 0.5);
                buttonFrame.Bounds.Height.ShouldBe(contentFrame.Bounds.Height, 0.5);
            });
        }
    }

    [Fact]
    public void Content_Frame_Uses_AddOnDecoratedBox_Rendering_Frame()
    {
        var searchEdit = new AtomUISearchEdit
        {
            Width            = 360,
            SearchButtonText = "Search"
        };

        ShowInWindow(searchEdit, () =>
        {
            var contentFrame = FindTemplatePart<global::AtomUI.Desktop.Controls.AddOnDecoratedBoxContentFrame>(
                searchEdit,
                "PART_ContentFrame");

            contentFrame.UseLayoutRounding.ShouldBeTrue();
        });
    }

    [Fact]
    public void Content_Frame_And_Search_Button_Overlap_Uses_Render_Scale_Aware_Thickness()
    {
        var searchEdit = new AtomUISearchEdit
        {
            Width            = 360,
            SearchButtonText = "Search"
        };

        ShowInWindow(searchEdit, window =>
        {
            window.SetRenderScaling(1.5);
            Dispatcher.UIThread.RunJobs();

            var contentFrame = FindTemplatePart<global::AtomUI.Desktop.Controls.AddOnDecoratedBoxContentFrame>(
                searchEdit,
                "PART_ContentFrame");
            var searchButton = FindTemplatePart<global::AtomUI.Desktop.Controls.Button>(searchEdit, "PART_RightAddOn");
            var sharedBorderOverlap = contentFrame.Bounds.Right - searchButton.Bounds.Left;

            sharedBorderOverlap.ShouldBe(2d / 3d, 0.001);
        });
    }

    [Fact]
    public void Narrow_Width_Constrains_All_Segments_To_Valid_Arrange_Rectangles()
    {
        var searchEdit = new AtomUISearchEdit
        {
            Width            = 360,
            LeftAddOn        = "https://",
            SearchButtonText = "Search"
        };

        ShowInWindow(searchEdit, () =>
        {
            var panel = searchEdit.GetVisualDescendants()
                                  .OfType<global::AtomUI.Desktop.Controls.SearchEditPanel>()
                                  .Single();
            var leftAddOn = FindTemplatePart<Control>(searchEdit, "PART_LeftAddOn");
            var contentFrame = FindTemplatePart<global::AtomUI.Desktop.Controls.AddOnDecoratedBoxContentFrame>(
                searchEdit,
                "PART_ContentFrame");
            var searchButton = FindTemplatePart<global::AtomUI.Desktop.Controls.Button>(searchEdit, "PART_RightAddOn");

            searchEdit.Width = 20;
            Dispatcher.UIThread.RunJobs();

            AssertValidBounds(panel.Bounds);
            AssertValidBounds(leftAddOn.Bounds);
            AssertValidBounds(contentFrame.Bounds);
            AssertValidBounds(searchButton.Bounds);

            searchButton.Bounds.X.ShouldBeGreaterThanOrEqualTo(0);
            searchButton.Bounds.Right.ShouldBeLessThanOrEqualTo(panel.Bounds.Width);
            leftAddOn.Bounds.Right.ShouldBeLessThanOrEqualTo(searchButton.Bounds.Left);
            contentFrame.Bounds.Right.ShouldBeLessThanOrEqualTo(panel.Bounds.Width);
        });
    }

    private static void AssertValidBounds(Rect bounds)
    {
        double.IsFinite(bounds.X).ShouldBeTrue();
        double.IsFinite(bounds.Y).ShouldBeTrue();
        double.IsFinite(bounds.Width).ShouldBeTrue();
        double.IsFinite(bounds.Height).ShouldBeTrue();
        bounds.Width.ShouldBeGreaterThanOrEqualTo(0);
        bounds.Height.ShouldBeGreaterThanOrEqualTo(0);
    }

    private static T FindTemplatePart<T>(Control control, string name)
        where T : Control
    {
        var part = control.GetVisualDescendants()
                          .OfType<T>()
                          .SingleOrDefault(item => item.Name == name);
        part.ShouldNotBeNull();
        return part!;
    }

    private static string ReadRepoFile(string relativePath)
    {
        return File.ReadAllText(GetRepoFile(relativePath));
    }

    private static string GetRepoFile(string relativePath)
    {
        var directory = AppContext.BaseDirectory;
        while (directory is not null)
        {
            var candidate = Path.Combine(directory, relativePath);
            if (File.Exists(candidate) || Directory.Exists(candidate))
            {
                return candidate;
            }

            directory = Directory.GetParent(directory)?.FullName;
        }

        return Path.Combine(AppContext.BaseDirectory, relativePath);
    }

    private static void ShowInWindow(Control content, Action assertion)
    {
        ShowInWindow(content, _ => assertion());
    }

    private static void ShowInWindow(Control content, Action<AvaloniaWindow> assertion)
    {
        var window = new AvaloniaWindow
        {
            Width   = 480,
            Height  = 160,
            Content = content
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            assertion(window);
        }
        finally
        {
            window.Close();
        }
    }
}
