# Modal Dialog 关闭动效设计

本文定义 Dialog / MessageBox 在 Overlay 宿主中的动效目标、完成边界与资源所有权。
公共契约见 [Modal 架构](overview.md)，宿主实现见 [实现原理](implementation.md)，
共享播放合同见 [Motion 系统](../../../../architecture/systems/motion/overview.md)。

## 1. 定位与原则

Overlay 的表面和遮罩使用两个真实目标，复用 Core Motion 的原生时钟与请求替代。
入场、离场和 teardown 有明确边界；关闭期间保持标题、正文、Footer、按钮和用户内容的真实控件树。
动效只改变表面 RenderTransform 与透明度，尺寸、位置、焦点、结果和业务关闭校验仍属于 Session / Presenter。

普通 Zoom 的终点包含 opacity 0，完整表面共同退出，不单独叠加内容透明度动画。
Window 宿主继续使用原生 DialogWindow 生命周期，不创建 Overlay 动效包装。

## 2. 目标与模板

```text
OverlayDialogPresenter
  Panel
    ContentControl PART_MaskHost (IsVisible = IsModal)
      OverlayDialogMask PART_DialogMask (实际透明度目标)
    Border PART_SurfaceMotionRoot (私有变换目标、semantic-scope-presenter)
      ContentPresenter (semantic-wrapper)
        DialogSurface (独立位置矩阵)
          ShadowsAwareContainer / Frame
            PART_SurfaceContentLayer
              Header / Content / Footer / Buttons
```

`PART_MaskHost` 保留条件物化：modeless 不将实际 Mask 及其语义 Border 放入视觉树。
私有表面隔离 Motion 变换与 wrapper 专用 Style 的自定义变换，DialogSurface 的位置矩阵保持独立。
wrapper 由 owner-scoped 专用 Style 定制，route 为 `>> .semantic-scope-presenter > .semantic-wrapper`。
scope marker 位于私有表面，公共 wrapper 位于其直接逻辑子节点，不增加模板层级边界。

## 3. 轨迹与参数

| 目标 | 入场 | 离场 | 时长与曲线 |
| --- | --- | --- | --- |
| 表面 | 普通 Zoom，scale .2→1、opacity 0→1 | scale 当前值→.2、opacity 当前值→0 | 入场 Slow / OutCirc；离场 Mid / InOutCirc |
| modal 遮罩 | opacity 0→1 | opacity 当前值→0 | 分别跟随入场 / 离场时长，Linear |
| 内容层 | 随整体表面呈现 | 随整体表面退出 | 不单独创建动画 |

有效时长由主题的 `OpeningMotionDuration` / `MotionDuration` 提供，默认消费 SharedToken Slow / Mid。
动效定义不被控件改写。共享曲线规格由 [内置 Motion 规格](../../../../architecture/systems/motion/built-in-motion.md) 拥有。

有效显式 placement target 提供其中心在私有表面坐标系内的原点；无有效锚点时使用实际 DialogSurface 中心。
本次原点在入场准备后冻结，离场继续使用该原点，避免触发元素移动造成 pivot 跳变。

## 4. 播放与完成

入场在首个可见帧前建立作用域透明度覆盖，完成一次真实布局准备后同步安装 Motion 起点，随后由原生时钟推进。
正常完成保留末帧；准备覆盖随实际播放接管释放。未就绪路径稳定收敛，不等待不存在的动画事件。

关闭意图先登记。已经开始呈现时，离场先取得当前值并替代入场，再取消旧入场令牌；不先归一到完整可见态。
表面与遮罩同步启动并聚合等待真实播放任务。当前 Presenter 的关闭任务结束后依次释放覆盖、断开 composition children、
Dispose Surface 并移除自身 layer 节点。内容和按钮在该边界前保持附着。

Show / Close / Dispose 的任务先保存到 Presenter 再开始可观察操作，重入调用得到同一任务。
关闭尚未呈现的宿主直接保持隐藏并稳定 teardown；禁用动效跳过轨迹，保留完整生命周期。
外部取消影响入场；关闭和 Dispose 继续负责自身宿主释放。业务事件与结果顺序由 DialogSession 决定。

## 5. 后端与资源

当前使用已验证的原生 Motion 路径，不使用提交后定时器推断动画完成。
合成端只有在真实完成、连续替代、终态和逻辑命中合同均有证据后才能进入共享后端。
批次已提交不能作为结束信号；平台状态与预算见 [性能验收](../../../../architecture/systems/motion/performance.md)。

Core 拥有每次执行的进度、取消源和属性覆盖。Presenter 只持有入场协同令牌、准备覆盖、操作任务和业务关闭意图。
入场令牌在完成 / 取消后释放；关闭、禁用和 teardown 释放该宿主的 Motion 覆盖，恢复最新底层值。
拖动、resize、maximize 与遮罩几何继续使用既有 owner，不逐帧重建内容或改变逻辑父子关系。

## 6. 验证

- 受控框架时钟验证首帧、完整时长、入场中关闭的矩阵 / 透明度连续性与真实结束。
- 真实 Presenter 验证 Footer / 按钮保持附着、modeless mask 缺席、专用 Semantic Part Style 命中与宿主移除。
- 原点验证覆盖显式 target、中心回退、触发元素移动及不同 owner 几何。
- 生命周期验证覆盖取消、禁用、重复关闭、操作重入与 WeakReference 回收。
- Headless 结果不证明屏幕帧率；Release Gallery、NativeAOT 和实际呈现按共享验收单独记录。
