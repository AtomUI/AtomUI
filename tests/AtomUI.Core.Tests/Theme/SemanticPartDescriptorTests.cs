using AtomUI.Theme;
using AtomUI.Theme.Schema;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Shouldly;
using Xunit;

namespace AtomUI.Core.Tests.Theme;

public class SemanticPartDescriptorTests
{
    [Fact]
    public void Descriptor_Preserves_A_Valid_Button_Contract()
    {
        var descriptor = new ControlSemanticDescriptor(
            typeof(Button),
            new ControlTokenIdentity("Tests", "Button"),
            [
                Root(),
                Part(
                    "icon",
                    "icon",
                    "semantic-icon",
                    typeof(Control),
                    SemanticPartCardinality.Multiple),
                Part(
                    "content",
                    "content",
                    "semantic-content",
                    typeof(ContentPresenter),
                    SemanticPartCardinality.Single)
            ]);

        descriptor.ControlType.ShouldBe(typeof(Button));
        descriptor.Identity.ShouldBe(new ControlTokenIdentity("Tests", "Button"));
        descriptor.Parts.Select(static part => part.Name)
                  .ShouldBe(["root", "content", "icon"]);
        descriptor.Parts.Single(static part => part.Name == "root").SelectorClass.ShouldBeNull();
        descriptor.Parts.Single(static part => part.Name == "root").SelectorRoute.ShouldBeNull();
        descriptor.Parts.Single(static part => part.Name == "root").StyleType.ShouldBeNull();
        descriptor.Parts.Single(static part => part.Name == "icon").SelectorClass.ShouldBe("semantic-icon");
        descriptor.Parts.Single(static part => part.Name == "icon")
                  .SelectorRoute.ShouldBe("/template/ .semantic-icon");
    }

    [Fact]
    public void Descriptor_Preserves_An_Owner_Relative_Runtime_Selector_Route()
    {
        var part = new SemanticPartDescriptor(
            "content",
            "content",
            "semantic-content",
            typeof(ContentPresenter),
            SemanticPartCardinality.Multiple,
            SemanticPartCustomization.Selector,
            null,
            false,
            "6.0",
            true,
            "/template/ .semantic-scope-items > .semantic-scope-item /template/ .semantic-content",
            typeof(Control));

        part.SelectorRoute.ShouldBe(
            "/template/ .semantic-scope-items > .semantic-scope-item /template/ .semantic-content");
        part.StyleType.ShouldBe(typeof(Control));
    }

    [Theory]
    [InlineData(".semantic-content")]
    [InlineData("/template/ .semantic-scope-items .semantic-content")]
    [InlineData("/template/ #PART_Items /template/ .semantic-content")]
    [InlineData("/template/ Control /template/ .semantic-content")]
    [InlineData("/template/ .semantic-scope-items > .semantic-other")]
    public void Selector_Route_Must_Use_Owner_Relative_Template_Or_Child_Class_Steps(string selectorRoute)
    {
        Should.Throw<ArgumentException>(() => new SemanticPartDescriptor(
            "content",
            "content",
            "semantic-content",
            typeof(ContentPresenter),
            SemanticPartCardinality.Multiple,
            SemanticPartCustomization.Selector,
            null,
            false,
            "6.0",
            true,
            selectorRoute));
    }

    [Theory]
    [InlineData("icon")]
    [InlineData("semantic-Icon")]
    [InlineData("semantic_icon")]
    [InlineData("semantic-icon-")]
    public void Selector_Class_Must_Use_The_Reserved_Kebab_Case_Namespace(string selectorClass)
    {
        Should.Throw<ArgumentException>(() => Part(
            "icon",
            "icon",
            selectorClass,
            typeof(Control),
            SemanticPartCardinality.Single));
    }

    [Fact]
    public void Root_Must_Use_Root_Customization_Without_A_Selector_Class()
    {
        Should.Throw<ArgumentException>(() => new SemanticPartDescriptor(
            "root",
            "root",
            "semantic-root",
            typeof(Button),
            SemanticPartCardinality.Single,
            SemanticPartCustomization.Root,
            null,
            false,
            "6.0",
            false));

        Should.Throw<ArgumentException>(() => new SemanticPartDescriptor(
            "root",
            "root",
            null,
            typeof(Button),
            SemanticPartCardinality.Single,
            SemanticPartCustomization.Selector,
            null,
            false,
            "6.0",
            false));
    }

    [Fact]
    public void Root_Rejects_Cross_Nested_Owners_Metadata()
    {
        Should.Throw<ArgumentException>(() => new SemanticPartDescriptor(
            "root",
            "root",
            null,
            typeof(Button),
            SemanticPartCardinality.Single,
            SemanticPartCustomization.Root,
            null,
            false,
            "6.0",
            false,
            crossNestedOwners: true));
    }

    [Fact]
    public void Root_Rejects_Rest_Hidden_Metadata()
    {
        Should.Throw<ArgumentException>(() => new SemanticPartDescriptor(
            "root",
            "root",
            null,
            typeof(Button),
            SemanticPartCardinality.Single,
            SemanticPartCustomization.Root,
            null,
            false,
            "6.0",
            false,
            restHidden: true));
    }

    [Fact]
    public void Selector_And_Theme_Requires_Strongly_Typed_Theme_Metadata()
    {
        Should.Throw<ArgumentException>(() => new SemanticPartDescriptor(
            "action",
            "action",
            "semantic-action",
            typeof(Button),
            SemanticPartCardinality.Single,
            SemanticPartCustomization.SelectorAndTheme,
            null,
            false,
            "6.0",
            false));
    }

    [Fact]
    public void Contract_Type_Must_Derive_From_StyledElement()
    {
        Should.Throw<ArgumentException>(() => Part(
            "value",
            "value",
            "semantic-value",
            typeof(string),
            SemanticPartCardinality.Single));
    }

    [Fact]
    public void Descriptor_Rejects_Unknown_Enum_Values_And_The_Reserved_Root_Class()
    {
        Should.Throw<ArgumentOutOfRangeException>(() => new SemanticPartDescriptor(
            "icon",
            "icon",
            "semantic-icon",
            typeof(Control),
            (SemanticPartCardinality)99,
            SemanticPartCustomization.Selector,
            null,
            false,
            "6.0",
            false));

        Should.Throw<ArgumentOutOfRangeException>(() => new SemanticPartDescriptor(
            "icon",
            "icon",
            "semantic-icon",
            typeof(Control),
            SemanticPartCardinality.Single,
            (SemanticPartCustomization)99,
            null,
            false,
            "6.0",
            false));

        Should.Throw<ArgumentException>(() => Part(
            "icon",
            "icon",
            "semantic-root",
            typeof(Control),
            SemanticPartCardinality.Single));

        Should.Throw<ArgumentException>(() => Part(
            "icon",
            "root",
            "semantic-icon",
            typeof(Control),
            SemanticPartCardinality.Single));
    }

    [Fact]
    public void Control_Descriptor_Rejects_Duplicate_Path_And_Selector_Class()
    {
        Should.Throw<ArgumentException>(() => new ControlSemanticDescriptor(
            typeof(Button),
            new ControlTokenIdentity("Tests", "Button"),
            [
                Root(),
                Part("leadingIcon", "icon", "semantic-leading-icon", typeof(Control), SemanticPartCardinality.Single),
                Part("trailingIcon", "icon", "semantic-trailing-icon", typeof(Control), SemanticPartCardinality.Single)
            ]));

        Should.Throw<ArgumentException>(() => new ControlSemanticDescriptor(
            typeof(Button),
            new ControlTokenIdentity("Tests", "Button"),
            [
                Root(),
                Part("leadingIcon", "leadingIcon", "semantic-icon", typeof(Control), SemanticPartCardinality.Single),
                Part("trailingIcon", "trailingIcon", "semantic-icon", typeof(Control), SemanticPartCardinality.Single)
            ]));
    }

    [Fact]
    public void Control_Descriptor_Requires_Exactly_One_Root()
    {
        Should.Throw<ArgumentException>(() => new ControlSemanticDescriptor(
            typeof(Button),
            new ControlTokenIdentity("Tests", "Button"),
            [Part("icon", "icon", "semantic-icon", typeof(Control), SemanticPartCardinality.Single)]));

        Should.Throw<ArgumentException>(() => new ControlSemanticDescriptor(
            typeof(Button),
            new ControlTokenIdentity("Tests", "Button"),
            [Root(), Root()]));
    }

    [Fact]
    public void Control_Descriptor_Rejects_Null_Parts_With_A_Contract_Exception()
    {
        Should.Throw<ArgumentException>(() => new ControlSemanticDescriptor(
            typeof(Button),
            new ControlTokenIdentity("Tests", "Button"),
            [Root(), null!]));
    }

    private static SemanticPartDescriptor Root()
    {
        return new SemanticPartDescriptor(
            "root",
            "root",
            null,
            typeof(Button),
            SemanticPartCardinality.Single,
            SemanticPartCustomization.Root,
            null,
            false,
            "6.0",
            false);
    }

    private static SemanticPartDescriptor Part(
        string name,
        string path,
        string selectorClass,
        Type contractType,
        SemanticPartCardinality cardinality)
    {
        return new SemanticPartDescriptor(
            name,
            path,
            selectorClass,
            contractType,
            cardinality,
            SemanticPartCustomization.Selector,
            null,
            false,
            "6.0",
            false);
    }
}
