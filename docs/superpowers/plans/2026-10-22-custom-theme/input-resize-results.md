# CT-13 TextArea Resize 分包结果

Own ResizeHandleSize原来不控制8×8命中区域；装饰框Render的7/4坐标和1描边也写死。实际两项RED665ms，绘制断言第三项RED779ms。先改父Render虽通过数据/AOT断言，实际Skia颜色像素仍0，证实不透明模板遮挡。用户明确批准“允许修复默认指示线”，唯一默认视觉例外是恢复该指示线可见。

## 实现及合同

ControlTheme声明Size→既有末层ResizeHandle的Width/Height、Color→Foreground、同owner Global LineWidth→内部描边。Handle.Render保留8参考单位的原几何并同比缩放；0无命中与绘制。删除父装饰框失效Render、状态、属性与Setter，无新wrapper、Part或公开Token。ClipToBounds=False保留超出1参考单位的原端点；未改父裁剪、滚动或指针事件。四尺寸及IsResizable原合同保持。

4项临时验证通过；两个独立配置scope、20/0/清除8、描边2/3、原生隐藏与实际DrawingGroup均覆盖。当前macOS arm64 NativeAOT实际运行验证默认8、显式20/0、颜色、线宽和清除，PASS；未验证平台不记通过。

## 脚本化视觉及交互

12个实际TextArea、四尺寸、明暗、clear/count、固定310×150及10行内容。真实鼠标左/中/右命中，拖拽默认+30和配置+20均通过，scroll extent202大于viewport150；未取消高度限制或内容。A/A0，1DIP检错34980/34839像素。

旧默认→最终上层绘制明暗各88像素变化，集中在已批准的指示线；ClipToBounds单变量增加16端点像素。临时提案与实际模板绘制暗色0、浅色2个指示线像素差异，提案不作为产品golden。

原175行距场景在150高度+30拖拽时覆盖下一行5像素，恢复留下9/12个RGB≤3边角像素。保留证据后单变量改测试行距200，未改控件高度/滚动/产品代码。浅色另有20像素输入光标阶段差异；使用公开FocusManager.Focus(null)统一失焦阶段，不关闭motion或光标功能。最终配置两主题各改变2144像素，实际Magenta352/344像素，恢复均0。检查了暗色实际配置截图。原焦点及重叠证据未覆盖或掩盖。

## 清理与最终验证

4项临时测试、两个外部工程及其专用产物均删除；74个媒体运行文件清理，Git媒体审计clean。清理后TextArea/输入生命周期/LineEdit边框及Button精确filter395项通过9s（进程29.68s），NET8 Release零警告/错误21.44s。成员检查2文件2控件0问题，作者自审确认指针事件未改、旧失效父输入已删、主题声明为唯一配置入口。F/G/V以及CT-13家族整体仍未完成。作者自审不代表独立评审，无提交。
