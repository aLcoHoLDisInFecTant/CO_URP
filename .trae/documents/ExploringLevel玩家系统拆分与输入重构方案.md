## 目标
- 解除 Player 与编码器桥（`EncoderInputBridgeV2`→`InputTranslator`→`ICommandTranslator`）的耦合，改为直接键盘输入。
- 依据功能拆分为两套可替换的玩家实现：
  1) 基础版：仅 WASD 移动；按住空格打开背包（Tatris System）。
  2) 高级版：正常 WASD 移动；空格预留；E/Q 触发抓钩与回旋镖（可同时保留）。
- 梳理文件结构，隔离原操作输入系统，保留兼容路径以便回退。

## 现状梳理（关键引用）
- 输入总线：`GameSessionExplore.cs:38-49` 在场景内选择编码器或键盘绑定，驱动 `InputTranslator.Tick()`（`GameSessionExplore.cs:33-34`）。
- Player 接入：`Player_Explore.cs:91-95` 注册为 `ICommandTranslator`，在 `TranslateCommand(...)` 内压入队列（`Player_Explore.cs:175-203`）。
- 键盘绑定使用旧版 `UnityEngine.Input`（`KeyBinding.cs:25-33`，`KeyBindingHolder.cs:10-21`）。
- 背包入口未与 Space 关联；现有操作在 `InventoryManager.cs:33-79`（Q/R 与鼠标）。
- 抓钩有可用 API（`GrappleLauncher.cs:10-43`），但状态脚本被注释；回旋镖运行体在 `BoomerangProjectile.cs:55-92,207-238`，发射器脚本被注释。

## 总体设计
- 新增两套玩家控制器，不再实现 `ICommandTranslator`，直接在 `Update()` 读取键盘：
  - `BasicPlayerController`：WASD 移动；按住 Space 打开背包 UI，松开关闭；背包打开时暂停玩家移动与交互。
  - `AdvancedPlayerController`：WASD 移动；Space 预留；E 触发抓钩（`LaunchTo/StartSwing/StopSwing`），Q 触发回旋镖（实例化 `BoomerangProjectile` 并管理返回）。
- 抽象出可复用的通用模块：移动（Rigidbody/CharacterController）、射线选点（从摄像机或屏幕中心）、UI 打开/关闭桥接到 `InventoryManager`。
- 保留原 `Player_Explore` 与输入总线以便场景选择旧实现；新实现与原输入系统完全隔离。

## 文件结构调整
- `Assets/Scripts/ExploringLevel/PlayerBasic/`
  - `BasicPlayerController.cs`
  - `BasicMovement.cs`（可选，封装移动）
  - `InventoryOpenHandler.cs`（桥接背包 UI）
- `Assets/Scripts/ExploringLevel/PlayerAdvanced/`
  - `AdvancedPlayerController.cs`
  - `AdvancedMovement.cs`（可选）
  - `GrappleController.cs`（调用 `GrappleLauncher`）
  - `BoomerangController.cs`（管理 `BoomerangProjectile`）
- `Assets/Scripts/ExploringLevel/PlayerCommon/`
  - `AimHelper.cs`（摄像机中心 Raycast/落点计算）
  - `IPlayerMovement.cs`（接口，可选）
- 保留原 `Assets/Scripts/Input/**` 与 `Player_Explore/**`，不再被新玩家引用。

## 具体实现步骤
1. 基础输入解耦
   - 在新控制器中移除对 `GameSessionExplore.Instance.AddCommandTranslator(this)` 的依赖；改为 `Update()` 里读取 `Input.GetKey(KeyCode.W/A/S/D)` 计算方向向量并驱动刚体/角色控制器。
   - 删除新控制器中的任何 `ECommand`/`InputTranslator` 相关引用；原 `GameSessionExplore` 仍可存在但不驱动新玩家。
2. 背包打开（Basic）
   - 引入 `InventoryOpenHandler`，持有 `InventoryManager` 或其 UI 根节点（Canvas/Panel）。
   - 按住 Space：`SetInventoryOpen(true)`；松开：`SetInventoryOpen(false)`；打开时禁用玩家移动与战斗输入，并将鼠标交给背包 UI（必要时锁/解锁光标）。
   - 若当前 `InventoryManager` 无显式打开方法，新增轻量接口（例如暴露 `Open/Close` 或直接切换 CanvasGroup/Panel 激活）。
3. 高级功能（Advanced）
   - 抓钩（E）：使用 `AimHelper` 从摄像机中心在可交互层做 `Physics.Raycast` 获取命中点；短按 E `LaunchTo(hit.point)` 或进入摆动 `StartSwing(hit.point)`；再按 E/右键 `StopSwing()`。
   - 回旋镖（Q）：实例化 `BoomerangProjectile`（参考 `boomerangPrefab` 序列化），设置发射方向与速度；在合适条件或再次按 Q 时触发返回；监听 `onReturnCallback` 完成回收。
   - 保持与 WASD 并行，Space 留空不触发背包。
4. 预览/摄像机兼容
   - 基础与高级移动可选择复用 `MovementPreviewController`（如需预览点）；否则实现直接移动并保留 Cinemachine 绑定（`ControlManager`）。
5. Prefab/场景切换
   - 复制现有 Player Prefab 为 `PlayerBasic.prefab` 与 `PlayerAdvanced.prefab`；分别挂载对应控制器与所需引用（`InventoryManager`、`GrappleLauncher`、`boomerangPrefab`）。
   - 在场景中选择其一并移除对 `GameSessionExplore` 输入翻译器的玩家注册；确保旧输入总线不影响新玩家。

## 验证方案
- 基础版：
  - 进入场景按 WASD 正常移动；按住 Space 背包 UI 打开，移动无效；松开 Space 背包关闭。
- 高级版：
  - WASD 正常移动；E 指向可抓取表面进行抓钩/摆动；再次按 E 取消。
  - Q 发射回旋镖，返回后回收到玩家；连续触发不异常。
- 两版均不依赖 `EncoderInputBridgeV2`、`InputTranslator`。

## 兼容与回退
- 保留旧 `Player_Explore` 与输入总线；如需回退，场景改回旧 Prefab 并重新注册到 `GameSessionExplore`。

## 可能关注点
- 背包 UI 的“打开入口”目前未实现 Space 逻辑，需要新增轻量打开/关闭接口或直接控制 UI 激活。
- 抓钩/回旋镖原状态脚本被注释，按键触发将直接调用运行体（`GrappleLauncher`、`BoomerangProjectile`），如需完整状态机可后续迭代。
- 项目使用旧版输入；若未来迁移到新输入系统（`PlayerInput/InputAction`），可在 `PlayerCommon` 抽象输入接口后替换实现。