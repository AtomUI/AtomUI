using AtomUI.Controls.Commons;
using AtomUI.Media;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Templates;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaSvg = Avalonia.Svg.Svg;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Controls.Tests.Empty;

public class EmptyExtensionTests
{
    private const string SvgPartName = "PART_SvgImage";

    static EmptyExtensionTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    // Templated subclass with an svg part and presenters for the object-typed members.
    private class TestEmpty : AbstractEmpty
    {
        private readonly bool _includeSvg;

        public TestEmpty(bool includeSvg = true, Color? shadow = null)
        {
            _includeSvg = includeSvg;
            BorderColor          = new SolidColorBrush(Color.Parse("#d9d9d9"));
            BorderColorSecondary = new SolidColorBrush(Color.Parse("#f0f0f0"));
            ShadowColor          = new SolidColorBrush(shadow ?? Color.Parse("#f5f5f5"));
            ContentColor         = new SolidColorBrush(Color.Parse("#fafafa"));
            BgColor              = new SolidColorBrush(Colors.White);

            Template = new FuncControlTemplate<TestEmpty>((owner, scope) =>
            {
                var panel = new StackPanel();
                if (_includeSvg)
                {
                    var svg = new AvaloniaSvg(new Uri("avares://AtomUI.Controls.Tests/")) { Name = SvgPartName };
                    scope.Register(SvgPartName, svg);
                    panel.Children.Add(svg);
                }
                panel.Children.Add(CreatePresenter("ImagePresenter", ImageProperty, ImageTemplateProperty, owner));
                panel.Children.Add(CreatePresenter("TitlePresenter", TitleProperty, TitleTemplateProperty, owner));
                panel.Children.Add(CreatePresenter("DescriptionPresenter", DescriptionProperty, null, owner));
                return panel;
            });
        }

        private static ContentPresenter CreatePresenter(
            string name,
            AvaloniaProperty<object?> content,
            AvaloniaProperty<IDataTemplate?>? template,
            TestEmpty owner)
        {
            var presenter = new ContentPresenter { Name = name };
            presenter.Bind(ContentPresenter.ContentProperty, owner.GetObservable(content));
            if (template is not null)
            {
                presenter.Bind(ContentPresenter.ContentTemplateProperty, owner.GetObservable(template));
            }
            return presenter;
        }
    }

    // A Mobile-style subclass: the preset defaults to Simple through a metadata override.
    private sealed class DefaultSimpleEmpty : TestEmpty
    {
        static DefaultSimpleEmpty()
        {
            PresetImageProperty.OverrideDefaultValue<DefaultSimpleEmpty>(PresetEmptyImage.Simple);
        }
    }

    private sealed class WindowContext : IDisposable
    {
        private readonly AvaloniaWindow _window;

        internal WindowContext(Control content)
        {
            _window = new AvaloniaWindow { Width = 400, Height = 400, Content = content };
            _window.Show();
            Dispatcher.UIThread.RunJobs();
        }

        public void Dispose()
        {
            _window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    private static AvaloniaSvg FindSvg(Control root)
    {
        return root.GetVisualDescendants().OfType<AvaloniaSvg>().Single();
    }

    private static ContentPresenter FindPresenter(Control root, string name)
    {
        return root.GetVisualDescendants().OfType<ContentPresenter>().Single(p => p.Name == name);
    }

    private static string ExpectedPreset(PresetEmptyImage preset, TestEmpty empty)
    {
        // Mirrors the production colour pipeline without going through the cache.
        var bg = ((ISolidColorBrush)empty.BgColor!).Color;
        Color Resolve(IBrush? brush) => ColorUtils.OnBackground(((ISolidColorBrush)brush!).Color, bg);
        var border = Resolve(empty.BorderColor);
        var borderSecondary = Resolve(empty.BorderColorSecondary);
        var shadow = Resolve(empty.ShadowColor);
        var content = Resolve(empty.ContentColor);
        return preset == PresetEmptyImage.Default
            ? BuiltInImageBuilder.BuildDefaultImage(shadow, border, borderSecondary)
            : BuiltInImageBuilder.BuildSimpleImage(content, border, shadow);
    }

    [Fact]
    public void Image_conflicts_with_explicit_preset()
    {
        // Image against an explicit local PresetImage.
        var empty = new TestEmpty { Image = "image" };
        using (new WindowContext(empty))
        {
            Should.Throw<ApplicationException>(() => empty.PresetImage = PresetEmptyImage.Default);
        }

        // Image against ImagePath and ImageSource.
        var withPath = new TestEmpty { Image = "image" };
        using (new WindowContext(withPath))
        {
            Should.Throw<ApplicationException>(() => withPath.ImagePath = "avares://x/y.svg");
        }

        var withSource = new TestEmpty { Image = "image" };
        using (new WindowContext(withSource))
        {
            Should.Throw<ApplicationException>(() => withSource.ImageSource = "<svg/>");
        }

        // Setting the conflict before attaching throws on attach.
        var late = new TestEmpty { Image = "image", PresetImage = PresetEmptyImage.Simple };
        Should.Throw<ApplicationException>(() => new WindowContext(late).Dispose());

        // Style values count as explicit values.
        var styled = new TestEmpty { Image = "image" };
        styled.Styles.Add(new Style(selector => selector.OfType<TestEmpty>())
        {
            Setters = { new Setter(AbstractEmpty.PresetImageProperty, PresetEmptyImage.Default) }
        });
        Should.Throw<ApplicationException>(() => new WindowContext(styled).Dispose());

        // The original three-way check still applies.
        var legacy = new TestEmpty { ImagePath = "avares://x/y.svg", ImageSource = "<svg/>" };
        Should.Throw<ApplicationException>(() => new WindowContext(legacy).Dispose());
    }

    [Fact]
    public void Default_preset_does_not_conflict_with_image()
    {
        var withImage = new DefaultSimpleEmpty { Image = "image" };
        using (new WindowContext(withImage))
        {
            withImage.PresetImage.ShouldBe(PresetEmptyImage.Simple);
            // An explicit Image suppresses the preset rendering.
            FindSvg(withImage).Source.ShouldBeNull();
            FindSvg(withImage).Path.ShouldBeNull();
            FindPresenter(withImage, "ImagePresenter").Content.ShouldBe("image");
        }

        // Without an explicit image the default preset still renders.
        var plain = new DefaultSimpleEmpty();
        using (new WindowContext(plain))
        {
            FindSvg(plain).Source.ShouldBe(ExpectedPreset(PresetEmptyImage.Simple, plain));
        }

        // Explicit ImagePath / ImageSource take precedence over the default preset instead of conflicting.
        const string source = "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"1\" height=\"1\" />";
        var imageFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.svg");
        File.WriteAllText(imageFile, source);
        try
        {
            var imagePath = new Uri(imageFile).AbsoluteUri;
            var withPath = new DefaultSimpleEmpty { ImagePath = imagePath };
            using (new WindowContext(withPath))
            {
                FindSvg(withPath).Path.ShouldBe(imagePath);
                FindSvg(withPath).Source.ShouldBeNull();
            }
        }
        finally
        {
            File.Delete(imageFile);
        }

        var withSource = new DefaultSimpleEmpty { ImageSource = source };
        using (new WindowContext(withSource))
        {
            FindSvg(withSource).Source.ShouldBe(source);
        }

        // An app-set preset is explicit again.
        var explicitPreset = new DefaultSimpleEmpty { Image = "image" };
        using (new WindowContext(explicitPreset))
        {
            Should.Throw<ApplicationException>(() => explicitPreset.PresetImage = PresetEmptyImage.Default);
        }
    }

    [Fact]
    public void Title_and_object_description_render()
    {
        typeof(AbstractEmpty).GetProperty(nameof(AbstractEmpty.Title))!.PropertyType.ShouldBe(typeof(object));
        typeof(AbstractEmpty).GetProperty(nameof(AbstractEmpty.Description))!.PropertyType.ShouldBe(typeof(object));
        typeof(AbstractEmpty).GetProperty(nameof(AbstractEmpty.Image))!.PropertyType.ShouldBe(typeof(object));
        AbstractEmpty.DescriptionProperty.PropertyType.ShouldBe(typeof(object));
        AbstractEmpty.TitleProperty.PropertyType.ShouldBe(typeof(object));
        AbstractEmpty.ImageProperty.PropertyType.ShouldBe(typeof(object));
        AbstractEmpty.TitleTemplateProperty.PropertyType.ShouldBe(typeof(IDataTemplate));
        AbstractEmpty.ImageTemplateProperty.PropertyType.ShouldBe(typeof(IDataTemplate));

        var description = new Border { Tag = "description" };
        var empty = new TestEmpty { Title = "Title", Description = description };
        using var context = new WindowContext(empty);

        var title = FindPresenter(empty, "TitlePresenter");
        title.Child.ShouldBeOfType<TextBlock>().Text.ShouldBe("Title");
        FindPresenter(empty, "DescriptionPresenter").Child.ShouldBeSameAs(description);

        // Plain strings keep working for Description.
        empty.Description = "No data";
        Dispatcher.UIThread.RunJobs();
        FindPresenter(empty, "DescriptionPresenter").Child.ShouldBeOfType<TextBlock>().Text.ShouldBe("No data");

        // Title accepts a template.
        empty.TitleTemplate = new FuncDataTemplate<object?>((value, _) => new Border { Tag = $"templated:{value}" });
        empty.Title = 42;
        Dispatcher.UIThread.RunJobs();
        title.Child.ShouldBeOfType<Border>().Tag.ShouldBe("templated:42");

        // Image object content flows through its own presenter.
        var image = new Border { Tag = "image" };
        empty.Image = image;
        Dispatcher.UIThread.RunJobs();
        FindPresenter(empty, "ImagePresenter").Child.ShouldBeSameAs(image);
    }

    [Fact]
    public void Preset_svg_is_cached_per_colors()
    {
        var first = new TestEmpty { PresetImage = PresetEmptyImage.Default };
        var second = new TestEmpty { PresetImage = PresetEmptyImage.Default };
        var other = new TestEmpty(shadow: Color.Parse("#112233")) { PresetImage = PresetEmptyImage.Default };
        var simple = new TestEmpty { PresetImage = PresetEmptyImage.Simple };
        var host = new StackPanel { Children = { first, second, other, simple } };
        using var context = new WindowContext(host);

        var firstSource = FindSvg(first).Source.ShouldNotBeNull();
        FindSvg(second).Source.ShouldBeSameAs(firstSource);
        FindSvg(first).Source.ShouldBe(ExpectedPreset(PresetEmptyImage.Default, first));

        var otherSource = FindSvg(other).Source.ShouldNotBeNull();
        otherSource.ShouldNotBe(firstSource);
        otherSource.ShouldBe(ExpectedPreset(PresetEmptyImage.Default, other));

        var simpleSource = FindSvg(simple).Source.ShouldNotBeNull();
        simpleSource.ShouldNotBe(firstSource);
        simpleSource.ShouldBe(ExpectedPreset(PresetEmptyImage.Simple, simple));

        // A later empty with the same inputs reuses the cached string; a colour change picks a new entry.
        var third = new TestEmpty { PresetImage = PresetEmptyImage.Simple };
        host.Children.Add(third);
        Dispatcher.UIThread.RunJobs();
        FindSvg(third).Source.ShouldBeSameAs(simpleSource);

        third.ShadowColor = new SolidColorBrush(Color.Parse("#445566"));
        Dispatcher.UIThread.RunJobs();
        FindSvg(third).Source.ShouldNotBeSameAs(simpleSource);
        FindSvg(third).Source.ShouldBe(ExpectedPreset(PresetEmptyImage.Simple, third));

        third.ShadowColor = new SolidColorBrush(Color.Parse("#f5f5f5"));
        Dispatcher.UIThread.RunJobs();
        FindSvg(third).Source.ShouldBeSameAs(simpleSource);
    }

    [Fact]
    public void Template_without_svg_part_is_tolerated()
    {
        var empty = new TestEmpty(includeSvg: false) { PresetImage = PresetEmptyImage.Default, Image = null };
        using (new WindowContext(empty))
        {
            empty.GetVisualDescendants().OfType<AvaloniaSvg>().ShouldBeEmpty();
            empty.PresetImage = PresetEmptyImage.Simple;
            empty.ImageSource = null;
            Dispatcher.UIThread.RunJobs();
        }

        var withImage = new TestEmpty(includeSvg: false) { Image = "image" };
        using (new WindowContext(withImage))
        {
            FindPresenter(withImage, "ImagePresenter").Content.ShouldBe("image");
            withImage.Title = "title";
            Dispatcher.UIThread.RunJobs();
        }
    }
}
