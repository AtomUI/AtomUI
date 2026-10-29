# CT13 CR3：CheckBox / RadioButton 有效 Global 视觉审查

本次对应[Global消费计划](input-check-radio-global.md)的六个AXAML文件、38处现有资源入口；不改变API、Own字段、选择、字体或布局公式。两个公共控件和两个Group分别读取自身有效Global，内部指示器的线宽/开关沿公开owner查询。内部列表继续既有Group模板绑定，未新增fallback改造；时长已有Owner绑定，不重复实现。

## 实际场景

当前Mac、Skia、680×380、scale1；真正Default / Default+Dark scope。三组独立Check/Radio覆盖unchecked、selected、disabled，另含各三个项目的CheckBoxGroup/RadioButtonGroup。两个版本都执行默认→AA→1DIP→复位→真实点击→Space→配置→清除→焦点恢复。布局、指针、焦点、选择、正常动效和完整图片坐标保持一致。

配置分别在CheckBox/RadioButton身份覆盖ColorText=#986123、ColorPrimary=#aa3678、LineWidth=2，在两个Group身份覆盖SpacingXS=22。控件算法保持默认Disabled：LineWidth作为有效输入影响既有Own派生，但不自动重新计算BorderThickness。另两项功能反例显式启用Global算法，验证LineWidth=3到实际BorderThickness=3；未改变框架算法合同。

独立自然合成和直接绘制分开记录。交错RTB诊断保留其原始计数，但不替代独立自然轨迹；没有裁剪、遮罩、容差或只挑某轮。

## 精确差异

| 比较 | Light | Dark |
| --- | ---: | ---: |
| 独立自然默认、AA、1DIP、复位、selected、ready原→候选 | 各0 | 各0 |
| 独立自然配置原→候选 | 5015，最大通道224 | 5015，最大通道199 |
| 独立自然配置恢复、焦点恢复原→候选 | 各3，最大通道9 | 各3，最大通道8 |
| 直接默认、输入、复位及恢复原→候选 | 各0 | 各0 |
| 直接配置原→候选 | 5020，最大通道224 | 5009，最大通道199 |
| 独立自然AA，原版/候选 | 0/0 | 0/0 |
| 直接AA、扰动复位、配置及焦点恢复，原版/候选 | 全部0 | 全部0 |
| 原版/候选、自然/直接1DIP检错 | 均4725 | 均4700 |

配置变化区域限定为x24–292、y27–200：有效文字与选中色、两个Group项目位置变化。disabled仍使用自身disabled颜色。旧Shared入口忽略Control Global的情形开始生效，默认与输入外观保持。

新的三点恢复差异仅在CheckBoxGroup首个选中项的外框下缘：(24,157)、(39,157)、(38,158)，Light通道最大分别5/9/4，Dark分别4/8/4。它们与CR1为不同坐标和不同轨迹，不沿用CR1批准。直接绘制同阶段全部0，交错RTB诊断中的原→候选恢复也为0；不据此把独立自然原始失败改成0。

## 保留的同版本自然失败

| 独立自然历史 | 原版Light/Dark | 候选Light/Dark |
| --- | ---: | ---: |
| ready→配置恢复 | 151 / 137 | 154 / 140 |
| idle→焦点恢复 | 154 / 140 | 151 / 137 |

以上最大通道分别为3/2或9/8，主要是原有指示器边缘；原→候选只差上文三点，所有原始失败保留。交错RTB诊断的自然AA为6/12、ready恢复154/140、焦点恢复157/149，原版与候选计数相同；该诊断有额外绘制历史，不能与独立自然数据混用。底层合成问题仍归CT25，不增加刷新或合成器选项，不替换官方Avalonia12.1.4。

## 当前门槛

原8项真实消费反例失败；两项初始线宽断言误用了Disabled算法，记录保留并排除其有效RED资格。按正式算法合同修正夹具后，原指示器主题的两项反例重新失败；候选最终15项通过（含实际WrapPanel、现有时长和LocalValue优先级），最终14消费者附加0/恢复断言亦通过。当前Mac managed实际NSWindow与NativeAOT各54项通过。

已查看明暗配置和默认完整图。用户于2026-10-27明确接受本文精确范围（call_d5a1b537ebc04beb801aff762a1dc76d:0），并要求CT25继续开放；不包含通用容差、Git或完整家族。原始失败与计数保持；AOT、清理及最终模块结果另记本包结果，不能以视觉批准代替。

本包清理、原51项及NET8结果已完成，见[消费结果](input-check-radio-global-results.md)。媒体现已删除，本文保留文字数据。
