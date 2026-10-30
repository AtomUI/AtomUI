using Avalonia.Controls;
using Avalonia.Media;
using Shouldly;
using Xunit;

namespace AtomUI.Localization.Tests;

public class LanguageResourceProviderTests
{
    [Fact]
    public void Provider_Projects_FlowDirection_From_The_Current_Language_Revision()
    {
        var runtime = LanguageManagerTestRuntime.Create();

        runtime.Provider.TryGetResource(
            LanguageResourceKeys.FlowDirection,
            null,
            out var leftToRight).ShouldBeTrue();
        runtime.Manager.ChangeLanguage(LanguageTags.ArSA);
        runtime.Provider.TryGetResource(
            LanguageResourceKeys.FlowDirection,
            null,
            out var rightToLeft).ShouldBeTrue();

        leftToRight.ShouldBe(FlowDirection.LeftToRight);
        rightToLeft.ShouldBe(FlowDirection.RightToLeft);
    }

    [Fact]
    public void Provider_Resolves_The_Exact_Boxed_Enum_Key_From_The_Current_Snapshot()
    {
        var runtime = LanguageManagerTestRuntime.Create();

        runtime.Provider.TryGetResource(RuntimeResourceKind.Value, null, out var english).ShouldBeTrue();
        runtime.Manager.ChangeLanguage(LanguageTags.ZhCN);
        runtime.Provider.TryGetResource(RuntimeResourceKind.Value, null, out var chinese).ShouldBeTrue();

        english.ShouldBe("English");
        chinese.ShouldBe("中文");
    }

    [Fact]
    public void Provider_Rejects_Unknown_Keys_And_Different_Enum_Types()
    {
        var runtime = LanguageManagerTestRuntime.Create();

        runtime.Provider.TryGetResource("Acme:Value", null, out var stringValue).ShouldBeFalse();
        runtime.Provider.TryGetResource(OtherResourceKind.Value, null, out var enumValue).ShouldBeFalse();

        stringValue.ShouldBeNull();
        enumValue.ShouldBeNull();
    }

    [Fact]
    public void Provider_Misses_Non_Language_Keys_Without_Allocating_After_First_Lookup()
    {
        var runtime = LanguageManagerTestRuntime.Create();
        // 未命中控件树的 ControlTheme（Type 键）、字符串键和 token enum 键都会查到 Application 层的本 provider。
        object[] keys = [typeof(Border), "SomeBrushKey", OtherResourceKind.Value];
        foreach (var key in keys)
        {
            runtime.Provider.TryGetResource(key, null, out _).ShouldBeFalse();
        }

        var allocated = GC.GetAllocatedBytesForCurrentThread();
        var found = false;
        for (var index = 0; index < 100; index++)
        {
            foreach (var key in keys)
            {
                found |= runtime.Provider.TryGetResource(key, null, out _);
            }
        }

        found.ShouldBeFalse();
        (GC.GetAllocatedBytesForCurrentThread() - allocated).ShouldBe(0);
    }

    [Fact]
    public void Provider_Raises_Exactly_One_Host_Notification_Per_Committed_Change()
    {
        var runtime = LanguageManagerTestRuntime.Create();
        var host = new Border();
        host.Resources.MergedDictionaries.Add(runtime.Provider);
        var notifications = 0;
        ((IResourceHost)host).ResourcesChanged += (_, _) => notifications++;

        runtime.Manager.ChangeLanguage(LanguageTags.ZhCN);
        runtime.Manager.ChangeLanguage(LanguageTags.ZhCN);
        runtime.Manager.ChangeLanguage(LanguageTags.ArSA);

        notifications.ShouldBe(2);
    }

    private enum OtherResourceKind
    {
        Value = 1
    }
}
