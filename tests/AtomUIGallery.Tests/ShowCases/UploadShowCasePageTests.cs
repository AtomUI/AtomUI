using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Shouldly;
using Xunit;

namespace AtomUIGallery.Tests.ShowCases;

public class UploadShowCasePageTests
{

    [Fact]
    public void Upload_ShowCase_Uses_The_Stream_Source_Metadata_Contract()
    {
        var viewSource = ReadRepoFile(
            "controlgallery/AtomUIGallery/ShowCases/DataEntry/Upload/Views/UploadShowCase.axaml.cs");
        var viewModelSource = ReadRepoFile(
            "controlgallery/AtomUIGallery/ShowCases/DataEntry/Upload/ViewModels/UploadViewModel.cs");

        viewSource.ShouldNotContain("FilePath");
        viewSource.ShouldContain("Path.GetExtension(fileInfo.Name)");
        viewSource.ShouldContain("fileInfo.Size ?? 0");
        viewSource.ShouldContain("fileInfo.Path ??");
        viewModelSource.ShouldContain("IReadOnlyList<FilePickerFileType> PngFileTypes");
        viewModelSource.ShouldContain("Patterns = [\"*.png\"]");
    }

    private static string ExtractUploadExampleItems(string source)
    {
        const string firstItemMarker  = "<gallery:ShowCaseItem";
        const string panelCloseMarker = "</gallery:ShowCasePanel>";

        var firstItemStart = source.IndexOf(firstItemMarker, StringComparison.Ordinal);
        firstItemStart.ShouldBeGreaterThanOrEqualTo(0);

        var panelCloseStart = source.IndexOf(panelCloseMarker, firstItemStart, StringComparison.Ordinal);
        panelCloseStart.ShouldBeGreaterThan(firstItemStart);

        return source[firstItemStart..panelCloseStart];
    }

    private static string NormalizeMarkup(string source)
    {
        return ShowCaseSnapshotMarkup.Normalize(StripUploadBehaviorMarkup(source));
    }

    private static string StripUploadBehaviorMarkup(string source)
    {
        var normalized = Regex.Replace(
            source,
            "\\s*UploadTransport=\"\\{Binding UploadTransport\\}\"",
            string.Empty,
            RegexOptions.CultureInvariant);

        normalized = Regex.Replace(
            normalized,
            "\\s*UploadTaskFailed=\"HandleUploadFailed\"",
            string.Empty,
            RegexOptions.CultureInvariant);

        normalized = Regex.Replace(
            normalized,
            "\\s*UploadTaskCompleted=\"HandleUploadCompleted\"",
            string.Empty,
            RegexOptions.CultureInvariant);

        normalized = Regex.Replace(
            normalized,
            "\\s*UploadTaskAboutToScheduling=\"Handle(?:Image|Png)UploadAboutToScheduling\"",
            string.Empty,
            RegexOptions.CultureInvariant);

        return normalized;
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
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, relativePath);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        return Path.Combine(AppContext.BaseDirectory, relativePath);
    }
}
