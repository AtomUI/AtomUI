using AtomUI.Toolkits.GalleryBase.Controls;
using Shouldly;
using Xunit;

namespace AtomUI.Toolkits.GalleryBase.Tests.Controls;

public class GalleryShowCaseRuntimeOptionsTests
{
    [Fact]
    public void Environment_Value_Is_Cached_Until_Reset_Refreshes_It()
    {
        var variableName = GalleryShowCaseRuntimeOptions.DisableDeferredLoadingEnvironmentVariable;
        var oldValue     = Environment.GetEnvironmentVariable(variableName);
        try
        {
            Environment.SetEnvironmentVariable(variableName, null);
            GalleryShowCaseRuntimeOptions.ResetDeferredLoadingDisabledOverride();
            GalleryShowCaseRuntimeOptions.IsDeferredLoadingDisabled.ShouldBeFalse();

            Environment.SetEnvironmentVariable(variableName, "1");

            GalleryShowCaseRuntimeOptions.IsDeferredLoadingDisabled.ShouldBeFalse();

            GalleryShowCaseRuntimeOptions.ResetDeferredLoadingDisabledOverride();
            GalleryShowCaseRuntimeOptions.IsDeferredLoadingDisabled.ShouldBeTrue();
        }
        finally
        {
            Environment.SetEnvironmentVariable(variableName, oldValue);
            GalleryShowCaseRuntimeOptions.ResetDeferredLoadingDisabledOverride();
        }
    }

    [Fact]
    public void Setter_Refreshes_Environment_And_Raises_The_Effective_Change()
    {
        var variableName = GalleryShowCaseRuntimeOptions.DisableDeferredLoadingEnvironmentVariable;
        var oldValue     = Environment.GetEnvironmentVariable(variableName);
        var changeCount  = 0;
        EventHandler handler = (_, _) => changeCount++;
        try
        {
            Environment.SetEnvironmentVariable(variableName, null);
            GalleryShowCaseRuntimeOptions.ResetDeferredLoadingDisabledOverride();
            GalleryShowCaseRuntimeOptions.DeferredLoadingDisabledChanged += handler;

            Environment.SetEnvironmentVariable(variableName, "yes");

            GalleryShowCaseRuntimeOptions.IsDeferredLoadingDisabled = false;

            GalleryShowCaseRuntimeOptions.IsDeferredLoadingDisabled.ShouldBeTrue();
            changeCount.ShouldBe(1);
        }
        finally
        {
            GalleryShowCaseRuntimeOptions.DeferredLoadingDisabledChanged -= handler;
            Environment.SetEnvironmentVariable(variableName, oldValue);
            GalleryShowCaseRuntimeOptions.ResetDeferredLoadingDisabledOverride();
        }
    }
}
