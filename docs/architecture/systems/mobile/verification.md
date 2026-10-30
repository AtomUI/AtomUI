# Mobile 验证架构

> 状态：预实现架构。本文定义已批准的目标契约，不表示当前仓库已经包含或发布 `AtomUI.Mobile.Controls`。

Mobile 验证按 Contract、Headless、Visual、Platform 和 Release 五层组织。某一层通过不能替代更高层证据；iOS 与 Android
分别记录，平台 Host 和测试项目实际存在前不写虚构命令。

## 五层验证

| 层级 | 证明内容 | 不能证明 |
| --- | --- | --- |
| Contract | Public API、默认值、Token key、pseudo-class、TemplatePart、Semantic Part、event、关闭原因 | 视觉、触摸和平台生命周期 |
| Headless | 状态机、属性优先级、选择模型、导航事务、手势仲裁、资源释放 | Simulator/Emulator 或真机体验 |
| Visual | Light/Dark、字号缩放、方向、Safe Area、键盘和关键状态基线 | 系统返回、IME、屏幕阅读器真实性 |
| Platform | Simulator/Emulator 和真机触摸、IME、返回、生命周期、无障碍 | Release 裁剪和发布产物 |
| Release | iOS Release/AOT 与 Android Release trim/AOT 的构建、安装和启动 | 未覆盖设备的全部体验 |

## 双平台证据

| 证据维度 | iOS | Android |
| --- | --- | --- |
| Build | 目标 TFM/RID 编译 | 目标 TFM/ABI 编译 |
| Virtual device | Simulator 场景 | Emulator 场景 |
| Physical device | 触摸、旋转、IME、前后台 | 触摸、旋转、IME、Activity 重建 |
| Accessibility | VoiceOver | TalkBack |
| Release | AOT 包安装和启动 | trim/AOT 包安装和启动 |

iOS-first 允许实现时间不同，不允许用 iOS 证据填充 Android 列。公共 API 只有在两个平台的对应验证路径存在后才能标记稳定。

## 无障碍

- AutomationPeer 暴露正确 role、name、value、state 和 action。
- Modal/Overlay 具有焦点圈闭、返回/escape 和正确阅读顺序。
- VoiceOver 与 TalkBack 分别在真机验证。
- 系统字号缩放不造成截断、重叠或焦点目标消失。
- platform profile 提供 iOS 44 pt、Android 48 dp 的最小触控目标策略；Control 不直接判断平台。
- Reduce Motion 保持终态、焦点和事件顺序，只降级非必要动效。

## IME 与输入

至少覆盖英文、中文和日文组合输入，以及 emoji、secure input、return action、selection、焦点切换和键盘反复显示/隐藏。
Input、TextArea、SearchBar、PasscodeInput、VirtualInput 和 NumberKeyboard 共享明确的 IME ownership 与 viewport 更新规则。

输入验证继续以 `DataValidationErrors` 为最高优先级。平台 IME 错误、业务异步错误和表单验证错误必须保持不同 owner，不能
互相覆盖或被 Overlay 关闭吞掉。

## Viewport 与布局

覆盖：

- 刘海、圆角、system bar 和零 inset 退化。
- 横竖屏、iPad 多任务和 Android multi-window。
- 输入面板打开、关闭、切换输入法和遮挡变化。
- 字体缩放、display scale 和宽/窄 viewport。
- Overlay 与页面同时响应 snapshot 更新。

平台状态变化必须在下一次有效布局前统一进入 `MobileViewportContext`，不能由 Control 使用 timer 或轮询补偿。

## 资源与生命周期

- Overlay、Picker、ImageViewer 和虚拟列表执行 1,000 次压力循环后不保留 session、owner、template part 或页面。
- `OnApplyTemplate`、detach、owner 变更、container recycle 和 host detach 都有对应释放验证。
- 进入后台暂停 transient timeout 和非必要动效；恢复不重复完成 Task 或 push 页面。
- Android Activity recreation 与 iOS host recreation 使用同一恢复不变量。
- 不允许 static event、进程级 mutable singleton 或 timer 持有 TopLevel 和 Control。

## 性能

- Drag、scroll、pinch 和 animation sampling 热路径不产生逐帧托管分配。
- 60 Hz 基线设备的稳定交互 p95 frame time 目标不超过 16.7 ms。
- 标准场景不允许未归因的 50 ms 以上主线程停顿。
- 每个波次记录包体、启动、布局和分配基线；超过 5% 的变化必须解释。
- 性能声明必须附设备、构建类型、场景、采样方法和日期。

## AOT 与 Trimming

- 内置注册使用显式 descriptor 或 Source Generator，不扫描程序集。
- 不新增 ReflectionBinding、字符串成员访问或动态 factory。
- Analyzer 通过不能替代真实 iOS/Android Release publish。
- Adapter、Token、Localization、ControlTheme 和 DataTemplate 在裁剪后仍可发现。
- Release 产物必须实际安装和启动，不能只保留 publish 成功日志。

## 单能力完成证据

一项能力只有同时具备以下证据才可以从兼容清单提升为完成：

1. Public API 与能力映射已经评审。
2. Contract 与 Headless tests 通过。
3. Light/Dark、字号缩放和关键状态视觉基线通过。
4. 触摸、手势、键盘和返回行为通过适用场景。
5. AutomationPeer 与屏幕阅读器证据存在。
6. iOS 与 Android 对应平台状态分别记录。
7. Release/AOT/trim、资源释放和性能门禁通过。
8. Gallery ShowCase、API、Token 和 Control 文档同步。

## 当前验证状态

当前没有 `AtomUI.Mobile.Controls` 项目，因此本文件只定义验证不变量，没有 Mobile Contract、Headless、Platform 或 Release
通过结论。现有 Desktop Control 测试或 iOS Demo 环境经验不能替代上述 Mobile 证据。
