# Dialog / MessageBox Semantic Part 真机视觉验收记录

> 日期：2026-09-12 · 控件家族：Modal（`Dialog` / `MessageBox`）· 分支：`feature/semantic-dialog`
> 交付提交：`7271b0a95`（语义部件 + `OverlayScope`/`IsPinnedOpen`，含用户 amend 并入的 Core using 清理）、`327346ea1`（示例文案修正）

## 1. 验收步骤与判定

入口路径：**Gallery 左侧导航 Feedback 分类 → Modal 页面**（`controlgallery/AtomUIGallery/ShowCases/Feedback/Modal`，
可直接检索 ShowCase 条目 ID `Modal`）。两个验收面分别位于：

- Examples 页签（默认）→「自定义语义结构的样式」条目
- Semantic Parts 页签（页面顶部第二个页签）→ Modal / MessageBox 两个预览

| # | 操作 | 预期 | 判定 | 证据 |
| --- | --- | --- | --- | --- |
| 1 | 进入 Semantic Parts 页签 | 出现两个纵向排列的预览（上 Modal、下 MessageBox），各自舞台内有遮罩压暗 + 居中对话框（标题、两行正文、Cancel/OK） | 通过 | 用户回传截图 |
| 2 | 观察遮罩范围 | 遮罩被限定在各自舞台内（`OverlayScope`），不铺满窗口、不遮挡右侧部件卡 | 通过 | 用户回传截图 |
| 3 | Examples 页签 →「自定义语义结构的样式」条目展开 | 条目内有两个触发按钮：Open Modal（主按钮）与 Open MessageBox；无任何 ToggleSwitch | 通过 | 用户回传截图 |
| 4 | 点击 Open MessageBox | MessageBox 呈现紫色遮罩、浅色标题栏/Footer、蓝色标题与关闭按钮、正文 32 内边距 | 通过 | 用户回传截图 |
| 5 | 逐张悬停两个预览的部件卡（各 9 张） | 对应部件描边高亮，每张卡恰好命中一个目标 | 未单独截图；已由自动化回归锁定（见 §2），真机如需可补充 | 自动化 |

判定依据为用户回传的截图（按约定不入库，本记录仅保留文字结论）。第 5 项在用户截图中未呈现悬停态，故不计入
「真机通过」，由下述自动化回归承担回归防护；如需真机悬停截图确认，可随时补充。

## 2. 自动化回归（补充覆盖）

- `tests/AtomUIGallery.Tests/ShowCases/ModalSemanticPartHighlightTests.cs`：进入语义页签后，Dialog 与 MessageBox
  两个预览各自 9 张部件卡逐张悬停，断言恰好一个 `SemanticPartAdorner`（含 `root`——舞台 owner 显式 420×320 铺满，
  因 Dialog 主题默认零尺寸而 resolver 对零尺寸目标不建 Adorner）；第三个原生 Window 宿主预览中，`root` 高亮舞台 owner、`container`/`body`/`footer` 经跨根在原生窗口内高亮、
  五个不物化部件（`mask`/`wrapper`/`header`/`title`/`close`）无高亮；遮罩按压不关闭钉住的预览；切回 Examples 清理全部高亮。
- `tests/AtomUI.Desktop.Controls.Tests/Dialog/DialogSemanticPartTests.cs`：descriptor/marker/逻辑父/cross-root/
  精确单节点命中（Dialog 与 MessageBox 各自）/钉住门控/Optional 语义，共 14 用例。

## 3. 结论

1–4 项真机验收通过（证据：用户回传截图）；第 5 项悬停高亮由自动化回归覆盖，真机悬停截图未回传、不宣称通过。
Visual acceptance 状态：**通过（含一项由自动化覆盖的待补真机悬停证据）**。

## 4. 2026-09-13 增补：窗口宿主样式修复与文案检查点（待真机验收）

修复（详见 changelog 2026-09-13）：DialogWindow 按 PopupRoot 范式覆写 IStyleHost.StylingParent；
container 背景迁出模板局部值；样式化窗口 Dialog 补 StandardButtons。复现工程真机截图已回传确认
（淡粉背景 + 品红加粗正文 + 绿色胶囊 Cancel/OK）。Gallery 本体待验收：

1. Examples 页签「自定义语义结构的样式」卡：引导句应为"分别打开语义样式定制后的 Dialog、MessageBox
   与窗口宿主 Dialog："；按钮为"打开样式化 Dialog / 打开样式化 MessageBox / 打开样式化窗口 Dialog"。
2. 点击"打开样式化窗口 Dialog"：独立 Basic Modal 窗口内三处样式（淡粉背景 / 品红加粗正文 /
   绿色胶囊 Cancel/OK）必须全部出现。
3. Semantic Parts 页签：常开预览对话框标题应为"基础模态框"（Basic Modal，对齐上游语义 demo）。
