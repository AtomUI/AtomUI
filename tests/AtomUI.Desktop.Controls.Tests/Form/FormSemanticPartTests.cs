using System.Xml.Linq;
using AtomUI.Controls;
using AtomUI.Theme;
using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using AvaloniaTextBlock = Avalonia.Controls.TextBlock;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomUIFormItem = AtomUI.Desktop.Controls.FormItem;
using AtomUIForm = AtomUI.Desktop.Controls.Form;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Form;

public class FormSemanticPartTests
{
    private const string ContentClass     = "semantic-content";
    private const string ExtraClass       = "semantic-extra";
    private const string HelpClass        = "semantic-help";
    private const string HelpItemClass    = "semantic-help-item";
    private const string LabelClass       = "semantic-label";
    private const string ThemePath        = "src/AtomUI.Desktop.Controls/Form/Themes/FormItemTheme.axaml";
    private const string FormThemePath    = "src/AtomUI.Desktop.Controls/Form/Themes/FormTheme.axaml";

    static FormSemanticPartTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Registered_Descriptor_Exposes_Only_The_Approved_FormItem_Parts()
    {
        var registry = Application.Current.ShouldNotBeNull()
                                  .GetThemeManager().ShouldNotBeNull()
                                  .SemanticParts;

        registry.TryGetControl(typeof(AtomUIFormItem), out var descriptor).ShouldBeTrue();
        descriptor.ShouldNotBeNull();
        descriptor.Parts.Select(static part => part.Name)
                  .ShouldBe(["root", "content", "extra", "help", "helpItem", "label"]);

        AssertRoot(descriptor);
        AssertPart(descriptor, "content", ContentClass, typeof(ContentPresenter),
            "/template/ .semantic-content");
        AssertPart(descriptor, "extra", ExtraClass, typeof(ContentPresenter),
            "/template/ .semantic-extra");
        AssertPart(descriptor, "help", HelpClass, typeof(StackPanel),
            "/template/ .semantic-help");
        AssertPart(descriptor, "label", LabelClass, typeof(AvaloniaTextBlock),
            "/template/ .semantic-label");

        var helpItem = descriptor.Parts.Single(static part => part.Name == "helpItem");
        helpItem.Path.ShouldBe("helpItem");
        helpItem.SelectorClass.ShouldBe(HelpItemClass);
        helpItem.SelectorRoute.ShouldBe("/template/ .semantic-help > .semantic-help-item");
        helpItem.ContractType.ShouldBe(typeof(AvaloniaTextBlock));
        helpItem.Cardinality.ShouldBe(SemanticPartCardinality.Multiple);
        helpItem.Customization.ShouldBe(SemanticPartCustomization.Selector);
        helpItem.CrossVisualRoot.ShouldBeFalse();
        helpItem.RuntimeCreated.ShouldBeTrue();
        helpItem.Since.ShouldBe("6.2.0");
        helpItem.StyleType.ShouldNotBeNull();

        registry.TryGetControl(typeof(AtomUIForm), out _).ShouldBeFalse();
        registry.TryGetControl(typeof(FormItemDecorator), out _).ShouldBeFalse();
        registry.TryGetControl(typeof(SubmitButton), out _).ShouldBeFalse();
        registry.TryGetControl(typeof(ResetButton), out _).ShouldBeFalse();
    }

    [Fact]
    public void Built_In_Template_Implements_Only_The_Approved_Static_Markers()
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
                   "Classes.semantic-label:TextBlock",
                   "Classes.semantic-content:ContentPresenter",
                   "Classes.semantic-help:StackPanel",
                   "Classes.semantic-help-item:TextBlock",
                   "Classes.semantic-extra:ContentPresenter"
               ]);
        markers.ShouldAllBe(static marker =>
            string.Equals(marker.Attribute.Value, "true", StringComparison.OrdinalIgnoreCase));
        template.Descendants().Any(static element => HasMarker(element, "semantic-root")).ShouldBeFalse();
        template.Descendants()
                .Any(static element => (string?)element.Attribute("Name") == "PART_ErrorMsg")
                .ShouldBeFalse();

        var literalMarkers = template.Descendants()
                                     .Attributes("Classes")
                                     .Where(static attribute => attribute.Value.Split(
                                         (char[]?)null,
                                         StringSplitOptions.RemoveEmptyEntries)
                                         .Any(static value => value.StartsWith(
                                             "semantic-",
                                             StringComparison.Ordinal)))
                                     .ToArray();
        literalMarkers.ShouldBeEmpty();
    }

    [Fact]
    public void Default_Theme_Does_Not_Consume_Semantic_Selectors()
    {
        foreach (var themePath in new[] { ThemePath, FormThemePath })
        {
            var document = XDocument.Load(GetRepoFile(themePath), LoadOptions.SetLineInfo);
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
    public async Task Generated_Semantic_Styles_Apply_To_The_FormItem_Template_Targets()
    {
        var registry = Application.Current.ShouldNotBeNull()
                                  .GetThemeManager().ShouldNotBeNull()
                                  .SemanticParts;
        registry.TryGetControl(typeof(AtomUIFormItem), out var descriptor).ShouldBeTrue();
        descriptor.ShouldNotBeNull();

        var formItem = CreatePopulatedFormItem(
            validators: [new StaticResultValidator(FormValidateResult.Error) { Message = "input your password" }]);
        formItem.Classes.Add("semantic-owner");
        var ownerStyle = new Style(selector => selector.OfType<AtomUIFormItem>().Class("semantic-owner"));
        ownerStyle.Setters.Add(new Setter(Control.TagProperty, "root"));
        foreach (var part in descriptor.Parts.Where(static part => part.Name != "root"))
        {
            var partStyle = (Style)Activator.CreateInstance(part.StyleType.ShouldNotBeNull()).ShouldNotBeNull();
            partStyle.Setters.Add(new Setter(Control.TagProperty, part.Name));
            ownerStyle.Children.Add(partStyle);
        }
        formItem.Styles.Add(ownerStyle);

        var window = Show(formItem);
        try
        {
            await formItem.ValidateValueAsync(CancellationToken.None);
            Dispatcher.UIThread.RunJobs();

            formItem.Tag.ShouldBe("root");
            FindSemanticControl<AvaloniaTextBlock>(formItem, LabelClass).Tag.ShouldBe("label");
            FindSemanticControl<ContentPresenter>(formItem, ContentClass).Tag.ShouldBe("content");
            FindSemanticControl<ContentPresenter>(formItem, ExtraClass).Tag.ShouldBe("extra");
            FindSemanticControl<StackPanel>(formItem, HelpClass).Tag.ShouldBe("help");
            FindSemanticControls<AvaloniaTextBlock>(formItem, HelpItemClass)
                    .ShouldAllBe(static item => Equals(item.Tag, "helpItem"));
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public async Task Extra_Renders_Below_The_Control_After_The_Help_Area()
    {
        // 对齐上游 antd: control-input 下方是纵向 additional 容器,
        // 先 explain(help) 再 extra, extra 与内容列对齐且不占用控件水平空间。
        var formItem = CreatePopulatedFormItem(
            validators: [new StaticResultValidator(FormValidateResult.Error) { Message = "err" }]);

        var window = Show(formItem);
        try
        {
            await formItem.ValidateValueAsync(CancellationToken.None);
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            var content = FindSemanticControl<ContentPresenter>(formItem, ContentClass);
            var help    = FindSemanticControl<StackPanel>(formItem, HelpClass);
            var extra   = FindSemanticControl<ContentPresenter>(formItem, ExtraClass);

            var contentTopLeft = content.TranslatePoint(new Point(), formItem).ShouldNotBeNull();
            var helpTopLeft    = help.TranslatePoint(new Point(), formItem).ShouldNotBeNull();
            var extraTopLeft   = extra.TranslatePoint(new Point(), formItem).ShouldNotBeNull();

            extraTopLeft.X.ShouldBe(contentTopLeft.X, 0.5);
            extraTopLeft.Y.ShouldBeGreaterThanOrEqualTo(contentTopLeft.Y + content.Bounds.Height - 0.5);
            extraTopLeft.Y.ShouldBeGreaterThanOrEqualTo(helpTopLeft.Y + help.Bounds.Height - 0.5);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public async Task HelpItem_Runtime_Nodes_Track_Validation_Messages()
    {
        var errorBrush   = new SolidColorBrush(Colors.Red);
        var warningBrush = new SolidColorBrush(Colors.Orange);

        var formItem = CreatePopulatedFormItem(
            validators:
            [
                new StaticResultValidator(FormValidateResult.Error) { Message = "Please input your password!" },
                new StaticResultValidator(FormValidateResult.Error) { Message = "Use at least 8 characters." }
            ]);
        formItem.ValidateStrategy = FormValidateStrategy.Parallel;
        formItem.ErrorMessageForeground = errorBrush;
        formItem.WarningMessageForeground = warningBrush;
        formItem.Help = "Password must contain letters and numbers.";

        var window = Show(formItem);
        try
        {
            await formItem.ValidateValueAsync(CancellationToken.None);
            Dispatcher.UIThread.RunJobs();

            var helpContainer = FindSemanticControl<StackPanel>(formItem, HelpClass);
            var helpItems = FindSemanticControls<AvaloniaTextBlock>(formItem, HelpItemClass);
            helpItems.Length.ShouldBe(3);
            helpItems.Select(static item => item.Text)
                     .ShouldBe(["Please input your password!", "Use at least 8 characters.",
                                "Password must contain letters and numbers."]);

            var runtimeItems = helpItems.Take(2).ToArray();
            runtimeItems.ShouldAllBe(item => ReferenceEquals(item.Foreground, errorBrush));

            // 消息在前、HelpText 在后
            var helpTextIndex = helpContainer.Children.IndexOf(helpItems[2]);
            helpContainer.Children.IndexOf(runtimeItems[0]).ShouldBeLessThan(helpTextIndex);
            helpContainer.Children.IndexOf(runtimeItems[1]).ShouldBeLessThan(helpTextIndex);

            // 重置：运行时节点清空，HelpText 保留
            formItem.ResetItemValue();
            Dispatcher.UIThread.RunJobs();
            FindSemanticControls<AvaloniaTextBlock>(formItem, HelpItemClass).Length.ShouldBe(1);

            // 重新验证为 warning：单条 warning 节点，使用 WarningMessageForeground
            formItem.Validators = [new StaticResultValidator(FormValidateResult.Warning) { Message = "weak password" }];
            await formItem.ValidateValueAsync(CancellationToken.None);
            Dispatcher.UIThread.RunJobs();
            var rebuilt = FindSemanticControls<AvaloniaTextBlock>(formItem, HelpItemClass);
            rebuilt.Length.ShouldBe(2);
            rebuilt.Single(static item => item.Text == "weak password")
                  .Foreground.ShouldBe(warningBrush);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public async Task Static_Part_Markers_Remain_Stable_When_Layout_And_State_Change()
    {
        var formItem = CreatePopulatedFormItem(
            validators: [new StaticResultValidator(FormValidateResult.Error) { Message = "err" }]);

        var window = Show(formItem);
        try
        {
            var label = FindSemanticControl<AvaloniaTextBlock>(formItem, LabelClass);
            var content = FindSemanticControl<ContentPresenter>(formItem, ContentClass);
            var extra = FindSemanticControl<ContentPresenter>(formItem, ExtraClass);
            var help = FindSemanticControl<StackPanel>(formItem, HelpClass);
            var helpText = FindSemanticControls<AvaloniaTextBlock>(formItem, HelpItemClass)
                                  .Single(static item => item.Text == "help me");

            formItem.Layout = FormItemLayout.Vertical;
            formItem.LabelText = null;
            formItem.Extra = null;
            await formItem.ValidateValueAsync(CancellationToken.None);
            Dispatcher.UIThread.RunJobs();

            FindSemanticControl<AvaloniaTextBlock>(formItem, LabelClass).ShouldBeSameAs(label);
            FindSemanticControl<ContentPresenter>(formItem, ContentClass).ShouldBeSameAs(content);
            FindSemanticControl<ContentPresenter>(formItem, ExtraClass).ShouldBeSameAs(extra);
            FindSemanticControl<StackPanel>(formItem, HelpClass).ShouldBeSameAs(help);
            FindSemanticControls<AvaloniaTextBlock>(formItem, HelpItemClass)
                    .ShouldContain(helpText);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void FormActionsItem_Reuses_The_FormItem_Template_Markers()
    {
        var actionsItem = new FormActionsItem
        {
            OwnerForm = new AtomUIForm(),
            Content = new Avalonia.Controls.Button()
        };
        actionsItem.Classes.Add("semantic-owner");
        var ownerStyle = new Style(selector => selector.OfType<AtomUIFormItem>().Class("semantic-owner"));
        ownerStyle.Setters.Add(new Setter(Control.TagProperty, "root"));
        actionsItem.Styles.Add(ownerStyle);

        var window = Show(actionsItem);
        try
        {
            actionsItem.Tag.ShouldBe("root");
            FindSemanticControl<AvaloniaTextBlock>(actionsItem, LabelClass).ShouldNotBeNull();
            FindSemanticControl<ContentPresenter>(actionsItem, ContentClass).ShouldNotBeNull();
            FindSemanticControl<StackPanel>(actionsItem, HelpClass).ShouldNotBeNull();
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Dynamic_FormItem_Lifecycle_Preserves_Markers()
    {
        var first = CreatePopulatedFormItem(validators: null);
        var second = CreatePopulatedFormItem(validators: null);
        var form = new AtomUIForm();
        form.Items.Add(first);

        var window = Show(form);
        try
        {
            FindSemanticControl<AvaloniaTextBlock>(first, LabelClass).Text.ShouldBe("Username");

            form.Items.Add(second);
            Dispatcher.UIThread.RunJobs();
            FindSemanticControl<AvaloniaTextBlock>(second, LabelClass).Text.ShouldBe("Username");

            form.DeleteFormItem(second);
            Dispatcher.UIThread.RunJobs();
            FindSemanticControl<AvaloniaTextBlock>(first, LabelClass).Text.ShouldBe("Username");
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void FormItem_Styles_Do_Not_Hit_Nested_Semantic_Input_Content()
    {
        var registry = Application.Current.ShouldNotBeNull()
                                  .GetThemeManager().ShouldNotBeNull()
                                  .SemanticParts;
        registry.TryGetControl(typeof(AtomUIFormItem), out var descriptor).ShouldBeTrue();
        descriptor.ShouldNotBeNull();

        var lineEdit = new LineEdit { Text = "atomui", PlaceholderText = "input" };
        var formItem = new AtomUIFormItem
        {
            OwnerForm = new AtomUIForm(),
            LabelText = "Name",
            FieldName = "name",
            Content   = lineEdit
        };
        formItem.Classes.Add("semantic-owner");
        var ownerStyle = new Style(selector => selector.OfType<AtomUIFormItem>().Class("semantic-owner"));
        foreach (var part in descriptor.Parts.Where(static part => part.Name != "root"))
        {
            var partStyle = (Style)Activator.CreateInstance(part.StyleType.ShouldNotBeNull()).ShouldNotBeNull();
            partStyle.Setters.Add(new Setter(Control.TagProperty, part.Name));
            ownerStyle.Children.Add(partStyle);
        }
        formItem.Styles.Add(ownerStyle);

        var window = Show(formItem);
        try
        {
            lineEdit.Tag.ShouldBeNull();
            lineEdit.GetVisualDescendants()
                    .OfType<Control>()
                      .Where(static control => control.Classes.Any(static marker =>
                          marker.StartsWith("semantic-", StringComparison.Ordinal)))
                      .ShouldAllBe(static control => control.Tag == null);
            FindSemanticControl<ContentPresenter>(formItem, ContentClass).Tag.ShouldBe("content");
        }
        finally
        {
            window.Close();
        }
    }

    private static AtomUIFormItem CreatePopulatedFormItem(IList<IFormValidator>? validators)
    {
        return new AtomUIFormItem
        {
            OwnerForm = new AtomUIForm(),
            LabelText = "Username",
            FieldName = "username",
            Help      = "help me",
            Extra     = "extra text",
            Content   = new FormItemAwareControl(),
            Validators = validators
        };
    }

    private static void AssertRoot(ControlSemanticDescriptor descriptor)
    {
        var root = descriptor.Parts.Single(static part => part.Name == "root");
        root.Path.ShouldBe("root");
        root.SelectorClass.ShouldBeNull();
        root.SelectorRoute.ShouldBeNull();
        root.ContractType.ShouldBe(typeof(AtomUIFormItem));
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

    private static T[] FindSemanticControls<T>(Control owner, string marker)
        where T : Control
    {
        return owner.GetVisualDescendants()
                    .OfType<T>()
                    .Where(control => control.Classes.Contains(marker))
                    .ToArray();
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
               bool.TryParse(classProperty.Value, out var enabled) &&
               enabled;
    }

    private static AvaloniaWindow Show(Control content)
    {
        var window = new AvaloniaWindow
        {
            Width = 480,
            Height = 320,
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

    private sealed class FormItemAwareControl : Control, IFormItemAware
    {
        public object? Value { get; set; }
        private EventHandler? _valueChanged;

        public event EventHandler? ValueChanged
        {
            add => _valueChanged += value;
            remove => _valueChanged -= value;
        }

        public void SetFormValue(object? value)
        {
            Value = value;
        }

        public object? GetFormValue() => Value;

        public void ClearFormValue()
        {
            Value = null;
        }

        public void NotifyValidateStatus(FormValidateStatus status)
        {
        }
    }

    private sealed class StaticResultValidator : IFormValidator
    {
        private readonly FormValidateResult _result;

        public StaticResultValidator(FormValidateResult result)
        {
            _result = result;
        }

        public string? Message { get; init; }

        public bool WarningOnly => _result == FormValidateResult.Warning;

        public Task<FormValidateResult> ValidateAsync(string fieldName, object? value, CancellationToken cancellationToken)
        {
            return Task.FromResult(_result);
        }
    }
}
