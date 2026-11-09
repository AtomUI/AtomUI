using AtomUI.Theme;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

// A Token-owner transparent page container: a CarouselPage has no Own Tokens and no Token reads, so the content it hosts keeps
// the Carousel as Token owner.
[TokenOwnerTransparent]
public class CarouselPage : ContentControl
{
}
