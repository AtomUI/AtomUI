namespace AtomUI.Generator;

// CompositeFormat is unavailable on netstandard2.0. Validate its syntax without formatting
// values or allocating an array proportional to an untrusted placeholder index.
internal static class StartupCompositeFormat
{
    internal static bool IsValid(string value)
    {
        var position = 0;
        while (position < value.Length)
        {
            var current = value[position++];
            if (current is not ('{' or '}'))
            {
                continue;
            }
            if (position < value.Length && value[position] == current)
            {
                position++;
                continue;
            }
            if (current == '}' || !Number(value, ref position))
            {
                return false;
            }
            Spaces(value, ref position);
            if (position < value.Length && value[position] == ',')
            {
                position++;
                Spaces(value, ref position);
                if (position < value.Length && value[position] == '-')
                {
                    position++;
                }
                if (!Number(value, ref position))
                {
                    return false;
                }
                Spaces(value, ref position);
            }
            if (position < value.Length && value[position] == ':')
            {
                position++;
                while (position < value.Length && value[position] != '}')
                {
                    if (value[position++] == '{')
                    {
                        return false;
                    }
                }
            }
            if (position >= value.Length || value[position++] != '}')
            {
                return false;
            }
        }
        return true;
    }

    private static bool Number(string value, ref int position)
    {
        if (position >= value.Length || value[position] < '0' || value[position] > '9')
        {
            return false;
        }
        position++;
        while (position < value.Length && value[position] >= '0' && value[position] <= '9')
        {
            position++;
        }
        return true;
    }

    private static void Spaces(string value, ref int position)
    {
        while (position < value.Length && value[position] == ' ')
        {
            position++;
        }
    }
}
