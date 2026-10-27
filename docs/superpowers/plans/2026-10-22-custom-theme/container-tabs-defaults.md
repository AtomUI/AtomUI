# CT14 N3-TAB：TabControl默认函数与现行CardSize依赖

主agent沿既有CT14 N3／Own V2设计连续执行，来源278429b50；沿现有feature/custom-theme和checkout，不创建分支／worktree，不发布NuGet，Git另审。

**Goal:** 迁移TabControlToken的34默认，接通现行CardPadding→CardSize依赖，保留原公式和资源身份；不新增字体依赖或padding钳制。

**Architecture:** 34静态默认、Revision1／DependencyDefaultsV2／IControlTokenDefaults；仅CardPadding显式接收有效Own CardSize，其他33项使用原Global／常量。保持字段顺序／类型／key／枚举数值／slot与明确下游值。

**Spec:** [容器N3计划](container-consumers.md)、[Own默认求值](own-defaults.md)。不是新增Tab交互设计；发现其他语义变更先独立审查。

## 已查明的边界

- 原TabControl命名空间精确283项通过。CardTabControl／CardTabStrip独立Global、无Own，模板显式复用TabControl Own；实际查询RootOwn需按identity，不按CLR继承推定。
- 原版明暗两项有效反例共12处：Own CardSize56／0已进入资源和两host内部CardSize，CardPadding资源与两种实际item padding仍为16,8。明确CardPadding=3,4,5,6及清除保持。原场景保留两host、三个item、SelectedIndex1、原尺寸和add按钮，不绕过触发条件。
- 当前公式严格为Thickness(Global.UniformlyPadding, (有效CardSize-Round(Global.FontSize*Global.RelativeLineHeight))/2-Global.LineWidth)。CardSize56应为16,16，0应为16,-12；框架实际接受有限负padding，不借鉴Segmented的MinHeight钳制。
- TitleFontSize22会改变实际文字字号，但当前公式仍读Global.FontSize。本片保留这条现行边界，不凭同值默认新增TitleFontSize依赖；其字体／实际测量语义归N2后续核定。
- CardSize当前主要消费到add按钮维度，CardPadding仅供Middle水平Card；Large／Small、垂直Card、Line、overflow、placement、选择／输入和Part路线保持，不修改控件或AXAML。

## 执行清单

- [x] 原283项及明暗消费者反例；有限负padding框架正对照。
- [x] 扩充V2合同RED和34字段明暗默认／Global／Control Global／Own／0／下游／恢复资源记录，原失败与保护项分开。
- [x] 34默认静态化、一条有效CardSize依赖；同反例GREEN，标准默认／metadata不变。后续另获批准的添加按钮尺寸修复见独立短设计，不归为纯Token迁移。
- [x] 四owner及四placement、尺寸和状态的原场景自然A/A、1DIP、新旧／恢复对照；保留滚动／裁剪／shadow，所示布局与N01–N16精确差异于2026-10-27单独批准，原FAIL保留。
- [x] 当前Mac managed1062／NativeAOT1063及408资源一致；8临时源／423文件工程／303媒体清理，原Tab283项、NET8零警告、完整成员与正式九文档／目录同步。具体Git另审。

切片证据及未闭合的直接原生Styles.Remove、HorizontalMargin语义、完整家族与平台出口见[本机结果](container-tabs-defaults-results.md)。本清单完成不代表CT14/CT25完成。

本片只交付默认机制及现有CardSize依赖；字体新依赖、Custom缺口、持续输入／动效、完整overflow专项、其他Host与原生最终像素及完整CT14／CT25仍开放。新增验证默认临时，保留原失败及媒体审查后清理，不新增全局常驻测试。
