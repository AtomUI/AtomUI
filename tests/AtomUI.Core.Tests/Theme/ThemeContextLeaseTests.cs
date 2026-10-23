using AtomUI.Theme;
using AtomUI.Theme.Styling;
using Avalonia.Controls;
using Avalonia.Styling;
using Shouldly;
using Xunit;

namespace AtomUI.Core.Tests.Theme;

[Collection(ThemeConfigProviderTestCollection.Name)]
public class ThemeContextLeaseTests
{
    [Fact]
    public void Attach_Installs_Context_Bridge_And_Explicit_Variant()
    {
        HeadlessTestApp.Run(() =>
        {
            var context = CreateContext();
            var host = new Window();

            using var lease = ThemeContextLease.Attach(host, context);
            host.ApplyStyling();

            host.GetValue(ThemeScope.ContextProperty).ShouldBeSameAs(context);
            host.GetValue(ThemeStyleQualification.ActiveProperty)
                .ShouldBeSameAs(context.Styles.GetValue(ThemeStyleContext.ActivationProperty));
            host.RequestedThemeVariant.ShouldBe(ThemeVariant.Light);
            host.Resources.MergedDictionaries
                .OfType<ThemeContextResourceBridge>()
                .ShouldHaveSingleItem();
            host.Close();
        });
    }

    [Fact]
    public void Owner_Replacement_Releases_The_Old_Bridge_And_Keeps_The_New_Local_State()
    {
        HeadlessTestApp.Run(() =>
        {
            var first = CreateContext();
            var second = CreateContext(globalPrimary: "#00b96b", registrationId: 1);
            var host = new Window();
            var firstLease = ThemeContextLease.Attach(host, first);

            using var secondLease = ThemeContextLease.Attach(host, second);

            host.GetValue(ThemeScope.ContextProperty).ShouldBeSameAs(second);
            host.Resources.MergedDictionaries
                .OfType<ThemeContextResourceBridge>()
                .ShouldHaveSingleItem()
                .OwnerContext.ShouldBeSameAs(second);
            firstLease.Dispose();
            host.GetValue(ThemeScope.ContextProperty).ShouldBeSameAs(second);
            host.Close();
        });
    }

    [Fact]
    public void Dispose_Removes_Only_State_Owned_By_The_Lease()
    {
        HeadlessTestApp.Run(() =>
        {
            var context = CreateContext();
            var host = new Window();
            var lease = ThemeContextLease.Attach(host, context);

            lease.Dispose();

            host.IsSet(ThemeScope.ContextProperty).ShouldBeFalse();
            host.IsSet(ThemeStyleQualification.ActiveProperty).ShouldBeFalse();
            host.Styles.ShouldBeEmpty();
            host.Resources.MergedDictionaries
                .OfType<ThemeContextResourceBridge>()
                .ShouldBeEmpty();
            host.Close();
        });
    }

    [Fact]
    public void Failed_Owner_Replacement_Restores_The_Previous_Lease_State()
    {
        HeadlessTestApp.Run(() =>
        {
            var first = CreateContext();
            var second = CreateContext(globalPrimary: "#00b96b", registrationId: 1);
            var host = new ThrowingWindow();
            using var firstLease = ThemeContextLease.Attach(host, first);
            host.ThrowOnNextContextChange = true;

            Should.Throw<InvalidOperationException>(() => ThemeContextLease.Attach(host, second));

            host.GetValue(ThemeScope.ContextProperty).ShouldBeSameAs(first);
            host.Resources.MergedDictionaries
                .OfType<ThemeContextResourceBridge>()
                .ShouldHaveSingleItem()
                .OwnerContext.ShouldBeSameAs(first);

            firstLease.Dispose();

            host.IsSet(ThemeScope.ContextProperty).ShouldBeFalse();
            host.Resources.MergedDictionaries
                .OfType<ThemeContextResourceBridge>()
                .ShouldBeEmpty();
            host.Close();
        });
    }

    [Fact]
    public void Manager_Dispose_Releases_Active_Context_Leases()
    {
        HeadlessTestApp.Run(() =>
        {
            var manager = new ThemeManager(static () => true);
            var context = new ThemeContext(
                manager,
                ThemeTokenResourceProviderTests.Compile(),
                0);
            var host = new Window();
            var lease = ThemeContextLease.Attach(host, context);

            manager.Dispose();

            host.IsSet(ThemeScope.ContextProperty).ShouldBeFalse();
            host.IsSet(TopLevel.RequestedThemeVariantProperty).ShouldBeFalse();
            host.Resources.MergedDictionaries
                .OfType<ThemeContextResourceBridge>()
                .ShouldBeEmpty();
            lease.Dispose();
            host.Close();
        });
    }

    [Fact]
    public void Dispose_Continues_Cleanup_When_A_Host_Property_Observer_Throws()
    {
        HeadlessTestApp.Run(() =>
        {
            var manager = new ThemeManager(static () => true);
            var context = new ThemeContext(
                manager,
                ThemeTokenResourceProviderTests.Compile(),
                0);
            var host = new ThrowingWindow();
            var lease = ThemeContextLease.Attach(host, context);
            host.ThrowOnNextContextChange = true;

            lease.Dispose();

            host.IsSet(TopLevel.RequestedThemeVariantProperty).ShouldBeFalse();
            host.Resources.MergedDictionaries
                .OfType<ThemeContextResourceBridge>()
                .ShouldBeEmpty();
            manager.Dispose();
            host.Close();
        });
    }

    private static ThemeContext CreateContext(
        string? globalPrimary = null,
        long registrationId = 0)
    {
        return new ThemeContext(
            new ThemeManager(static () => true),
            ThemeTokenResourceProviderTests.Compile(globalPrimary),
            registrationId);
    }

    private sealed class ThrowingWindow : Window
    {
        internal bool ThrowOnNextContextChange { get; set; }

        protected override void OnPropertyChanged(Avalonia.AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            if (ThrowOnNextContextChange && change.Property == ThemeScope.ContextProperty)
            {
                ThrowOnNextContextChange = false;
                throw new InvalidOperationException("context update failed");
            }
        }
    }
}
