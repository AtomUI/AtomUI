// This source file is adapted from the Collections.Pooled.
// (https://github.com/jtmueller/Collections.Pooled/tree/master/Collections.Pooled/)

namespace AtomUI.Collections.Pooled;

/// <summary>
/// A read-only list that also exposes its items as a <see cref="Span" />.
/// </summary>
/// <typeparam name="T">
/// The type of elements in the read-only pooled list.
/// </typeparam>

internal interface IReadOnlyPooledList<T> : IReadOnlyList<T>
{
#pragma warning disable CS0419
    /// <summary>
    /// Gets a <see cref="System.ReadOnlySpan{T}" /> for the items currently in the collection.
    /// </summary>
#pragma warning restore CS0419
    ReadOnlySpan<T> Span { get; }
}