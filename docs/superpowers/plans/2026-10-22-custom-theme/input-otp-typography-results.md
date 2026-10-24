# CT-13 G3-C：OTP 根与分隔符排版

根字体族与根/分隔符三档字号接入明确的OtpLineEdit有效Global；最终Setter在ControlTheme，复用Core有限可选转换器和真实继承源。未配置及清除时根/分隔符恢复当前祖先，cell保留Large16/Middle14/Small12/Custom14。没有新增Own、公开API、资源ID、状态旗标或代码字体订阅。cell预设与根FontSize的原有独立职责保留，应用用专用Cell Style定制cell。

## 前后与实际消费

初始9项实际失败；扩展后旧源码10失败、1项GC正对照通过、1s。候选11项加63项相关既有验证共74/74通过、3s。覆盖四尺寸的root/cell/InputTextPresenter/separator字体族，明确23/25/27字号、祖先19及清除，普通Style/local优先级、Italic/Bold/Condensed实际typeface、Control配置/兄弟隔离、换父21及文本保持；移除后的root/provider均回收。没有把空集合或编译错误当成功。

OtpLineEdit增加内部只读InheritanceParent桥。原有FormItem接口region移到private实现之前，两块完整原文SHA256及字节保持已核对，29项既有成员顺序问题清零；没有改写原输入处理或接口方法体。成员audit最终1文件/1控件/0问题。

## Skia 与 NativeAOT

真实Skia、HeadlessDrawing=false、正常动效，1480×690、scale1。四尺寸×四variant共16根/96格，父Arial19。明暗10个默认/同输入/禁用/只读/恢复阶段共20张旧新完整RGBA均0；A/A0，1DIP检错27314/14012像素，移回及配置清除0。配置后root/cell/separator实际Menlo及23/25/27，旧新82148/77263像素变化属于字体消费。真实KeyTextInput输入654321，合法转移焦点后取静态画面，未暂停或关闭blink。cell引用保持。

查看了明亮配置与暗色恢复。大字配置在固定列宽下末格受原布局约束裁剪；未放宽尺寸或掩盖裁剪，不用本矩阵宣称任意大字配置无裁剪，CT20继续核定尺寸合同。默认各格完整可见，所有96格的配置值另有实际断言。

当前Mac arm64 NativeAOT明暗实跑均通过实际字体、96格身份和真实输入；22张JIT/AOT完整RGBA均0。首次外部驱动缺Homebrew链接搜索路径而报ssl库缺失，补驱动专用-L参数后发布和运行通过。无IL2026/IL3050；OS26动态库与目标12的链接提示保留。没有原生最终显示面或其他平台结论。

## 清理与最终检查

11项临时测试、423个驱动/发布文件及两文件旧源备份已删除；本字体媒体运行71文件清理，内置audit clean，常驻测试净增0。清理后OtpLineEdit精确filter66项通过、2s；Core TokenResourceExtensionTests 1项通过、15ms；NET8 Desktop Release含依赖零警告/错误、17.88s。正式overview/implementation/token/changelog已同步；作者自审、成员及diff检查通过，无subagent/CUA/提交。

另发现旧版暗色separator继承黑色而不可见，字体修复未改变此颜色。仅修改公开root Style的颜色原型已完成，明暗默认变化2560/2960像素、禁用2960/2880像素，几何不变；用户2026-10-24已明确批准该默认视觉改变，后续独立执行[颜色分包](input-otp-color.md)。8张颜色审查图及两文件原型驱动属于仍开放的后续颜色任务，独立保存在外部临时目录，此记录描述当时字体分包状态；后续颜色修改及清理由颜色分包记录。其他输入选择家族、原生显示面及完整性能继续开放，不把G3-C作为完整CT13验收。
