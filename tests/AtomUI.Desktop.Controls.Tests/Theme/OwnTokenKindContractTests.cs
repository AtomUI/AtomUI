using System.Reflection;
using AtomUI.Theme.Schema;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.Theme;

public class OwnTokenKindContractTests
{
    [Theory]
    [InlineData(typeof(Button))]
    [InlineData(typeof(global::AtomUI.Desktop.Controls.ColorPicker))]
    [InlineData(typeof(global::AtomUI.Desktop.Controls.DataGrid))]
    public void Own_Token_Kind_Enum_Values_Match_Descriptor_Slots(Type controlType)
    {
        var descriptors = CreateGeneratedControlDescriptors(controlType.Assembly)
                          .Where(static descriptor => descriptor.OwnTokens.Count > 0)
                          .ToArray();
        descriptors.Length.ShouldBeGreaterThan(0);

        foreach (var descriptor in descriptors)
        {
            var kindType = descriptor.OwnTokens[0].ResourceKey.GetType();
            kindType.IsEnum.ShouldBeTrue(descriptor.Identity.Id);
            kindType.Name.ShouldBe(descriptor.Identity.Id + "TokenKind");
            Enum.GetUnderlyingType(kindType).ShouldBe(typeof(int), descriptor.Identity.Id);

            // XxxTokenKind values are what AbstractControlDesignToken.IsExplicit<TKind> reads as the Own slot.
            var kindValues = Enum.GetValues(kindType).Cast<object>()
                                 .ToDictionary(value => Enum.GetName(kindType, value)!,
                                               static value => Convert.ToInt32(value));
            kindValues.Count.ShouldBe(descriptor.OwnTokens.Count, descriptor.Identity.Id);
            foreach (var token in descriptor.OwnTokens)
            {
                token.ResourceKey.GetType().ShouldBe(kindType, $"{descriptor.Identity.Id}.{token.Name}");
                kindValues.ShouldContainKey(token.Name);
                kindValues[token.Name].ShouldBe(token.Slot, $"{descriptor.Identity.Id}.{token.Name}");
                Convert.ToInt32(token.ResourceKey).ShouldBe(token.Slot, $"{descriptor.Identity.Id}.{token.Name}");
            }

            kindValues.Values.OrderBy(static value => value)
                      .ShouldBe(descriptor.OwnTokens.Select(static token => token.Slot).OrderBy(static slot => slot),
                          descriptor.Identity.Id);
        }
    }

    private static IEnumerable<ControlTokenDescriptor> CreateGeneratedControlDescriptors(Assembly assembly)
    {
        var factory = assembly.GetTypes().Single(static type => type.Name == "GeneratedThemeSchemaDescriptorFactory");
        var methods = factory.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                             .Where(static method => method.Name.StartsWith("CreateControlDescriptor_", StringComparison.Ordinal)
                                                     && method.GetParameters().Length == 0
                                                     && method.ReturnType == typeof(ControlTokenDescriptor));
        foreach (var method in methods)
        {
            yield return (ControlTokenDescriptor)method.Invoke(null, null)!;
        }
    }
}
