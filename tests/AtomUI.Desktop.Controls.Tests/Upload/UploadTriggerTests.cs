using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.Upload;

public class UploadTriggerTests
{
    [Fact]
    public void UploadTrigger_Delegates_File_And_Directory_Actions_To_Owning_Upload()
    {
        var source = ReadRepoFile("src/AtomUI.Desktop.Controls/Upload/UploadTrigger.cs");

        source.ShouldContain("SourceKind == UploadSourceKind.Directories");
        source.ShouldContain("owner.SelectDirectoriesAsync()");
        source.ShouldContain("owner.SelectFilesAsync()");
        source.ShouldNotContain("UploadTriggerContent");
    }

    [Fact]
    public void UploadTrigger_Handles_Clicks_From_Button_Content()
    {
        var source = ReadRepoFile("src/AtomUI.Desktop.Controls/Upload/UploadTrigger.cs");

        source.ShouldContain("AddHandler(PointerReleasedEvent");
        source.ShouldContain("RoutingStrategies.Bubble");
        source.ShouldContain("handledEventsToo: true");
        source.ShouldContain("HandlePointerReleased");
        source.ShouldNotContain("if (e.Handled)");
    }

    [Fact]
    public void UploadTrigger_Observes_Picker_Tasks_Without_AsyncVoid()
    {
        var source = ReadRepoFile("src/AtomUI.Desktop.Controls/Upload/UploadTrigger.cs");
        var handledIndex = source.IndexOf("e.Handled = true;", StringComparison.Ordinal);
        var taskIndex = source.IndexOf("var task =", StringComparison.Ordinal);

        source.ShouldContain("private void HandlePointerReleased");
        source.ShouldNotContain("async void HandlePointerReleased");
        source.ShouldContain("ObserveSelectionOperationAsync(task)");
        handledIndex.ShouldBeGreaterThan(-1);
        taskIndex.ShouldBeGreaterThan(handledIndex);
    }

    [Fact]
    public void UploadTrigger_Restores_Picture_Shape_Visual_Shell()
    {
        var source = ReadRepoFile("src/AtomUI.Desktop.Controls/Upload/UploadTrigger.cs");
        var theme  = ReadRepoFile("src/AtomUI.Desktop.Controls/Upload/Themes/UploadTriggerTheme.axaml");

        source.ShouldContain("Upload.ListTypeProperty.AddOwner<UploadTrigger>()");
        source.ShouldContain("MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<UploadTrigger>()");
        source.ShouldNotContain("SetCurrentValue(CornerRadiusProperty");
        theme.ShouldContain("^[ListType=PictureCircle]");
        theme.ShouldContain("UploadShapeConverters.CircleCornerRadius");
        theme.ShouldContain("CompiledBinding $self.Bounds");
        source.ShouldContain("FindAncestorOfType<Upload>()");
        source.ShouldContain("ClearOwnerBindings()");
        source.ShouldContain("BindingPriority.Template");

        theme.ShouldContain("atom:DashedBorder Name=\"TriggerContentFrame\"");
        theme.ShouldContain("Background=\"{TemplateBinding Background}\"");
        theme.ShouldContain("BorderBrush=\"{TemplateBinding BorderBrush}\"");
        theme.ShouldContain("BorderThickness=\"{TemplateBinding BorderThickness}\"");
        theme.ShouldContain("CornerRadius=\"{TemplateBinding CornerRadius}\"");
        theme.ShouldContain("Width\" Value=\"{atom:UploadTokenResource PictureCardSize}\"");
        theme.ShouldContain("Height\" Value=\"{atom:UploadTokenResource PictureCardSize}\"");
        theme.ShouldContain("StrokeDashArray\" Value=\"4, 2\"");
        theme.ShouldContain("ColorFillAlter");
        theme.ShouldContain("ColorBorder");
        theme.ShouldContain("ColorPrimary");
        theme.ShouldContain("HorizontalContentAlignment\" Value=\"Center\"");
        theme.ShouldContain("VerticalContentAlignment\" Value=\"Center\"");
    }

    [Fact]
    public void UploadList_Owns_Internal_ScrollViewer()
    {
        var listTheme = ReadRepoFile("src/AtomUI.Desktop.Controls/Upload/Themes/UploadListTheme.axaml");
        var rootTheme = ReadRepoFile("src/AtomUI.Desktop.Controls/Upload/Themes/UploadTheme.axaml");

        listTheme.ShouldContain("<atom:ScrollViewer");
        listTheme.ShouldContain("IsLiteMode=\"True\"");
        listTheme.ShouldContain("AllowAutoHide=\"True\"");
        listTheme.ShouldContain("MaxHeight=\"{TemplateBinding ListMaxHeight}\"");
        listTheme.ShouldContain("VerticalScrollBarVisibility=\"{TemplateBinding ListScrollBarVisibility}\"");
        rootTheme.ShouldNotContain("<ScrollViewer");
    }

    [Fact]
    public void Upload_Template_Keeps_Trigger_And_List_Spaced()
    {
        var rootTheme = ReadRepoFile("src/AtomUI.Desktop.Controls/Upload/Themes/UploadTheme.axaml");

        rootTheme.ShouldContain("StackPanel Name=\"RootLayout\"");
        rootTheme.ShouldContain("Orientation=\"Vertical\"");
        rootTheme.ShouldContain("Spacing=\"{atom:UploadTokenResource SpacingXS}\"");
        rootTheme.ShouldNotContain("ControlTokenScope.Identity");
        rootTheme.ShouldNotContain("<DockPanel Name=\"RootLayout\"");
        rootTheme.ShouldNotContain("DockPanel.Dock=\"Top\"");
    }

    [Fact]
    public void UploadTextListItem_Indents_Uploading_Progress_From_File_Name_Start()
    {
        var theme = ReadRepoFile("src/AtomUI.Desktop.Controls/Upload/Themes/DefaultList/UploadTextListItemTheme.axaml");
        var token = ReadRepoFile("src/AtomUI.Desktop.Controls/Upload/UploadToken.cs");

        token.ShouldContain("TextListProgressPadding");
        token.ShouldContain("ControlTokenDefault(nameof(TextListProgressPadding))");
        token.ShouldContain("context.Global.FontSize + context.Global.UniformlyPaddingXS");
        theme.ShouldContain("Border Name=\"ProgressBarFrame\"");
        theme.ShouldContain("Padding=\"{atom:UploadTokenResource TextListProgressPadding}\"");
        theme.ShouldContain("<atom:ProgressBar Name=\"ProgressBar\"");
    }

    [Fact]
    public void UploadPictureUploadingContent_Does_Not_Show_Hover_Actions()
    {
        var baseTheme      = ReadRepoFile("src/AtomUI.Desktop.Controls/Upload/Themes/AbstractUploadPictureContentTheme.axaml");
        var uploadingTheme = ReadRepoFile("src/AtomUI.Desktop.Controls/Upload/Themes/PictureShapeList/UploadPictureShapeUploadingContentTheme.axaml");
        var previewTheme   = ReadRepoFile("src/AtomUI.Desktop.Controls/Upload/Themes/PictureShapeList/UploadPictureShapePreviewContentTheme.axaml");

        baseTheme.ShouldNotContain("<Style Selector=\"^:pointerover\">");
        baseTheme.ShouldContain("^[Status=Success]:pointerover");
        baseTheme.ShouldContain("^[Status=Failed]:pointerover");
        baseTheme.ShouldContain("^[Status=Cancelled]:pointerover");

        uploadingTheme.ShouldNotContain("Name=\"Mask\"");
        uploadingTheme.ShouldNotContain("DeleteOutlined");
        previewTheme.ShouldContain("Name=\"Mask\"");
        previewTheme.ShouldContain("DeleteOutlined");
    }

    [Fact]
    public void UploadPictureUploadingContent_Centers_Status_Content_As_A_Group()
    {
        var uploadingTheme = ReadRepoFile("src/AtomUI.Desktop.Controls/Upload/Themes/PictureShapeList/UploadPictureShapeUploadingContentTheme.axaml");

        uploadingTheme.ShouldContain("StackPanel Name=\"UploadingContentLayout\"");
        uploadingTheme.ShouldContain("VerticalAlignment=\"Center\"");
        uploadingTheme.ShouldNotContain("VerticalAlignment=\"Top\"");
        uploadingTheme.ShouldNotContain("Margin=\"0, 10, 0, 0\"");
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

        throw new FileNotFoundException($"Unable to locate repository file '{relativePath}'.");
    }
}
