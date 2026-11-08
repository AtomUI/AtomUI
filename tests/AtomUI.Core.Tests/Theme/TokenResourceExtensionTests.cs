using AtomUI.Theme.Resources;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Shouldly;
using Xunit;

namespace AtomUI.Core.Tests.Theme;

[Collection(ThemeConfigProviderTestCollection.Name)]
public class TokenResourceExtensionTests
{
    [Fact]
    public void Shared_Extension_Returns_A_Global_Key_Without_Inspecting_The_Parent_Stack()
    {
        var key = new SharedTokenResourceExtension(SharedTokenKind.ColorPrimary)
                  .ProvideValue(new ThrowingServiceProvider())
                  .ShouldBeOfType<DynamicResourceExtension>()
                  .ResourceKey;

        key.ShouldBe(SharedTokenKind.ColorPrimary);
    }

    [Fact]
    public void Owner_Extension_Requires_A_Kind_Like_Token_Extensions()
    {
        Should.Throw<InvalidOperationException>(
                  () => new OwnerTokenResourceExtension().ProvideValue(new ThrowingServiceProvider()))
              .Message.ShouldBe("A Token resource key is required.");
    }

    private sealed class ThrowingServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType)
        {
            throw new InvalidOperationException($"SharedTokenResource must not request '{serviceType}'.");
        }
    }
}
