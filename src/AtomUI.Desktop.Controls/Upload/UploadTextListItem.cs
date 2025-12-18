namespace AtomUI.Desktop.Controls;

internal class UploadTextListItem : AbstractUploadListItem
{
    static UploadTextListItem()
    {
        IconButton.ClickEvent.AddClassHandler<UploadTextListItem>((o, args) => o.HandleActionButtonClicked((args.Source as IconButton)!));
    }

    private void HandleActionButtonClicked(IconButton button)
    {
        if (button.Tag is UploadListActions actionType)
        {
            if (actionType == UploadListActions.Remove)
            {
                RaiseTaskRemoveRequestEvent();
            }
        }
    }
}