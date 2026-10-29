# CT-12：HyperLinkButton 原生绘制输入与 SimpleButton 边界

## 最终合同与修复

HyperLinkButton的Frame读取最终root Background、BorderBrush及BorderThickness；CornerRadius、Padding和BackgroundSizing继续沿既有模板链。root Background默认保持null，Frame用编译TemplatedParent绑定的TargetNullValue透明回退维持原命中面，优先级明确为Template。没有新增转换器、C#绑定、属性写回或优先级引擎。

删除原来无条件把Frame.Background压成Transparent的普通/ghost/disabled规则；它们此前令最终root值无法绘制。默认仍透明、无边框，IsGhost不额外覆盖最终root背景。应用请求非零BorderThickness时，disabled的既有Own BorderColorDisabled才能参与实际描边；保留17字段、V2默认函数和原资源身份，只补XML及正式合同说明。

原始8项Frame消费/状态/普通Style反例失败，两个正对照（原生包装边界与整行命中）通过。首候选11项通过后，作者自审新增三项原默认值断言，发现root getter从null变Transparent；该3FAIL被保留。改用Frame空值回退后13项功能通过，最终加完整异步视觉轨迹共14PASS。Hyper的180×36左/中/右真实点击3次，SimpleButton同尺寸真实点击3次；视觉夹具所有控件220×36始终保持。

## 完整帧精确比较

Skia、620×380、scale1，Default/Default+Dark实际scope；normal→AA→1DIP→恢复→原生定制→真实hover/pressed/release→禁用Own改色→配置恢复→清除定制。正常动效启用，异步等待并硬断言当前颜色已到达base目标，保持同一实例、焦点与尺寸。

| 比较 | Light | Dark |
| --- | ---: | ---: |
| 前/后AA | 0 / 0 | 0 / 0 |
| 1DIP检错，前/后 | 8382 / 8382 | 8369 / 8369 |
| 默认、AA、扰动恢复、清除定制前→后 | 全部0 | 全部0 |
| 定制背景/边框、pressed、released、配置及恢复前→后 | 各23404 | 各23445 |
| hover前→后 | 23381 | 23445 |
| 候选禁用Own改色 | 1032 | 1032 |
| 候选配置恢复 | 0 | 0 |

最大通道差Light255/Dark225。全部差异逐像素归因到Local paint、Disabled Own、Ghost root三行，场景其他区域（含SimpleButton和原生Button）均0；完整帧每个像素仍参与比较，没有裁剪、遮罩、对齐或容差。已查看明暗定制、禁用改色和默认恢复完整图。变化是原本被忽略的现有root/Own输入开始绘制；视觉审批待明确结论，不以默认0自动豁免定制差异。

## SimpleButton 核定边界

用户已决定[保留原生兼容包装](buttons-simple-boundary.md)（call_365dea7a230e47369242a467487bfd79:0）：无新增AtomUI身份、Own或预设主题。真实Default模板为Avalonia ContentControl的ContentPresenter/TextBlock；原生背景、描边、厚度、圆角和Padding委托及整行输入已验证，独立ControlTokenAccessor仍明确拒绝未注册类型。明暗前后完整场景中该行默认/定制0像素差异；宿主通过普通TextElement.Foreground继承提供文本颜色，未伪造SimpleButton控件配置。

## 当前出口

当前Mac实际NSWindow scale2、360×330，managed63PASS：默认null/透明目标、Local及ghost原生值、disabled Own实际描边、普通Style及撤回、作用域更新/恢复、焦点、真实模板替换/重挂、全部220×36尺寸和SimpleButton原生委托/未注册边界。

NativeAOT、视觉批准、临时源码/媒体清理、精确模块及NET8结果待追加。完整CT12/CT25、跨平台、其他业务状态与已有残留问题继续开放；仅主agent作者自审，不提交、不调度Actions、不发布NuGet。

用户已批准此绘制接线与精确差异（call_8c06c814740f4e6aad8a68729fc15d73:0），不含Git提交或完整家族验收。实际NativeAOT Mach-O arm64程序同样63PASS、exit0；真实ILC输入为官方12.1.4/lib/net10.0/Avalonia.Base.dll。没有新增裁剪诊断，保留本机OpenSSL/Brotli高于macOS12的五条链接警告，不能外推最低OS。Token成员审计1文件/0控件0问题，默认函数和字段数没有变化。

清理完成：两份临时源码删除，常驻测试净增0；66PNG+manifest共67文件删除、媒体audit clean；外部Native项目及全部产物423文件、1,378,807,694 bytes删除。清理后精确模块132PASS/0FAIL/0Skip（3秒），NET8 Release构建0 warning/0 error（27.29秒）。Token成员审计1文件0控件0问题；25份相关Markdown的237本地链接有效，CSV精确三行更新，diff clean。按仅主agent约束完成作者自审，无独立评审。此绘制/原生边界分包完成，完整CT12/CT25仍开放，见[owner出口核对](buttons-exit-audit.md)。
