using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace AtomUIGallery.ShowCases.FloatButton;

public partial class FloatButtonShowCase : GalleryReactiveUserControl<FloatButtonViewModel>
{
    public const string LanguageId = nameof(FloatButtonShowCase);

    public static IMultiValueConverter DescriptionOffset { get; } = new DescriptionOffsetConverter();

    public FloatButtonShowCase()
    {
        InitializeComponent();
        this.WhenActivated(disposables =>
        {
            if (DataContext is FloatButtonViewModel vm)
            {
                vm.IsOpened = true;
            }
        });
    }

    private sealed class DescriptionOffsetConverter : IMultiValueConverter
    {
        public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values is not [double viewportWidth, double viewportHeight, double scrollWidth, double buttonSize,
                              double bottomInset, double fontSize, Thickness padding] ||
                parameter is not string { Length: 2 } position ||
                viewportWidth <= 0 || viewportHeight <= 0 || scrollWidth <= 0 || buttonSize <= 0)
            {
                return AvaloniaProperty.UnsetValue;
            }

            var index = position[0] - '0';
            const int columns = 3;
            const double gap = 8;
            // Reserve the default maximum description width for each slot.
            var maxButtonWidth = buttonSize * 2.5;
            var slotWidth = maxButtonWidth + gap;
            var fitsInOneRow = viewportWidth >= columns * slotWidth + gap;
            if (position[1] == 'X')
            {
                var viewportCenterOffset = (viewportWidth - scrollWidth) / 2;
                var lastColumnCenter = viewportWidth / 2 - gap - maxButtonWidth / 2;
                return fitsInOneRow
                    ? viewportCenterOffset + lastColumnCenter - (columns - 1 - index) * slotWidth
                    : viewportCenterOffset;
            }

            var rowHeight = Math.Max(buttonSize, 2 * Math.Ceiling(fontSize * 1.15) + Math.Min(padding.Top + padding.Bottom, 24));
            var bottomRowCenter = (viewportHeight - rowHeight) / 2 - bottomInset;
            return fitsInOneRow ? bottomRowCenter : bottomRowCenter - (columns - 1 - index) * (rowHeight + gap);
        }
    }
}
