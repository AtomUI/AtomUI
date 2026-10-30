using System.Collections;
using System.Globalization;
using AtomUI.Localization.Data;

namespace AtomUI.Localization;

internal sealed class LanguageDataCatalogDescriptor : LanguageCatalogDescriptor
{
    private readonly int[] _slotsByValue;

    internal LanguageDataCatalogDescriptor(LanguageModuleCatalog catalog)
        : base(catalog.CatalogId, typeof(Enum), new DataUnits(catalog.Units), retainUnits: true)
    {
        AssemblyName = catalog.AssemblyName;
        MetadataName = catalog.MetadataName;
        _slotsByValue = new int[catalog.Units.Length];
        for (var slot = 0; slot < catalog.Units.Length; slot++)
        {
            _slotsByValue[catalog.Units[slot].EnumValue] = slot;
        }
    }

    internal string AssemblyName { get; }
    internal string MetadataName { get; }

    internal override bool TryGetUnitSlot(object resourceKey, out int unitSlot)
    {
        // Generated catalogs use implicit, non-negative enum values. IConvertible handles every
        // legal underlying enum type without discovering fields or constructing reflected types.
        if (resourceKey is Enum value)
        {
            try
            {
                var number = ((IConvertible)value).ToInt32(CultureInfo.InvariantCulture);
                if ((uint)number < (uint)_slotsByValue.Length)
                {
                    unitSlot = _slotsByValue[number];
                    return true;
                }
            }
            catch (OverflowException)
            {
                // An undefined enum value outside the generated range is not a resource key.
            }
        }
        unitSlot = -1;
        return false;
    }

    private sealed class DataUnits : IReadOnlyList<LanguageCatalogUnitDescriptor>
    {
        private readonly LanguageModuleUnit[] _units;
        private LanguageCatalogUnitDescriptor[]? _descriptors;
        internal DataUnits(LanguageModuleUnit[] units) => _units = units;
        public int Count => _units.Length;
        public LanguageCatalogUnitDescriptor this[int index]
        {
            get
            {
                var descriptors = Volatile.Read(ref _descriptors);
                if (descriptors is null)
                {
                    descriptors = new LanguageCatalogUnitDescriptor[_units.Length];
                    for (var slot = 0; slot < descriptors.Length; slot++)
                    {
                        descriptors[slot] = new LanguageCatalogUnitDescriptor(_units[slot].Key, _units[slot].IsFormatted);
                    }
                    descriptors = Interlocked.CompareExchange(ref _descriptors, descriptors, null) ?? descriptors;
                }
                return descriptors[index];
            }
        }
        public IEnumerator<LanguageCatalogUnitDescriptor> GetEnumerator()
        {
            for (var slot = 0; slot < Count; slot++)
            {
                yield return this[slot];
            }
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
