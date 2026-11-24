# 语言包 Token 求值设计

`AtomUI.DesignLanguages.AntDesign`、`AtomUI.DesignLanguages.PrimeNG` 和 `AtomUI.DesignLanguages.Shadcn`
负责该语言的种子默认值、语言 Token 和全局 Map / Alias Token。控件阶段的取值契约见[控件 Token 取值规则设计](control-token-rules.md)。

语言包的接入示例见[编写设计语言](../../../guides/theming/writing-design-languages.md#33-三个方法各写什么)。

## 框架入口

| `DesignLanguage<TLanguageToken>` 入口 | 必须写入 |
| --- | --- |
| `DefineSeeds` | 全局种子与语言种子的默认值 |
| `DeriveLanguage` | 语言的非种子 Token |
| `DeriveGlobal` | 全局 Map 与 Alias Token |

`DeriveGlobal` 是一次运行时求值阶段。语言包在该入口内依次应用 Map 和 Alias 规则，Alias 读取已生效的 Map 值。
主题编译器在调用前写入并冻结显式 Map / Alias 值。种子与显式值的优先级见[单遍管线与冻结语义](design-languages.md#9-单遍管线与冻结语义)。

## 包内结构与命名

求值实现位于各语言包的 `TokenEvaluation/`，命名空间为 `AtomUI.DesignLanguages.<Language>.TokenEvaluation`。
命名空间表达语言归属，包内规则类型表达其写入的 Token 层。文件名对应主规则类型，例如 `TokenEvaluation/AliasTokenRules.cs`。
`Apply` 方法把取值写入求值期间传入的目标。

| 语言 | 迁移前求值实现 | 求值规则 |
| --- | --- | --- |
| Ant Design、PrimeNG | `AntDesignMapDerivation.Derive`、`PrimeNGMapDerivation.Derive` | `MapTokenRules.Apply` |
| Ant Design、PrimeNG、Shadcn | `AntDesignAliasDerivation.Derive`、`PrimeNGAliasDerivation.Derive`、`ShadcnAliasDerivation.Derive` | `AliasTokenRules.Apply` |
| Shadcn | `ShadcnGlobalDerivation.Derive` | `GlobalTokenRules.Apply` |
| Shadcn | `ShadcnLanguageTokens.Derive` | `LanguageTokenRules.Apply` |

Ant Design 和 PrimeNG 的 `DeriveGlobal` 各自调用 `MapTokenRules.Apply`、`AliasTokenRules.Apply`，随后写入该入口的补充值。
PrimeNG 在求值前选择当前风格的预设。Shadcn 的 `DeriveGlobal` 调用 `GlobalTokenRules.Apply`，由它依次应用 Map、Alias 和全局补充值。
Shadcn 的 `DefineSeeds` 与 `DeriveLanguage` 分别调用 `LanguageTokenRules` 的种子定义和 `Apply` 方法。

| 语言 | Map 内部顺序 |
| --- | --- |
| Ant Design | 先应用默认规则，再按变体依次叠加紧凑规则和暗色规则 |
| PrimeNG | 先应用明暗基础规则，再写入预设色彩、过渡与尺寸，最后按密度叠加紧凑规则 |
| Shadcn | 先应用明暗基础规则，再写入语言色彩和尺寸，最后按密度叠加紧凑规则 |

Tailwind 色板通过[色板生成脚本](../../../../scripts/generate-shadcn-tailwind-palette.py)从固定版本的 `theme.css` 生成。
调整生成文件的路径或命名空间时，在脚本中更新输出并重新生成，核对色板数值与顺序。

## 迁移边界与验收

迁移以语言包内部的目录、命名空间、规则类型及其调用处为范围。框架入口的签名、语言和风格标识、取值公式与表数据、
赋值顺序及显式值冻结语义是迁移前后对照的基准。

- **阶段顺序**。逐项对照三套语言在默认、暗色和紧凑变体中的 Map 叠加顺序、Alias 输入以及入口的补充赋值。
  Shadcn 同时核对种子和语言 Token 进入全局求值的路径。
- **输出一致性**。以同一份变体、种子和显式覆盖比较迁移前后的语言 Token 和全局 Map / Alias 值。
  核对 Alias 对显式 Map 覆盖的读取与冻结项优先级。
- **构建与契约**。构建三个语言包及调用其求值实现的构建和测试项目，运行
  [Ant Design](../../../../tests/AtomUI.Desktop.Controls.Tests/Theme/AntDesignControlContractTests.cs)、
  [PrimeNG](../../../../tests/AtomUI.DesignLanguages.PrimeNG.Tests/PrimeNGContractTests.cs) 和
  [Shadcn](../../../../tests/AtomUI.DesignLanguages.Shadcn.Tests/ShadcnContractTests.cs) 的现有定向契约测试。
