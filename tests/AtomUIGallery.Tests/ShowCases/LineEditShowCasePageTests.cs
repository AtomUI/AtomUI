using System.Security.Cryptography;
using System.Text;
using Avalonia.Controls;
using Shouldly;
using Xunit;

namespace AtomUIGallery.Tests.ShowCases;

public class LineEditShowCasePageTests
{

    [Fact]
    public void LineEdit_ShowCase_Declares_The_Semantic_Preview_And_Style_Example()
    {
        var source = ReadRepoFile(
            "controlgallery/AtomUIGallery/ShowCases/DataEntry/LineEdit/Views/LineEditShowCase.axaml");
        var english = ReadRepoFile(
            "controlgallery/AtomUIGallery/ShowCases/DataEntry/LineEdit/Localization/en-US.xlf");
        var semanticExample = ExtractShowCaseItem(source, "line-edit-semantic-part");

        source.ShouldContain("<gallery:GalleryShowCaseHost.SemanticPartsContentTemplate>");
        source.ShouldContain("Name=\"LineEditSemanticPreview\"");
        source.ShouldContain("SemanticOwner=\"{Binding #LineEditSemanticOwner}\"");
        source.ShouldContain("SemanticOwnerType=\"{x:Type atom:LineEdit}\"");
        source.ShouldContain("Name=\"LineEditSemanticOwner\"");
        source.ShouldContain("SizeType=\"Middle\"");
        source.ShouldContain("StyleVariant=\"Outlined\"");
        source.ShouldContain("IsAllowClear=\"True\"");
        source.ShouldContain("IsShowCount=\"True\"");
        foreach (var path in new[] { "root", "prefix", "input", "suffix", "clear", "count" })
        {
            source.ShouldContain($"Path=\"{path}\"");
        }

        source.ShouldContain("Title=\"TextArea\"");
        source.ShouldContain("SemanticOwner=\"{Binding #TextAreaSemanticOwner}\"");
        source.ShouldContain("SemanticOwnerType=\"{x:Type atom:TextArea}\"");
        source.ShouldContain("Name=\"TextAreaSemanticOwner\"");
        source.ShouldContain("Path=\"textarea\"");
        source.ShouldContain("IsShowCount=\"True\"");
        source.ShouldContain("MaxLength=\"100\"");

        source.ShouldContain("Title=\"SearchEdit\"");
        source.ShouldContain("SemanticOwner=\"{Binding #SearchEditSemanticOwner}\"");
        source.ShouldContain("SemanticOwnerType=\"{x:Type atom:SearchEdit}\"");
        source.ShouldContain("Name=\"SearchEditSemanticOwner\"");
        source.ShouldContain("Path=\"button\"");
        source.ShouldContain("SearchButtonStyle=\"Primary\"");
        source.ShouldContain("IsOperating=\"True\"");

        var passwordPreview = ExtractSemanticPreview(source, "Name=\"PasswordSemanticPreview\"");
        passwordPreview.ShouldContain("Title=\"Password\"");
        passwordPreview.ShouldContain("SemanticOwner=\"{Binding #PasswordSemanticOwner}\"");
        passwordPreview.ShouldContain("SemanticOwnerType=\"{x:Type atom:LineEdit}\"");
        passwordPreview.ShouldContain("Name=\"PasswordSemanticOwner\"");
        passwordPreview.ShouldContain("PasswordChar=\"•\"");
        passwordPreview.ShouldContain("RevealPassword=\"False\"");
        passwordPreview.ShouldContain("IsEnableRevealButton=\"True\"");
        passwordPreview.ShouldContain("InnerLeftContent=\"{antdicons:AntDesignIconProvider Kind=UserOutlined}\"");
        passwordPreview.ShouldContain("IsAllowClear=\"True\"");
        passwordPreview.ShouldContain("IsShowCount=\"True\"");
        passwordPreview.ShouldContain("MaxLength=\"20\"");
        foreach (var path in new[] { "root", "prefix", "input", "suffix", "clear", "count" })
        {
            passwordPreview.ShouldContain($"Path=\"{path}\"");
        }

        var otpPreview = ExtractSemanticPreview(source, "Name=\"OtpSemanticPreview\"");
        otpPreview.ShouldContain("Title=\"OtpLineEdit\"");
        otpPreview.ShouldContain("SemanticOwner=\"{Binding #OtpSemanticOwner}\"");
        otpPreview.ShouldContain("SemanticOwnerType=\"{x:Type atom:OtpLineEdit}\"");
        otpPreview.ShouldContain("Name=\"OtpSemanticOwner\"");
        otpPreview.ShouldContain("Length=\"6\"");
        otpPreview.ShouldContain("Separator=\"-\"");
        otpPreview.ShouldNotContain("IsAllowClear");
        foreach (var path in new[] { "root", "cell", "cellList", "separator" })
        {
            otpPreview.ShouldContain($"Path=\"{path}\"");
        }

        semanticExample.ShouldContain("SourceKey=\"line-edit-semantic-part\"");
        semanticExample.ShouldContain("Selector=\"atom|LineEdit.style-class-base\"");
        semanticExample.ShouldContain("Selector=\"atom|LineEdit.style-class-object\"");
        semanticExample.ShouldContain("Selector=\"atom|LineEdit.style-class-object:focus-within\"");
        semanticExample.ShouldContain("<Setter Property=\"BorderBrush\" Value=\"#D9D9D9\" />");
        semanticExample.ShouldContain("<Setter Property=\"BorderBrush\" Value=\"#A9A9A9\" />");
        semanticExample.ShouldContain("Selector=\"atom|LineEdit.style-class-fn\"");
        semanticExample.ShouldContain("Selector=\"atom|LineEdit.style-class-password\"");
        semanticExample.ShouldContain("Selector=\"atom|TextArea.style-class-textarea\"");
        semanticExample.ShouldContain("Selector=\"atom|OtpLineEdit.style-class-otp\"");
        semanticExample.ShouldContain("Selector=\"atom|SearchEdit.style-class-search\"");
        semanticExample.ShouldNotContain("BorderThickness");
        semanticExample.ShouldNotContain("CornerRadius");
        // Part styling must use the generated dedicated Semantic Part styles
        // (TextAreaCountStyle / SearchEditInputStyle / SearchEditButtonStyle),
        // never hand-written part selectors.
        CountOccurrences(semanticExample, "<atom:TextAreaCountStyle x:SetterTargetType=\"TextBlock\">").ShouldBe(1);
        CountOccurrences(semanticExample, "<atom:SearchEditInputStyle x:SetterTargetType=\"TextPresenter\">").ShouldBe(1);
        CountOccurrences(semanticExample, "<atom:SearchEditButtonStyle x:SetterTargetType=\"atom:Button\">").ShouldBe(1);
        semanticExample.ShouldNotContain("/template/ atom|TextBlock.semantic-count");
        semanticExample.ShouldNotContain("/template/ .semantic-input");
        semanticExample.ShouldNotContain(".semantic-scope-input-frame");
        semanticExample.ShouldNotContain("clr-namespace:AtomUI.Theme.Styling");
        CountOccurrences(semanticExample, "<Setter Property=\"Foreground\" Value=\"#4DA8DA\" />").ShouldBe(2);
        semanticExample.ShouldContain("<Setter Property=\"TextElement.Foreground\" Value=\"#4DA8DA\" />");
        CountOccurrences(semanticExample, "<Setter Property=\"BorderBrush\" Value=\"#4DA8DA\" />").ShouldBe(2);
        semanticExample.ShouldContain("<Setter Property=\"CaretBrush\" Value=\"#4DA8DA\" />");
        CountOccurrences(semanticExample, "#696FC7").ShouldBe(1);
        CountOccurrences(semanticExample, "#BDE3C3").ShouldBe(2);
        CountOccurrences(semanticExample, "#F5D3C4").ShouldBe(1);
        CountOccurrences(semanticExample, "#6E8CFB").ShouldBe(1);
        CountOccurrences(semanticExample, "#4DA8DA").ShouldBeGreaterThanOrEqualTo(3);
        CountOccurrences(semanticExample, "<atom:LineEdit").ShouldBe(3);
        semanticExample.ShouldContain("Classes=\"style-class-base style-class-fn\"");
        semanticExample.ShouldContain("Classes=\"style-class-base style-class-password\"");
        semanticExample.ShouldContain("SizeType=\"Middle\"");
        semanticExample.ShouldContain("Length=\"6\"");
        semanticExample.ShouldContain("Separator=\"*\"");
        semanticExample.ShouldContain("<Setter Property=\"CellWidth\" Value=\"32\" />");
        semanticExample.ShouldContain("<Setter Property=\"CellBorderBrush\" Value=\"#6E8CFB\" />");
        semanticExample.ShouldContain("SizeType=\"Large\"");

        foreach (var staleCaption in new[] { "SemanticPartTextStyleTitle", "SemanticPartPrefixStyleTitle", "SemanticPartClearStyleTitle", "SemanticPartAccentStyleTitle" })
        {
            source.ShouldNotContain(staleCaption);
            english.ShouldNotContain($"unit id=\"{staleCaption}\"");
        }
        english.ShouldContain("<source>Custom Semantic Part styling</source>");

        foreach (var caption in new[]
                 {
                     "SemanticTextAreaRootDescription", "SemanticTextAreaDescription",
                     "SemanticTextAreaClearDescription", "SemanticTextAreaCountDescription",
                     "SemanticSearchRootDescription", "SemanticSearchPrefixDescription",
                     "SemanticSearchInputDescription", "SemanticSearchSuffixDescription",
                     "SemanticSearchClearDescription", "SemanticSearchButtonDescription",
                     "SemanticPasswordRootDescription", "SemanticPasswordPrefixDescription",
                     "SemanticPasswordInputDescription", "SemanticPasswordSuffixDescription",
                     "SemanticPasswordClearDescription", "SemanticPasswordCountDescription",
                     "SemanticOtpRootDescription", "SemanticOtpCellListDescription",
                     "SemanticOtpCellDescription", "SemanticOtpSeparatorDescription"
                 })
        {
            english.ShouldContain($"unit id=\"{caption}\"");
        }
        english.ShouldContain("unit id=\"SemanticRootDescription\"");
        english.ShouldContain("unit id=\"SemanticPrefixDescription\"");
        english.ShouldContain("unit id=\"SemanticInputDescription\"");
        english.ShouldContain("unit id=\"SemanticSuffixDescription\"");
        english.ShouldContain("unit id=\"SemanticClearDescription\"");
        english.ShouldContain("unit id=\"SemanticCountDescription\"");
        english.ShouldContain("unit id=\"SemanticPartStyleTitle\"");
        english.ShouldContain("unit id=\"SemanticPartStyleDescription\"");
    }

    private static string ExtractLineEditExampleItems(string source)
    {
        const string firstItemMarker  = "<gallery:ShowCaseItem";
        const string panelCloseMarker = "</gallery:ShowCasePanel>";

        var firstItemStart = source.IndexOf(firstItemMarker, StringComparison.Ordinal);
        firstItemStart.ShouldBeGreaterThanOrEqualTo(0);

        var panelCloseStart = source.IndexOf(panelCloseMarker, firstItemStart, StringComparison.Ordinal);
        panelCloseStart.ShouldBeGreaterThan(firstItemStart);

        return StripDeferredLoadingMarkup(source[firstItemStart..panelCloseStart]);
    }

    private static string ExtractSemanticPreview(string source, string nameMarker)
    {
        var nameIndex = source.IndexOf(nameMarker, StringComparison.Ordinal);
        nameIndex.ShouldBeGreaterThanOrEqualTo(0);

        const string previewStartMarker = "<gallery:SemanticPartPreview";
        const string previewEndMarker   = "</gallery:SemanticPartPreview>";

        var previewStart = source.LastIndexOf(previewStartMarker, nameIndex, StringComparison.Ordinal);
        previewStart.ShouldBeGreaterThanOrEqualTo(0);

        var previewEnd = source.IndexOf(previewEndMarker, nameIndex, StringComparison.Ordinal);
        previewEnd.ShouldBeGreaterThan(nameIndex);

        return source[previewStart..(previewEnd + previewEndMarker.Length)];
    }

    private static string ExtractShowCaseItem(string source, string titleMarker)
    {
        var titleIndex = source.IndexOf(titleMarker, StringComparison.Ordinal);
        titleIndex.ShouldBeGreaterThanOrEqualTo(0);

        const string itemStartMarker = "<gallery:ShowCaseItem";
        const string itemEndMarker   = "</gallery:ShowCaseItem>";

        var itemStart = source.LastIndexOf(itemStartMarker, titleIndex, StringComparison.Ordinal);
        itemStart.ShouldBeGreaterThanOrEqualTo(0);

        var itemEnd = source.IndexOf(itemEndMarker, titleIndex, StringComparison.Ordinal);
        itemEnd.ShouldBeGreaterThan(titleIndex);

        return source[itemStart..(itemEnd + itemEndMarker.Length)];
    }

    private static string StripDeferredLoadingMarkup(string source)
    {
        return ShowCaseSnapshotMarkup.StripDeferredLoadingMarkup(source);
    }

    private static string NormalizeMarkup(string source)
    {
        return ShowCaseSnapshotMarkup.Normalize(source);
    }

    private static string ComputeSha256(string source)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(source));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static string ReadSnapshotHash(string source)
    {
        return source
            .Split('\n')
            .First(line => line.StartsWith("sha256:", StringComparison.Ordinal))
            .Split(':', 2)[1]
            .Trim();
    }

    private static int ReadSnapshotCount(string source)
    {
        return int.Parse(source
            .Split('\n')
            .First(line => line.StartsWith("count:", StringComparison.Ordinal))
            .Split(':', 2)[1]
            .Trim());
    }

    private static int CountOccurrences(string source, string value)
    {
        var count      = 0;
        var startIndex = 0;
        while (true)
        {
            var matchIndex = source.IndexOf(value, startIndex, StringComparison.Ordinal);
            if (matchIndex < 0)
            {
                return count;
            }

            count++;
            startIndex = matchIndex + value.Length;
        }
    }

    private static string ReadRepoFile(string relativePath)
    {
        var path = GetRepoFile(relativePath);
        File.Exists(path).ShouldBeTrue($"Expected repository file to exist: {relativePath}");
        return File.ReadAllText(path);
    }

    private static string GetRepoFile(string relativePath)
    {
        var path = GetRepoPath(relativePath);
        return File.Exists(path)
            ? path
            : Path.Combine(AppContext.BaseDirectory, relativePath);
    }

    private static string GetRepoPath(string relativePath)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, relativePath);
            if (File.Exists(candidate) || Directory.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        return Path.Combine(AppContext.BaseDirectory, relativePath);
    }
}
