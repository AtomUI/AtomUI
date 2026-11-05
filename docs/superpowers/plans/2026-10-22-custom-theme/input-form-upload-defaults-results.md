# CT13 FU1：Form/Upload 默认协议结果

2026-10-30，现有feature/custom-theme目录，单agent、无新分支/worktree或提交。本包只验收默认求值机制与既有Own消费，不代表完整Form/Upload或CT13。

## 实现与兼容

Form8/Upload13既有Own采用DependencyDefaultsV2、声明修订1，21个静态默认方法只读取有效Global或原常量，无Own依赖边。名称、类型、公共资源ID与slot保持。原表达式与顺序保持，特别是47.5×2.55仍为121.12499999999999，默认卡片102、progress线高2、FontSize+PaddingXS左内缩与vertical margin默认0不变。

VerticalLabelPadding归Legacy/CompatibilityRetained。生产搜索仅有声明与默认，内置模板使用VerticalLabelMargin；保留Thickness、0/0/0/有效PaddingXS默认、资源ID及显式配置读取，不添加内置效果。真实测试与原生检查把该字段改为20/30/40/50时label/form Bounds不变，资源值正确；正式文档不再把它写成可调内置padding。

## 证据

- 真实注册descriptor的2项V1→V2要求先FAIL，原快照2项正对照通过；迁移后4项PASS722ms。
- 明暗×默认/控件Global/显式Own/清除，168条值、运行类型、key类型/ID/slot逐项完全一致。21字段没有扩大范围或取整。
- 最终7临时专项PASS1s：Form7有效Own进入真实标签/必填/colon/spacing/inline panel/vertical margin，Upload13进入实际文本、picture/card/circle与drop资产；模式切换保留同一文件对象，清除恢复。最初TextBlock命名空间遮蔽和横向StackPanel匹配过宽属于夹具错误，均已改为准确Avalonia类型/公开ItemsPanelRoot，未改产品绕过。
- 1320×850完整Skia窗口，三Form布局、四Upload模式及三种终态/待上传文件、默认Drop区域保持。明暗原版/候选14对全RGBA0；正常motion、AA与1DIP检错/复位有效。已查看默认与Own配置关键图；Dark配置故意使用深色LabelColor，低对比与fixture标题继承限制在原/新均存在，只证明兼容，不宣称任意用户配色可读。
- 当前Mac实际NSWindow managed/AOT各699检查PASS：168个既有值及literal ID/name/type、实际标签字号/drop图标与兼容字段无布局副作用。AOT无IL2026/IL3050或抑制，5条本机OpenSSL/Brotli deployment链接警告保留。
- Token成员门禁2辅助文件/0Control/0问题；默认生成器精确1项通过。NET8 Release 0警告/错误、25.57s。

本包产品代码仅两个Token类与兼容注释；未修改上传调度、输入或视觉状态机。作者另步自审覆盖表达式、默认ID/slot、Own覆盖/清除和生成器/AOT，无评审agent，未代用户批准其他视觉。

5临时源（19728 bytes）、2源副本、424文件原生工程（1251280648 bytes）、28PNG+manifest共29媒体全部清理，audit clean；常驻测试净增0。清理后首轮Form/Upload165/167通过，两项旧源码断言分别仍锁V1表达式与当前基线已迁移的Shared motion字面值。既有测试已改查V2静态默认公式及Owner motion，原默认布局/模板/动效检查保留，未改CT12生产代码或进度。最终精确Form/Upload167PASS、0FAIL/Skip、2s，git diff --check通过。

后续继续剩余Global/字体、专用Part、状态及家族出口；AM2/3/4/5限定审查仍独立待批。
