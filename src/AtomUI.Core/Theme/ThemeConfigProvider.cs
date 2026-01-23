using System.Diagnostics;
using System.Reflection;
using AtomUI.Theme.Styling;
using AtomUI.Theme.TokenSystem;
using Avalonia;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Metadata;
using Avalonia.Styling;

namespace AtomUI.Theme;

using AtomUITheme = Theme;

public class ThemeConfigProvider : Control, IThemeConfigProvider
{
    #region 公共属性定义

    public static readonly StyledProperty<Control?> ContentProperty =
        AvaloniaProperty.Register<ThemeConfigProvider, Control?>(nameof(Content));

    public static readonly StyledProperty<List<string>> AlgorithmsProperty =
        AvaloniaProperty.Register<ThemeConfigProvider, List<string>>(nameof(Algorithms));

    public static readonly StyledProperty<List<TokenSetter>> SharedTokenSettersProperty =
        AvaloniaProperty.Register<ThemeConfigProvider, List<TokenSetter>>(nameof(SharedTokenSetters));

    public static readonly StyledProperty<List<ControlTokenInfoSetter>> ControlTokenInfoSettersProperty =
        AvaloniaProperty.Register<ThemeConfigProvider, List<ControlTokenInfoSetter>>(nameof(ControlTokenInfoSetters));

    [Content]
    public Control? Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    public List<string> Algorithms
    {
        get => GetValue(AlgorithmsProperty);
        set => SetValue(AlgorithmsProperty, value);
    }

    public List<TokenSetter> SharedTokenSetters
    {
        get => GetValue(SharedTokenSettersProperty);
        set => SetValue(SharedTokenSettersProperty, value);
    }

    public List<ControlTokenInfoSetter> ControlTokenInfoSetters
    {
        get => GetValue(ControlTokenInfoSettersProperty);
        set => SetValue(ControlTokenInfoSettersProperty, value);
    }

    public DesignToken SharedToken => _sharedToken;
    public bool IsDarkMode { get; protected set; }
    
    public ThemeVariant ThemeVariant { get; }

    public Dictionary<string, IControlDesignToken> ControlTokens => _controlTokens;

    #endregion

    #region 内部属性定义
    
    private DesignToken _sharedToken;
    private Dictionary<string, IControlDesignToken> _controlTokens;
    private static int _idSeed = 1;

    #endregion

    private bool _needCalculateTokenResources = true;

    static ThemeConfigProvider()
    {
        AffectsMeasure<ThemeConfigProvider>(ContentProperty);
        ContentProperty.Changed.AddClassHandler<ThemeConfigProvider>((x, e) => x.ContentChanged(e));
    }

    public ThemeConfigProvider()
    {
        _controlTokens          = new Dictionary<string, IControlDesignToken>();
        _sharedToken            = new DesignToken();
        Algorithms              = new List<string>();
        SharedTokenSetters      = new List<TokenSetter>();
        ControlTokenInfoSetters = new List<ControlTokenInfoSetter>();
        ThemeVariant            = new ThemeVariant($"ThemeConfigProvider-{_idSeed++}", null);
    }

    private void ContentChanged(AvaloniaPropertyChangedEventArgs change)
    {
        var oldChild = (Control?)change.OldValue;
        var newChild = (Control?)change.NewValue;

        if (oldChild != null)
        {
            ((ISetLogicalParent)oldChild).SetParent(null);
            LogicalChildren.Clear();
            VisualChildren.Remove(oldChild);
        }

        if (newChild != null)
        {
            ((ISetLogicalParent)newChild).SetParent(this);
            VisualChildren.Add(newChild);
            LogicalChildren.Add(newChild);
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (_needCalculateTokenResources)
        {
            CalculateTokenResources();
            _needCalculateTokenResources = false;
        }
    }

    private void CalculateTokenResources()
    {
        var checkedAlgorithms = AtomUITheme.CheckAlgorithmNames(Algorithms);
        var algorithms = new List<ThemeAlgorithm>()
        {
            ThemeAlgorithm.Default
        };
        if (checkedAlgorithms.Contains(ThemeAlgorithm.Dark))
        {
            algorithms.Add(ThemeAlgorithm.Dark);
            IsDarkMode    = true;
        }
        if (checkedAlgorithms.Contains(ThemeAlgorithm.Compact))
        {
            algorithms.Add(ThemeAlgorithm.Compact);
            IsDarkMode    = false;
        }

        IThemeVariantCalculator? baseCalculator = null;
        IThemeVariantCalculator? calculator     = null;
        foreach (var algorithm in algorithms)
        {
            calculator     = ThemeManager.Current?.CreateThemeVariantCalculator(algorithm, baseCalculator);
            baseCalculator = calculator;
        }

        Debug.Assert(calculator != null);
        
        // TODO 看后期是否需要改进，做一个缓存
        var seedTokenKeys = DesignToken.GetTokenProperties(DesignTokenKind.Seed).Select(p => p.Name).ToHashSet();
        var mapTokenKeys = DesignToken.GetTokenProperties(DesignTokenKind.Map).Select(p => p.Name).ToHashSet();
        var aliasTokenKeys =  DesignToken.GetTokenProperties(DesignTokenKind.Alias).Select(p => p.Name).ToHashSet();
        
        var sharedTokenConfigMap = new Dictionary<DesignTokenKind, Dictionary<string, string>>();
        
        sharedTokenConfigMap[DesignTokenKind.Seed] = new Dictionary<string, string>();
        sharedTokenConfigMap[DesignTokenKind.Map]   = new Dictionary<string, string>();
        sharedTokenConfigMap[DesignTokenKind.Alias]   = new Dictionary<string, string>();

        foreach (var tokenSetter in SharedTokenSetters)
        {
            if (seedTokenKeys.Contains(tokenSetter.Key))
            {
                sharedTokenConfigMap[DesignTokenKind.Seed].Add(tokenSetter.Key, tokenSetter.Value);
            }
            else if (mapTokenKeys.Contains(tokenSetter.Key))
            {
                sharedTokenConfigMap[DesignTokenKind.Map].Add(tokenSetter.Key, tokenSetter.Value);
            }
            else if (aliasTokenKeys.Contains(tokenSetter.Key))
            {
                sharedTokenConfigMap[DesignTokenKind.Alias].Add(tokenSetter.Key, tokenSetter.Value);
            }
        }

        _sharedToken.LoadConfig(sharedTokenConfigMap[DesignTokenKind.Seed]);
        // 计算得到 Map Tokens
        calculator.Calculate(_sharedToken);

        // 覆盖 Map Token
        _sharedToken.LoadConfig(sharedTokenConfigMap[DesignTokenKind.Map]);

        // 交付最终的基础色
        _sharedToken.ColorBgBase   = calculator.ColorBgBase;
        _sharedToken.ColorTextBase = calculator.ColorTextBase;
        _sharedToken.CalculateAliasTokenValues();
        
        // 覆盖 Alias Token
        _sharedToken.LoadConfig(sharedTokenConfigMap[DesignTokenKind.Alias]);

        var resourceDictionary = new ResourceDictionary();
        _sharedToken.BuildResourceDictionary(resourceDictionary);

        CollectControlTokens();
        foreach (var entry in ControlTokens)
        {
            // 如果没有修改就使用全局的
            entry.Value.AssignSharedToken(_sharedToken);
        }

        var controlTokenConfig = new Dictionary<string, ControlTokenConfigInfo>();
        foreach (var controlTokenInfoSetter in ControlTokenInfoSetters)
        {
            var key = AtomUITheme.GenerateTokenQualifiedKey(controlTokenInfoSetter.TokenId,
                controlTokenInfoSetter.Catalog);
            var configInfo = new ControlTokenConfigInfo();
            configInfo.Catalog         = controlTokenInfoSetter.Catalog;
            configInfo.TokenId         = controlTokenInfoSetter.TokenId;
            configInfo.EnableAlgorithm = controlTokenInfoSetter.EnableAlgorithm;
            foreach (var setter in controlTokenInfoSetter.Setters)
            {
                if (setter is ControlTokenSetter)
                {
                    configInfo.Tokens.Add(setter.Key, setter.Value);
                }
                else
                {
                    configInfo.SharedTokens.Add(setter.Key, setter.Value);
                }
            }

            controlTokenConfig.Add(key, configInfo);
        }

        foreach (var entry in controlTokenConfig)
        {
            var tokenId           = entry.Key;
            var catalog           = entry.Value.Catalog;
            var qualifiedTokenKey = AtomUITheme.GenerateTokenQualifiedKey(tokenId, catalog);
            var controlTokenInfo  = entry.Value;
            if (!ControlTokens.ContainsKey(qualifiedTokenKey))
            {
                continue;
            }

            var copiedSharedToken = (DesignToken)_sharedToken.Clone();

            var controlTokenConfigMap = new Dictionary<DesignTokenKind, Dictionary<string, string>>();

            controlTokenConfigMap[DesignTokenKind.Seed]  = new Dictionary<string, string>();
            controlTokenConfigMap[DesignTokenKind.Map]   = new Dictionary<string, string>();
            controlTokenConfigMap[DesignTokenKind.Alias] = new Dictionary<string, string>();

            foreach (var tokenSetter in controlTokenInfo.SharedTokens)
            {
                if (seedTokenKeys.Contains(tokenSetter.Key))
                {
                    controlTokenConfigMap[DesignTokenKind.Seed].Add(tokenSetter.Key, tokenSetter.Value);
                }
                else if (mapTokenKeys.Contains(tokenSetter.Key))
                {
                    controlTokenConfigMap[DesignTokenKind.Map].Add(tokenSetter.Key, tokenSetter.Value);
                }
                else if (aliasTokenKeys.Contains(tokenSetter.Key))
                {
                    controlTokenConfigMap[DesignTokenKind.Alias].Add(tokenSetter.Key, tokenSetter.Value);
                }
            }

            if (controlTokenInfo.EnableAlgorithm)
            {
                copiedSharedToken.LoadConfig(controlTokenConfigMap[DesignTokenKind.Seed]);
                calculator.Calculate(copiedSharedToken);
                copiedSharedToken.LoadConfig(controlTokenConfigMap[DesignTokenKind.Map]);
                copiedSharedToken.CalculateAliasTokenValues();
                copiedSharedToken.LoadConfig(controlTokenConfigMap[DesignTokenKind.Alias]);
            }
            else
            {
                copiedSharedToken.LoadConfig(controlTokenConfigMap[DesignTokenKind.Seed]);
                copiedSharedToken.LoadConfig(controlTokenConfigMap[DesignTokenKind.Map]);
                copiedSharedToken.LoadConfig(controlTokenConfigMap[DesignTokenKind.Alias]);
            }

            var controlToken = (ControlTokens[qualifiedTokenKey] as AbstractControlDesignToken)!;
            controlToken.AssignSharedToken(copiedSharedToken);
            controlToken.SetHasCustomTokenConfig(true);
            controlToken.SetCustomTokens(controlTokenInfo.Tokens.Keys.ToList());
        }

        foreach (var token in ControlTokens.Values)
        {
            var controlToken = (token as AbstractControlDesignToken)!;
            controlToken.CalculateTokenValues(IsDarkMode);
            var controlTokenType = controlToken.GetType();
            var tokenAttr        = controlTokenType.GetCustomAttribute<ControlDesignTokenAttribute>();
            var qualifiedTokenKey =
                AtomUITheme.GenerateTokenQualifiedKey(controlToken.GetId(), tokenAttr?.ResourceCatalog);

            if (controlTokenConfig.TryGetValue(qualifiedTokenKey, out var tokenConfigInfo))
            {
                controlToken.LoadConfig(tokenConfigInfo.Tokens);
            }

            controlToken.BuildResourceDictionary(resourceDictionary);
            
            if (controlToken.HasCustomTokenConfig())
            {
                controlToken.BuildSharedResourceDeltaDictionary(_sharedToken);
            }
        }
        
        Resources.MergedDictionaries.Add(resourceDictionary);
    }

    protected void CollectControlTokens()
    {
        _controlTokens.Clear();
        var controlTokenTypes = ThemeManager.Current?.ControlTokenTypes ?? [];
        foreach (var tokenType in controlTokenTypes)
        {
            var obj = Activator.CreateInstance(tokenType);
            if (obj is AbstractControlDesignToken controlToken)
            {
                var attr = tokenType.GetCustomAttribute<ControlDesignTokenAttribute>();
                Debug.Assert(attr != null);
                var qualifiedKey = AtomUITheme.GenerateTokenQualifiedKey(controlToken.GetId(), attr.ResourceCatalog);
                _controlTokens.Add(qualifiedKey, controlToken);
            }
        }
    }

    public IControlDesignToken? GetControlToken(string tokenId, string? catalog = null)
    {
        var qualifiedKey = AtomUITheme.GenerateTokenQualifiedKey(tokenId, catalog);
        return ControlTokens.GetValueOrDefault(qualifiedKey);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (VisualRoot != null)
        {
            if (change.Property == ControlTokenInfoSettersProperty ||
                change.Property == SharedTokenSettersProperty ||
                change.Property == AlgorithmsProperty)
            {
                CalculateTokenResources();
            }
        }
    }
}