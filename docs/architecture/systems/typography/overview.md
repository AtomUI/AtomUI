# AtomUI 字体子系统架构

AtomUI 的文本外观由两件互不相干的事情决定：字体文件有没有被注册进 Avalonia 的 `FontManager`，以及主题编译出的
字号与行高 Token 是多少。前者住在 `AtomUI.Fonts.*` 包里，后者住在 `AtomUI.Core` 的主题管线里，两者只在 Avalonia
真正去解析一个 `FontFamily` 字符串的那一刻才发生关系。

理解这条分界是理解字体子系统的前提。Token 系统不知道字体有没有注册成功，字体注册也不感知主题状态；没有任何
一处代码把两者对账。本目录的大部分边界问题都源于此。

主题配置怎么合并、怎么编译、Snapshot 怎么发布，这些由 [主题系统架构](../theming/runtime.md) 定义，本目录不重复，
只说明字体在那条管线里占哪个位置。

## 两条轴

字体资源注册是一次性的。字体包提供一个 `EmbeddedFontCollection`，把包内 `Assets` 目录和一个 `fonts:` 标识绑在
一起；应用在 `AppBuilder` 或 `IAtomUIBuilder` 阶段调用注册方法，集合进入 `FontManager.Current`，此后不再变化。
这条轴不读写任何 Token。

```text
轴 A  字体资源注册（与主题无关）

  AtomUI.Fonts.X/Assets/*.otf|ttf
        |  AvaloniaResource
        v
  EmbeddedFontCollection( fonts:X , avares://AtomUI.Fonts.X/Assets )
        |  WithXFont() / UseXFont()
        v
  FontManager.Current  ──> 可解析 "fonts:X#Family Name"
```

字体 Token 派生则跟着每一次主题事务重新求值。`FontFamily` 和 `FontSize` 是两个 Seed Token，前者是一条字体族
回退链，后者是基准字号。`FontSize` 经 `CalculateFontMapTokenValues` 展开成完整的字号、行高与文本高度阶梯，最终
随 `ThemeSnapshot` 一起发布给 AXAML 和控件运行时。

```text
轴 B  字体 Token 派生（主题管线内）

  Seed: FontFamily, FontSize
        |  ThemeManager.AddDefaultFont 注入 FontFamily
        v
  ThemeConfigNormalizer ──> ThemeConfigMerger ──> ThemeCompiler
                                                     |
                                    CalculateFontMapTokenValues
                                                     v
  Map: FontSizeSM/LG/XL, FontSizeHeading1..5,
       RelativeLineHeight[SM|LG], RelativeLineHeightHeading1..5,
       FontHeight[SM|LG]
  Alias: FontSizeIcon, FontWeightStrong
        |
        v
  ThemeSnapshot ──> ResourceProvider ──> {atom:SharedTokenResource FontSize}
```

`FontFamily` 在轴 B 里始终是一个不透明字符串。编译器不解析它，不校验其中的 `fonts:` 段是否已在轴 A 注册过，
也不会因为某个字族缺失而产生诊断。字符串一路传到 Avalonia，由 Avalonia 自己逐项尝试。

## 职责范围

字体子系统提供的东西不多：把字体文件封装成 `FontCollection` 并给出注册入口；定义 `FontFamily` 与 `FontSize`
两个 Seed Token；从 `FontSize` 按 Ant Design 的阶梯算法派生出 19 个 Map Token 和 2 个 Alias Token；以及
`TextUtils`、`FontUtils` 两个文本度量工具。

几件容易被误认为属于这里的事情并不属于：

字体族不能在运行时被"切换"。没有 `SetFontFamily` 这类 API，改字体族的唯一途径是替换主题配置，走
[覆盖路径](font-family-resolution.md)。

标题字号 Token（`FontSizeHeading1..5`）只是五个数值，AtomUI 没有对应的 Typography 语义控件，也没有把这些数值
映射到某种 `Title` 等级的机制。应用要自己决定哪一级用哪个 Token。

相对行高不会自动变成 Avalonia 的绝对 `LineHeight`。AtomUI 的行高 Token 是无量纲比值，Avalonia 的 `LineHeight`
是像素，中间那次乘法由消费控件自己写，[consumption.md](consumption.md) 说明了当前的约定和它的代价。

文本的渲染、换行和 shaping 属于 Avalonia 文本栈。字体文件的许可与分发也不在这里定义。

## 源码归属

| 项目或包 | 职责 | 运行时依赖 |
|---|---|---|
| `src/AtomUI.Core` | `FontFamily` / `FontSize` Seed Token、字号派生算法、字体 Token schema、`ThemeManager` 默认字体注入、`TextUtils`、`FontUtils` | 是 |
| `src/AtomUI.Fonts.AlibabaSans` | Alibaba Sans 拉丁字族资源与 `FontCollection`，6 个字重 | 是，按需引用 |
| `src/AtomUI.Fonts.AlibabaPuHuiTi` | Alibaba 普惠体 3.0 中文字族资源与 `FontCollection`，1 个字重 | 是，按需引用 |
| `src/AtomUI.Controls` | 引用 `AtomUI.Fonts.AlibabaSans`，使默认字体资源随控件包进入产物 | 是 |
| `src/AtomUI.Generator` | 从 Token 定义生成资源键、`TokenDescriptor` 和 schema 注册 | 否，Analyzer |

依赖方向是 `AtomUI.Fonts.* → AtomUI.Core`，反过来不成立：Core 不引用任何字体包。但 Core 里的默认字体族 Token
第一项恰好写着 `fonts:AlibabaSans#Alibaba Sans`。这是一个字符串形态的前向引用，能不能解析取决于应用运行时注册了
什么，编译期给不出任何保证。这条边界的完整后果见 [font-family-resolution.md](font-family-resolution.md)。

## 本目录文档

[font-packages.md](font-packages.md) 讲轴 A：`FontCollection` 怎么构成，两个包各自提供什么，注册入口有哪几个，
以及两个包之间那些不对称的地方。

[font-family-resolution.md](font-family-resolution.md) 讲字体族怎么被解析：默认回退链的构成与意图、前向引用
造成的静默回退、`FontManagerOptions` 那层平台兜底、三条覆盖路径的优先级，以及字体族注入在启动链路里的确切时序。

[tokens-and-derivation.md](tokens-and-derivation.md) 讲轴 B：23 个 Token 的分级、`e^(i/5)` 阶梯算法与行高公式、
档位到 Token 的映射，以及三个算法变体的差异——其中 Compact 的行为与 Ant Design 不一致。

[consumption.md](consumption.md) 讲控件和主题资产怎么读这些 Token，相对行高那次乘法该怎么写，以及
`TextUtils.CalculateTextSize` 为什么不能放进布局热路径。

[verification.md](verification.md) 收口：AOT 与性能约束、公共契约的兼容性矩阵、该测什么、目前实际测到了什么，
以及一张已知不一致清单。

字体包的源码组织与发布边界见 [Fonts 模块](../../../modules/fonts/overview.md)；Token 定义与派生算法的源码
所有权见 [AtomUI.Core 模块](../../../modules/core/overview.md)。

应用侧的操作路径见 [主题定制指南](../../../guides/theming/customization.md)，主题文件里字体 Token 的书写格式见
[主题定义 XML v2](../../../reference/theming/theme-definition-xml-v2.md)。要新做一个字体包，操作步骤见
[制作 AtomUI 字体包](../../../guides/typography/creating-a-font-package.md)。
