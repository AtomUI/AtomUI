using AtomUI.Controls;
using Shouldly;
using Xunit;

namespace AtomUI.Core.Tests.AppLifecycle;

public class ImageLoadingApplicationBuilderTests
{
    [Fact]
    public void Independent_UseAtomUI_Builders_Publish_Separate_Image_Loaders()
    {
        HeadlessTestApp.Run(() =>
        {
            var firstApplication = new Avalonia.Application();
            var secondApplication = new Avalonia.Application();
            try
            {
                ApplicationStartup.AttachWith(firstApplication, builder => builder.UseImageLoading());
                ApplicationStartup.AttachWith(secondApplication, builder => builder.UseImageLoading());

                var firstLoader = firstApplication.GetImageLoader();
                var secondLoader = secondApplication.GetImageLoader();

                firstLoader.ShouldNotBeSameAs(secondLoader);
                firstLoader.Snapshot.ShouldBe(default(ImageLoaderSnapshot));
                secondLoader.Snapshot.ShouldBe(default(ImageLoaderSnapshot));
            }
            finally
            {
                ApplicationScopeRegistry.Get(secondApplication)?.Dispose();
                ApplicationScopeRegistry.Get(firstApplication)?.Dispose();
            }

            firstApplication.TryGetImageLoader().ShouldBeNull();
            secondApplication.TryGetImageLoader().ShouldBeNull();
        });
    }
}
