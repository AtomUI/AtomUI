# CT-12 剩余按钮消费链结果

本分包接通DefaultBorderColorDisabled与GutterToFlyout，正式语义归[Button Token](../../../controls/desktop/general/button/token.md)与实现文档。保留未配置的原外观、通用禁用色和Flyout原0间距；无新增Public API，无提交。

## 有效消费与生命周期

原3项失败722ms：默认禁用描边配置Magenta仍Green，打开Flyout间距23仍0（两种Host偏好）。实际AtomUI Window复现，最初plain Window无Overlay层的报错不计作功能前证。

8项临时验证最终通过915ms：通用/特定禁用色、raw资源与LocalValue、打开时配置/清除/显式0、共享Flyout实际Target A/B、替换/分离/重开、MenuFlyout及RequestedPlacement保留、释放图GC。属性测试明确关闭自身motion以检查终值；独立Skia视觉保持正常motion，不能据属性测试声明动画通过。

转换器接受资源提供器投射的IBrush与Color；ControlTheme完成最终禁用Setter。非Visual Flyout在模板外，由Button.cs中的关联Flyout方法观察实际Target，使用Template优先级绑定，关闭/替换/分离释放；两个私有句柄清理后均null。原生Avalonia Flyout没有对应Margin合同，不注入。

整个关闭图立即GC最初失败，禁用新增订阅的A/B通过；两个订阅已释放，推进正常合成器渲染提交后，新实现的Button及Flyout均可回收585ms。没有在产品加入延时/重试。长寿命且仍被Button.Flyout赋值的Flyout由Avalonia基类Opened/Closed强事件关联旧Button，移除新订阅仍失败638ms；这是另一既有关系，CT-25继续审查，不宣称本分包修复。

## 视觉与AOT

25个实际按钮、四尺寸、六种disabled角色；Color预设必须与Variant成对给定，首次fixture遗漏已修正后重新采旧新基线，没有覆盖原截图。真实鼠标Click计数1并打开Overlay Flyout。明暗idle/展开默认前后0px；A/A0，1DIP检错21792/22240。特定描边和间距配置实际改变22266/16584px。查看了修正后的暗色配置画面。

恢复后未改trigger边缘9/5个RGB1像素，与禁用新间距订阅、仅使用原有Flyout.MarginToAnchor API移动的A/B完全相同；配置/恢复新旧同阶段均0px。没有使用容差、遮罩或重建场景，不把9/5称作恢复0；原生渲染现象继续CT-25。

当前macOS26/arm64 NativeAOT实际使用公开配置验证禁用色、打开间距、LocalValue优先及清除，PASS。Headless两种Host偏好不代表两个原生OS后端通过；没有扩大平台结论。

## 清理与最终验证

8项临时测试、两个外部视觉/AOT项目、临时资源值跟踪均删除；41个媒体运行文件清理，内置Git媒体审计clean。清理后Button及Flyout生命周期相关filter361项通过8s，成员布局2文件1控件0问题。NET8 Release补restore后零警告/错误24.50s；首次no-restore因AOT更新的assets仅含net10而失败，不是产品编译错误。15个本分包文档链接存在，git diff --check通过。作者另步自审资源优先级、owner共享、句柄释放及默认兼容，这是作者自审，不是独立评审。

Button ExtraContent四字段和GroupBorderColor缺少实际区域的兼容决策、loading等完整家族出口仍开放，不据两个字段消费宣告CT-12全部完成。
