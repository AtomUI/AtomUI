/*
The MIT License (MIT)

Copyright (c) AvaloniaUI OÜ
All Rights Reserved

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System;

namespace AtomUI.Generator;

// Converts HSL/HSV literal components to ARGB while generating C# constants.
internal static class ThemeLiteralColorConversion
{
    private static uint FromArgb(byte a, byte r, byte g, byte b) =>
        ((uint)a << 24) | ((uint)r << 16) | ((uint)g << 8) | b;

    internal static uint HsvToArgb(
        double hue,
        double saturation,
        double value,
        double alpha = 1.0)
    {
        while (hue >= 360.0)
        {
            hue -= 360.0;
        }

        while (hue < 0.0)
        {
            hue += 360.0;
        }

        saturation = saturation < 0.0 ? 0.0 : saturation;
        saturation = saturation > 1.0 ? 1.0 : saturation;

        value = value < 0.0 ? 0.0 : value;
        value = value > 1.0 ? 1.0 : value;

        alpha = alpha < 0.0 ? 0.0 : alpha;
        alpha = alpha > 1.0 ? 1.0 : alpha;

        var chroma = saturation * value;
        var min = value - chroma;

        // Clamped saturation and value make chroma nonnegative. Preserve tiny positive
        // chroma: rounding to ARGB bytes can distinguish it from an achromatic color.
        if (chroma <= 0)
        {
            return FromArgb(
                (byte)Math.Round(alpha * 255),
                (byte)Math.Round(min * 255),
                (byte)Math.Round(min * 255),
                (byte)Math.Round(min * 255));
        }

        int sextant = (int)(hue / 60);
        double intermediateColorPercentage = (hue / 60) - sextant;
        double max = chroma + min;

        double r = 0;
        double g = 0;
        double b = 0;

        switch (sextant)
        {
            case 0:
                r = max;
                g = min + (chroma * intermediateColorPercentage);
                b = min;
                break;
            case 1:
                r = min + (chroma * (1 - intermediateColorPercentage));
                g = max;
                b = min;
                break;
            case 2:
                r = min;
                g = max;
                b = min + (chroma * intermediateColorPercentage);
                break;
            case 3:
                r = min;
                g = min + (chroma * (1 - intermediateColorPercentage));
                b = max;
                break;
            case 4:
                r = min + (chroma * intermediateColorPercentage);
                g = min;
                b = max;
                break;
            case 5:
                r = max;
                g = min;
                b = min + (chroma * (1 - intermediateColorPercentage));
                break;
        }

        return FromArgb(
            (byte)Math.Round(alpha * 255),
            (byte)Math.Round(r * 255),
            (byte)Math.Round(g * 255),
            (byte)Math.Round(b * 255));
    }

    internal static uint HslToArgb(
        double hue,
        double saturation,
        double lightness,
        double alpha = 1.0)
    {
        while (hue >= 360.0)
        {
            hue -= 360.0;
        }

        while (hue < 0.0)
        {
            hue += 360.0;
        }

        saturation = saturation < 0.0 ? 0.0 : saturation;
        saturation = saturation > 1.0 ? 1.0 : saturation;

        lightness = lightness < 0.0 ? 0.0 : lightness;
        lightness = lightness > 1.0 ? 1.0 : lightness;

        alpha = alpha < 0.0 ? 0.0 : alpha;
        alpha = alpha > 1.0 ? 1.0 : alpha;

        double chroma = (1 - Math.Abs((2 * lightness) - 1)) * saturation;
        double h1 = hue / 60;
        double x = chroma * (1 - Math.Abs((h1 % 2) - 1));
        double m = lightness - (0.5 * chroma);
        double r1, g1, b1;

        if (h1 < 1)
        {
            r1 = chroma;
            g1 = x;
            b1 = 0;
        }
        else if (h1 < 2)
        {
            r1 = x;
            g1 = chroma;
            b1 = 0;
        }
        else if (h1 < 3)
        {
            r1 = 0;
            g1 = chroma;
            b1 = x;
        }
        else if (h1 < 4)
        {
            r1 = 0;
            g1 = x;
            b1 = chroma;
        }
        else if (h1 < 5)
        {
            r1 = x;
            g1 = 0;
            b1 = chroma;
        }
        else
        {
            r1 = chroma;
            g1 = 0;
            b1 = x;
        }

        return FromArgb(
            (byte)Math.Round(255 * alpha),
            (byte)Math.Round(255 * (r1 + m)),
            (byte)Math.Round(255 * (g1 + m)),
            (byte)Math.Round(255 * (b1 + m)));
    }
}
