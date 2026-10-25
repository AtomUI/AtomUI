# CT25：Picker 有效模板替换的旧部件释放

沿现有分支／checkout，由主 agent 实施，不创建分支或 worktree，不自动提交；本包不属于冻结的 30 文件快照。

## 范围和设计

修复当前 RangeTimePicker 原始临时失败，保留 Generic13/9、真正 null→新模板操作、全部业务选值和复用 presenter。验收扩展到旧视觉树中所有没有被新树合法复用的部件。无祖先条件／有祖先条件的原生控件以及不加载 AtomUI 的独立 Avalonia12.1.4 程序确认一条框架模板样式帧保留路径；另外两条实际路径为复用 Icon 的旧 TemplatedParent、旧 Popup 子节点中的模板绑定。

逐项撤去证明三条路径均需处理；三项诊断组合可释放两个原代表部件，所有非复用旧部件组合也通过。最初全旧节点检查的一项“失败”是仍在新树使用的图标模板内部节点，现按实际对象身份排除合法复用，保持被替换部件的释放断言。

实现采用限定于 Picker 模板替换的兼容边界，不注册全局 StyledElement 拦截：

1. Core 的 TemplatePartThemeCompatibility 只跟踪显式传入 owner 的现有部件，在 TemplatedParent 真正改变时清除旧 TemplatedParentTheme 帧，静态回调立即取消订阅，ConditionalWeakTable 防止重复且不强持有节点。调用唯一已核实的 Avalonia 内部方法，固定反射与精确保留声明都留在 Core Reflection 边界，不扫描类型或新增资源引擎。
2. IconPresenter 在实际图标挂载／卸载时同步模板归属，并清除旧模板主题帧；保留已有 Template 优先级的绑定、尺寸、画刷和显式值优先级。
3. InfoPickerInput 有效重套模板时跟踪旧部件；新模板就绪后关闭退休的旧 Popup、释放其模板自有 Child 的旧归属并断开 Child，再装配新 Popup。保留 public PickerPresenter 实例和已提交／草稿状态，不通过重建 presenter 满足 GC。

这里的出口限定为已有有效模板被有效新模板替换；永久 Template=null 的全部缓存字段语义未在本切片验收，不据此填写整个 CT25 完成。

## 实施和出口

- [x] 原失败、原生正反对照、真实12.1.4方法及剩余堆路径；独立程序、两份堆和反编译文本已记录后删除。
- [x] 仅诊断验证最小三项组合；未据此宣称产品修复。
- [ ] 同一场景无诊断操作的产品 RED→GREEN；默认及展开／固定弹层的重新绑定、选择／草稿、旧 Popup/Child/图标及全部非复用旧部件生命周期。
- [ ] 检查接触文件中的绑定边界；可等价声明的迁入 ControlTheme，不能等价者写明实际拓扑、时机或优先级原因。保持成员布局。
- [ ] 严格默认、重套模板及配置复位视觉对照、A/A／1DIP；原生 managed／AOT、NET8，保持选值、输入几何和弹层交互合同。
- [ ] 删除临时测试、驱动、备份及媒体，更新正式共享输入／图标文档和文本结果，清理后只做明确模块的精确验证及作者自审。不混入待批准 30 文件提交。

## 2026-10-25 后续机制与已执行证据

有效模板替换在 native OnTemplateChanged 提前解除旧 Popup／时间输入监听；OnApply 通过正常 Content API 交接复用 presenter，再释放旧 Popup 的模板自有 Child。新模板完成后的 Loaded 回调才装配原 Local 优先级的 pin relay／打开弹层，避免 Popup.Show 同步布局重入尚未完成的 native ApplyTemplate。回调仅持有弱目标、整数版本及标量焦点状态；连续替换保留编辑位置，外部焦点及卸载取消恢复。

Desktop ArrowDecoratedBox 退休时仅清理实际 Visual 节点自己的原生合成 Children，阻断 Avalonia12.1.4 Border 保留的旧 parent 链，不遍历或删除应用提供的 ChildCompositionVisual 子树。TimePickerPresenter 重新挂载已活动的编辑面板时保留草稿。

诊断清理后的相同17项场景：原产品17FAIL55.334s，候选17PASS73.460s；连续替换、外部焦点、卸载和应用合成子树所有权另6项通过。加入相邻时间／日期编辑与 Popup 检查，68项通过66.785s。早期4个 GC 失败源于临时测试自身局部变量保留，已限定并改为 NoInlining helper，没有计作产品反例。

真实 Mac managed：Avalonia.Native.WindowImpl、scale2；16个 Overlay 加16个 Native PopupRoot，共32场景／739检查通过159.844s。使用实际原生键盘设备和 input manager，typed GetPresentationSource():IInputRoot，断言事件实际到达。第一次 false 请求用 SetCurrentValue 被主题 Setter 覆盖，其16个 host 失败不计 Native 证据；后改为明确 Local 配置，没有改产品强制 Host。

Native 反例证明新容器已列出复用 presenter，但旧清理将其 Parent 清空；诊断单变量补父节点后通过，产品改为正常 Content 交接，未采用 ISetLogicalParent 强制写回。新增检查还需确认后续公开 PickerPresenter 更新继续经原 TemplateBinding 生效。

AOT 首次已生成原生目标文件，链接缺本机 OpenSSL 搜索路径；仅补已存在的 Homebrew openssl@3／brotli 的 LIBRARY_PATH 后发布通过12.083s，无 IL2026／IL3050，保留5项本机库最低系统版本警告。实际 AOT 运行仍在进行。严格像素证据、NET8、正式文档、临时清理和最终模块检查尚未完成，不据 managed 通过宣布完整家族或 CT25 验收完成。

8文件／5控件成员审计零问题；TimePickerPresenter 五个内部方法补专用 region，非 region 非空源码行哈希相同。已审查 runtime presenter／Icon 的拓扑、Local／Template 优先级和时机，现有代码保留具体兜底原因；最终清理后仍需复核。
