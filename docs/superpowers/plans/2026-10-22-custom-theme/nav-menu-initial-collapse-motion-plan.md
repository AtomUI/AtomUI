# NavMenu 初始折叠首次展开实施计划

**Goal:** 恢复初始折叠菜单首次展开的既有宽度动效，保持属性优先级、Auto/0、约束和取消合同。

**Spec:** [已批准短设计](nav-menu-initial-collapse-motion.md)。主agent沿当前目录执行；不创建分支/worktree/clone，不发布NuGet，新Git快照另审。

**Architecture:** 在现有Width coercion入口保留本实例请求宽度，展开目标读取该输入；不改变Prepare顺序、计时器、easing、duration、内部布局宽度或原生优先级。若优先级实验不支持此方案，停止该实现并修订设计。

## 顺序与接口核对

本片只改`src/AtomUI.Desktop.Controls/NavMenu/NavMenu.cs`；临时验证放所属测试项目`Temporary/CT14NavInitialMotion`，最终删除。TabControl修复与本片没有共享产品接口，串行构建，分别验证与记账。没有新增公开API或跨模块依赖。

- [ ] 原420×240、Inline、Width300、初始折叠48的首次展开RED；先正常展开再折叠的轨迹作正对照。监听真实Bounds事件，不靠某次轮询恰好采到中间帧。
- [ ] 原生Style、Local、绑定及Animation优先级、折叠时修改宽度和撤销覆盖的目标核对；低优先级变化不得替换实际获胜输入。记录实际目标与最终Bounds。
- [ ] 0、Auto初始与有历史回退、Min/Max约束、关闭动效、展开中反向、切模式、卸载取消，分别锁定现有合同。已有正常行为可为原版PASS，不冒充有效RED。
- [ ] 最小修改coerce输入与展开目标；同一有效RED转GREEN，上述保护项保持。仅一个实例double，无Visual引用或新订阅。
- [ ] 实际明暗场景首帧/中间帧/终帧与默认/恢复、原生managed/NativeAOT逐项验证；保留原始视觉失败和计数，不用通用容差。
- [ ] 删除本片临时源码、媒体和工程；清理后的精确`FullyQualifiedName~AtomUI.Desktop.Controls.Tests.NavMenu.`、NET8 Release、完整成员审计、文档与`git diff --check`。

## 精确命令与出口

临时RED/GREEN：`dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj -c Release --framework net10.0 --no-restore --filter FullyQualifiedName~Temporary.CT14NavInitialMotion`。预期原首次展开用例因没有中间Bounds失败，候选通过；编译异常不计RED。

最终精确模块：同项目同配置，filter为`FullyQualifiedName~AtomUI.Desktop.Controls.Tests.NavMenu.`，预期0失败、0Skip且非零用例。NET8仅构建实际产品模块。具体计数、源码身份、测试处置和未覆盖边界写结果记录；不以本片或CI通过关闭完整CT14/CT25。

新提交仅在具体快照另获授权后执行；本计划没有自动提交步骤。
