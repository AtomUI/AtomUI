using System.Runtime.CompilerServices;

namespace AtomUI.Controls.AsyncLoad;

/// <summary>
/// An equality comparer that treats two objects as equal only when they are the same instance, whatever the
/// equality members of their type say.
/// </summary>
/// <typeparam name="T">
/// The reference type of the objects to compare.
/// </typeparam>
public sealed class ReferenceEqualityComparer<T> : IEqualityComparer<T> where T : class
{
    /// <summary>
    /// The shared comparer for <typeparamref name="T" />. The comparer has no state, so a second instance is
    /// never needed.
    /// </summary>
    public static readonly ReferenceEqualityComparer<T> Instance = new();

    /// <summary>
    /// Determines whether <paramref name="x" /> and <paramref name="y" /> are the same instance.
    /// </summary>
    /// <param name="x">
    /// The first object to compare.
    /// </param>
    /// <param name="y">
    /// The second object to compare.
    /// </param>
    /// <returns>
    /// <see langword="true" /> when both refer to the same object or are both <see langword="null" />;
    /// otherwise <see langword="false" />.
    /// </returns>
    public bool Equals(T? x, T? y) => ReferenceEquals(x, y);

    /// <summary>
    /// Returns the identity hash code of <paramref name="obj" />, which is the same for objects that
    /// <c>Equals</c> considers equal.
    /// </summary>
    /// <param name="obj">
    /// The object to hash.
    /// </param>
    /// <returns>
    /// A hash code that does not depend on the content of the object.
    /// </returns>
    public int GetHashCode(T obj) => RuntimeHelpers.GetHashCode(obj);
}
