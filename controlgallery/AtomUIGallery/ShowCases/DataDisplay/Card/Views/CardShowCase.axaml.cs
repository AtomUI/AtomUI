using AtomUI;

namespace AtomUIGallery.ShowCases.Card;

public partial class CardShowCase : GalleryReactiveUserControl<CardViewModel>
{
    public const string LanguageId = nameof(CardShowCase);

    public CardShowCase()
    {
        InitializeComponent();
    }
}
