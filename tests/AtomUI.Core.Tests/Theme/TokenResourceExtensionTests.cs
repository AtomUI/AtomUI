using AtomUI.Theme.Resources;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Diagnostics;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Markup.Xaml.XamlIl.Runtime;
using Avalonia.Media;
using Shouldly;
using Xunit;

namespace AtomUI.Core.Tests.Theme;

[Collection(ThemeConfigProviderTestCollection.Name)]
public class TokenResourceExtensionTests
{
    [Fact]
    public void Shared_Extension_Returns_A_Global_Key_Asking_Only_For_The_Template_Context()
    {
        var serviceProvider = new ThrowingServiceProvider();
        var key = new SharedTokenResourceExtension(SharedTokenKind.ColorPrimary)
                  .ProvideValue(serviceProvider)
                  .ShouldBeOfType<DynamicResourceExtension>()
                  .ResourceKey;

        key.ShouldBe(SharedTokenKind.ColorPrimary);
        serviceProvider.Requested.ShouldBe([typeof(IAvaloniaXamlIlControlTemplateProvider)]);
    }

    [Fact]
    public void Shared_Extension_Outside_A_Control_Template_Binds_With_Local_Value_Priority()
    {
        var value = new SharedTokenResourceExtension(SharedTokenKind.ColorPrimary)
            .ProvideValue(new ThrowingServiceProvider());

        BoundPriority(value, SharedTokenKind.ColorPrimary).ShouldBe(BindingPriority.LocalValue);
    }

    [Fact]
    public void Shared_Extension_In_A_Control_Template_Binds_With_Template_Priority()
    {
        var serviceProvider = new ThrowingServiceProvider(new ControlTemplateMarker());
        var value = new SharedTokenResourceExtension(SharedTokenKind.ColorPrimary)
            .ProvideValue(serviceProvider);

        value.ShouldBeOfType<DynamicResourceExtension>().ResourceKey.ShouldBe(SharedTokenKind.ColorPrimary);
        serviceProvider.Requested.ShouldBe([typeof(IAvaloniaXamlIlControlTemplateProvider)]);
        BoundPriority(value, SharedTokenKind.ColorPrimary).ShouldBe(BindingPriority.Template);
    }

    [Fact]
    public void Derived_Extension_In_A_Control_Template_Uses_Its_Own_Resource_Key_With_Template_Priority()
    {
        var value = new ProbeTokenResourceExtension(ProbeTokenKind.Fill)
            .ProvideValue(new ThrowingServiceProvider(new ControlTemplateMarker()));

        value.ShouldBeOfType<DynamicResourceExtension>().ResourceKey.ShouldBe(ProbeTokenResourceExtension.FillKey);
        BoundPriority(value, ProbeTokenResourceExtension.FillKey).ShouldBe(BindingPriority.Template);
    }

    [Fact]
    public void Owner_Extension_Requires_A_Kind_Like_Token_Extensions()
    {
        Should.Throw<InvalidOperationException>(
                  () => new OwnerTokenResourceExtension().ProvideValue(new ThrowingServiceProvider()))
              .Message.ShouldBe("A Token resource key is required.");
    }

    private static BindingPriority BoundPriority(object value, object resourceKey)
    {
        var priority = BindingPriority.Unset;
        HeadlessTestApp.Run(() =>
        {
            var border = new Border();
            border.Resources[resourceKey] = Brushes.Red;
            border.Bind(Border.BackgroundProperty, value.ShouldBeAssignableTo<BindingBase>()!);

            border.Background.ShouldBe(Brushes.Red);
            priority = border.GetDiagnostic(Border.BackgroundProperty).Priority;
        });
        return priority;
    }

    private enum ProbeTokenKind
    {
        Fill
    }

    private sealed class ProbeTokenResourceExtension(ProbeTokenKind kind) : TokenResourceExtension<ProbeTokenKind>(kind)
    {
        internal const string FillKey = "Probe.Fill";

        protected override object GetResourceKey(ProbeTokenKind kind) => FillKey;
    }

    /// <summary>
    /// Allows only the control-template context query that <see cref="DynamicResourceExtension"/> performs
    /// to choose Template priority; resolving a resource or an anchor while loading markup would request
    /// anything else.
    /// </summary>
    private sealed class ThrowingServiceProvider(IAvaloniaXamlIlControlTemplateProvider? templateProvider = null)
        : IServiceProvider
    {
        internal List<Type> Requested { get; } = [];

        public object? GetService(Type serviceType)
        {
            Requested.Add(serviceType);
            if (serviceType == typeof(IAvaloniaXamlIlControlTemplateProvider))
            {
                return templateProvider;
            }
            throw new InvalidOperationException($"TokenResource must not request '{serviceType}'.");
        }
    }

    private sealed class ControlTemplateMarker : IAvaloniaXamlIlControlTemplateProvider
    {
    }
}
