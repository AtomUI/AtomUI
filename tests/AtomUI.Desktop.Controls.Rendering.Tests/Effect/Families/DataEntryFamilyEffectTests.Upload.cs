using System.Collections.ObjectModel;
using AtomUI.Controls;
using AtomUI.Desktop.Controls.DesignTokens;

using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;
using Xunit;

namespace AtomUI.Desktop.Controls.Rendering.Tests.Effect.Families;

/// <summary>
/// Upload: the text, picture and picture-card lists with files in every status, the picture-card trigger and the drag area.
/// UploadTrigger, UploadDefaultDropArea and UploadDropZone are Token-owner transparent and read the Upload's Tokens, so
/// their rows run against the Upload.
/// </summary>
public partial class DataEntryFamilyEffectTests
{
    internal static Upload CreateUpload(
        UploadListType listType = UploadListType.Text,
        bool dragger = false,
        bool disabled = false,
        params FileUploadStatus[] statuses)
    {
        var files = new ObservableCollection<UploadFileItem>();
        var index = 0;
        foreach (var status in statuses.Length == 0 ? new[] { FileUploadStatus.Success } : statuses)
        {
            files.Add(new UploadFileItem
            {
                Name         = $"file-{index++}-{status}.txt",
                Status       = status,
                Progress     = status == FileUploadStatus.Uploading ? 40 : 0,
                ErrorMessage = status == FileUploadStatus.Failed ? "Upload failed" : null
            });
        }

        object trigger = dragger
            ? new UploadDropZone { Content = new UploadDefaultDropArea { SubHeader = "Single or bulk upload" } }
            : new UploadTrigger { Content = listType is UploadListType.PictureCard or UploadListType.PictureCircle ? "+ Upload" : "Click to upload" };
        return new Upload
        {
            Width          = 520,
            AutoUpload     = false,
            ListType       = listType,
            IsEnabled      = !disabled,
            TriggerContent = trigger,
            Files          = files
        };
    }

    private static Control UploadItemAt(Control upload, int index) =>
        upload.GetVisualDescendants().OfType<AbstractUploadListItem>().ElementAt(index);

    private static UploadTextListItemHeader TextHeaderAt(Control upload, int index) =>
        upload.GetVisualDescendants().OfType<UploadTextListItemHeader>().ElementAt(index);

    private static Control PictureContentAt(Control upload, int index) =>
        upload.GetVisualDescendants().OfType<AbstractUploadPictureContent>().ElementAt(index);

    private static Control DropArea(Control upload) => Descendant<UploadDefaultDropArea>(upload);

    private static Control CardTrigger(Control upload) => Descendant<UploadTrigger>(upload);

    public static IEnumerable<object[]> UploadRows() =>
        HostRows("Upload", "UploadTrigger", "UploadDefaultDropArea", "UploadDropZone");

    [Theory]
    [MemberData(nameof(UploadRows))]
    public void Upload_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        Run(UploadScenario(token), UploadTokens.Identity, token);
    }

    private static Scenario UploadScenario(string token)
    {
        switch (token)
        {
            // Text list: a hovered item shows its delete action (ActionsColor) and hover background.
            case "ActionsColor" or "ControlItemBgHover" or "IconSize":
                return new Scenario(static () => CreateUpload(), static upload => Pseudo(TextHeaderAt(upload, 0), ":pointerover"),
                    new EffectOptions { Region = static upload => TextHeaderAt(upload, 0) });
            // Text list item: icon, file name, background, radius, line height and paddings.
            case "ColorIcon":
                return new Scenario(static () => CreateUpload(), null,
                    new EffectOptions { Region = static upload => Descendant<Control>(TextHeaderAt(upload, 0), "NormalIcon") });
            case "ColorLink" or "ColorFillSecondary" or "RelativeLineHeight" or "TextListNamePadding" or
                "TextListItemPadding" or "UniformlyPaddingXXS":
                return new Scenario(static () => CreateUpload(), null,
                    new EffectOptions { Region = static upload => TextHeaderAt(upload, 0) });
            case "BorderRadiusSM":
                return new Scenario(static () => CreateUpload(), null, new EffectOptions
                {
                    Region  = static upload => TextHeaderAt(upload, 0),
                    Perturb = static _ => new CornerRadius(12)
                });
            // A failed text item: name, icon and delete action in the error colour.
            case "ColorError":
                return new Scenario(static () => CreateUpload(statuses: [FileUploadStatus.Failed]), null,
                    new EffectOptions { Region = static upload => TextHeaderAt(upload, 0) });
            // An uploading text item: the progress line, its padding and the gap above it.
            case "TextListProgressLineHeight" or "TextListProgressPadding" or "FontSize":
                return new Scenario(static () => CreateUpload(statuses: [FileUploadStatus.Uploading]), null,
                    new EffectOptions { Region = static upload => Descendant<Border>(UploadItemAt(upload, 0), "ProgressBarFrame") });
            case "SpacingXXS":
                return new Scenario(static () => CreateUpload(statuses: [FileUploadStatus.Uploading]), null,
                    new EffectOptions { Region = static upload => UploadItemAt(upload, 0) });
            // The gap between the trigger and the list, and between text items.
            case "SpacingXS" or "TextListItemMargin" or "UniformlyMarginXS":
                return new Scenario(static () => CreateUpload(statuses: [FileUploadStatus.Success, FileUploadStatus.Success]));
            // Picture list: frame, padding, thumbnails, item gap and the delete action.
            case "PaddingXXS":
                return new Scenario(static () => CreateUpload(UploadListType.Picture), null,
                    new EffectOptions { Region = static upload => Descendant<IconButton>(PictureContentAt(upload, 0)) });
            case "ColorBorder" or "BorderThickness" or "PaddingXS" or "ColorTextSecondary" or
                "PictureListPreviewerSize" or "SizeXXL" or "UploadThumbnailSize" or "FontSizeHeading2" or
                "PictureListItemMargin":
                return new Scenario(static () => CreateUpload(UploadListType.Picture,
                    statuses: [FileUploadStatus.Success, FileUploadStatus.Pending]));
            case "BorderRadiusLG":
                return new Scenario(static () => CreateUpload(UploadListType.Picture), null, new EffectOptions
                {
                    Region  = static upload => PictureContentAt(upload, 0),
                    Perturb = static _ => new CornerRadius(24)
                });
            case "ColorBgTextHover":
                return new Scenario(static () => CreateUpload(UploadListType.Picture),
                    static upload => Pseudo(Descendant<IconButton>(PictureContentAt(upload, 0)), ":pointerover"),
                    new EffectOptions { Region = static upload => Descendant<IconButton>(PictureContentAt(upload, 0)) });
            // Picture card: card size, trigger fill and the mask over a hovered failed card.
            case "PictureCardSize" or "ControlHeightLG" or "ColorFillAlter":
                return new Scenario(static () => CreateUpload(UploadListType.PictureCard));
            case "ColorBgMask" or "ColorTextLightSolid" or "FontSizeLG":
                return new Scenario(static () => CreateUpload(UploadListType.PictureCard, statuses: [FileUploadStatus.Failed]),
                    static upload => Pseudo(PictureContentAt(upload, 0), ":pointerover"),
                    new EffectOptions { Region = static upload => PictureContentAt(upload, 0) });
            case "ColorPrimary":
                return new Scenario(static () => CreateUpload(UploadListType.PictureCard),
                    static upload => Pseudo(CardTrigger(upload), ":pointerover"),
                    new EffectOptions { Region = static upload => CardTrigger(upload) });
            // The drag area: frame, icon, header and description.
            case "DragIconSize" or "FontSizeHeading3" or "DragIconMargin" or "UniformlyMargin" or "DragHeaderMargin" or
                "UniformlyMarginXXS" or "ColorTextHeading" or "ColorTextDescription" or "Padding":
                return new Scenario(static () => CreateUpload(dragger: true), null,
                    new EffectOptions { Region = static upload => DropArea(upload) });
            case "ColorPrimaryHover":
                return new Scenario(static () => CreateUpload(dragger: true), static upload => Pseudo(DropArea(upload), ":pointerover"),
                    new EffectOptions { Region = static upload => DropArea(upload) });
            case "ColorTextDisabled":
                return new Scenario(static () => CreateUpload(dragger: true, disabled: true), null,
                    new EffectOptions { Region = static upload => DropArea(upload) });
            default:
                // EnableMotion (read back), MotionDuration* (read back).
                return new Scenario(static () => CreateUpload());
        }
    }
}
