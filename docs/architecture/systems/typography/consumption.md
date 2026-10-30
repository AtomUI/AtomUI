# 字体 Token 消费契约

Token 编译出来之后，控件和主题资产怎么读它们。本文还讲相对行高到绝对行高那次乘法为什么落在控件身上、以及
`TextUtils` 的使用边界。Token 本身的定义与派生见 [tokens-and-derivation.md](tokens-and-derivation.md)。

## AXAML

字体 Token 通过共享 Token 资源扩展消费：

```xml
<Setter Property="FontSize" Value="{atom:SharedTokenResource FontSize}" />
<Setter Property="FontWeight" Value="{atom:SharedTokenResource FontWeightStrong}" />
<Setter Property="MinHeight" Value="{atom:SharedTokenResource FontHeight}" />
```

`{atom:SharedTokenResource ...}` 读的永远是全局 Token。要让某个控件响应 Control 级覆盖，得用生成的
`{atom:XxxTokenResource ...}`，它同时看该 Control 的 Effective Global Token 和 Own Token。规则由
[主题系统架构](../theming/runtime.md) 定义。

内置主题里的实际使用频次：

| Token | 次数 | Token | 次数 |
|---|---|---|---|
| `FontSize` | 47 | `FontHeightLG` | 9 |
| `FontSizeLG` | 21 | `FontWeightStrong` | 8 |
| `FontSizeSM` | 17 | `FontHeightSM` | 5 |
| `FontHeight` | 16 | `FontFamily` | 4 |
| `FontSizeIcon` | 10 | | |

`FontFamily` 只出现 4 次，因为绝大多数控件靠 Avalonia 的属性继承拿到字体族，只有需要脱离继承链时才显式设置。
标题字号 Token 在内置主题里一次都没被消费——它们是给应用层排版用的，AtomUI 自己不用。

## C#

Control Token 类通过 `EffectiveGlobalToken` 读全局字体 Token：

```csharp
var lineHeight   = EffectiveGlobalToken.RelativeLineHeight;
var lineHeightLG = EffectiveGlobalToken.RelativeLineHeightLG;
```

`EffectiveGlobalToken` 已经应用了当前作用域和 Control 级覆盖，不需要再解析。控件实现不该直接访问 `DesignToken`
实例，也不该自己重跑派生算法——那会绕过覆盖，得到与主题不一致的值。

## 相对行高那次乘法

Avalonia 的 `TextBlock.LineHeight` 是绝对像素，AtomUI 的行高 Token 是无量纲比值，中间必须有一次乘法。子系统没有
提供隐式桥接，这次乘法由消费控件自己写。

当前约定是声明一个 `Relative*LineHeight` 的 `StyledProperty` 绑 Token，在属性变更时乘以有效字号写入绝对属性：

```csharp
public static readonly StyledProperty<double> RelativeLineHeightProperty =
    AvaloniaProperty.Register<DescriptionDefaultItem, double>(nameof(RelativeLineHeight));

// OnPropertyChanged 中
if (change.Property == RelativeLineHeightProperty || change.Property == FontSizeProperty)
{
    SetCurrentValue(LineHeightProperty, RelativeLineHeight * FontSize);
}
```

主题侧绑 Token：

```xml
<Setter Property="RelativeLineHeight" Value="{atom:SharedTokenResource RelativeLineHeight}" />
```

两个触发条件都得监听。只监听相对行高的话，字号变化（主题切换、`SizeType` 变化、局部 Token 覆盖）之后行高不会
跟着更新，表现为行距和字号对不上——而且这种 bug 只在字号变化时才出现，静态截图看不出来。

目前有五个控件各写了一遍这段逻辑：

| 控件 | 目标属性 | 备注 |
|---|---|---|
| `AbstractResult` | `HeaderLineHeight` / `SubHeaderLineHeight` | Header 与 SubHeader 两组独立换算 |
| `TimelineIndicator` | `IndicatorMinHeight` | 换算结果用于指示器最小高度，不是文本行高 |
| `DescriptionDefaultItem` | `LineHeight` | |
| `DescriptionBorderedCell` | `LineHeight` | |
| `UploadTextListItemHeader` | `EffectiveLineHeight` | |

五处的乘法和失效逻辑是同一套，但没有共享 helper 或基类。新增文本控件只能照抄，并自己记得覆盖两个触发条件。
这是现状，不是推荐形态，记在 [verification.md](verification.md) 的已知不一致清单里。

## 文本度量

两个静态工具，都无状态：

| API | 位置 | 用途 |
|---|---|---|
| `TextUtils.CalculateTextSize` | `AtomUI.Core/Media/TextUtils.cs` | 用 `TextLayout` 测量给定字族、字号、字形、字重下的文本尺寸 |
| `FontUtils.ConvertEmToPixel` | `AtomUI.Core/Media/FontUtils.cs` | 按字号与渲染缩放把 em 值换算为像素 |

```csharp
public static Size CalculateTextSize(string text,
                                    double fontSize,
                                    FontFamily fontFamily,
                                    FontStyle fontStyle = FontStyle.Normal,
                                    FontWeight fontWeight = FontWeight.Normal)
{
    var       typeface   = new Typeface(fontFamily, fontStyle, fontWeight);
    using var textLayout = new TextLayout(text, typeface, fontSize, null);
    return new Size(textLayout.Width, textLayout.Height);
}
```

每次调用都构造 `Typeface` 和 `TextLayout` 再立即释放，是分配路径。**不要放进 Measure、Arrange 或指针移动这类
高频回调**，需要反复测量就缓存结果，或者直接持有并复用一个 `TextLayout` 实例。

`ConvertEmToPixel` 是纯乘法，无分配，任意路径都可以用。

`AtomUI.Core/Media/TextFormatting` 下的 `FormattedTextSource` 和 `InlinesTextSource`
服务富文本与内联元素排版，属渲染适配层，不在字体子系统的契约范围内。
