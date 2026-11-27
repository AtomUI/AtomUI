namespace AtomUI.Controls.Utils;

/// <summary>
/// Names the built-in matching rules of an <see cref="IValueFilter" />. A rule combines the relation to the filter
/// text (starts with, contains or equals) with the comparison (culture-sensitive or ordinal, case-sensitive or not).
/// </summary>
public enum ValueFilterMode
{
    /// <summary>
    /// No built-in filter: <see cref="ValueFilterFactory.BuildFilter" /> returns <see langword="null" />, and the
    /// control decides for itself how items are matched without a filter.
    /// </summary>
    None = 0,
    
    /// <summary>
    /// Specifies a culture-sensitive, case-insensitive filter where the
    /// returned items start with the specified text.
    /// </summary>
    StartsWith = 1,
    
    /// <summary>
    /// Specifies a culture-sensitive, case-sensitive filter where the
    /// returned items start with the specified text.
    /// </summary>
    StartsWithCaseSensitive = 2,
    
    /// <summary>
    /// Specifies an ordinal, case-insensitive filter where the returned
    /// items start with the specified text.
    /// </summary>
    StartsWithOrdinal = 3,
    
    /// <summary>
    /// Specifies an ordinal, case-sensitive filter where the returned items
    /// start with the specified text.
    /// </summary>
    StartsWithOrdinalCaseSensitive = 4,
    
    /// <summary>
    /// Specifies a culture-sensitive, case-insensitive filter where the
    /// returned items contain the specified text.
    /// </summary>
    Contains = 5,
    
    /// <summary>
    /// Specifies a culture-sensitive, case-sensitive filter where the
    /// returned items contain the specified text.
    /// </summary>
    ContainsCaseSensitive = 6,
    
    /// <summary>
    /// Specifies an ordinal, case-insensitive filter where the returned
    /// items contain the specified text.
    /// </summary>
    ContainsOrdinal = 7,
    
    /// <summary>
    /// Specifies an ordinal, case-sensitive filter where the returned items
    /// contain the specified text.
    /// </summary>
    ContainsOrdinalCaseSensitive = 8,
    
    /// <summary>
    /// Specifies a culture-sensitive, case-insensitive filter where the
    /// returned items equal the specified text.
    /// </summary>
    Equals = 9,
    
    /// <summary>
    /// Specifies a culture-sensitive, case-sensitive filter where the
    /// returned items equal the specified text.
    /// </summary>
    EqualsCaseSensitive = 10,
    
    /// <summary>
    /// Specifies an ordinal, case-insensitive filter where the returned
    /// items equal the specified text.
    /// </summary>
    EqualsOrdinal = 11,
    
    /// <summary>
    /// Specifies an ordinal, case-sensitive filter where the returned items
    /// equal the specified text.
    /// </summary>
    EqualsOrdinalCaseSensitive = 12,
    
    /// <summary>
    /// Specifies that a custom filter is used. 
    /// </summary>
    Custom = 13,
}