using System.Text.RegularExpressions;

namespace AtomUI.Controls;

/// <summary>
/// A validator that requires the value to be a string holding an absolute URL: an <c>http</c>, <c>https</c> or
/// <c>ftp</c> scheme, a dotted domain name whose last label has at least two letters, and an optional port, path,
/// query and fragment. <c>localhost</c> and IP addresses do not match, and a missing or blank value fails.
/// </summary>
public class FormUrlValidator : AbstractFormValidator
{
    private static readonly Regex UrlRegex = new Regex(
        @"^(https?|ftp)://" +                              // Scheme
        @"(([a-zA-Z0-9\-]+\.)+[a-zA-Z]{2,})" +              // Domain name with any number of subdomain levels
        @"(:\d+)?" +                                        // Optional port
        @"(/[^\s?#]*)?" +                                   // Optional path
        @"(\?[^\s#]*)?" +                                   // Optional query
        @"(#[^\s]*)?" +                                     // Optional fragment
        @"$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase,
        TimeSpan.FromMilliseconds(250));
    
    /// <summary>
    /// Matches the value against the URL pattern when it is a string that is not null, empty or white space only;
    /// every other value fails. The field name and the cancellation token are not used.
    /// </summary>
    /// <returns>
    /// <see langword="true" /> when the value matches the pattern, otherwise <see langword="false" />. A match that
    /// exceeds the 250 ms limit of the pattern also gives <see langword="false" />.
    /// </returns>
    protected override async Task<bool> ValidateCoreAsync(string fieldName, object? value, CancellationToken cancellationToken)
    {
        var isValid  = true;
        var strValue = value as string;
        if (string.IsNullOrWhiteSpace(strValue))
        {
            isValid = false;
        }
        else
        {
            try
            {
                isValid = UrlRegex.IsMatch(strValue);
            }
            catch (RegexMatchTimeoutException)
            {
                isValid = false;
            }
        }
        
        return await Task.FromResult(isValid);
    }
}