using System.Reflection;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Immutable;

namespace AtomUI.Theme.TokenSystem;

/// <summary>
/// 所有的组件 Token 定义是除了全局的 Token 的之外的专属于当前的组件的 Token 值
/// </summary>
public abstract class AbstractControlDesignToken : AbstractDesignToken, IControlDesignToken
{
    protected DesignToken SharedToken;

    private string _id;
    private bool _isCustomTokenConfig;
    private IList<string> _customTokens;
    private IResourceDictionary _sharedResourceDeltaDictionary;

    protected AbstractControlDesignToken(string id)
    {
        _id                            = id;
        _isCustomTokenConfig           = false;
        SharedToken                    = default!;
        _customTokens                  = new List<string>();
        _sharedResourceDeltaDictionary = new ResourceDictionary();
    }

    public string GetId()
    {
        return _id;
    }

    public void AssignSharedToken(DesignToken sharedToken)
    {
        SharedToken = sharedToken;
    }

    public override void BuildResourceDictionary(IResourceDictionary dictionary)
    {
        var type = GetType();
        // internal 这里也考虑进去，还是具体的 Token 自己处理？
        var tokenProperties = type.GetProperties(BindingFlags.Public |
                                                 BindingFlags.NonPublic |
                                                 BindingFlags.Instance |
                                                 BindingFlags.FlattenHierarchy);
        var ns                     = type.Namespace;
        var tokenResourceNamespace = GetTokenResourceCatalog();
        foreach (var property in tokenProperties)
        {
            var tokenName  = $"{ns}.{_id}.{property.Name}";
            var tokenValue = property.GetValue(this);
            if ((property.PropertyType == typeof(Color) || property.PropertyType == typeof(Color?)) && 
                tokenValue is not null)
            {
                tokenValue = new ImmutableSolidColorBrush((Color)tokenValue);
            }

            dictionary[new TokenResourceKey(tokenName, tokenResourceNamespace)] = tokenValue;
        }
        
    }

    internal void BuildSharedResourceDeltaDictionary(DesignToken globalSharedToken)
    {
        if (SharedToken == globalSharedToken)
        {
            return;
        }

        _sharedResourceDeltaDictionary.Clear();
        var sharedTokenType = SharedToken.GetType();
        var tokenProperties = sharedTokenType.GetProperties(BindingFlags.Public |
                                                            BindingFlags.NonPublic |
                                                            BindingFlags.Instance |
                                                            BindingFlags.FlattenHierarchy);
        foreach (var property in tokenProperties)
        {
            var name                   = property.Name;
            var globalSharedTokenValue = property.GetValue(globalSharedToken);
            var localSharedTokenValue  = property.GetValue(SharedToken);
            // Token 值一般都是值类型
            if (globalSharedTokenValue == null && localSharedTokenValue == null)
            {
                continue;
            }

            if (globalSharedTokenValue != null && localSharedTokenValue != null)
            {
                if (globalSharedTokenValue.Equals(localSharedTokenValue))
                {
                    continue;
                }
            }

            _sharedResourceDeltaDictionary[new TokenResourceKey(name)] = localSharedTokenValue;
        }
    }

    /// <summary>
    /// 一般 control token 尽量不继承, 先看看
    /// </summary>
    /// <param name="tokenName"></param>
    /// <returns></returns>
    public bool HasToken(string tokenName)
    {
        var type = GetType();
        return type.GetProperty(tokenName, BindingFlags.Instance | BindingFlags.Public) is not null;
    }

    public virtual void CalculateTokenValues(bool isDarkMode)
    {
    }

    public bool HasCustomTokenConfig()
    {
        return _isCustomTokenConfig;
    }

    public void SetHasCustomTokenConfig(bool value)
    {
        _isCustomTokenConfig = value;
    }

    public IList<string> GetCustomTokens()
    {
        return _customTokens;
    }

    public void SetCustomTokens(IList<string> customTokens)
    {
        _customTokens = customTokens;
    }

    public IResourceDictionary GetSharedResourceDeltaDictionary()
    {
        return _sharedResourceDeltaDictionary;
    }
}