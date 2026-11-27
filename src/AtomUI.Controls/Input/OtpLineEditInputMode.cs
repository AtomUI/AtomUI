namespace AtomUI.Desktop.Controls;

/// <summary>
/// Restricts the characters that a one-time password input keeps.
/// </summary>
public enum OtpLineEditInputMode
{
    /// <summary>
    /// Every character is kept.
    /// </summary>
    Text,
    /// <summary>
    /// Only digits are kept.
    /// </summary>
    Numeric,
    /// <summary>
    /// Only letters and digits are kept.
    /// </summary>
    AlphaNumeric
}
