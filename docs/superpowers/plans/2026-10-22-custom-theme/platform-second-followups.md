# CT-25 第二轮 CI 后续包

用户批准的20文件快照已分为成员布局提交938f6e3c9与行为／验证宿主提交1fe3358ec，推送到既有feature/custom-theme。[第二轮验证](https://github.com/AtomUI/AtomUI/actions/runs/40061147545)已结束：三个真实OS NativeAOT、两个Browser模式均通过；三个选测任务仍失败。此次批准已执行完毕，后续变更须重新交用户审查，不扩大为后续提交许可。不创建分支或worktree，不发布NuGet。

Mac选测已通过Build.Tasks 91、Controls.Shared 200、Controls 27、Core 367、DataGrid 309、Rendering 77、Desktop 4,128及Generator 465项；随后Localization.Integration的五项中两项失败。Linux前六个项目通过，Desktop为4,119通过／9失败。Windows在测试选择阶段失败，未运行测试；首轮BOM解析错误已消失。后续项目尚未执行，不能把部分通过写成全部选测通过。

## W5 Windows 选测路径

项目级helper映射使用str(Path(owner).parent)生成glob前缀，在Windows产生反斜杠，仓库索引与匹配规则则使用POSIX分隔符。仅将该处改为Path(owner).parent.as_posix()，不改变选择范围或缺口门禁。

五项临时验证直接运行实际Planner与内存Inventory，使用PureWindowsPath复现三项失败，两个正对照通过；修改后五项通过。实际仓库在Windows路径语义下，三个DataGrid helper输入选择一个项目／42个类，缺口0，18.837秒。临时文件2,619字节已删除，新增常驻用例0；清理后非UTF-8 locale、禁用Python UTF-8模式的planner显式路径tooling iterate通过73.052秒。

## W6 NuGet 元数据文件名

SDK10.0.402生成package/services/metadata/core-properties/nuget.psmdcp，既有两个包内容断言只接受GUID文件名。两处私有断言helper现在同时接受精确nuget.psmdcp与原GUID形式；原目录约束及任意名称、嵌套路径、越界和载荷文件拒绝保持。产品打包逻辑没有变化。

临时16项直接调用实际helper的验证得到2失败／14正对照通过，修改后16项通过；临时文件1,619字节及目录已删除，新增常驻用例0。两个文件全文件成员与控制流审计问题0。清理后原来失败的两个实际打包／消费集成方法在本地SDK10.0.300通过，333.263秒，证明旧格式仍兼容；新版精确文件名由前述临时验证覆盖，尚不冒充新版SDK CI复跑结果。

## W7 Linux Desktop 原触发条件

只读挂载当前checkout及Git元数据、独立产物卷的Linux arm64／SDK10.0.402容器完整复现九项失败，无源码副本、分支或worktree。三个日期确认用例、通知缩放后悬停、四个弹窗中心断言及MessageBox最小宽度均保留原尺寸、动作和断言。

仅将Desktop测试宿主改为UseSkia与UseHeadlessDrawing=false，三个日期确认用例通过；六项仍失败。弹窗中心Y分别为316而非325、401而非410、521而非530，MessageBox实际最小宽度408而非410。通知初次悬停在Skia下通过，缩放后仍失败。该渲染对照与DataGrid首轮证据互相独立。

只读探针记录了目标与全部视觉／逻辑祖先、bounds、transform、clip、z-order、可见性及实际hit。Linux窗口阴影为左36／上27／右36／下45 DIP；1240×820的正文可用frame是(36,27,1168,748)，中心(620,401)。实际弹窗恰在此中心，原断言却使用包含透明阴影的ClientSize中心。[既有正式合同](../../../controls/desktop/feedback/modal/implementation.md#85-overlay-宿主与窗口几何)要求正文按可见frame居中。四个弹窗用例改为按frame断言；自然高度、原窗口尺寸、完整四次缩放、滚动viewport、footer、焦点与用户offset保持。offset在缩小owner后仍断言clamp且不回写，恢复原owner后恢复原偏移。

MessageBox原480×360窗口可用宽度为408，正确容量限制会将410的主题minimum暂降为408。原窄窗口保留，并在同一Surface中补充480→700→480往返：每步断言min(410,capacity)、实际宽度不低于minimum且不越过capacity，验证可用空间恢复时minimum回到410。没有靠扩大原触发窗口删除窄窗口条件，也未改产品minimum或frame。

通知测试在布局后立即输入，合成树可能仍保留旧宽度。精确Avalonia12.1.4的RequestCommitAsync合同为变更在render thread应用后完成。单变量对照保持point(448,95.5)和缩放后(788,95.5)：提交前分别命中根Panel和空；仅等待合成提交后，两点都命中NotificationCard内Border，完整展开／缩放收起／新位置展开／离开收起通过。既有方法增加有界提交等待helper，保持正常motion、尺寸及原输入动作，不按成功条件等待、不改产品事件。九项候选全部通过，30.385秒（含构建）。

临时只读helper、四个源备份及两份完整祖先trace已清理，只保留文字证据摘要；只读插桩在finally中用当前字节恢复。净新增常驻用例0。七个C#文件全文件成员与控制流审计问题0；清理后五个精确相关类在Linux通过126项（82.481秒含构建）、Mac通过126项（74.718秒含构建）。未运行本地change或全量回归。既有Range生命周期失败、DateViewer页头及像素出口、CT-11至CT-23和CT-26未完成项继续开放，CT-24按用户要求暂缓。
