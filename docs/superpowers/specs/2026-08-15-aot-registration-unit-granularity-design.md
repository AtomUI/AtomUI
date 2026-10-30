# AOT Registration Unit 粒度收敛设计

> 历史记录：本文涉及的旧控件注册/裁剪方案已被 [TypeMap 正式架构](../../architecture/foundations/aot-and-trimming.md)替代，不再作为维护约束。其余内容保留当时的设计上下文。


> 日期：2026-08-15
>
> 当时记录的状态：方案已确认并于 2026-08-15 实现、验证完成。正式契约由
> [AOT Registration Unit 粒度](../../architecture/foundations/control-registration-contracts.md) 所有，本文保留问题证据、
> 方案比较、实施边界和验收基线。

## 背景

当前 linked registration 根据源码和主题的顶层目录自动拆分 Registration Unit。该策略对包含大量独立控件族的
`AtomUI.Desktop.Controls` 有明显裁剪价值，但被无差别应用到 DataGrid、ColorPicker、Extras、GalleryBase 和第三方包后，
把内部实现目录误当成了可独立运行的产品边界。

这会产生两类问题：

1. 正确性依赖包作者补写 `AtomUIRegistrationUnit` metadata；遗漏后只在 trimmed 或 NativeAOT 应用中暴露。
2. 第三方作者必须理解 Unit ownership、内部主题归属和跨 Unit 依赖，学习成本与普通 Control 开发不成比例。

真实隔离消费验证已经证明该问题不是理论风险。应用只调用 `UseDesktopColorPicker()` 并静态使用 `ColorPicker` 时，当前计划
只保留 `ColorPicker` Unit；但 `ColorPicker` 的 C# 实现会创建 `ColorPickerView`，其主题按目录归入 `ColorView` Unit。
如果没有项目文件中的人工 metadata 修补，生成计划缺少运行时必需主题。

DataGrid 当前也会按实现目录拆成 `Cell`、`Column`、`DataGrid`、`Row` 和 `Utils`，并存在 `DataGrid -> Row -> DataGrid`
循环。这说明这些目录首先是源码组织，不是天然的独立注册边界。

## 设计目标

- 默认行为必须以正确性为先，不能依赖包作者完整描述内部实现图。
- 普通第三方作者只理解 Control Package，不需要学习 Registration Unit。
- 小型或单一控件族包不因 AOT 接入增加项目文件 metadata。
- 大型多控件包仍可主动启用细粒度裁剪。
- 任何静态分析不确定性只能扩大保留范围，不能产生静默漏注册。
- 普通 full 注册、Package Core 顺序和公开 `UseXxxControls()` 调用方式保持不变。

## 非目标

- 本次不取消 Registration Unit 协议或应用级静态使用分析。
- 本次不让 Generator 猜测或生成 `UseXxxControls()` 方法体。
- 本次不根据静态 Control 使用自动调用可选包入口。
- 本次不把 Package Core 主题无条件并入 Control Unit，也不改变 `selectAssets` 语义。
- 本次不通过运行时反射、程序集扫描或异常重试修补缺失注册。

## 最终决策

引入统一的 Package 粒度属性：

```xml
<AtomUIRegistrationGranularity>Package</AtomUIRegistrationGranularity>
```

允许值只有：

| 值 | 语义 | 适用对象 |
| --- | --- | --- |
| `Package` | 当前 Control Package 生成一个完整 Registration Unit | 默认值；第三方包和单一控件族包 |
| `Directory` | 按稳定控件族目录生成多个 Registration Unit | 显式高级优化；大型多控件包 |

属性未声明时必须等价于 `Package`。未知值在包自身编译阶段报错，不能静默退回任一模式。

Theme Schema、Theme Asset、ControlMap 和 linked registration Generator 必须读取同一个规范化粒度策略，不能各自推导。

## Package 模式

Package 模式把当前包中的以下内容放进同一个内部 Unit：

- 所有由当前程序集定义并参与 linked registration 的 public Control。
- internal Presenter、Cell、View、Track 和辅助 Control。
- Control descriptor 与可选 Own Token schema。
- 所有 Control-owned Theme Asset factory。
- 包内 Control 之间的 C# 和 AXAML 实现依赖。

这个 Unit 的 identity 由 Generator 从 Package identity 稳定派生，对普通作者不可见。应用使用包内任一 Control 时，保留完整
Package Unit；应用仍必须显式调用包的 `UseXxxControls()` 入口。

Package Core 继续独立于 Control Unit，并保持当前执行顺序：Provider、Language Catalog、内置 Bundle、Global Token、Theme
Algorithm、平台 selector 和 initializer 不因粒度变化而重新分类。

Package 模式下：

- 不读取或要求 `AtomUIRegistrationUnit` metadata。
- 不要求包作者声明 Unit dependency。
- 普通 Control Theme 不需要 `AtomUIPackageSharedTheme`。
- 只有必须在未选择任何 Control Unit 时仍随入口加载的真正包级主题，才属于 `PackageShared`。
- 无法识别的 Control-owned 资源必须使当前 Package 使用 full fallback，不能被猜成 Package Core。

## Directory 模式

只有明确包含多个独立控件族、并且实际体积收益值得承担额外验证成本的包才设置：

```xml
<AtomUIRegistrationGranularity>Directory</AtomUIRegistrationGranularity>
```

Directory 模式继续使用稳定的控件族目录作为默认 Unit 边界，但必须遵守以下规则：

- Presenter、Cell、View、Semantic Part 和基础主题跟随所属公开控件族，不因内部子目录继续拆分。
- Generator 从可证明的 AXAML 与 C# 使用生成同 Package Unit 依赖；作者不手写依赖列表。
- 已知循环依赖由 fragment 的 `TryEnterUnit` 去重保证正确性；循环只会降低裁剪收益，不能造成漏注册。
- 无法证明 owner 或依赖时，对当前 Package 执行 full fallback并给出带来源位置的诊断。
- `AtomUIRegistrationUnit` 只保留为 resource-only Theme 或非常规源码布局的高级归属覆盖。
- `AtomUIPackageSharedTheme` 只表达真正跨多个 Unit 且必须随 Package Core 保留的资源，不能用于掩盖 owner 解析失败。

Directory 是显式优化承诺，不是普通第三方包的接入步骤。启用它的包必须拥有 generated plan 行为测试、trimmed JIT、
NativeAOT 和体积对比验证。

## 第一方包映射

| Package | 粒度 | 原因 |
| --- | --- | --- |
| `AtomUI.Desktop.Controls` | `Directory` | 包含大量相对独立的公开控件族，细粒度裁剪收益明确 |
| `AtomUI.Desktop.Controls.DataGrid` | `Package` | 单一产品控件族，内部目录关系紧密且存在循环 |
| `AtomUI.Desktop.Controls.ColorPicker` | `Package` | 单一产品控件族，公开入口会创建跨目录内部 View |
| `AtomUI.Desktop.Controls.Extras` | `Package` | 可选补充包，默认正确性优先于未经证明的目录粒度收益 |
| `AtomUI.Toolkits.GalleryBase` | `Package` | Gallery 工具包内部协作紧密，不作为第三方细粒度样板 |
| 第三方 Control Package | `Package` | 零 Unit 配置、保守完整、可预测 |

`AtomUI.Controls` Common 层仍由 Desktop 完整注册，不成为独立 linked Package，也不声明粒度。

## 第三方作者体验

普通第三方作者只需要：

1. 声明稳定的 `AtomUIRegistrationPackageId`。
2. 按 Control、可选 Own Token 和 `Themes/` 约定组织源码。
3. 在真实公开入口上添加 `[ControlPackageRegistrationEntry]`。
4. 在入口中保持普通 full 注册与 generated plan 分支，以及 Provider、Localization、initializer 的顺序。
5. 让应用显式调用 `UseXxxControls()`。

普通作者不写：

- 入口类型名或方法名字符串。
- `AtomUIRegistrationUnit`。
- Unit dependency 列表。
- `AvaloniaXaml Update="..."` ownership 修补。
- linker XML。
- 手工 descriptor、Theme manifest 或运行时程序集扫描。

完整操作指南由
[第三方 AtomUI Control Package 指南](../../guides/theming/third-party-control-packages.md) 所有。

## 拒绝的方案

### 所有包继续默认按目录拆分

目录是代码组织信息，不足以证明运行时独立性。继续依赖 metadata 修补会把核心正确性转移给包作者。

### 把无法归属的内部主题自动放进 Package Core

这会改变资源加载时机、扩大资源 key 可见性，并可能绕过平台 asset selector。它也不能解决 C# 创建跨 Unit Control 的问题。

### 所有包永久全量注册

虽然正确，但会取消 Desktop 大型控件包已经能够获得的有效裁剪收益。粒度应由 Package 产品边界决定，而不是全局二选一。

### 让第三方作者手写 Unit 依赖图

这会形成第二份实现事实来源，重构后容易失配，也无法覆盖动态创建和生成代码。可证明依赖应由 Generator 生成，不确定时
使用 full fallback。

## 实施范围

后续实现必须：

1. 增加并验证 `AtomUIRegistrationGranularity`，默认值为 `Package`。
2. 建立 Theme Schema 与 Theme Asset Generator 共用的粒度策略和 Unit identity 派生。
3. 让 Package 模式忽略目录拆分并覆盖全部 Control-owned 资产。
4. 仅为 `AtomUI.Desktop.Controls` 显式启用 `Directory`。
5. 删除 DataGrid 和 ColorPicker 为修补目录归属而添加的 `AvaloniaXaml Update` metadata。
6. 保留 Directory 模式的严格 owner、依赖和 fallback 诊断。
7. 增加真实第三方包、ColorPicker 和 DataGrid generated registration 集成测试。
8. 更新 Public API/Build property 契约和 NuGet consumer build asset 测试。

## 验收标准

- 普通第三方包只声明 Package ID 和真实入口即可生成完整、安全的 linked registration。
- ColorPicker 隔离应用只使用 `ColorPicker` 时仍完整保留运行时创建的 `ColorPickerView` 主题。
- DataGrid 不再按 Cell、Column、Row、Utils 等实现目录拆分。
- Desktop Directory 模式继续按控件族裁剪，并保留当前体积收益门槛。
- Package 和 Directory 模式的 ordinary/generated descriptor、asset、Language、Provider 和 initializer 行为一致。
- 不确定 owner 或动态依赖只触发 Package full fallback，不产生静默漏注册。
- trimmed JIT、NativeAOT、WebAssembly AOT 和 Gallery NativeAOT smoke 全部通过。
- 第三方指南不要求普通作者理解或填写 Unit metadata。
