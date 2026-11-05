# CT12 V11：Button.BoxShadow 已选定合同

2026-10-29，用户已明确选择“公开BoxShadow属性（推荐，133 Own不变）”。本文记录该已选合同，不重新解释PrimaryShadow，也不新增Part。

Button公开`BoxShadows BoxShadow`及`BoxShadowProperty`，Dropdown继承。通过Avalonia Border.BoxShadowProperty.AddOwner注册，使用原生Style/local及ClearValue规则，不增加存在位flag。未定制时ControlTheme以编译Self绑定读取既有VariantShadow；模板阴影层再读取根BoxShadow。计算、Own/Global身份、预设色旧Blur3/Spread0和所有默认状态保持。

显式值（包括空BoxShadows）覆盖全部颜色/变体的阴影层；本地值优于Style，清除后恢复当前Style或当前主题默认。默认颜色/变体/disabled/ghost规则仍由原计算表达；用户显式覆盖是原生根视觉定制。阴影不参与布局测量，不新增节点、命中区或代码绑定。Button/Dropdown自有主题都必须接入；Submit/Reset/DialogButton复用Button主题的路径核对，其他家族自有模板不无条件扩展。

五Own终端仍133字段，Token语义/ID及Semantic Part范围保持。Root用普通原生Style；不公开ShadowsFrame名字，不新增shadow Part。AOT使用编译Avalonia属性访问；无反射、源程序集补丁或强制整窗重绘。

验收：先用注册属性缺失的运行失败证明V11缺口；实际阴影层验证Style/local/空值/清除、Own与preset默认更新、重模板与重挂/释放；默认明暗同历史像素0、显式定制命中及恢复；NET8、当前Mac AOT和所需原生显示。临时源/媒体删除后精确模块验证；本轮不提交/推送，不创建分支/worktree，单agent执行。
