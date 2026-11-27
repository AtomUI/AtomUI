namespace AtomUI.Desktop.Controls;

/// <summary>
/// The pseudo-classes that an <see cref="AbstractAutoComplete" /> sets on itself.
/// </summary>
public static class AutoCompletePseudoClass
{
    /// <summary>
    /// Set while the suggestion popup is open. Use the selector <c>:candidateopen</c> to style the open state.
    /// </summary>
    public const string CandidatePopupOpen = ":candidateopen";
}