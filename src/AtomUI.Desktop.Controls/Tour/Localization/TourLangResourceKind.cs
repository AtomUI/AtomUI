using AtomUI.Localization;

namespace AtomUI.Desktop.Controls.Localization;

/// <summary>
/// Keys of the localized labels of the step navigation buttons of a tour: previous, next and finish.
/// </summary>
[LanguageCatalog]
public enum TourLangResourceKind
{
    /// <summary>
    /// Label of the button that moves to the previous step.
    /// </summary>
    Previous,
    /// <summary>
    /// Label of the button that moves to the next step.
    /// </summary>
    Next,
    /// <summary>
    /// Label of the button that finishes the tour.
    /// </summary>
    Finish
}
