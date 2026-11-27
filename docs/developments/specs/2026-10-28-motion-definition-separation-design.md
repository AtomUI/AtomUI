# Motion 定义与接入入口拆分

用户在基线 `2de819b6a` 上确认并要求开始重构：抽象动效基类命名为 `AbstractMotion`，AXAML 继续使用 `atom:Motion.Enter`、`atom:Motion.IsVisible`。

## 已确认的方案

- `AbstractMotion` 拥有实例配置、受保护构造函数、`Build`、`PlayAsync` 和请求准备；不拥有静态接入状态。
- `Motion` 是 sealed 类型，私有构造函数，拥有附加属性、可见性登记、Reset、IsCurrent 和 GetVisibilityTask。它不继承 AbstractMotion。
- 附加属性 owner 仍为 Motion，Enter/Leave 值类型改为 AbstractMotion；所有属性名、默认值、绑定优先级、静态回调与生命周期保持一致。
- 全部内置及第三方示例子类、组合、运行时形参、控件属性及集合同步使用 AbstractMotion。公开命名空间和 XMLNS 映射不变。
- `AbstractMotion.cs` 与 `Motion.cs` 分别承载定义和接入；合并现有 Motion.Visibility.cs，避免两个很小的入口 partial 文件。
- 不修改时钟、插值、布局、播放状态机和控件交互；不增加兼容继承层或新的运行时包装对象。

## 取舍

只改原类型名会改变 AXAML owner；保留旧基类再增加新基类会增加无用层级。采用已讨论并完成附加属性原型验证的职责拆分。

## 验收

真实生产 owner 的编译 AXAML 消费自定义 AbstractMotion 子类；覆盖首次显隐、绑定更新、关闭/重开及 detach。既有 Core、Motion 消费者、Gallery 定向验证通过；生产 net8 构建和 Gallery NativeAOT 验证。临时验证删除，零净增永久用例。代码交用户审查，不自行提交。
