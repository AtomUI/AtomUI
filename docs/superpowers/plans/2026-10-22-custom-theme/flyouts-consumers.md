# CT15 Flyouts 消费执行（2026-11-01）

输入4metadata/11Own：FlyoutHost、FlyoutPresenter、MenuFlyoutPresenter、TreeViewFlyoutPresenter；Own为FlyoutHost5与TreeViewFlyoutPresenter6，保持原137/274。Own未决3仍仅GroupBox，不能凭这里无NeedsDecision宣称运行完成。Tour已完成工程/清理后限定验证、44对视觉待审；其原异常/缺帧及其他边界保持。构建与采样始终非阻塞独占共享run.lock，不并行写产物。

FlyoutHost5：MarginToAnchor、OverlayHostShadow、PopupRootShadow、HorizontalOffset、VerticalOffset。前三已有公开host主题真实Setter，Motion Mid/EnableMotion复用现有Owner资源。Horizontal/Vertical不在host主题Setter，真实路径是MenuFlyout/TreeViewFlyout的pointer placement offsets；Flyout.BindPointerPlacementOffsets/CreateGlobalResourceBinding与Style优先现成机制需复用，不把字段当无消费者或重接到普通placement。须先4算法44默认/type/key/slot冻结，两Own定义分别V2R1且保持原四shadow/尺寸公式。

TreeViewFlyoutPresenter6：背景、内容padding、min120/max800宽、min ControlHeightSM×3/max×30高、Padding(UniformlyPaddingXXS,BorderRadiusLG.TopLeft/2)。实际树内容/原高度约束/滚动条/裁剪和pointer placement须保留，不以去height/max约束避开输入问题。

归属先核：ToolTip/PopupConfirm限定表与CT13/14/其他闭合家族共享Flyout机制不能重写；具体Host/Presenter的字体、Global、semantic root/arrow/container及跨visualroot资源/Native优先/动态退出仍按本包实际反证补。公开owner与共享composition素材必须分清，不让公共leaf的独立身份无故被其他家族覆盖。44默认通过也不等于完整家族完成。

下一：读四控件正式文档及既有批准/模板/测试，先取得普通/Pointer两宿主、明暗Compact/RTL、Own/Global/原生字体、专用Style、触发/关闭/焦点/键盘/滚动与资源退休的有效前证；再复用机制修必要消费者。正常Skia旧新/AOT同完整历史、独立A/A/1DIP与中间恢复，原失败留账；临时清理后精确模块/NET8、CSV/总表/作者自审和具体限定视觉审查。无全量/提交/分支/worktree/子agent。

当前结果见[执行账本](flyouts-consumers-results.md)与[作者自审](flyouts-consumers-review.md)。第一轮三版揭示实际Popup热偏移/Host重模板断言缺口，补有效RED后修复，40聚焦用例通过；最终重放使用补强同轨迹，原批不与最终批混算。尚未清理及最终验收，不能写家族完成；下一[Collapse2/15](collapse-consumers.md)只读准备。


清理后最终验收（2026-11-01）：共享锁空闲后独占执行，Flyouts既有17PASS/0FAIL/0Skip（用例760ms，命令31.37s）；NET8 Desktop Release 0警告/0错误（构建19.83s，命令20.01s）。23临时/3源25325字节、1191取证、320driver/两publish各15、488原会话文件已清理，最后6文件及final marker也删除。git diff --check通过。48对及列明有效默认纠正待限定批准，原40失败/52缺帧、4恢复micro/12跨运行定时、平台/完整出口及CT15保留；不改CT12/13/14批准及CT25/FW1，无提交。
