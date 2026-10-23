# CT-11 M2：Transition 动态时长原型结果

本包只验收 Core 接线及 Button、DropdownButtonBase、InputControlFrame、RadioIndicator 四个模板文件，不代表全部 79 个原 literal 消费文件已完成。

修复前真实控件 Transition 均停留应用默认 200ms，未使用作用域 800ms；覆盖全局/Control 配置、已有实例、Dropdown 独立 identity、共享 LineEdit 模板 frame 及 Radio Mid/Fast。共享 frame 先完成正常 Dispatcher 初始化，保留 StateMotion 过渡抑制；旧 XAML 在同条件下仍失败。

接线保持 style Setter，通过原生绑定在实际 Control 解析有效 owner/context/resource。每个 owner 独立原生集合，动态更新保留对象身份；本地覆盖/清除、兄弟隔离、同实例 provider 800→400→800 及完整图释放后 GC 均通过。8 类有限构造工厂复制实际属性、时长、延迟与 easing，无反射激活。

临时 10/10 通过，5.0566秒。最后一项夹具最初声明 TransformOperations 类型槽，不满足实际 Transition<ITransform> 合同，报 InvalidCastException；改成真实 ITransform 槽后通过。此夹具错误不计作产品失败。

Skia/Headless Drawing=false，Avalonia 12.1.4.0，800×540、scale1，24个实例覆盖四列及 Large/Middle/Small/Custom/禁用/输入错误：Light/Dark A/A 0/0；1 DIP 检错16032/16468像素；四文件旧源码→新源码 idle/hover/pressed 各0/0；自定义时长后 idle 和恢复 idle 各0/0。旧源 A/B 四文件逐字节恢复。已查看明暗 idle/pressed 关键图像，布局、状态与文本保持。实际指针路径触发 hover/down/up，未关闭动效；此处比较稳定终态，真实时钟曲线仍由 M3/CT-25验收。

临时测试位置 `tests/AtomUI.Desktop.Controls.Tests/Temporary/CustomThemeMotion`；运行 Desktop.Tests `--framework net10.0 --filter 'FullyQualifiedName~Temporary.CustomThemeMotion' --logger 'console;verbosity=normal'`。外部驱动 `/private/tmp/atomui-transition-theme-visual/Visual.csproj`，阶段 before/after，暗色 `ATOMUI_FRAME_DARK=1`。清理及最终 focused/NET8/当前平台 AOT 结果在完成后补写。

作者自审：没有新优先级引擎、全局控件订阅或逐帧集合创建；定制仍使用原生 ControlTheme Setter。当前内部接口未声明为产品公开 API；其余集合、所有 owner 及原生最终显示面未借本包标为通过。

清理后 Desktop focused 76/76通过（1秒），NET8 Desktop（含 Core/Common/Shared）0警告0错误（22.49秒）。临时10项、外部驱动及专用 bin/obj/AOT 产物已删除；媒体cleanup审计clean。当前macOS26/arm64的43MB Mach-O NativeAOT实际运行四类模板通过。首次链接缺libssl，补LIBRARY_PATH指向本机openssl@3/brotli后成功，链接库最低OS26与target12警告保留，不声明macOS12兼容或其他平台通过。
