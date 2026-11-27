namespace AtomUI.Theme.Schema;

/// <summary>
/// The role of a token in the derivation: a global seed, Map or Alias token, a control token, or a derived language
/// token (a language token marked as a seed has <see cref="Seed" />).
/// </summary>
public enum TokenStage : byte
{
    /// <summary>
    /// A global seed token, or a language token marked as a seed: an input to derivation that no rule writes.
    /// </summary>
    Seed,
    /// <summary>
    /// A global token the design language derives from the seeds. A control token rule may override it for one control.
    /// </summary>
    Map,
    /// <summary>
    /// A global token derived for a purpose of use. A control token rule may override it for one control.
    /// </summary>
    Alias,
    /// <summary>
    /// A token of a control's own token class, which every design language computes in a control token rule.
    /// </summary>
    Control,
    /// <summary>
    /// A language token other than a seed.
    /// </summary>
    Language
}
