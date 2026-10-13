using System.Collections;
using System.Reflection;
using Shouldly;
using Xunit;
using static AtomUI.Generator.Tests.Registration.TypeMapRegistrationGeneratorTests;

namespace AtomUI.Generator.Tests.Registration;

// Keep runtime-backed generator cases in the existing integration class: a separate parallel
// xUnit class can initialize Avalonia on another thread and break its UI-thread disposal contract.
public partial class TypeMapIntegrationTests
{
    [Theory]
    [InlineData("instance", "local:Button.ActionTheme", true)]
    [InlineData("instance", "local:ThemeOverrides.ActionTheme", false)]
    [InlineData("instance", "other:Button.ActionTheme", false)]
    [InlineData("inherited", "local:BaseButton.ActionTheme", true)]
    [InlineData("inherited", "local:Button.ActionTheme", true)]
    [InlineData("hidden", "local:BaseButton.ActionTheme", false)]
    [InlineData("hidden", "local:Button.ActionTheme", true)]
    [InlineData("override", "local:BaseButton.ActionTheme", true)]
    public void Qualified_Property_Must_Resolve_To_The_Semantic_Instance_Slot(string kind, string propertyElement, bool assigned)
    {
        var baseProperty = kind == "override" ? "public virtual Avalonia.Styling.ControlTheme? ActionTheme { get; set; }" : "public Avalonia.Styling.ControlTheme? ActionTheme { get; set; }";
        var ownProperty = kind switch
        {
            "inherited" => "",
            "hidden" => "public new Avalonia.Styling.ControlTheme? ActionTheme { get; set; }",
            "override" => "public override Avalonia.Styling.ControlTheme? ActionTheme { get; set; }",
            _ => "public Avalonia.Styling.ControlTheme? ActionTheme { get; set; }"
        };
        var parent = kind == "instance" ? "Avalonia.Controls.Control" : "BaseButton";
        var source = $$"""
            namespace Demo {
                public class Host : Avalonia.Controls.Primitives.TemplatedControl { }
                public class Unrelated : Avalonia.Controls.Control { }
                public class BaseButton : Avalonia.Controls.Control { {{baseProperty}} }
                [AtomUI.Theme.SemanticPart("action", SelectorClass = "semantic-action", ContractType = typeof(Avalonia.Controls.Control), Since = "6.0.0",
                    Customization = AtomUI.Theme.SemanticPartCustomization.SelectorAndTheme, ThemePropertyName = "ActionTheme", RuntimeCreated = true, SelectorRoute = ">> .semantic-action")]
                public class Button : {{parent}} { {{ownProperty}} }
                public sealed class ThemeOverrides : Avalonia.AvaloniaObject {
                    public static readonly Avalonia.AttachedProperty<Avalonia.Styling.ControlTheme?> ActionThemeProperty = Avalonia.AvaloniaProperty.RegisterAttached<ThemeOverrides, Avalonia.Controls.Control, Avalonia.Styling.ControlTheme?>("ActionTheme");
                    public static void SetActionTheme(Avalonia.Controls.Control control, Avalonia.Styling.ControlTheme? value) => control.SetValue(ActionThemeProperty, value);
                    public static Avalonia.Styling.ControlTheme? GetActionTheme(Avalonia.Controls.Control control) => control.GetValue(ActionThemeProperty);
                }
            }
            namespace Other {
                public sealed class Button : Avalonia.AvaloniaObject {
                    public static readonly Avalonia.AttachedProperty<Avalonia.Styling.ControlTheme?> ActionThemeProperty = Avalonia.AvaloniaProperty.RegisterAttached<Button, Avalonia.Controls.Control, Avalonia.Styling.ControlTheme?>("ActionTheme");
                    public static void SetActionTheme(Avalonia.Controls.Control control, Avalonia.Styling.ControlTheme? value) => control.SetValue(ActionThemeProperty, value);
                    public static Avalonia.Styling.ControlTheme? GetActionTheme(Avalonia.Controls.Control control) => control.GetValue(ActionThemeProperty);
                }
            }
            """;
        var asset = new TextFile("Host/Themes/HostTheme.axaml", $$"""
            <ResourceDictionary xmlns="https://github.com/avaloniaui" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml" xmlns:local="using:Demo" xmlns:other="using:Other">
                <ControlTheme x:Key="Host" TargetType="local:Host"><Setter Property="Template"><ControlTemplate>
                    <local:Button><{{propertyElement}}><ControlTheme TargetType="local:Unrelated" /></{{propertyElement}}></local:Button>
                </ControlTemplate></Setter></ControlTheme>
            </ResourceDictionary>
            """);
        var (_, assembly) = TypeMapIntegrationTests.Emit(source, asset);
        AssertButtonFragment(assembly, assigned);
    }

    private static void AssertButtonFragment(Assembly assembly, bool assigned)
    {
        var core = LoadCore();
        var collectionType = core.GetType("AtomUI.Registration.ControlPackageRegistrationBuilder")!;
        var collection = Activator.CreateInstance(collectionType, nonPublic: true)!;
        var fragment = assembly.GetTypes().Where(type => type.BaseType?.FullName == "AtomUI.Registration.ControlRegistrationFragmentAttribute")
            .Select(type => Activator.CreateInstance(type, nonPublic: true)!)
            .Single(instance => ((string)instance.GetType().GetProperty("FragmentId")!.GetValue(instance)!).Contains(":Demo.Button,", StringComparison.Ordinal));
        fragment.GetType().GetMethod("Add")!.Invoke(fragment, [collection]);
        var providerType = core.GetType("AtomUI.Theme.Resources.ControlThemesProvider")!;
        var provider = Activator.CreateInstance(providerType)!;
        providerType.GetProperty("Id")!.SetValue(provider, "Probe");
        // 分级启动为 Build 增加了余量收集重载；本测试按精确 2 参签名解析原路径。
        var buildMethod = collectionType.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
            .Single(method => method.Name == "Build" && method.GetParameters().Length == 2);
        var package = buildMethod.Invoke(collection, ["Probe", provider])!;
        var semantics = ((IEnumerable)package.GetType().GetProperty("SemanticControls")!.GetValue(package)!).Cast<object>().Single();
        var part = ((IEnumerable)semantics.GetType().GetProperty("Parts")!.GetValue(semantics)!).Cast<object>()
            .Single(item => Equals(item.GetType().GetProperty("Name")!.GetValue(item), "action"));
        var theme = part.GetType().GetProperty("Theme")!.GetValue(part)!;
        var target = (Type)theme.GetType().GetProperty("TargetType")!.GetValue(theme)!;
        target.FullName.ShouldBe(assigned ? "Demo.Unrelated" : "Avalonia.Controls.Control");
        var assets = ((IEnumerable)package.GetType().GetProperty("ThemeAssets")!.GetValue(package)!).Cast<object>().ToArray();
        assets.Length.ShouldBe(assigned ? 1 : 0);
        if (assigned)
        {
            var binding = ((IEnumerable)assets[0].GetType().GetProperty("SemanticThemeBindings")!.GetValue(assets[0])!).Cast<object>().ShouldHaveSingleItem();
            binding.GetType().GetProperty("PropertyName")!.GetValue(binding).ShouldBe("ActionTheme");
            binding.GetType().GetProperty("TargetType")!.GetValue(binding).ShouldBe(assembly.GetType("Demo.Unrelated"));
        }
    }
}
