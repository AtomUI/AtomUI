# CT-15 P1 Timeline 部件与默认输入结果

2026-10-24，在现有 `feature/custom-theme` worktree、HEAD `5381fb6c9` 及之前各包未提交改动上执行；仅主 agent，无新分支/worktree、提交、CUA 或全量测试。P1 不代表 CT-15 或完整 Timeline 家族完成。

## 修复与合同

- L03：删除 Arrange 写入 dot BorderThickness/CornerRadius 的本地值，默认值放 ControlTheme Setter；生成的 `TimelineItemIconStyle` 真正覆盖描边／圆角。默认盒边长仍为 dotSize + borderWidth，中心、rail 溢出和 ClipToBounds 保持。图标宿主按自身 DesiredSize 排列，专用 border 进入实际测量盒。
- L04：删除 OnApplyTemplate 的 `IndicatorColor ??= DefaultIndicatorColor`，dot／IconPresenter 用 AXAML MultiBinding 实时选择明确色或默认色。内部 Global 从实际公开 TimelineItem 取值，Timeline Own 几何属于 root；独立 primitive 使用 Shared 回退。三处 internal AvaloniaProperty 通过有限静态引用桥接供 XAML 使用，不反射、不新增注册或代码样式绑定。
- Timeline 11 字段迁移 DependencyDefaultsV2 Revision 1。ItemPaddingBottomLG 读取有效 Own ItemPaddingBottom，明确 LG 仍优先；其余保留原 Global／常量公式、名称、类型、资源 ID 和 slot。
- T19：LastItemContentMinHeight 成为明确 Own 的 OptionalOverride，适用于两方向最后可见项。未配置的原生实际 MinHeight=0 与历史资源默认 48 同时保留，不自动改变布局；73／明确 0／清除 0 和专用 content Style 81 的优先级均验证。
- 六处成员纯移动保留原访问级别、注册顺序和值，逐块 SHA 与非空白行集合证明无方法体变化，再进行行为修复；最终成员审计 3 文件／1 控件／0 问题。

## 失败前证与运行检查

真实编译的 AXAML fixture 只使用生成的 TimelineItemIconStyle／TimelineItemContentStyle，没有 Name／Loaded／Unloaded 定制回退；Part 查找仅用于读取断言。修改前最终 10 项为 **8 FAIL／2 正对照 PASS，727ms**。失败覆盖 dot 样式被覆盖、图标宿主错误盒、热更新及公开 Item Global owner、两方向末项高度、普通／倒序 pending Own 依赖。

候选与原有 Timeline 合计 62 项通过（1s）。扩展检查图标色、同项重挂 scope／兄弟隔离、保留 provider 时移除 graph 的四个 WeakReference，以及模板重建后的实时色；最终临时 **14 PASS，838ms**。重建只通过 Template 生命周期 API，不对 Part 设置颜色或样式。首次嵌套 TemplateBinding 推断及 internal x:Static 编译失败均是实现过程诊断，不作为产品失败前证。

## Skia 与 NativeAOT

真实 Skia，UseHeadlessDrawing=false，正常 motion，1400×740／scale 1；两方向 × Start/End/Alternate 六根、22 项，含 label／无 label、普通／倒序 pending、自定义图标和原始高度约束。公开 action 的真实鼠标输入命中一次，A→B→A 保持所有 item 引用。

Pending 的 AtomUI Icon 会被创建路径强制 Spin，首次用 ClockCircle 作静态 pending 的 fixture 因此不稳定，未接受为基线。静态矩阵改用产品支持的原生 PathIcon 分支，保留 pending/reverse；默认循环另作实际动态观察，没有关闭动效或冻结 clock。

| 对照 | 实际结果 |
| --- | --- |
| 同版本 A/A、1 DIP 恢复、配置清除 | 均 0 RGBA 像素差异 |
| 仅移动 1 DIP 的检错 | Light 20,827／Dark 20,715 像素 |
| 旧新默认／A/A／移动／恢复／清除／实际输入 | 12 张均 0 像素差异 |
| 专用 shape border/radius=1 的预期修复 | Light 1,658／Dark 1,802 像素；实际 Setter 命中 |
| 明确 Item Global Primary Lime + root LastMin73／Padding7 | Light 168,152／Dark 167,990 像素；末项73、pending14及颜色通过实际属性检查 |
| 同实例后赋公开 Magenta／清除 | 实际 dot 颜色跟随，明确 Orange 图标保持；回到原始完整画面0 |
| 未配置 11 Own 的 ID／slot／值 | 两主题共22行，旧新 Compiler 与 AOT逐字相同 |
| 当前 Mac osx-arm64 NativeAOT 对 JIT | 18 张完整 RGBA 均0，实际 Style／颜色／Own／输入断言通过 |
| 默认 pending Spin 实际运行 | Light 4481ms／Dark4465ms，各23帧均不同，1s周期配置和布局保持 |

默认值表 SHA256：Light `ec33cc9ed7710c4491e363310aa0929fd0a30b6767e77c9bb7f5374e2f006cd9`；Dark `69cfbe303af62b9dbb92392a07bef394d3eae2e35896455ba5e47615ceb1cfd4`。已实际查看默认／配置／shape／spinner 关键画面，无遮罩、对齐修图、容差或布局回避。默认 Dark 标题／内容仍为旧版黑色；P1 保持该画面，不宣称解决排版／可读性。

AOT 串行 publish 并实际启动；无 IL 警告，五条已知 Homebrew dylib 的 macOS 最低版本 linker 警告单列。此为当前 Mac Headless/Skia + NativeAOT，非系统最终显示面或其他 OS 证据。Spinner 证明正常循环仍运行，未宣称完成同时间原点的全动效轨迹兼容验收。

## 清理与最终验证

已删除 14 项临时验证所在 2 文件、外部驱动 428 文件、4 份精确源码备份所在 JSON；65 个媒体／默认表／manifest 文件按 evidence cleanup 审计后全部删除。只保存文本数值与日志，净增永久测试为0。

清理后 Timeline 精确 filter **52 PASS，979ms**；NET8 Desktop（覆盖 Common）Release **18.43s，0 警告／0 错误**。作者自审为本单 agent 任务的审查边界，不冒充独立审查。

## 未完成范围

P2 的 root／title／content 字体及其他 Global、逐 Own 定制／极值、Compact／RTL 与完整业务状态继续；原生最终显示面和其他平台归 CT-25。不得把 P1 的六根矩阵换算为137项 CT-15 范围或完整家族通过；总进度见 [tasks](tasks.md)。
