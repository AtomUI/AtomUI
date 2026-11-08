using AtomUI.Theme;
using AtomUI.Theme.Compilation;
using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Styling;
using Shouldly;
using Xunit;

namespace AtomUI.Core.Tests.Theme;

[Collection(ThemeConfigProviderTestCollection.Name)]
public class ThemeScopeTokenOwnerTests
{
    private static readonly ControlTokenIdentity s_lineEditIdentity = new("AtomUI", "LineEdit");
    private static readonly ControlTokenIdentity s_buttonIdentity = new("AtomUI", "Button");

    [Fact]
    public void Default_ControlTheme_Injects_TokenOwner_Once()
    {
        HeadlessTestApp.Run(() =>
        {
            var manager = CreateManager();
            var theme = new ControlTheme(typeof(LineEditThemeTestControl));

            manager.NotifyControlThemeResolved(typeof(LineEditThemeTestControl), theme);
            manager.NotifyControlThemeResolved(typeof(LineEditThemeTestControl), theme);

            var owners = theme.Setters.OfType<Setter>()
                              .Where(static setter => setter.Property == ThemeScope.TokenOwnerProperty)
                              .ToArray();
            owners.Length.ShouldBe(1);
            owners[0].Value.ShouldBe(s_lineEditIdentity);
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
            unregistered.Setters.ShouldBeEmpty();
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
            new ControlTokenDescriptor(typeof(ButtonThemeTestControl), s_buttonIdentity)
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
