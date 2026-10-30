using AtomUI.Desktop.Controls;
using AtomUI.Toolkits.GalleryBase.Controls;
using AtomUI.Toolkits.GalleryBase.Controls.DesignTokens;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Toolkits.GalleryBase.Tests.Controls;

public class ShowCaseItemDeferredContentTests : IDisposable
{
    private readonly string? _oldDeferredLoadingEnvironmentValue;

    public ShowCaseItemDeferredContentTests()
    {
        AvaloniaTestApp.EnsureInitialized();
        _oldDeferredLoadingEnvironmentValue = Environment.GetEnvironmentVariable(
            GalleryShowCaseRuntimeOptions.DisableDeferredLoadingEnvironmentVariable);
        Environment.SetEnvironmentVariable(
            GalleryShowCaseRuntimeOptions.DisableDeferredLoadingEnvironmentVariable,
            null);
        GalleryShowCaseRuntimeOptions.ResetDeferredLoadingDisabledOverride();
    }

    public void Dispose()
    {
        Environment.SetEnvironmentVariable(
            GalleryShowCaseRuntimeOptions.DisableDeferredLoadingEnvironmentVariable,
            _oldDeferredLoadingEnvironmentValue);
        GalleryShowCaseRuntimeOptions.ResetDeferredLoadingDisabledOverride();
    }

    [Fact]
    public void Materializing_Without_Data_Does_Not_Set_A_Local_Null_DataContext()
    {
        var item = CreateDeferredItem();

        item.MaterializeDeferredContent();

        var content = item.Content.ShouldBeOfType<Border>();
        content.IsSet(StyledElement.DataContextProperty).ShouldBeFalse();
    }

    [Fact]
    public void Materializing_With_Explicit_Data_Assigns_It_To_The_Content()
    {
        var data = new object();
        var item = CreateDeferredItem();
        item.DeferredContent = data;

        item.MaterializeDeferredContent();

        item.Content.ShouldBeOfType<Border>().DataContext.ShouldBeSameAs(data);
    }

    [Fact]
    public void Ordinary_Template_Uses_A_Visible_Skeleton_For_Deferred_Content()
    {
        AssertSkeletonPlaceholder(badgeText: null);
    }

    [Fact]
    public void Badge_Template_Uses_A_Visible_Skeleton_For_Deferred_Content()
    {
        AssertSkeletonPlaceholder("New");
    }

    private static ShowCaseItem CreateDeferredItem() =>
        new()
        {
            IsDeferredContentEnabled = true,
            DeferredContentTemplate  = new FuncDataTemplate<object?>((_, _) => new Border())
        };

    private static void AssertSkeletonPlaceholder(string? badgeText)
    {
        var item = CreateDeferredItem();
        item.BadgeText                 = badgeText;
        item.DeferredPlaceholderHeight = 240;
        var window = new AvaloniaWindow { Width = 500, Height = 500, Content = item };
        var expectedCornerRadius = new CornerRadius(13);
        window.Resources[ShowCaseItemTokenKind.DeferredPlaceholderCornerRadius] = expectedCornerRadius;
        try
        {
            window.Show();
            item.ApplyTemplate();
            Dispatcher.UIThread.RunJobs();

            var placeholder = item.GetVisualDescendants()
                                  .OfType<Skeleton>()
                                  .SingleOrDefault(skeleton => skeleton.Name == "PART_DeferredPlaceholder");
            placeholder.ShouldNotBeNull();
            placeholder.IsVisible.ShouldBeTrue();
            placeholder.IsLoading.ShouldBeTrue();
            placeholder.IsActive.ShouldBeTrue();
            placeholder.IsShowTitle.ShouldBeFalse();
            placeholder.ParagraphRows.ShouldBe(4);
            placeholder.MinHeight.ShouldBe(240);
            placeholder.CornerRadius.ShouldBe(expectedCornerRadius);
            var placeholderFrame = placeholder.GetVisualChildren().OfType<Border>().Single();
            placeholderFrame.CornerRadius.ShouldBe(expectedCornerRadius);
            placeholderFrame.BorderBrush.ShouldNotBeNull();
            placeholderFrame.BorderThickness.ShouldNotBe(default);
            item.IsDeferredContentMaterialized.ShouldBeFalse();
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }
}
