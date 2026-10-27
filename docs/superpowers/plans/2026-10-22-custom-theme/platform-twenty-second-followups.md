# 第二十二批待审快照：Tab本机切片及第二十一轮结果

基线`278429b508275312d8592afed47806b38425e2cd`，现有`feature/custom-theme`和checkout。拟一次提交并推送既有分支，再用AtomUI仓库凭证运行一次既有三OS/两Browser验证；本文件是待审具体范围，不是执行授权。不创建分支/worktree/clone，不发布NuGet。

## 产品与证据

- 产品只改TabControl目录的五文件：34项V2默认、一条CardPadding依赖，以及两类Card添加按钮声明式尺寸和状态退出恢复。没有NavMenu产品改动。
- [Tab结果](container-tabs-defaults-results.md)：原默认3FAIL1PASS→4PASS；历史四项35/51→29，最终11项保护检查通过。真实Mac NSWindow/scale2 managed1062/AOT1063、408资源相同。
- [所示布局及N01–N16](tabs-layout-noise-review.md)已批准；原始自然FAIL不抹去。直接绘制20AA/6恢复/36兼容对均0，自然16对限定于冻结的帧/计数/坐标/SHA，没有通用容差。
- 8临时源、303媒体、423文件原生工程已清理，媒体审计clean。清理源码后的原Tab283项/0Skip、未改NavMenu原307项、NET8 Release零警告通过。完整三份C#成员审计无问题；作者按主agent约束另步自审，不称独立评审。
- [第二十一轮](platform-twenty-first-results.md)attempt1的8个job均成功，三OS各6405选测、三NativeAOT各26、两Browser各23，源码均为基线；各370资产和27对本地化阶段完整。没有重新调度。
- [NavMenu调查](nav-menu-initial-collapse-motion-results.md)只保留设计、TDD与被否决候选的文本：原13FAIL2PASS，候选13PASS2FAIL，普通Border的Animation coercion也1FAIL；产品已恢复HEAD。

## 实施偏离与风险

水平高度从设计中的DesiredSize路径改为有通知的Bounds.Height，绑定放在模板内Style Setter以保存NameScope；这两点由原29/NaN反例、完整方向恢复及GC证明，属于当前实现判断。自定义模板中的测量请求与实际Bounds可能不同，不能以本片代替完整家族验收。

直接原生Styles.Remove残留、HorizontalMargin的Undecided语义、NavMenu有效宽度取值、其他Host/输入/持续动效/原生最终显示面及完整CT11–CT19/CT25仍开放。CT24暂缓；CI或分包通过不会关闭这些任务。

## 精确范围（27文件）

产品5文件：

- `src/AtomUI.Desktop.Controls/TabControl/TabControlToken.cs`
- `src/AtomUI.Desktop.Controls/TabControl/CardTabControl.cs`
- `src/AtomUI.Desktop.Controls/TabControl/TabStrip/CardTabStrip.cs`
- `src/AtomUI.Desktop.Controls/TabControl/Themes/CardTabControlTheme.axaml`
- `src/AtomUI.Desktop.Controls/TabControl/Themes/TabStrip/CardTabStripTheme.axaml`

正式文档9文件：TabControl的overview/implementation/token/semantic-part/changelog，以及TabStrip的overview/implementation/semantic-part/changelog，均位于`docs/controls/desktop/navigation/`下对应目录。

计划与记录13文件，均位于当前计划目录：

- `container-consumers.md`
- `control-inventory.csv`
- `token-contract-review.csv`
- `tasks.md`
- `container-tabs-defaults.md`
- `container-tabs-defaults-results.md`
- `tabs-add-button-layout-design.md`
- `tabs-layout-noise-review.md`
- `nav-menu-initial-collapse-motion.md`
- `nav-menu-initial-collapse-motion-plan.md`
- `nav-menu-initial-collapse-motion-results.md`
- `platform-twenty-first-results.md`
- `platform-twenty-second-followups.md`

不包含临时验证源码、媒体、外部工程、构建产物、`.superpowers`审计工作区或后续研发。具体27文件的blob及补丁摘要由`.superpowers/sdd/container-tabs-audit/batch22-snapshot.json`冻结；只有针对该快照的新授权才能执行提交。
