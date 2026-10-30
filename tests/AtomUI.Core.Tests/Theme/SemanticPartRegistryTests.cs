using AtomUI.Theme;
using AtomUI.Theme.Schema;
using Avalonia.Controls;
using Shouldly;
using Xunit;

namespace AtomUI.Core.Tests.Theme;

public class SemanticPartRegistryTests
{
    [Fact]
    public void Registry_Orders_And_Indexes_Descriptors_Deterministically()
    {
        var button = Descriptor(typeof(Button), "Button");
        var checkBox = Descriptor(typeof(CheckBox), "CheckBox");

        var registry = new SemanticPartRegistry([checkBox, button]);

        registry.Controls.Select(static descriptor => descriptor.Identity.Id)
                .ShouldBe(["Button", "CheckBox"]);
        registry.TryGetControl(typeof(Button), out var byType).ShouldBeTrue();
        byType.ShouldBeSameAs(button);
        registry.TryGetControl(new ControlTokenIdentity("Tests", "CheckBox"), out var byIdentity)
                .ShouldBeTrue();
        byIdentity.ShouldBeSameAs(checkBox);
    }

    [Fact]
    public void Registry_Rejects_Duplicate_Control_Type_Or_Identity()
    {
        var button = Descriptor(typeof(Button), "Button");

        Should.Throw<ArgumentException>(() => new SemanticPartRegistry([
            button,
            Descriptor(typeof(Button), "ButtonAlias")
        ]));

        Should.Throw<ArgumentException>(() => new SemanticPartRegistry([
            button,
            Descriptor(typeof(CheckBox), "Button")
        ]));
    }

    [Fact]
    public void Empty_Registry_Uses_A_Stable_Shared_Instance()
    {
        SemanticPartRegistry.Empty.Controls.ShouldBeEmpty();
        SemanticPartRegistry.Empty.TryGetControl(typeof(Button), out _).ShouldBeFalse();
    }

    [Fact]
    public void Registry_Rejects_Null_Descriptors_With_A_Contract_Exception()
    {
        Should.Throw<ArgumentException>(() => new SemanticPartRegistry([null!]));
    }

    private static ControlSemanticDescriptor Descriptor(Type controlType, string id)
    {
        return new ControlSemanticDescriptor(
            controlType,
            new ControlTokenIdentity("Tests", id),
            [
                new SemanticPartDescriptor(
                    "root",
                    "root",
                    null,
                    controlType,
                    SemanticPartCardinality.Single,
                    SemanticPartCustomization.Root,
                    null,
                    false,
                    "6.0",
                    false)
            ]);
    }
}
