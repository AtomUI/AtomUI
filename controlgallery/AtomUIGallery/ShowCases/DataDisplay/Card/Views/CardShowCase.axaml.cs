using System.Reactive.Disposables;
using AtomUI;
using AtomUI.Theme;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Media;

namespace AtomUIGallery.ShowCases.Card;

public partial class CardShowCase : GalleryReactiveUserControl<CardViewModel>
{
    public const string LanguageId = nameof(CardShowCase);

    public CardShowCase()
    {
        InitializeComponent();

        this.WhenActivated(disposables =>
        {
            var application = Application.Current;
            if (application != null)
            {
                application.ActualThemeVariantChanged += HandleActualThemeVariantChanged;
                disposables.Add(Disposable.Create(() => application.ActualThemeVariantChanged -= HandleActualThemeVariantChanged ));
            }
        });
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        ConfigureBorderlessBgFrame();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        ConfigureBorderlessBgFrame();
    }

    private void HandleActualThemeVariantChanged(object? sender, EventArgs e)
    {
        ConfigureBorderlessBgFrame();
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        ConfigureBorderlessBgFrame();
    }

    private void ConfigureBorderlessBgFrame()
    {
        var application = Application.Current;
        if (application != null)
        {
            if (DataContext is CardViewModel cardViewModel)
            {
                if (application.GetThemeManager()?.CurrentTheme?.Appearance == ThemeAppearance.Dark)
                {
                    cardViewModel.BorderlessFrameBg = new SolidColorBrush(Color.FromRgb(48, 48, 48));
                }
                else
                {
                    cardViewModel.BorderlessFrameBg = new SolidColorBrush(Color.FromRgb(240, 242, 245));
                }
            }
        }
    }
}
