using System.Xml.Linq;
using AtomUI.Controls.Primitives;
using AtomUI.Desktop.Controls.Primitives;
using AtomUI.Theme;
using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;
using AtomUITimePicker = AtomUI.Desktop.Controls.TimePicker;
using AtomUIRangeTimePicker = AtomUI.Desktop.Controls.RangeTimePicker;

namespace AtomUI.Desktop.Controls.Tests.TimePickers;

public class TimePickerSemanticPartTests
{
    private const string PrefixClass = "semantic-prefix";
    private const string InputClass = "semantic-input";
    private const string SecondaryInputClass = "semantic-secondary-input";
    private const string SuffixClass = "semantic-suffix";
    private const string ClearClass = "semantic-clear";
    private const string PopupRootClass = "semantic-popup-root";
    private const string PopupContainerClass = "semantic-popup-container";
    private const string PopupContentClass = "semantic-time-content";
    private const string PopupColumnClass = "semantic-time-column";
    private const string PopupItemClass = "semantic-time-item";
    private const string PopupFooterClass = "semantic-popup-footer";

    private const string InfoPickerInputThemePath =
        "src/AtomUI.Desktop.Controls/Primitives/InfoPickerInput/Themes/InfoPickerInputTheme.axaml";
    private const string RangeInfoPickerInputThemePath =
        "src/AtomUI.Desktop.Controls/Primitives/InfoPickerInput/Themes/RangeInfoPickerInputTheme.axaml";
    private const string PickerClearUpButtonThemePath =
        "src/AtomUI.Desktop.Controls/Primitives/InfoPickerInput/Themes/PickerClearUpButtonTheme.axaml";
    private const string RangeTimePickerHostPath =
        "src/AtomUI.Desktop.Controls/TimePicker/Themes/RangeTimePickerTheme.axaml";
    private const string TimePickerPresenterThemePath =
        "src/AtomUI.Desktop.Controls/TimePicker/Themes/TimePickerPresenterTheme.axaml";
    private const string TimeViewThemePath =
        "src/AtomUI.Desktop.Controls/TimePicker/Themes/TimeViewTheme.axaml";

    private static readonly string[] SemanticThemePaths =
    [
        InfoPickerInputThemePath,
        RangeInfoPickerInputThemePath,
        PickerClearUpButtonThemePath,
        RangeTimePickerHostPath,
        TimePickerPresenterThemePath,
        TimeViewThemePath
    ];

    private static readonly string[] ApprovedTimePickerPartNames =
    [
        "root", "clear", "input", "popup.column", "popup.container", "popup.content",
        "popup.footer", "popup.item", "popup.root", "prefix", "suffix"
    ];

    private static readonly string[] ApprovedRangeTimePickerPartNames =
    [
        "root", "clear", "input", "popup.column", "popup.container", "popup.content",
        "popup.footer", "popup.item", "popup.root", "prefix", "secondaryInput", "suffix"
    ];

    static TimePickerSemanticPartTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Registered_Descriptor_Exposes_Only_The_Approved_TimePicker_Parts()
    {
        var descriptor = GetDescriptor(typeof(AtomUITimePicker));

        descriptor.Parts.Select(static part => part.Name)
                  .ShouldBe(ApprovedTimePickerPartNames);

        AssertRoot(descriptor, typeof(AtomUITimePicker));
        AssertPart(descriptor, "prefix", PrefixClass, typeof(ContentPresenter),
            "/template/ .semantic-scope-input /template/ .semantic-scope-prefix > .semantic-prefix");
        AssertPart(descriptor, "input", InputClass, typeof(TextBox),
            "/template/ .semantic-input");
        AssertPart(descriptor, "suffix", SuffixClass, typeof(StackPanel),
            "/template/ .semantic-scope-input /template/ .semantic-scope-suffix > .semantic-suffix");
        AssertPart(descriptor, "clear", ClearClass, typeof(IconButton),
            ">> .semantic-scope-handle /template/ .semantic-clear");
        AssertPart(descriptor, "popup.root", PopupRootClass, typeof(ArrowDecoratedBox),
            "/template/ .semantic-popup-root");
        AssertPart(descriptor, "popup.container", PopupContainerClass, typeof(DockPanel),
            "/template/ .semantic-popup-root >> .semantic-popup-container");
        AssertPart(descriptor, "popup.content", PopupContentClass, typeof(Avalonia.Controls.Grid),
            "/template/ .semantic-popup-root >> .semantic-time-content");
        AssertPart(descriptor, "popup.column", PopupColumnClass, typeof(Panel),
            "/template/ .semantic-popup-root >> .semantic-time-column",
            SemanticPartCardinality.Multiple);
        AssertPart(descriptor, "popup.item", PopupItemClass, typeof(Avalonia.Controls.ListBoxItem),
            "/template/ .semantic-popup-root >> .semantic-time-item",
            SemanticPartCardinality.Multiple);
        AssertPart(descriptor, "popup.footer", PopupFooterClass, typeof(AtomUI.Controls.Primitives.PixelAlignedBorder),
            "/template/ .semantic-popup-root >> .semantic-popup-footer");
    }

    [Fact]
    public void Registered_Descriptor_Exposes_Only_The_Approved_RangeTimePicker_Parts()
    {
        var descriptor = GetDescriptor(typeof(AtomUIRangeTimePicker));

        descriptor.Parts.Select(static part => part.Name)
                  .ShouldBe(ApprovedRangeTimePickerPartNames);

        AssertRoot(descriptor, typeof(AtomUIRangeTimePicker));
        AssertPart(descriptor, "prefix", PrefixClass, typeof(ContentPresenter),
            "/template/ .semantic-scope-input /template/ .semantic-scope-prefix > .semantic-prefix");
        AssertPart(descriptor, "input", InputClass, typeof(TextBox),
            "/template/ .semantic-input");
        AssertPart(descriptor, "secondaryInput", SecondaryInputClass, typeof(TextBox),
            "/template/ .semantic-secondary-input");
        AssertPart(descriptor, "suffix", SuffixClass, typeof(StackPanel),
            "/template/ .semantic-scope-input /template/ .semantic-scope-suffix > .semantic-suffix");
        AssertPart(descriptor, "clear", ClearClass, typeof(IconButton),
            ">> .semantic-scope-handle /template/ .semantic-clear");
        AssertPart(descriptor, "popup.root", PopupRootClass, typeof(ArrowDecoratedBox),
            "/template/ .semantic-popup-root");
        AssertPart(descriptor, "popup.container", PopupContainerClass, typeof(DockPanel),
            "/template/ .semantic-popup-root >> .semantic-popup-container");
        AssertPart(descriptor, "popup.content", PopupContentClass, typeof(Avalonia.Controls.Grid),
            "/template/ .semantic-popup-root >> .semantic-time-content");
        AssertPart(descriptor, "popup.column", PopupColumnClass, typeof(Panel),
            "/template/ .semantic-popup-root >> .semantic-time-column",
            SemanticPartCardinality.Multiple);
        AssertPart(descriptor, "popup.item", PopupItemClass, typeof(Avalonia.Controls.ListBoxItem),
            "/template/ .semantic-popup-root >> .semantic-time-item",
            SemanticPartCardinality.Multiple);
        AssertPart(descriptor, "popup.footer", PopupFooterClass, typeof(AtomUI.Controls.Primitives.PixelAlignedBorder),
            "/template/ .semantic-popup-root >> .semantic-popup-footer");
    }

    [Fact]
    public void Host_Trigger_Templates_Implement_The_Approved_Static_Markers()
    {
        AssertMarkers(GetRepoFile(InfoPickerInputThemePath), [
            "Classes.semantic-scope-input:AddOnDecoratedBox",
            "Classes.semantic-prefix:AddOnContentPresenter",
            "Classes.semantic-suffix:StackPanel",
            "Classes.semantic-scope-handle:PickerClearUpButton",
            "Classes.semantic-input:InfoPickerTextBox",
            "Classes.semantic-popup-root:ArrowDecoratedBox"
        ]);
        AssertMarkers(GetRepoFile(PickerClearUpButtonThemePath), [
            "Classes.semantic-clear:InputClearIconButton"
        ]);
    }

    [Fact]
    public void Range_Owner_Template_Carries_The_Range_Trigger_Zone_Markers()
    {
        AssertMarkers(GetRepoFile(RangeTimePickerHostPath), [
            "Classes.semantic-scope-input:AddOnDecoratedBox",
            "Classes.semantic-prefix:AddOnContentPresenter",
            "Classes.semantic-suffix:StackPanel",
            "Classes.semantic-scope-handle:PickerClearUpButton",
            "Classes.semantic-input:InfoPickerTextBox",
            "Classes.semantic-secondary-input:InfoPickerTextBox",
            "Classes.semantic-popup-root:ArrowDecoratedBox"
        ]);
    }

    [Fact]
    public void Runtime_Assembly_Templates_Carry_The_Popup_Part_Markers()
    {
        AssertMarkers(GetRepoFile(TimePickerPresenterThemePath), [
            "Classes.semantic-popup-container:DockPanel",
            "Classes.semantic-popup-footer:PixelAlignedBorder"
        ]);
        AssertMarkers(GetRepoFile(TimeViewThemePath), [
            "Classes.semantic-time-content:Grid",
            "Classes.semantic-time-column:Panel",
            "Classes.semantic-time-column:Panel",
            "Classes.semantic-time-column:Panel",
            "Classes.semantic-time-column:Panel"
        ]);
    }

    [Fact]
    public void Default_Theme_Does_Not_Consume_Semantic_Selectors()
    {
        foreach (var path in SemanticThemePaths)
        {
            var document = XDocument.Load(GetRepoFile(path), LoadOptions.SetLineInfo);
            var selectors = document.Descendants()
                                    .Where(static element => element.Name.LocalName == "Style")
                                    .Attributes("Selector")
                                    .Select(static attribute => attribute.Value)
                                    .ToArray();

            selectors.ShouldAllBe(static selector =>
                !selector.Contains("semantic-", StringComparison.Ordinal));
        }

        var sharedRangeTheme = XDocument.Load(GetRepoFile(RangeInfoPickerInputThemePath), LoadOptions.SetLineInfo);
        sharedRangeTheme.Descendants()
                        .Any(static element => element.Attributes()
                            .Any(static attribute => attribute.Name.LocalName.StartsWith(
                                "Classes.semantic-",
                                StringComparison.Ordinal)))
                        .ShouldBeFalse();
    }

    [Fact]
    public void Generated_Semantic_Styles_Apply_To_The_TimePicker_Trigger_Targets()
    {
        var descriptor = GetDescriptor(typeof(AtomUITimePicker));
        var timePicker = new AtomUITimePicker
        {
            Width = 320,
            IsMotionEnabled = false,
            ContentLeftAddOn = "prefix"
        };
        timePicker.Classes.Add("semantic-owner");
        var ownerStyle = new Style(selector => selector.OfType<AtomUITimePicker>().Class("semantic-owner"));
        ownerStyle.Setters.Add(new Setter(Control.TagProperty, "root"));
        foreach (var part in descriptor.Parts.Where(static part => part.Name != "root" &&
                                                                  !part.Name.StartsWith("popup.", StringComparison.Ordinal)))
        {
            var partStyle = CreatePartStyle(part, part.Name);
            ownerStyle.Children.Add(partStyle);
        }
        timePicker.Styles.Add(ownerStyle);

        var window = Show(timePicker);
        try
        {
            timePicker.Tag.ShouldBe("root");
            FindSemanticControl<ContentPresenter>(timePicker, PrefixClass).Tag.ShouldBe("prefix");
            FindSemanticControl<TextBox>(timePicker, InputClass).Tag.ShouldBe("input");
            FindSemanticControl<StackPanel>(timePicker, SuffixClass).Tag.ShouldBe("suffix");
            FindSemanticControl<IconButton>(timePicker, ClearClass).Tag.ShouldBe("clear");
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Generated_Semantic_Styles_Apply_To_The_RangeTimePicker_Trigger_Targets()
    {
        var descriptor = GetDescriptor(typeof(AtomUIRangeTimePicker));
        var rangePicker = new AtomUIRangeTimePicker
        {
            Width = 420,
            IsMotionEnabled = false,
            ContentLeftAddOn = "prefix"
        };
        rangePicker.Classes.Add("semantic-owner");
        var ownerStyle = new Style(selector => selector.OfType<AtomUIRangeTimePicker>().Class("semantic-owner"));
        ownerStyle.Setters.Add(new Setter(Control.TagProperty, "root"));
        foreach (var part in descriptor.Parts.Where(static part => part.Name != "root" &&
                                                                  !part.Name.StartsWith("popup.", StringComparison.Ordinal)))
        {
            var partStyle = CreatePartStyle(part, part.Name);
            ownerStyle.Children.Add(partStyle);
        }
        rangePicker.Styles.Add(ownerStyle);

        var window = Show(rangePicker);
        try
        {
            rangePicker.Tag.ShouldBe("root");
            FindSemanticControl<ContentPresenter>(rangePicker, PrefixClass).Tag.ShouldBe("prefix");
            FindSemanticControl<TextBox>(rangePicker, InputClass).Tag.ShouldBe("input");
            FindSemanticControl<TextBox>(rangePicker, SecondaryInputClass).Tag.ShouldBe("secondaryInput");
            FindSemanticControl<StackPanel>(rangePicker, SuffixClass).Tag.ShouldBe("suffix");
            FindSemanticControl<IconButton>(rangePicker, ClearClass).Tag.ShouldBe("clear");
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Popup_Parts_Expose_Markers_When_Opened()
    {
        var timePicker = CreatePicker<AtomUITimePicker>();
        timePicker.IsPickerOpen = true;
        timePicker.IsPopupPinnedOpen = true;

        ShowInWindow(timePicker, window =>
        {
            var popup = timePicker.GetVisualDescendants().OfType<Popup>().Single();
            popup.IsOpen.ShouldBeTrue();

            var popupRoot = window.GetVisualDescendants()
                                  .OfType<Control>()
                                  .First(control => control.Classes.Contains(PopupRootClass));
            popupRoot.ShouldNotBeNull();

            AssertSingleMarker<Control>(window, PopupContainerClass);
            AssertSingleMarker<Control>(window, PopupContentClass);
            AssertSingleMarker<Control>(window, PopupFooterClass);

            CountMarkers(window, PopupColumnClass).ShouldBe(4);
            CountMarkers(window, PopupItemClass).ShouldBeGreaterThanOrEqualTo(21);
        });
    }

    [Fact]
    public void Range_Popup_Parts_Expose_Markers_When_Opened()
    {
        var rangePicker = CreatePicker<AtomUIRangeTimePicker>();
        rangePicker.IsPickerOpen = true;
        rangePicker.IsPopupPinnedOpen = true;

        ShowInWindow(rangePicker, window =>
        {
            AssertSingleMarker<Control>(window, PopupRootClass);
            AssertSingleMarker<Control>(window, PopupContainerClass);
            AssertSingleMarker<Control>(window, PopupContentClass);
            AssertSingleMarker<Control>(window, PopupFooterClass);
            CountMarkers(window, PopupColumnClass).ShouldBe(4);
            CountMarkers(window, PopupItemClass).ShouldBeGreaterThanOrEqualTo(21);
        });
    }

    [Fact]
    public void Popup_Item_Markers_Survive_ClockIdentifier_Toggle_And_Reopen()
    {
        var timePicker = CreatePicker<AtomUITimePicker>();
        timePicker.IsPickerOpen = true;
        timePicker.IsPopupPinnedOpen = true;

        ShowInWindow(timePicker, window =>
        {
            var before = CountMarkers(window, PopupItemClass);
            before.ShouldBeGreaterThanOrEqualTo(21);

            timePicker.ClockIdentifier = ClockIdentifierType.HourClock12;
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            var twelveHours = CountMarkers(window, PopupItemClass);
            twelveHours.ShouldBeGreaterThanOrEqualTo(28);
            CountMarkers(window, PopupColumnClass).ShouldBe(4);

            timePicker.ClockIdentifier = ClockIdentifierType.HourClock24;
            timePicker.MinuteIncrement = 5;
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            CountMarkers(window, PopupItemClass).ShouldBeGreaterThanOrEqualTo(21);
            CountMarkers(window, PopupColumnClass).ShouldBe(4);

            timePicker.IsPickerOpen = false;
            Dispatcher.UIThread.RunJobs();
            timePicker.IsPickerOpen = true;
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            CountMarkers(window, PopupItemClass).ShouldBeGreaterThanOrEqualTo(21);
            CountMarkers(window, PopupContainerClass).ShouldBe(1);
            CountMarkers(window, PopupContentClass).ShouldBe(1);
        });
    }

    [Fact]
    public void Footer_Visibility_Does_Not_Remove_The_Footer_Marker()
    {
        var timePicker = CreatePicker<AtomUITimePicker>();
        timePicker.IsNeedConfirm = false;
        timePicker.IsShowNow = false;
        timePicker.IsPickerOpen = true;
        timePicker.IsPopupPinnedOpen = true;

        ShowInWindow(timePicker, window =>
        {
            var footer = window.GetVisualDescendants()
                               .OfType<Control>()
                               .First(control => control.Classes.Contains(PopupFooterClass));
            footer.ShouldNotBeNull();
            footer.IsVisible.ShouldBeFalse();
        });
    }

    [Fact]
    public void Semantic_Input_Style_Keeps_The_SizeType_Height_Baseline()
    {
        var descriptor = GetDescriptor(typeof(AtomUITimePicker));
        var baselinePicker = new AtomUITimePicker
        {
            Width = 320,
            SizeType = CustomizableSizeType.Small,
            IsMotionEnabled = false
        };
        var styledPicker = new AtomUITimePicker
        {
            Width = 320,
            SizeType = CustomizableSizeType.Small,
            IsMotionEnabled = false
        };
        styledPicker.Classes.Add("semantic-owner");
        var ownerStyle = new Style(selector => selector.OfType<AtomUITimePicker>().Class("semantic-owner"));
        var inputPart = descriptor.Parts.Single(static part => part.Name == "input");
        var inputStyle = CreatePartStyle(inputPart, "input");
        inputStyle.Setters.Add(new Setter(TextBox.PaddingProperty, new Thickness(24, 8)));
        ownerStyle.Children.Add(inputStyle);
        styledPicker.Styles.Add(ownerStyle);

        var panel = new StackPanel
        {
            Children = { baselinePicker, styledPicker }
        };
        var window = Show(panel);
        try
        {
            var baseline = baselinePicker.Bounds.Height;
            var styled = styledPicker.Bounds.Height;
            baseline.ShouldBeGreaterThan(0);
            styled.ShouldBe(baseline);
        }
        finally
        {
            window.Close();
        }
    }

    private static ControlSemanticDescriptor GetDescriptor(Type controlType)
    {
        var registry = Application.Current.ShouldNotBeNull()
                                  .GetThemeManager().ShouldNotBeNull()
                                  .SemanticParts;
        registry.TryGetControl(controlType, out var descriptor).ShouldBeTrue();
        return descriptor.ShouldNotBeNull();
    }

    private static void AssertRoot(ControlSemanticDescriptor descriptor, Type controlType)
    {
        var root = descriptor.Parts.Single(static part => part.Name == "root");
        root.Path.ShouldBe("root");
        root.SelectorClass.ShouldBeNull();
        root.SelectorRoute.ShouldBeNull();
        root.ContractType.ShouldBe(controlType);
        root.Cardinality.ShouldBe(SemanticPartCardinality.Single);
        root.Customization.ShouldBe(SemanticPartCustomization.Root);
        root.StyleType.ShouldBeNull();
    }

    private static void AssertPart(
        ControlSemanticDescriptor descriptor,
        string name,
        string selectorClass,
        Type contractType,
        string selectorRoute,
        SemanticPartCardinality cardinality = SemanticPartCardinality.Single)
    {
        var part = descriptor.Parts.Single(candidate => candidate.Name == name);
        part.Path.ShouldBe(name);
        part.SelectorClass.ShouldBe(selectorClass);
        part.SelectorRoute.ShouldBe(selectorRoute);
        part.ContractType.ShouldBe(contractType);
        part.Cardinality.ShouldBe(cardinality);
        part.Customization.ShouldBe(SemanticPartCustomization.Selector);
        part.Since.ShouldBe("6.2.0");
        part.StyleType.ShouldNotBeNull();
    }

    private static Style CreatePartStyle(SemanticPartDescriptor part, string tag)
    {
        var partStyle = (Style)Activator.CreateInstance(part.StyleType.ShouldNotBeNull()).ShouldNotBeNull();
        partStyle.Setters.Add(new Setter(Control.TagProperty, tag));
        return partStyle;
    }

    private static T CreatePicker<T>()
        where T : InfoPickerInput, new()
    {
        return new T
        {
            Width = 320,
            IsMotionEnabled = false
        };
    }

    private static void AssertMarkers(string themeFile, string[] expected)
    {
        var document = XDocument.Load(themeFile, LoadOptions.SetLineInfo);
        var markers = document.Descendants()
                              .SelectMany(static element => element.Attributes()
                                  .Where(static attribute => attribute.Name.LocalName.StartsWith(
                                      "Classes.semantic-",
                                      StringComparison.Ordinal)))
                              .Select(static attribute => $"{attribute.Name.LocalName}:{attribute.Parent!.Name.LocalName}")
                              .ToArray();
        markers.ShouldBe(expected);

        document.Descendants()
                .Any(static element => element.Attributes()
                    .Any(static attribute => attribute.Name.LocalName == "Classes.semantic-root"))
                .ShouldBeFalse();
    }

    private static int CountMarkers(AvaloniaWindow window, string marker)
    {
        return window.GetVisualDescendants()
                     .OfType<Control>()
                     .Count(control => control.Classes.Contains(marker));
    }

    private static void AssertSingleMarker<T>(AvaloniaWindow window, string marker)
        where T : Control
    {
        CountMarkers(window, marker).ShouldBe(1, $"Expected a single '{marker}' marker.");
    }

    private static T FindSemanticControl<T>(Control owner, string marker)
        where T : Control
    {
        return owner.GetVisualDescendants()
                    .OfType<T>()
                    .Single(control => control.Classes.Contains(marker));
    }

    private static AvaloniaWindow Show(Control content)
    {
        var window = new AvaloniaWindow
        {
            Width = 520,
            Height = 220,
            Content = content
        };
        window.Show();
        content.ApplyTemplate();
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();
        return window;
    }

    private static void ShowInWindow(Control content, Action<AvaloniaWindow> assertion)
    {
        var overlayPanel = new ScopeAwareOverlayLayerPanel
        {
            Width = 640,
            Height = 640
        };
        overlayPanel.Children.Add(content);
        var visualLayerManager = new VisualLayerManager
        {
            Child = overlayPanel
        };
        EnablePopupOverlayLayer(visualLayerManager);

        var window = new AvaloniaWindow
        {
            Width = 640,
            Height = 640,
            Content = visualLayerManager
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            content.ApplyTemplate();
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
            assertion(window);
        }
        finally
        {
            window.Close();
        }
    }

    private static void EnablePopupOverlayLayer(VisualLayerManager visualLayerManager)
    {
        var property = typeof(VisualLayerManager).GetProperty(
            "EnablePopupOverlayLayer",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

        property.ShouldNotBeNull();
        property.SetValue(visualLayerManager, true);
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
