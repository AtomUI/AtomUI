# CT13 CB1：ComboBox 十一项 Global-only 默认迁移

主agent、现有分支和目录，承接已批准Own V2机制。只修改ComboBoxToken默认声明及相关文档，不触及Popup/输入/选择实现或新增字段、别名、身份。ItemDisabledColor、HandleHoverColor的实际消费另包核定，不借默认迁移关闭消费者缺口。

十一项默认都是Global-only或常量：PopupContentPadding=(UniformlyPaddingXXS, BorderRadiusLG.TopLeft/2)；ItemColor/ItemHoverColor=ColorTextSecondary；选中文字=ColorText；禁用文字=ColorTextDisabled；ItemBgColor=ColorBgElevated（不是ColorBgContainer）；hover背景=ColorBgTextHover；selected背景=ControlItemBgActive；handle hover=ColorPrimary；ItemPadding=(UniformlyPaddingSM,0)；ItemMargin=(0,0.5)。保留运算、字段顺序/类型、key/ID/slot，不虚构ItemHoverColor到ItemColor的Own依赖。

- [x] CB1-R：原注册descriptor仍V1，临时协议反例失败；原明暗默认/key/ID/slot及独立Global/Own/0/清除正对照先运行。
- [x] CB1-B：真实ComboBox默认、交互、已打开Overlay popup、明暗完整帧；AA/1DIP/复位和新旧同轨迹，真实键盘/选择保持。
- [x] CB1-I：sealed partial + V2 rev1/IControlTokenDefaults，十一静态默认函数；删除空ctor与whole evaluator，不改其他源码或模板。
- [x] CB1-V：十一字段/明暗/显式值/0/恢复，实际popup/条目消费者与当前Mac managed/AOT、NET8及成员审计。缺口保留给后续消费者，不写成已完成。
- [x] CB1-C：临时源/原生工程/媒体清理后精确ComboBox模块；正式Token/implementation/changelog及CSV/总表/作者另步自审。无Git授权，不关闭全家族或CT25。

CB1：ComboBox11项Global-only默认V2 rev1，无Own边；原注册协议1FAIL/旧GlobalOwn正对照1PASS→候选3PASS，明暗22条默认/类型/key/ID/slot相同。保留三行cap/四项/滚动条节点可见/真实DownEnter，22对新旧完整帧及AA/复位/配置恢复0；1DIP7643/3933检出。Mac产品Window/Overlay managed/AOT各102，NET8零警告24.82s；两临时源、419文件工程、44PNG+manifest清理，原ComboBox69PASS/0Skip，单Token审计0问题。仅默认机制，ItemDisabledColor/HandleHoverColor消费及完整家族/CT25继续，无Git。
