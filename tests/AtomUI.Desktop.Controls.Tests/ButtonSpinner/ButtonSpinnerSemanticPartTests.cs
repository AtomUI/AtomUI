using System.Xml.Linq;
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
using AtomUIButtonSpinner = AtomUI.Desktop.Controls.ButtonSpinner;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.ButtonSpinnerTests;

public class ButtonSpinnerSemanticPartTests
{
    private const string ActionsClass = "semantic-actions";
    private const string ContentClass = "semantic-content";
    private const string DecreaseButtonClass = "semantic-decrease-button";
    private const string IncreaseButtonClass = "semantic-increase-button";
    private const string InnerLeftContentClass = "semantic-inner-left-content";
    private const string InnerRightContentClass = "semantic-inner-right-content";
    private const string ScopeFrameClass = "semantic-scope-frame";

    private const string OwnerThemePath =
        "src/AtomUI.Desktop.Controls/ButtonSpinner/Themes/ButtonSpinnerTheme.axaml";
    private const string FrameThemePath =
        "src/AtomUI.Desktop.Controls/ButtonSpinner/Themes/ButtonSpinnerDecoratedBoxTheme.axaml";
    private const string HandleThemePath =
        "src/AtomUI.Desktop.Controls/ButtonSpinner/Themes/ButtonSpinnerHandleTheme.axaml";

    static ButtonSpinnerSemanticPartTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Registered_Descriptor_Exposes_Only_The_Approved_ButtonSpinner_Parts()
    {
        var registry = Application.Current.ShouldNotBeNull()
                                  .GetThemeManager().ShouldNotBeNull()
                                  .SemanticParts;

        registry.TryGetControl(typeof(AtomUIButtonSpinner), out var descriptor).ShouldBeTrue();
        descriptor.ShouldNotBeNull();
        descriptor.Parts.Select(static part => part.Name)
                  .ShouldBe(["root", "actions", "content", "decreaseButton", "increaseButton",
                             "innerLeftContent", "innerRightContent"]);

        AssertRoot(descriptor);

        AssertPart(descriptor, "actions", ActionsClass, typeof(TemplatedControl),
            "/template/ .semantic-scope-frame >> .semantic-actions");
        AssertPart(descriptor, "content", ContentClass, typeof(ContentPresenter),
            "/template/ .semantic-scope-frame /template/ .semantic-content");
        AssertPart(descriptor, "decreaseButton", DecreaseButtonClass, typeof(IconButton),
            ">> .semantic-actions /template/ .semantic-decrease-button");
        AssertPart(descriptor, "increaseButton", IncreaseButtonClass, typeof(IconButton),
            ">> .semantic-actions /template/ .semantic-increase-button");
        AssertPart(descriptor, "innerLeftContent", InnerLeftContentClass, typeof(ContentPresenter),
            "/template/ .semantic-scope-frame /template/ .semantic-inner-left-content");
        AssertPart(descriptor, "innerRightContent", InnerRightContentClass, typeof(ContentPresenter),
            "/template/ .semantic-scope-frame /template/ .semantic-inner-right-content");
    }

    /// <summary>
    /// The frame slots are shared with NumericUpDown, whose <c>prefix</c> / <c>suffix</c> routes are
    /// broad descendants (<c>/template/ .semantic-scope-spinner >> .semantic-prefix</c>) that reach
    /// into the decorated box subtree. Reusing those class names here would make NumericUpDown's
    /// route resolve two nodes and break its published <c>Single</c> cardinality.
    /// </summary>
    [Fact]
    public void Frame_Slots_Do_Not_Reuse_The_NumericUpDown_Prefix_Or_Suffix_Classes()
    {
        var registry = Application.Current.ShouldNotBeNull()
                                  .GetThemeManager().ShouldNotBeNull()
                                  .SemanticParts;

        registry.TryGetControl(typeof(AtomUIButtonSpinner), out var descriptor).ShouldBeTrue();
        descriptor.ShouldNotBeNull();

        descriptor.Parts.Select(static part => part.SelectorClass)
                  .ShouldNotContain("semantic-prefix");
        descriptor.Parts.Select(static part => part.SelectorClass)
                  .ShouldNotContain("semantic-suffix");

        var frameTheme = XDocument.Load(GetRepoFile(FrameThemePath), LoadOptions.SetLineInfo);
        frameTheme.Descendants().Any(static element => HasMarker(element, "semantic-prefix")).ShouldBeFalse();
        frameTheme.Descendants().Any(static element => HasMarker(element, "semantic-suffix")).ShouldBeFalse();
    }

    [Fact]
    public void Owner_Template_Declares_The_Scope_Anchor_And_The_Actions_Marker()
    {
        var template = SingleTemplate(OwnerThemePath);

        var scopeFrames = FindMarkedElements(template, ScopeFrameClass);
        scopeFrames.Count.ShouldBe(1);
        scopeFrames[0].Name.LocalName.ShouldBe("ButtonSpinnerDecoratedBox");

        var actions = FindMarkedElements(template, ActionsClass);
        actions.Count.ShouldBe(1);
        actions[0].Name.LocalName.ShouldBe(nameof(ButtonSpinnerHandle));

        template.Descendants().Any(static element => HasMarker(element, "semantic-root")).ShouldBeFalse();
    }

    [Fact]
    public void Frame_Template_Declares_Content_And_Inner_Content_Slots()
    {
        var template = SingleTemplate(FrameThemePath);

        var contents = FindMarkedElements(template, ContentClass);
        contents.Count.ShouldBe(1);
        contents[0].Name.LocalName.ShouldBe(nameof(ContentPresenter));

        var innerLeft = FindMarkedElements(template, InnerLeftContentClass);
        innerLeft.Count.ShouldBe(1);
        innerLeft[0].Name.LocalName.ShouldBe("AddOnContentPresenter");

        var innerRight = FindMarkedElements(template, InnerRightContentClass);
        innerRight.Count.ShouldBe(1);
        innerRight[0].Name.LocalName.ShouldBe("AddOnContentPresenter");

        innerLeft[0].ShouldNotBeSameAs(innerRight[0]);
        template.Descendants().Any(static element => HasMarker(element, "semantic-root")).ShouldBeFalse();
    }

    [Fact]
    public void Handle_Template_Declares_Both_Step_Button_Markers()
    {
        var template = SingleTemplate(HandleThemePath);

        var increase = FindMarkedElements(template, IncreaseButtonClass);
        increase.Count.ShouldBe(1);
        increase[0].Name.LocalName.ShouldBe(nameof(IconButton));

        var decrease = FindMarkedElements(template, DecreaseButtonClass);
        decrease.Count.ShouldBe(1);
        decrease[0].Name.LocalName.ShouldBe(nameof(IconButton));

        increase[0].ShouldNotBeSameAs(decrease[0]);
        template.Descendants().Any(static element => HasMarker(element, "semantic-root")).ShouldBeFalse();
    }

    [Fact]
    public void Every_Theme_Uses_Static_Class_Property_Markers_Only()
    {
        foreach (var path in new[] { OwnerThemePath, FrameThemePath, HandleThemePath })
        {
            var document = XDocument.Load(GetRepoFile(path), LoadOptions.SetLineInfo);

            document.Descendants()
                    .Attributes()
                    .Where(static attribute =>
                        attribute.Name.LocalName == "Classes" &&
                        attribute.Value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)
                             .Any(static value => value.StartsWith("semantic-", StringComparison.Ordinal)))
                    .ShouldBeEmpty();

            document.Descendants()
                    .Attributes()
                    .Where(static attribute => attribute.Name.LocalName.StartsWith(
                        "Classes.semantic-",
                        StringComparison.Ordinal))
                    .ShouldAllBe(static attribute =>
                        string.Equals(attribute.Value, "true", StringComparison.OrdinalIgnoreCase));
        }
    }

    [Fact]
    public void Generated_Semantic_Styles_Apply_To_The_Template_Targets()
    {
        var registry = Application.Current.ShouldNotBeNull()
                                  .GetThemeManager().ShouldNotBeNull()
                                  .SemanticParts;
        registry.TryGetControl(typeof(AtomUIButtonSpinner), out var descriptor).ShouldBeTrue();
        descriptor.ShouldNotBeNull();

        var spinner = CreateSpinner();
        spinner.Classes.Add("semantic-owner");
        var ownerStyle = new Style(selector => selector.OfType<AtomUIButtonSpinner>().Class("semantic-owner"));
        ownerStyle.Setters.Add(new Setter(Control.TagProperty, "root"));
        foreach (var part in descriptor.Parts.Where(static part => part.Name != "root"))
        {
            var partStyle = (Style)Activator.CreateInstance(part.StyleType.ShouldNotBeNull()).ShouldNotBeNull();
            partStyle.Setters.Add(new Setter(Control.TagProperty, part.Name));
            ownerStyle.Children.Add(partStyle);
        }
        spinner.Styles.Add(ownerStyle);

        var window = Show(spinner);
        try
        {
            spinner.Tag.ShouldBe("root");
            FindSemanticControl<TemplatedControl>(spinner, ActionsClass).Tag.ShouldBe("actions");
            FindSemanticControl<ContentPresenter>(spinner, ContentClass).Tag.ShouldBe("content");
            FindSemanticControl<IconButton>(spinner, DecreaseButtonClass).Tag.ShouldBe("decreaseButton");
            FindSemanticControl<IconButton>(spinner, IncreaseButtonClass).Tag.ShouldBe("increaseButton");
            FindSemanticControl<ContentPresenter>(spinner, InnerLeftContentClass).Tag.ShouldBe("innerLeftContent");
            FindSemanticControl<ContentPresenter>(spinner, InnerRightContentClass).Tag.ShouldBe("innerRightContent");
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Markers_Remain_Stable_When_Handle_State_Changes()
    {
        var spinner = CreateSpinner();

        var window = Show(spinner);
        try
        {
            var content = FindSemanticControl<ContentPresenter>(spinner, ContentClass);
            var innerLeft = FindSemanticControl<ContentPresenter>(spinner, InnerLeftContentClass);
            var innerRight = FindSemanticControl<ContentPresenter>(spinner, InnerRightContentClass);
            var actions = FindSemanticControl<TemplatedControl>(spinner, ActionsClass);
            var increase = FindSemanticControl<IconButton>(spinner, IncreaseButtonClass);
            var decrease = FindSemanticControl<IconButton>(spinner, DecreaseButtonClass);

            spinner.IsButtonSpinnerFloatable = true;
            spinner.IsButtonSpinnerVisible = false;
            spinner.ButtonSpinnerLocation = ButtonSpinnerLocation.Left;
            spinner.InnerLeftContent = null;
            spinner.InnerRightContent = null;
            Dispatcher.UIThread.RunJobs();

            FindSemanticControl<ContentPresenter>(spinner, ContentClass).ShouldBeSameAs(content);
            FindSemanticControl<ContentPresenter>(spinner, InnerLeftContentClass).ShouldBeSameAs(innerLeft);
            FindSemanticControl<ContentPresenter>(spinner, InnerRightContentClass).ShouldBeSameAs(innerRight);
            FindSemanticControl<TemplatedControl>(spinner, ActionsClass).ShouldBeSameAs(actions);
            FindSemanticControl<IconButton>(spinner, IncreaseButtonClass).ShouldBeSameAs(increase);
            FindSemanticControl<IconButton>(spinner, DecreaseButtonClass).ShouldBeSameAs(decrease);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Default_Theme_Does_Not_Consume_Semantic_Selectors()
    {
        foreach (var path in new[] { OwnerThemePath, FrameThemePath, HandleThemePath })
        {
            var document = XDocument.Load(GetRepoFile(path), LoadOptions.SetLineInfo);
            document.Descendants()
                    .Where(static element => element.Name.LocalName == "Style")
                    .Attributes("Selector")
                    .Select(static attribute => attribute.Value)
                    .ShouldAllBe(static selector => !selector.Contains("semantic-", StringComparison.Ordinal));
        }
    }

    private static AtomUIButtonSpinner CreateSpinner()
    {
        return new AtomUIButtonSpinner
        {
            Content          = "Semantic Spinner",
            InnerLeftContent = "$",
            InnerRightContent = "RMB"
        };
    }

    private static void AssertRoot(ControlSemanticDescriptor descriptor)
    {
        var root = descriptor.Parts.Single(static part => part.Name == "root");
        root.Path.ShouldBe("root");
        root.SelectorClass.ShouldBeNull();
        root.SelectorRoute.ShouldBeNull();
        root.ContractType.ShouldBe(typeof(AtomUIButtonSpinner));
        root.Cardinality.ShouldBe(SemanticPartCardinality.Single);
        root.Customization.ShouldBe(SemanticPartCustomization.Root);
        root.StyleType.ShouldBeNull();
    }

    private static void AssertPart(
        ControlSemanticDescriptor descriptor,
        string name,
        string selectorClass,
        Type contractType,
        string selectorRoute)
    {
        var part = descriptor.Parts.Single(candidate => candidate.Name == name);
        part.Path.ShouldBe(name);
        part.SelectorClass.ShouldBe(selectorClass);
        part.SelectorRoute.ShouldBe(selectorRoute);
        part.ContractType.ShouldBe(contractType);
        part.Cardinality.ShouldBe(SemanticPartCardinality.Single);
        part.Customization.ShouldBe(SemanticPartCustomization.Selector);
        part.CrossVisualRoot.ShouldBeFalse();
        part.RuntimeCreated.ShouldBeFalse();
        part.Since.ShouldBe("6.2.0");
        part.StyleType.ShouldNotBeNull();
    }

    private static T FindSemanticControl<T>(Control owner, string marker)
        where T : Control
    {
        return owner.GetVisualDescendants()
                    .OfType<T>()
                    .Single(control => control.Classes.Contains(marker));
    }

    private static XElement SingleTemplate(string relativePath)
    {
        var document = XDocument.Load(GetRepoFile(relativePath), LoadOptions.SetLineInfo);
        var templates = document.Descendants()
                                .Where(static element => element.Name.LocalName == "ControlTemplate")
                                .ToArray();
        templates.Length.ShouldBe(1);
        return templates[0];
    }

    private static List<XElement> FindMarkedElements(XElement template, string marker)
    {
        return template.Descendants()
                       .Where(element => HasMarker(element, marker))
                       .ToList();
    }

    private static bool HasMarker(XElement element, string marker)
    {
        if (((string?)element.Attribute("Classes"))
            ?.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)
            .Contains(marker, StringComparer.Ordinal) == true)
        {
            return true;
        }

        var classProperty = element.Attribute($"Classes.{marker}");
        return classProperty is not null &&
               bool.TryParse(classProperty.Value, out var isEnabled) &&
               isEnabled;
    }

    private static AvaloniaWindow Show(Control content)
    {
        var window = new AvaloniaWindow
        {
            Width   = 480,
            Height  = 160,
            Content = content
        };
        window.Show();
        content.ApplyTemplate();
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();
        return window;
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
