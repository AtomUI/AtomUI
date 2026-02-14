namespace AtomUI.Desktop.Controls;

internal interface INavMenuInteractionHandler
{
    /// <summary>
    /// Attaches the interaction handler to a menu.
    /// </summary>
    /// <param name="menu">The menu.</param>
    void Attach(NavMenu menu);

    /// <summary>
    /// Detaches the interaction handler from the attached menu.
    /// </summary>
    void Detach(NavMenu menu);
    
    void Select(NavMenuItem menuItem);
}