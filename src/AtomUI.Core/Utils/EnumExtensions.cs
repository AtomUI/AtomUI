using System.Runtime.CompilerServices;

namespace AtomUI.Utils;

/// <summary>
/// Provides bit tests for enum values: whether a value has all the bits of a given set, or shares at
/// least one bit with it.
/// </summary>
public static class EnumExtensions
{
    /// <summary>
    /// Returns whether <paramref name="value" /> has all the bits that are set in <paramref name="flags" />.
    /// </summary>
    /// <typeparam name="T">
    /// An enum type whose underlying type is 1, 2, 4 or 8 bytes wide.
    /// </typeparam>
    /// <param name="value">
    /// The value to test.
    /// </param>
    /// <param name="flags">
    /// The bits that must all be set.
    /// </param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe bool HasAllFlags<T>(this T value, T flags) where T : unmanaged, Enum
    {
        if (sizeof(T) == 1)
        {
            var byteValue = Unsafe.As<T, byte>(ref value);
            var byteFlags = Unsafe.As<T, byte>(ref flags);
            return (byteValue & byteFlags) == byteFlags;
        }

        if (sizeof(T) == 2)
        {
            var shortValue = Unsafe.As<T, short>(ref value);
            var shortFlags = Unsafe.As<T, short>(ref flags);
            return (shortValue & shortFlags) == shortFlags;
        }

        if (sizeof(T) == 4)
        {
            var intValue = Unsafe.As<T, int>(ref value);
            var intFlags = Unsafe.As<T, int>(ref flags);
            return (intValue & intFlags) == intFlags;
        }

        if (sizeof(T) == 8)
        {
            var longValue = Unsafe.As<T, long>(ref value);
            var longFlags = Unsafe.As<T, long>(ref flags);
            return (longValue & longFlags) == longFlags;
        }

        throw new NotSupportedException("Enum with size of " + Unsafe.SizeOf<T>() + " are not supported");
    }

    /// <summary>
    /// Returns whether <paramref name="value" /> and <paramref name="flags" /> share at least one set bit.
    /// </summary>
    /// <typeparam name="T">
    /// An enum type whose underlying type is 1, 2, 4 or 8 bytes wide.
    /// </typeparam>
    /// <param name="value">
    /// The value to test.
    /// </param>
    /// <param name="flags">
    /// The bits to look for. Any one of them is enough.
    /// </param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe bool HasAnyFlag<T>(this T value, T flags) where T : unmanaged, Enum
    {
        if (sizeof(T) == 1)
        {
            var byteValue = Unsafe.As<T, byte>(ref value);
            var byteFlags = Unsafe.As<T, byte>(ref flags);
            return (byteValue & byteFlags) != 0;
        }

        if (sizeof(T) == 2)
        {
            var shortValue = Unsafe.As<T, short>(ref value);
            var shortFlags = Unsafe.As<T, short>(ref flags);
            return (shortValue & shortFlags) != 0;
        }

        if (sizeof(T) == 4)
        {
            var intValue = Unsafe.As<T, int>(ref value);
            var intFlags = Unsafe.As<T, int>(ref flags);
            return (intValue & intFlags) != 0;
        }

        if (sizeof(T) == 8)
        {
            var longValue = Unsafe.As<T, long>(ref value);
            var longFlags = Unsafe.As<T, long>(ref flags);
            return (longValue & longFlags) != 0;
        }

        throw new NotSupportedException("Enum with size of " + Unsafe.SizeOf<T>() + " are not supported");
    }
}