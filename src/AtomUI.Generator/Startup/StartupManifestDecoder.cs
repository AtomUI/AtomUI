namespace AtomUI.Generator;

internal static class StartupManifestDecoder
{
    internal static T Read<T>(string kind, string identity, string assemblyName, string data, Func<string, T> decode)
    {
        try
        {
            return decode(data);
        }
        catch (Exception error) when (error is InvalidDataException || error is FormatException ||
                                      error is ArgumentException || error is EndOfStreamException)
        {
            throw new InvalidDataException(kind + " '" + identity + "' from assembly '" + assemblyName +
                "' could not be decoded: " + error.Message.TrimEnd('.'), error);
        }
    }
}
