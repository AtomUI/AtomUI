# CT14 N3-T：TreeView Global-only 默认函数迁移

按[现有容器计划N3](container-consumers.md)在当前checkout／feature/custom-theme单agent实施；不创建分支／worktree、不提交，不纳入C9／C10第十六批Git快照。

## 范围与已核对合同

TreeViewToken的12个字段都只依赖EffectiveGlobal，没有Own→Own依赖。原实现的两项实际scope探针已通过：Global和Control专属Global的ControlHeightSM39／ControlItemBgHover#123456均到达两个真实节点及资源，不能把迁移描述成消费bug修复。此前明暗24行真实资源／类型／slot基线已取得，原TreeView57项通过。

只将该internal Token改为DependencyDefaultsV2／Revision1与12个静态默认函数，读取context.Global，保留字段名、类型、资源key／枚举数值、默认结果和已有明确Own优先级。没有新增Own字段、Control API、Part、状态、绑定或主题布局。既有空构造保留；不顺手修改TreeView事件、回收和视觉。

## 实施与验收

1. 临时运行期descriptor合同先验证Version2／Revision1缺口，并补录两主题24项实际枚举数值、类型、slot、resource projection／默认值。原scope探针继续保留；将Version缺口与产品bug反例区分。
2. 替换整段CalculateTokenValues为对应12个默认函数，生成器输出实际descriptor进入V2；补充12字段默认相等、Global及Control Global、Own透明／0／显式下游和撤回／同实例重挂。原scope兼容不能伪报成原消费失败。
3. 正常产品TreeView完整层级和原数据／尺寸下，新旧明暗默认、配置、恢复及展开／选中阶段同轨迹自然Skia、独立A/A及1DIP，实际查看后清理。不得改默认几何或UX来消差。
4. 当前Mac managed／NativeAOT实际公开View路径及资源；临时源、外部宿主和媒体清理后只跑原TreeView精确模块、Desktop NET8 Release、完整成员／文档及作者另步自审。任何剩余视觉／平台问题保留，完整N2／N3／N4及CT14不能提前关闭。

此版本迁移不改变公开默认字段值；代码仅为已有N3默认求值机制接入。新的交互或视觉变化不在本片；若实际出现，先定位再按主题规范单独审查。

本机实现、资源／视觉／Mac managed与AOT、临时清理及最终57项／NET8已完成，详见[结果与证据边界](container-tree-defaults-results.md)。其余家族验收及新Git审查仍开放。
