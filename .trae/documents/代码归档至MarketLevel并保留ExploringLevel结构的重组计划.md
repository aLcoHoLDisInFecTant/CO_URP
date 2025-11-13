## 目标
- 将“超市玩法”相关脚本集中到 `Assets/Scripts/MarketLevel/**`，包括基础玩家控制器与超市交互。
- 保留探索玩法相关脚本在 `Assets/Scripts/ExploringLevel/**`（如高级玩家控制器与抓钩/回旋镖）。

## 调整范围
- 移动：
  - `ExploringLevel/PlayerBasic/BasicPlayerController.cs` → `MarketLevel/Player/BasicPlayerController.cs`
  - `ExploringLevel/Supermarket/SupermarketItem.cs` → `MarketLevel/Supermarket/SupermarketItem.cs`
  - `ExploringLevel/TetrisSystem/Sorting/SortingAreaTrigger.cs` → `MarketLevel/Sorting/SortingAreaTrigger.cs`
- 保留：
  - `ExploringLevel/PlayerAdvanced/AdvancedPlayerController.cs` 保持原路径；抓钩与回旋镖相关脚本保持在 `ExploringLevel/Player/**`
  - `ExploringLevel/TetrisSystem/**` 保持共享位置，供两套玩法复用。

## 兼容性
- 类名与命名空间不变，场景引用不会因路径改变而影响运行时类型解析；若已有 Prefab 引用新脚本（当前均为新增，未绑定），移动后不影响。

## 验证
- 编译通过（类名不变）；在 Market 场景中挂载 `BasicPlayerController` 与 `SupermarketItem/SortingAreaTrigger`，在 Explore 场景中保留 `AdvancedPlayerController`。