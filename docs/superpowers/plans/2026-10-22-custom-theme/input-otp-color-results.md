# CT-13 OTP 分隔符颜色结果

用户2026-10-24明确批准默认分隔符颜色修复；原型普通明／暗变化2560／2960像素、禁用2960／2880像素。本分包依[计划](input-otp-color.md)执行。产品只在OtpLineEdit ControlTheme增加根Foreground的ColorText Setter与禁用ColorTextDisabled Setter。通过原生继承进入separator，不新增Own／ID／API；cell输入表面、字体、墨迹测量、内容和输入合同保持。

7项临时验证的有效RED为4FAIL／3PASS（1s）；正对照为根本地值、专用Semantic Part Style及明色普通默认。修复后7临时＋66既有OTP共73PASS（3s）。测试使用编译AXAML内生成的OtpLineEditSeparatorStyle及TextElement.Foreground，锁定专用Style／Setter／无Name、Loaded、Unloaded代码回退，并验证普通／禁用真实命中与优先级。最初错误引用运行时XAML loader的编译失败不计RED。

脚本为1480×690、scale1的真实Skia Headless Window，4variant×4尺寸、96cell及80可见separator，祖先Arial19、固定123456，普通motion。19阶段包含A/A、1DIP、禁用／恢复、只读、字体配置／清除、实际键盘输入654321、控件颜色、透明、清除、根本地前景、专用Style及停用／移除。原版与候选执行完整相同历史；原版每主题112颜色失败，候选两主题全部通过。逐行只读对照：root320行全部值相同；cell1920行含Foreground完全相同；separator1600行几何／字体相同，前景按批准范围变化。实际输入值和cell引用保留。

四张默认普通／禁用画面与用户批准的proposed原型全帧RGBA均0；相对原版普通2560／2960、禁用2960／2880像素。A/A=0，1DIP检错27314明／17132暗，扰动恢复／启用恢复／只读恢复／字体清除均0。颜色清除后相对输入阶段另有46个y=52边缘像素，最终恢复35个，布局／实际前景相同。用原版已有的公开根Style声明同样ColorText／ColorTextDisabled输入，再重放完整19阶段：明暗38张与候选逐像素全部0，包含边缘变化。未裁剪、遮罩、容差或重建一侧消差；具体原生栅格函数仍由CT25核查，不将同源复现表述为已修复栅格问题。

另在首轮复现原生Styles.Remove后旧紫色仍保留，旧源／候选相同。本地Avalonia StyledElement.DetachStyles只遍历逻辑子树；模板节点的清理差异已由CT08托管lease处理。后续颜色轨迹使用同一AXAML专用Style的根class资格，先停用再移除，两侧相同；该原始失败未删记录，原生直接移除问题归CT25，不能将其算作本颜色修复解决。Style实际route命中已通过，未采用代码Part赋样式。

当前Mac NativeAOT发布及明暗38阶段实际断言全部通过，与JIT38张全帧RGBA全部0；无IL警告，5条Homebrew OpenSSL/Brotli较新macOS dylib部署目标警告单列。此为当前Mac执行和Skia合成器证据，不代表原生最终显示面或Windows/Linux/Browser。已查看暗色普通／禁用、明色字体大字、暗色配置、明色专用Style及透明画面，默认分隔符可见／禁用弱化且cell排布保持；大字在固定列宽中溢出与原版相同，颜色分包不扩展尺寸合同。

执行入口：`dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj -c Release --framework net10.0 --filter 'FullyQualifiedName~CT13OtpColor|FullyQualifiedName~OtpLineEdit'`；临时驱动`dotnet build Visual.csproj -c Release`后以`dotnet AtomUI.ThemeAssetsValidation.dll before3|after3|old-native light|dark <registered-output>`运行；NativeAOT以`dotnet publish Visual.csproj -c Release -r osx-arm64 --self-contained true -p:PublishAot=true`及同参数二进制运行。旧源A/B逐字节finally恢复，未用HEAD覆盖既有修改。

7项临时验证／2文件、431个外部驱动与发布文件（1,369,758,715字节）、两份源字节备份均已删除；批准原型运行9文件、本轮210文件已清理且媒体audit均clean。清理后OTP精确filter66PASS（2s），NET8 Desktop Release含依赖0警告／0错误（21.08s）；35个本地文档链接有效、git diff --check通过，cached diff为空，新增常驻测试为0。作者自审确认产品差异仅两个ControlTheme Setter及空白，未触及C#／注册／公式／资源编号；无分支、worktree、提交、推送或子agent。完整CT13家族及CT25平台／性能／原生直接Style移除出口仍开放。
