using System.Collections;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using AtomUI.Utils;
using Avalonia.Collections;

namespace AtomUI.Controls.Data;

/// <summary>
/// Sort level that a list collection view applies to its records: it compares either the value of a property of the
/// record or the record itself, and puts the records in ascending or descending order. Create instances with
/// <c>FromPath</c> or <see cref="FromComparer" />, or derive from this class to compare records in another way.
/// </summary>
/// <remarks>
/// A description that sorts by a property path reads the value through the generated data member accessors of the
/// record type, or through reflection where dynamic code is supported. Where neither is available, as in a NativeAOT
/// application, sorting with the description throws an <see cref="InvalidOperationException" />.
/// </remarks>
public abstract class ListSortDescription : IListSortDescription
{
    /// <summary>
    /// Path of the property whose value is compared, or <see langword="null" /> when the records themselves are
    /// compared.
    /// </summary>
    public virtual string? PropertyPath => null;
    
    /// <inheritdoc />
    public virtual ListSortDirection Direction => ListSortDirection.Ascending;
    
    /// <inheritdoc />
    public bool HasPropertyPath => !string.IsNullOrEmpty(PropertyPath);
    
    /// <inheritdoc />
    public abstract IComparer<object> Comparer { get; }

    /// <summary>
    /// Orders an unsorted sequence of records by this level, in the order of <see cref="Direction" />. The view calls
    /// it for the first sort description only. The default implementation orders the records with
    /// <see cref="Comparer" />.
    /// </summary>
    /// <param name="seq">
    /// The records to order.
    /// </param>
    /// <returns>
    /// The ordered records, as a sequence that <see cref="ThenBy" /> can order further.
    /// </returns>
    public virtual IOrderedEnumerable<object> OrderBy(IEnumerable<object> seq)
    {
        return seq.OrderBy(o => o, Comparer);
    }

    /// <summary>
    /// Adds this level to a sequence that the levels before it have already ordered: records that compare equal under
    /// those levels are ordered by this level, in the order of <see cref="Direction" />. The view calls it for every
    /// sort description after the first. The default implementation orders the records with <see cref="Comparer" />.
    /// </summary>
    /// <param name="seq">
    /// The sequence ordered by the preceding sort levels.
    /// </param>
    /// <returns>
    /// The sequence with this level added as the next sort level.
    /// </returns>
    public virtual IOrderedEnumerable<object> ThenBy(IOrderedEnumerable<object> seq)
    {
        return seq.ThenBy(o => o, Comparer);
    }

    /// <summary>
    /// Returns a description that sorts by the same property path or comparer with the opposite
    /// <see cref="Direction" />. This description is not changed.
    /// </summary>
    /// <returns>
    /// The description with the opposite direction. The base implementation has nothing to switch and returns this
    /// instance unchanged; a subclass that has a direction overrides it to return a new instance.
    /// </returns>
    public virtual ListSortDescription SwitchSortDirection()
    {
        return this;
    }

    internal virtual void Initialize(Type itemType)
    {
        Initialize(itemType, null, RuntimeFeature.IsDynamicCodeSupported);
    }

    internal virtual void Initialize(Type itemType, bool isDynamicCodeSupported)
    {
        Initialize(itemType, null, isDynamicCodeSupported);
    }

    internal virtual void Initialize(Type itemType, IDataMemberAccessorDescriptor? dataMemberAccessorDescriptor)
    {
        Initialize(itemType, dataMemberAccessorDescriptor, RuntimeFeature.IsDynamicCodeSupported);
    }

    internal virtual void Initialize(Type itemType,
                                     IDataMemberAccessorDescriptor? dataMemberAccessorDescriptor,
                                     bool isDynamicCodeSupported)
    {
    }

    private interface IPropertyPathAccessor
    {
        Type? ValueType { get; }

        IComparer? Comparer { get; }

        void Initialize(Type itemType);

        object? GetValue(object instance);
    }

    private sealed class DataMemberPropertyPathAccessor : IPropertyPathAccessor
    {
        private readonly IDataMemberAccessor _accessor;

        public Type ValueType => _accessor.ValueType;

        public IComparer? Comparer => _accessor.Comparer;

        public DataMemberPropertyPathAccessor(IDataMemberAccessor accessor)
        {
            _accessor = accessor;
        }

        public void Initialize(Type itemType)
        {
        }

        public object? GetValue(object instance)
        {
            return _accessor.GetValue(instance);
        }
    }

    private sealed class RuntimeReflectionPropertyPathAccessor : IPropertyPathAccessor
    {
        private readonly string _propertyPath;
        private Type? _valueType;

        public Type? ValueType => _valueType;

        public IComparer? Comparer => null;

        public RuntimeReflectionPropertyPathAccessor(string propertyPath)
        {
            _propertyPath = propertyPath;
        }

        [UnconditionalSuppressMessage("Trimming", "IL2026",
            Justification = "Legacy reflection path is created only when dynamic code is available; NativeAOT requires generated data member accessors.")]
        public void Initialize(Type itemType)
        {
            _valueType = itemType.GetNestedPropertyType(_propertyPath);
        }

        [UnconditionalSuppressMessage("Trimming", "IL2026",
            Justification = "Legacy reflection path is created only when dynamic code is available; NativeAOT requires generated data member accessors.")]
        public object? GetValue(object instance)
        {
            if (_valueType == null)
            {
                Initialize(instance.GetType());
                if (_valueType == null)
                {
                    return null;
                }
            }

            object? propertyValue =
                TypeHelper.GetNestedPropertyValue(instance, _propertyPath, _valueType, out Exception? exception);
            if (exception != null)
            {
                throw exception;
            }

            return propertyValue;
        }
    }
    
    /// <summary>
    /// Creates a sort level that compares the value of a property of each record.
    /// </summary>
    /// <param name="propertyPath">
    /// Path of the property to compare. An empty path compares the records themselves.
    /// </param>
    /// <param name="direction">
    /// The order to put the records in.
    /// </param>
    /// <param name="culture">
    /// The culture that orders string values. <see langword="null" /> uses <see cref="CultureInfo.CurrentCulture" />.
    /// </param>
    public static ListSortDescription FromPath(string propertyPath, ListSortDirection direction = ListSortDirection.Ascending, CultureInfo? culture = null)
    {
        return new ListPathSortDescription(propertyPath, direction, null, culture);
    }

    /// <summary>
    /// Creates a sort level that compares the value of a property of each record with a caller-supplied comparer.
    /// </summary>
    /// <param name="propertyPath">
    /// Path of the property to compare. An empty path compares the records themselves.
    /// </param>
    /// <param name="direction">
    /// The order to put the records in.
    /// </param>
    /// <param name="comparer">
    /// The comparer for two property values, in ascending order. It replaces the comparer of the value type, and it
    /// receives <see langword="null" /> when the property of a record is <see langword="null" />.
    /// </param>
    public static ListSortDescription FromPath(string propertyPath, ListSortDirection direction, IComparer comparer)
    {
        return new ListPathSortDescription(propertyPath, direction, comparer, null);
    }

    /// <summary>
    /// Creates a sort level that compares the records themselves with a caller-supplied comparer.
    /// </summary>
    /// <param name="comparer">
    /// The comparer for two records, in ascending order. It receives the records themselves, not a property of them.
    /// </param>
    /// <param name="direction">
    /// The order to put the records in. Descending negates the result of <paramref name="comparer" />.
    /// </param>
    public static ListSortDescription FromComparer(IComparer comparer, ListSortDirection direction = ListSortDirection.Ascending)
    {
        return new ListComparerSortDescription(comparer, direction);
    }

    /// <summary>
    /// Compares the values of a sort level that sorts by a property path: two strings with the compare info of the
    /// culture, any other values with the default object comparer, and <see langword="null" /> as the smallest value.
    /// </summary>
    private class CultureSensitiveComparer : Comparer<object>
    {
        /// <summary>
        /// The culture whose compare info orders strings. It is never <see langword="null" />.
        /// </summary>
        private CultureInfo _culture;

        /// <summary>
        /// Creates a comparer that orders strings with the compare info of <paramref name="culture" />.
        /// </summary>
        /// <param name="culture">
        /// The culture to use in string comparisons. <see langword="null" /> uses
        /// <see cref="CultureInfo.InvariantCulture" />.
        /// </param>
        public CultureSensitiveComparer(CultureInfo? culture)
        {
            _culture = culture ?? CultureInfo.InvariantCulture;
        }

        /// <summary>
        /// Compares two values: <see langword="null" /> sorts first, two strings by the culture of this comparer, and
        /// any other pair of values by the default object comparer.
        /// </summary>
        /// <param name="x">
        /// The first value to compare.
        /// </param>
        /// <param name="y">
        /// The second value to compare.
        /// </param>
        /// <returns>
        /// A negative number if <paramref name="x" /> is less than <paramref name="y" />, zero if they are equal, and
        /// a positive number if <paramref name="x" /> is greater than <paramref name="y" />.
        /// </returns>
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
        private readonly IComparer? _explicitComparer;
        private Type? _propertyType;
        private IComparer? _activeComparer;
        private IComparer<object?>? _activeComparerTyped;
        private IPropertyPathAccessor? _propertyAccessor;

        private IComparer<object?>? InternalComparer
        {
            get
            {
                if (_activeComparerTyped == null && _activeComparer != null)
                {
                    if (_activeComparer is IComparer<object?> c)
                    {
                        _activeComparerTyped = c;
                    }
                    else
                    {
                        _activeComparerTyped = Comparer<object?>.Create((x, y) => _activeComparer.Compare(x, y));
                    }
                }

                return _activeComparerTyped;
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
            _explicitComparer = internalComparer;
            _activeComparer   = internalComparer;
            _comparer         = new Lazy<IComparer<object>>(() => Comparer<object>.Create(Compare));
        }

        private ListPathSortDescription(ListPathSortDescription inner, ListSortDirection direction)
        {
            _propertyPath             = inner._propertyPath;
            _direction                = direction;
            _propertyType             = inner._propertyType;
            _cultureSensitiveComparer = inner._cultureSensitiveComparer;
            _explicitComparer         = inner._explicitComparer;
            _activeComparer           = inner._activeComparer;
            _activeComparerTyped      = inner._activeComparerTyped;
            _propertyAccessor         = inner._propertyAccessor;

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
                EnsurePropertyAccessor(o.GetType(), null, RuntimeFeature.IsDynamicCodeSupported);
                return _propertyAccessor?.GetValue(o);
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
            return DataMemberRuntimeComparerFactory.CreateForNotStringType(type);
        }

        private void EnsurePropertyAccessor(Type itemType,
                                            IDataMemberAccessorDescriptor? dataMemberAccessorDescriptor,
                                            bool isDynamicCodeSupported)
        {
            if (_propertyAccessor != null || !HasPropertyPath)
            {
                return;
            }

            IPropertyPathAccessor propertyAccessor =
                CreatePropertyAccessor(itemType, dataMemberAccessorDescriptor, isDynamicCodeSupported);
            propertyAccessor.Initialize(itemType);
            _propertyAccessor = propertyAccessor;
            _propertyType     = propertyAccessor.ValueType;
            SetActiveComparer(GetComparerForAccessor(propertyAccessor));
        }

        private IPropertyPathAccessor CreatePropertyAccessor(Type itemType,
                                                             IDataMemberAccessorDescriptor? dataMemberAccessorDescriptor,
                                                             bool isDynamicCodeSupported)
        {
            if (TryResolveDataMemberAccessor(itemType, dataMemberAccessorDescriptor, out var accessor))
            {
                return new DataMemberPropertyPathAccessor(accessor);
            }

            if (!isDynamicCodeSupported)
            {
                throw new InvalidOperationException(
                    $"AOT-safe data member accessor for path '{_propertyPath}' on type '{itemType.FullName}' was not found. " +
                    $"Mark the data type with [{nameof(GenerateDataMemberAccessorsAttribute)}] or pass an {nameof(IDataMemberAccessorDescriptor)}.");
            }

            return new RuntimeReflectionPropertyPathAccessor(_propertyPath);
        }

        private bool TryResolveDataMemberAccessor(Type itemType,
                                                  IDataMemberAccessorDescriptor? dataMemberAccessorDescriptor,
                                                  out IDataMemberAccessor accessor)
        {
            if (dataMemberAccessorDescriptor != null &&
                IsDescriptorCompatible(dataMemberAccessorDescriptor, itemType) &&
                dataMemberAccessorDescriptor.TryGetAccessor(_propertyPath, out accessor!))
            {
                return true;
            }

            if (DataMemberAccessorRegistry.TryGetCompatible(itemType, out var descriptor) &&
                descriptor.TryGetAccessor(_propertyPath, out accessor!))
            {
                return true;
            }

            accessor = null!;
            return false;
        }

        private static bool IsDescriptorCompatible(IDataMemberAccessorDescriptor descriptor, Type itemType)
        {
            return descriptor.DataType == itemType || descriptor.DataType.IsAssignableFrom(itemType);
        }

        private IComparer? GetComparerForAccessor(IPropertyPathAccessor accessor)
        {
            if (_explicitComparer != null)
            {
                return _explicitComparer;
            }

            if (accessor.ValueType == typeof(string))
            {
                return _cultureSensitiveComparer.Value;
            }

            return accessor.Comparer ?? (accessor.ValueType == null ? null : GetComparerForType(accessor.ValueType));
        }

        private void SetActiveComparer(IComparer? comparer)
        {
            if (!ReferenceEquals(_activeComparer, comparer))
            {
                _activeComparer      = comparer;
                _activeComparerTyped = null;
            }
        }

        private void EnsurePropertyType(object? x, object? y, bool isDynamicCodeSupported)
        {
            if (_propertyAccessor != null || !HasPropertyPath)
            {
                return;
            }

            if (x != null)
            {
                EnsurePropertyAccessor(x.GetType(), null, isDynamicCodeSupported);
                return;
            }

            if (y != null)
            {
                EnsurePropertyAccessor(y.GetType(), null, isDynamicCodeSupported);
            }
        }

        private int Compare(object? x, object? y)
        {
            int result = 0;

            EnsurePropertyType(x, y, RuntimeFeature.IsDynamicCodeSupported);

            var v1 = GetValue(x);
            var v2 = GetValue(y);

            result = _activeComparer?.Compare(v1, v2) ?? 0;

            if (Direction == ListSortDirection.Descending)
            {
                return -result;
            }
            return result;
        }

        internal override void Initialize(Type itemType,
                                          IDataMemberAccessorDescriptor? dataMemberAccessorDescriptor,
                                          bool isDynamicCodeSupported)
        {
            base.Initialize(itemType, dataMemberAccessorDescriptor, isDynamicCodeSupported);

            if (HasPropertyPath)
            {
                EnsurePropertyAccessor(itemType, dataMemberAccessorDescriptor, isDynamicCodeSupported);
            }
            else
            {
                _propertyType ??= itemType;
                SetActiveComparer(_explicitComparer ?? GetComparerForType(_propertyType));
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

/// <summary>
/// Sort level that compares the records themselves with a caller-supplied comparer. <see
/// cref="ListSortDescription.FromComparer" /> creates it.
/// </summary>
public class ListComparerSortDescription : ListSortDescription
{
    private readonly IComparer _innerComparer;
    private readonly ListSortDirection _direction;
    private readonly IComparer<object> _comparer;

    /// <summary>
    /// The comparer supplied to the constructor. It receives the records themselves and ignores
    /// <see cref="Direction" />; <see cref="Comparer" /> applies the direction to its result.
    /// </summary>
    public IComparer SourceComparer => _innerComparer;
    /// <summary>
    /// Compares two records with <see cref="SourceComparer" /> and negates the result when
    /// <see cref="Direction" /> is <see cref="ListSortDirection.Descending" />.
    /// </summary>
    public override IComparer<object> Comparer => _comparer;
    /// <inheritdoc />
    public override ListSortDirection Direction => _direction;
    /// <summary>
    /// Creates a sort level that compares the records themselves with <paramref name="comparer" />.
    /// </summary>
    /// <param name="comparer">
    /// The comparer for two records, in ascending order. It receives the records themselves, not a property of them,
    /// and it is exposed unchanged as <see cref="SourceComparer" />.
    /// </param>
    /// <param name="direction">
    /// The order to put the records in. Descending negates the result of <paramref name="comparer" />.
    /// </param>
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
    
    /// <summary>
    /// Returns a new description with the opposite direction that compares with the same
    /// <see cref="SourceComparer" />. This description is not changed.
    /// </summary>
    public override ListSortDescription SwitchSortDirection()
    {
        var newDirection = _direction == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;
        return new ListComparerSortDescription(_innerComparer, newDirection);
    }
}

/// <summary>
/// The sort levels of a collection view in the order in which the view applies them: the first entry orders the
/// records, and each following entry orders the records that the entries before it left equal.
/// </summary>
public class SortDescriptionList : AvaloniaList<IListSortDescription> {}
