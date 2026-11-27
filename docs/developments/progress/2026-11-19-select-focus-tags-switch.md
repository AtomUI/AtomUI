# TreeSelect 焦点、Select 候选与标签布局、Switch 滑块

在 `bugfix/shadcn` 的 1300 × 900 DIP Gallery 运行原始场景。驱动通过真实指针事件打开、切换、悬停、点选和按住控件，并通过窗口编号抓取 2× macOS 原生帧。另以 Avalonia Headless + Skia 核对焦点交接过程。

| 场景 | 原因和前后结果 |
| --- | --- |
| TreeSelect Basic → Binding Single | 同一个 `:focus-within` 同时由 Avalonia 和输入框合并焦点维护，左侧失焦发生移除、添加、移除。配合同一次手势关闭原下拉，录屏里左侧焦点环会恢复再消失。改用独立伪类合并焦点，阴影列表增减时透明过渡。Shadcn Luma 浅色原生记录 36 次状态采样，深色 30 次，Skia 27 次。旧环 Alpha 从 77 单调减至 0，新环从 0 增至 77。右侧搜索输入和弹层正常开启。 |
| Select Basic，Lucy 已选中、Yiminghe 悬停 | 修复前两行背景相接，实测 0 物理像素。Shadcn 的状态背景在每行上下各内缩 1 DIP，浅色与深色共留出 4 物理像素。行高仍为 72 物理像素，行首有效命中并提交选项。 |
| Select Two-way binding / SelectedOptions | 单行标签原先把中号从 36 撑至 40 DIP，移动了下拉和后续内容。原生浅、深色实测空、选 Lucy、移除均为 36 DIP。临时实际 Gallery 测试 5 项验证小、中、大、Tags 以及 Maia 中号，搜索获得焦点，过滤文字输入和双向绑定生效，标签实际换行时容器增高。 |
| ToggleSwitch Basic 按住与释放 | Shadcn Luma 原生按住时由 36 × 36 变为 46.8 × 36 物理像素。浅、深色修复后按住仍为 36 × 36，释放切换状态并移动 44 物理像素。Ant Design 保留 1.3 倍按压宽度。临时回归通过普通、小号、自定义、Ant 对照 4 项。 |

已检查修复前后原生截图与关键焦点采样。阴影出现、消失的插值临时测试由 2 项失败变为 2 项通过。

取证已审计并清理。删除临时脚本、截图和测试后，`net10.0` 定向验证通过 315 项：Core 动画 8 项、Desktop 控件 156 项、Shadcn 主题 85 项、GalleryBase 42 项、Gallery 21 项、Card 阴影渲染 3 项。`git diff --check` 通过。
