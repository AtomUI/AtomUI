# CT17／CT25：StepsProgressBar 标签边界

## 合同与有效前证

原Gallery ProgressBar_Semantic_Preview_Matches_The_Official_Type_And_Gradient_State在真实Skia字体下失败：StepsProgressBar宽115 DIP，indicator右边界116 DIP。保留1280×900、产品主题、渐变开启、四类型切换到steps、Value80／Steps5／80%文案，未改控件尺寸或字体来规避。

AbstractGeneralStepsProgressBar仍按未取整的文字尺寸减算末端坐标，但模板indicator会向上取整宽度。候选仅在HasTemplateProgressVisuals与UseLayoutRounding均开启时按LayoutHelper实际scale取整信息尺寸，再计算既有Start／Center／End坐标；旧自绘模板继续未取整计算。API、Token、步骤数／颜色、窗口及轨道尺寸均不变。

## 脚本与像素

临时CT25RealGeometryVisualProbe通过原Gallery页面公开属性驱动；实际Avalonia.Controls.Window＋VisualLayerManager，Mac arm64 Headless/Skia、RenderScaling1、1280×900。Light配置Default算法，Dark配置完整Default＋Dark算法，并同时设置Window请求variant；两种实际主题整图不同，不能以RequestedThemeVariant单独充当暗色证据。

旧版与候选各2项通过捕获，候选两种主题均indicator右边界115、width115。每个版本／主题独立main、repeat和probe实例；probe单独执行同一根内容Margin+1 DIP→恢复。真实帧采样覆盖Transition时长并连续三帧相同，旧新完整历史相同，没有遮罩、容差或诊断重绘产品代码。

20项整图比较全符合预期：8组独立A/A为0、4组1 DIP均检出、4组同实例恢复为0；4组旧新默认／恢复差异相同，Light238、Dark239像素，闭区间(439,393)–(467,403)。只移动80%文字的末端锚点1 DIP；全部其他像素不变。

已实际查看明暗候选和差异图：轨道、步骤色块、文本、页头／页签、part列表与面板布局保持，差异仅百分比文字位置；原暗色part名称使用黑色的既有Gallery问题不由此修复，不把这项验收扩大为CT21完成。

复现入口：CT25_VISUAL_OUTPUT=<登记的项目外artifacts> CT25_VISUAL_VERSION=old或new CT25_EXPECT_CANDIDATE=0或1 dotnet test tests/AtomUIGallery.Tests/AtomUIGallery.Tests.csproj -c Release -f net10.0 --filter FullyQualifiedName~CT25RealGeometryVisualProbe -m:1 -nr:false -p:UseSharedCompilation=true -p:EmitCompilerGeneratedFiles=false。临时驱动现已删除，源码及媒体SHA和20项数值保存在.superpowers/sdd/gallery-linux-hover/steps-progress-*文本证据。

## 状态与边界

临时C#与旧源备份已删除，净增常驻测试0；现有Gallery原断言、邻近Steps／Progress模块及NET8检查在清理后的工作树继续执行。本次所示默认视觉变化等待用户单独审查，之前普通ProgressBar的批准不自动覆盖Steps。未获Git提交批准；不代表原生显示面、NativeAOT像素或完整CT17／CT25验收通过。撤回的短列表扩展与首次未切算法的暗色采样均不作为本项证据，专用媒体已删除。

## 用户审查

2026-10-25用户明确答复“允许 Steps 默认标签边界修复（推荐）”。批准仅覆盖上述默认百分比1 DIP锚点修复及明238／暗239像素；不含Git提交、原生显示面或完整家族验收。20项比较和媒体SHA文字已保存，当前清理登记的24张取证／差异图片。

清理确认：登记的24张最终媒体及run目录已删除；同实例恢复与差异计数／SHA文字保留。清理后Gallery整个项目Mac412及本机Linux412通过；四常量纯成员移位后Desktop相关111项、Gallery Progress5项和NET8 Release零警告／错误通过。四常量移位前后37个Roslyn成员token完全相同。
