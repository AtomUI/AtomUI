using Shouldly;
using Xunit;

namespace AtomUI.Controls.Shared.Tests.ImageLoading;

public class ImageLoadingOptionsBuilderTests
{
    [Fact]
    public void Default_Persistent_Cache_Directory_Uses_The_Host_Application_Name()
    {
        var builder = new ImageLoadingOptionsBuilder
        {
            IsPersistentCacheEnabled = true
        };

        var options = builder.Build("Acme.ImageHost");
        var applicationDataRoot = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        if (string.IsNullOrWhiteSpace(applicationDataRoot))
        {
            applicationDataRoot = Path.GetTempPath();
        }

        options.PersistentCacheDirectory.ShouldBe(Path.Combine(
            applicationDataRoot,
            "Acme.ImageHost",
            "image-cache",
            "debb2dd585131eb9"));
    }

    [Fact]
    public void Default_Persistent_Cache_Directory_Falls_Back_To_AtomUI_When_Application_Name_Is_Unavailable()
    {
        var builder = new ImageLoadingOptionsBuilder
        {
            IsPersistentCacheEnabled = true
        };

        var options = builder.Build(null!);
        var applicationDataRoot = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        if (string.IsNullOrWhiteSpace(applicationDataRoot))
        {
            applicationDataRoot = Path.GetTempPath();
        }

        options.PersistentCacheDirectory.ShouldBe(Path.Combine(
            applicationDataRoot,
            "AtomUI",
            "image-cache",
            "b2698e9ba9507572"));
    }
}
