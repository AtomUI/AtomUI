# CT13 AM5：复合输入实际 Global owner

> 使用 superpowers:executing-plans，沿当前分支/目录单agent执行；用户已授权继续完成CT13，不重复申请方法确认、不提交/新建分支/worktree。

**Goal:** 四个私有输入消费实际公开父owner的Global，保留既有状态、标准Foreground与专用Style原生优先级。

**Architecture:** 四个私有输入实现只读IControlThemeGlobalOwnerSource，声明实际模板父owner。复用现有OwnerTokenResource绑定，以SourceControlType明确原主题所属输入，并在遇到私有输入时改用其实际公开父owner；正常原生资源查询保留局部Resources覆盖。Own与真正公开子Control保持身份，不增加实例Token字典、ResourceProvider、ThemeContext、缓存或全局订阅。标准root Foreground声明式投射，不复制状态选择器。

**Tech Stack:** 当前AXAML/ResourceProvider、NET10临时专项、Skia自然/直接、Mac managed/AOT、NET8。

**Spec:** [剩余反例](input-autocomplete-mentions-remaining-audit.md)、[渲染合同](../../../architecture/systems/theming/control-rendering.md)、[消费规则](../../../engineering/development/theming/consumption.md)。

## Global Constraints / Review Focus

- AM2/3/4仍待限定视觉批准，不把“继续”写成批准。
- CT12代码、进度和六临时文件不改；采样锁用户已解除。
- 默认实际字体/颜色/几何、正常输入/光标/选择/只读/禁用与状态职责保持；无全量回归或通用像素容差。
- 真正公开Button等保留自身Global；既有共享Own不重命名/别名/新增字段；资源全树不能误转投父owner。
- 两个provider迁移、重模板/重挂与GC必须证明更新与释放；默认/状态/Local/专用Input Style分别核对，不猜优先级。

## Task 1：资源身份 RED→GREEN

**Files:** Core/Theme/Resources/IControlThemeGlobalOwnerSource.cs与OwnerTokenResourceExtension.cs；四私有输入C#只读接口；LineEdit/SearchEdit/TextArea/SearchButton四主题的Global来源；临时CustomThemeAutoMentionGlobalOwner。

**Interfaces:** 复用现有owner绑定与原生GetResourceObservable；新增internal只读owner关系及SourceControlType可选参数。公共API/Own/key不变，Core ThemeTokenResourceProvider恢复原实现。

- [ ] 还原16实际消费者反例，期望#c332a1/#123456/#fedcba，先观察有效FAIL；记录原同历史默认/配置/状态绘制。
- [ ] 四私有输入只提供实际模板父owner；公共输入主题的86个Global来源及SearchButton四处组合来源明确原所属输入。普通公开子Control先保留自己的identity，Own不改。
- [ ] 原16转绿；foreign配置/透明0/更新清除、Own不转投与公开子Button独立身份正对照。
- [ ] 局部原生Resources覆盖与清除的4个真实RED转绿；普通资源层级不能被直接snapshot读取绕过。

## Task 2：公开属性、状态及生命周期

**Files:** 共享Auto与Mentions根主题、四模板；不增加C#控件绑定。

- [ ] 标准root Foreground当前值到实际文本先RED，记录默认root/实际文字；据真实结果添加根主题默认与Local模板投射。
- [ ] 原四variant/default/error/warning/disabled/readonly、普通Style/Local与四专用Input Style覆盖及撤回；状态按原主题承担，不复制伪类/状态机。
- [ ] 实际输入/caret/selection不重建，两个scope迁移、重挂/重模板、provider与root/目标GC；无逐帧资源查询或后台工作。

## Task 3：验收与清理

- [ ] 同历史明暗/四owner的默认/状态/配置/恢复；AA/1DIP复位；正常光标与动画保持，真实状态绘制检查。必要直接后端A/B分开，不抹掉自然失败。
- [ ] 当前Mac managed/AOT运行资源/优先级/生命周期关键消费者，NET8、成员门禁及正式文档。
- [ ] 另步作者自审，删除本包临时验证/源副本/原生工程/媒体，精确AutoComplete/Mention/Input直接消费者最终测试；视觉变化汇入具体审查，不关闭整个CT13。

原型裁决：实例ResourceProvider虽通过功能/原生测试，自审补出的4个原生Resources覆盖反例均失败，也不符合实例Token字典/私有作用域约束。该架构已撤回，相关像素/6101项原生结果仅为被否决原型记录，不能当最终实现通过；其Core snapshot重载和slot helper均已恢复。最终架构沿现有owner绑定完成，必须重新取证。
