# CT19 B08：渐变节点文字动态颜色

既有总计划 B08 要求：已有 GradientColorPicker 保持 ActivatedStopIndex 不变，修改前景、禁用和主题时，实际文字读取当前状态。沿用既有主题设计和用户 CT11–CT26 研发授权，CT24 暂缓。没有新公开 API 或品牌设计。

## 范围与设计

GenerateValueText 创建 WrapPanel 的运行时 TextBlock，HandleGradientActiveStopChanged 直接写本地 Foreground；它只在 index 变化时执行。disabled selector 错指 TextBlock#PART_ColorTextPanel。

采用 AXAML 优先：代码只维护真实节点的内部选择 class；ControlTheme 为文本区绑定 owner Foreground，active 标记和 disabled 分支声明颜色。新建或复用文字在生成结束后投射当前 index。正常内容继承 panel 的语义 Style 前景，显式 formatter 本地值按 Avalonia 优先级保留。空值不携带旧 active 标记。

备选是监听颜色和禁用变化后重写本地值；这仍阻断 Semantic Style 优先级且重复声明颜色，不采用。逐 TextBlock relay 属于可由主题表达的绑定，同样不采用。不修改默认选中语义（选中节点按现行 ColorTextDisabled），不调整输入、弹层或尺寸。

## 文件与任务

1. Temporary/CT19GradientText：精确执行已有 index 下前景、disabled、主题变化、生成后 index、clear、隐藏恢复；保留真实产品模板。原源码运行并记录有效失败；不把编译错误当 RED。
2. GradientColorPicker.cs 与 Themes/GradientColorPickerTheme.axaml：只修改上述断链；内部闲置 snapshot 属性可删除，public/protected 契约不变。不混入成员布局重排。相同测试 GREEN，检查 formatter 本地优先级、专用 description Style 和旧 part、owner 释放。
3. 原版与候选 Light/Dark 默认、active、前景、disabled、恢复等同轨迹真实 Skia 像素；独立 AA、1DIP 正对照，必要变化单列请审查。真实 Mac managed、NativeAOT 定向功能检查，未覆盖平台不填通过。
4. 同步现有 ColorPicker implementation/changelog；不改冻结30文件。删除本次临时源、专用工具、工程和媒体（媒体有待批准时明确保留），运行精确 ColorPicker 相邻测试、NET8与 diff 检查。作者自审，不自动提交；待用户审查。

## 已知边界

此切片不代表24项 Own 默认求值、完整颜色输入、渐变、scope、尺寸矩阵或 CT19、CT25完成。原始 API 的 SetPresetColor formatter 委托形状不在本修复中变更。
