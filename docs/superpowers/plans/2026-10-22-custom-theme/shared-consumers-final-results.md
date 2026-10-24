# CT-10 根排版与共享消费总出口

日期：2026-10-24。继续既有 `feature/custom-theme` / `AtomUIV6-custom-theme`，基线 `5381fb6c9`，未提交。前序 S1–S4 与附加区分包的证据继续有效，本页补齐剩余根 owner 和总出口。

## 实际修复

- ComboBox、Select、Cascader、TreeSelect、DatePicker、RangeDatePicker、TimePicker、RangeTimePicker、NumericUpDown、公开 ButtonSpinner 的可选字体族接入真实继承父与控制配置存在位。根字号使用单一原生主题 Setter，实际前景进入框体和内部输入，保留完整 typeface、状态及显式覆盖。
- Select/Cascader/TreeSelect 的 Small 继续读取 FontSize，默认 14；ComboBox/日期 Small 使用 FontSizeSM。Custom 沿原有 Middle 字号默认，未扩大尺寸接管。
- NumericUpDown 保留原真实内嵌文本 14 DIP 默认；根 FontSize 成为真实输入，内部 TextBox 不再重新覆盖。IsCustomFontSize=true 恢复真实继承父，23 DIP 反例取得失败后通过；普通/自定义两项均通过。内部 NumericUpDownSpinner 不接受公开 ButtonSpinner 的可选默认。
- 超链接、可选文本、CompactSpaceAddOn、滚动、SplitView/Splitter、Masonry、Window/标题栏、Icon、ArrowDecoratedBox、Popup 与焦点 AdornerLayer 的既有 Global 字段接通自身身份；CandidateList/Item 的共享 ListBox 主题沿实际 owner 读取 Global。未改其历史 Own 命名依赖。
- 只新增内部只读继承桥、有限字号选择与 AXAML 接线；没有新建身份、Own 字段、并行主题引擎或模板节点代码定制。五个生产控件的成员布局审计通过；移动前后方法/成员 token 指纹相同。

目录全部 **104** 个主键的逐项裁定见[实际消费者表](shared-consumers-catalog.md)：其中 **25** 项真实登记，其他对象按抽象层、布局、源 Visual、内部模板或 framework asset 处理。表内静态无独立消费结论不计作 104 个独立视觉场景。

## 失败与通过

原源码精确重放 53 项为 **32 FAIL / 21 PASS**；最后 Icon/箭头框/Popup/Candidate 两项/焦点 Border 六条链分别取得有效失败。最终合跑 **59/59 PASS，17 秒**，包括十 owner 的动态配置/恢复/原生优先级、完整字体和前景、同 context 迁移/兄弟隔离、长期父节点移除后的 GC。数值自定义字体补检 **2/2 PASS，568ms**。

无效夹具不计入失败：namespace 与类型冲突、普通 Avalonia Window 没有目标 AdornerLayer、未规范身份的本地资源 key、错误的 `600ms` 字面量，以及动画中间值比较。修正后分别使用实际 AtomUI Window/登记 identity/合法 TimeSpan，并等待真实过渡结束。原源码 A/B 的 44 份文件逐字节恢复。

## 兼容与实际像素

| 输入 | 旧行为 | 最终行为与兼容 |
| --- | --- | --- |
| 控制级字体族/字号/前景 | 部分根忽略配置；内部文本仍用共享默认 | 实际 owner 配置进入真实目标；Style/本地属性优先 |
| NumericUpDown 原生 FontSize | 内嵌字号预设重新覆盖 | 根值到达实际文本；原默认 14 保持，自定义标志允许继承 |
| 注册共享对象 Global | 多处直接读取 Shared | 自身登记身份生效，组合公开子控件保持独立 |
| native/framework 资产 | 没有独立产品身份 | 保留 scope Shared、原生属性或明确 owner 投射 |

外部驱动使用真实 Skia、HeadlessDrawing=false、缩放 1，明暗实际主题分别启动；没有 CUA。主场景为 40 个根实例和 6 个共享控件，扩展场景含真实 AtomUI Window、Icon、箭头框、候选列表/项、ListBox、键盘焦点及已打开 Popup。同一实例完成配置/字体往返，尺寸、裁剪、焦点和打开状态保持。

- 初始 46 控件：默认前后 0；A/A=0；独立 1 DIP 检错明/暗 **44123/18036** 像素，恢复 0；原生字体变化 **25862/25857**，配置变化 **302529/302601**，均恢复 0。
- 扩展场景的默认、焦点、已打开 Popup、原生几何变化/恢复、配置恢复和关闭按相同轨迹比较。首轮配置触发焦点边缘 **13/11** 像素变化；以原生 item.MinHeight 32→40→32 单变量重放，旧源码精确复现相同变化，最大通道差 **12/6**。同轨迹旧新为 0；后续配置往返为 0。未裁剪、遮罩、容差或把关闭弹层当作恢复证明。这是既有原生重绘事实，纳入 CT-25 兼容证据，不伪称修复。
- NumericUpDown 自定义字体在 JIT/NativeAOT 驱动中同时断言根与真实内部文本为 23，再恢复。

当前 Mac/osx-arm64 NativeAOT 扩展场景实际运行通过；明暗各 18 阶段，共 **36 帧** 与 JIT 全部 0 差异，包含自定义字体标志与已打开 Popup。没有 IL2026/IL3050；保留已有 Homebrew OpenSSL/Brotli 链接库以 macOS 26 构建而 target=12 的链接器提示。未据此推断其他 OS。

## 清理后模块验证

59 项与补检 2 项临时源均已删除，常驻用例净增 0。三份既有测试同步实际契约：TreeSelect 改用实际小尺寸 root/frame；Splitter 和标题栏的资源归属断言同步新 owner。

- Desktop 最终合并精确 filter：**960/960 PASS，22 秒**；移除空选择器后 ComboBox 最终组 **13/13 PASS，898ms**。
- Core 资源扩展与桥：**3/3 PASS，157ms**。
- Desktop NET8 Release（含 Core/Common/Shared 依赖）：**0 warning / 0 error，15.83 秒**。
- 六文件成员审计：**0 issues**；仅移动布局的成员 token 指纹一致。

完整控件家族 Own、业务状态、持续动效与其他平台仍依各自 CT 任务验收；本出口负责 CT-10 的共享消费合同。

视觉媒体 **208** 文件、外部驱动及 bin/obj/AOT **423** 文件、14 份源 A/B/审计脚本及备份已删除；cleanup 自带仓库媒体 audit clean。61 项临时验证全部删除。两份主目录仍为 721 个原主键，CT-10 的 104 项仅同步消费审查状态，未把候选场景写成独立像素验收。145 条修改文档本地链接无缺失，diff check 通过。

作者另行自审了最终 owner、字号/默认/自定义标志、公开子控件隔离、父订阅释放、AXAML 原生优先级和完整差异。自审补检捕获并修正 NumericUpDown 自定义字体回退；无未解决阻断项。没有独立 reviewer，遵守本任务不使用 subagent 的约束；没有创建分支/worktree 或提交。**CT-10 S1–S5 已全部完成**。
