namespace AtomUI.Controls.Primitives;

/// <summary>
/// Immutable path of a tree node: the segments that identify the node, ordered from the root to the node.
/// A segment is never empty and never contains <c>/</c>. The path without segments is <see cref="Empty" />.
/// </summary>
/// <remarks>
/// <para>
/// Equality is the default record equality over the segment arrays, so paths that are built separately are
/// not equal even when their segments match. Compare the segments or <see cref="ToString" /> to test for
/// the same path.
/// </para>
/// </remarks>
public sealed record TreeNodePath
{
    private static readonly string[] s_emptySegments = Array.Empty<string>();
    private static readonly IReadOnlyList<string> s_emptySegmentsView = Array.AsReadOnly(s_emptySegments);
    private readonly string[] _segmentsArray;
    
    /// <summary>
    /// The segments of the path, ordered from the root to the node. Read-only; empty for <see cref="Empty" />.
    /// </summary>
    public IReadOnlyList<string> Segments { get; }
    
    /// <summary>
    /// The path without segments. Every path starts with it, and it is the parent of every one-segment path.
    /// </summary>
    public static TreeNodePath Empty { get; } = new TreeNodePath(s_emptySegments, true);
    
    /// <summary>
    /// Creates a path from segments ordered from the root to the node.
    /// </summary>
    /// <param name="segments">
    /// The segments to copy, so that later changes to the array do not affect the path. Each segment must be
    /// non-empty and must not contain <c>/</c>. An empty array gives <see cref="Empty" />.
    /// </param>
    /// <exception cref="ArgumentException">
    /// <paramref name="segments" /> is <see langword="null" />, a segment is an empty string, or a segment
    /// contains <c>/</c>.
    /// </exception>
    public TreeNodePath(string[] segments)
    {
        _segmentsArray = ValidateAndCopySegments(segments);
        Segments       = CreateSegmentsView(_segmentsArray);
    }
    
    /// <summary>
    /// Creates a path by splitting a <c>/</c>-separated path string.
    /// </summary>
    /// <param name="pathString">
    /// The path string, for example <c>a/b</c>. Empty segments such as those in <c>/a//b/</c> are skipped, and
    /// an empty string gives <see cref="Empty" />. Must not be <see langword="null" />.
    /// </param>
    public TreeNodePath(string pathString)
    {
        if (pathString.Length == 0)
        {
            _segmentsArray = s_emptySegments;
            Segments       = s_emptySegmentsView;
            return;
        }
        
        _segmentsArray = ParsePathString(pathString);
        Segments       = CreateSegmentsView(_segmentsArray);
    }

    private TreeNodePath(string[] trustedSegments, bool _)
    {
        _segmentsArray = trustedSegments;
        Segments        = CreateSegmentsView(_segmentsArray);
    }

    private static string[] ValidateAndCopySegments(string[] segments)
    {
        ArgumentNullException.ThrowIfNull(segments);

        if (segments.Length == 0)
        {
            return s_emptySegments;
        }
        
        var result = new string[segments.Length];
        
        for (int i = 0; i < segments.Length; i++)
        {
            var segment = segments[i];

            if (segment.Length == 0)
            {
                throw new ArgumentException($"The {i}th path segment is an empty string", nameof(segments));
            }

            if (segment.Contains('/'))
            {
                throw new ArgumentException($"Path segment '{segment}' {i}th contains illegal character '/'", nameof(segments));
            }
            
            result[i] = segment;
        }
        
        return result;
    }

    private static string[] ParsePathString(string pathString)
    {
        if (pathString.IndexOf('/') == -1)
        {
            return [pathString];
        }

        var segmentCount = CountPathSegments(pathString);
        if (segmentCount == 0)
        {
            return s_emptySegments;
        }

        var result       = new string[segmentCount];
        var segmentStart = -1;
        var segmentIndex = 0;

        for (var i = 0; i <= pathString.Length; i++)
        {
            if (i < pathString.Length && pathString[i] != '/')
            {
                if (segmentStart < 0)
                {
                    segmentStart = i;
                }
                continue;
            }

            if (segmentStart >= 0)
            {
                result[segmentIndex++] = pathString.Substring(segmentStart, i - segmentStart);
                segmentStart           = -1;
            }
        }

        return result;
    }

    private static IReadOnlyList<string> CreateSegmentsView(string[] segments)
    {
        return segments.Length == 0 ? s_emptySegmentsView : Array.AsReadOnly(segments);
    }

    private static int CountPathSegments(string pathString)
    {
        var count     = 0;
        var inSegment = false;
        foreach (var c in pathString)
        {
            if (c == '/')
            {
                inSegment = false;
            }
            else if (!inSegment)
            {
                count++;
                inSegment = true;
            }
        }

        return count;
    }
    
    /// <summary>
    /// Gets the segment at the given position; position 0 is the segment next to the root.
    /// </summary>
    /// <param name="index">
    /// The zero-based position of the segment.
    /// </param>
    public string this[int index] => _segmentsArray[index];

    /// <summary>
    /// The number of segments in the path; 0 for <see cref="Empty" />.
    /// </summary>
    public int Length => _segmentsArray.Length;

    /// <summary>
    /// Returns the segments joined with <c>/</c>, the form that the string constructor splits back into the
    /// same path. The empty path gives an empty string.
    /// </summary>
    public override string ToString() => string.Join("/", _segmentsArray);


    /// <summary>
    /// Returns whether this path is <paramref name="other" /> or a descendant of it, by comparing the leading
    /// segments ordinally. Every path starts with <see cref="Empty" />.
    /// </summary>
    /// <param name="other">
    /// The path of the candidate ancestor.
    /// </param>
    public bool StartsWith(TreeNodePath other)
    {
        if (ReferenceEquals(this, other) || other.Length == 0)
        {
            return true;
        }
        if (other.Length > Length)
        {
            return false;
        }
        
        for (int i = 0; i < other.Length; i++)
        {
            if (!string.Equals(_segmentsArray[i], other._segmentsArray[i], StringComparison.Ordinal))
            {
                return false;
            }
        }
        return true;
    }
    
    /// <summary>
    /// Returns the path without its last segment.
    /// </summary>
    /// <returns>
    /// A new path: <see cref="Empty" /> when this path has one segment, and <see langword="null" /> when this
    /// path is <see cref="Empty" />.
    /// </returns>
    public TreeNodePath? GetParent()
    {
        if (_segmentsArray.Length == 0)
        {
            return null;
        }
        if (_segmentsArray.Length == 1)
        {
            return Empty;
        }

        var newSegments = new string[_segmentsArray.Length - 1];
        Array.Copy(_segmentsArray, newSegments, newSegments.Length);
        return new TreeNodePath(newSegments, true);
    }
    
    /// <summary>
    /// Returns a new path made of this path followed by one segment.
    /// </summary>
    /// <param name="segment">
    /// The segment to append. Must not be empty and must not contain <c>/</c>.
    /// </param>
    /// <returns>
    /// A new path. This path is not changed.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="segment" /> is <see langword="null" />, empty or contains <c>/</c>.
    /// </exception>
    public TreeNodePath Append(string segment)
    {
        ArgumentException.ThrowIfNullOrEmpty(segment);

        if (segment.Contains('/'))
        {
            throw new ArgumentException("Segments cannot contain '/'", nameof(segment));
        }
        
        var newSegments = new string[_segmentsArray.Length + 1];
        Array.Copy(_segmentsArray, newSegments, _segmentsArray.Length);
        newSegments[^1] = segment;
        return new TreeNodePath(newSegments, true);
    }
    
    /// <summary>
    /// Returns a new path made of this path followed by <paramref name="other" />.
    /// </summary>
    /// <param name="other">
    /// The path to append.
    /// </param>
    /// <returns>
    /// A new path, this path when <paramref name="other" /> is <see cref="Empty" />, or
    /// <paramref name="other" /> when this path is <see cref="Empty" />.
    /// </returns>
    public TreeNodePath Append(TreeNodePath other)
    {
        if (other.Length == 0)
        {
            return this;
        }
        if (Length == 0)
        {
            return other;
        }

        var newSegments = new string[_segmentsArray.Length + other.Length];
        Array.Copy(_segmentsArray, newSegments, _segmentsArray.Length);
        Array.Copy(other._segmentsArray, 0, newSegments, _segmentsArray.Length, other.Length);
        
        return new TreeNodePath(newSegments, true);
    }
    
    /// <summary>
    /// Returns a new path with the segment at <paramref name="index" /> replaced by <paramref name="newValue" />.
    /// </summary>
    /// <param name="index">
    /// The zero-based position of the segment to replace.
    /// </param>
    /// <param name="newValue">
    /// The replacement segment. Must not be empty and must not contain <c>/</c>.
    /// </param>
    /// <returns>
    /// A new path, or this path when the segment already equals <paramref name="newValue" />.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index" /> is not below <see cref="Length" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="newValue" /> is <see langword="null" />, empty or contains <c>/</c>.
    /// </exception>
    public TreeNodePath WithSegment(int index, string newValue)
    {
        if ((uint)index >= (uint)_segmentsArray.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }
        ArgumentException.ThrowIfNullOrEmpty(newValue);
        if (newValue.Contains('/'))
        {
            throw new ArgumentException("Segments cannot contain '/'", nameof(newValue));
        }
        if (string.Equals(_segmentsArray[index], newValue, StringComparison.Ordinal))
        {
            return this;
        }

        var newSegments = new string[_segmentsArray.Length];
        Array.Copy(_segmentsArray, newSegments, _segmentsArray.Length);
        newSegments[index] = newValue;
        
        return new TreeNodePath(newSegments, true);
    }
}
