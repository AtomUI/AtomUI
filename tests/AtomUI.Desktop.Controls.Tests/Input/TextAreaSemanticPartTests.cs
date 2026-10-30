using System.Xml.Linq;
using AtomUI.Theme;
using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaButton = Avalonia.Controls.Button;
using AvaloniaWindow = Avalonia.Controls.Window;
using AtomUITextArea = AtomUI.Desktop.Controls.TextArea;

namespace AtomUI.Desktop.Controls.Tests.Input;

public class TextAreaSemanticPartTests
{
    private const string ClearClass = "semantic-clear";
    private const string CountClass = "semantic-count";
    private const string TextareaClass = "semantic-textarea";
    private const string ThemePath =
        "src/AtomUI.Desktop.Controls/Input/Themes/TextAreaTheme.axaml";

    static TextAreaSemanticPartTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Registered_Descriptor_Exposes_TextArea_Clear_And_Count_Parts()
    {
        var registry = Application.Current.ShouldNotBeNull()
                                  .GetThemeManager().ShouldNotBeNull()
                                  .SemanticParts;

        registry.TryGetControl(typeof(AtomUITextArea), out var descriptor).ShouldBeTrue();
        descriptor.ShouldNotBeNull();
        descriptor.Parts.Select(static part => part.Name)
                  .ShouldBe(["root", "clear", "count", "textarea"]);

        var root = descriptor.Parts.Single(static part => part.Name == "root");
        root.ContractType.ShouldBe(typeof(AtomUITextArea));
        root.StyleType.ShouldBeNull();

        AssertPart(descriptor, "clear", ClearClass, typeof(AvaloniaButton),
            "/template/ .semantic-scope-input-frame /template/ .semantic-scope-suffix > .semantic-suffix > .semantic-clear");
        AssertPart(descriptor, "count", CountClass, typeof(TextBlock),
            "/template/ .semantic-count");
        AssertPart(descriptor, "textarea", TextareaClass, typeof(TextPresenter),
            "/template/ .semantic-textarea");
    }

    [Fact]
    public void Built_In_Template_Implements_The_Approved_Static_Markers()
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
                   "Classes.semantic-count:TextBlock",
                   "Classes.semantic-scope-input-frame:TextAreaDecoratedBox",
                   "Classes.semantic-prefix:AddOnContentPresenter",
                   "Classes.semantic-suffix:StackPanel",
                   "Classes.semantic-clear:InputClearIconButton",
                   "Classes.semantic-content:Panel",
                   "Classes.semantic-placeholder:TextBlock",
                   "Classes.semantic-textarea:InputTextPresenter"
               ]);
        markers.ShouldAllBe(static marker =>
            string.Equals(marker.Attribute.Value, "true", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Generated_Semantic_Styles_Apply_To_The_TextArea_Template_Targets()
    {
        var registry = Application.Current.ShouldNotBeNull()
                                  .GetThemeManager().ShouldNotBeNull()
                                  .SemanticParts;
        registry.TryGetControl(typeof(AtomUITextArea), out var descriptor).ShouldBeTrue();
        descriptor.ShouldNotBeNull();

        var textArea = new AtomUITextArea
        {
            Width = 320,
            Lines = 2,
            Text = "atomui",
            MaxLength = 12,
            IsAllowClear = true,
            IsShowCount = true,
            IsMotionEnabled = false
        };
        textArea.Classes.Add("semantic-owner");
        var ownerStyle = new Style(selector => selector.OfType<AtomUITextArea>().Class("semantic-owner"));
        ownerStyle.Setters.Add(new Setter(Control.TagProperty, "root"));
        foreach (var part in descriptor.Parts.Where(static part => part.Name != "root"))
        {
            var partStyle = (Style)Activator.CreateInstance(part.StyleType.ShouldNotBeNull()).ShouldNotBeNull();
            partStyle.Setters.Add(new Setter(Control.TagProperty, part.Name));
            ownerStyle.Children.Add(partStyle);
        }
        textArea.Styles.Add(ownerStyle);

        var window = Show(textArea);
        try
        {
            textArea.Tag.ShouldBe("root");
            FindSemanticControl<AvaloniaButton>(textArea, ClearClass).Tag.ShouldBe("clear");
            FindSemanticControl<TextBlock>(textArea, CountClass).Tag.ShouldBe("count");
            FindSemanticControl<TextPresenter>(textArea, TextareaClass).Tag.ShouldBe("textarea");
        }
        finally
        {
            window.Close();
        }
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

    private static T FindSemanticControl<T>(AtomUITextArea owner, string marker)
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
            Width = 480,
            Height = 200,
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
