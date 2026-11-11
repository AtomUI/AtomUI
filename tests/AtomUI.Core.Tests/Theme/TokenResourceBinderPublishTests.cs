using AtomUI.Data;
using AtomUI.Theme;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Resources;
using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Styling;
using Avalonia.Threading;
using Shouldly;
using Xunit;

namespace AtomUI.Core.Tests.Theme;

/// <summary>
/// <see cref="TokenResourceBinder"/> bindings on a StyledElement host resolve through <see cref="TokenResourceObservable"/>:
/// a theme publish that leaves the bound key's value unchanged is skipped, everything else refreshes.
/// </summary>
[Collection(ThemeConfigProviderTestCollection.Name)]
public class TokenResourceBinderPublishTests
{
    [Fact]
    public void Publish_That_Leaves_The_Global_Key_Unchanged_Is_Skipped_And_A_Change_Is_Published()
    {
        HeadlessTestApp.Run(() =>
        {
            using var fixture = new OwnerTokenResourceTests.Fixture(new ThemeConfigBuilder().Build());
            var control = new FrameTestPrimitive();
            fixture.Show(control);
            using var binding = TokenResourceBinder.CreateGlobalTokenBinding(
                control, TemplatedControl.BorderBrushProperty, SharedTokenKind.ColorBorder);
            var observable = Observable(control);
            OwnerTokenResourceTests.BorderColor(control).ShouldBe(fixture.GlobalBorderColor());
            var resolved = observable.ResolveCount;

            fixture.Apply(new ThemeConfigBuilder().WithToken("ColorPrimary", "#00b96b").Build(),
                ThemeTransitionReason.UserRequest);
            observable.ResolveCount.ShouldBe(resolved, "an unchanged key must not be re-resolved by a publish");

            fixture.Apply(new ThemeConfigBuilder().WithToken("ColorBorder", "#0000FF").Build(),
                ThemeTransitionReason.UserRequest);
            observable.ResolveCount.ShouldBe(resolved + 1);
            OwnerTokenResourceTests.BorderColor(control).ShouldBe(Color.Parse("#0000FF"));

            // A non-publish ResourcesChanged (a local resource dictionary change) resolves fully.
            control.Resources[SharedTokenKind.ColorBorder] = new ImmutableSolidColorBrush(Colors.Red);
            observable.ResolveCount.ShouldBe(resolved + 2);
            OwnerTokenResourceTests.BorderColor(control).ShouldBe(Colors.Red);
        });
    }

    [Fact]
    public void Control_Key_Follows_The_Owner_Override_And_Skips_Unrelated_Publishes()
    {
        HeadlessTestApp.Run(() =>
        {
            using var fixture = new OwnerTokenResourceTests.Fixture(new ThemeConfigBuilder().Build());
            var control = new FrameTestPrimitive();
            var owner = new ButtonThemeTestControl();
            fixture.Show(new StackPanel { Children = { owner, control } });
            using var binding = TokenResourceBinder.CreateControlTokenBinding(
                owner, control, TemplatedControl.BorderBrushProperty, SharedTokenKind.ColorBorder);
            var observable = Observable(owner);
            var resolved = observable.ResolveCount;

            fixture.Apply(new ThemeConfigBuilder().WithToken("ColorPrimary", "#00b96b").Build(),
                ThemeTransitionReason.UserRequest);
            observable.ResolveCount.ShouldBe(resolved, "Button reads the root ColorBorder, which did not change");

            fixture.Apply(new ThemeConfigBuilder()
                          .WithControl(new ControlTokenIdentity("AtomUI", "Button"),
                              new ControlThemeConfigBuilder().WithToken("ColorBorder", "#00FF00").Build())
                          .Build(),
                ThemeTransitionReason.UserRequest);
            observable.ResolveCount.ShouldBe(resolved + 1);
            OwnerTokenResourceTests.BorderColor(control).ShouldBe(Color.Parse("#00FF00"));
        });
    }

    [Fact]
    public void Variant_Switch_And_Scoped_Override_Refresh_The_Binding()
    {
        HeadlessTestApp.Run(() =>
        {
            using var fixture = new OwnerTokenResourceTests.Fixture(new ThemeConfigBuilder().Build());
            var control = new FrameTestPrimitive();
            var provider = new ThemeConfigProvider { Child = control };
            fixture.Show(provider);
            using var binding = TokenResourceBinder.CreateGlobalTokenBinding(
                control, TemplatedControl.BorderBrushProperty, SharedTokenKind.ColorBorder);
            var light = OwnerTokenResourceTests.BorderColor(control);

            provider.Config = new ThemeConfigBuilder().WithToken("ColorBorder", "#00FF00").Build();
            Dispatcher.UIThread.RunJobs();
            OwnerTokenResourceTests.BorderColor(control).ShouldBe(Color.Parse("#00FF00"));

            provider.Config = new ThemeConfigBuilder().Build();
            Dispatcher.UIThread.RunJobs();
            OwnerTokenResourceTests.BorderColor(control).ShouldBe(light);

            fixture.Apply(new ThemeConfigBuilder().WithAlgorithms(ThemeAlgorithm.Default, ThemeAlgorithm.Dark).Build(),
                ThemeTransitionReason.UserRequest);
            OwnerTokenResourceTests.BorderColor(control).ShouldNotBe(light, "a variant switch republishes the binding");
            OwnerTokenResourceTests.BorderColor(control).ShouldBe(fixture.GlobalBorderColor());
        });
    }

    [Fact]
    public void Root_Publish_Reaches_A_Binding_Inside_A_Scoped_Provider()
    {
        HeadlessTestApp.Run(() =>
        {
            using var fixture = new OwnerTokenResourceTests.Fixture(new ThemeConfigBuilder().Build());
            var control = new FrameTestPrimitive();
            var provider = new ThemeConfigProvider
            {
                Config = new ThemeConfigBuilder().WithToken("ColorPrimary", "#00b96b").Build(),
                Child  = control
            };
            fixture.Show(provider);
            using var binding = TokenResourceBinder.CreateGlobalTokenBinding(
                control, TemplatedControl.BorderBrushProperty, SharedTokenKind.ColorBorder);
            var observable = Observable(control);
            var resolved = observable.ResolveCount;

            // The very first publish after the binding resolved is the one a stale per-context version would misread.
            fixture.Apply(new ThemeConfigBuilder().WithToken("ColorBorder", "#0000FF").Build(), ThemeTransitionReason.UserRequest);

            observable.ResolveCount.ShouldBe(resolved + 1, "the root change reaches the scoped binding once");
            OwnerTokenResourceTests.BorderColor(control).ShouldBe(Color.Parse("#0000FF"));

            fixture.Apply(new ThemeConfigBuilder().WithToken("ColorBorder", "#0000FF").WithToken("ColorText", "#123456").Build(),
                ThemeTransitionReason.UserRequest);
            observable.ResolveCount.ShouldBe(resolved + 1, "an unrelated root change leaves the scoped ColorBorder alone");
        });
    }

    [Fact]
    public void Token_That_Resolves_To_Null_Unsets_The_Binding_Instead_Of_Writing_Null()
    {
        HeadlessTestApp.Run(() =>
        {
            // Avalonia's ResourceObservable maps a found-null resource to UnsetValue (FindResource(...) ?? UnsetValue), and
            // an unset binding value applies the property's default (BindingEntryBase.SetValue), so FontFamily stays
            // FontFamily.Default; writing null instead would leave the property null.
            using var fixture = new OwnerTokenResourceTests.Fixture(new ThemeConfigBuilder().Build());
            var control = new FrameTestPrimitive();
            control.Resources[SharedTokenKind.FontFamily] = null;
            fixture.Show(control);
            control.FontFamily.ShouldBe(FontFamily.Default);

            using var binding = TokenResourceBinder.CreateGlobalTokenBinding(
                control, TemplatedControl.FontFamilyProperty, SharedTokenKind.FontFamily, BindingPriority.Style);

            control.FontFamily.ShouldNotBeNull("a null resource must unset the binding, not write null");
            control.FontFamily.ShouldBe(FontFamily.Default);

            // Once the key resolves to a value the binding carries it, and a null again returns the default.
            var monospace = new FontFamily("monospace");
            control.Resources[SharedTokenKind.FontFamily] = monospace;
            control.FontFamily.ShouldBe(monospace);
            control.Resources[SharedTokenKind.FontFamily] = null;
            control.FontFamily.ShouldBe(FontFamily.Default);
        });
    }

    private static TokenResourceObservable Observable(StyledElement host) =>
        OwnerTokenResourceTests.EventHandlers(typeof(StyledElement), host, nameof(StyledElement.ResourcesChanged))
                               .Select(static handler => handler.Target)
                               .OfType<TokenResourceObservable>()
                               .ShouldHaveSingleItem();
}
