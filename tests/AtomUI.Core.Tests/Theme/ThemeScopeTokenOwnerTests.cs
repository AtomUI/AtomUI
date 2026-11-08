using AtomUI.Theme;
using AtomUI.Theme.Compilation;
using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Styling;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;

namespace AtomUI.Core.Tests.Theme;

[Collection(ThemeConfigProviderTestCollection.Name)]
public class ThemeScopeTokenOwnerTests
{
    private static readonly ControlTokenIdentity s_lineEditIdentity = new("AtomUI", "LineEdit");
    private static readonly ControlTokenIdentity s_buttonIdentity = new("AtomUI", "Button");
    private static readonly ControlTokenIdentity s_containerIdentity = new("AtomUI", "Popup");

    [Fact]
    public void Default_ControlTheme_Injects_TokenOwner_Once()
    {
        HeadlessTestApp.Run(() =>
        {
            var manager = CreateManager();
            var theme = new ControlTheme(typeof(LineEditThemeTestControl));

            manager.NotifyControlThemeResolved(typeof(LineEditThemeTestControl), theme);
            manager.NotifyControlThemeResolved(typeof(LineEditThemeTestControl), theme);

            var owners = OwnerSetters(theme);
            owners.Length.ShouldBe(1);
            owners[0].Value.ShouldBe(s_lineEditIdentity);
            // The owner is not a plain theme Setter: a theme BasedOn this one must not inherit it.
            theme.Setters.OfType<Setter>().ShouldNotContain(static setter => setter.Property == ThemeScope.TokenOwnerProperty);
        });
    }

    [Fact]
    public void Non_Type_Key_Or_Unregistered_Control_Does_Not_Inject()
    {
        HeadlessTestApp.Run(() =>
        {
            var manager = CreateManager();
            var byName = new ControlTheme(typeof(LineEditThemeTestControl));
            var unregistered = new ControlTheme(typeof(DefaultThemeTestControl));

            manager.NotifyControlThemeResolved("LineEditTheme", byName);
            manager.NotifyControlThemeResolved(typeof(DefaultThemeTestControl), unregistered);

            byName.Setters.ShouldBeEmpty();
            byName.Children.ShouldBeEmpty();
            unregistered.Setters.ShouldBeEmpty();
            unregistered.Children.ShouldBeEmpty();
        });
    }

    [Fact]
    public void Template_Primitive_Inherits_Owner_Identity()
    {
        HeadlessTestApp.Run(() =>
        {
            var manager = CreateManager();
            var primitive = new FrameTestPrimitive();
            var control = new LineEditThemeTestControl
            {
                Theme = ResolveTheme(manager, typeof(LineEditThemeTestControl), () => primitive)
            };

            using var host = Show(control);

            primitive.GetValue(ThemeScope.TokenOwnerProperty).ShouldBe(s_lineEditIdentity);
            ThemeScope.GetTokenOwner(primitive).ShouldBe(s_lineEditIdentity);
        });
    }

    [Fact]
    public void Nested_Registered_Control_Carries_Its_Own_Identity()
    {
        HeadlessTestApp.Run(() =>
        {
            var manager = CreateManager();
            var innerPrimitive = new FrameTestPrimitive();
            var button = new ButtonThemeTestControl
            {
                Theme = ResolveTheme(manager, typeof(ButtonThemeTestControl), () => innerPrimitive)
            };
            var outerPrimitive = new FrameTestPrimitive();
            var control = new LineEditThemeTestControl
            {
                Theme = ResolveTheme(manager, typeof(LineEditThemeTestControl),
                    () => new StackPanel { Children = { outerPrimitive, button } })
            };

            using var host = Show(control);

            outerPrimitive.GetValue(ThemeScope.TokenOwnerProperty).ShouldBe(s_lineEditIdentity);
            button.GetValue(ThemeScope.TokenOwnerProperty).ShouldBe(s_buttonIdentity);
            innerPrimitive.GetValue(ThemeScope.TokenOwnerProperty).ShouldBe(s_buttonIdentity);
        });
    }

    [Fact]
    public void Unregistered_Subclass_Theme_BasedOn_A_Default_Theme_Inherits_The_Host_Owner()
    {
        // NumericUpDownSpinner's theme is BasedOn the resolved ButtonSpinner default theme so that application
        // replacements of that theme flow into it; the injected owner must stay with the registered type.
        HeadlessTestApp.Run(() =>
        {
            var manager = CreateManager();
            var buttonDefault = ResolveTheme(manager, typeof(ButtonThemeTestControl), static () => new FrameTestPrimitive());
            var derived = new DerivedButtonThemeTestControl
            {
                Theme = new ControlTheme(typeof(DerivedButtonThemeTestControl)) { BasedOn = buttonDefault }
            };
            var button = new ButtonThemeTestControl { Theme = buttonDefault };
            var control = new LineEditThemeTestControl
            {
                Theme = ResolveTheme(manager, typeof(LineEditThemeTestControl),
                    () => new StackPanel { Children = { derived, button } })
            };

            using var host = Show(control);
            derived.ApplyTemplate();

            derived.GetValue(ThemeScope.TokenOwnerProperty).ShouldBe(s_lineEditIdentity);
            derived.GetVisualChildren().OfType<FrameTestPrimitive>().ShouldHaveSingleItem()
                   .GetValue(ThemeScope.TokenOwnerProperty).ShouldBe(s_lineEditIdentity);
            button.GetValue(ThemeScope.TokenOwnerProperty).ShouldBe(s_buttonIdentity);
        });
    }

    [Fact]
    public void Replaced_Theme_Without_BasedOn_Has_No_Owner()
    {
        HeadlessTestApp.Run(() =>
        {
            var manager = CreateManager();
            var defaultTheme = new ControlTheme(typeof(LineEditThemeTestControl));
            manager.NotifyControlThemeResolved(typeof(LineEditThemeTestControl), defaultTheme);
            var primitive = new FrameTestPrimitive();
            var replaced = new ControlTheme(typeof(LineEditThemeTestControl))
            {
                Setters = { new Setter(TemplatedControl.TemplateProperty, CreateTemplate(() => primitive)) }
            };
            var control = new LineEditThemeTestControl { Theme = replaced };

            using var host = Show(control);

            primitive.GetValue(ThemeScope.TokenOwnerProperty).ShouldBeNull();
        });
    }

    [Fact]
    public void Owner_Transparent_Container_Gets_No_Injected_Owner()
    {
        HeadlessTestApp.Run(() =>
        {
            var manager = CreateManager();
            var theme = new ControlTheme(typeof(TransparentContainerTestControl));

            manager.NotifyControlThemeResolved(typeof(TransparentContainerTestControl), theme);

            OwnerSetters(theme).ShouldBeEmpty();
        });
    }

    [Fact]
    public void Content_Of_An_Owner_Transparent_Container_Inherits_The_Host_Owner()
    {
        HeadlessTestApp.Run(() =>
        {
            var manager = CreateManager();
            var primitive = new FrameTestPrimitive();
            var container = new TransparentContainerTestControl
            {
                Theme = ResolveTheme(manager, typeof(TransparentContainerTestControl), () => primitive)
            };
            var control = new LineEditThemeTestControl
            {
                Theme = ResolveTheme(manager, typeof(LineEditThemeTestControl), () => container)
            };

            using var host = Show(control);
            container.ApplyTemplate();

            container.GetValue(ThemeScope.TokenOwnerProperty).ShouldBe(s_lineEditIdentity);
            primitive.GetValue(ThemeScope.TokenOwnerProperty).ShouldBe(s_lineEditIdentity);
        });
    }

    [Fact]
    public void Content_Of_A_Standalone_Owner_Transparent_Container_Has_No_Owner()
    {
        // Without a host the content has no owner, so owner reads fall back to the global values.
        HeadlessTestApp.Run(() =>
        {
            var manager = CreateManager();
            var primitive = new FrameTestPrimitive();
            var container = new TransparentContainerTestControl
            {
                Theme = ResolveTheme(manager, typeof(TransparentContainerTestControl), () => primitive)
            };

            using var host = Show(container);

            primitive.GetValue(ThemeScope.TokenOwnerProperty).ShouldBeNull();
            ThemeScope.GetTokenOwner(primitive).ShouldBeNull();
        });
    }

    private static Setter[] OwnerSetters(ControlTheme theme) =>
        theme.Children.OfType<Style>()
             .SelectMany(static style => style.Setters.OfType<Setter>())
             .Where(static setter => setter.Property == ThemeScope.TokenOwnerProperty)
             .ToArray();

    private static ControlTheme ResolveTheme(ThemeManager manager, Type controlType, Func<Control> content)
    {
        var theme = new ControlTheme(controlType)
        {
            Setters = { new Setter(TemplatedControl.TemplateProperty, CreateTemplate(content)) }
        };
        manager.NotifyControlThemeResolved(controlType, theme);
        return theme;
    }

    private static FuncControlTemplate CreateTemplate(Func<Control> content) =>
        new((_, _) => content());

    private static Host Show(Control control)
    {
        var window = new Window { Content = control };
        window.Show();
        control.ApplyTemplate();
        return new Host(window);
    }

    private static ThemeManager CreateManager()
    {
        var registry = TypedThemeSnapshotCacheTests.CreateRegistry([
            new ControlTokenDescriptor(typeof(LineEditThemeTestControl), s_lineEditIdentity),
            new ControlTokenDescriptor(typeof(ButtonThemeTestControl), s_buttonIdentity),
            new ControlTokenDescriptor(typeof(TransparentContainerTestControl), s_containerIdentity)
        ]);
        var input = TypedThemeSnapshotCacheTests.CreateInput(registry);
        var snapshot = new ThemeCompiler().Compile(input).Snapshot!;
        var prepared = ThemeTransactionPreparation.Succeeded(snapshot, ThemeSnapshotCacheKey.Create(input));
        var manager = new ThemeManager(
            static () => true,
            (_, _, _) => ValueTask.FromResult(prepared));
        manager.ApplyThemeAsync(new ThemeRequest("Test", null, ThemeTransitionReason.Startup))
               .GetAwaiter()
               .GetResult()
               .Status.ShouldBe(ThemeTransitionStatus.Committed);
        return manager;
    }

    private sealed class Host(Window window) : IDisposable
    {
        public void Dispose()
        {
            window.Content = null;
            window.Close();
        }
    }
}

internal sealed class FrameTestPrimitive : TemplatedControl
{
}

/// <summary>A registered, owner-transparent container (the role of the AtomUI Popup).</summary>
[TokenOwnerTransparent]
internal sealed class TransparentContainerTestControl : TemplatedControl
{
}
