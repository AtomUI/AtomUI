using Avalonia;

namespace AtomUI.Utils;

/// <summary>
/// Floating point helpers: comparisons with a tolerance, rounding, and conversions between angle units.
/// </summary>
public static class MathUtils
{
    // smallest such that 1.0+DoubleEpsilon != 1.0
    internal const double DoubleEpsilon = 2.2204460492503131e-016;

    private const float FloatEpsilon = 1.192092896e-07F;

    /// <summary>
    /// Rounds <paramref name="value" /> to the given number of decimal places.
    /// </summary>
    /// <param name="value">
    /// The value to round.
    /// </param>
    /// <param name="fixedCount">
    /// The number of decimal places to keep.
    /// </param>
    /// <returns>
    /// The rounded value.
    /// </returns>
    /// <remarks>
    /// A midpoint is rounded to the even neighbor. As the value is scaled in binary floating point, a decimal that
    /// appears to be an exact midpoint, such as 2.675 for two places, may round either way.
    /// </remarks>
    public static double RoundToFixedPoint(double value, int fixedCount)
    {
        var factor = Math.Pow(10, fixedCount);
        return Math.Round(value * factor) / factor;
    }

    /// <summary>
    /// AreClose - Returns whether or not two doubles are "close".  That is, whether or
    /// not they are within epsilon of each other. The epsilon grows with the magnitudes of both values:
    /// it is the sum of their absolute values plus 10, multiplied by <see cref="DoubleEpsilon" />.
    /// </summary>
    /// <param name="value1">
    /// The first double to compare.
    /// </param>
    /// <param name="value2">
    /// The second double to compare.
    /// </param>
    public static bool AreClose(double value1, double value2)
    {
        //in case they are Infinities (then epsilon check does not work)
        if (value1 == value2)
        {
            return true;
        }

        var eps   = (Math.Abs(value1) + Math.Abs(value2) + 10.0) * DoubleEpsilon;
        var delta = value1 - value2;
        return -eps < delta && eps > delta;
    }

    /// <summary>
    /// AreClose - Returns whether or not two doubles are "close".  That is, whether or
    /// not they are within epsilon of each other.
    /// </summary>
    /// <param name="value1">
    /// The first double to compare.
    /// </param>
    /// <param name="value2">
    /// The second double to compare.
    /// </param>
    /// <param name="eps">
    /// The fixed epsilon value used to compare.
    /// </param>
    public static bool AreClose(double value1, double value2, double eps)
    {
        //in case they are Infinities (then epsilon check does not work)
        if (value1 == value2)
        {
            return true;
        }

        var delta = value1 - value2;
        return -eps < delta && eps > delta;
    }

    /// <summary>
    /// AreClose - Returns whether or not two floats are "close".  That is, whether or
    /// not they are within epsilon of each other. The epsilon grows with the magnitudes of both values:
    /// it is the sum of their absolute values plus 10, multiplied by <see cref="FloatEpsilon" />.
    /// </summary>
    /// <param name="value1">
    /// The first float to compare.
    /// </param>
    /// <param name="value2">
    /// The second float to compare.
    /// </param>
    public static bool AreClose(float value1, float value2)
    {
        //in case they are Infinities (then epsilon check does not work)
        if (value1 == value2)
        {
            return true;
        }

        var eps   = (Math.Abs(value1) + Math.Abs(value2) + 10.0f) * FloatEpsilon;
        var delta = value1 - value2;
        return -eps < delta && eps > delta;
    }

    /// <summary>
    /// LessThan - Returns whether or not the first double is less than the second double.
    /// That is, whether or not the first is strictly less than *and* not within epsilon of
    /// the other number.
    /// </summary>
    /// <param name="value1">
    /// The first double to compare.
    /// </param>
    /// <param name="value2">
    /// The second double to compare.
    /// </param>
    public static bool LessThan(double value1, double value2)
    {
        return value1 < value2 && !AreClose(value1, value2);
    }

    /// <summary>
    /// LessThan - Returns whether or not the first float is less than the second float.
    /// That is, whether or not the first is strictly less than *and* not within epsilon of
    /// the other number.
    /// </summary>
    /// <param name="value1">
    /// The first single float to compare.
    /// </param>
    /// <param name="value2">
    /// The second single float to compare.
    /// </param>
    public static bool LessThan(float value1, float value2)
    {
        return value1 < value2 && !AreClose(value1, value2);
    }

    /// <summary>
    /// GreaterThan - Returns whether or not the first double is greater than the second double.
    /// That is, whether or not the first is strictly greater than *and* not within epsilon of
    /// the other number.
    /// </summary>
    /// <param name="value1">
    /// The first double to compare.
    /// </param>
    /// <param name="value2">
    /// The second double to compare.
    /// </param>
    public static bool GreaterThan(double value1, double value2)
    {
        return value1 > value2 && !AreClose(value1, value2);
    }

    /// <summary>
    /// GreaterThan - Returns whether or not the first float is greater than the second float.
    /// That is, whether or not the first is strictly greater than *and* not within epsilon of
    /// the other number.
    /// </summary>
    /// <param name="value1">
    /// The first float to compare.
    /// </param>
    /// <param name="value2">
    /// The second float to compare.
    /// </param>
    public static bool GreaterThan(float value1, float value2)
    {
        return value1 > value2 && !AreClose(value1, value2);
    }

    /// <summary>
    /// LessThanOrClose - Returns whether or not the first double is less than or close to
    /// the second double.  That is, whether or not the first is strictly less than or within
    /// epsilon of the other number.
    /// </summary>
    /// <param name="value1">
    /// The first double to compare.
    /// </param>
    /// <param name="value2">
    /// The second double to compare.
    /// </param>
    public static bool LessThanOrClose(double value1, double value2)
    {
        return value1 < value2 || AreClose(value1, value2);
    }

    /// <summary>
    /// LessThanOrClose - Returns whether or not the first float is less than or close to
    /// the second float.  That is, whether or not the first is strictly less than or within
    /// epsilon of the other number.
    /// </summary>
    /// <param name="value1">
    /// The first float to compare.
    /// </param>
    /// <param name="value2">
    /// The second float to compare.
    /// </param>
    public static bool LessThanOrClose(float value1, float value2)
    {
        return value1 < value2 || AreClose(value1, value2);
    }

    /// <summary>
    /// GreaterThanOrClose - Returns whether or not the first double is greater than or close to
    /// the second double.  That is, whether or not the first is strictly greater than or within
    /// epsilon of the other number.
    /// </summary>
    /// <param name="value1">
    /// The first double to compare.
    /// </param>
    /// <param name="value2">
    /// The second double to compare.
    /// </param>
    public static bool GreaterThanOrClose(double value1, double value2)
    {
        return value1 > value2 || AreClose(value1, value2);
    }

    /// <summary>
    /// GreaterThanOrClose - Returns whether or not the first float is greater than or close to
    /// the second float.  That is, whether or not the first is strictly greater than or within
    /// epsilon of the other number.
    /// </summary>
    /// <param name="value1">
    /// The first float to compare.
    /// </param>
    /// <param name="value2">
    /// The second float to compare.
    /// </param>
    public static bool GreaterThanOrClose(float value1, float value2)
    {
        return value1 > value2 || AreClose(value1, value2);
    }

    /// <summary>
    /// IsOne - Returns whether or not the double is "close" to 1, that is, within
    /// <c>10 * DoubleEpsilon</c> of it. Unlike <see cref="AreClose(double, double)" />, this test is faster
    /// and uses an epsilon that does not grow with the magnitude of the value.
    /// </summary>
    /// <param name="value">
    /// The double to compare to 1.
    /// </param>
    public static bool IsOne(double value)
    {
        return Math.Abs(value - 1.0) < 10.0 * DoubleEpsilon;
    }

    /// <summary>
    /// IsOne - Returns whether or not the float is "close" to 1, that is, within
    /// <c>10 * FloatEpsilon</c> of it. Unlike <see cref="AreClose(float, float)" />, this test is faster
    /// and uses an epsilon that does not grow with the magnitude of the value.
    /// </summary>
    /// <param name="value">
    /// The float to compare to 1.
    /// </param>
    public static bool IsOne(float value)
    {
        return Math.Abs(value - 1.0f) < 10.0f * FloatEpsilon;
    }

    /// <summary>
    /// IsZero - Returns whether or not the double is "close" to 0, that is, within
    /// <c>10 * DoubleEpsilon</c> of it. Unlike <see cref="AreClose(double, double)" />, this test is faster
    /// and uses an epsilon that does not grow with the magnitude of the value.
    /// </summary>
    /// <param name="value">
    /// The double to compare to 0.
    /// </param>
    public static bool IsZero(double value)
    {
        return Math.Abs(value) < 10.0 * DoubleEpsilon;
    }

    /// <summary>
    /// IsZero - Returns whether or not the float is "close" to 0, that is, within
    /// <c>10 * FloatEpsilon</c> of it. Unlike <see cref="AreClose(float, float)" />, this test is faster
    /// and uses an epsilon that does not grow with the magnitude of the value.
    /// </summary>
    /// <param name="value">
    /// The float to compare to 0.
    /// </param>
    public static bool IsZero(float value)
    {
        return Math.Abs(value) < 10.0f * FloatEpsilon;
    }

    /// <summary>
    /// Converts an angle in degrees to radians.
    /// </summary>
    /// <param name="angle">
    /// The angle in degrees.
    /// </param>
    /// <returns>
    /// The angle in radians.
    /// </returns>
    public static double Deg2Rad(double angle)
    {
        return angle * (Math.PI / 180d);
    }

    /// <summary>
    /// Converts an angle in gradians to radians.
    /// </summary>
    /// <param name="angle">
    /// The angle in gradians.
    /// </param>
    /// <returns>
    /// The angle in radians.
    /// </returns>
    public static double Grad2Rad(double angle)
    {
        return angle * (Math.PI / 200d);
    }

    /// <summary>
    /// Converts an angle in turns to radians.
    /// </summary>
    /// <param name="angle">
    /// The angle in turns.
    /// </param>
    /// <returns>
    /// The angle in radians.
    /// </returns>
    public static double Turn2Rad(double angle)
    {
        return angle * 2 * Math.PI;
    }

    /// <summary>
    /// Calculates the point of an angle on an ellipse.
    /// </summary>
    /// <param name="centre">
    /// The center point of the ellipse.
    /// </param>
    /// <param name="radiusX">
    /// The x radius of the ellipse.
    /// </param>
    /// <param name="radiusY">
    /// The y radius of the ellipse.
    /// </param>
    /// <param name="angle">
    /// The angle in radians.
    /// </param>
    /// <returns>
    /// A point on the ellipse.
    /// </returns>
    public static Point GetEllipsePoint(Point centre, double radiusX, double radiusY, double angle)
    {
        return new Point(radiusX * Math.Cos(angle) + centre.X, radiusY * Math.Sin(angle) + centre.Y);
    }
}