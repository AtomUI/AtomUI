using AtomUI.Theme;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Resources;
using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Shouldly;
using Xunit;

namespace AtomUI.Core.Tests.Theme;

/// <summary>
/// <see cref="OwnerTokenSet" /> resolves each entry exactly like an <see cref="OwnerTokenBinding" /> on the same
/// target, with one set of subscriptions, and only resolves entries that have been read.
/// </summary>
[Collection(ThemeConfigProviderTestCollection.Name)]
public class OwnerTokenSetTests
{
    private static readonly ControlTokenIdentity s_lineEditIdentity = new("AtomUI", "LineEdit");
    private static readonly ControlTokenIdentity s_buttonIdentity = new("AtomUI", "Button");

    [Fact]
    public void Entry_Resolves_On_First_Read_To_The_Owner_Value_The_Binding_Reads()
    {
        HeadlessTestApp.Run(() =>
        {
            using var fixture = new OwnerTokenResourceTests.Fixture(LineEditConfig("ColorBorder", "#FF0000"));
            var primitive = fixture.CreatePrimitive();
            fixture.Show(fixture.CreateOwner(typeof(LineEditThemeTestControl), primitive));
            var changes = new List<(int, object?, object?)>();
            using var set = new OwnerTokenSet(primitive, 2, (index, old, value) => changes.Add((index, old, value)));

            set.SetKind(0, SharedTokenKind.ColorBorder);
            set.ResolveCount.ShouldBe(0, "an unread entry is not resolved");

            ColorOf(set.GetValue(0)).ShouldBe(Color.Parse("#FF0000"));
            ColorOf(set.GetValue(0)).ShouldBe(OwnerTokenResourceTests.BorderColor(primitive));
            set.ResolveCount.ShouldBe(1);
            set.GetValue(0);
            set.ResolveCount.ShouldBe(1, "a read entry is cached until something changes");
            changes.ShouldBeEmpty("a first read hands the value to the reader instead of reporting a change");
            set.GetValue(1).ShouldBeNull("an entry without a Token has no value");
        });
    }

    [Fact]
    public void Publish_Resolves_Only_Read_Entries_Whose_Key_Changed()
    {
        HeadlessTestApp.Run(() =>
        {
            // Button has no Controls[Button] configuration, so its effective globals are the root values.
            using var fixture = new OwnerTokenResourceTests.Fixture(new ThemeConfigBuilder().Build());
            var primitive = fixture.CreatePrimitive();
            fixture.Show(fixture.CreateOwner(typeof(ButtonThemeTestControl), primitive));
            var changes = new List<(int Index, object? Value)>();
            using var set = new OwnerTokenSet(primitive, 3, (index, _, value) => changes.Add((index, value)));
            set.SetKind(0, SharedTokenKind.ColorBorder);
            set.SetKind(1, SharedTokenKind.ColorPrimary);
            set.SetKind(2, SharedTokenKind.ColorText);
            set.GetValue(0);
            set.GetValue(1);
            var resolved = set.ResolveCount;

            fixture.Apply(new ThemeConfigBuilder().WithToken("ColorText", "#123456").Build(), ThemeTransitionReason.UserRequest);
            set.ResolveCount.ShouldBe(resolved, "the changed key belongs to an unread entry; read keys are unchanged");
            changes.ShouldBeEmpty();

            fixture.Apply(new ThemeConfigBuilder().WithToken("ColorText", "#123456").WithToken("ColorBorder", "#0000FF").Build(),
                ThemeTransitionReason.UserRequest);
            set.ResolveCount.ShouldBe(resolved + 1, "only the read entry whose key changed is resolved");
            changes.Count.ShouldBe(1);
            changes[0].Index.ShouldBe(0);
            ColorOf(changes[0].Value).ShouldBe(Color.Parse("#0000FF"));

            // A local resource change is not a publish: every read entry resolves again.
            primitive.Resources[OwnerTokenBinding.GetResourceKey(s_buttonIdentity, SharedTokenKind.ColorBorder)] =
                new ImmutableSolidColorBrush(Colors.Red);
            set.ResolveCount.ShouldBe(resolved + 3);
            ColorOf(set.GetValue(0)).ShouldBe(Colors.Red);
        });
    }

    [Fact]
    public void Owner_Change_Retargets_Read_Entries_Without_Accumulating_Listeners()
    {
        HeadlessTestApp.Run(() =>
        {
            using var fixture = new OwnerTokenResourceTests.Fixture(new ThemeConfigBuilder()
                                                                   .WithControl(s_lineEditIdentity, BorderConfig("#FF0000"))
                                                                   .WithControl(s_buttonIdentity, BorderConfig("#00FF00"))
                                                                   .Build());
            var primitive = fixture.CreatePrimitive();
            var lineEditHost = new Decorator();
            var buttonHost = new Decorator();
            fixture.Show(new StackPanel
            {
                Children =
                {
                    fixture.CreateOwner(typeof(LineEditThemeTestControl), lineEditHost),
                    fixture.CreateOwner(typeof(ButtonThemeTestControl), buttonHost)
                }
            });
            lineEditHost.Child = primitive;
            var listeners = TokenOwnerListeners.Count(primitive);
            var changes = new List<object?>();
            using var set = new OwnerTokenSet(primitive, 1, (_, _, value) => changes.Add(value));
            set.SetKind(0, SharedTokenKind.ColorBorder);
            ColorOf(set.GetValue(0)).ShouldBe(Color.Parse("#FF0000"));

            lineEditHost.Child = null;
            buttonHost.Child   = primitive;
            ColorOf(set.GetValue(0)).ShouldBe(Color.Parse("#00FF00"));
            ColorOf(changes[^1]).ShouldBe(Color.Parse("#00FF00"));

            buttonHost.Child   = null;
            lineEditHost.Child = primitive;
            ColorOf(set.GetValue(0)).ShouldBe(Color.Parse("#FF0000"));
            TokenOwnerListeners.Count(primitive).ShouldBe(listeners + 1, "one set registers one owner listener");

            set.Dispose();
            TokenOwnerListeners.Count(primitive).ShouldBe(listeners, "disposing releases the owner listener");
            OwnerTokenResourceTests.EventHandlers(typeof(StyledElement), primitive, nameof(StyledElement.ResourcesChanged))
                                   .ShouldNotContain(handler => handler.Target == set);
        });
    }

    [Fact]
    public void Changing_The_Token_Of_A_Read_Entry_Reports_The_New_Value()
    {
        HeadlessTestApp.Run(() =>
        {
            using var fixture = new OwnerTokenResourceTests.Fixture(LineEditConfig("ColorBorder", "#FF0000"));
            var primitive = fixture.CreatePrimitive();
            fixture.Show(fixture.CreateOwner(typeof(LineEditThemeTestControl), primitive));
            var changes = new List<(object? Old, object? Value)>();
            using var set = new OwnerTokenSet(primitive, 1, (_, old, value) => changes.Add((old, value)));
            set.SetKind(0, SharedTokenKind.ColorBorder);
            set.GetValue(0);

            set.SetKind(0, SharedTokenKind.ColorPrimary);
            changes.Count.ShouldBe(1);
            ColorOf(changes[0].Old).ShouldBe(Color.Parse("#FF0000"));
            primitive.TryFindResource(OwnerTokenBinding.GetResourceKey(s_lineEditIdentity, SharedTokenKind.ColorPrimary),
                out var primary).ShouldBeTrue();
            ColorOf(changes[0].Value).ShouldBe(ColorOf(primary));

            set.SetKind(0, null);
            changes.Count.ShouldBe(2);
            changes[1].Value.ShouldBeNull();
        });
    }

    private static ControlThemeConfig BorderConfig(string color) =>
        new ControlThemeConfigBuilder().WithToken("ColorBorder", color).Build();

    private static ThemeConfig LineEditConfig(string token, string value) =>
        new ThemeConfigBuilder()
            .WithControl(s_lineEditIdentity, new ControlThemeConfigBuilder().WithToken(token, value).Build())
            .Build();

    private static Color ColorOf(object? value) => value.ShouldBeAssignableTo<ISolidColorBrush>()!.Color;
}
