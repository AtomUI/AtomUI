namespace AtomUI.Controls.Utils;

public enum ValueFilterMode
{
    /// <summary>
    /// Specifies that no filter is used. All items are returned.
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