# CT19 C9 色谱键盘与棋盘格生命周期结果

状态：产品候选已实现，功能与明暗取证完成；限定视觉审查与清理后模块验证已完成，本批Git审查另记，不代表完整CT19／CT25通过。C9短设计已获用户明确批准；Git未授权。沿既有feature/custom-theme及checkout，不新建分支／worktree，不委派，不发布NuGet。

## 合同与最小改动

两类公开View保留完整调色板、滑轨、输入、渐变stop、尺寸、裁剪和正常动效。色谱ControlTheme声明Focusable，复用默认AdornerLayer，移除不存在的FocusEllipse选择器；六模式只修正VS的Up／Down饱和度方向，保留RTL、步长、Ctrl、范围、wrap及非方向键路由。

透明棋盘的资源标量继续由ControlTheme绑定，AbstractColorSlider／ColorBlock仅解除派生DrawingBrush更新的挂树限制，避免离树期间输入已变而保留模板返回时没有再次通知。沿用既有最多32项的值缓存，不新增字段、订阅、公开签名、Token或Part。AbstractColorSlider的纯成员布局先独立保存，所有成员语法指纹相同、静态注册初始化顺序不变；行为改动另记。

## 有效前后证据

- 原始自然焦点8项全失败；主题接通后焦点／指针4项通过，四组有效方向矩阵均在VS Up失败（S .423529→.433529，期望.413529）。最小方向修正后原8项通过。
- 棋盘格公开Own配置四项：在树两项通过、整个provider离树改6→10后两项失败（标量10／tile6）；修正slider后同源ColorPreview仍有两个反例，再对ColorBlock作同一根因修复。
- 扩展至24项通过：两类View、LTR／RTL、六模式四方向实际事件到达、HSV和实际Color Alpha字节／View值、zh-CN数值Ctrl与en-US命名颜色Ctrl、边界／退化、禁用恢复、Tab／ShiftTab／真实指针、原实例／重模板、邻接实际文本和六对象GC；棋盘格还覆盖明确0、撤回、多次离树、Purple／Cyan／Orange与独立Provider隔离。
- 有效测试使用原始框架输入与公开面板输入准备，不设内部Focusable或HSV制造成功。首次把Own配到Global-only View身份、观测隐藏未物化节点、初始公开Value未重放活动HSV均作为准备错误保留，未计入产品RED。

命令：`dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj -c Release --no-restore --filter 'FullyQualifiedName~CT19SpectrumKeyboard|FullyQualifiedName~CT19Checkerboard'`。最后24项通过后删除三份Temporary C#，净增常驻测试0。

## 正式自然合成与独立诊断

Mac arm64、Headless/Skia、scale1、产品Window1200×820，Light／Dark×普通／渐变View×main／aa／dip。各版本同一完整历史详见[31对限定审查表](color-spectrum-keyboard-noise-review.md)。公开Alpha输入每次都执行60→61，避免同显61却未通知造成初值差异；旧新探索首轮及其真实计数仍保留，未冒充兼容通过。

- 正式新旧各84帧：候选486／486检查通过，旧版420／486；旧失败66项包含自然焦点、VS Up与离树棋盘格。
- 55／56组A/A为0，56组1DIP全部检出（35,891–54,849像素）；比较完整RGBA，不缩放、对齐、裁剪或遮罩。
- 旧新四组default与四组最终reset整图均0；另外六个兼容阶段比较为1／5／7像素，24个同版本Own恢复为12–61像素，外加候选暗色渐变vs-up一组A/A为7像素。共31对已获用户单独限定批准，坐标／每对上限／单通道上限完整列入表，原计数保持失败。
- 独立即时Skia诊断新旧各56帧，56组A/A、16组Own恢复、16组新旧兼容阶段全部0。32对恢复／兼容的505／516节点观测表一致（几何／矩阵／可见性／裁剪、文字字体／画刷、棋盘标量、Image源Bitmap SHA）。这不能证明每个正式历史帧内部缓存原因，也不代替自然合成基线。

可见预期变化如下，均为正式自然合成的整图像素计数：

| 场景 | 普通明 | 普通暗 | 渐变明 | 渐变暗 |
| --- | ---: | ---: | ---: | ---: |

| 自然键盘焦点 | 2593 | 2596 | 2592 | 2594 |
| VS Up实际移动 | 5352 | 5353 | 5351 | 5353 |
| 同模板离树tile10 | 1224 | 1238 | 2161 | 2225 |

已经打开新旧明色焦点、暗色渐变VS Up、重挂棋盘格及全图差异图：默认焦点框落在实际色谱边界，旧版固定Tab落在滑轨；Up后选择环及颜色输入正常更新；棋盘格变化位于滑轨／预览／渐变条。无取消控件、隐藏滑轨或移除原触发条件。

诊断准备曾因临时probe的nullable注解、旧Save重载，以及零尺寸节点AspectRatio无穷值的JSON观测而失败，原日志保留；改为有效非空断言、新编码重载及显式保存命名浮点值。产品未因诊断失败增加补偿逻辑。

## 真实Mac managed／NativeAOT

每次8个真实NSWindow，ClientSize1200×820、scale2，Light／Dark×两类View×LTR／RTL。原始键鼠经实际PlatformImpl.Input及IInputRoot，覆盖192个方向键、自然焦点、实际adorner、同模板棋盘6→离树10→clear8及ColorPreview。managed388／388通过。

NativeAOT原二进制首轮383／388：Light／Gradient／RTL最后VS四个原始按键未到达，累计20而非24个；该失败仍保留，内部原因未证明。只读激活／焦点trace构建388／388，192个pre-key均Window active且Spectrum focused；同一个原始无trace二进制独立聚焦复跑388／388。后两轮通过不解释或抹去首轮缺失，不据此宣布完整平台验收。产品没有加延迟、重试按键或强制聚焦。

AOT使用`dotnet publish <外部临时工程> -c Release -f net10.0 -r osx-arm64 -p:PublishAot=true --self-contained true --disable-build-servers -nr:false`，实际编译与运行完成。Homebrew库链接有5项系统目标版本警告；无产品trim／AOT警告，不声称旧macOS支持，也未验证原生最终像素。

运行身份：managed DLL SHA `ded447ea5b6e0c3861d51d85be66f5259acd6ba8f68095aae7b832e18ba364fa`；原AOT `f588431c77373fca47b8bb99c8d0427bcfef16f2377ae617e510832b19598074`；trace AOT `c73aaabb920c9e34181219c0afceccb6f6df4ad2ac9d0ba07022ed9a9c04e148`。

## 清理与最终验证

三份临时测试已删除；外部临时工程443个文件／1,426,522,949字节及所生成产物已删除，无残留运行进程。视觉批准后457张PNG及manifest共458文件已清理，Git媒体审计clean、登记目录已删除；截图、驱动与二进制不进入Git。精确路径、命令、结果和SHA保留文字账本。

三个完整控件文件成员审计：3文件／3Control／0问题。清理后`FullyQualifiedName~ColorPicker`原有27项全部通过；ColorPicker项目NET8 Release构建0警告／0错误，`git diff --check`通过。作者另步自审已完成，不代替独立审查；滑轨键盘、原生最终像素、首轮AOT输入缺失与完整家族仍开放。

## 源身份

基线393d718b38a704a1d29ef6a94168c856b4249188；候选：

| 路径（src/AtomUI.Desktop.Controls.ColorPicker/） | SHA256 |
| --- | --- |

| `ColorBlock.cs` | `e1e00ebb00cfca04c57a92b9652142a428a1cf3045d0734d65d858af40b9e1ba` |
| `ColorSlider/AbstractColorSlider.cs` | `ea4271c326e5d86f54b062bf23787d2971d213bb642c2f82a4262e7c2bea64dd` |
| `ColorView/ColorSpectrum.cs` | `9fd4e5da88d7440ce9b8b45965444a0adb7faae4196ba0a1c02a284b9d7edc06` |
| `Themes/ColorView/ColorSpectrumTheme.axaml` | `da06c8501489bb035d6d9a04f64db26db10bf4ec92785e244c3346757a03ba97` |

2026-10-25用户明确允许棋盘格重挂视觉修复并接受31对限定自然差异；此前C9键盘短设计批准继续有效。此C9／棋盘切片的实现、功能、有限视觉审查及临时清理完成；首轮AOT输入缺口继续CT25，不表示完整家族、原生最终显示面或Git批准。
