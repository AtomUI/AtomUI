# 应用资源宿主设计

本文定义 AtomUI 主题与语言资源接入 Application 的共同宿主、优先级和生命周期，是 Core、启动与本地化模块的
资源挂载契约。主题计算和内部资产顺序分别由[主题运行时](runtime.md)与[惰性物化](lazy-materialization.md)拥有。

本文定义当前分支的资源宿主实现。源码对齐情况、已完成证据与尚未完成的验收统一见
[资源宿主验证](application-resource-host-verification.md)；不能据本页推断修复已经合入或发布。

## 1. 职责与边界

资源宿主负责把已构建的框架资源接入 Avalonia 查询链，不负责重新计算 Token、改变语言状态或代替 Style 匹配。

- 主题与语言资源必须在 Application.Initialize 之前可用，保留 AXAML 和 Initialize 内创建 Shell 的消费时序。
- 正常 Application.Resources 根字典声明不得使框架资源丢失；基础主题子项的注册位置不得意外替换 AtomUI 的共享键。
- 应用显式资源覆盖、正常局部作用域及 Avalonia 属性优先级必须保持明确，不通过内部资源抢占绕过用户配置。
- Provider、资源字典、索引和已物化资产复用同一实例，不因换主题或语言重新挂载。
- 每个资源只有一个物理挂载所有者和一个实际 owner；初始化、失败回滚与释放闭环。

此边界不改变公共 API、Token 和主题键、控件模板、动画、Dispatcher、后台 Prepare、Splash 或主窗体交接。
Core 不依赖 Fluent 等外部框架，也不识别外部主题的具体类型。

## 2. 共同宿主与物理结构

框架资源挂在 `Application.Styles.Resources.MergedDictionaries`，不挂在可由应用 AXAML 替换的
Application.Resources 中，也不通过 ThemeManager 作为 Style 子项的位置决定资源优先级。

```text
Application
├── Resources                         应用自己的资源字典
└── Styles
    ├── Resources
    │   ├── 宿主直接条目与主题分支
    │   └── MergedDictionaries
    │       ├── ThemeManager.Resources
    │       ├── LanguageResourceProvider
    │       └── 已有应用合并字典
    └── Style 子项                    保留正常注册/声明顺序
        ├── 根 ThemeContext Style
        ├── FlowDirection Style
        └── 外部主题与用户 Style
```

“共同宿主”表示同一个原生字典位置，不引入第二套 ThemeManager、资源路由代理或应用服务定位器。
图中 Style 子项仅表示职责，具体相对顺序保留现有注册与 AXAML 声明规则。
ThemeManager 挂载自己的 Resources 字典，不同时把自身作为全局 Style 挂载，避免资源树重复 AddOwner。
根 ThemeContext Style 独立进入 Application.Styles；其 Selector 与 Setter 保持原样，不与 FlowDirection Style 合并。

主题字典内部的 Token provider、静态资产 provider、包和资产顺序不变；语言 Provider 仍由独立 LocalizationHost
提供。共享挂载位置不使语言进入 ThemeTransaction，也不使 ThemeManager 取得语言状态所有权。

普通 Application.Styles 内容声明和 Application.Resources 根字典声明均须支持。直接替换根 Styles.Resources、
清空全部全局接入对象或手工拆除框架资源，不在自动恢复契约内；不为这些显式改写增加持续监听和重新排序。

## 3. 资源优先级

### 3.1 应用级顺序

更近的控件、页面和独立宿主仍按真实作用域查找。到达 Application 后，从高到低为：

1. Application.Resources 的正常资源解析。
2. Application.Styles.Resources 的直接条目和主题分支。
3. 共同宿主中优先级高于框架前缀的应用合并字典。
4. LanguageResourceProvider 与 ThemeManager.Resources；主题字典内部顺序由现有主题契约定义。
5. Application.Styles 的子 Style 资源，继续按 Avalonia 的正常集合规则解析。

Provider 对不属于自己的键保持正常 miss 或开放回退。资源层较高不代表拥有整个字符串或类型命名空间。
本节描述资源键解析，不改变 Local Value、Binding、Style Setter、ControlTheme 或伪类的属性优先级。

### 3.2 框架前缀

框架资源插入合并集合的起始段，不追加到已有应用合并字典之后：

1. ApplicationScope 在起始位置挂入语言 Provider。
2. ThemeManager 随后在起始位置挂入主题 Resources。

最终框架前缀为 `[主题资源, 语言 Provider]`。合并字典倒序查询时，已有应用字典先于框架默认资源，原有应用字典之间
的相对顺序不变。用户后续显式调整合并集合时仍遵循正常次序语义。

### 3.3 覆盖兼容性

| 定制入口 | 契约 |
| --- | --- |
| Application.Resources 的同键资源 | 应用显式覆盖优先，语言和主题默认值均不抢占 |
| 根 Styles.Resources 的直接条目及已有合并字典 | 按宿主直接条目和框架前缀规则保留覆盖 |
| 外部主题子项中的共享基础类型键 | AtomUI 框架默认资源先于子项命中，不依赖外部主题加载先后 |
| 独立 Style.Resources 的同键资源 | 位于框架宿主层之下，可能与迁移前结果不同 |
| 页面/控件局部资源、公开主题属性 | 按真实作用域及属性优先级处理，独立窗口与 Popup 分别验证 |
| 普通用户 Style Setter、专用 Semantic Part Style | 不调整原匹配和属性优先级 |

独立 Style.Resources 的同键覆盖存在可观察兼容性变化，不能宣称全部定制方式保持相同结果。
全局默认资源替换使用 Application.Resources，局部替换使用局部 Resources 或控件公开主题属性。
发布迁移说明必须记录这个边界；没有兼容性审查和对应验证，不得把宿主迁移作为无损清理发布。

## 4. 所有权与生命周期

| 所有者 | 持有对象与挂载记录 | 对称释放 |
| --- | --- | --- |
| ApplicationScope（Core） | 同一个语言 Provider 的实际目标字典、FlowDirection Style | 初始化回滚/应用 Dispose 时移除自己的对象，再释放本地化服务 |
| ThemeManager（Core） | 同一个主题 Resources 的实际目标字典、根 ThemeContext Style | 主题初始化回滚/Dispose 时卸载自己的对象，保留原主题事务和系统外观释放流程 |
| LocalizationHost（Localization） | LanguageManager、Localizer 与 Provider 的服务关系 | 释放本地化服务，不管理全局主题资源集合 |

ApplicationScope 编排整个应用生命周期，ThemeManager 保留自己资源的唯一物理挂载责任。ApplicationScope 通过
初始化与 Dispose 编排 ThemeManager，不重复移除后者拥有的字典。ThemeManager 的 internal 独立初始化入口也必须
有完整的应用资源挂载和释放，不能依赖调用者额外构造 ApplicationScope。

### 4.1 初始化边界

所有可能调用 AddOwner、资源绑定或通知的挂载均进入可回滚范围。调用 Add/Insert 前记录实际目标及本次对象；即使
集合已经插入对象或 AddOwner 写入 owner 后才抛错，回滚也必须尝试移除。不能只在方法成功返回后记录拥有关系。

完整资源挂载发生在自有服务 Attach、应用 initializer 和包钩子实际消费资源之前。保留原幂等与重复初始化规则，
不引入 Loaded/Dispatcher 后补资源的中间阶段。启动整体顺序仍由[启动生命周期](../startup/lifecycle.md)拥有。

### 4.2 失败、释放与通知

- 只从记录的实际目标移除本模块自己的对象，不在释放时重新读取一个可能已被宿主替换的 Resources 属性。
- 不 Clear 全局 Styles 或目标合并集合；应用字典和外部框架对象必须保留。
- 按依赖顺序卸载 Style、资源字典和订阅，再清理其数据；回滚和正常释放必须可安全重复进入。
- 一个资源或订阅清理抛错时，继续清理其他对象，保留原始异常及既有聚合异常语义。
- 语言变化沿原 Provider 资源通知传播，主题变化沿原 ThemeContext/variant 发布流程传播，不重挂载刷新。
- 启动后的包追加复用当前宿主及原 Provider，保留候选/已提交视图、钩子与事务回滚边界。

允许记录真实目标字典和根 Style 引用；不增加每控件标记、定时器、集合监听、资源副本或反射回退。

## 5. 完整性、性能与 AOT

宿主迁移保留包启用、OnCommit/OnFirstUse、Token 索引、惰性快照和资产、开放动态资源以及追加事务。
对外部键的查询前移可能触发开放资产的首次构造，不能把所有 miss 都视作免费，也不能通过跳过合法回退加速。

每应用一次的宿主字典/集合开销必须报告；不得新增随控件数量增长的宿主、订阅或查询分配。
主题切换不重新排序全局 Styles，不重复绑定已有字典，正常主题内部查询顺序不变。资产索引和 fallback 继续遵循
[惰性物化与资源查询](lazy-materialization.md)，不得扫描或预加载全部资源来构建新索引。

保持编译型 AXAML、静态资源引用和生成式注册，不增加公共 API 或 Core 对外部控件框架的引用。
普通 JIT 通过不代替裁剪/NativeAOT 发布验收。完整场景、采样基线、门槛和证据状态见
[资源宿主验证](application-resource-host-verification.md)。

## 6. 相关设计

- [主题运行时](runtime.md)：初始 snapshot、Root ThemeContext、主题事务与资源发布。
- [启动生命周期](../startup/lifecycle.md)：Initialize 前提交、自有服务及包钩子的消费边界。
- [本地化运行时](../localization/runtime.md)：语言 revision、服务实例、Provider 与 RTL。
- [Core 模块](../../../modules/core/overview.md)：应用资源接入的源码所有权。
- [开发者集成指南](../../../guides/theming/framework-theme-integration.md)：普通 AXAML 接入、覆盖迁移与版本限制。
