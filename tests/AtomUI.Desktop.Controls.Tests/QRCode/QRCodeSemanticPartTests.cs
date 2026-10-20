using System.Xml.Linq;
using AtomUI.Controls;
using AtomUI.Controls.Commons;
using AtomUI.Controls.Primitives;
using AtomUI.Theme;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomUIQRCode = AtomUI.Desktop.Controls.QRCode;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.DataDisplay;

public class QRCodeSemanticPartTests
{
    private const string CoverClass = "semantic-cover";
    private const string ThemePath = "src/AtomUI.Desktop.Controls/QRCode/Themes/QRCodeTheme.axaml";

    static QRCodeSemanticPartTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Registered_Descriptor_Exposes_Only_Root_And_Cover()
    {
        var manager = Application.Current.ShouldNotBeNull().GetThemeManager().ShouldNotBeNull();

        manager.SemanticParts.TryGetControl(typeof(AtomUIQRCode), out var descriptor).ShouldBeTrue();
        descriptor.ShouldNotBeNull();
        descriptor.Parts.Select(static part => part.Name).ShouldBe(["root", "cover"]);

        var root = descriptor.Parts.Single(static part => part.Name == "root");
        root.Path.ShouldBe("root");
        root.SelectorClass.ShouldBeNull();
        root.SelectorRoute.ShouldBeNull();
        root.ContractType.ShouldBe(typeof(AtomUIQRCode));
        root.Cardinality.ShouldBe(SemanticPartCardinality.Single);
        root.Customization.ShouldBe(SemanticPartCustomization.Root);
        root.StyleType.ShouldBeNull();
        root.CrossVisualRoot.ShouldBeFalse();
        root.RuntimeCreated.ShouldBeFalse();
        root.Since.ShouldBe("6.2.0");

        var cover = descriptor.Parts.Single(static part => part.Name == "cover");
        cover.Path.ShouldBe("cover");
        cover.SelectorClass.ShouldBe(CoverClass);
        cover.SelectorRoute.ShouldBe($"/template/ .{CoverClass}");
        cover.ContractType.ShouldBe(typeof(Border));
        cover.Cardinality.ShouldBe(SemanticPartCardinality.Single);
        cover.Customization.ShouldBe(SemanticPartCustomization.Selector);
        cover.StyleType.ShouldBe(typeof(AtomUIQRCode).Assembly.GetType("AtomUI.Theme.Styling.QRCodeCoverStyle"));
        cover.CrossVisualRoot.ShouldBeFalse();
        cover.RuntimeCreated.ShouldBeFalse();
        cover.Since.ShouldBe("6.2.0");
    }

    [Fact]
    public void Built_In_Template_Implements_The_Static_Cover_And_Root_Surface_Contract()
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
               .ShouldBe(["Classes.semantic-cover:Border"]);
        markers.ShouldAllBe(static marker =>
            string.Equals(marker.Attribute.Value, "true", StringComparison.OrdinalIgnoreCase));
        template.Descendants().Any(static element => HasMarker(element, "semantic-root")).ShouldBeFalse();

        var rootSurface = template.Elements().Single();
        rootSurface.Name.LocalName.ShouldBe("PixelAlignedBorder");
        rootSurface.Attribute("Name")?.Value.ShouldBe("Frame");
        rootSurface.Attribute("Width")?.Value.ShouldBe("{TemplateBinding Size}");
        rootSurface.Attribute("Height")?.Value.ShouldBe("{TemplateBinding Size}");
        rootSurface.Attribute("Background")?.Value.ShouldBe("{TemplateBinding Background}");
        rootSurface.Attribute("BorderBrush")?.Value.ShouldBe("{TemplateBinding BorderBrush}");
        rootSurface.Attribute("BorderThickness")?.Value.ShouldBe("{TemplateBinding BorderThickness}");
        rootSurface.Attribute("CornerRadius")?.Value.ShouldBe("{TemplateBinding CornerRadius}");

        var contentFrame = template.Descendants()
                                   .Single(static element => (string?)element.Attribute("Name") == "ContentFrame");
        contentFrame.Name.LocalName.ShouldBe("Border");
        contentFrame.Attribute("Padding")?.Value.ShouldBe("{TemplateBinding Padding}");

        var cover = template.Descendants().Single(static element => HasMarker(element, CoverClass));
        cover.Name.LocalName.ShouldBe("Border");
        cover.Attribute("Name")?.Value.ShouldBe("Cover");
        cover.Descendants()
             .Where(static element => element.Name.LocalName == "Panel")
             .Select(static element => (string?)element.Attribute("Name"))
             .ShouldContain("LoadingLayout");
        cover.Descendants()
             .Where(static element => element.Name.LocalName == "Panel")
             .Select(static element => (string?)element.Attribute("Name"))
             .ShouldContain("ExpiredLayout");
        cover.Descendants()
             .Where(static element => element.Name.LocalName == "Panel")
             .Select(static element => (string?)element.Attribute("Name"))
             .ShouldContain("ScannedLayout");
    }

    [Fact]
    public void Built_In_Theme_Does_Not_Consume_The_Semantic_Cover_Selector()
    {
        var document = XDocument.Load(GetRepoFile(ThemePath), LoadOptions.SetLineInfo);

        document.Descendants()
                .Where(static element => element.Name.LocalName == "Style")
                .Select(static element => (string?)element.Attribute("Selector"))
                .Where(static selector => selector?.Contains(".semantic-", StringComparison.Ordinal) == true)
                .ShouldBeEmpty();
    }

    [Fact]
    public void Status_Changes_Reuse_The_Same_Cover_Target()
    {
        var qrCode = new AtomUIQRCode
        {
            Value = "https://ant.design",
            Status = QRCodeStatus.Active
        };
        var window = Show(qrCode);

        try
        {
            var cover = FindCover(qrCode);
            cover.IsVisible.ShouldBeFalse();

            foreach (var status in new[] { QRCodeStatus.Loading, QRCodeStatus.Expired, QRCodeStatus.Scanned })
            {
                qrCode.Status = status;
                Dispatcher.UIThread.RunJobs();
                FindCover(qrCode).ShouldBeSameAs(cover);
                cover.IsVisible.ShouldBeTrue();
            }

            qrCode.Status = QRCodeStatus.Active;
            Dispatcher.UIThread.RunJobs();
            FindCover(qrCode).ShouldBeSameAs(cover);
            cover.IsVisible.ShouldBeFalse();
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Generated_Cover_Style_And_Root_Style_Project_To_Their_Approved_Targets()
    {
        var manager = Application.Current.ShouldNotBeNull().GetThemeManager().ShouldNotBeNull();
        manager.SemanticParts.TryGetControl(typeof(AtomUIQRCode), out var descriptor).ShouldBeTrue();
        descriptor.ShouldNotBeNull();

        var background = new SolidColorBrush(Color.Parse("#1A1890FF"));
        var borderBrush = new SolidColorBrush(Color.Parse("#1890FF"));
        var padding = new Thickness(16);
        var borderThickness = new Thickness(2);
        var cornerRadius = new CornerRadius(8);
        var qrCode = new AtomUIQRCode
        {
            Value = "https://ant.design/",
            Status = QRCodeStatus.Loading
        };
        qrCode.Classes.Add("semantic-demo");
        var ownerStyle = new Style(selector => selector.OfType<AtomUIQRCode>().Class("semantic-demo"))
        {
            Setters =
            {
                new Setter(TemplatedControl.BackgroundProperty, background),
                new Setter(TemplatedControl.BorderBrushProperty, borderBrush),
                new Setter(TemplatedControl.BorderThicknessProperty, borderThickness),
                new Setter(TemplatedControl.CornerRadiusProperty, cornerRadius),
                new Setter(TemplatedControl.PaddingProperty, padding)
            }
        };
        var coverPart = descriptor.Parts.Single(static part => part.Name == "cover");
        var coverStyle = (Style)Activator.CreateInstance(coverPart.StyleType.ShouldNotBeNull()).ShouldNotBeNull();
        coverStyle.Setters.Add(new Setter(Control.TagProperty, "styled-cover"));
        ownerStyle.Children.Add(coverStyle);
        qrCode.Styles.Add(ownerStyle);

        var window = Show(qrCode);
        try
        {
            var frame = qrCode.GetVisualChildren()
                              .OfType<PixelAlignedBorder>()
                              .Single(static border => border.Name == "Frame");
            var contentFrame = qrCode.GetVisualDescendants()
                                     .OfType<Border>()
                                     .Single(static border => border.Name == "ContentFrame");
            var cover = FindCover(qrCode);

            frame.Width.ShouldBe(qrCode.Size);
            frame.Height.ShouldBe(qrCode.Size);
            frame.Background.ShouldBeSameAs(background);
            frame.BorderBrush.ShouldBeSameAs(borderBrush);
            frame.BorderThickness.ShouldBe(borderThickness);
            frame.CornerRadius.ShouldBe(cornerRadius);
            contentFrame.Padding.ShouldBe(padding);
            cover.Tag.ShouldBe("styled-cover");
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Semitransparent_Root_Background_Does_Not_Regenerate_The_Bitmap()
    {
        var qrCode = new AtomUIQRCode
        {
            Value = "https://ant.design/"
        };
        var window = Show(qrCode);

        try
        {
            var bitmapProperty = typeof(AbstractQRCode).GetProperty(
                "Bitmap",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var originalBitmap = bitmapProperty.ShouldNotBeNull().GetValue(qrCode).ShouldNotBeNull();
            var background = new SolidColorBrush(Color.Parse("#1A1890FF"));

            qrCode.Background = background;
            Dispatcher.UIThread.RunJobs();

            bitmapProperty.GetValue(qrCode).ShouldBeSameAs(originalBitmap);
            qrCode.GetVisualChildren()
                  .OfType<PixelAlignedBorder>()
                  .Single(static border => border.Name == "Frame")
                  .Background.ShouldBeSameAs(background);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Icon_And_IconSize_Changes_Regenerate_The_Bitmap_For_Excavation()
    {
        using var icon = new RenderTargetBitmap(new PixelSize(40, 40), new Vector(96, 96));
        var qrCode = new AtomUIQRCode
        {
            Value = "https://ant.design/"
        };
        var window = Show(qrCode);

        try
        {
            var bitmapProperty = typeof(AbstractQRCode).GetProperty(
                "Bitmap",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var originalBitmap = bitmapProperty.ShouldNotBeNull().GetValue(qrCode).ShouldNotBeNull();

            qrCode.Icon = icon;
            Dispatcher.UIThread.RunJobs();
            var bitmapWithIcon = bitmapProperty.GetValue(qrCode).ShouldNotBeNull();
            bitmapWithIcon.ShouldNotBeSameAs(originalBitmap);

            qrCode.IconSize = 48;
            Dispatcher.UIThread.RunJobs();
            bitmapProperty.GetValue(qrCode).ShouldNotBeSameAs(bitmapWithIcon);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Built_In_Template_Scales_The_Bitmap_And_Icon_In_The_Same_QR_Surface()
    {
        var document = XDocument.Load(GetRepoFile(ThemePath), LoadOptions.SetLineInfo);
        var template = document.Descendants()
                               .Single(static element => element.Name.LocalName == "ControlTemplate");
        var scaler = template.Descendants()
                             .Single(static element => (string?)element.Attribute("Name") == "QRCodeSurfaceScaler");
        var surface = scaler.Elements().Single();

        scaler.Name.LocalName.ShouldBe("Viewbox");
        surface.Name.LocalName.ShouldBe("Grid");
        surface.Attribute("Width")?.Value.ShouldBe("{TemplateBinding Size}");
        surface.Attribute("Height")?.Value.ShouldBe("{TemplateBinding Size}");

        document.Descendants()
                .Where(static element => element.Name.LocalName == "Setter")
                .Single(static element => (string?)element.Attribute("Property") == "IconBgColor")
                .Attribute("Value")?.Value.ShouldBe("Transparent");
    }

    private static AvaloniaWindow Show(AtomUIQRCode qrCode)
    {
        var window = new AvaloniaWindow
        {
            Width = 360,
            Height = 260,
            Content = qrCode
        };
        window.Show();
        qrCode.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();
        return window;
    }

    private static Border FindCover(AtomUIQRCode qrCode)
    {
        return qrCode.GetVisualDescendants()
                     .OfType<Border>()
                     .Single(static border => border.Classes.Contains(CoverClass));
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
