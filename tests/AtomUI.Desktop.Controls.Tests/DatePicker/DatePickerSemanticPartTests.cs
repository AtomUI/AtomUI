using System.Xml.Linq;
using AtomUI.Controls.Primitives;
using AtomUI.Desktop.Controls.Internal.DateViewer;
using AtomUI.Desktop.Controls.Primitives;
using AtomUI.Theme;
using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;
using AtomUIDatePicker = AtomUI.Desktop.Controls.DatePicker;
using AtomUIRangeDatePicker = AtomUI.Desktop.Controls.RangeDatePicker;

namespace AtomUI.Desktop.Controls.Tests.DatePickers;

public class DatePickerSemanticPartTests
{
    private const string PrefixClass = "semantic-prefix";
    private const string InputClass = "semantic-input";
    private const string SecondaryInputClass = "semantic-secondary-input";
    private const string SuffixClass = "semantic-suffix";
    private const string ClearClass = "semantic-clear";
    private const string PopupRootClass = "semantic-popup-root";
    private const string PopupContainerClass = "semantic-popup-container";
    private const string PopupHeaderClass = "semantic-header";
    private const string PopupBodyClass = "semantic-body";
    private const string PopupContentClass = "semantic-content";
    private const string CellClass = "semantic-cell";
    private const string PopupFooterClass = "semantic-popup-footer";

    private const string InfoPickerInputThemePath =
        "src/AtomUI.Desktop.Controls/Primitives/InfoPickerInput/Themes/InfoPickerInputTheme.axaml";
    private const string RangeInfoPickerInputHostPath =
        "src/AtomUI.Desktop.Controls/DatePicker/Themes/RangeDatePickerTheme.axaml";
    private const string PickerClearUpButtonThemePath =
        "src/AtomUI.Desktop.Controls/Primitives/InfoPickerInput/Themes/PickerClearUpButtonTheme.axaml";
    private const string DatePickerPresenterThemePath =
        "src/AtomUI.Desktop.Controls/DatePicker/Themes/DatePickerPresenterTheme.axaml";
    private const string RangeDatePickerPresenterThemePath =
        "src/AtomUI.Desktop.Controls/DatePicker/Themes/RangeDatePickerPresenterTheme.axaml";
    private const string DateViewerThemePath =
        "src/AtomUI.Desktop.Controls/DateViewer/Themes/DateViewerTheme.axaml";
    private const string RangeDateViewerThemePath =
        "src/AtomUI.Desktop.Controls/DateViewer/Themes/RangeDateViewerTheme.axaml";
    private const string DatePanelThemePath =
        "src/AtomUI.Desktop.Controls/DateViewer/Themes/DatePanelTheme.axaml";
    private const string DateViewerCellThemePath =
        "src/AtomUI.Desktop.Controls/DateViewer/Themes/DateViewerCellTheme.axaml";

    private static readonly string[] SemanticThemePaths =
    [
        InfoPickerInputThemePath,
        "src/AtomUI.Desktop.Controls/Primitives/InfoPickerInput/Themes/RangeInfoPickerInputTheme.axaml",
        PickerClearUpButtonThemePath,
        RangeInfoPickerInputHostPath,
        DatePickerPresenterThemePath,
        RangeDatePickerPresenterThemePath,
        DateViewerThemePath,
        RangeDateViewerThemePath,
        DatePanelThemePath,
        DateViewerCellThemePath
    ];

    private static readonly string[] ApprovedDatePickerPartNames =
    [
        "root", "clear", "input", "popup.body", "popup.cell", "popup.container", "popup.content",
        "popup.footer", "popup.header", "popup.root", "prefix", "suffix"
    ];

    private static readonly string[] ApprovedRangeDatePickerPartNames =
    [
        "root", "clear", "input", "popup.body", "popup.cell", "popup.container", "popup.content",
        "popup.footer", "popup.header", "popup.root", "prefix", "secondaryInput", "suffix"
    ];

    static DatePickerSemanticPartTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Registered_Descriptor_Exposes_Only_The_Approved_DatePicker_Parts()
    {
        var descriptor = GetDescriptor(typeof(AtomUIDatePicker));

        descriptor.Parts.Select(static part => part.Name)
                  .ShouldBe(ApprovedDatePickerPartNames);

        AssertRoot(descriptor, typeof(AtomUIDatePicker));
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
        AssertPart(descriptor, "popup.header", PopupHeaderClass, typeof(TemplatedControl),
            "/template/ .semantic-popup-root >> .semantic-header");
        AssertPart(descriptor, "popup.body", PopupBodyClass, typeof(Panel),
            "/template/ .semantic-popup-root >> .semantic-body");
        AssertPart(descriptor, "popup.content", PopupContentClass, typeof(TemplatedControl),
            "/template/ .semantic-popup-root >> .semantic-content");
        AssertPart(descriptor, "popup.cell", CellClass, typeof(TemplatedControl),
            "/template/ .semantic-popup-root >> .semantic-cell",
            SemanticPartCardinality.Multiple);
        AssertPart(descriptor, "popup.footer", PopupFooterClass, typeof(PixelAlignedBorder),
            "/template/ .semantic-popup-root >> .semantic-popup-footer");
    }

    [Fact]
    public void Registered_Descriptor_Exposes_Only_The_Approved_RangeDatePicker_Parts()
    {
        var descriptor = GetDescriptor(typeof(AtomUIRangeDatePicker));

        descriptor.Parts.Select(static part => part.Name)
                  .ShouldBe(ApprovedRangeDatePickerPartNames);

        AssertRoot(descriptor, typeof(AtomUIRangeDatePicker));
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
        AssertPart(descriptor, "popup.header", PopupHeaderClass, typeof(TemplatedControl),
            "/template/ .semantic-popup-root >> .semantic-header",
            SemanticPartCardinality.Multiple);
        AssertPart(descriptor, "popup.body", PopupBodyClass, typeof(Panel),
            "/template/ .semantic-popup-root >> .semantic-body");
        AssertPart(descriptor, "popup.content", PopupContentClass, typeof(TemplatedControl),
            "/template/ .semantic-popup-root >> .semantic-content",
            SemanticPartCardinality.Multiple);
        AssertPart(descriptor, "popup.cell", CellClass, typeof(TemplatedControl),
            "/template/ .semantic-popup-root >> .semantic-cell",
            SemanticPartCardinality.Multiple);
        AssertPart(descriptor, "popup.footer", PopupFooterClass, typeof(PixelAlignedBorder),
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
        AssertMarkers(GetRepoFile(RangeInfoPickerInputHostPath), [
            "Classes.semantic-scope-input:AddOnDecoratedBox",
            "Classes.semantic-prefix:AddOnContentPresenter",
            "Classes.semantic-suffix:StackPanel",
            "Classes.semantic-scope-handle:PickerClearUpButton",
            "Classes.semantic-input:InfoPickerTextBox",
            "Classes.semantic-secondary-input:InfoPickerTextBox",
            "Classes.semantic-popup-root:DualMonthArrowDecoratedBox"
        ]);
        AssertMarkers(GetRepoFile(PickerClearUpButtonThemePath), [
            "Classes.semantic-clear:InputClearIconButton"
        ]);
    }

    [Fact]
    public void Runtime_Assembly_Templates_Carry_The_Popup_Part_Markers()
    {
        var presenterMarkers = new[]
        {
            "Classes.semantic-popup-container:DockPanel",
            "Classes.semantic-popup-footer:PixelAlignedBorder"
        };
        AssertMarkers(GetRepoFile(DatePickerPresenterThemePath), presenterMarkers);
        AssertMarkers(GetRepoFile(RangeDatePickerPresenterThemePath), presenterMarkers);

        AssertMarkers(GetRepoFile(DateViewerThemePath), [
            "Classes.semantic-header:DateViewerHeader",
            "Classes.semantic-footer:PixelAlignedBorder",
            "Classes.semantic-body:Panel",
            "Classes.semantic-content:DatePanel"
        ]);
        AssertMarkers(GetRepoFile(RangeDateViewerThemePath), [
            "Classes.semantic-header:DateViewerHeader",
            "Classes.semantic-header:DateViewerHeader",
            "Classes.semantic-body:StackPanel",
            "Classes.semantic-content:DatePanel",
            "Classes.semantic-content:DatePanel",
            "Classes.semantic-header:DateViewerHeader",
            "Classes.semantic-body:Panel",
            "Classes.semantic-content:DatePanel"
        ]);
        AssertMarkers(GetRepoFile(DatePanelThemePath), [
            "Classes.semantic-scope-panel:DockPanel",
            "Classes.semantic-scope-cells:Grid"
        ]);
        AssertMarkers(GetRepoFile(DateViewerCellThemePath), [
            "Classes.semantic-cell-content:ContentControl"
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
    }

    [Fact]
    public void Generated_Semantic_Styles_Apply_To_The_DatePicker_Trigger_Targets()
    {
        var descriptor = GetDescriptor(typeof(AtomUIDatePicker));
        var datePicker = new AtomUIDatePicker
        {
            Width = 320,
            IsMotionEnabled = false,
            ContentLeftAddOn = "prefix"
        };
        datePicker.Classes.Add("semantic-owner");
        var ownerStyle = new Style(selector => selector.OfType<AtomUIDatePicker>().Class("semantic-owner"));
        ownerStyle.Setters.Add(new Setter(Control.TagProperty, "root"));
        foreach (var part in descriptor.Parts.Where(static part => part.Name != "root" &&
                                                                  !part.Name.StartsWith("popup.", StringComparison.Ordinal)))
        {
            var partStyle = CreatePartStyle(part, part.Name);
            ownerStyle.Children.Add(partStyle);
        }
        datePicker.Styles.Add(ownerStyle);

        var window = Show(datePicker);
        try
        {
            datePicker.Tag.ShouldBe("root");
            FindSemanticControl<ContentPresenter>(datePicker, PrefixClass).Tag.ShouldBe("prefix");
            FindSemanticControl<TextBox>(datePicker, InputClass).Tag.ShouldBe("input");
            FindSemanticControl<StackPanel>(datePicker, SuffixClass).Tag.ShouldBe("suffix");
            FindSemanticControl<IconButton>(datePicker, ClearClass).Tag.ShouldBe("clear");
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Generated_Semantic_Styles_Apply_To_The_RangeDatePicker_Trigger_Targets()
    {
        var descriptor = GetDescriptor(typeof(AtomUIRangeDatePicker));
        var rangePicker = new AtomUIRangeDatePicker
        {
            Width = 420,
            IsMotionEnabled = false,
            ContentLeftAddOn = "prefix"
        };
        rangePicker.Classes.Add("semantic-owner");
        var ownerStyle = new Style(selector => selector.OfType<AtomUIRangeDatePicker>().Class("semantic-owner"));
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
        var datePicker = CreatePicker<AtomUIDatePicker>();
        datePicker.IsPickerOpen = true;
        datePicker.IsPopupPinnedOpen = true;

        ShowInWindow(datePicker, window =>
        {
            var popup = datePicker.GetVisualDescendants().OfType<Popup>().Single();
            popup.IsOpen.ShouldBeTrue();

            var popupRoot = window.GetVisualDescendants()
                                  .OfType<Control>()
                                  .First(control => control.Classes.Contains(PopupRootClass));
            popupRoot.ShouldNotBeNull();

            AssertSingleMarker<Control>(window, PopupContainerClass);
            var viewer = window.GetVisualDescendants().OfType<DateViewer>().Single();
            viewer.GetVisualDescendants().OfType<DateViewerHeader>().Count().ShouldBe(1);
            viewer.GetVisualDescendants().OfType<DatePanel>().Count().ShouldBe(1);
            AssertSingleMarker<Control>(window, PopupFooterClass);

            var cells = CountMarkers(window, CellClass);
            cells.ShouldBeGreaterThanOrEqualTo(28);
            window.GetVisualDescendants()
                  .OfType<DateViewerCell>()
                  .ShouldAllBe(cell => cell.Classes.Contains(CellClass));
        });
    }

    [Fact]
    public void Range_Popup_Parts_Expose_Two_Shared_Panel_Content_Markers()
    {
        var rangePicker = CreatePicker<AtomUIRangeDatePicker>();
        rangePicker.IsPickerOpen = true;
        rangePicker.IsPopupPinnedOpen = true;

        ShowInWindow(rangePicker, window =>
        {
            window.GetVisualDescendants().OfType<RangeDateViewer>().Single()
                  .GetVisualDescendants().OfType<DatePanel>().Count().ShouldBe(2);
            CountMarkers(window, CellClass).ShouldBeGreaterThanOrEqualTo(56);
        });
    }

    [Fact]
    public void Timed_Range_Popup_Exposes_One_Visible_Shared_Panel()
    {
        var rangePicker = CreatePicker<AtomUIRangeDatePicker>();
        rangePicker.IsShowTime = true;
        rangePicker.IsPickerOpen = true;
        rangePicker.IsPopupPinnedOpen = true;

        ShowInWindow(rangePicker, window =>
        {
            window.GetVisualDescendants().OfType<RangeDateViewer>().Single()
                  .GetVisualDescendants().OfType<DatePanel>()
                  .Count(control => control.IsVisible)
                  .ShouldBe(1);
            CountMarkers(window, CellClass).ShouldBeGreaterThanOrEqualTo(28);
        });
    }

    [Fact]
    public void Popup_Cell_Markers_Survive_Month_Rebuild_And_Reopen()
    {
        var datePicker = CreatePicker<AtomUIDatePicker>();
        datePicker.IsPickerOpen = true;
        datePicker.IsPopupPinnedOpen = true;

        ShowInWindow(datePicker, window =>
        {
            var before = CountMarkers(window, CellClass);
            before.ShouldBeGreaterThanOrEqualTo(28);

            datePicker.SelectedDateTime = DateTime.Today.AddMonths(3);
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            var after = CountMarkers(window, CellClass);
            after.ShouldBeGreaterThanOrEqualTo(28);
            after.ShouldBe(before);

            datePicker.IsPickerOpen = false;
            Dispatcher.UIThread.RunJobs();
            datePicker.IsPickerOpen = true;
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            CountMarkers(window, CellClass).ShouldBe(after);
            CountMarkers(window, PopupContainerClass).ShouldBe(1);
        });
    }

    [Fact]
    public void Footer_Visibility_Does_Not_Remove_The_Footer_Marker()
    {
        var datePicker = CreatePicker<AtomUIDatePicker>();
        datePicker.IsNeedConfirm = false;
        datePicker.IsShowNow = false;
        datePicker.IsPickerOpen = true;
        datePicker.IsPopupPinnedOpen = true;

        ShowInWindow(datePicker, window =>
        {
            var footer = window.GetVisualDescendants()
                               .OfType<Control>()
                               .First(control => control.Classes.Contains(PopupFooterClass));
            footer.ShouldNotBeNull();
            footer.IsVisible.ShouldBeFalse();
        });
    }

    [Fact]
    public void Popup_Footer_Contract_Type_Covers_The_Marked_Element()
    {
        VerifyFooterContract(GetDescriptor(typeof(AtomUIDatePicker)), CreatePicker<AtomUIDatePicker>());
        VerifyFooterContract(GetDescriptor(typeof(AtomUIRangeDatePicker)), CreatePicker<AtomUIRangeDatePicker>());

        static void VerifyFooterContract(ControlSemanticDescriptor descriptor, InfoPickerInput picker)
        {
            var footerPart = descriptor.Parts.Single(static part => part.Name == "popup.footer");
            picker.IsPickerOpen = true;
            picker.IsPopupPinnedOpen = true;

            ShowInWindow(picker, window =>
            {
                var footer = window.GetVisualDescendants()
                                   .OfType<Control>()
                                   .First(control => control.Classes.Contains(PopupFooterClass));
                footer.ShouldNotBeNull();
                footerPart.ContractType.IsInstanceOfType(footer).ShouldBeTrue(
                    $"the marked footer element ({footer.GetType().Name}) must satisfy the " +
                    $"popup.footer ContractType ({footerPart.ContractType.Name})");
            });
        }
    }

    [Fact]
    public void Semantic_Input_Style_Keeps_The_SizeType_Height_Baseline()
    {
        var descriptor = GetDescriptor(typeof(AtomUIDatePicker));
        var baselinePicker = new AtomUIDatePicker
        {
            Width = 320,
            SizeType = CustomizableSizeType.Small,
            IsMotionEnabled = false
        };
        var styledPicker = new AtomUIDatePicker
        {
            Width = 320,
            SizeType = CustomizableSizeType.Small,
            IsMotionEnabled = false
        };
        styledPicker.Classes.Add("semantic-owner");
        var ownerStyle = new Style(selector => selector.OfType<AtomUIDatePicker>().Class("semantic-owner"));
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

    [Fact]
    public void Popup_Root_Style_Paints_The_Popup_Frame_Border()
    {
        var descriptor = GetDescriptor(typeof(AtomUIRangeDatePicker));
        var rangePicker = CreatePicker<AtomUIRangeDatePicker>();
        rangePicker.IsPickerOpen = true;
        rangePicker.IsPopupPinnedOpen = true;
        rangePicker.Classes.Add("semantic-owner");
        var ownerStyle = new Style(selector => selector.OfType<AtomUIRangeDatePicker>().Class("semantic-owner"));
        var part = descriptor.Parts.Single(static candidate => candidate.Name == "popup.root");
        var partStyle = CreatePartStyle(part, "popup.root");
        partStyle.Setters.Add(new Setter(TemplatedControl.BorderBrushProperty, Brushes.Purple));
        partStyle.Setters.Add(new Setter(TemplatedControl.BorderThicknessProperty, new Thickness(1)));
        ownerStyle.Children.Add(partStyle);
        rangePicker.Styles.Add(ownerStyle);

        ShowInWindow(rangePicker, window =>
        {
            var popupRoot = window.GetVisualDescendants()
                                  .OfType<ArrowDecoratedBox>()
                                  .First(control => control.Classes.Contains(PopupRootClass));
            popupRoot.BorderThickness.ShouldBe(new Thickness(1));
            popupRoot.BorderBrush.ShouldBe(Brushes.Purple);

            var decorator = popupRoot.GetVisualDescendants()
                                     .OfType<Border>()
                                     .First(border => border.Name == "PART_ContentDecorator");
            decorator.BorderThickness.ShouldBe(new Thickness(1));
            decorator.BorderBrush.ShouldBe(Brushes.Purple);
        });
    }

    [Fact]
    public void Root_BorderBrush_Relay_Reaches_The_Trigger_Frame()
    {
        var styledPicker = new AtomUIDatePicker
        {
            Width = 320,
            IsMotionEnabled = false,
            BorderBrush = Brushes.Purple
        };
        var baselinePicker = new AtomUIDatePicker
        {
            Width = 320,
            IsMotionEnabled = false
        };
        var panel = new StackPanel { Children = { styledPicker, baselinePicker } };

        var window = Show(panel);
        try
        {
            var styledBox = styledPicker.GetVisualDescendants()
                                        .OfType<AddOnDecoratedBox>()
                                        .First();
            styledBox.BorderBrush.ShouldBe(Brushes.Purple);

            var baselineBox = baselinePicker.GetVisualDescendants()
                                            .OfType<AddOnDecoratedBox>()
                                            .First();
            baselineBox.BorderBrush.ShouldNotBe(Brushes.Purple);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Bordered_Popup_Hides_The_Floating_Arrow()
    {
        var descriptor = GetDescriptor(typeof(AtomUIRangeDatePicker));

        var borderedPicker = CreatePicker<AtomUIRangeDatePicker>();
        borderedPicker.IsPickerOpen = true;
        borderedPicker.IsPopupPinnedOpen = true;
        borderedPicker.Classes.Add("semantic-owner");
        var ownerStyle = new Style(selector => selector.OfType<AtomUIRangeDatePicker>().Class("semantic-owner"));
        var part = descriptor.Parts.Single(static candidate => candidate.Name == "popup.root");
        var partStyle = CreatePartStyle(part, "popup.root");
        partStyle.Setters.Add(new Setter(TemplatedControl.BorderBrushProperty, Brushes.Purple));
        partStyle.Setters.Add(new Setter(TemplatedControl.BorderThicknessProperty, new Thickness(1)));
        ownerStyle.Children.Add(partStyle);
        borderedPicker.Styles.Add(ownerStyle);

        ShowInWindow(borderedPicker, window =>
        {
            var popupRoot = window.GetVisualDescendants()
                                  .OfType<ArrowDecoratedBox>()
                                  .First(control => control.Classes.Contains(PopupRootClass));
            popupRoot.Classes.Contains(":bordered").ShouldBeTrue();
            var arrowLayout = popupRoot.GetVisualDescendants()
                                       .OfType<Panel>()
                                       .First(candidate => candidate.Name == "ArrowPositionLayout");
            arrowLayout.IsVisible.ShouldBeFalse();
        });

        var baselinePicker = CreatePicker<AtomUIRangeDatePicker>();
        baselinePicker.IsPickerOpen = true;
        baselinePicker.IsPopupPinnedOpen = true;

        ShowInWindow(baselinePicker, window =>
        {
            var popupRoot = window.GetVisualDescendants()
                                  .OfType<ArrowDecoratedBox>()
                                  .First(control => control.Classes.Contains(PopupRootClass));
            popupRoot.Classes.Contains(":bordered").ShouldBeFalse();
            var arrowLayout = popupRoot.GetVisualDescendants()
                                       .OfType<Panel>()
                                       .First(candidate => candidate.Name == "ArrowPositionLayout");
            arrowLayout.IsVisible.ShouldBeTrue();
        });
    }

    [Fact]
    public void Pinned_Open_Disables_Light_Dismiss_On_The_Template_Popup()
    {
        var datePicker = CreatePicker<AtomUIDatePicker>();
        datePicker.IsPickerOpen = true;
        datePicker.IsPopupPinnedOpen = true;

        ShowInWindow(datePicker, window =>
        {
            var popup = datePicker.GetVisualDescendants().OfType<Popup>().Single();
            popup.IsOpen.ShouldBeTrue();
            popup.IsLightDismissEnabled.ShouldBeFalse(
                "a pinned preview popup must not leave a light-dismiss overlay blocking the rest of the window");
            popup.OverlayInputPassThroughElement.ShouldNotBeNull(
                "the input box stays registered as the overlay pass-through element, matching AutoComplete/Select");
        });
    }

    [Fact]
    public void Pinned_Open_Does_Not_Block_Hit_Testing_Outside_The_Popup()
    {
        var datePicker = CreatePicker<AtomUIDatePicker>();
        datePicker.IsPickerOpen = true;
        datePicker.IsPopupPinnedOpen = true;

        var outsideButton = new Button { Content = "outside" };
        var stack = new StackPanel
        {
            Children = { datePicker, outsideButton }
        };

        ShowInWindow(stack, window =>
        {
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            var popup = datePicker.GetVisualDescendants().OfType<Popup>().Single();
            popup.IsOpen.ShouldBeTrue();

            var popupBounds = popup.Bounds;
            var buttonPoint = outsideButton.TranslatePoint(
                new Point(outsideButton.Bounds.Width / 2, outsideButton.Bounds.Height / 2),
                window.ShouldNotBeNull()).ShouldNotBeNull();
            var hit = window.GetVisualDescendants()
                            .OfType<Visual>()
                            .LastOrDefault();
            // Window has no direct InputHitTest; walk the visual roots instead.
            hit = HitTest(window, buttonPoint);
            hit.ShouldNotBeNull();
            var reached = ReferenceEquals(hit, outsideButton) ||
                          outsideButton.GetVisualDescendants().Contains(hit) ||
                          hit is Avalonia.Controls.Button;
            reached.ShouldBeTrue(
                    $"hit outside popup should reach the button but was {hit.GetType().Name} " +
                    $"(popup at {popupBounds}); visual chain: " +
                    string.Join(" <- ", AncestorNames(hit)));
        });
    }

    private static Visual? HitTest(Visual root, Point point)
    {
        foreach (var child in root.GetVisualChildren())
        {
            var bounds = new Rect(child.Bounds.Size);
            var local = point - (Vector)child.Bounds.Position;
            if (bounds.Contains(local))
            {
                var deeper = HitTest(child, local);
                if (deeper is not null)
                {
                    return deeper;
                }

                return child;
            }
        }

        return root.Bounds.Contains(point) ? root : null;
    }

    private static IEnumerable<string> AncestorNames(Visual? visual)
    {
        while (visual is not null)
        {
            yield return visual.GetType().Name + (visual is Control control && control.Name is { } name ? $"#{name}" : string.Empty);
            visual = visual.GetVisualParent();
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
