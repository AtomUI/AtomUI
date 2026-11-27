using System.ComponentModel;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// The standard buttons a dialog can offer. The values are flags: combine them to build a
/// <see cref="DialogStandardButtons" /> set. Each value stands for one button whose text the button box supplies
/// and whose <see cref="DialogButtonRole" /> is fixed by the value.
/// </summary>
[Flags]
public enum DialogStandardButton
{
    /// <summary>
    /// No standard button: the value of an empty <see cref="DialogStandardButtons" /> set.
    /// </summary>
    NoButton = 0x00000000,
    /// <summary>
    /// An "OK" button defined with the AcceptRole.
    /// </summary>
    Ok = 0x00000001,
    /// <summary>
    /// An "Open" button defined with the AcceptRole.
    /// </summary>
    Open = 0x00000002,
    /// <summary>
    /// A "Save" button defined with the AcceptRole.
    /// </summary>
    Save = 0x00000004,
    /// <summary>
    /// A "Cancel" button defined with the RejectRole.
    /// </summary>
    Cancel = 0x00000008,
    /// <summary>
    /// A "Close" button defined with the RejectRole.
    /// </summary>
    Close = 0x00000010,
    /// <summary>
    /// A "Discard" or "Don't Save" button, depending on the platform, defined with the DestructiveRole.
    /// </summary>
    Discard = 0x00000020,
    /// <summary>
    /// An "Apply" button defined with the ApplyRole.
    /// </summary>
    Apply = 0x00000040,
    /// <summary>
    /// A "Reset" button defined with the ResetRole.
    /// </summary>
    Reset = 0x00000080,
    /// <summary>
    /// A "Reload" button defined with the ActionRole.
    /// </summary>
    Reload = 0x00000100,
    /// <summary>
    /// A "Restore Defaults" button defined with the ResetRole.
    /// </summary>
    RestoreDefaults = 0x00000200,
    /// <summary>
    /// A "Help" button defined with the HelpRole.
    /// </summary>
    Help = 0x00000400,
    /// <summary>
    /// A "Save All" button defined with the AcceptRole.
    /// </summary>
    SaveAll = 0x00000800,
    /// <summary>
    /// A "Yes" button defined with the YesRole.
    /// </summary>
    Yes = 0x00001000,
    /// <summary>
    /// A "Yes to All" button defined with the YesRole.
    /// </summary>
    YesToAll = 0x00002000,
    /// <summary>
    /// A "No" button defined with the NoRole.
    /// </summary>
    No = 0x00004000,
    /// <summary>
    /// A "No to All" button defined with the NoRole.
    /// </summary>
    NoToAll = 0x00008000,
    /// <summary>
    /// An "Abort" button defined with the RejectRole.
    /// </summary>
    Abort = 0x00010000,
    /// <summary>
    /// A "Retry" button defined with the AcceptRole.
    /// </summary>
    Retry = 0x00020000,
    /// <summary>
    /// An "Ignore" button defined with the AcceptRole.
    /// </summary>
    Ignore = 0x00040000,
}

/// <summary>
/// An immutable set of standard buttons for a dialog button box, stored as a combination of
/// <see cref="DialogStandardButton" /> flags.
/// </summary>
public readonly struct DialogStandardButtons : IEquatable<DialogStandardButtons>
{
    private static readonly EnumConverter DialogStandardButtonConverter = new(typeof(DialogStandardButton));

    private const int ValidButtonMask =
        (int)(DialogStandardButton.Ok |
              DialogStandardButton.Open |
              DialogStandardButton.Save |
              DialogStandardButton.Cancel |
              DialogStandardButton.Close |
              DialogStandardButton.Discard |
              DialogStandardButton.Apply |
              DialogStandardButton.Reset |
              DialogStandardButton.Reload |
              DialogStandardButton.RestoreDefaults |
              DialogStandardButton.Help |
              DialogStandardButton.SaveAll |
              DialogStandardButton.Yes |
              DialogStandardButton.YesToAll |
              DialogStandardButton.No |
              DialogStandardButton.NoToAll |
              DialogStandardButton.Abort |
              DialogStandardButton.Retry |
              DialogStandardButton.Ignore);

    /// <summary>
    /// Gets the combined <see cref="DialogStandardButton" /> flags this set was created with.
    /// </summary>
    public DialogStandardButton ButtonFlags { get; }

    /// <summary>
    /// Initializes a set that contains the given standard buttons.
    /// </summary>
    /// <param name="buttonFlags">
    /// The buttons the set contains.
    /// </param>
    public DialogStandardButtons(DialogStandardButton buttonFlags)
    {
        ButtonFlags = buttonFlags;
    }
    
    /// <summary>
    /// Returns whether both sets contain the same buttons.
    /// </summary>
    public static bool operator ==(DialogStandardButtons left, DialogStandardButtons right) => left.Equals(right);
    /// <summary>
    /// Returns whether the two sets differ in the buttons they contain.
    /// </summary>
    public static bool operator !=(DialogStandardButtons left, DialogStandardButtons right) => !left.Equals(right);

    /// <summary>
    /// Returns whether every button in <paramref name="flag" /> is part of this set. For
    /// <see cref="DialogStandardButton.NoButton" /> the result is always <see langword="true" />, so test an
    /// empty set with <see cref="Count" /> instead.
    /// </summary>
    /// <param name="flag">
    /// One standard button, or a combination of them.
    /// </param>
    public bool HasFlag(DialogStandardButton flag)
    {
        return (ButtonFlags & flag) == flag;
    }
    
    /// <summary>
    /// Returns whether <paramref name="other" /> contains the same buttons as this set.
    /// </summary>
    /// <param name="other">
    /// The set to compare with this set.
    /// </param>
    public bool Equals(DialogStandardButtons other)
    {
        return ButtonFlags == other.ButtonFlags;
    }
    /// <summary>
    /// Returns whether <paramref name="obj" /> is a <see cref="DialogStandardButtons" /> with the same buttons.
    /// </summary>
    /// <param name="obj">
    /// The object to compare with this set.
    /// </param>
    public override bool Equals(object? obj) => obj is DialogStandardButtons other && Equals(other);
    /// <summary>
    /// Returns the hash code of <see cref="ButtonFlags" />; sets with the same buttons have the same hash code.
    /// </summary>
    public override int GetHashCode() => ButtonFlags.GetHashCode();
    /// <summary>
    /// Returns the flags as text, for example <c>Ok, Cancel</c>.
    /// </summary>
    public override string ToString() => ButtonFlags.ToString();
    
    /// <summary>
    /// Converts the invariant text form of standard buttons into a set.
    /// </summary>
    /// <param name="s">
    /// The text to convert, in a form the enum converter of <see cref="DialogStandardButton" /> accepts.
    /// </param>
    /// <returns>
    /// The set described by <paramref name="s" />.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the conversion yields no <see cref="DialogStandardButton" /> value.
    /// </exception>
    public static DialogStandardButtons Parse(string s)
    {
        var dialogButton = DialogStandardButtonConverter.ConvertFromInvariantString(s) as DialogStandardButton?;
        if (dialogButton == null)
        {
            throw new InvalidOperationException("Invalid DialogStandardButton format.");
        }
        return new DialogStandardButtons(dialogButton.Value);
    }
    
    /// <summary>
    /// Allows a single standard button to be used where a set is expected.
    /// </summary>
    public static implicit operator DialogStandardButtons(DialogStandardButton flags) => new DialogStandardButtons(flags);
        
    /// <summary>
    /// Gets the number of standard buttons in this set. Bits that stand for no defined standard button are
    /// ignored.
    /// </summary>
    public int Count
    {
        get
        {
            var flags = (int)ButtonFlags & ValidButtonMask;
            var count = 0;
            while (flags != 0)
            {
                flags &= flags - 1;
                count++;
            }
            return count;
        }
    }
}
