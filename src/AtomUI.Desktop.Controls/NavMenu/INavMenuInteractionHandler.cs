namespace AtomUI.Desktop.Controls;

internal interface INavMenuInteractionHandler
{
    /// <summary>
    /// Attaches the interaction handler to a menu.
    /// </summary>
    void Attach(NavMenu menu);

    /// <summary>
    /// Detaches the interaction handler from the attached menu.
    /// </summary>
    void Detach(NavMenu menu);
    
    /// <summary>
    /// Commits a user item activation: selection (leaf) or submenu activation
    /// (parent), followed by command execution and the item click event unless
    /// a synchronous selection change supersedes the leaf activation.
    /// </summary>
    void CommitItemActivation(NavMenuItem menuItem);

    void ClearSelection();

    void Forget(NavMenuItem menuItem);

    /// <summary>
    /// Sets whether the menu shows keyboard focus (CSS <c>:focus-visible</c>): the keyboard active item, or
    /// the menu root when no item is active, draws the focus outline.
    /// </summary>
    void SetKeyboardFocusVisible(bool isVisible);
}
