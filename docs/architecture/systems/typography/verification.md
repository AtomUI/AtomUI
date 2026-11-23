# 字体子系统的 AOT、兼容性与验证

本文收口三件事：字体子系统受哪些 AOT 与性能约束、哪些改动会破坏公共契约、以及该测什么和目前实际测到了什么。
最后一节汇总当前实现里已经确认的不一致点。

## AOT 与裁剪

字体这条链路对 AOT 相对友好，因为注册和 Token 元数据都是静态的：字体集合用显式构造，不做程序集扫描，不用
`Activator.CreateInstance`；Token descriptor、资源键和 schema 注册全由 Generator 在编译期产出，运行时不反射
Token 属性；字号派生是纯数值运算，只在主题编译期跑一次；Token 在 Snapshot 里以已解析值保存，AXAML 查询不做
字符串解析。

字体资源本身不受裁剪影响——`AvaloniaResource` 不参与 IL 裁剪，所以字体文件不会因为"没有静态引用"被剪掉。
Gallery Desktop 以 `PublishAot=true` 和 `IsTrimmable=true` 发布，字体包随控件包进入产物。

完整规则见 [AOT 编程规范](../../../engineering/development/aot-programming-guidelines.md)。

## 性能边界

| 路径 | 约束 |
|---|---|
| 字号派生 | 只在主题编译期执行；不得在 Measure、Arrange 或属性变更回调中重新派生 |
| Token 资源查询 | 不分配对象，不做字符串解析、反射或 LINQ |
| `TextUtils.CalculateTextSize` | 分配路径，禁止用于高频回调，见 [consumption.md](consumption.md) |
| 相对行高换算 | 纯乘法，可以在属性变更回调中执行 |
| 字体族回退链 | 链越长驻留字符串越大，不要堆无效字族名 |

最后一条不只是"没用"的问题：`FontFamily` 是引用类型 Token，`ThemeRetainedBytesEstimator` 按
`ObjectOverhead + 字族名字符串` 估算它的驻留开销并计入主题缓存容量，过长的链会挤占缓存预算。

## 兼容性

| 变更 | 兼容性 |
|---|---|
| 新增字体包 | 兼容增加 |
| 向已有包 `Assets` 增加字重文件 | 兼容增加，`fonts:` 标识与字族名不变 |
| 修改 `fonts:<Key>` 集合标识 | 破坏性变更，已有 `FontFamily` 字符串失效 |
| 修改字体包内字族名 | 破坏性变更 |
| 新增字体 Token | 兼容增加 |
| 删除或重命名字体 Token | 破坏性变更 |
| 新增字体包的字族名常量类 | 兼容增加 |
| 修改 `FontSize` 默认值 | 视觉破坏性变更，影响全部派生档位 |
| 修改字号阶梯或行高公式 | 视觉破坏性变更 |
| 修改 Compact 字体派生基数 | 视觉破坏性变更，见 [tokens-and-derivation.md](tokens-and-derivation.md) |
| 修改默认字体族回退链顺序 | 视觉破坏性变更 |
| 调整 Token 分级（Seed / Map / Alias） | 破坏性变更，改变可覆盖语义 |

需要单独强调"视觉破坏性变更"这一类：字号阶梯是设计语言基线，任何输出数值的变化都会跨全部控件传播，而且不会有
编译错误提示。这类改动必须进 [`docs/releases/`](../../../releases/overview.md) 的 API 变更文档。

## 该测什么

1. `CalculateFontSize` 在典型基数（12–20）下的档位输出与取偶行为。
2. `CalculateLineHeight` 公式与 `FontHeight*` 的取整结果。
3. Ant Design 语言默认密度与紧凑密度两个变体的字体 Token 输出，并显式断言二者的差异语义。
4. 暗色变体不改变字体 Token。
5. `AddDefaultFont` 的注入与不覆盖语义：无 Builder 默认值、有默认值、配置已含 `FontFamily` 三种情况。
6. `FontFamilyTokenValueConverter` 的解析成功与失败路径。
7. 主题定义文件中字体族与字号 Token 的读取、绑定与规范化。
8. 字体集合注册后 `fonts:<Key>#<Family>` 可解析，未注册时回退不抛异常。
9. 相对行高到绝对行高的转换在字号变化与行高变化两个触发条件下都生效。
10. NativeAOT 发布后字体资源可加载、字形正确。

## 目前实际测到了什么

字体 Token 的类型契约、解析和主题绑定在 `AtomUI.Core.Tests` 的 Theme 测试里被间接覆盖：

| 测试 | 覆盖内容 |
|---|---|
| `ThemeSchemaRegistryTests` | Token descriptor 注册与分级 |
| `ThemeTokenValueParserTests` | 包含 `FontFamily` 在内的 Token 值解析 |
| `ThemeDocumentReaderTests` | 主题文件中字体 Token 的读取 |
| `ThemeDefinitionBinderTests` | 字体 Token 的绑定与规范化 |
| `InputFoundationGlobalTokenTests` | 输入控件对字号与行高 Token 的消费 |

仓库里没有任何专门的字体或排版测试文件，上面第 1、3、4、5、8、9 项**没有直接断言**。补齐这些是既有缺口，其中
第 3 项本身就能暴露下面清单里的 Compact 偏差——那个问题存在至今没被发现，正是因为没人断言过 Compact 的字体输出。

## 已知不一致

这些是当前实现里已经确认的问题，列在这里是为了避免后来者把它们当成有意设计而照着沿用：

| 项 | 现状 | 影响 | 详见 |
|---|---|---|---|
| Compact 字体基数 | 用 `FontSize` 而非 `FontSizeSM`，Compact 不压缩字号 | 与 Ant Design 输出不一致 | [tokens-and-derivation.md](tokens-and-derivation.md) |
| 共享函数里的上游注释 | `// Smaller size font-size as base` 留在共享派生函数内 | 注释与实际基数不符，会误导后续修改 | [tokens-and-derivation.md](tokens-and-derivation.md) |
| 自然常数 | 硬编码 `2.71828` 而非 `Math.E` | 当前基数下无数值差异，属潜在精度隐患 | [tokens-and-derivation.md](tokens-and-derivation.md) |
| 字族常量类 | 只有 `AlibabaPuHuiTi` 提供，`AlibabaSans` 缺失 | 调用方得硬写 `fonts:AlibabaSans#Alibaba Sans` | [font-packages.md](font-packages.md) |
| 扩展类命名空间 | `AlibabaSansThemeManagerBuilderExtensions` 在全局命名空间，`AlibabaPuHuiTi` 版在包命名空间 | 两个包的接入体验不对称 | [font-packages.md](font-packages.md) |
| 默认字体族前向引用 | Core 的默认链引用 Core 不依赖的包 | 未注册时静默回退，没有任何诊断信号 | [font-family-resolution.md](font-family-resolution.md) |
| 相对行高换算重复 | 五个控件各自实现同一换算与失效逻辑 | 无共享 helper，新控件需照抄且容易漏触发条件 | [consumption.md](consumption.md) |
