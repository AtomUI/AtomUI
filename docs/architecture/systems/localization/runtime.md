# AtomUI 多语言运行时架构

本文定义本地化运行时的状态所有权、启动构建、查询、回退和语言切换语义。运行时位于
`AtomUI.Localization`，不依赖 ThemeManager 的内部事务或语言字段。

源码不使用 `Runtime/` 作为兜底目录，也不把 `Runtime` 作为内部类型的通用后缀。应用生命周期拥有的服务组合
为 `LocalizationHost`，当前语言的原子状态持有者为 `LanguageContext`，一次完整发布为
`LanguageRevision`。物理目录遵循概览文档定义的 `Catalog/`、`Resources/`、`Services/` 加根目录核心模型的
紧凑分组。

## 核心组件

```text
LanguageManager
├── LanguageCatalogRegistry
├── IReadOnlyDictionary<LanguageTag, LanguageSnapshot>
├── LanguageContext
├── LanguageResourceProvider
├── LanguageState
└── Localizer
```

| 组件 | 职责 | 不负责 |
|---|---|---|
| `LanguageCatalogRegistry` | 冻结 Catalog schema、slot、翻译来源和语言元数据 | 当前语言、资源通知 |
| `LanguageSnapshot` | 保存某一语言完成回退后的全部资源值 | 解析 XLIFF、动态修改 |
| `LanguageContext` | 原子持有并发布当前 `LanguageRevision` | 构建 Snapshot、发布资源通知 |
| `LanguageRevision` | 把同一次提交的 Snapshot 和 State 绑定为不可分割值 | 维护支持语言集合 |
| `LanguageManager` | 验证并提交全局语言变化，发布状态和事件 | 主题事务、下载语言包 |
| `LanguageResourceProvider` | 把当前 Snapshot 投影为 Avalonia 动态资源 | 保存第二份翻译状态 |
| `Localizer` | 为 C# 调用提供强类型查询和格式化 | 字符串 Catalog 路径解析 |
| `LocalizationHost` | 持有 Manager、Localizer、Provider 并管理释放；Registry/Snapshots 委托当前 Manager | 执行查询或语言切换算法 |

## 启动构建

`UseAtomUI()` 按以下顺序构建本地化运行时：

1. 创建根 `IAtomUIBuilder`，并接入应用生成的 Language Module bootstrap。
2. 执行用户配置；`UseXxxControls()` 等扩展注册各自生成的 Language Module。
3. `ILocalizationBuilder` 收集 Catalog descriptor、内置 Translation Bundle、语言包 Bundle 和应用 Override。
4. 规范化 `defaultLanguage`、`supportedLanguages` 与 `LanguageDefinition`；支持语言按首次出现顺序去重，显式
   `LanguageDefinition` 仍要求标签唯一，并验证 Culture 与方向元数据。
5. 构建并冻结 `LanguageCatalogRegistry`，解析来源优先级、Catalog 契约和 slot。
6. 为每个支持语言构建完整不可变 `LanguageSnapshot`；任何必需资源无法解析时启动失败。
7. 创建初始 `LanguageRevision` 并交给唯一 `LanguageContext`，再初始化 `LanguageManager` 和稳定
   `LanguageResourceProvider`。
8. 将 Provider 挂载到 Application 资源链，并注册 `ILanguageManager`、`ILocalizer` 的同一运行时实例。
9. 由 `LocalizationHost` 持有构建完成的本地化服务和生命周期，在完整初始资源可用后再让应用进入首帧，
   不在首帧后补做默认语言切换。

默认同步路径在 `Build()` 后冻结全部注册。opt-in 分级启动有一次受控 Stage 2 收集窗口，结束后永久冻结；
这不开放一般运行期动态追加 Catalog/Bundle 或下载语言包。分期实现与目标差异见下节。

### 分级构建与增量演化

Stage 1 对已注册 Catalog/全部支持语言构建 Snapshot。Stage 2 冻结输入后构建新 registry，按 Catalog identity
映射复用 descriptor、Bundle 引用和语言候选链均未变的行；变更行重新验证和解析。
Manager 用 `PrepareEvolution/ValidateEvolution/CommitEvolutionCore` 协调准备和提交。
应用级协调由 Core 的[分级启动演化](../../foundations/staged-startup-evolution.md)负责，本地化仍拥有自己的
数据模型和普通语言切换事务。

增量准备按以下边界执行：

- 复用单位是不可变 Catalog 行及其源/覆盖证据；只重建新增、schema 变化或收到相关新 Bundle/Override 的 Catalog。
- 语言候选链、来源优先级、`en-US` 覆盖、格式化参数及 Culture/方向相关输入均进入失效判定；不能因 Catalog ID
  相同就复用。受影响的父语言 Bundle 要使使用其回退结果的语言行失效。
- 所有已声明支持语言在框架完成时仍具备完整 Snapshot，语言切换继续是已构建结果的引用交换；不以首次切换
  惰性解析取代既有覆盖契约。
- registry 增补后的 slot 可以重排，旧行按 Catalog identity 映射到新槽；禁止通过旧数组下标猜测新身份。
- 一次查询捕获匹配的 Catalog registry、Snapshot 与 State。演化不能先单独交换 registry、再交换 revision，
  导致后台 `Get/Format` 在两者之间读取混代状态。
- 支持语言/定义与所有语言 Snapshot 一并准备；提交时复核当前语言版本。当前语言已变化则重验/重准备，
  不得在主题已经发布后才发现语言提交不能完成。
- `LocalizationHost`、Manager、Localizer、Provider 身份稳定；宿主不再持有一份已过期的 Stage 1 Snapshot
  集作为永久备份。旧读取视图结束且缓存无用途后应可回收。
- 框架统一 CommitCore 完成前不发布语言资源/事件。普通 `ChangeLanguage` 仍独立运行，不进入主题事务。

验证比较 staged 与同步构建的全部支持语言结果，覆盖 slot 重排、父语言翻译追加、应用覆盖、RTL、格式化、
准备期间语言切换和失败后继续切换。增量目标是减少重复 Catalog 解析，不承诺省去完整覆盖校验或所有 O(C) 索引工作。

## Catalog registry

Registry 为每个 Catalog 分配在该 registry revision 内稳定的 slot，并记录：

- 生成式 Catalog ID、enum CLR 类型和稳定 Key 到 slot 的映射。
- 稳定 unit Key、enum member 到 Catalog 内部 slot 的生成式映射。
- 每个语言的 Translation Bundle、来源优先级和源文本指纹。
- `en-US` 完整源文本。
- 用于 Avalonia enum resource key 查询的生成式映射。

Registry 使用 `Type` 定位 Catalog，并通过 Generator 输出的 enum member switch 定位 slot。enum 底层数值只参与
编译后的 switch 分派，不是可序列化的 unit identity；Registry 不得通过 `GetFields()`、`Enum.GetNames()`、
`Enum.GetName()`、`ToString()`、Attribute 反射或 `Activator.CreateInstance()` 发现 Catalog 或构造 Key。

## LanguageSnapshot

Snapshot 是某个请求语言的完整、不可变、已经完成回退解析的资源表：

```text
LanguageSnapshot
├── RequestedLanguage
├── Resolved values by catalog slot and unit slot
├── Resolved source language per value
└── Revision-independent immutable storage
```

查询热路径不再遍历 BCP 47 回退链，也不合并 ResourceDictionary。Snapshot 构建阶段完成全部解析，
`ILocalizer.Get()` 和 Avalonia 资源查询只执行 slot 定位与数组读取。

Snapshot 不暴露可变 Dictionary，不缓存调用方格式化结果。所有支持语言 Snapshot 在启动时构建，确保静态
语言切换为确定的内存引用交换，不在用户操作时突然解析大量资源。

## LanguageState

```csharp
public sealed record LanguageState(
    LanguageTag CurrentLanguage,
    CultureInfo FormattingCulture,
    LanguageTextDirection TextDirection,
    long Revision);
```

- `CurrentLanguage` 是当前请求并已提交的文本语言。
- `FormattingCulture` 用于 `ILocalizer.Format()` 以及 AtomUI 控件的数字、日期和时间显示。
- `TextDirection` 是 LTR 或 RTL，供根视觉宿主投影 `FlowDirection`。
- `Revision` 每次成功切换递增，`NoOp` 不递增。

语言、格式化 Culture 和文字方向相关但不等价。应用可以为私有语言标签提供自定义 Culture 和方向。

## 回退算法

每个 Catalog、每个 unit 独立使用同一确定链：

```text
exact tag -> script/parent candidates -> neutral language -> en-US
```

示例：

```text
zh-Hant-HK -> zh-Hant -> zh -> en-US
zh-CN      -> zh-Hans -> zh -> en-US
zh-TW      -> zh-Hant -> zh -> en-US
fr-CA      -> fr -> en-US
```

候选链由 BCP 47 结构和固定 CLDR likely-subtags 数据生成，不用字符串 `Split('-')` 临时猜测。`zh-CN` 不把
`zh-TW` 作为候选，反向同理。只有显式提供的中性 `zh` Translation Bundle 才能被两个脚本族共同使用。

`en-US` 是最终安全值，但对于非英语支持语言，构建时直接落到 `en-US` 不计为翻译覆盖；这一规则防止应用
声明支持日语却悄悄显示整页英文。

## 语言切换事务

静态语言切换使用小型、独立的同步提交，不复用 ThemeTransaction：

1. 验证 UI 线程访问、Manager 未释放且目标属于支持语言；失败分别抛出文档规定的类型化异常。
2. 目标与当前语言相同则返回 `NoOp`。
3. 读取启动时构建的目标 Snapshot 和对应 `LanguageDefinition`。
4. 创建包含目标 Snapshot 与新 `LanguageState` 的 `LanguageRevision`，并在提交点由 `LanguageContext` 原子发布。
5. `LanguageResourceProvider` 发布一次 hosted resources changed 通知。
6. 根文字方向投影与 Snapshot 属于同一提交，不暴露文本已变而方向未变的状态。
7. 发布一次 `LanguageChanged`，事件参数包含旧/新 State 和结果 revision。

提交前失败不改变旧状态；提交点之后不执行可能失败的解析、加载或用户代码。事件订阅方抛出异常不得回滚
已提交语言，也不得阻止其他订阅方按既定事件策略收到通知。

## Stable LanguageResourceProvider

Provider 在 Application 初始化时只挂载一次。它始终从 `LanguageManager` 的当前 Snapshot 读取，不为每种语言
维护一份可变 Avalonia `ResourceDictionary`，切换时也不从 MergedDictionaries 删除或添加字典。

Generator 生成的 `XxxLangResourceExtension` 继续基于 enum key 创建动态资源引用。Provider 负责把 exact enum
类型和值映射到 Catalog/unit slot。语言通知只使已有动态资源重新查询，不替换 Markup Extension 或资源 key。

非 StyledElement 的 XAML 目标如果只能在 ProvideValue 时取得静态值，应通过 `ILocalizer` 读取当前 Snapshot；
这类对象不会假装获得自身不存在的 Avalonia 动态资源生命周期。

## 线程模型

- `ChangeLanguage()`、Provider 挂载和资源通知只在 UI 线程发生。
- Snapshot 与 State 使用不可变对象和原子引用发布，`ILocalizer.Get()` 可从后台线程无锁读取一个完整 revision。
- 单次 `Get()` 或 `Format()` 必须只观察一个 Snapshot；切换不能让一次格式化混用两个语言 revision。
- 事件在 UI 线程发布。后台服务需要响应语言变化时自行调度耗时工作，不阻塞提交路径。

## Culture 策略

AtomUI 默认不修改以下全局状态：

```csharp
CultureInfo.CurrentCulture
CultureInfo.CurrentUICulture
CultureInfo.DefaultThreadCurrentCulture
CultureInfo.DefaultThreadCurrentUICulture
```

AtomUI 控件必须读取 `LanguageState.FormattingCulture`，不能依赖调用线程碰巧具有相同 Culture。应用确实需要
同步进程 Culture 时，应在应用层订阅已提交的 `LanguageChanged` 并采用显式策略；该行为不进入 Manager 默认语义。

## 生命周期

`LanguageManager` 与 Application 生命周期一致。释放时必须解绑 Application 资源宿主、根 FlowDirection 投影和
内部订阅，并清空公开事件。Snapshot 和生成表不持有 Visual、ViewModel、DI Scope 或翻译包文件句柄。
