## 目标
- 将探索场景下的 NPC 系统迁移并归档到 `Assets/Scripts/MarketLevel/**`，用于超市玩法。
- 新增两项玩法机制：
  - 拥堵减速：根据路段内路人 NPC 密度动态降低玩家移动速度（百分比）。
  - 收银排队：结账处形成队列，玩家需在 NPC 离开后、且轮到队首时才能结账。

## 代码结构调整
- 迁移文件（类名与命名空间保持不变）：
  - `ExploringLevel/NPCsystem/**` → `MarketLevel/NPC/**`
  - 如存在增强寻路脚本：`EnhancedNPCController.cs`、`NavMeshBakeManager.cs` 一并迁移到 `MarketLevel/NPC/Nav/**`
- 保留共享模块：
  - 继续在 `ExploringLevel/TetrisSystem/**` 保留背包系统，供两种玩法复用
  - 玩家控制器：超市用 `MarketLevel/Player/BasicPlayerController.cs`；探索用 `ExploringLevel/PlayerAdvanced/AdvancedPlayerController.cs`

## 拥堵减速设计
- 新增 `MarketLevel/Crowd/CrowdDensityZone.cs`：
  - 以 `BoxCollider` 触发器定义路段范围，实时统计范围内 `NavMeshAgent`/NPC 数量与面积，计算密度 `density = count / area`
  - 采用分段或曲线映射为玩家速度系数 `speedMultiplier ∈ [min, 1]`（例如 `speedMultiplier = 1 / (1 + k * density)` 或阈值分段）
  - 通过事件或服务更新玩家速度：
    - 新增 `PlayerSpeedModifier.cs`（组件挂在玩家）：维护一个可叠加的 `modifierStack` 并对基础速度应用最小值或乘积；`CrowdDensityZone` 在 `OnTriggerStay` 更新玩家的拥堵项，在 `OnTriggerExit` 移除
- 关键接口
  - `CrowdDensityZone.SetDensityFactor(float m)`（内部计算得到）；
  - `PlayerSpeedModifier.SetModifier(string key, float multiplier)` / `RemoveModifier(key)`；
  - `BasicPlayerController` 读取 `PlayerSpeedModifier.CurrentMultiplier` 应用到 `moveSpeed`
- 性能与稳定
  - 采用每 `0.2s` 采样一次，以避免每帧开销；
  - 支持黑/白名单层筛选（仅统计 `LayerMask NPC`）与最大上限截断；
  - 进入多个重叠拥堵区时取最小乘子或按规则合并

## 收银排队设计
- 新增 `MarketLevel/Cashier/CashierQueueManager.cs`：
  - 维护收银点 `Transform queuePositions[]`（若干等距站位）、`Transform servicePoint`（结账台）
  - 维护队列 `Queue<QueueEntry>`，`QueueEntry` 包含 `isPlayer`、`entityTransform`、`agent`、`onServed` 回调
  - `OnTriggerEnter`：NPC 或玩家进入收银区，若未在队列中则加入；分配站位并导航过去
  - 服务流程：
    - 当前服务者到达 `servicePoint` 后计时 `serviceDuration`；完成后出队并发出 `onServed`；若是玩家则触发结账事件
    - 队列前移：所有后续成员按顺序移动到前一个站位；站位不足时后续成员在等待区停留
- 玩家结账规则
  - 玩家在队列中且位于队首并当前无 NPC 服务中时，触发 `PlayerCheckout()`（可通知 HUD 或计分系统）
  - 若玩家到达收银区但队列不为空，玩家加入队列并等待；HUD 显示“排队中”提示
- 与 NPC 行为的对接
  - 在 NPC 购物行为终点（离场前）可选任务：前往某个收银点；若进入收银区则交给队列管理器；完成服务后离场
  - `EnhancedNPCController` 保持导航与拥挤规避，队列中使用 `NavMeshAgent.isStopped=false`、目标位置为队列站位

## 玩家控制与交互
- `BasicPlayerController` 接入 `PlayerSpeedModifier`，将 `moveSpeed * CurrentMultiplier` 用于移动
- 背包整理与拾取不受收银队列影响；在服务进行时可禁用移动或降低速度（可选）
- 在 HUD 中显示排队状态、预计等待时间（按 `serviceDuration * remainingQueueLength` 粗略估计）

## 配置与参数
- 拥堵区：
  - `densityMin/Max`、`k`（映射系数）、`sampleInterval`、`agentLayer`、`areaFromCollider`
- 队列：
  - `queuePositions[]`（手动摆放或自动生成）、`servicePoint`、`serviceDuration`、`maxQueueLength`、`entryFilter`（只允许带有 `NPCController` 或 `Player` 标签的实体）

## 场景接线（以 MarketDemo 为例）
- 在主通道、入口附近与收银台前布置 `CrowdDensityZone`；设置合理触发范围与层过滤
- 在每个收银台前布置 `CashierQueueManager`，绑定若干站位与结账台位置；将玩家物体添加 `PlayerSpeedModifier`
- NPCManager 保持不变；如需 NPC 在购物后前往收银台，可在其行为表中追加“收银”目标

## 验证方案
- 拥堵：
  - 不同密度下，玩家速度曲线符合预期；离开拥堵区恢复正常速度；重叠拥堵区取最慢值
- 队列：
  - 多个 NPC 与玩家同时进入收银区，按先来后到顺序服务；玩家到队首且未有服务者时可立即结账；服务完成后队列前移
- 回归：
  - 与背包整理与拾取机制兼容；不会因拥堵或排队引发死锁；NPC 导航在队列中无抖动与卡死

## 风险与优化
- 大量 NPC 会增加碰撞与寻路开销：
  - 控制 `maxNPCCount` 与 `spawnInterval`；拥堵检测降采样；队列中静止成员 `isStopped=true`
- 服务点聚集导致 NavMesh 冲突：
  - 站位按网格偏移放置，尽量避免彼此重叠；必要时开启 `NavMeshObstacle` carving

## 实施步骤
1. 迁移 `NPCsystem/**` 到 `MarketLevel/NPC/**`
2. 新建 `CrowdDensityZone.cs` 与 `PlayerSpeedModifier.cs` 并接入玩家移动
3. 新建 `CashierQueueManager.cs`、定义队列与服务流程；在场景接线
4. 在 NPC 行为表增加可选“收银”子流程（或保持仅玩家结账需求）
5. 验证三组场景用例并调参（密度→速度曲线、服务时间、队列长度）

如确认方案，我将开始代码迁移与实现上述模块，并为 MarketDemo 场景完成接线与运行验证。