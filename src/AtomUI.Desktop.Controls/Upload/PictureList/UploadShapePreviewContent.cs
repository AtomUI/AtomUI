using AtomUI.Desktop.Controls.Themes;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace AtomUI.Desktop.Controls;

internal class UploadShapePreviewContent : AbstractUploadPictureContent
{
    public static readonly StyledProperty<IList<string>?> SourcesProperty =
        AvaloniaProperty.Register<UploadShapePreviewContent, IList<string>?>(nameof(Sources));
    
    public IList<string>? Sources
    {
        get => GetValue(SourcesProperty);
        set => SetValue(SourcesProperty, value);
    }
    
    static UploadShapePreviewContent()
    {
        IconButton.ClickEvent.AddClassHandler<UploadShapePreviewContent>((o, args) => o.HandleActionButtonClicked((args.Source as IconButton)!));
    }
    
    private void HandleActionButtonClicked(IconButton button)
    {
        if (button.Tag is UploadListActions actionType)
        {
            if (actionType == UploadListActions.Preview)
            {
                if (_uploadImagePreviewer != null)
                {
                    _uploadImagePreviewer.OpenDialog();
                }
            }
        }
    }
    
    private UploadImagePreviewer? _uploadImagePreviewer;

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == FilePathProperty)
        {
            if (FilePath != null)
            {
                var sources = new List<string>();
                sources.Add(FilePath.ToString());
                SetCurrentValue(SourcesProperty, sources);
            }
            else
            {
                SetCurrentValue(SourcesProperty, null);
            }
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _uploadImagePreviewer = e.NameScope.Find<UploadImagePreviewer>(UploadThemeConstants.ImagePreviewerPart);
    }
}