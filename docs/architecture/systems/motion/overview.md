# Motion 动效系统架构

> 状态：重构目标架构。本文定义 Motion 重构采用的契约；当前源码与验收差异由[验证状态](verification.md)维护。

Motion 是跨 Core、公共控件和具体控件包的显式动效能力。公共模型采用 `Motion` 抽象基类与具体动效子类，
目标是已有 Control，不要求 Actor 基类、Actor 接口或通用动画包装控件。

## 系统定位

本系统负责有限入场、离场、关键帧、内容展开和组合动效的定义、准备、执行、替代及释放。
普通 hover、checked、颜色等状态过渡继续由 ControlTheme 声明；状态初始化使用
[共享初始化协议](../control-infrastructure/transition-initialization.md)。持续过程反馈保留适合自身的执行方式。

内置轨迹按 Ant Design 对应组件的动效规格校准，API 使用通用命名。版本对照、性能样本和迁移进度不是公共契约。
本次目标不保留旧 Motion/Actor API 的兼容层；焦点、键盘、滚动、布局约束、模板语义和业务生命周期仍需维持其合同。

## 不变量

- 内置与第三方动效使用相同的受保护构建接口；子类不自行接管计时、取消或清理。
- 每次播放拥有独立运行快照和资源；Motion 配置对象不持有运行目标、任务、取消源或订阅。
- 同一目标同时只有一个显式 Motion 请求；同目标并行效果通过组合表达，不实行部分通道替代。
- 新请求先完成准备，再接管旧请求；旧 continuation 不得修改新请求或新模板。
- 运行资源与有意保留的终态呈现分开管理，二者都有确定的释放边界。
- 目标能力不满足时明确返回或失败，不通过重组视觉树、降级交互或无限等待掩盖缺口。
- 动效结束、批次提交、批次绘制和显示器呈现是不同事实，验证时分别记录。
- 性能目标按真实帧预算和支持矩阵验收，不依靠缩短时长、删效果或改变布局方式达标。

## 模块职责

| 模块 | 拥有 | 不拥有 |
| --- | --- | --- |
| AtomUI.Core | Motion 定义、构建、曲线、运行结果、执行所有权、内部原生适配及布局能力合同 | 具体控件状态、Desktop 宿主或公共布局控件 |
| AtomUI.Controls | 可复用内容视口/布局原语与必要模板接入 | 第二套时钟或通用业务状态机 |
| 具体控件包 | 模板、组件参数、显示意图、物理宿主和业务事件 | 各自复制的计时、取消和属性恢复协议 |
| 第三方动效库 | Motion 子类、参数、纯曲线/插值及符合合同的布局扩展 | 绕过执行协议的全局目标注册或宽泛类型抢占 |
| Gallery 与性能工具 | 原生场景、布局/输入、资源和真实帧时证据 | 改变产品默认行为以制造性能收益 |

项目依赖沿用 [Core 与 Control 包边界](../../foundations/dependency-graph.md)。基础视觉动效不依赖 Desktop 初始化，
Core 不反向引用 Controls；先使用现有项目，不为本系统新增运行时项目。

## 公开模型

公共命名空间为 `AtomUI.Animations`，AXAML 使用 `xmlns:atom="https://atomui.net"`。
`Motion` 为抽象基类，Fade、Zoom、Slide、Move、Expand/Collapse 和组合为具体子类。
应用通过实例的 `PlayAsync` 播放，也可通过 `atom:Motion.IsVisible`、`Enter`、`Leave` 声明节点展示过程。
具体成员语义由[定义与扩展](definitions-and-extension.md)拥有。

```text
Motion 子类及配置 → 本次几何/参数快照 → 统一播放协议
                                      ├─ 满足能力的合成执行
                                      ├─ 原生动画时钟
                                      └─ 专用布局能力
                                           ↓
                              当前请求的终态与资源释放
```

## 文档导航

| 文档 | 所有权 |
| --- | --- |
| [定义与第三方扩展](definitions-and-extension.md) | 基类、构建、配置、曲线、轨迹、组合与公开接入语义 |
| [内置轨迹与组件规格](built-in-motion.md) | 端点、原点、曲线控制点、时长及组件实际选择 |
| [播放与生命周期](playback-and-lifecycle.md) | 请求、结果、取消、替代、FillMode、终态持有和属性所有权 |
| [目标、布局与控件接入](targets-and-layout.md) | 无 Actor 目标、执行能力、内容视口、裁剪/命中和宿主边界 |
| [性能与呈现验收](performance.md) | 帧预算、调度/分配约束、场景、测量口径和性能门槛 |
| [验证状态与完成门禁](verification.md) | 当前源码差异、能力验证、定向验证与发布可用性 |

相关入口：[Core 模块](../../../modules/core/overview.md)、[Controls 模块](../../../modules/controls/overview.md)、
[Control 基础设施](../control-infrastructure/overview.md)。单个控件继续拥有自己的公开 API、Token、Semantic Part 和事件合同。
