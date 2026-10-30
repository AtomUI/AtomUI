using System.Xml.Linq;
using AtomUI.Controls;
using AtomUI.Controls.Primitives;
using AtomUI.Theme;
using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomUIAlert = AtomUI.Desktop.Controls.Alert;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Feedback;

public class AlertSemanticPartTests
{
    private const string IconClass = "semantic-icon";
    private const string SectionClass = "semantic-section";
    private const string TitleClass = "semantic-title";
    private const string DescriptionClass = "semantic-description";
    private const string ActionsClass = "semantic-actions";
    private const string CloseClass = "semantic-close";
    private const string ThemePath = "src/AtomUI.Desktop.Controls/Alert/Themes/AlertTheme.axaml";

    static AlertSemanticPartTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Registered_Descriptor_Exposes_The_Approved_Alert_Parts()
    {
        var manager = Application.Current.ShouldNotBeNull().GetThemeManager().ShouldNotBeNull();

        manager.SemanticParts.TryGetControl(typeof(AtomUIAlert), out var descriptor).ShouldBeTrue();
        descriptor.ShouldNotBeNull();
        descriptor.Parts.Select(static part => part.Name)
                  .ShouldBe(["root", "actions", "close", "description", "icon", "section", "title"]);

        AssertRoot(descriptor.Parts.Single(static part => part.Name == "root"));
        AssertPart(descriptor, "actions", ActionsClass, typeof(ContentPresenter));
        AssertPart(descriptor, "close", CloseClass, typeof(IconButton));
        AssertPart(descriptor, "description", DescriptionClass, typeof(Label));
        AssertPart(descriptor, "icon", IconClass, typeof(Icon), SemanticPartCardinality.Multiple);
        AssertPart(descriptor, "section", SectionClass, typeof(StackPanel));
        AssertPart(descriptor, "title", TitleClass, typeof(Control), SemanticPartCardinality.Multiple);
    }

    [Fact]
    public void Built_In_Template_Implements_The_Static_Marker_And_Root_Surface_Contract()
    {
        var document = XDocument.Load(GetRepoFile(ThemePath), LoadOptions.SetLineInfo);
        var template = document.Descendants()
                               .Single(static element => element.Name.LocalName == "ControlTemplate");
        var markers = template.Descendants()
                              .SelectMany(static element => element.Attributes()
                                  .Where(static attribute => attribute.Name.LocalName.StartsWith(
                                      "Classes.semantic-",
                                      StringComparison.Ordinal))
                                  .Select(attribute => (Element: element, Attribute: attribute)))
                              .ToArray();

        markers.Select(static marker => $"{marker.Attribute.Name.LocalName}:{marker.Element.Name.LocalName}")
               .ShouldBe([
                   "Classes.semantic-icon:CheckCircleFilled",
                   "Classes.semantic-icon:InfoCircleFilled",
                   "Classes.semantic-icon:ExclamationCircleFilled",
                   "Classes.semantic-icon:CloseCircleFilled",
                   "Classes.semantic-close:IconButton",
                   "Classes.semantic-actions:ContentPresenter",
                   "Classes.semantic-section:StackPanel",
                   "Classes.semantic-title:Label",
                   "Classes.semantic-title:MarqueeLabel",
                   "Classes.semantic-description:Label"
               ]);
        markers.ShouldAllBe(static marker =>
            string.Equals(marker.Attribute.Value, "true", StringComparison.OrdinalIgnoreCase));
        template.Descendants().Any(static element => HasMarker(element, "semantic-root")).ShouldBeFalse();

        var frame = template.Elements().Single();
        frame.Name.LocalName.ShouldBe(nameof(PixelAlignedBorder));
        frame.Attribute("Background")?.Value.ShouldBe("{TemplateBinding Background}");
        frame.Attribute("BorderBrush")?.Value.ShouldBe("{TemplateBinding BorderBrush}");
        frame.Attribute("BorderThickness")?.Value.ShouldBe("{TemplateBinding BorderThickness}");
        frame.Attribute("CornerRadius")?.Value.ShouldBe("{TemplateBinding CornerRadius}");
        frame.Attribute("StrokeDashArray")?.Value.ShouldBe("{TemplateBinding StrokeDashArray}");
        frame.Attribute("Padding")?.Value.ShouldBe("{TemplateBinding Padding}");
    }

    [Fact]
    public void Root_Style_Projects_Stroke_Dash_Array_To_The_Template_Surface()
    {
        IReadOnlyList<double> strokeDashArray = [4d, 2d];
        var alert = new AtomUIAlert
        {
            Message = "Object styles",
            StrokeDashArray = strokeDashArray
        };

        var window = Show(alert);
        try
        {
            var frame = alert.GetVisualChildren()
                             .OfType<PixelAlignedBorder>()
                             .Single();
            frame.StrokeDashArray.ShouldBeSameAs(strokeDashArray);

            IReadOnlyList<double> updatedStrokeDashArray = [2d, 2d];
            alert.StrokeDashArray = updatedStrokeDashArray;
            Dispatcher.UIThread.RunJobs();
            frame.StrokeDashArray.ShouldBeSameAs(updatedStrokeDashArray);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Built_In_Theme_Does_Not_Consume_Semantic_Selectors()
    {
        var document = XDocument.Load(GetRepoFile(ThemePath), LoadOptions.SetLineInfo);

        document.Descendants()
                .Where(static element => element.Name.LocalName == "Style")
                .Select(static element => (string?)element.Attribute("Selector"))
                .Where(static selector => selector?.Contains(".semantic-", StringComparison.Ordinal) == true)
                .ShouldBeEmpty();
    }

    [Fact]
    public void Built_In_Theme_Defines_Complete_Description_State_Baselines()
    {
        var source = File.ReadAllText(GetRepoFile(ThemePath));

        source.ShouldContain("Selector=\"^:not(:has-description)\"");
        source.ShouldContain("{atom:AlertTokenResource DefaultPadding}");
        source.ShouldContain("{atom:AlertTokenResource DefaultIconSize}");
        source.ShouldContain("{atom:AlertTokenResource IconDefaultMargin}");
        source.ShouldContain("Selector=\"^:has-description\"");
        source.ShouldContain("{atom:AlertTokenResource WithDescriptionPadding}");
        source.ShouldContain("{atom:AlertTokenResource WithDescriptionIconSize}");
        source.ShouldContain("{atom:AlertTokenResource IconWithDescriptionMargin}");
    }

    [Fact]
    public void Generated_Semantic_Styles_Apply_To_All_Template_Targets()
    {
        var manager = Application.Current.ShouldNotBeNull().GetThemeManager().ShouldNotBeNull();
        manager.SemanticParts.TryGetControl(typeof(AtomUIAlert), out var descriptor).ShouldBeTrue();
        descriptor.ShouldNotBeNull();

        var alert = CreateRichAlert();
        alert.Classes.Add("styled-alert");
        var ownerStyle = new Style(selector => selector.OfType<AtomUIAlert>().Class("styled-alert"));
        foreach (var partName in new[] { "icon", "section", "title", "description", "actions", "close" })
        {
            AddGeneratedPartStyle(ownerStyle, descriptor, partName, $"styled-{partName}");
        }
        alert.Styles.Add(ownerStyle);

        var window = Show(alert);
        try
        {
            FindSemanticControls<Icon>(alert, IconClass)
                .ShouldAllBe(static icon => Equals(icon.Tag, "styled-icon"));
            FindSemanticControl<StackPanel>(alert, SectionClass).Tag.ShouldBe("styled-section");
            FindSemanticControls<Control>(alert, TitleClass)
                .ShouldAllBe(static title => Equals(title.Tag, "styled-title"));
            FindSemanticControl<Label>(alert, DescriptionClass).Tag.ShouldBe("styled-description");
            FindSemanticControl<ContentPresenter>(alert, ActionsClass).Tag.ShouldBe("styled-actions");
            FindSemanticControl<IconButton>(alert, CloseClass).Tag.ShouldBe("styled-close");
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void State_Changes_Preserve_Static_Marker_Identity_And_Visibility()
    {
        var alert = CreateRichAlert();
        var window = Show(alert);
        try
        {
            var icons = FindSemanticControls<Icon>(alert, IconClass);
            var titles = FindSemanticControls<Control>(alert, TitleClass);
            var description = FindSemanticControl<Label>(alert, DescriptionClass);
            var actions = FindSemanticControl<ContentPresenter>(alert, ActionsClass);
            var close = FindSemanticControl<IconButton>(alert, CloseClass);

            icons.Count.ShouldBe(4);
            titles.Count.ShouldBe(2);
            icons.Count(static icon => icon.IsVisible).ShouldBe(1);
            description.IsVisible.ShouldBeTrue();
            actions.IsVisible.ShouldBeTrue();
            close.IsVisible.ShouldBeTrue();

            foreach (var type in Enum.GetValues<AlertType>())
            {
                alert.Type = type;
                Dispatcher.UIThread.RunJobs();
                FindSemanticControls<Icon>(alert, IconClass).ShouldBe(icons);
                icons.Count(static icon => icon.IsVisible).ShouldBe(1);
            }

            alert.IsShowIcon = false;
            alert.IsMessageMarqueeEnabled = true;
            alert.Description = null;
            alert.ExtraAction = null;
            alert.IsClosable = false;
            Dispatcher.UIThread.RunJobs();

            FindSemanticControls<Icon>(alert, IconClass).ShouldBe(icons);
            FindSemanticControls<Control>(alert, TitleClass).ShouldBe(titles);
            icons[0].Parent.ShouldBeOfType<Panel>().IsVisible.ShouldBeFalse();
            titles.Single(static title => title.Name == "MessageLabel").IsVisible.ShouldBeFalse();
            titles.Single(static title => title.Name == "MarqueeLabel").IsVisible.ShouldBeTrue();
            description.IsVisible.ShouldBeFalse();
            actions.IsVisible.ShouldBeFalse();
            close.IsVisible.ShouldBeFalse();
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void ExtraAction_Changes_After_Template_Application_Update_The_PseudoClass_And_Presenter()
    {
        var alert = new AtomUIAlert
        {
            Message = "Deployment status"
        };
        var window = Show(alert);
        try
        {
            var actions = FindSemanticControl<ContentPresenter>(alert, ActionsClass);
            alert.Classes.Contains(AlertPseudoClass.HasExtraAction).ShouldBeFalse();
            actions.IsVisible.ShouldBeFalse();

            alert.ExtraAction = new Avalonia.Controls.Button { Content = "Details" };
            Dispatcher.UIThread.RunJobs();

            alert.Classes.Contains(AlertPseudoClass.HasExtraAction).ShouldBeTrue();
            actions.IsVisible.ShouldBeTrue();

            alert.ExtraAction = null;
            Dispatcher.UIThread.RunJobs();

            alert.Classes.Contains(AlertPseudoClass.HasExtraAction).ShouldBeFalse();
            actions.IsVisible.ShouldBeFalse();
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Retemplate_Releases_The_Old_Close_Button_And_Preserves_Markers()
    {
        var alert = CreateRichAlert();
        var closeRequests = 0;
        alert.CloseRequest += (_, _) => closeRequests++;
        var window = Show(alert);
        try
        {
            var oldClose = FindSemanticControl<IconButton>(alert, CloseClass);
            oldClose.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            closeRequests.ShouldBe(1);

            ReapplyDefaultTemplate(alert);
            var newClose = FindSemanticControl<IconButton>(alert, CloseClass);
            newClose.ShouldNotBeSameAs(oldClose);
            FindSemanticControls<Icon>(alert, IconClass).Count.ShouldBe(4);
            FindSemanticControls<Control>(alert, TitleClass).Count.ShouldBe(2);

            oldClose.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            closeRequests.ShouldBe(1);
            newClose.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            closeRequests.ShouldBe(2);
        }
        finally
        {
            window.Close();
        }
    }

    private static AtomUIAlert CreateRichAlert()
    {
        return new AtomUIAlert
        {
            Type = AlertType.Success,
            IsShowIcon = true,
            IsClosable = true,
            Message = "Deployment completed",
            Description = "All production checks passed.",
            ExtraAction = new Avalonia.Controls.Button { Content = "View details" }
        };
    }

    private static void AssertRoot(SemanticPartDescriptor part)
    {
        part.Path.ShouldBe("root");
        part.SelectorClass.ShouldBeNull();
        part.SelectorRoute.ShouldBeNull();
        part.ContractType.ShouldBe(typeof(AtomUIAlert));
        part.Cardinality.ShouldBe(SemanticPartCardinality.Single);
        part.Customization.ShouldBe(SemanticPartCustomization.Root);
        part.StyleType.ShouldBeNull();
        part.CrossVisualRoot.ShouldBeFalse();
        part.RuntimeCreated.ShouldBeFalse();
        part.Since.ShouldBe("6.2.0");
    }

    private static void AssertPart(
        ControlSemanticDescriptor descriptor,
        string name,
        string selectorClass,
        Type contractType,
        SemanticPartCardinality cardinality = SemanticPartCardinality.Single)
    {
        var part = descriptor.Parts.Single(candidate => candidate.Name == name);
        part.Path.ShouldBe(name);
        part.SelectorClass.ShouldBe(selectorClass);
        part.SelectorRoute.ShouldBe($"/template/ .{selectorClass}");
        part.ContractType.ShouldBe(contractType);
        part.Cardinality.ShouldBe(cardinality);
        part.Customization.ShouldBe(SemanticPartCustomization.Selector);
        part.StyleType.ShouldBe(typeof(AtomUIAlert).Assembly.GetType(
            $"AtomUI.Theme.Styling.Alert{char.ToUpperInvariant(name[0])}{name[1..]}Style"));
        part.CrossVisualRoot.ShouldBeFalse();
        part.RuntimeCreated.ShouldBeFalse();
        part.Since.ShouldBe("6.2.0");
    }

    private static void AddGeneratedPartStyle(
        Style ownerStyle,
        ControlSemanticDescriptor descriptor,
        string partName,
        object tag)
    {
        var part = descriptor.Parts.Single(candidate => candidate.Name == partName);
        var style = (Style)Activator.CreateInstance(part.StyleType.ShouldNotBeNull())
                                    .ShouldNotBeNull();
        style.Setters.Add(new Setter(Control.TagProperty, tag));
        ownerStyle.Children.Add(style);
    }

    private static AvaloniaWindow Show(AtomUIAlert alert)
    {
        var window = new AvaloniaWindow
        {
            Width = 560,
            Height = 240,
            Content = alert
        };
        window.Show();
        alert.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();
        return window;
    }

    private static void ReapplyDefaultTemplate(AtomUIAlert alert)
    {
        alert.SetValue(TemplatedControl.TemplateProperty, null);
        Dispatcher.UIThread.RunJobs();
        alert.ClearValue(TemplatedControl.TemplateProperty);
        alert.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();
    }

    private static T FindSemanticControl<T>(AtomUIAlert alert, string marker)
        where T : Control
    {
        return FindSemanticControls<T>(alert, marker).Single();
    }

    private static IReadOnlyList<T> FindSemanticControls<T>(AtomUIAlert alert, string marker)
        where T : Control
    {
        return alert.GetVisualDescendants()
                    .OfType<T>()
                    .Where(control => control.Classes.Contains(marker))
                    .ToArray();
    }

    private static bool HasMarker(XElement element, string marker)
    {
        var classProperty = element.Attribute($"Classes.{marker}");
        return classProperty is not null &&
               bool.TryParse(classProperty.Value, out var isEnabled) &&
               isEnabled;
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
