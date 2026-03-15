using System.Collections;
using System.ComponentModel;
using System.Globalization;
using AtomUI.Utils;
using Avalonia.Collections;

namespace AtomUI.Controls.Data;

public abstract class ListSortDescription
{
    public virtual string? PropertyPath => null;
    
    public virtual ListSortDirection Direction => ListSortDirection.Ascending;
    
    public bool HasPropertyPath => !string.IsNullOrEmpty(PropertyPath);
    
    public abstract IComparer<object> Comparer { get; }

    public virtual IOrderedEnumerable<object> OrderBy(IEnumerable<object> seq)
    {
        return seq.OrderBy(o => o, Comparer);
    }

    public virtual IOrderedEnumerable<object> ThenBy(IOrderedEnumerable<object> seq)
    {
        return seq.ThenBy(o => o, Comparer);
    }

    public virtual ListSortDescription SwitchSortDirection()
    {
        return this;
    }

    internal virtual void Initialize(Type itemType)
    {
    }

    private static object? InvokePath(object item, string propertyPath, Type propertyType)
    {
        object? propertyValue =
            TypeHelper.GetNestedPropertyValue(item, propertyPath, propertyType, out Exception? exception);
        if (exception != null)
        {
            throw exception;
        }

        return propertyValue;
    }
    
    public static ListSortDescription FromPath(string propertyPath, ListSortDirection direction = ListSortDirection.Ascending, CultureInfo? culture = null)
    {
        return new ListPathSortDescription(propertyPath, direction, null, culture);
    }

    public static ListSortDescription FromPath(string propertyPath, ListSortDirection direction, IComparer comparer)
    {
        return new ListPathSortDescription(propertyPath, direction, comparer, null);
    }

    public static ListSortDescription FromComparer(IComparer comparer, ListSortDirection direction = ListSortDirection.Ascending)
    {
        return new ListComparerSortDescription(comparer, direction);
    }

    /// <summary>
    /// Creates a comparer class that takes in a CultureInfo as a parameter,
    /// which it will use when comparing strings.
    /// </summary>
    private class CultureSensitiveComparer : Comparer<object>
    {
        /// <summary>
        /// Private accessor for the CultureInfo of our comparer
        /// </summary>
        private CultureInfo _culture;

        /// <summary>
        /// Creates a comparer which will respect the CultureInfo
        /// that is passed in when comparing strings.
        /// </summary>
        /// <param name="culture">The CultureInfo to use in string comparisons</param>
        public CultureSensitiveComparer(CultureInfo? culture)
        {
            _culture = culture ?? CultureInfo.InvariantCulture;
        }

        /// <summary>
        /// Compares two objects and returns a value indicating whether one is less than, equal to or greater than the other.
        /// </summary>
        /// <param name="x">first item to compare</param>
        /// <param name="y">second item to compare</param>
        /// <returns>Negative number if x is less than y, zero if equal, and a positive number if x is greater than y</returns>
        /// <remarks>
        /// Compares the 2 items using the specified CultureInfo for string and using the default object comparer for all other objects.
        /// </remarks>
        public override int Compare(object? x, object? y)
        {
            if (x == null)
            {
                if (y != null)
                {
                    return -1;
                }

                return 0;
            }

            if (y == null)
            {
                return 1;
            }

            // at this point x and y are not null
            if (x is string stringX && y is string stringY)
            {
                return _culture.CompareInfo.Compare(stringX, stringY);
            }

            return Default.Compare(x, y);
        }
    }

    private class ListPathSortDescription : ListSortDescription
    {
        private readonly ListSortDirection _direction;
        private readonly string _propertyPath;
        private readonly Lazy<CultureSensitiveComparer> _cultureSensitiveComparer;
        private readonly Lazy<IComparer<object>> _comparer;
        private Type? _propertyType;
        private IComparer? _internalComparer;
        private IComparer<object?>? _internalComparerTyped;

        private IComparer<object?>? InternalComparer
        {
            get
            {
                if (_internalComparerTyped == null && _internalComparer != null)
                {
                    if (_internalComparer is IComparer<object?> c)
                    {
                        _internalComparerTyped = c;
                    }
                    else
                    {
                        _internalComparerTyped = Comparer<object?>.Create((x, y) => _internalComparer.Compare(x, y));
                    }
                }

                return _internalComparerTyped;
            }
        }

        public override string PropertyPath => _propertyPath;
        public override IComparer<object> Comparer => _comparer.Value;
        public override ListSortDirection Direction => _direction;

        public ListPathSortDescription(string propertyPath, ListSortDirection direction, IComparer? internalComparer,
                                       CultureInfo? culture)
        {
            _propertyPath = propertyPath;
            _direction    = direction;
            _cultureSensitiveComparer = new Lazy<CultureSensitiveComparer>(() =>
                new CultureSensitiveComparer(culture ?? CultureInfo.CurrentCulture));
            _internalComparer = internalComparer;
            _comparer         = new Lazy<IComparer<object>>(() => Comparer<object>.Create(Compare));
        }

        private ListPathSortDescription(ListPathSortDescription inner, ListSortDirection direction)
        {
            _propertyPath             = inner._propertyPath;
            _direction                = direction;
            _propertyType             = inner._propertyType;
            _cultureSensitiveComparer = inner._cultureSensitiveComparer;
            _internalComparer         = inner._internalComparer;
            _internalComparerTyped    = inner._internalComparerTyped;

            _comparer = new Lazy<IComparer<object>>(() => Comparer<object>.Create(Compare));
        }

        private object? GetValue(object? o)
        {
            if (o == null)
            {
                return null;
            }
            
            if (HasPropertyPath)
            {
                return InvokePath(o, _propertyPath, _propertyType!);
            }

            if (_propertyType == o.GetType())
            {
                return o;
            }
               
            return null;
        }

        private IComparer? GetComparerForType(Type type)
        {
            if (type == typeof(string))
            {
                return _cultureSensitiveComparer.Value;
            }
            return GetComparerForNotStringType(type);
        }

        internal static IComparer? GetComparerForNotStringType(Type type)
        {
            if (System.Runtime.CompilerServices.RuntimeFeature.IsDynamicCodeSupported == false)
            {
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) &&
                    type.GetGenericArguments()[0].IsAssignableTo(typeof(IComparable)))
                {
                    return Comparer<object>.Create((x, y) => (x as IComparable)!.CompareTo(y));
                }
                    
                if (type.IsAssignableTo(typeof(IComparable)))
                {
                    //enum should be here
                    return Comparer<object>.Create((x, y) => (x as IComparable)!.CompareTo(y));
                }
                return Comparer<object>.Create((x, y) => 0); //avoid using reflection to avoid crash on AOT
            }
            return (typeof(Comparer<>).MakeGenericType(type).GetProperty("Default"))?.GetValue(null, null) as
                IComparer;
        }

        private Type? GetPropertyType(object o)
        {
            return o.GetType().GetNestedPropertyType(_propertyPath);
        }

        private int Compare(object? x, object? y)
        {
            int result = 0;

            if (_propertyType == null)
            {
                if (x != null)
                {
                    _propertyType = GetPropertyType(x);
                }

                if (_propertyType == null && y != null)
                {
                    _propertyType = GetPropertyType(y);
                }
            }

            var v1 = GetValue(x);
            var v2 = GetValue(y);

            if (_propertyType != null)
            {
                _internalComparer = GetComparerForType(_propertyType);
            }

            result = _internalComparer?.Compare(v1, v2) ?? 0;

            if (Direction == ListSortDirection.Descending)
            {
                return -result;
            }
            return result;
        }

        internal override void Initialize(Type itemType)
        {
            base.Initialize(itemType);

            if (_propertyType == null)
            {
                _propertyType = itemType.GetNestedPropertyType(_propertyPath);
            }
            else
            {
                _internalComparer = GetComparerForType(_propertyType);
            }
        }

        public override IOrderedEnumerable<object> OrderBy(IEnumerable<object> seq)
        {
            if (Direction == ListSortDirection.Descending)
            {
                return seq.OrderByDescending(GetValue, InternalComparer);
            }
            return seq.OrderBy(GetValue, InternalComparer);
        }

        public override IOrderedEnumerable<object> ThenBy(IOrderedEnumerable<object> seq)
        {
            if (Direction == ListSortDirection.Descending)
            {
                return seq.ThenByDescending(GetValue, InternalComparer);
            }
            return seq.ThenBy(GetValue, InternalComparer);
        }

        public override ListSortDescription SwitchSortDirection()
        {
            var newDirection = _direction == ListSortDirection.Ascending
                ? ListSortDirection.Descending
                : ListSortDirection.Ascending;
            return new ListPathSortDescription(this, newDirection);
        }
    }
}

public class ListComparerSortDescription : ListSortDescription
{
    private readonly IComparer _innerComparer;
    private readonly ListSortDirection _direction;
    private readonly IComparer<object> _comparer;

    public IComparer SourceComparer => _innerComparer;
    public override IComparer<object> Comparer => _comparer;
    public override ListSortDirection Direction => _direction;
    public ListComparerSortDescription(IComparer comparer, ListSortDirection direction)
    {
        _innerComparer = comparer;
        _direction     = direction;
        _comparer      = Comparer<object>.Create((x, y) => Compare(x, y));
    }

    private int Compare(object x, object y)
    {
        int result = _innerComparer.Compare(x, y);

        if (Direction == ListSortDirection.Descending)
        {
            return -result;
        }

        return result;
    }
    
    public override ListSortDescription SwitchSortDirection()
    {
        var newDirection = _direction == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;
        return new ListComparerSortDescription(_innerComparer, newDirection);
    }
}

public class ListSortDescriptionList : AvaloniaList<ListSortDescription> {}