# CT-11 动效总出口结果（已完成）

本记录承接[M1–M4](motion-consumers.md)、[原集合目录](motion-transition-catalog.md)及[本轮执行计划](motion-exit.md)。源码基线为`4560091deea4e56c14e89a70e2f418e72a8afd64`，沿用官方Avalonia12.1.4、既有分支和工作目录，单agent执行。2026-10-29已完成CT11动效消费合同出口、两份限定视觉审查、临时验证及媒体清理，并核对最终同一源码的991项聚焦结果。Windows专用控件真实平台、原生最终像素/输入及已接受的有限合成差异继续按CT25独立验收；不代表CT12–CT20完整家族通过。

## 本轮补足的消费链

- 118个可独立挂载的公开动效owner，初始Control Global停动配置有72个有效不匹配；DataGrid和GalleryBase各追加1个。现有开关Setter改用实际owner资源，Button/IconButton/ToggleIconButton继承主题不再固定基类identity。没有新增时钟、订阅引擎或公开开关。
- NavMenu整体宽度motion原只读取Application Mid。4个有效RED与1个默认正对照，经AXAML接入内部既有MotionDuration属性后5PASS；默认200ms、CubicEaseOut、timer、宽度恢复算法保持。本轮不重启此前否决的宽度恢复或框架源码补丁。
- 13项代码动画输入原均未使用Control Global；CollapseItem的第一次预期误配父Collapse，已按其独立identity纠正并用旧主题重验RED，再恢复候选。保持公开子Control identity。
- Drawer、Overlay Presenter/Surface、Window Surface、Count/Dot Badge六场景初始全部有效RED。五个主题入口读取已知公开owner；独立Window复用ThemeContextLease，优先有效逻辑owner，回退原物理owner。
- Drawer及Overlay热更新仍失败，单变量原生LogicalChildren实验两组不加入FAIL、两组加入PASS。正式打开/关闭对称管理真实逻辑成员；未生根Dialog的Overlay复用已有DialogResourceBridge，独立Window使用placement target作为资源owner回退，保持物理modal ownership。
- 两个浮动Host子按钮原800ms实际200ms；补完整逻辑成员后，候选曾把原`584,424,40,40`位置退化为`0,0,40,40`。两个明确几何RED保留；改为读取真正VisualParent并对称释放既有SizeChanged订阅后，位置、热更新及detach/reattach均通过。
- Tour遮罩补完整逻辑成员，已有mask跟随scope热时长。共享层转交同时清理前owner引用，关闭旧Tour不再隐藏或解除新owner遮罩；两项有效RED变2PASS。

## 合同分类

BorderBeam保留已有独立装饰动效合同，常驻测试明确要求不因Global停动而关闭；本轮没有改变该边界。

Default/TextTourIndicator、FormItemDecorator、两种NavMenu header、ScrollBarThumb、SplitView及UploadTrigger的8个原生输入不能仅凭元数据中有bool属性推断为独立Global消费者：它们由组合owner传入或是显式原生开关，Tour indicator本身没有循环动画。原默认false及Local true/false分别验证，不添加无实际效果的getter式接线。完整家族的其他Token/Part/状态合同继续由CT12–CT20承担。

当前93组动态集合是M4原83组加M2及后续按钮消费入口；实际模板source/selector探针精确匹配85组，DataGrid真实row/header/expander3组、GalleryBase1组、实际Pagination icon1组及两个兼容Indicator primitive分别补验，共92组本机实际路径。Windows专用CaptionButton的真实平台状态保留CT25；不将macOS上的类型物化视作Windows UI验收。兼容Indicator primitive无新增内置使用，Thumb默认0、明确Card owner900ms及清除0保持。

## 已取得的验证

| 对象 | 实际结果与边界 |
| --- | --- |
| Desktop临时精确切片 | 46PASS，19.1582s；后续仅增加临时视觉驱动，未清理前不作为最终树receipt |
| DataGrid | 原开关1RED/实际三个集合1正对照；候选2PASS、3.9324s |
| GalleryBase | 原开关1RED；候选1PASS，时长800→456且集合身份保持 |
| 跨根输入 | 附加/未附加Dialog及两个Host，最终11PASS、6.3162s；含热更新/0/清除 |
| GC | 8类关闭图、33个对象全部回收；1PASS、4.1885s，不以RSS代替对象证据 |
| 当前Mac managed | 实际NSWindow，120个owner、357项检查PASS |
| 当前Mac NativeAOT | 实际arm64 Mach-O执行同样357项PASS；不等同原生最终像素或原始输入 |
| NET8 Release | GalleryBase含Core/Shared/Controls/Desktop零警告错误24.19s；DataGrid零8.23s；ColorPicker零6.93s |
| 既有图/交互模块 | Drawer/FloatButton/Dialog/Tour/Window/NavMenu精确namespace合计912PASS、0Skip、33s，清理前检查 |
| 成员布局 | 10文件、9控件、0问题；另步整理5文件，前后完整token snapshot JSON一致，初始化顺序单独审查 |

NativeAOT发布保留5条Homebrew OpenSSL/Brotli的最低系统版本链接警告（库OS26、target12），不声明macOS12已验证。没有新增IL2026/IL3050抑制。

Native驱动曾因Opened在Hide/Show后重新触发而重复启动，已停止该轮，改为事件先退订的一次性驱动后重跑；旧运行不计通过。外部临时工程还纠正了`/var`与`/private/var`路径和扩展命名空间准备错误，这些不计产品RED。

## 视觉与最终出口

候选Light/Dark各121根场景、522帧；同版本稳定取样与独立校准先执行。Light校准AA0、1DIP1009、恢复0。对照使用同一当前目录临时精确回放基线的76个源码文件，完整相同历史执行后finally按SHA恢复候选；不创建checkout、分支或worktree。

两侧四次独立校准均AA0/1DIP1009/恢复0；新旧完整轨迹Light522对中520对0，Dark522对中505对0。所示[12对暗色传播](motion-exit-dark-scope-review.md)已获用户限定批准。其余7对及独立重放新增的1对见[8对合成历史审查](motion-exit-compositor-review.md)，也已获用户明确限定接受，继续保留CT25专项；原始FAIL计数不改写，没有在产品增加强制重绘或通用容差。指针和有效状态一致的单变量整窗重绘16对均0，只作为合成损伤历史定位。

临时验证清理：Desktop11源64964bytes、DataGrid1源4680bytes、GalleryBase1源2320bytes均已删除，常驻净增0；外部Native工程/managed/AOT共466文件1475633787bytes已删除。登记媒体运行2261文件已清理，Git媒体audit clean，无取证二进制进入仓库。所有Temporary目录、外部驱动目录和媒体目录均无本包残留。

最终清理后精确验证：图/交互6namespace912PASS0Skip35s；DataGrid主题及真实detail expander42PASS0Skip16s；GalleryBase IconInfoItem2PASS0Skip653ms；Controls Avatar7PASS0Skip352ms；ColorPicker/有效Global/按钮scope28PASS0Skip2s，合计991PASS。Gallery原静态状态检查仍期待Shared入口，已同步为Owner入口后重跑通过；未新增常驻用例。媒体清理后再次核对76源码候选SHA与上述检查输入严格相同，按仓库同输入证据复用规则不重复扩大执行。作者另步自审及文档本地链接检查通过，无独立评审或子agent。CT11合同内无未决子任务；CT25及其他家族边界如上，未创建commit或push。
