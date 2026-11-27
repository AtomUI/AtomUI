using Avalonia;

namespace AtomUI.Utils;

/// <summary>
/// Extension methods and factories for scaling, rotating and translating 2D affine matrices.
/// </summary>
public static class MatrixUtils
{
    /// <summary>
    /// Multiplies <paramref name="matrix" /> by a scale about the point
    /// (<paramref name="centerX" />, <paramref name="centerY" />).
    /// </summary>
    /// <param name="matrix">
    /// The matrix to scale. It is replaced by the product.
    /// </param>
    /// <param name="scaleX">
    /// The horizontal scale factor.
    /// </param>
    /// <param name="scaleY">
    /// The vertical scale factor.
    /// </param>
    /// <param name="centerX">
    /// The x coordinate of the point that stays fixed.
    /// </param>
    /// <param name="centerY">
    /// The y coordinate of the point that stays fixed.
    /// </param>
    public static void ScaleAt(ref this Matrix matrix, double scaleX, double scaleY, double centerX, double centerY)
    {
        matrix *= CreateScaling(scaleX, scaleY, centerX, centerY);
    }

    /// <summary>
    /// Adds the offset to the translation part of <paramref name="matrix" />. The linear part of the matrix is left
    /// unchanged, so the offset is not scaled or rotated by it.
    /// </summary>
    /// <param name="matrix">
    /// The matrix to translate. It is replaced by the result.
    /// </param>
    /// <param name="offsetX">
    /// The amount to add to the x translation.
    /// </param>
    /// <param name="offsetY">
    /// The amount to add to the y translation.
    /// </param>
    public static void Translate(ref this Matrix matrix, double offsetX, double offsetY)
    {
        matrix = new Matrix(matrix.M11, matrix.M12, matrix.M21, matrix.M22, matrix.M31 + offsetX, matrix.M32 + offsetY);
    }

    /// <summary>
    /// Multiplies <paramref name="matrix" /> by a rotation of <paramref name="angle" /> about the point
    /// (<paramref name="centerX" />, <paramref name="centerY" />).
    /// </summary>
    /// <param name="matrix">
    /// The matrix to rotate. It is replaced by the product.
    /// </param>
    /// <param name="angle">
    /// The rotation angle in radians.
    /// </param>
    /// <param name="centerX">
    /// The x coordinate of the point that stays fixed.
    /// </param>
    /// <param name="centerY">
    /// The y coordinate of the point that stays fixed.
    /// </param>
    public static void RotateAt(ref this Matrix matrix, double angle, double centerX, double centerY)
    {
        matrix *= CreateRotationRadians(angle, centerX, centerY);
    }

    /// <summary>
    /// Creates a rotation by <paramref name="angle" /> about the point
    /// (<paramref name="centerX" />, <paramref name="centerY" />).
    /// </summary>
    /// <param name="angle">
    /// The rotation angle in radians.
    /// </param>
    /// <param name="centerX">
    /// The x coordinate of the point that stays fixed.
    /// </param>
    /// <param name="centerY">
    /// The y coordinate of the point that stays fixed.
    /// </param>
    /// <returns>
    /// The rotation matrix.
    /// </returns>
    public static Matrix CreateRotationRadians(double angle, double centerX, double centerY)
    {
        var m12     = Math.Sin(angle);
        var num     = Math.Cos(angle);
        var offsetX = centerX * (1.0 - num) + centerY * m12;
        var offsetY = centerY * (1.0 - num) - centerX * m12;

        return new Matrix(num, m12, -m12, num, offsetX, offsetY);
    }

    private static Matrix CreateScaling(double scaleX, double scaleY, double centerX, double centerY)
    {
        return new Matrix(scaleX, 0.0, 0.0, scaleY, centerX - scaleX * centerX, centerY - scaleY * centerY);
    }

    private static Matrix CreateScaling(double scaleX, double scaleY)
    {
        return new Matrix(scaleX, 0.0, 0.0, scaleY, 0.0, 0.0);
    }

    private static Matrix CreateSkewRadians(double skewX, double skewY)
    {
        return new Matrix(1.0, Math.Tan(skewY), Math.Tan(skewX), 1.0, 0.0, 0.0);
    }

    private static Matrix CreateTranslation(double offsetX, double offsetY)
    {
        return new Matrix(1.0, 0.0, 0.0, 1.0, offsetX, offsetY);
    }

    private static Matrix CreateIdentity()
    {
        return new Matrix(1.0, 0.0, 0.0, 1.0, 0.0, 0.0);
    }
}