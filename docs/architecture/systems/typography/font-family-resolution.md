# 字体族解析与回退

`FontFamily` 是一个 Seed Token，值是一条字体族回退链。本文讲这条链的默认构成、它在什么情况下会静默失效、有哪
几种方式覆盖它，以及它在启动链路里什么时候被写进主题配置。字号与行高的派生是另一回事，见
[tokens-and-derivation.md](tokens-and-derivation.md)。

## 默认链

`DesignToken.InitSeedTokenValues` 里的默认值是：

```text
fonts:AlibabaSans#Alibaba Sans,
Segoe UI, Segoe UI Symbol, Helvetica Neue,
Noto Sans, Noto Sans CJK SC,
文泉驿正黑, Microsoft YaHei, PingFang SC,
$Default
```

排列意图是先用自带的拉丁字族，然后按 Windows、macOS、Linux 各自常见的系统字族逐级退让，最后交给 Avalonia 的
`$Default`。

值得注意的是中文部分：`Microsoft YaHei`、`PingFang SC`、`Noto Sans CJK SC`、`文泉驿正黑` 全都是系统字族，默认链里
没有任何自带中文字体。桌面三大平台上这通常够用，但在没有中文系统字族的宿主里就会缺字，此时必须显式注册中文
字体包并覆盖这条链——Browser Gallery 就是这么做的。

## 首项可能根本不存在

默认链的第一项引用 `fonts:AlibabaSans`，而 `AtomUI.Core` 并不依赖那个包。于是同一份默认值会有两种截然不同的
结果：应用注册过 AlibabaSans 时首项命中自带字族；没注册时 `fonts:AlibabaSans` 解析失败，Avalonia 静默跳过它，
从 `Segoe UI` 开始往下试。

两条路径都不抛异常、不产生诊断、不写日志。差别只体现在最终渲染出来的字形上。换句话说，"要不要注册 AlibabaSans"
是一个会改变视觉输出、却在编译期和运行期都没有任何信号的决定。

想要确定性的字形，只有两个办法：显式注册字体包，或者把 `FontFamily` 覆盖成一条完全由自己掌握的链。依赖默认值
"应该能工作"是不成立的。

## 平台层的兜底

`WithAtomUIDefaultOptions()` 在 `AppBuilder` 上另外加了一层 Avalonia 级回退：

```csharp
.With(new FontManagerOptions
{
    FontFallbacks = [new FontFallback { FontFamily = new FontFamily("Microsoft YaHei") }]
})
```

这层和 Token 里的回退链不是一回事，两者作用在不同阶段：

```text
Token FontFamily 链   ──> 逐项尝试字族，命中即用
        |  全部未命中或选中字族缺该字形
        v
FontManagerOptions.FontFallbacks  ──> Microsoft YaHei
        |  仍然缺字
        v
Avalonia $Default
```

Token 链决定正常排版用哪个字族，`FontFallbacks` 只处理已选定字族里缺少某个字形的情况。前者管"用什么字体"，
后者管"这个字画不出来怎么办"，互相替代不了。

## 三条覆盖路径

改字体族有三个入口，优先级完全由主题配置的合并规则决定，字体子系统没有引入额外机制：

| 路径 | 作用域 | 用法 |
|---|---|---|
| `IThemeManagerBuilder.WithDefaultFontFamily` | 应用根作用域初始值 | `builder.WithDefaultFontFamily(FontFamily.Parse("..."))` |
| `ThemeConfig.Tokens["FontFamily"]` | 根或局部作用域 | 主题配置或主题定义文件中的 Token 覆盖 |
| `ControlThemeConfig` 的 `FontFamily` 覆盖 | 单个 Control identity | Control 级 Global Token 覆盖 |

`WithDefaultFontFamily` 有 `FontFamily` 和 `string` 两个重载，后者内部走 `FontFamily.Parse`，二者等价。

### Builder 默认值不是第二份状态

容易误解的一点是 `WithDefaultFontFamily` 并没有在 `ThemeManager` 里维护一份独立的字体族状态。`AddDefaultFont`
会把它折叠成 `ThemeConfig.Tokens` 里的一项，而且只在配置本身还没提供 `FontFamily` 时才写：

```csharp
private ThemeConfig? AddDefaultFont(ThemeConfig? config)
{
    const string fontFamilyToken = "FontFamily";
    if (FontFamily is null || config?.Tokens.ContainsKey(fontFamilyToken) == true)
    {
        return config;
    }
    // 复制 Tokens 并写入 FontFamily，返回新的不可变 ThemeConfig
}
```

因此显式 Token 覆盖总是胜过 Builder 默认值，两者不会叠加也不会冲突。`AddDefaultFont` 返回的是新的不可变
`ThemeConfig`，不原位修改传入配置。

### 视觉树继承绕过整套机制

Avalonia 自身的 `FontFamily` 属性继承依然有效，而且它是 LocalValue，优先级高于 Token。Browser Gallery 直接在
View 根节点上设置：

```csharp
private static readonly FontFamily s_appFontFamily =
    FontFamily.Parse($"fonts:AlibabaSans#Alibaba Sans, {AlibabaPuHuiTiFontConstants.FontFamily}, $Default");

public BrowserGalleryView()
{
    FontFamily = s_appFontFamily;
}
```

这条路径完全绕过主题系统，主题切换不会重置它。它适合宿主级的字体决策，不该出现在控件实现里——控件那样做会让
应用的 Token 覆盖失效。

## 注入时序

字体在启动过程中的位置：

```text
1. AppBuilder
     WithAtomUIDefaultOptions()          -> FontManagerOptions.FontFallbacks
     With<Package>Font()                 -> FontManager 集合（可选）

2. Application.Initialize -> UseAtomUI(builder => ...)
     builder.Use<Package>Font()          -> FontManager 集合（可选）
     builder.WithDefaultFontFamily(...)  -> ThemeManagerBuilder.FontFamily

3. ThemeManagerBuilder.Build()
     new ThemeManager { FontFamily = FontFamily }

4. ThemeManager.InitializeApplication()
     AddDefaultFont(null)                -> 规范化运行时默认配置
     AddDefaultFont(initialRequest)      -> 规范化初始配置
     CompiledThemeCatalog.LoadInitial()
     首个 ThemeSnapshot 编译与发布

5. 运行时 ApplyThemeAsync(request)
     AddDefaultFont(request.Config)      -> 归一化后进入主题事务
```

第 4 步的顺序是关键：注入发生在首个 Snapshot 编译之前，不是编译完再回头改已发布的状态，所以不存在第一帧用错
字体再闪一下的情况。第 5 步说明注入在每次主题请求时都会重跑，因此运行时切换主题不会把 Builder 提供的默认字体族
丢掉。

字体集合注册（第 1、2 步里的 `Use*Font`）与主题事务无关，主题切换过程中不能增删集合。

## Token 值形态

`FontFamily` 是复合值，文本形态由专用 converter 定义。`FontFamilyTokenValueConverter` 用 `FontFamily.Parse`
解析，失败时抛 `InvalidOperationException`，不静默回退——这与字族解析失败时的静默跳过是两件事：**Token 字符串
本身格式错误会炸，字符串里的某个字族不存在则不会。** 主题定义 XML 中的字体族值走同一个 parser，格式见
[主题定义 XML v2](../../../reference/theming/theme-definition-xml-v2.md)。

在 Token 值表里 `FontFamily` 被当作引用类型值处理，和 `ImmutableTransform` 同类，不走值类型的装箱优化路径。
`ThemeRetainedBytesEstimator` 按 `ObjectOverhead + 字族名字符串` 估算它的驻留开销并计入主题缓存容量，所以往链里
堆无效字族名不只是没用，还会占缓存预算。
