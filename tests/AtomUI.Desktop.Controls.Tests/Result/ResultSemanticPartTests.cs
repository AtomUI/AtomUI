using System.Reflection;
using System.Xml.Linq;
using AtomUI.Controls;
using AtomUI.Controls.Commons;
using AtomUI.Controls.Primitives;
using AtomUI.Icons.AntDesign;
using AtomUI.Theme;
using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomUIResult = AtomUI.Desktop.Controls.Result;
using AvaloniaWindow = Avalonia.Controls.Window;
using SvgControl = Avalonia.Svg.Svg;

namespace AtomUI.Desktop.Controls.Tests.Feedback;

public class ResultSemanticPartTests
{
    private const string IconClass = "semantic-icon";
    private const string TitleClass = "semantic-title";
    private const string SubTitleClass = "semantic-sub-title";
    private const string ExtraClass = "semantic-extra";
    private const string BodyClass = "semantic-body";
    private const string ThemePath = "src/AtomUI.Desktop.Controls/Result/Themes/ResultTheme.axaml";

    static ResultSemanticPartTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Registered_Descriptor_Exposes_The_Approved_Result_Parts()
    {
        var manager = Application.Current.ShouldNotBeNull().GetThemeManager().ShouldNotBeNull();

        manager.SemanticParts.TryGetControl(typeof(AtomUIResult), out var descriptor).ShouldBeTrue();
        descriptor.ShouldNotBeNull();
        descriptor.Parts.Select(static part => part.Name)
                  .ShouldBe(["root", "body", "extra", "icon", "subTitle", "title"]);

        AssertRoot(descriptor.Parts.Single(static part => part.Name == "root"));
        AssertPart(descriptor, "icon", IconClass, typeof(Control), SemanticPartCardinality.Multiple);
        AssertPart(descriptor, "title", TitleClass, typeof(ContentPresenter));
        AssertPart(descriptor, "subTitle", SubTitleClass, typeof(ContentPresenter));
        AssertPart(descriptor, "extra", ExtraClass, typeof(ContentPresenter));
        AssertPart(descriptor, "body", BodyClass, typeof(ContentPresenter));
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
                   "Classes.semantic-icon:ContentPresenter",
                   "Classes.semantic-icon:Svg",
                   "Classes.semantic-title:ContentPresenter",
                   "Classes.semantic-sub-title:ContentPresenter",
                   "Classes.semantic-extra:ContentPresenter",
                   "Classes.semantic-body:ContentPresenter"
               ]);
        markers.ShouldAllBe(static marker =>
            string.Equals(marker.Attribute.Value, "true", StringComparison.OrdinalIgnoreCase));
        template.Descendants().Any(static element => HasMarker(element, "semantic-root")).ShouldBeFalse();

        var frame = template.Elements().Single();
        frame.Name.LocalName.ShouldBe(nameof(DashedBorder));
        frame.Attribute("Name")?.Value.ShouldBe("Frame");
        frame.Attribute("Background")?.Value.ShouldBe("{TemplateBinding Background}");
        frame.Attribute("BorderBrush")?.Value.ShouldBe("{TemplateBinding BorderBrush}");
        frame.Attribute("BorderThickness")?.Value.ShouldBe("{TemplateBinding BorderThickness}");
        frame.Attribute("CornerRadius")?.Value.ShouldBe("{TemplateBinding CornerRadius}");
        frame.Attribute("StrokeDashArray")?.Value.ShouldBe("{TemplateBinding StrokeDashArray}");
        frame.Attribute("Padding")?.Value.ShouldBe("{TemplateBinding Padding}");
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
    public void Generated_Semantic_Styles_Apply_To_All_Six_Template_Targets()
    {
        var manager = Application.Current.ShouldNotBeNull().GetThemeManager().ShouldNotBeNull();
        manager.SemanticParts.TryGetControl(typeof(AtomUIResult), out var descriptor).ShouldBeTrue();
        descriptor.ShouldNotBeNull();

        var result = CreateRichResult();
        result.Classes.Add("styled-result");
        var ownerStyle = new Style(selector => selector.OfType<AtomUIResult>().Class("styled-result"));
        foreach (var partName in new[] { "icon", "title", "subTitle", "extra", "body" })
        {
            AddGeneratedPartStyle(ownerStyle, descriptor, partName, $"styled-{partName}");
        }
        result.Styles.Add(ownerStyle);

        var window = Show(result);
        try
        {
            var iconTargets = FindIconTargets(result);
            iconTargets.ShouldAllBe(static control => Equals(control.Tag, "styled-icon"));
            iconTargets.OfType<ContentPresenter>()
                       .Single()
                       .GetVisualDescendants()
                       .OfType<Control>()
                       .ShouldAllBe(static control => !Equals(control.Tag, "styled-icon"));
            FindSemanticControl<ContentPresenter>(result, TitleClass).Tag.ShouldBe("styled-title");
            FindSemanticControl<ContentPresenter>(result, SubTitleClass).Tag.ShouldBe("styled-subTitle");
            FindSemanticControl<ContentPresenter>(result, ExtraClass).Tag.ShouldBe("styled-extra");
            FindSemanticControl<ContentPresenter>(result, BodyClass).Tag.ShouldBe("styled-body");
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Root_Style_Projects_The_Standard_Result_Surface()
    {
        var background = new SolidColorBrush(Color.Parse("#FFFFFF"));
        var borderBrush = new SolidColorBrush(Color.Parse("#1890FF"));
        var padding = new Thickness(16);
        var borderThickness = new Thickness(2);
        var cornerRadius = new CornerRadius(6);
        IReadOnlyList<double> strokeDashArray = [4d, 2d];
        var result = CreateRichResult();

        var strokeDashArrayProperty = typeof(AbstractResult)
                                      .GetProperty("StrokeDashArray", BindingFlags.Instance | BindingFlags.Public);
        strokeDashArrayProperty.ShouldNotBeNull();
        strokeDashArrayProperty.SetValue(result, strokeDashArray);
        result.Background = background;
        result.BorderBrush = borderBrush;
        result.BorderThickness = borderThickness;
        result.CornerRadius = cornerRadius;
        result.Padding = padding;

        var window = Show(result);
        try
        {
            var frame = result.GetVisualChildren()
                              .OfType<DashedBorder>()
                              .Single(static border => border.Name == "Frame");
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

    [Fact]
    public void Status_And_Content_Changes_Preserve_Static_Marker_Identity()
    {
        var result = CreateRichResult();
        var window = Show(result);
        try
        {
            var icons = FindIconTargets(result);
            var title = FindSemanticControl<ContentPresenter>(result, TitleClass);
            var subTitle = FindSemanticControl<ContentPresenter>(result, SubTitleClass);
            var extra = FindSemanticControl<ContentPresenter>(result, ExtraClass);
            var body = FindSemanticControl<ContentPresenter>(result, BodyClass);

            icons.Count.ShouldBe(2);
            foreach (var status in Enum.GetValues<ResultStatus>())
            {
                result.Status = status;
                Dispatcher.UIThread.RunJobs();

                FindIconTargets(result).ShouldBe(icons);
                icons.Count(static icon => icon.IsVisible).ShouldBe(1);
            }

            result.Status = ResultStatus.Success;
            result.Icon = new SmileOutlined();
            result.Header = null;
            result.SubHeader = null;
            result.Extra = null;
            result.Content = null;
            Dispatcher.UIThread.RunJobs();

            FindIconTargets(result).ShouldBe(icons);
            FindSemanticControl<ContentPresenter>(result, TitleClass).ShouldBeSameAs(title);
            FindSemanticControl<ContentPresenter>(result, SubTitleClass).ShouldBeSameAs(subTitle);
            FindSemanticControl<ContentPresenter>(result, ExtraClass).ShouldBeSameAs(extra);
            FindSemanticControl<ContentPresenter>(result, BodyClass).ShouldBeSameAs(body);
            title.IsVisible.ShouldBeFalse();
            subTitle.IsVisible.ShouldBeFalse();
            extra.IsVisible.ShouldBeTrue();
            body.IsVisible.ShouldBeFalse();

            var iconPresenter = icons.OfType<ContentPresenter>().Single();
            iconPresenter.Child.ShouldBeOfType<SmileOutlined>();
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Header_Font_Size_Changes_Recompute_The_Corresponding_Line_Height()
    {
        var result = CreateRichResult();
        var window = Show(result);
        try
        {
            var title = FindSemanticControl<ContentPresenter>(result, TitleClass);
            var subTitle = FindSemanticControl<ContentPresenter>(result, SubTitleClass);
            var titleRelativeLineHeight = title.LineHeight / title.FontSize;
            var subTitleRelativeLineHeight = subTitle.LineHeight / subTitle.FontSize;

            result.HeaderFontSize = 40;
            result.SubHeaderFontSize = 20;
            Dispatcher.UIThread.RunJobs();

            title.FontSize.ShouldBe(40);
            title.LineHeight.ShouldBe(titleRelativeLineHeight * 40, 0.001);
            subTitle.FontSize.ShouldBe(20);
            subTitle.LineHeight.ShouldBe(subTitleRelativeLineHeight * 20, 0.001);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Extra_Semantic_Part_Stretches_Across_Result_And_Centers_Its_Content()
    {
        var result = CreateRichResult();
        var window = Show(result);
        try
        {
            var rootLayout = result.GetVisualDescendants()
                                   .OfType<StackPanel>()
                                   .Single(static panel => panel.Name == "RootLayout");
            var extra = FindSemanticControl<ContentPresenter>(result, ExtraClass);

            extra.Bounds.Width.ShouldBe(rootLayout.Bounds.Width, 0.001);
            extra.HorizontalContentAlignment.ShouldBe(HorizontalAlignment.Center);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Retemplate_Releases_The_Old_Icon_Presenter_And_Preserves_Markers()
    {
        var result = CreateRichResult();
        var window = Show(result);
        try
        {
            var oldPresenter = FindSemanticControl<ContentPresenter>(result, IconClass);
            var oldImage = FindSemanticControl<SvgControl>(result, IconClass);

            ReapplyDefaultTemplate(result);

            var newPresenter = FindSemanticControl<ContentPresenter>(result, IconClass);
            var newImage = FindSemanticControl<SvgControl>(result, IconClass);
            newPresenter.ShouldNotBeSameAs(oldPresenter);
            newImage.ShouldNotBeSameAs(oldImage);
            FindIconTargets(result).Count.ShouldBe(2);

            var currentIcon = newPresenter.Child.ShouldBeAssignableTo<Control>()!;
            currentIcon.Width = 13;
            oldPresenter.Content = new SmileOutlined();
            oldPresenter.ApplyTemplate();
            Dispatcher.UIThread.RunJobs();

            currentIcon.Width.ShouldBe(13);
        }
        finally
        {
            window.Close();
        }
    }

    private static AtomUIResult CreateRichResult()
    {
        return new AtomUIResult
        {
            Status = ResultStatus.Info,
            Header = "classNames Object",
            SubHeader = "This is a subtitle",
            Extra = new Button { Content = "Action" },
            Content = "Content area"
        };
    }

    private static void AssertRoot(SemanticPartDescriptor part)
    {
        part.Path.ShouldBe("root");
        part.SelectorClass.ShouldBeNull();
        part.SelectorRoute.ShouldBeNull();
        part.ContractType.ShouldBe(typeof(AtomUIResult));
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
        part.StyleType.ShouldBe(typeof(AtomUIResult).Assembly.GetType(
            $"AtomUI.Theme.Styling.Result{char.ToUpperInvariant(name[0])}{name[1..]}Style"));
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

    private static AvaloniaWindow Show(AtomUIResult result)
    {
        var window = new AvaloniaWindow
        {
            Width = 640,
            Height = 560,
            Content = result
        };
        window.Show();
        result.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();
        return window;
    }

    private static void ReapplyDefaultTemplate(AtomUIResult result)
    {
        result.SetValue(TemplatedControl.TemplateProperty, null);
        Dispatcher.UIThread.RunJobs();
        result.ClearValue(TemplatedControl.TemplateProperty);
        result.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();
    }

    private static T FindSemanticControl<T>(AtomUIResult result, string marker)
        where T : Control
    {
        return FindSemanticControls<T>(result, marker).Single();
    }

    private static IReadOnlyList<Control> FindIconTargets(AtomUIResult result)
    {
        return
        [
            FindSemanticControl<ContentPresenter>(result, IconClass),
            FindSemanticControl<SvgControl>(result, IconClass)
        ];
    }

    private static IReadOnlyList<T> FindSemanticControls<T>(AtomUIResult result, string marker)
        where T : Control
    {
        return result.GetVisualDescendants()
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
