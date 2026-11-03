# CT15 Drawer 消费者执行计划（2026-10-31，准备阶段）

输入3metadata/13Own：Drawer、DrawerContainer、DrawerInfoContainer；Token13字段全有源码消费者，当前不是运行验收。沿当前feature/custom-theme与工作目录，保留既有改动，不创建分支/worktree/子agent，不提交/全量。FloatButton正在独占共享产物三版采样，本文仅只读准备，不修改产品或并行构建。

## 真实边界与复用

Drawer为Control，运行时容器ContentControl，Info为HeaderedContentControl。容器由Drawer真正LogicalChildren管理并挂ScopeAwareAdornerLayer，复用CT11/现有OpenOn迁移/关闭释放/父子push及Window chrome suppression lease；不重新研发已获批准共享机制，不改变标题栏、visible-frame、滚动嵌套或原始内容。八专用Semantic Style route为跨根`>> .semantic-*`，沿现有CrossRootsChanged与ThemeVariant relay验证，不能code-behind改part颜色绕过。

13 Own：四方向BoxShadows、HeaderMargin/FooterPadding、三档Dimension378/520/736、CloseIconPadding/Margin、ContentPadding、PushOffsetPercent0.4。尺寸在Drawer.ApplyDialogSizeTokenBinding按Small/Middle/Large及Custom fallbackSmall原合同绑定，EffectiveDialogSize保留绝对/百分比和OpenOn visible frame解析；不能默认把Custom改成另一语义。PushOffset活跃嵌套Drawer真实计算需独立验证。

## 执行顺序及验收点

1. 冻结原源及四算法52条默认/type/key/slot；先取原有效值、实际open容器/字体/owner/专用Style/热Global的可失败反证。原body为固定宽高滚动原长内容，保留Title/Extra/Footer、Close/Mask、四placement、嵌套父子与两OpenOn。
2. Token迁V2R1保持全部默认；基于实际有效角色复用公开Drawer Global owner代理/字体继承机制，私有容器不伪造注册identity。Owner资源/anchor与真正逻辑membership热更新，原生Local/动态绑定保留。
3. 八专用AXAML Style静态形态与实际节点命中；Global背景/Mask/标题字体字号/图标、Own13热通知，三档/Custom/百分比/Native优先。只做有前证的修复，ControlTheme能等价表达则声明式；跨树运行时计算兜底注明原因并拥有释放句柄。
4. 保留真实输入：关闭、mask禁用/钉住、Escape及焦点/键盘原合同，嵌套scroll/clip/viewport、push与关闭恢复、OpenOn切换、模板/挂载/主题动态切换与退休资源。原几何/逻辑/视觉祖先与命中链先核，不以关闭motion/去heightlimit/删内容消差。
5. 正常Skia原版/候选/AOT同历史、四算法、静态稳定与持续态定时帧、独立A/A及1DIP检错；同坐标RGBA零通用容差。原始失败/恢复非零先记账，未知来源不擅归CT25/FW1，OS/Browser无证据保持未验。
6. 查看关键图与差异后同步五正式文档、两CSV、总表/剩余表及作者另步自审。删除临时验证/专用产物/媒体，再精确Drawer模块与NET8；全量不自行运行。仅具体限定有意视觉变化可申请批准，不把工程或分包通过当完整Drawer/CT15关闭。

## 当前只读发现

- 旧whole CalculateTokenValues，四shadow常量与九其它字段默认均清楚，未做迁移。
- Drawer容器MotionDuration已取DrawerTokenResource.MotionDurationSlow，是CT11既有修复；Mask背景、brush transition与Info标题/按钮视觉仍Shared路径，须先证明局部Global未到实际目标。
- Drawer无字体public wrapper，不能按TemplatedControl盲加FontSize；须读实际继承值并选择现有TextElement attached/可选Override合同，公开API扩张不是默认解。
- 当前三个metadata的NeedsDecision/StaticChecked是目录状态，不是13Own中另添未决字段。原NeedsDecision5（Float两、GroupBox三）与137/274不变。


既有证据复用：ControlEffectiveGlobalBindingTests.Motion_Binding_Uses_The_Control_Effective_Global_Token已验证Drawer EnableMotion来源；DrawerContainer.MotionDuration Slow、OpenOn的BindVisualAncestor、ThemeVariant relay、Visible frame/chrome lease、嵌套关闭/placement回放与跨根八Part已有各自契约，须复用而不重写。当前缺口是正文/标题/Mask等局部Global真实视觉、52默认V2、所有Own和尺寸/动态scope/原生优先的逐目标取证；不把既有Motion通过当完整家族完成。


## 首轮有效反证与接入

临时探针前两次为字符串/using/命名空间/nullable编译错误，已修正，不算产品RED。原实现10例中4默认导出PASS、6 Global目标FAIL：容器Family仍$Default、Size12而非Menlo/31；Surface白而非#123456、Mask#73000000而非#3300FF00、Title16而非29、Close14而非31。四算法52条默认/type/key/slot及原字体root/container/body12、Black已冻结。

候选复用ControlTypographyBindings与真实InheritanceParent桥，在Drawer ControlTheme声明可选TextElement Family/Size；无显式覆盖继续原继承12/Black，不擅加新正文颜色角色。Container与Info通过现有IControlThemeGlobalOwnerSource归公共Drawer，Info模板Shared语义值按anchor换Owner，Mask归自身代理；现有CT11 Slow/Mid与完整LogicalChildren/lease/ThemeVariant路径不重写。Token13迁V2R1保留所有原shadow表达式、尺寸/边距常量与顺序，未加新Token。

首个候选遗漏接口namespace/精确返回类型导致编译失败，已按既有实际InheritanceParent签名修正，不计有效反例。最终首轮47PASS/0FAIL/0Skip（10临时+37既有，12s），四算法52默认/type/key/slot逐字相同、四组默认字体记录相同。当前只是接入切片；Own13真实分支、八专用Style、尺寸/动态/输入/退出、成员gate、Skia/AOT、清理后验收与限定批准均待，完整Drawer/CT15不关闭。


限定批准登记：2026-10-31，用户明确回复“批准65对及两项Reserved，其余保留”（request_user_input_async / call_e7be1ded2cd04cea8d90b50314dba528 / item 0）。仅FloatButton表内65对明确静态（含列明原窄宽换行/窗口上缘裁剪）与CircleBadgeOffset/SquareBadgeOffset两项Reserved兼容分类获批准；两字段仍无scalar消费者，不改写成Active/ConsumerVerified，不缩分母。NeedsDecision由5降为3，仅余GroupBox三字段。其余115旧新非零、34跨运行非零、恢复/平台/GC/极值及完整FloatButton/CT15保留，CT12/13/14既定批准和CT25/FW1不变，不授权提交。


## 正常Skia取证合同

准备独立Headless+真实Skia驱动，官方12.1.4、region dirty rect、普通motion及原800×600窗口/Scroll240×180/extent1400与Original title/extra/footer/content全部保留。37主静态状态+8定时open/close+4独立cal状态，每算法49帧，三版完整相同历史；静态≥750ms及12同RGBA，timed0/64/128/320记实际时刻。同坐标RGBA零通用容差。

轨迹覆盖专用Style与clear、Own四placement/三档/Custom/Native绝对及百分比、Global/Native字体、RTL、两层重模板、重挂、原滚动内容同实例、嵌套push与Native/关闭恢复、真实wheel、Pinned关闭按钮/Mask、focus+Enter关闭、原Escape保持打开、Mask关闭与最后重开/恢复。键盘/指针必须走框架输入与实际命中，不能把直接IsOpen赋值当输入证据。驱动准备阶段若误复用已有视觉父级，先修探针并如实保留，不改产品迁就。

Drawer运行时根在carrier之外，校准只在独立实例的私有Container视觉根受控扰动RenderTransform 1DIP，先断言null且未本地设置，之后ClearValue精确恢复；这是检错实验，不是发布部件定制。八Part定制唯一路径仍专用AXAML Style，不以代码改部件属性。当前尚未采样/比较/清理，不先称视觉通过。


## 2026-11-01 可见缺陷下一步（先原条件RED）

待当前原/候选/AOT预修复三版完成，先按378宽、原Original title/extra、Font23/Title29/Menlo记录HeaderText绘制宽度与Extra真实位置、完整祖先clip链及RTL；项目TextBlock默认ClipToBounds=False，但不修改该共享默认。候选最小方向是Drawer标题局部省略/裁剪，保留Title完整属性与原Extra/尺寸/高度预算；实现与限定视觉需单独证据，未现在自批。

正常motion已打开时重Container模板及其后Info模板，原/候选均仅剩mask，需新增actor.IsVisible/Opacity与原180/1400预算反例，保留原模板更换及输入顺序。下一局部处理模板当前状态回放和旧actor运行所有权，不能靠强制reattach重建或隐藏输入触发规避。原196×三版与47/49先行帧、原8中间失败全部留账，修复后完整三版同历史重新重放；不将整Global画面或旧新0当正确体验。


清理后最终验收（2026-11-01）：空闲run.lock独占后精确Drawer37项+既有Motion Global1项共38PASS/0FAIL/0Skip（用例1s，命令31.17s）；NET8 Desktop Release 0警告/0错误（构建17.97s、命令18.15s），git diff --check通过。独立最终4文件及marker已清理，原临时/1518取证/550会话均无残留。不跑全量、不新建分支/worktree/子agent、不提交。20对静态待限定批准，其余定时/OS/Browser/GC/极值与完整Drawer/CT15保留，下一Tour7/9。


限定批准登记：2026-11-01，用户明确回复“批准表内20对，其余边界保留”（request_user_input_async / call_cf78d1ab3bc3431289c5246103fe223e / item 0）。仅Drawer表内20对明确静态变化获批准；其余原失败、缺帧、恢复/持续态、平台/GC/极值和完整家族/CT15继续保留，不授权提交。
