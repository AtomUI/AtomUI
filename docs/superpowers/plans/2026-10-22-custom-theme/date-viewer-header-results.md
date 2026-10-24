# CT-16 D4-C 页头原型与默认视觉审查

状态：本分包已验证并清理，默认范围页头对齐修复已获明确批准；日期完整家族及CT-25既有视觉／释放问题继续开放。执行依据见[计划](date-viewer-header.md)。没有新增公共API、Token字段或ID。

原Content范围viewer的两个页头均固定288 DIP，实际面板在显示周列时各420 DIP、无周列时各372 DIP；次页头起点也因此与次面板分离。原960×760范围viewer、1560×1220窗口、四个实际viewer、日期2026-10-15／范围15至18、禁用22日、周列均保留。

原型在三个DateViewerHeader各自的Styles中使用无Selector的Style setter，将Width编译绑定到其模板内对应DatePanel.Bounds.Width。精确Avalonia12.1.4的Style.TryAttach规定无Selector仅匹配host自身，避免向嵌套owner扩散；仍处在ControlTemplate namescope，编译绑定可解析，不依赖反射或C#找节点赋值。单独移至ControlTheme外层Style的原型因编译期找不到模板名字而被撤回；直接写Width属性的简单原型也不能满足专用Style优先级。最终候选不增加PART名称。

原直接Width的优先级为LocalValue；实际生成的RangeDateViewerHeaderStyle已命中Tag、Height61和紫色Foreground，但Width315被288覆盖。候选让专用Style同时覆盖Width315，失活资格class后恢复按实际面板对齐。Sibling未被污染，公开HeaderTemplate仍生效。该定制使用编译AXAML的专用Style，没有代码部件赋值。

七项临时验证在相同最终驱动下，原版为4失败／3正对照通过，候选7项通过（45.854秒含构建）。覆盖Compact／Content与周列、同实例切换和Own宽度更新清除、HeaderTemplate／ShowHeader、专用Style覆盖与退出、实际时间范围Picker单面板。Own宽度属于既有DateViewer Own家族；RangeDateViewer仅有独立Global身份，错误地对其配置不存在的Own不能算有效验证。时间Picker使用既有产品Window提供真实overlay；最初普通无模板Window的无popup错误已排除。

## 当前 Mac Skia 视觉证据

原版、候选均重放默认→关闭周列→恢复周列→隐藏范围页头→恢复页头的完整历史，每个版本另建独立A/A与1 DIP场景。两种实际主题均通过公开ThemeManager应用并断言Window.ActualThemeVariant，650ms采样窗口未关闭动效。全部按原坐标及1560×1220全图RGBA精确比较，无缩放、对齐、遮罩或容差。

| 历史状态 | 明色变化像素 | 暗色变化像素 | 全图变化范围 |
| --- | ---: | ---: | --- |
| 默认显示周列 | 2,302 | 2,302 | (697,433)至(1438,465)，Content范围页头 |
| 关闭周列 | 2,060 | 2,060 | (697,433)至(1342,465)，Content范围页头 |
| 恢复周列／恢复页头 | 各2,302 | 各2,302 | 同默认范围 |
| 隐藏范围页头 | 0 | 0 | 整图相同 |

20组独立A/A全为0；20组1 DIP正对照全能检出，范围29,975至33,650像素。已实际查看明暗新旧默认图、默认差异图、无周列与隐藏页头图：标题在对应面板居中，前后翻页按钮位于外缘，底线完整对应面板；日期、选中范围、禁用项、单viewer及Compact区保持。全图差异范围也证实这些区域没有变化。

旧版自身暗色往返仍有20／10个字形像素差异，候选同一历史与旧版逐状态比较没有新增该差异；该既有问题继续归CT-25，未用容差豁免。原生最终显示面不由本次Skia比较证明。

媒体已在实际审查及批准后清理，audit通过；没有提交或嵌入报告。默认视觉批准仅针对上述页头对齐差异，不等于提交或家族整体验收。

## 原生补充与驱动纠正

当前 Mac 的独立页头专项 NativeAOT 编译成功（136.978秒），实际窗口运行在单面板 Own 宽度更新断言失败，不能记作原生验收通过。此前通过的三个 OS 标准运行任务不覆盖本专项。

已通过的原生检查包括双面板对应几何、周列切换、Compact Own 更新／清除、编译专用 Style、公开 HeaderTemplate、页头显隐，以及时间范围 Picker 确实采用单面板且初始页头与面板等宽。失败时面板 Width 属性已经是344，但实际面板、页头及 viewer 都仍是288；实际客户区为1560×982，宿主系统约束了请求的1220高度，因此也不能将它称为与Skia全图相同的原生像素证据。

相同原生 JIT 驱动复现同一失败：header (0,0,288,32)、panel (0,0,288,269)、panel.Width344、viewer (0,0,288,301)。单独把 RunJobs 驱动改为真实 Dispatcher.MainLoop 仍复现；“未泵送原生消息循环”的假设已否定。

继续只读观测证明失败时pickerOpen=false、viewer未附加、没有TopLevel：临时驱动把额外Picker放在y=1186，超出了Mac实际982高的客户区，Popup按既有离开可见锚点合同关闭；失败检查误读已卸载的旧布局。这个Picker位置是专项驱动新增内容，不属于原四viewer复现场景。单变量只将额外Picker的y从1186改为0，原四viewer位置、尺寸和完整操作历史保持；同一JIT驱动两种主题共72项检查通过（原生运行21.879秒）。更新时实际pickerOpen=true、viewer附加、panel／header／viewer均344，清除恢复288，没有放宽断言或关闭后重开规避。

修正驱动的NativeAOT编译88.617秒，实际原生运行20.491秒，完成72项检查并正常退出。全部临时入口、主题及Style覆盖均在finally恢复执行前的三个源码文件字节；采用已批准候选后的finally也保留该准确候选。此前无效宿主运行与链接环境失败记录保留，不计作产品失败或通过。原生几何检查仍不等于1560×1220最终显示面像素验收。

用户在2026-10-24明确回复“允许默认范围页头对齐修复”，覆盖本记录中2,302／2,060像素的对齐变化与专用Style优先级修正；未扩大为Git提交批准。

## 输入、生命周期与清理后的出口

保留原四个viewer及窗口、数据、周列和正常动效，按第二日期面板右缘计算真正输入点。原版有／无周列两项均在实际命中断言失败；候选的框架指针事件各产生一次导航Click，翻页再返回不改原范围。真实键盘右移只改变焦点，范围点击19→禁用22→21保持禁用防线并提交19至21。模板替换／恢复生成新的Header和Panel，随后周列、Presentation往返继续对齐；移除owner后，保留原窗口的5个WeakReference全部释放。既有“保留owner时旧模板部件未释放”仍由CT-25单独处理，未因此填作已修复。

最终11项临时功能／输入／生命周期检查通过（39.474秒含构建），已删除5个临时源／AXAML共29,450字节、6份源备份／驱动共38,070字节，以及独占原生发布输出1,059个文件共900,209,821字节；全部媒体已清理。净新增常驻测试0。

清理后执行`dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter 'FullyQualifiedName~DateViewer|FullyQualifiedName~Calendar|FullyQualifiedName~DatePicker'`，337项通过；Desktop Release NET8构建0警告／0错误（33.36秒）。正式implementation、token、semantic-part、changelog及三个目录的限定备注已同步。作者自审确认产品变更仅一个RangeDateViewer主题的三处Width绑定位置与来源，无新模板名字、代码部件赋值、运行时反射或ID改变。
