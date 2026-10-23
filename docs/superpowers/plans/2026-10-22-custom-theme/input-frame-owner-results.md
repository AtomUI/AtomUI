# CT-13 共享输入表面与内部按钮分包记录

InputControlFrameTheme84处、InputClearIconButton5处、RevealButton6处共95个Shared读取改用真实公开owner的Global。保留原状态选择器/优先级/默认资源、根画刷中继、几何与动效归一，不新增identity/Own或样式优先级引擎。

## 真实消费者与路线

首次12项失败中，TextArea Reveal用例观察了尚未完成切换的基模板；正式TextArea模板没有该部件，故该项不构成有效缺陷前证。更正为实际无Reveal合同，不新增密码按钮。其余11项真实缺陷为四种框体颜色仍Shared、四种clear和三种reveal尺寸仍14而非配置22。状态断言在单测禁用motion以观察终值，正式视觉保留正常motion。

只替换95处资源后，四frame及TextBox按钮有效，LineEdit/TextArea/Search的ContentRightAddOn子树没有TemplatedParent，另六个按钮仍失效。实际树为按钮→StackPanel→AddOnContentPresenter→decorated frame→公开输入。Core原OwnerTokenResource查询补逻辑父回退，在上下文/自身父/模板父/逻辑挂载变化时重新解析，按上下文+资源键去重，不订阅每个祖先、不缓存全局强引用Control。最终属性仍由AXAML/native binding持有。

17项专项PASS1s：四框体normal/hover/error及根显式画刷恢复、四clear和三reveal的实际尺寸/状态/清除、TextArea无Reveal、相同Context真实clear内容迁移LineEdit22→Search30→22、四种输入从长寿命父移除后的GC。

## 视觉和输入

26控件：四公开文本owner×四variant/四尺寸，以及ComboBox、Select、Cascader、TreeSelect、NumericUpDown、DatePicker、RangeDatePicker、TimePicker、RangeTimePicker和完整六格OTP。相同全轨迹明暗idle/hover/disabled/error/warning/ready全部0像素；配置before/after改变63865/63876。A/A0，独立1DIP检错17638/17627、回位0/1个RGB1，该实例不参与主基线。原始320宽、TextArea104高/十行及滚动/裁剪保持；已查看完整暗色配置画面，其他十个公开owner在本轮输入专属配置下保持隔离。

实际框架鼠标输入在明暗各验证48次clear左/中/右命中、12次reveal切换通过；实际布局大小22，与配置一致。未用直接调用事件或改Text冒充按钮点击。

配置清除后的before/after合成帧仍有7/18个像素、maxDelta7，继续CT25，不予豁免。明暗六阶段默认0只证明该矩阵，不能代替所有家族/弹层/持续动效或最终原生显示验收。

## 未完成出口

当前Mac NativeAOT初次编译的性能诊断输出使用匿名JSON触发临时驱动IL2026/IL3050；改为有限Invariant文本后重新发布，无这两项告警。初次立即颜色断言在JIT同样失败；改为保留正常motion、900ms及实际帧稳定采样后，JIT与NativeAOT明暗16实例的frame/clear配置及清除均实际PASS，四张关键画面差异0，并查看完整暗色AOT画面。已知本机Brotli/OpenSSL最低OS链接提示不推断旧macOS支持。旧源码精确反转后，正确完成布局的11项实际消费者全部RED1s；四源SHA256精确恢复。既有hover/focus测试helper原先忽略identity/algorithm，只配置Shared，现改为不同Control种子与真实算法模式、断言typed实际颜色，保持原有用例数量；一项锁旧Shared资源写法的断言同步为Owner路由。AOT运行已补。17项临时验证、外部工程427专用文件、四个中间源码备份和49个媒体运行文件已删除，Git媒体审计clean；清理后Core21项225ms、受影响输入/组合/按钮/Radio的精确filter525项17s通过。NET8 Release零警告/错误18.24s；本地链接与git diff --check通过。作者自审，无subagent/提交，CT13家族仍开放。

## 性能证据与开放项

最初26控件的控件级配置对照中，旧实现没有消费95处字段，原始hot10约185ms对候选627ms不代表相同有效更新量。另以16个相同输入、全局相同颜色/尺寸更新进行五轮独立进程采样：旧cold中位696.34ms/21390112B，候选689.64ms/23203064B；十轮往返旧547.87ms/31948664B，候选613.61ms/40229608B。时间范围有重叠，不能据此声称更快或无回归；分配增加明确存在，CT25继续查绑定/通知及实际绘制成本。此为当前Mac Headless/Skia样本，非全平台或最终显示面性能验收。
