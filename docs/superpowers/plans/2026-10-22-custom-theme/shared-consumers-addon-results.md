# CT-10 共享附加区域与尺寸图标消费

日期：2026-10-23；基线 `5381fb6c9`，现有 `feature/custom-theme` 工作树。主 agent 独立实施、作者自审，不创建分支/worktree、不提交。本包关闭共享资产中已确认的消费缺口；全 owner 根排版和 CT-10 总出口仍按家族证据回填。

## 输入、真实节点与归属

| 路径 | 本包消费与实际边界 |
| --- | --- |
| AddOnDecoratedBox | 29 处 Shared 改用既有 OwnerTokenResource：背景/描边、三档 padding/行高、内容间距、后缀前景及 error/warning/disabled 文字和图标色；模板、状态机、优先级不变 |
| ButtonSpinnerContentPanel | 一处 SpacingXXS 覆盖同步使用实际 owner，避免派生模板覆盖共享修复 |
| SizeTypeAwareIconPresenter | 自身主题提供 Width/Height 基础和三档默认，模板 IconPresenter 只通过既有 TemplateBinding 消费；Custom 不再命中 Middle 预设 |
| 公开 IconPresenter | 保留自身 Control Global 画刷身份；显式 PathIcon 宽度/前景优先，清除宽度后重新消费包装层尺寸 |

实际外侧 addon owner 为 LineEdit、SearchEdit、ComboBox、Select、Cascader、TreeSelect、DatePicker、RangeDatePicker、TimePicker、RangeTimePicker、NumericUpDown 和独立 ButtonSpinner。TextArea 没有外侧 addon，不以不存在的节点制造产品失败。

两项树拓扑判定：SearchEdit 的 ContentLayout 是 DockPanel，没有 HorizontalSpacing 合同；其搜索 Button 也有自己的 PART_ContentPresenter，观测主内容必须同时限定 TemplatedParent。NumericUpDownSpinner 虽继承 ButtonSpinner，却是未独立注册的 internal 组合节点，实际 OwnerTokenResource 继续到 NumericUpDown。公开 ButtonSpinner 则使用自己的 identity。静态目录中的候选归属不替代实际运行路线。

尺寸默认放内部包装层，避免从公开 IconPresenter 的 self 错取外层 owner；不修改公开子控件身份，不新增 API、Own、binding 类或代码订阅。新增资源关系均由原生 AXAML binding 持有，detach/reparent 更新沿既有 OwnerTokenResource，释放由原生绑定负责。

## 红绿与生命周期

临时用例位于 `tests/AtomUI.Desktop.Controls.Tests/Temporary/CustomThemeSharedAddOn/`，精确命令：

```sh
dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter FullyQualifiedName~Temporary.CustomThemeSharedAddOn --nologo -v:minimal
```

原始背景仍为 `#05000000` 而非配置 Magenta，间距仍 4 而非 13，主内容行高仍 22 而非 37；图标三档仍 12/14/16 而非 21/25/29，Custom 显式 31×27 被内部 14×14 压制。初始 TextArea 外侧 addon、SearchEdit 面板类型/同名 presenter、Numeric 内部节点身份等夹具错误均已纠正，不计入产品失败。

最终同一套 20 项用例，在仅将本包三份 AXAML 还原到 HEAD 时 **19 FAIL / 1 PASS，1 秒**；恢复候选后 **20/20 PASS，1 秒**。三份产品源码逐字节恢复。独立正对照是正常释放；不把已有生命周期行为虚报为本次修复。

验证包括 12 owner 的真实 surface/描边/适用间距/行高、同实例配置/清除、SearchEdit 兄弟隔离、实际 error 色、padding 17,9、三档图标及 Custom、同 Context 包装图标 ButtonSpinner25→LineEdit31→25、公开 IconPresenter 独立 Blue 与显式 child Gold/41 的优先级，以及长寿命父节点移除后 provider/owner/frame/wrapper/icon 五个 WeakReference 均可回收。释放夹具必须推进真实 Headless render tick；仅 RunJobs 不证明 compositor 已处理移除。

## 视觉与兼容

自驱动外部 `Visual.csproj` 引用本工作树 Desktop 源码，实际 Skia/HeadlessDrawing=false、Avalonia12.1.4.0、1160×1100、scale1。56 控件覆盖 13 类 frame owner×四种尺寸及四个实际附加图标；四种 variant、disabled、error/warning 和配置往返采用完全相同轨迹。

重放为 `dotnet /private/tmp/atomui-ct10-addon-visual/bin/Debug/net10.0/Visual.dll before|after light|dark <受管 artifacts 目录>`。旧源码只反向本包三份 AXAML，结束后逐字节恢复。A/A 与 1 DIP 检错使用独立窗口，主场景不受扰动；全帧零容差，不裁剪、对齐或遮罩。

临时宿主最初使用 Avalonia Window 的白色默认背景，暗色文字不可见；该轮视觉不作为暗色验收。宿主改用实际 Shared.ColorBgLayout 后，两侧重新采样。以下为修正宿主后的最终数值。

修正后的明暗各 14 组旧默认→新默认，合计 **28 组全部 0 像素**。A/A 两种主题均 0，独立 1 DIP 检错各 6 像素、回位各 0。配置实际改变 Light **66780** / Dark **23998** 像素，同实例清除各 0；旧/新同恢复阶段也各 0。已查看明暗完整默认、配置、error 关键帧；根内容、addon、四尺寸、handle 与裁剪位置均符合对应原始矩阵。

当前 Mac osx-arm64 NativeAOT 实际运行相同 56 控件/完整轨迹，两种主题均 PASS；包括实际已物化图标配置尺寸和清除。与 JIT 对照明暗各 15 阶段，合计 **30 张全帧全部 0 像素**；已查看暗色 AOT 配置完整图。发布没有 IL2026/IL3050；保留本机 Homebrew OpenSSL/Brotli 最低 macOS26 与目标12 的 linker 提示，不据此宣称旧 macOS 或其他平台通过。

兼容影响仅针对原来失效的控制级 addon 配置与 Custom 包装层显式尺寸。默认框体、handle 预留、输入/日期/选中状态、variant 和 public 子 Control 身份保持；本包没有修改交互事件、裁剪、滚动或已有默认值。根 FontFamily/FontSize/Weight/Style 的全部 owner 验收、Own V2 和完整家族状态不由本页替代。

## 处置与最终出口

20 项临时验证源码已删除；外部驱动及 bin/obj 共 424 个文件、受管媒体与元数据 113 个文件已删除，媒体 audit clean、audit_error=null。没有残留本次进程。全局常驻用例净新增 0。

清理后第一次 focused 为 **295 PASS / 1 FAIL，13 秒**。失败的现有 `TreeSelect_Multiple_Empty_Small_Uses_Small_Content_Padding` 用源码正则锁定旧 `Padding` Setter；该测试与 TreeSelect 产品文件在本次修改前均与 HEAD 一致，前序 S3 已将内部默认移为 PresetContentPadding。本次只把既有 1 项测试改为真实 Small 空多选的根/实际框体 **7,0**，显式 **17,9** 生效与清除恢复，单项 **1/1 PASS，608ms**；没有修改 TreeSelect 产品源码，没有新增常驻用例或 helper。此断言保护资源默认与真实 frame 接线，编译器不能检查其效果；沿用既有全局 contract 用例，净新增 0。

清理后最终 34 类 focused：**296/296 PASS，10 秒**；Desktop NET8 Release：**0 warning / 0 error，21.92 秒**。实际范围包含共享 AddOn、ButtonSpinner、NumericUpDown、输入/选择/日期时间的相关布局/状态/原生覆盖/专用Style、尺寸合同及图标画刷；没有运行 change/affected-wide/full。验证入口如下（使用已由清理后构建产生的 Debug/net10.0 测试程序集）：

```sh
dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-build --no-restore --filter 'FullyQualifiedName~AddOnDecoratedBoxVariantTests|FullyQualifiedName~AddOnContentTemplateTests|FullyQualifiedName~ButtonSpinnerRootSurfaceTests|FullyQualifiedName~ButtonSpinnerSpacingTests|FullyQualifiedName~ButtonSpinnerSemanticPartTests|FullyQualifiedName~NumericUpDownRootSurfaceTests|FullyQualifiedName~NumericUpDownSpinnerLayoutTests|FullyQualifiedName~NumericUpDownSpinnerPrefixLayoutTests|FullyQualifiedName~NumericUpDownAddonIconTests|FullyQualifiedName~NumericUpDownSemanticPartTests|FullyQualifiedName~TextInputRootBrushRelayTests|FullyQualifiedName~SearchEditBehaviorTests|FullyQualifiedName~SearchEditLayoutTests|FullyQualifiedName~SearchEditSemanticPartTests|FullyQualifiedName~LineEditSemanticPartTests|FullyQualifiedName~TextAreaSemanticPartTests|FullyQualifiedName~TextBoxVisualStateTests|FullyQualifiedName~ComboBoxRootSurfaceRelayTests|FullyQualifiedName~ComboBoxSemanticPartTests|FullyQualifiedName~SelectRootSurfaceRelayTests|FullyQualifiedName~SelectBehaviorTests|FullyQualifiedName~SelectSemanticPartTests|FullyQualifiedName~CascaderThemeContractTests|FullyQualifiedName~CascaderSemanticPartTests|FullyQualifiedName~TreeSelectThemeContractTests|FullyQualifiedName~TreeSelectSemanticPartTests|FullyQualifiedName~TreeSelectBehaviorTests|FullyQualifiedName~DatePickerBehaviorTests|FullyQualifiedName~DatePickerSemanticPartTests|FullyQualifiedName~TimePickerBehaviorTests|FullyQualifiedName~TimePickerSemanticPartTests|FullyQualifiedName~CustomizableSizeTypeContractTests|FullyQualifiedName~ControlEffectiveGlobalBindingTests|FullyQualifiedName~IconPresenterBrushTests' --nologo -v:minimal
```

作者另起只读审查步骤：三份 AXAML 的默认/owner/公开子身份/Custom 与显式值、原生订阅释放、真实主内容观测、缓存影响和测试生命周期符合本包合同；无新增产品 C# 类型/字段/订阅，成员排列与初始化顺序不变。本包未处理 Critical/Important/Minor 均为 0。139 个修改文档内的本地链接均有效，两个目录表主键集合保持，verification audit 无错误，git diff --check 通过。没有独立 reviewer、分支创建、提交或全量回归。

本包完成；CT-10 整体仍进行中。全 owner 根字体、完整 Global/Own 字段、其他家族/平台及 CT-25 性能和原生显示面证据继续按共享计划的 owner 表回填；不将共享分包或本机 AOT 等同于全库完成。
