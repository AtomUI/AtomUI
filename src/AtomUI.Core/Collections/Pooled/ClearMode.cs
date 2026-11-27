// This source file is adapted from the Collections.Pooled.
// (https://github.com/jtmueller/Collections.Pooled/tree/master/Collections.Pooled/)

namespace AtomUI.Collections.Pooled;

/// <summary>
/// Controls whether a <see cref="PooledList{T}" /> resets the slots it stops using, so that references stored in
/// them can be collected. The setting applies when elements are removed, when AddSpan and InsertSpan hand out new
/// slots, and when the internal array goes back to the <c>ArrayPool&lt;T&gt;</c>.
/// </summary>
    internal enum ClearMode
    {
        /// <summary>
        /// Clears when <c>T</c> is a reference type or a value type that contains references. Other value types are not
        /// cleared.
        /// </summary>
        /// <remarks>
        /// In builds without the <c>NETCOREAPP2_1_OR_GREATER</c> symbol the element type cannot be inspected, so every
        /// element type is cleared, as with <see cref="Always" />.
        /// </remarks>
        Auto = 0,
        /// <summary>
        /// Clears the slots whatever the element type is.
        /// </summary>
        Always = 1,
        /// <summary>
        /// Never clears the slots.
        /// </summary>
        /// <remarks>
        /// For a list of reference types, or of value types that contain references, the internal array keeps the
        /// discarded objects reachable, so the garbage collector cannot reclaim them.
        /// </remarks>
        Never = 2
    }