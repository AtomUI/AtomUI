using AtomUI.Registration;
using AtomUI.Startup.Data;
using Avalonia.Controls;
using Shouldly;
using Xunit;

namespace AtomUI.Core.Tests.Registration;

public class StartupPackageIndexDisabledPackageTests
{
    private const string DisabledMessage = "Control package 'Fixture.Disabled' is referenced but not enabled.";

    [Fact]
    public void Resources_Owned_By_A_Disabled_Package_Fail_Explicitly_By_String_And_Type_Key()
    {
        var image = new StartupImage(StartupImageFormat.Id, 0, [], [],
        [
            new("s:Fixture.DisabledResource", "Fixture.Disabled"),
            new(StartupPackageIndex.ResourceKey(typeof(Border))!, "Fixture.Disabled")
        ]);
        var index = StartupPackageIndex.Create([], image);

        Should.Throw<InvalidOperationException>(() => index.ThrowIfPackageDisabled("Fixture.DisabledResource"))
              .Message.ShouldStartWith(DisabledMessage);
        Should.Throw<InvalidOperationException>(() => index.ThrowIfPackageDisabled(typeof(Border)))
              .Message.ShouldStartWith(DisabledMessage);
        Should.NotThrow(() => index.ThrowIfPackageDisabled("Fixture.UnownedResource"));
        Should.NotThrow(() => index.ThrowIfPackageDisabled(typeof(Button)));
    }
}
