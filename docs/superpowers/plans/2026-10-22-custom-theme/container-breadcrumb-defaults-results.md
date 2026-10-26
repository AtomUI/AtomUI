# Breadcrumb 八项默认函数迁移结果

对应[执行计划](container-breadcrumb-defaults.md)。基线 ff0d3d572，沿现有 checkout／feature/custom-theme 单 agent 实施，没有新分支／worktree 或提交。产品仅改 BreadcrumbToken.cs，候选 SHA256 为 `67abc670f8f4cbfd9c92a4364d276b10d71a297d1039a25970721427e7126a34`。

## 合同与有效前后证据

八项从整组 V1 求值迁移至 DependencyDefaultsV2／Revision1 的静态默认函数。全部读取原有效 Global，没有 Own→Own 边；保留原字段／类型／ID、key 枚举数值／dense slot、原公式、明确 Own／0／撤回。没有改模板、selector、绑定、Part、尺寸、导航、hover 或默认 motion。原消费者已经正常，不能描述成消费缺失修复。

原 Breadcrumb 精确模块 18 项通过。临时验证先修正 namespace／扩展方法导入的编译准备错误；另一次输入把 PaddingXXS／MarginXXS 当作 Uniformly 字段，两个断言失败，查明它们是独立 Global 字段后修正输入。有效原版为两项明暗兼容通过、V2 声明合同一项失败；候选三项通过。随后扩充 Control Global、原始值／资源投射／slot 只读观测及独立 A/A、DIP 实例，候选七项、原版六项兼容用例通过；V2 合同不作为原版兼容用例运行。

真实资源及两分隔符验证覆盖 Global、Control Global、八项 Own、明确零内外距、撤回和同实例重挂，真实框架 MouseMove 进入链接，断言 IsPointerOver 及内容／背景最终颜色；三个条目引用保持、分隔符仍为 N−1。原始值和资源投射分开保存；正式明暗默认 16 项及 36 组历史状态数组、288 条字段记录的新旧值／类型／identity／key 数值／slot 全部相同。

## 视觉与宿主校正

环境为 Mac arm64、Headless/Skia、scale1、800×320、产品 AtomUI Window。最初媒体 path 输出误当 JSON，缺少映射导致六项准备失败；修正后首批 84 帧仍仅设置 variant，查看发现白色暗色宿主及直接填充窗口导致的高条目。这批只作准备问题记录，不计入正式主题验收。

正式脚本通过应用 ThemeManager 的真实事务切换 Light／Dark，分别断言 CurrentTheme.Appearance、ActualThemeVariant、完整窗口背景像素及自然行高；StackPanel 供给常规自然高度。两个版本保留完全相同历史：default→Global→Control Global→Own→真实 hover→零内外距→同实例重挂→reset。main／A/A／1DIP 为独立实例，扰动仅在外部容器左距增加 1 DIP。默认动效保留，等待至少 600ms、核对实际 Transition 上限，再采集连续三同帧；不裁剪、对齐、缩放或设置通用容差。

正式共 84 帧：28 组 A/A、12 组恢复、42 组新旧比较均为零差异；28 组 1 DIP 扰动全部检出。已查看明暗默认、暗色 Own hover、明色 Control Global 及暗色零间距画面：自然行高的文本／斜线对齐、默认可读，配置颜色／间距及零值按输入生效。Own 使用深色固定色值验证原样保留，暗色下对比度低，不作为推荐配色或新设计；新旧该输入仍零差异。

原 84 准备帧和正式 84 帧共 168 张 PNG，加 manifest 共 169 文件已由 evidence.py 清理，运行目录不存在，Git 媒体审计 clean；原始比较结果未改写，没有新视觉变化需要扩大历史容差。

## 原生、清理与最终出口

Mac managed／NativeAOT 实际 Light／Dark NSWindow、scale2，分别 136／137 项检查通过，AOT 动态代码禁用；各 80 条真实资源／元数据记录完全一致，覆盖默认／Global／Control Global／Own／恢复及保留条目的重挂。原生驱动不模拟指针事件，不把资源验证当作原生最终像素。外部工程最初遇到 /var 符号链接导致的相对依赖路径、缺少 AtomUI 命名空间两类准备错误；使用规范绝对路径并补导入后构建零警告／错误。AOT 链接器报告 Homebrew 的 OpenSSL／Brotli dylib 面向 macOS26、链接目标为12；只验证当前 Mac，不声称 macOS12 兼容。两个进程均结束，外部工程 419 文件／1,381,002,127 字节已删除。

临时 C# 驱动已经删除，常驻测试净增 0；原 Breadcrumb 精确 filter 在清理后重新构建并通过 18 项、0 Skip，测试 797ms。Desktop NET8 Release 构建零警告／错误，58.44 秒。单个完整 Token 文件成员审计：1 文件、0 Control、0 问题。正式八条 Token／一条 Control 目录已回填，不扩大为完整家族状态。

最终命令为 `dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj -c Release --framework net10.0 --no-restore --filter FullyQualifiedName~Breadcrumb` 及 `dotnet build src/AtomUI.Desktop.Controls/AtomUI.Desktop.Controls.csproj -c Release -f net8.0 --no-restore --disable-build-servers -nr:false`。未运行本地 change/full 或扩大测试范围。作者另步审查核对八项表达式、字段类型／顺序、资源身份及清理记录；它不是独立评审。

完整 Breadcrumb／容器家族、N2／N4、图标与键盘等其余业务状态、其他 Host 及原生最终像素仍开放。本片没有提交，不沿用已执行的 27 文件 Git 批准。
