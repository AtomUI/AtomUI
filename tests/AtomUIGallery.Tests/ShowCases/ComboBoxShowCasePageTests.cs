using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using AtomUI.Controls.Primitives;
using AtomUI.Desktop.Controls;
using AtomUI.Toolkits.GalleryBase.Controls;
using AtomUIGallery.ShowCases.ComboBox;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using ReactiveUI;
using Shouldly;
using Xunit;
using AtomUIComboBox = AtomUI.Desktop.Controls.ComboBox;
using AvaloniaTextBox = Avalonia.Controls.TextBox;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUIGallery.Tests.ShowCases;

public class ComboBoxShowCasePageTests
{

    [Fact]
    public void ComboBox_ShowCase_Basic_DropDown_Presents_Item_Rows()
    {
        AvaloniaTestApp.EnsureInitialized();
        var oldDeferredLoadingDisabled = GalleryShowCaseRuntimeOptions.IsDeferredLoadingDisabled;
        try
        {
            GalleryShowCaseRuntimeOptions.IsDeferredLoadingDisabled = true;
            var page = new ComboBoxShowCase
            {
                DataContext = new ComboBoxViewModel(new TestScreen())
            };

            ShowInWindow(page, window =>
            {
                var basicComboBox = page.GetVisualDescendants()
                                        .OfType<AtomUIComboBox>()
                                        .First(comboBox => comboBox.Items.Count > 10);

                basicComboBox.IsDropDownOpen = true;
                Dispatcher.UIThread.RunJobs();

                var popupItems = window.GetVisualDescendants()
                                       .OfType<AtomUI.Desktop.Controls.ComboBoxItem>()
                                       .Where(item => item.Content is string)
                                       .ToList();

                popupItems.Count.ShouldBeGreaterThan(
                    0,
                    "The Basic ComboBox popup must render item rows in the real Gallery showcase, not an empty popup strip.");

                var popupFrame = GetVisualDescendant<Border>(window, "PopupFrame");
                popupFrame.Bounds.Height.ShouldBeGreaterThan(
                    96,
                    "The Basic ComboBox popup must not be clipped to the popup padding in the real Gallery showcase.");
            });
        }
        finally
        {
            GalleryShowCaseRuntimeOptions.IsDeferredLoadingDisabled = oldDeferredLoadingDisabled;
        }
    }

    [Fact]
    public void ComboBox_ShowCase_Editable_Filter_DropDown_Presents_Candidate_Rows()
    {
        AvaloniaTestApp.EnsureInitialized();
        var oldDeferredLoadingDisabled = GalleryShowCaseRuntimeOptions.IsDeferredLoadingDisabled;
        try
        {
            GalleryShowCaseRuntimeOptions.IsDeferredLoadingDisabled = true;
            var page = new ComboBoxShowCase
            {
                DataContext = new ComboBoxViewModel(new TestScreen())
            };

            ShowInWindow(page, window =>
            {
                var editableComboBox = page.GetVisualDescendants()
                                           .OfType<AtomUIComboBox>()
                                           .First(comboBox => comboBox.IsEditable && comboBox.IsFilterEnabled);

                editableComboBox.Text           = "Al";
                editableComboBox.IsDropDownOpen = true;
                Dispatcher.UIThread.RunJobs();

                var popupFrame = GetVisualDescendant<Border>(window, "PopupFrame");
                var popupItems = popupFrame.GetVisualDescendants()
                                       .OfType<AtomUI.Desktop.Controls.ComboBoxItem>()
                                       .Where(item => item.IsVisible && item.Content is string)
                                       .Select(item => item.Content)
                                       .ToList();

                popupItems.ShouldBe(
                [
                    "Alpha",
                    "Alpine"
                ]);

                popupFrame.Bounds.Height.ShouldBeGreaterThan(
                    48,
                    "The Editable filtering showcase popup must render filtered candidate rows, not an empty strip.");
            });
        }
        finally
        {
            GalleryShowCaseRuntimeOptions.IsDeferredLoadingDisabled = oldDeferredLoadingDisabled;
        }
    }

    [Fact]
    public void ComboBox_ShowCase_Editable_Filter_Accepts_Text_Input_When_DropDown_Is_Closed()
    {
        AvaloniaTestApp.EnsureInitialized();
        var oldDeferredLoadingDisabled = GalleryShowCaseRuntimeOptions.IsDeferredLoadingDisabled;
        try
        {
            GalleryShowCaseRuntimeOptions.IsDeferredLoadingDisabled = true;
            var page = new ComboBoxShowCase
            {
                DataContext = new ComboBoxViewModel(new TestScreen())
            };

            ShowInWindow(page, window =>
            {
                var editableComboBox = page.GetVisualDescendants()
                                           .OfType<AtomUIComboBox>()
                                           .First(comboBox => comboBox.IsEditable && comboBox.IsFilterEnabled);
                var textBox = GetVisualDescendant<AvaloniaTextBox>(editableComboBox, "PART_EditableTextBox");

                editableComboBox.IsDropDownOpen = false;
                Dispatcher.UIThread.RunJobs();

                var clickPoint = editableComboBox.TranslatePoint(
                    new Point(16, editableComboBox.Bounds.Height / 2),
                    window);
                clickPoint.ShouldNotBeNull();

                window.MouseMove(clickPoint.Value);
                window.MouseDown(clickPoint.Value, MouseButton.Left);
                window.MouseUp(clickPoint.Value, MouseButton.Left);
                Dispatcher.UIThread.RunJobs();

                textBox.IsFocused.ShouldBeTrue();

                window.KeyTextInput("A");
                Dispatcher.UIThread.RunJobs();

                editableComboBox.Text.ShouldBe("A");
                editableComboBox.FilterValue.ShouldBe("A");
                editableComboBox.IsDropDownOpen.ShouldBeTrue(
                    "Typing into the real Gallery editable filtering ComboBox should open the candidate popup.");
            });
        }
        finally
        {
            GalleryShowCaseRuntimeOptions.IsDeferredLoadingDisabled = oldDeferredLoadingDisabled;
        }
    }

    [Fact]
    public void ComboBox_ShowCase_Editable_Filter_No_Match_Shows_Empty_Indicator()
    {
        AvaloniaTestApp.EnsureInitialized();
        var oldDeferredLoadingDisabled = GalleryShowCaseRuntimeOptions.IsDeferredLoadingDisabled;
        try
        {
            GalleryShowCaseRuntimeOptions.IsDeferredLoadingDisabled = true;
            var page = new ComboBoxShowCase
            {
                DataContext = new ComboBoxViewModel(new TestScreen())
            };

            ShowInWindow(page, window =>
            {
                var editableComboBox = page.GetVisualDescendants()
                                           .OfType<AtomUIComboBox>()
                                           .First(comboBox => comboBox.IsEditable && comboBox.IsFilterEnabled);

                editableComboBox.Text           = "NoMatch";
                editableComboBox.IsDropDownOpen = true;
                Dispatcher.UIThread.RunJobs();

                var popupFrame = GetVisualDescendant<Border>(window, "PopupFrame");
                popupFrame.GetVisualDescendants()
                          .OfType<AtomUI.Desktop.Controls.ComboBoxItem>()
                          .Where(item => item.IsVisible && item.Content is string)
                          .ShouldBeEmpty();

                var emptyIndicator = GetVisualDescendant<Control>(popupFrame, "PART_EmptyIndicator");
                emptyIndicator.IsVisible.ShouldBeTrue(
                    "The real Gallery editable filtering ComboBox should show Empty when no candidate matches.");
                emptyIndicator.GetVisualDescendants()
                              .OfType<Empty>()
                              .SingleOrDefault()
                              .ShouldNotBeNull();
            });
        }
        finally
        {
            GalleryShowCaseRuntimeOptions.IsDeferredLoadingDisabled = oldDeferredLoadingDisabled;
        }
    }

    [Fact]
    public void ComboBox_ShowCase_Editable_Filter_Focuses_Text_Input_When_DropDown_Opens()
    {
        AvaloniaTestApp.EnsureInitialized();
        var oldDeferredLoadingDisabled = GalleryShowCaseRuntimeOptions.IsDeferredLoadingDisabled;
        try
        {
            GalleryShowCaseRuntimeOptions.IsDeferredLoadingDisabled = true;
            var page = new ComboBoxShowCase
            {
                DataContext = new ComboBoxViewModel(new TestScreen())
            };

            ShowInWindow(page, window =>
            {
                var editableComboBox = page.GetVisualDescendants()
                                           .OfType<AtomUIComboBox>()
                                           .First(comboBox => comboBox.IsEditable && comboBox.IsFilterEnabled);
                var textBox = GetVisualDescendant<AvaloniaTextBox>(editableComboBox, "PART_EditableTextBox");

                editableComboBox.IsDropDownOpen = true;
                Dispatcher.UIThread.RunJobs();

                textBox.IsFocused.ShouldBeTrue(
                    "The real Gallery editable filtering ComboBox must focus its input after opening the drop-down.");

                window.KeyTextInput("A");
                Dispatcher.UIThread.RunJobs();

                editableComboBox.Text.ShouldBe("A");
                editableComboBox.FilterValue.ShouldBe("A");
            });
        }
        finally
        {
            GalleryShowCaseRuntimeOptions.IsDeferredLoadingDisabled = oldDeferredLoadingDisabled;
        }
    }

    private static string ExtractComboBoxExampleItems(string source)
    {
        const string firstItemMarker  = "<gallery:ShowCaseItem";
        const string panelCloseMarker = "</gallery:ShowCasePanel>";

        var firstItemStart = source.IndexOf(firstItemMarker, StringComparison.Ordinal);
        firstItemStart.ShouldBeGreaterThanOrEqualTo(0);

        var panelCloseStart = source.IndexOf(panelCloseMarker, firstItemStart, StringComparison.Ordinal);
        panelCloseStart.ShouldBeGreaterThan(firstItemStart);

        return source[firstItemStart..panelCloseStart];
    }

    private static string NormalizeMarkup(string source)
    {
        return ShowCaseSnapshotMarkup.Normalize(source);
    }

    private static string ComputeSha256(string source)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(source));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static string ReadSnapshotHash(string source)
    {
        return source
            .Split('\n')
            .First(line => line.StartsWith("sha256:", StringComparison.Ordinal))
            .Split(':', 2)[1]
            .Trim();
    }

    private static int ReadSnapshotCount(string source)
    {
        return int.Parse(source
            .Split('\n')
            .First(line => line.StartsWith("count:", StringComparison.Ordinal))
            .Split(':', 2)[1]
            .Trim());
    }

    private static int CountShowCaseItemElements(string source)
    {
        return Regex.Matches(source, @"<gallery:ShowCaseItem(\s|>)", RegexOptions.CultureInvariant).Count;
    }

    private static T GetVisualDescendant<T>(Control control, string name)
        where T : Control
    {
        var descendant = control.GetVisualDescendants()
                                .OfType<T>()
                                .FirstOrDefault(x => x.Name == name);
        descendant.ShouldNotBeNull();
        return descendant;
    }

    private static int CountOccurrences(string source, string value)
    {
        var count      = 0;
        var startIndex = 0;
        while (true)
        {
            var matchIndex = source.IndexOf(value, startIndex, StringComparison.Ordinal);
            if (matchIndex < 0)
            {
                return count;
            }

            count++;
            startIndex = matchIndex + value.Length;
        }
    }

    private static string ReadRepoFile(string relativePath)
    {
        var path = GetRepoFile(relativePath);
        File.Exists(path).ShouldBeTrue($"Expected repository file to exist: {relativePath}");
        return File.ReadAllText(path);
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

        return Path.Combine(AppContext.BaseDirectory, relativePath);
    }

    private static void ShowInWindow(Control content, Action<AvaloniaWindow> assertion)
    {
        var window = new AvaloniaWindow
        {
            Width   = 1024,
            Height  = 768,
            Content = CreatePopupOverlayHost(content)
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            content.ApplyTemplate();
            content.Measure(Size.Infinity);
            content.Arrange(new Rect(content.DesiredSize));
            Dispatcher.UIThread.RunJobs();
            assertion(window);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    private static VisualLayerManager CreatePopupOverlayHost(Control content)
    {
        var overlayPanel = new ScopeAwareOverlayLayerPanel
        {
            Width  = 1024,
            Height = 768
        };
        overlayPanel.Children.Add(content);

        var visualLayerManager = new VisualLayerManager
        {
            Child = overlayPanel
        };
        EnablePopupOverlayLayer(visualLayerManager);
        return visualLayerManager;
    }

    private static void EnablePopupOverlayLayer(VisualLayerManager visualLayerManager)
    {
        var property = typeof(VisualLayerManager).GetProperty(
            "EnablePopupOverlayLayer",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

        property.ShouldNotBeNull();
        property.SetValue(visualLayerManager, true);
    }

    private sealed class TestScreen : IScreen
    {
        public RoutingState Router { get; } = new();
    }
}
