# 设计语言包模块

`src/DesignLanguages/<Lang>/` 是三种第一方设计语言：Ant Design、Shadcn 与 PrimeNG。每种语言由一个语言包和每个控件包各一个
取值包组成。控件包只声明 Token，取值全部在这里。架构见[设计语言架构](../../architecture/systems/theming/design-languages.md)，
编写方法见[编写设计语言](../../guides/theming/writing-design-languages.md)，应用如何选用见
[设计语言包指南](../../guides/theming/design-language-packages.md)。

旧的 `src/AtomUI.Themes.PrimeNG`、`src/AtomUI.Themes.Shadcn` 预设主题包建立在已移除的主题算法上，由这里的同名语言取代。

## 包清单

| 语言 | 语言包 | 取值包 | 风格 | 可调种子 |
|---|---|---|---|---|
| Ant Design | `AtomUI.DesignLanguages.AntDesign`（含具名主题 Daybreak Blue） | `AtomUI.Controls.Themes.AntDesign`、`AtomUI.Desktop.Controls.Themes.AntDesign`、`AtomUI.Desktop.Controls.ColorPicker.Themes.AntDesign`、`AtomUI.Desktop.Controls.DataGrid.Themes.AntDesign` | `Default` | `ColorPrimary` |
| Shadcn | `AtomUI.DesignLanguages.Shadcn` | `AtomUI.Controls.Themes.Shadcn`、`AtomUI.Desktop.Controls.Themes.Shadcn`、`AtomUI.Desktop.Controls.ColorPicker.Themes.Shadcn`、`AtomUI.Desktop.Controls.DataGrid.Themes.Shadcn` | `Vega`、`Nova`（缺省）、`Maia`、`Lyra`、`Mira`、`Luma`、`Sera`、`Rhea` | `ColorPrimary`、语言种子 `BaseColor` |
| PrimeNG | `AtomUI.DesignLanguages.PrimeNG` | `AtomUI.Controls.Themes.PrimeNG`、`AtomUI.Desktop.Controls.Themes.PrimeNG`、`AtomUI.Desktop.Controls.ColorPicker.Themes.PrimeNG`、`AtomUI.Desktop.Controls.DataGrid.Themes.PrimeNG` | `Aura`（缺省）、`Lara`、`Nora` | `ColorPrimary` |

三种语言都提供浅色与深色、常规与紧凑密度，形态目前只有桌面（Ant Design 另声明移动形态，取值待 M6）。

## 结构

```text
src/DesignLanguages/<Lang>/
+-- AtomUI.DesignLanguages.<Lang>/          语言：<Lang>Language（四个轴、种子、语言 Token、全局推导）、<Lang>Token、
|                                           Use<Lang>() 入口、README（来源与版本、规则表、不支持清单）
+-- AtomUI.Controls.Themes.<Lang>/          AtomUI.Controls 的控件推导与 AddCommonControls()
+-- AtomUI.Desktop.Controls.Themes.<Lang>/  桌面控件的推导与 AddDesktopControls()；README 列出逐控件等级
+-- AtomUI.Desktop.Controls.ColorPicker.Themes.<Lang>/   AddDesktopColorPicker()
\-- AtomUI.Desktop.Controls.DataGrid.Themes.<Lang>/      AddDesktopDataGrid()
```

- 三种语言互不派生、互不引用：Shadcn 与 PrimeNG 各自带一份 Ant Design 的 Map 与 Alias 公式副本，再写自己的规则（方案 5.6）。
- `Use<Lang>()` 启用语言；`Add*()` 只登记取值，控件包由应用自己的 `UseDesktopControls()` 等启用。
- 每个推导类写全它的控件 Token（`ATOMUIGEN050`），取值包覆盖它所对应控件包的每个 Token 类（`ATOMUIGEN052`），在启动时还
  校验控件包的 Token Schema 摘要。
- 每种语言有自己的测试工程：`tests/AtomUI.DesignLanguages.Shadcn.Tests`、`tests/AtomUI.DesignLanguages.PrimeNG.Tests`；
  Ant Design 的契约测试在 `tests/AtomUI.Desktop.Controls.Tests/Theme/AntDesignControlContractTests.cs`。三者都运行
  `AtomUI.Theme.ContractTests` 的设计语言契约。

## 约束

- 取值的来源与版本、规则表与不支持清单写在各包 README 里，与实际行为一致；README 的不支持清单是验收项。
- 语言包只引用核心；取值包只引用对应的控件包与本语言的语言包及取值包（方案 3.3）。
- 推导是纯函数；显式值在推导前冻结，推导不判断「用户是否设置过」。
- 写进 Token 的值没效果属于主题系统或控件的缺陷，不在取值包里打补丁。

## 相关文档

- [设计语言架构](../../architecture/systems/theming/design-languages.md)
- [编写设计语言](../../guides/theming/writing-design-languages.md)
- [设计语言包指南](../../guides/theming/design-language-packages.md)
- [第三方控件包](../../guides/theming/third-party-control-packages.md)
- [主题定义 XML v2](../../reference/theming/theme-definition-xml-v2.md)
