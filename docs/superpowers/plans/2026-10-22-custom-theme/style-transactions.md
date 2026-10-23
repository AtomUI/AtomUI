# CT-08C 统一主题表现事务

规范依据：[品牌配方](../../../architecture/systems/theming/brand-recipes.md)、[运行时](../../../architecture/systems/theming/runtime.md)。沿用已授权 worktree；主 agent 独立执行；不提交；新增验证默认临时。

## 验收合同

- 同 Token、不同资产必须更新；完全相同输入不得重建 Style。按有序资产身份及修订比较，不能将工厂运行结果或 Visual 放入 Token snapshot。
- root、初次 scope、scope 修改、目录重载与 package append 共享有效表现输入。scope Inherit=false 清空父输入；继承 scope 合并父资产并以本层同 ID 替换。每个 Context 独立资格键。
- 全部 Context 准备与失活接线完成后，在同一 UI 调用栈预激活；复核管理器代际、scope stamp、host 集合、真实 styling 祖先与已接线组。提交只交换引用；Publish 后不回滚。
- 准备/接线/激活失败恢复全部已尝试 Context；恢复不完整或退役不完整必须有表现错误与有界待清理责任。普通观察者异常仍是 Token Publish warning。
- 默认空资产不提前创建 UI 属性对象，不改变已有纯 Token 测试线程合同。不重建控件、内容、焦点或选择。
- 版本化表现结果与 Token 结果分开。清理在下一次变更/显式重试/最终 Dispose 执行，失败不能无界累积并阻止新的资产装配。

## 执行清单

### C1 有效输入和批次原语

- [x] 临时验证：资产顺序/修订、继承重置、相同输入 NoOp、多 Context 激活失败还原、真实宿主拓扑/组篡改。
- [x] `Configuration/ThemeConfig.cs` 内部持有只读资产输入（公开注册/manifest 归 CT-09），`ThemeRequestCacheKey` 纳入资产身份。
- [x] `Styling/ThemeStyleBatch.cs`、`ThemeStyleInputs.cs`：只为有效变化建立 update；捕获完整 Context/host stamps；批次接线/激活/校验/提交/清理。
- [x] `ThemeStyleUpdate`/`HostLease` 公开内部校验门，保证批次引用提交无回调。

### C2 所有管理器路径

- [x] 根请求/FollowSystem、scope 注册/修改：使用统一批次，NoOp 同时比较 Token 与表现；失败无半提交。
- [x] catalog 重载与 package append：同一输入保留表现代际，捕获/校验 host，不能漏掉重入。
- [x] 明确 package 自有候选读取/回滚边界，不能在可回滚 package 阶段提前提交不可回滚表现。

### C3 结果与清理

- [x] 版本化 `ThemePresentationResult` 及 root/scope/catalog 可观察结果；失败诊断与 Publish warning 分离。
- [x] provider/window/root 外层不得仅 Debug 吞掉表现清理失败；待清理持有者可重试且有界；失败 attach 同样登记。
- [x] 临时故障注入：观察者、恢复、退役、失败 owner 迁移、关闭与重入；释放旧 host/graph。

### C4 验收与交付

- [x] 所有关键反例先 RED 后 GREEN，记录实际数量/命令/限制。
- [x] 脚本化 A/A、1DIP 检错、默认前后零像素，以及同实例样式 A→B→A。原生显示面、其他平台诚实标未验证。
- [x] 更新正式 runtime/brand/verification 与任务表；作者自审。
- [x] 删除临时测试/驱动/媒体，完成清理后 Core/受影响 Desktop 精确 filter、NET8 构建与 diff 检查。
