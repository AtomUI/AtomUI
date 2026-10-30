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

    private sealed class ThrowingServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType)
        {
            throw new InvalidOperationException($"SharedTokenResource must not request '{serviceType}'.");
        }
    }
}
