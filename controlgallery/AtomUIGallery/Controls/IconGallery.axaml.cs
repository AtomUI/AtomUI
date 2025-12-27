using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Reflection;
using AtomUI.Controls;
using AtomUIGallery.Models;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls.Primitives;

namespace AtomUIGallery.Controls;

public class IconGallery : TemplatedControl
{
    public static readonly StyledProperty<IconThemeType?> IconThemeTypeProperty =
        AvaloniaProperty.Register<IconInfoItem, IconThemeType?>(
            nameof(IconThemeType));

    public IconThemeType? IconThemeType
    {
        get => GetValue(IconThemeTypeProperty);
        set => SetValue(IconThemeTypeProperty, value);
    }

    #region 内部属性定义

    internal static readonly StyledProperty<AvaloniaList<PackageIconItem>?> IconInfosProperty =
        AvaloniaProperty.Register<IconInfoItem, AvaloniaList<PackageIconItem>?>(
            nameof(IconInfos));

    internal AvaloniaList<PackageIconItem>? IconInfos
    {
        get => GetValue(IconInfosProperty);
        set => SetValue(IconInfosProperty, value);
    }

    #endregion

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        ReLoadIcons();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IconThemeTypeProperty)
        {
            if (VisualRoot is not null)
            {
                ReLoadIcons();
            }
        }
    }

    private void ReLoadIcons()
    {
        var allIconClasses = CachedLoadedAssemblyTypeScanner.GetInheritedTypes<Icon>("AtomUI.Icons.AntDesign");
        var targetClasses = allIconClasses.Where(t => t.FullName?.EndsWith(IconThemeType.ToString()!) ?? false).OrderBy(t => t.Name).ToList();
        var list = new AvaloniaList<PackageIconItem>();
        foreach (var iconInfoType in targetClasses)
        {
            var obj = Activator.CreateInstance(iconInfoType);
            if (obj is Icon icon)
            {
                list.Add(new PackageIconItem(iconInfoType.Name, icon));
            }
        }
        IconInfos = list;
    }
}

public static class CachedLoadedAssemblyTypeScanner
{
    private static FrozenSet<Assembly>? _loadedAssemblies;
    private static DateTime _lastAssemblyCheck = DateTime.MinValue;
    private static readonly TimeSpan AssemblyCacheTimeout = TimeSpan.FromSeconds(5);
    
    private static readonly ConcurrentDictionary<string, FrozenSet<Type>> _typeScanCache = new();
    
    public static Assembly GetLoadedAssembly(string assemblyName)
    {
        ArgumentException.ThrowIfNullOrEmpty(assemblyName);
        
        var assemblies = GetCachedLoadedAssemblies();
        
        var assembly = assemblies.FirstOrDefault(asm => 
            asm.GetName().Name?.Equals(assemblyName, StringComparison.OrdinalIgnoreCase) == true);
        
        return assembly ?? throw new FileNotFoundException($"未找到已加载的程序集: {assemblyName}");
    }


    private static FrozenSet<Assembly> GetCachedLoadedAssemblies()
    {
        if (_loadedAssemblies is null || DateTime.UtcNow - _lastAssemblyCheck > AssemblyCacheTimeout)
        {
            _loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies()
                .ToFrozenSet();
            _lastAssemblyCheck = DateTime.UtcNow;
        }
        
        return _loadedAssemblies;
    }


    public static IReadOnlySet<Type> GetInheritedTypes(string assemblyName, Type baseType, bool includeAbstract = false)
    {
        ArgumentNullException.ThrowIfNull(baseType);
        
        var cacheKey = $"{assemblyName}|{baseType.FullName}|{includeAbstract}";
        
        return _typeScanCache.GetOrAdd(cacheKey, key =>
        {
            var assembly = GetLoadedAssembly(assemblyName);
            var types = ScanInheritedTypes(assembly, baseType, includeAbstract);
            return types.ToFrozenSet();
        });
    }
    
    public static IReadOnlySet<Type> GetInheritedTypes<TBase>(string assemblyName, bool includeAbstract = false) where TBase : class
    {
        return GetInheritedTypes(assemblyName, typeof(TBase), includeAbstract);
    }
    
    private static IEnumerable<Type> ScanInheritedTypes(Assembly assembly, Type baseType, bool includeAbstract)
    {
        Type[] types;
        
        try
        {
            types = assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            types = ex.Types.Where(t => t is not null).ToArray()!;
        }

        foreach (var type in types)
        {
            if (IsValidInheritedType(type, baseType, includeAbstract))
            {
                yield return type;
            }
        }
    }

    private static bool IsValidInheritedType(Type? type, Type baseType, bool includeAbstract)
    {
        return type is not null && 
               type != baseType && 
               type.IsClass && 
               baseType.IsAssignableFrom(type) &&
               (includeAbstract || !type.IsAbstract);
    }
    
    public static void ClearCache()
    {
        _typeScanCache.Clear();
        _loadedAssemblies = null;
    }
}