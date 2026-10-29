# CT13 S4：选择手柄父尺寸与反馈间距结果

状态：最小实现、本机专项与临时清理完成，[限定视觉审查](input-select-handle-visual-review.md)已独立批准；完整家族保持开放。

## 根因与最小实现

原三公开根的箭头Width固定Shared.IconSize=14，未到达父配置23/19/17；真实公开Form/FormItem成功反馈场景的IconLayout间距仍8，未到达父SpacingXS=18/11/7。纠正场景准备后6FAIL/3PASS、1s；增加编译专用Suffix Style的类型、Setter值、覆盖与退出资格恢复当前值后6FAIL/6PASS、1s。箭头颜色及真实hover清除尺寸/颜色本来已正确，不混入本包修改。

IconPresenter是独立公开identity，直接在该目标上改OwnerTokenResource会读它自己。候选仅在内部SelectHandle增加一个非空double StyledProperty IndicatorSize，默认0由ControlTheme提供实际父IconSize，常规14，声明AffectsMeasure；四个尺寸Setter用TemplateBinding。IconLayout读取真实父SpacingXS。没有新Own字段、identity、包装层、C#绑定或颜色/清除/反馈状态机变化。Core/IconPresenter原Template优先级转发和生命周期保持，公开子显式值规则未改变。

临时场景初期Form命名空间、LabelText、非泛型IEnumerable编译准备，以及隐藏清除按钮导致Single歧义、默认成功模板不显示“OK”数据的观测失败均保留，未计作产品RED。最终全部通过公开Form/FormItem生成真实成功反馈图标，没有直接向内部节点灌状态。

## 专项与结构门禁

候选13PASS、13s；最终成员整理后直接绘制13PASS、13s，覆盖父尺寸/0/清除、公开Presenter自身41干扰、原颜色/真实hover清除、三生成专用Suffix Style及其关键Setter/当前值恢复、公开成功反馈与实际间距。

原文件缺少规范成员分区，初次结构门禁失败保留。按规则整理公开属性/事件、内部属性、DirectProperty邻接字段、生命周期hook和private方法；50个成员、imports及类型header语法指纹前后完全相同，仅调整位置/分区，未改成员逻辑。静态属性注册的相对顺序保留，初始化无helper先后依赖；最终成员门禁1文件/1控件/0问题。没有用功能测试代替结构结果。

## 全帧及原UX

Skia940×520，三普通根加三公开Form反馈根、四尺寸/明暗、每根固定宽260及原“Original”选择，保留标签、内容、反馈、最小高度和布局；原自然1PASS12s、直接1PASS13s。A/A=0，1DIP扰动可检出并恢复0；旧新完整轨迹一致，末尾真实hover/点击清除第一根选择，其余保持。

300条根类型/反馈形态/尺寸档、数据/allow-clear状态和宽度记录前后相同，300条箭头画刷值也相同。Select的Middle/Custom在配置23时按原自然内容测量从32增为33，宽度不变；这是配置尺寸影响，不是默认几何变化。四档默认、清除、AA/复位及最终恢复旧新均0。

自然/独立直接绘制各50对中24非修复对0，26配置对按真实箭头尺寸/0和反馈间距改变；两后端具体计数分别记录于[视觉审查](input-select-handle-visual-review.md)，不建立容差。已查看原明色Large配置、候选明色Middle配置、暗色Small0及暗色清除完整帧：尺寸/间距生效、成功反馈与原内容保留，0仅隐藏相应箭头，未移除反馈或降低输入能力。

## 原生与后续门槛

实际NSWindow managed初次与最终成员版本各4202项，最终arm64 NativeAOT也为4202项、exit0，覆盖六根/四尺寸/明暗、0/清除、公开Form成功反馈/真实间距、三专用Style覆盖和退出资格。只验证当前Mac API/模板/布局/资源，不是原生最终像素或原生点击。官方Avalonia12.1.4；没有IL2026/IL3050或启动图抑制，五条本机OpenSSL/Brotli部署版本提示保留。NET8 Release0警告/0错误、19.88s。

作者另步自审核对了父输入/公开子身份、全部声明式绑定、实际测量更新、原47个成员逐项代码完全保留及50成员整理前后指纹、无新非Visual绑定宿主/代码订阅/缓存；无独立评审。5个临时C#/AXAML、源码/布局快照和媒体映射已删除；外部工程426文件、1,243,582,999 bytes删除；媒体201文件清理且audit clean。常驻测试净增0，无本包进程遗留。

清理后Select/TreeSelect/Cascader精确148PASS、0Skip、6s；git diff --check通过。全部原始未提交改动保留，26对具体配置视觉已独立批准。

无Git操作；不借本包或其它分包批准关闭完整家族/CT25。

```bash
CT_HANDLE_MEDIA="$PWD/.superpowers/sdd/completion-current/select-handle-media.json" CT_HANDLE_TEXT="$PWD/.superpowers/sdd/completion-current" CT_HANDLE_STAGE=after-direct CT_HANDLE_CAPTURE=direct dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter FullyQualifiedName~Temporary.CustomThemeSelectHandle
dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter 'FullyQualifiedName~AtomUI.Desktop.Controls.Tests.SelectControl|FullyQualifiedName~AtomUI.Desktop.Controls.Tests.TreeSelectControl|FullyQualifiedName~AtomUI.Desktop.Controls.Tests.Cascader'
dotnet build src/AtomUI.Desktop.Controls/AtomUI.Desktop.Controls.csproj --framework net8.0 --configuration Release --no-restore
```
