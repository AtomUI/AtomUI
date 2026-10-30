namespace AtomUI.Desktop.Controls;

/// <summary>
/// Immutable date-range input. Partial and reverse inputs remain intact;
/// selection owners decide when to commit and order the endpoints.
/// </summary>
public sealed record DateViewerRange(DateTime? Start, DateTime? End);
