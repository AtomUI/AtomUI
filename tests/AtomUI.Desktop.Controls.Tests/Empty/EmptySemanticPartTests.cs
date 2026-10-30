using System.Reflection;
using System.Xml.Linq;
using AtomUI.Controls;
using AtomUI.Controls.Commons;
using AtomUI.Controls.Primitives;
using AtomUI.Theme;
using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomUIEmpty = AtomUI.Desktop.Controls.Empty;
using AtomUITextBlock = AtomUI.Desktop.Controls.TextBlock;
using AvaloniaButton = Avalonia.Controls.Button;
using AvaloniaSvg = Avalonia.Svg.Svg;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.DataDisplay;

public class EmptySemanticPartTests
{
    private const string ImageClass = "semantic-image";
    private const string DescriptionClass = "semantic-description";
    private const string FooterClass = "semantic-footer";
    private const string ThemePath = "src/AtomUI.Desktop.Controls/Empty/Themes/EmptyTheme.axaml";

    static EmptySemanticPartTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Registered_Descriptor_Exposes_Only_The_Approved_Parts()
    {
        var manager = Application.Current.ShouldNotBeNull().GetThemeManager().ShouldNotBeNull();

        manager.SemanticParts.TryGetControl(typeof(AtomUIEmpty), out var descriptor).ShouldBeTrue();
        descriptor.ShouldNotBeNull();
        descriptor.Parts.Select(static part => part.Name)
                  .ShouldBe(["root", "description", "footer", "image"]);

        AssertRoot(descriptor.Parts.Single(static part => part.Name == "root"));
        AssertPart(
            descriptor.Parts.Single(static part => part.Name == "image"),
            ImageClass,
            typeof(Control),
            typeof(AtomUIEmpty).Assembly.GetType("AtomUI.Theme.Styling.EmptyImageStyle"));
        AssertPart(
            descriptor.Parts.Single(static part => part.Name == "description"),
            DescriptionClass,
            typeof(AtomUITextBlock),
            typeof(AtomUIEmpty).Assembly.GetType("AtomUI.Theme.Styling.EmptyDescriptionStyle"));
        AssertPart(
            descriptor.Parts.Single(static part => part.Name == "footer"),
            FooterClass,
            typeof(ContentPresenter),
            typeof(AtomUIEmpty).Assembly.GetType("AtomUI.Theme.Styling.EmptyFooterStyle"));
    }

    [Fact]
    public void Built_In_Template_Implements_The_Static_Marker_And_Binding_Contract()
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
                   "Classes.semantic-image:Svg",
                   "Classes.semantic-description:TextBlock",
                   "Classes.semantic-footer:ContentPresenter"
               ]);
        markers.ShouldAllBe(static marker =>
            string.Equals(marker.Attribute.Value, "true", StringComparison.OrdinalIgnoreCase));
        template.Descendants().Any(static element => HasMarker(element, "semantic-root")).ShouldBeFalse();

        var description = template.Descendants()
                                  .Single(static element => HasMarker(element, DescriptionClass));
        description.Attribute("Text")?.Value.ShouldBe("{TemplateBinding Description}");
        description.Attribute("IsVisible")?.Value.ShouldBe("{TemplateBinding IsDescriptionVisible}");

        var footer = template.Descendants()
                             .Single(static element => HasMarker(element, FooterClass));
        footer.Attribute("Content")?.Value.ShouldBe("{TemplateBinding Footer}");
        footer.Attribute("ContentTemplate")?.Value.ShouldBe("{TemplateBinding FooterTemplate}");
        footer.Attribute("IsVisible")?.Value
              .ShouldBe("{TemplateBinding Footer, Converter={x:Static ObjectConverters.IsNotNull}}");

        var rootSurface = template.Elements().Single();
        rootSurface.Name.LocalName.ShouldBe(nameof(DashedBorder));
        rootSurface.Attribute("StrokeDashArray")?.Value.ShouldBe("{TemplateBinding StrokeDashArray}");
    }

    [Fact]
    public void Built_In_Theme_Does_Not_Consume_Semantic_Selectors_Or_Expose_A_Root_Marker()
    {
        var document = XDocument.Load(GetRepoFile(ThemePath), LoadOptions.SetLineInfo);

        document.Descendants()
                .Where(static element => element.Name.LocalName == "Style")
                .Select(static element => (string?)element.Attribute("Selector"))
                .Where(static selector => selector?.Contains(".semantic-", StringComparison.Ordinal) == true)
                .ShouldBeEmpty();
        document.Descendants()
                .Any(static element => HasMarker(element, "semantic-root"))
                .ShouldBeFalse();
    }

    [Fact]
    public void Footer_Public_API_Is_Styled_And_Defaults_To_Null()
    {
        var footerProperty = GetPublicProperty("Footer");
        var footerTemplateProperty = GetPublicProperty("FooterTemplate");
        var footerStyledProperty = GetPublicStyledProperty("FooterProperty");
        var footerTemplateStyledProperty = GetPublicStyledProperty("FooterTemplateProperty");
        var empty = new AtomUIEmpty();

        footerProperty.PropertyType.ShouldBe(typeof(object));
        footerTemplateProperty.PropertyType.ShouldBe(typeof(IDataTemplate));
        footerStyledProperty.PropertyType.ShouldBe(typeof(object));
        footerTemplateStyledProperty.PropertyType.ShouldBe(typeof(IDataTemplate));
        footerProperty.GetValue(empty).ShouldBeNull();
        footerTemplateProperty.GetValue(empty).ShouldBeNull();
    }

    [Fact]
    public void Root_Stroke_Dash_API_Is_Styled_And_Defaults_To_Null()
    {
        var strokeDashArrayProperty = GetPublicProperty("StrokeDashArray");
        var strokeDashArrayStyledProperty = GetPublicStyledProperty("StrokeDashArrayProperty");
        var empty = new AtomUIEmpty();

        strokeDashArrayProperty.PropertyType.ShouldBe(typeof(IReadOnlyList<double>));
        strokeDashArrayStyledProperty.PropertyType.ShouldBe(typeof(IReadOnlyList<double>));
        strokeDashArrayProperty.GetValue(empty).ShouldBeNull();
    }

    [Fact]
    public void Footer_Supports_Late_Content_Control_Template_And_Null_Toggles()
    {
        var empty = new AtomUIEmpty
        {
            PresetImage = PresetEmptyImage.Default
        };
        var window = Show(empty);

        try
        {
            var footer = FindSemanticControl<ContentPresenter>(empty, FooterClass);
            footer.IsVisible.ShouldBeFalse();

            SetFooter(empty, "Create Now");
            Dispatcher.UIThread.RunJobs();
            footer.IsVisible.ShouldBeTrue();
            footer.Content.ShouldBe("Create Now");

            var action = new AvaloniaButton { Content = "Retry" };
            SetFooter(empty, action);
            Dispatcher.UIThread.RunJobs();
            footer.Content.ShouldBeSameAs(action);
            footer.GetVisualDescendants().ShouldContain(action);

            var template = new FuncDataTemplate<object?>((_, _) => new Border { Tag = "templated-footer" });
            SetFooterTemplate(empty, template);
            SetFooter(empty, new object());
            Dispatcher.UIThread.RunJobs();
            footer.ContentTemplate.ShouldBeSameAs(template);
            footer.GetVisualDescendants()
                  .OfType<Border>()
                  .Single(static border => Equals(border.Tag, "templated-footer"));

            SetFooter(empty, null);
            Dispatcher.UIThread.RunJobs();
            footer.IsVisible.ShouldBeFalse();
            footer.Content.ShouldBeNull();
            footer.GetVisualDescendants().ShouldNotContain(action);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Description_Visibility_Works_Before_Template_At_Runtime_And_After_Retemplate()
    {
        var empty = new AtomUIEmpty
        {
            PresetImage = PresetEmptyImage.Simple,
            IsDescriptionVisible = false
        };
        var window = Show(empty);

        try
        {
            var firstDescription = FindSemanticControl<AtomUITextBlock>(empty, DescriptionClass);
            firstDescription.IsVisible.ShouldBeFalse();

            empty.IsDescriptionVisible = true;
            Dispatcher.UIThread.RunJobs();
            firstDescription.IsVisible.ShouldBeTrue();

            empty.IsDescriptionVisible = false;
            Dispatcher.UIThread.RunJobs();
            firstDescription.IsVisible.ShouldBeFalse();

            ReapplyDefaultTemplate(empty);

            var secondDescription = FindSemanticControl<AtomUITextBlock>(empty, DescriptionClass);
            secondDescription.ShouldNotBeSameAs(firstDescription);
            secondDescription.IsVisible.ShouldBeFalse();
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Image_Source_Modes_Update_The_Renderer_Without_Replacing_The_Semantic_Target()
    {
        const string imageSource = "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"1\" height=\"1\" />";
        var imageFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.svg");
        File.WriteAllText(imageFile, imageSource);
        var empty = new AtomUIEmpty
        {
            PresetImage = PresetEmptyImage.Default
        };
        var window = Show(empty);

        try
        {
            var image = FindSemanticControl<AvaloniaSvg>(empty, ImageClass);
            var defaultSource = image.Source.ShouldNotBeNull();
            image.Path.ShouldBeNull();

            empty.PresetImage = PresetEmptyImage.Simple;
            Dispatcher.UIThread.RunJobs();
            FindSemanticControl<AvaloniaSvg>(empty, ImageClass).ShouldBeSameAs(image);
            image.Source.ShouldNotBeNull().ShouldNotBe(defaultSource);
            image.Path.ShouldBeNull();

            empty.PresetImage = null;
            var imagePath = new Uri(imageFile).AbsoluteUri;
            empty.ImagePath = imagePath;
            Dispatcher.UIThread.RunJobs();
            FindSemanticControl<AvaloniaSvg>(empty, ImageClass).ShouldBeSameAs(image);
            image.Path.ShouldBe(imagePath);
            image.Source.ShouldBeNull();

            empty.ImagePath = null;
            empty.ImageSource = imageSource;
            Dispatcher.UIThread.RunJobs();
            FindSemanticControl<AvaloniaSvg>(empty, ImageClass).ShouldBeSameAs(image);
            image.Source.ShouldBe(imageSource);
            image.Path.ShouldBeNull();
        }
        finally
        {
            window.Close();
            File.Delete(imageFile);
        }
    }

    [Theory]
    [InlineData("Large", "EmptyImgHeight", "DescriptionMargin")]
    [InlineData("Middle", "EmptyImgHeightMD", "DescriptionMarginSM")]
    [InlineData("Small", "EmptyImgHeightSM", "DescriptionMarginSM")]
    public void Size_Baselines_Keep_Image_Description_And_Footer_Token_Routes(
        string sizeType,
        string imageToken,
        string descriptionToken)
    {
        var document = XDocument.Load(GetRepoFile(ThemePath), LoadOptions.SetLineInfo);
        var sizeStyles = document.Descendants()
                                 .Where(static element => element.Name.LocalName == "Style")
                                 .Where(element => (string?)element.Attribute("Selector") == $"^[SizeType={sizeType}]")
                                 .ToArray();

        sizeStyles.SelectMany(static style => style.Descendants())
                  .Where(static element => element.Name.LocalName == "Setter")
                  .Select(static setter => (string?)setter.Attribute("Value"))
                  .ShouldContain($"{{atom:EmptyTokenResource {imageToken}}}");
        sizeStyles.SelectMany(static style => style.Descendants())
                  .Where(static element => element.Name.LocalName == "Setter")
                  .Select(static setter => (string?)setter.Attribute("Value"))
                  .ShouldContain($"{{atom:EmptyTokenResource {descriptionToken}}}");

        document.Descendants()
                .Where(static element => element.Name.LocalName == "Setter")
                .Where(static setter => (string?)setter.Attribute("Property") == "Margin")
                .Select(static setter => (string?)setter.Attribute("Value"))
                .ShouldContain("{atom:EmptyTokenResource FooterMargin}");
    }

    [Fact]
    public void Generated_Semantic_Styles_Apply_To_All_Three_Targets()
    {
        var manager = Application.Current.ShouldNotBeNull().GetThemeManager().ShouldNotBeNull();
        manager.SemanticParts.TryGetControl(typeof(AtomUIEmpty), out var descriptor).ShouldBeTrue();
        descriptor.ShouldNotBeNull();

        var empty = new AtomUIEmpty
        {
            PresetImage = PresetEmptyImage.Default
        };
        empty.Classes.Add("styled-empty");
        var ownerStyle = new Style(selector => selector.OfType<AtomUIEmpty>().Class("styled-empty"));
        AddGeneratedPartStyle(ownerStyle, descriptor, "image", "styled-image");
        AddGeneratedPartStyle(ownerStyle, descriptor, "description", "styled-description");
        AddGeneratedPartStyle(ownerStyle, descriptor, "footer", "styled-footer");
        empty.Styles.Add(ownerStyle);

        var window = Show(empty);
        try
        {
            FindSemanticControl<Control>(empty, ImageClass).Tag.ShouldBe("styled-image");
            FindSemanticControl<AtomUITextBlock>(empty, DescriptionClass).Tag.ShouldBe("styled-description");
            FindSemanticControl<ContentPresenter>(empty, FooterClass).Tag.ShouldBe("styled-footer");
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Root_Style_Projects_The_Standard_TemplatedControl_Surface()
    {
        var background = new SolidColorBrush(Color.Parse("#E8E4FF"));
        var borderBrush = new SolidColorBrush(Color.Parse("#696FC7"));
        var padding = new Thickness(12);
        var borderThickness = new Thickness(2);
        var cornerRadius = new CornerRadius(8);
        IReadOnlyList<double> strokeDashArray = [4d, 2d];
        var strokeDashArrayProperty = GetPublicStyledProperty("StrokeDashArrayProperty");
        var empty = new AtomUIEmpty
        {
            PresetImage = PresetEmptyImage.Simple
        };
        empty.Classes.Add("styled-root");
        empty.Styles.Add(new Style(selector => selector.OfType<AtomUIEmpty>().Class("styled-root"))
        {
            Setters =
            {
                new Setter(TemplatedControl.BackgroundProperty, background),
                new Setter(TemplatedControl.BorderBrushProperty, borderBrush),
                new Setter(TemplatedControl.BorderThicknessProperty, borderThickness),
                new Setter(TemplatedControl.CornerRadiusProperty, cornerRadius),
                new Setter(TemplatedControl.PaddingProperty, padding),
                new Setter(strokeDashArrayProperty, strokeDashArray)
            }
        });

        var window = Show(empty);
        try
        {
            var frame = empty.GetVisualChildren()
                             .OfType<DashedBorder>()
                             .Single(static border => border.TemplatedParent is AtomUIEmpty);
            frame.Background.ShouldBeSameAs(background);
            frame.BorderBrush.ShouldBeSameAs(borderBrush);
            frame.BorderThickness.ShouldBe(borderThickness);
            frame.CornerRadius.ShouldBe(cornerRadius);
            frame.Padding.ShouldBe(padding);
            frame.StrokeDashArray.ShouldBeSameAs(strokeDashArray);
        }
        finally
        {
            window.Close();
        }
    }

    private static void AssertRoot(SemanticPartDescriptor part)
    {
        part.Path.ShouldBe("root");
        part.SelectorClass.ShouldBeNull();
        part.SelectorRoute.ShouldBeNull();
        part.ContractType.ShouldBe(typeof(AtomUIEmpty));
        part.Cardinality.ShouldBe(SemanticPartCardinality.Single);
        part.Customization.ShouldBe(SemanticPartCustomization.Root);
        part.StyleType.ShouldBeNull();
        part.CrossVisualRoot.ShouldBeFalse();
        part.RuntimeCreated.ShouldBeFalse();
        part.Since.ShouldBe("6.2.0");
    }

    private static void AssertPart(
        SemanticPartDescriptor part,
        string selectorClass,
        Type contractType,
        Type? expectedStyleType)
    {
        expectedStyleType.ShouldNotBeNull();
        part.Path.ShouldBe(part.Name);
        part.SelectorClass.ShouldBe(selectorClass);
        part.SelectorRoute.ShouldBe($"/template/ .{selectorClass}");
        part.ContractType.ShouldBe(contractType);
        part.Cardinality.ShouldBe(SemanticPartCardinality.Single);
        part.Customization.ShouldBe(SemanticPartCustomization.Selector);
        part.StyleType.ShouldBe(expectedStyleType);
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

    private static AvaloniaWindow Show(AtomUIEmpty empty)
    {
        var window = new AvaloniaWindow
        {
            Width = 360,
            Height = 240,
            Content = empty
        };
        window.Show();
        empty.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();
        return window;
    }

    private static void ReapplyDefaultTemplate(AtomUIEmpty empty)
    {
        empty.SetValue(TemplatedControl.TemplateProperty, null);
        Dispatcher.UIThread.RunJobs();
        empty.ClearValue(TemplatedControl.TemplateProperty);
        empty.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();
    }

    private static T FindSemanticControl<T>(AtomUIEmpty empty, string marker)
        where T : Control
    {
        return empty.GetVisualDescendants()
                    .OfType<T>()
                    .Single(control => control.Classes.Contains(marker));
    }

    private static void SetFooter(AtomUIEmpty empty, object? value)
    {
        GetPublicProperty("Footer").SetValue(empty, value);
    }

    private static void SetFooterTemplate(AtomUIEmpty empty, IDataTemplate? value)
    {
        GetPublicProperty("FooterTemplate").SetValue(empty, value);
    }

    private static PropertyInfo GetPublicProperty(string name)
    {
        return typeof(AbstractEmpty).GetProperty(name, BindingFlags.Instance | BindingFlags.Public)
                                    .ShouldNotBeNull();
    }

    private static AvaloniaProperty GetPublicStyledProperty(string name)
    {
        return typeof(AbstractEmpty).GetField(name, BindingFlags.Static | BindingFlags.Public)
                                    .ShouldNotBeNull()
                                    .GetValue(null)
                                    .ShouldNotBeNull()
                                    .ShouldBeAssignableTo<AvaloniaProperty>();
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
