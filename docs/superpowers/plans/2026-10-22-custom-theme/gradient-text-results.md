# CT19 B08：渐变触发文字审查记录

本切片处理既有 B08 的固定选择下动态颜色和恢复，不属于待批准30文件。CT24继续暂缓。完整 ColorPicker Own 默认、输入、尺寸及 CT19、CT25 仍开放，未自动提交或发布。

## 合同和实现

渐变值、活动节点和文字内容保持原有职责。正常文字读取 owner 当前前景，默认来自该 GradientColorPicker 的 Effective Global；活动节点保留 ColorTextDisabled 的弱化语义；禁用、作用域和主题变化即时投射。formatter 明确设置的本地颜色遵循原生优先级，移除或替换 formatter 时先退役其可带任意本地值和类型的输出。

原版在节点选择变化时把画刷写入本地值，后续更新不再生效；disabled selector 还将实际 WrapPanel 写成 TextBlock。候选只同步内部活动标记，颜色放在 ControlTheme。运行时文字不在模板父链中，节点样式声明在 WrapPanel.Styles 的逻辑子树作用域；description 专用 Style 仍作用于面板，不用 code-behind 设置 Part 颜色。

新增专属颜色反例还证明 SharedTokenResource 只取作用域全局值，不能表达此处的公开 owner。三个颜色入口使用既有 OwnerTokenResource；没有新增反射、资源旁路、公开 API、Token、Part 或字段。

## 有效反例与功能

- 初始8项在原源码全部失败。补齐 formatter 退出、专用 description AXAML、隐藏恢复及同实例整图恢复后，原源码15项中14FAIL、1PASS；通过项是完成真实渲染退役后的节点释放保障。
- 初版释放探针在尚未提交渲染时失败；提交实际渲染帧后，无产品生命周期改动即通过。不把该早期探针当泄漏证据。
- 专属颜色反例在前一候选读到全局 #990066，期望 GradientColorPicker #886633，实际失败；修复 owner 后14项功能通过。此后14功能、2完整恢复、2主视觉轨迹共18项通过72.321s，新增2个专属颜色轨迹通过69.942s；20个临时验证均已执行，未运行全量或 change 范围。
- 真实 Mac managed，scale2、NSWindow，明暗各 OverlayPopupHost 和 Native PopupRoot，4场景66检查通过16.353s。当前同源 NativeAOT 4场景66项通过19.217s，DynamicCode=false；产物SHA256为98d5cc18e9eaeefc4617e21553cb404ddb975f2470e5faed8be1a568ff3e0cd4。覆盖普通及 Control Effective Global、前景、禁用恢复、formatter、同一文字实例和打开后提交值保持。使用公开 pinned 配置开启，实际 Host 只读观测；不把本功能检查称为输入或原生像素验收。
- 最终AOT发布106.154s，无IL2026／IL3050，保留5项既有Homebrew库最低系统版本链接警告。首次发布的临时驱动序列化警告已通过直接输出消除，没有抑制警告或修改产品规避。

## 视觉协议及当前证据

真实 Headless/Skia 合成帧，产品 Window900×720、scale1，正常 motion；覆盖实际 transitions 至少750ms，并有连续3个相同帧。独立 main、A/A、1DIP 实例，同一完整11步历史；前缀只控制文件名。正式驱动以真实指针事件开关，检查事件到达和结果，内部选择只读。首版通过内部打开状态推进的取证只作诊断，已记录、查看并清理。

最终源码：GradientColorPicker.cs 的SHA256为9ee5b2cfd2b52dc43954f304b1a6abe448fb242db9de4d52c5af52da2e678c1c，主题为e39f226f4c38a205fc6da22f08bac152f7423fa041d930fd62d50c29b33cf8d0。旧版与最终候选各132帧，另有明暗完整恢复8帧。44组主轨迹逐字段检查尺寸、位置、选择、值、文字内容及bounds，无差异；88组A/A均0，88组1DIP全部检出。

| 输入／阶段 | 明色变化像素 | 暗色变化像素 |
| --- | ---: | ---: |
| owner Foreground=Purple，活动节点不变 | 646 | 646 |
| 作用域禁用色#cc2277，scope／disabled／enabled | 1228 | 1228 |
| 全局轨迹恢复正常色（旧版残留Purple） | 1228 | 1228 |
| formatter 明确Orange，重新展开 | 612 | 612 |
| Control专属 ColorText=#223344、ColorTextDisabled=#886633，scope／enabled | 1217 | 1228 |
| Control专属配置下disabled | 1228 | 1228 |
| Control专属配置下close，恢复正常文字 | 1207 | 1228 |

两类配方的默认、普通展开、自定义formatter未展开和最终reset，新旧整图全部0。Control轨迹的restore也为0。非零差异全部在触发文字区域(68,83)至(252,96)内。没有增加全局容差。

实际查看了当前最终源码的明色前景、暗色禁用、明色专属配置前后和formatter展开画面；变化仅文字颜色，色谱、输入、渐变节点、箭头和弹层几何保持，formatter 的文字及布局未改。

默认对角渐变在产品既有打开逻辑中会复制并规范为水平，原版及候选初末均有色块550像素变化。该变化不是本修复造成，不能称其整图往返为0。补充显式公开水平画刷并核对 StartPoint、EndPoint、Opacity、SpreadMethod、stops 后，原版明暗各残留1228像素旧文字，候选两幅整图均0；没有强制重绘、裁剪、遮罩、平移或容差。

## 清理后的验证与出口

4个临时C#／AXAML源、17份TRX（先压缩成文本结果）、2项源码备份均已删除，常驻测试净新增0。Native临时项目436文件／1324398743字节已移除，进程已结束。408张已登记PNG包含272张原版／最终证据及136张中间候选；仍在仓库外等待视觉审查，audit为clean且findings为空。

清理后精确 ColorPickerPinnedOpenTests／ColorPickerSemanticPartTests 共20PASS，15.883s；NET8 Release 构建20.116s，0警告／错误。C#成员审计1文件／1控件／0问题，git diff检查通过。作者自审，未调用子agent；B08当前无未修复Critical／Important，视觉审查及平台边界保留。

仍需所示视觉变化的明确审查和相应媒体清理；不能因此宣称原生最终像素、所有平台、完整ColorPicker家族或CT19／CT25完成。公开旧SetPresetColor委托形状问题留在CT19剩余合同工作，不改成已经验证。若需提交，另行提供具体快照给用户审查。冻结30文件、H1及其他待审查媒体均保持原范围。

## 本轮用户审查结论与媒体清理

用户明确回复“允许渐变文字动态颜色与恢复修复（推荐）”。只批准报告所示动态文字颜色、formatter退出恢复及Control专属配置变化。 此批准不含Git提交、原生最终像素或完整家族／CT任务验收。

已保留文本结果、原始差异数及408张PNG的逐文件SHA256；经标记目录工具清理仓库外本项媒体，删除451文件，清理审计clean且findings为空。前文待审／媒体保留描述为历史状态；本段为当前结论。常驻测试净新增0，清理后的既有模块验证见上文。
