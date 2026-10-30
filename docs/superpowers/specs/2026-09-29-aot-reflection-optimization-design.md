# AOT 裁剪与反射边界优化设计

日期：2026-09-29

## 1. 背景

AtomUI 当前采用逐 Control 注册片段、逐主题资产工厂和 TypeMap 条件映射完成自动注册与精细裁剪。当前
`osx-arm64`、Release、真实 Desktop Minimal 样例的 NativeAOT 主程序为 20.23 MiB；同口径 full 注册对照为
44.42 MiB，缩小 54.46%，但仍未达到 18 MiB 的阶段门槛。

同时，Avalonia 12.1.3 已提供部分公开访问路径，AtomUI 中仍有少量可以移除或收窄的反射访问。其余反射目标仍为
`private`、`internal` 或 `private protected`，不能以清理为由改变 Popup、输入、焦点、窗口装饰、滚动或平台行为。

本设计合并两类工作：

1. 删除已经存在公开等价路径或重复持有对象的反射访问。
2. 缩小 NativeAOT 保留范围、启动分配和 Browser 构建成本，同时保持自动注册、图片能力和发布失败门禁。

## 2. 目标

- 保持一行包入口与自动精细裁剪，不恢复应用 usage graph、Sidecar、Unit、SCC 或显式控件清单。
- 保持公共 API、主题契约、布局、视觉、交互、事件顺序、资源优先级和平台能力。
- 删除公开 API 已能等价表达的反射访问，收窄固定成员反射造成的宽泛 DAM 保留。
- 降低未使用图片服务时的启动对象创建，并验证是否影响 NativeAOT 主程序体积。
- 缩小 TypeMap key 的字符串及元数据负担，保持跨包身份、确定性与冲突诊断。
- 避免内置固定图标通过通用动态 Provider 保留整套图标工厂。
- 降低主题注册启动分配和 Browser 发布的重复读取、哈希与类型遍历。
- 为每项优化提供同口径前后证据；没有收益或引入复杂度/回归的性能改动必须回退。

## 3. 非目标

- 不删除仍无公开等价路径的反射访问。
- 不关闭全球化、默认图片能力、网络图片、SVG、诊断或异常信息以换取体积。
- 不把 AXAML 功能视觉迁移到 C# 动态创建。
- 不放宽 AOT、Browser 后端、receipt、Mark/Sweep、输出验证或增量失效门禁。
- 不将单次 smoke 时间当作性能结论。
- 不在本任务中提交、推送、创建 PR 或修改发布版本。

## 4. 反射清理

### 4.1 Gallery OverlayLayer

`GalleryBrowserShellView.ConfigureOverlayLayers` 对 `OverlayLayer` 的读取改用公开
`OverlayLayer.GetOverlayLayer(visualLayerManager)`。`EnablePopupOverlayLayer`、`PopupOverlayLayer` 和
`LightDismissOverlayLayer` 仍为内部契约，继续使用现有路径。只有不再使用的通用反射 helper 才删除。

验证 Overlay、PopupOverlay 和 LightDismiss 三层在 Browser Gallery 初始化后均完成物化，原 z-order 与行为不变。

### 4.2 Tooltip 输入根

标准 Avalonia 输入根由 `PresentationSource` 同时实现 `IInputRoot` 和 `IPresentationSource`；其 `RootElement` 等价返回
`RootVisual`。Tooltip 改为通过公开 `IPresentationSource.RootVisual` 获取根。

对于非 `IPresentationSource` 的输入根，必须保留现有行为契约：若当前生产路径从未产生该类型，则明确失败或保留受控
fallback，不能静默把未知根当作不同窗口。覆盖窗口、Overlay Popup、独立 PopupRoot、LeaveWindow 和测试输入根。
完成后删除 `IInputRootReflectionExtensions`。

### 4.3 VisualLayerManager 层查询

Avalonia `VisualLayerManager.AddLayer` 同时把注册层加入 `_layers` 和直接 `VisualChildren`。AtomUI 的 `FindLayer<T>` 改为
遍历直接视觉子节点并排除 `Decorator.Child`，保持 `_layers` 的追加顺序和“只查注册层”语义。

删除 `GetLayers` 与 `_layers` FieldInfo。`AddLayer`、`PopupOverlayLayer` 仍为非公开且承担按需创建、logical parent、资源和
attach 通知，继续保留反射访问。

### 4.4 TextBox presenter

`AutoCompleteTextAreaBox` 和 `MentionTextArea` 已在 `OnApplyTemplate` 获取 `PART_TextPresenter`。Caret/trigger 计算改用该
template-lifetime 缓存，并在每次 re-template 重新替换引用。保持模板未应用或缺少必需部件时的既有失败语义，不增加默认
矩形或吞异常 fallback。

删除 `TextBoxReflectionExtensions` 中 `_presenter` 的 FieldInfo/getter；`_scrollViewer` 写入、垂直空间、undo snapshot 和
text input 仍有不可替代语义，继续保留。

### 4.5 FocusManager

ListView 仅查询当前焦点，且调用发生在可取得 TopLevel 的交互路径，改用公开 `TopLevel.FocusManager`。
AutoComplete 使用具体 `FocusManager.GetFocusedElement(scope)`，公开 `IFocusManager` 没有 scope 重载；旧内部 helper 还为
detached/non-Visual 元素提供 locator fallback，因此该路径继续保留。

若 ListView 的实际调用存在 detached 测试或 locator fallback 依赖，则本项保留反射并记录证据，不以 null 行为变化换取清理。

## 5. 图片服务 HTTP transport 延迟创建

### 5.1 行为

`UseCommonControls` 仍默认注册完整图片服务和 raster/SVG codec。`ImageLoaderPipeline` 不在应用启动时创建
`HttpImageTransport`；第一次规范化后的 HTTP/HTTPS source 真正进入读取路径时才创建。

本项只改变 transport 对象、handler 和 `HttpClient` 的创建时机，不改变支持的 source、请求规范化、cache key、重定向、
header、timeout、调度、共享 operation、取消或错误映射。

### 5.2 生命周期

延迟 transport 由 `ImageLoaderPipeline` 唯一拥有：

- Acquire：第一个 HTTP source reader 请求，在并发安全路径中创建一次。
- Release：`ImageLoaderPipeline.Dispose`；未创建时不执行释放。
- 并发：多个首次请求只能观察同一实例；失败创建不得发布半初始化实例。
- Dispose 竞态：dispose 开始后拒绝新创建；创建与 dispose 竞争时，已创建实例必须恰好释放一次。
- 外部 handler：沿用当前 `HttpClient(..., disposeHandler: true)` 所有权，不改变配置和测试注入契约。

先以启动对象/构造次数证明结构收益，再同口径发布判断代码可达性是否发生变化。若主程序体积不变，结论限定为启动分配优化。

## 6. 固定框架成员反射的精确保留

`TypeMemberExtension` 和 `ObjectExtension` 是公共兼容 API，保持签名与通用 DAM 契约。AtomUI 内部对固定 Avalonia 类型和固定
成员名的专用反射，不再通过要求整个成员类别的通用 helper 保留。

每个固定访问点采用可由当前 trimmer/ILC 识别的常量反射，或精确到成员签名的 `DynamicDependency`。实施时逐项对照本地
Avalonia 12.1.3 源码，保留继承查找、BindingFlags、重载选择和异常行为。不得用 `UnsafeAccessor` 统一替换，也不得删除
Popup、输入、窗口或 Wayland 的真实私有契约。

用 ILC map/mstat 比较相关类型的反射元数据和方法保留；AOT analyzer 必须无新增 warning，NativeAOT 与 Browser 两种模式
必须实际执行覆盖到的路径。

## 7. TypeMap 短 key

### 7.1 格式

生成器以完整 Group 身份、Control 完整程序集身份和 trigger 完整身份作为 hash 输入，输出固定长度、版本化的内部 key。
key 仍是纯字符串，不包含 Type、代理、descriptor 或 factory 根。

同一 Group 内生成期维护 `key -> 完整输入` 表；不同输入得到相同 key 时报告生成诊断并停止，不允许覆盖或依赖哈希碰撞概率。
生成顺序不影响 key，程序集版本和类型身份的现有语义保持不变。

### 7.2 兼容性

TypeMap attribute 和候选数组由同一次包生成共同产生。Browser linker 把 key 当作不透明身份，不从 key 反解析业务含义。
冷 NuGet、直接/传递 ProjectReference、预编译 adapter 和旧包组合必须纳入验证。

若 key 格式属于隐藏 ABI 兼容范围，则提升相应 ABI 版本并给出旧版本明确失败或兼容读取，不能静默误注册。最终选择以现有
ABI 文档和跨版本 fixture 的实际结果为准。

## 8. 内置固定图标

AtomUI 内置 AXAML 中形如 `{AntDesignIconProvider DeleteOutlined}` 的常量引用改用对应具体图标类型；用户仍可使用动态
`AntDesignIconProvider`，其公共 API、枚举、运行时选择和异常语义不变。

同一迁移模式最多先覆盖四个控件，比较链接后无关图标存在性、主程序/程序集体积、模板创建和 Gallery 行为。至少三项产生
可测保留收益且没有 Gallery 回归，才继续下一批；否则停止推广并回退没有收益的迁移。不得用 C# 动态创建替换 AXAML。

## 9. 注册启动分配

先通过分配或构造计数定位以下成本，再选择最小实现：

- 每个已选资产复制同一份全局 Token 名称。
- 同一资产在启动阶段重复排序、物化和计算 schema/contract fingerprint。
- `ControlPackageRegistrationBuilder.Mutate(Action)` 等短生命周期捕获。

独立陈旧生成契约验证、跨包冲突检测、资源顺序和 fingerprint 负例必须保留。若消除复制需要新增公共运行时 API，停止该方案，
改用生成器可表达的内部共享事实或保留现状；本任务不以公共 API 扩张换取小额分配减少。

## 10. Browser 构建成本

### 10.1 文件哈希

Build.Tasks 的 SHA256 改为流式读取，避免 `File.ReadAllBytes` 的整文件分配。单次 task/进程内相同规范路径和文件身份可复用
结果；不同构建调用仍重新验证，不能使用进程外陈旧缓存绕过内容门禁。

### 10.2 Linker 索引

Browser linker 在一次运行中按完整程序集/类型身份建立不可变索引，供 TypeMap 声明、选中目标和输出契约验证复用。
索引只组织 Cecil 定义和官方 `Annotations.IsMarked` 结果，不计算新的可达性闭包，不改变 Mark 后转换和 Sweep/Output 双阶段验证。

### 10.3 任务进程

隔离构建任务的独立进程是稳定性边界。只有 target timing 证明进程启动为主要成本，且能保持单次使用隔离、输入重验和
错误归属时，才考虑合并调用；否则本轮只优化重复 I/O 和扫描。

## 11. 分阶段执行与停止条件

1. 先完成五项反射清理，每项独立测试。
2. HTTP transport 延迟创建，先生命周期测试，再测启动和 NativeAOT。
3. 精确反射保留，以 map/mstat 证明保留范围变化。
4. TypeMap 短 key，完成生成器、运行时、Browser 和冷消费矩阵。
5. 固定图标四控件试点，触发 rollout audit 后再继续。
6. 测量并处理注册启动分配。
7. 测量并处理 Browser 构建 I/O/索引。
8. 同口径重跑最终 NativeAOT、Browser 和受影响验证。

同一优化目标最多三轮实现与测量；三轮仍无主要指标改善时回退性能专属变化，只保留已证明的正确性、反射边界清理和测量工具。

## 12. 测试与验收

### 12.1 定向测试

- Gallery overlay 三层创建及 Browser shell 约定。
- Tooltip native/overlay popup、LeaveWindow 和根识别。
- VisualLayerManager 注册层顺序、普通同型 Child、嵌套与 rehost。
- AutoComplete/Mentions 首次模板、re-template、缺件、caret/trigger 定位。
- ListView attached/detached 焦点与 focus scope。
- ImageLoader 未使用、首次 HTTP、并发首次访问、取消、dispose-before-use、dispose 竞态与 handler 所有权。
- Generator key 确定性、唯一性、碰撞失败、多个 trigger 共用代理、跨 Group/程序集。
- 固定图标模板和无关图标裁剪。
- Build.Tasks hash、receipt、输入变化/no-op；linker 多包、多 Group、空 map 和输出验证。

### 12.2 发布与消费

- 非裁剪桌面、trimmed CoreCLR、Desktop NativeAOT。
- Browser trimmed interpreter 与 Browser AOT 的真实运行。
- 直接、传递、预编译 adapter、干净 NuGet cache 消费。
- 未用 Control/proxy/factory/icon 缺失，已用模板、Token、Semantic Part、主题切换和冻结正常。
- 缺工具、ABI 不匹配、receipt 不匹配、残留 accessor 和陈旧输入继续失败。

### 12.3 性能与体积

所有前后比较固定 SDK、RID、配置、字体、样例、环境和原生库路径。至少记录：

- Minimal NativeAOT 主程序及排除调试符号的 payload。
- Size optimization profile 的同口径 selected/full 数据。
- 图片服务未使用时的 transport/handler 构造次数和可用的分配指标。
- TypeMap 候选 key 数量、生成字符数、部署字符串/元数据及启动查询时间。
- 固定图标试点的 retained icon 数与主程序/程序集体积。
- 注册冻结阶段的分配/耗时。
- Browser clean/no-op publish 总耗时、目标耗时和峰值内存。

单次运行只作为 smoke；需要声明时间改善时使用相同 warmup/iteration 策略并报告 mean、median、P95。结构性收益与时间收益分开。

## 13. 文档与交付

- 更新 AOT/TypeMap ABI、图片加载生命周期、生成器和 Browser linker 文档中发生变化的长期契约。
- 更新未发布迁移说明与当前实测状态，不把临时外部源码路径写入长期架构事实。
- 使用 affected verification 选择日常测试；不默认执行全量回归。
- 最终运行 `git diff --check`，报告实际测试数量、发布模式、测量口径、收益、未完成平台和残余风险。
- 不创建 commit，等待用户审查。

## 14. 风险与回退

- 反射替代风险：非标准 root、模板时序和 detached fallback。每项独立文件范围，可机械回退。
- 图片延迟创建风险：首次并发与 dispose 竞态。以唯一 owner 和恰好一次释放测试约束。
- key 风险：隐藏 ABI 与跨版本包组合。以版本门禁和冷消费 fixture 约束。
- 图标风险：AXAML 类型替换影响模板实例。分四控件试点并执行 rollout circuit breaker。
- 注册优化风险：削弱陈旧契约检查。所有 fingerprint 负例必须先存在并持续通过。
- Browser 优化风险：陈旧缓存或漏验产物。不得跨构建复用未绑定输入内容的结果。

任一阶段发现公共 API、行为、视觉或资源生命周期无法保持时，停止该阶段并重新设计，不把降级行为作为优化交付。
