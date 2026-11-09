using System.Xml.Linq;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.ImagePreviewer;

public class ImagePreviewerLoadingVisualTests
{
    [Fact]
    public void Single_ImagePreviewer_Template_Relays_Cover_Size_To_Cover_Control()
    {
        var theme = ReadRepoFile("src/AtomUI.Desktop.Controls/ImagePreviewer/Themes/ImagePreviewerTheme.axaml");

        theme.ShouldContain("Width=\"{TemplateBinding CoverWidth}\"");
        theme.ShouldContain("Height=\"{TemplateBinding CoverHeight}\"");
    }

    [Fact]
    public void Cover_Default_Loading_Uses_Image_Skeleton()
    {
        var theme = ReadRepoFile("src/AtomUI.Desktop.Controls/ImagePreviewer/Themes/ImagePreviewerCoverTheme.axaml");

        theme.ShouldContain("Name=\"PART_LoadingPresenter\"");
        theme.ShouldContain("MinWidth=\"{atom:ImagePreviewerTokenResource CoverImageWidth}\"");
        theme.ShouldContain("MinHeight=\"{atom:ImagePreviewerTokenResource CoverImageWidth}\"");
        theme.ShouldContain("atom:SkeletonImage");
        theme.ShouldContain("Name=\"PART_LoadingSkeleton\"");
        theme.ShouldContain("IsActive=\"False\"");
        theme.ShouldContain("^:loading:not(:has-image) /template/ Border#PART_LoadingPresenter");
        theme.ShouldContain("^:loading-skeleton /template/ atom|SkeletonImage#PART_LoadingSkeleton");
        theme.ShouldContain("<Setter Property=\"IsActive\" Value=\"True\" />");
    }

    [Fact]
    public void Default_Error_Text_Is_Localized_For_Cover_And_Viewer()
    {
        var coverTheme  = ReadRepoFile("src/AtomUI.Desktop.Controls/ImagePreviewer/Themes/ImagePreviewerCoverTheme.axaml");
        var viewerTheme = ReadRepoFile("src/AtomUI.Desktop.Controls/ImagePreviewer/Themes/ImageViewerTheme.axaml");
        var enUS        = ReadLocalizationFile("src/AtomUI.Desktop.Controls/ImagePreviewer/Localization/en-US.xlf");
        var zhCN        = ReadLocalizationFile("src/AtomUI.Desktop.Controls/ImagePreviewer/Localization/zh-CN.xlf");
        var zhTW        = ReadLocalizationFile("src/AtomUI.Desktop.Controls/ImagePreviewer/Localization/zh-TW.xlf");

        coverTheme.ShouldNotContain("Image load failed");
        viewerTheme.ShouldNotContain("Image load failed");
        coverTheme.ShouldContain("Name=\"PART_ErrorPresenter\"");
        coverTheme.ShouldContain("IsVisible=\"{TemplateBinding HasError}\"");
        coverTheme.ShouldContain("MinWidth=\"{atom:ImagePreviewerTokenResource CoverImageWidth}\"");
        coverTheme.ShouldContain("MinHeight=\"{atom:ImagePreviewerTokenResource CoverImageWidth}\"");
        coverTheme.ShouldContain("{atom:ImagePreviewerLangResource ImageLoadFailed}");
        viewerTheme.ShouldContain("{atom:ImagePreviewerLangResource ImageLoadFailed}");
        enUS.ContainsKey("ImageLoadFailed").ShouldBeTrue();
        zhCN.ContainsKey("ImageLoadFailed").ShouldBeTrue();
        zhTW.ContainsKey("ImageLoadFailed").ShouldBeTrue();
    }

    [Fact]
    public void Default_Error_Icon_Uses_Prominent_Size_For_Cover_And_Viewer()
    {
        var coverTheme  = ReadRepoFile("src/AtomUI.Desktop.Controls/ImagePreviewer/Themes/ImagePreviewerCoverTheme.axaml");
        var viewerTheme = ReadRepoFile("src/AtomUI.Desktop.Controls/ImagePreviewer/Themes/ImageViewerTheme.axaml");

        coverTheme.ShouldContain("<Setter Property=\"Width\" Value=\"{atom:ImagePreviewerTokenResource ErrorIconSize}\" />");
        coverTheme.ShouldContain("<Setter Property=\"Height\" Value=\"{atom:ImagePreviewerTokenResource ErrorIconSize}\" />");
        viewerTheme.ShouldContain("<Setter Property=\"Width\" Value=\"{atom:ImagePreviewerTokenResource ErrorIconSize}\" />");
        viewerTheme.ShouldContain("<Setter Property=\"Height\" Value=\"{atom:ImagePreviewerTokenResource ErrorIconSize}\" />");
    }

    [Fact]
    public void Dialog_ImageViewer_Paints_Dialog_Background_To_Cover_Csd_State_Transition_Frames()
    {
        var dialogSource = ReadRepoFile("src/AtomUI.Desktop.Controls/ImagePreviewer/ImagePreviewerDialog.cs");
        var viewerTheme  = ReadRepoFile("src/AtomUI.Desktop.Controls/ImagePreviewer/Themes/ImageViewerTheme.axaml");

        dialogSource.ShouldContain("viewer[!ImageViewer.BackgroundProperty]");
        dialogSource.ShouldContain("this[!BackgroundProperty]");
        viewerTheme.ShouldContain("<Panel Background=\"{TemplateBinding Background}\">");
    }

    [Fact]
    public void Viewer_Loading_Presenter_Is_Gated_By_The_Has_Image_Pseudo_Class()
    {
        var viewerTheme = ReadRepoFile("src/AtomUI.Desktop.Controls/ImagePreviewer/Themes/ImageViewerTheme.axaml");

        viewerTheme.ShouldContain("^:loading:not(:has-image) /template/ Border#PART_LoadingPresenter");
        viewerTheme.ShouldNotContain("IsVisible=\"{TemplateBinding IsCurrentImageLoading}\"");
    }

    private static string ReadRepoFile(string relativePath)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, relativePath);
            if (File.Exists(candidate))
            {
                return File.ReadAllText(candidate);
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException($"Could not find repository file: {relativePath}");
    }

    private static IReadOnlyDictionary<string, string> ReadLocalizationFile(string relativePath)
    {
        var document = XDocument.Parse(ReadRepoFile(relativePath));
        XNamespace xliff = "urn:oasis:names:tc:xliff:document:2.0";
        return document.Descendants(xliff + "unit")
                       .ToDictionary(
                           unit => (string?)unit.Attribute("id") ?? throw new InvalidDataException(
                               $"XLIFF unit in '{relativePath}' has no key."),
                           unit => unit.Descendants(xliff + "target").SingleOrDefault()?.Value ??
                                   unit.Descendants(xliff + "source").Single().Value,
                           StringComparer.Ordinal);
    }
}
