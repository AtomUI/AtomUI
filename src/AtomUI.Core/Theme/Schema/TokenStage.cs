namespace AtomUI.Theme.Schema;

/// <summary>
/// The role of a token in the derivation: a global seed, Map or Alias token, a control token, or a derived language
/// token (a language token marked as a seed has <see cref="Seed"/>).
/// </summary>
public enum TokenStage : byte
{
    Seed,
    Map,
    Alias,
    Control,
    Language
}
