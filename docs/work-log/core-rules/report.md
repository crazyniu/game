# T1 交付报告

## 完成内容

已完整核对 `tasks/T1_core.md` 与 `docs/DESIGN.md`，检查并保留中断前有效的 Unity 项目、Core 实现和测试脚本，补齐缺失的 EditMode 测试并实际运行验收。没有修改 `art/`，没有提交 git。

- 项目版本为 Unity **6000.6.7f1**。`build/create-project.log` 记录了指定 Unity 可执行文件以 `-batchmode -quit -createProject` 和官方 2D 模板成功创建项目、退出码 0；本次未覆盖重建已有项目。
- 2D 编辑模式已设置，默认尺寸 **1080×1920**、默认竖屏，横屏自动旋转关闭。根目录原有 `.gitignore` 已覆盖 Unity 的 Library、Temp、Logs、obj、UserSettings、csproj、sln 等生成文件，本次未改动它。
- `BoardState` / `PerchState` 保存有序鸟列表、容量、种类数与默认 false 的 `Locked` / `Extra` 字段；支持深克隆、JSON 序列化和固定小端整数编码的 64 位 FNV-1a 稳定哈希。
- `Rules` 仅移动来源最右端一只鸟，允许异种叠放；目标满容量且全同种时立即清空，返回移动鸟及收集栖木索引、种类。通关和死局均由当前实际内容重新计算。
- `GameSession` 以完整状态快照支持无限撤销、收集后的恢复及重开；重开清空撤销历史。预留标志不参与规则。
- 新增 **30 个 EditMode 用例**，覆盖指定规则、撤销、重开、克隆独立性、JSON 往返、哈希固定向量与状态差异，以及 **1000 个固定种子的随机序列**。
- 测试脚本同时检查 Unity 退出码与 XML：要求全部通过、失败数为 0、用例至少 25 个；每次运行先删除旧 XML，避免误读之前的结果。

## 文件清单

核心交付文件（已核对保留）：

- `PerchSort/Assets/Scripts/Core/BoardState.cs`
- `PerchSort/Assets/Scripts/Core/Move.cs`
- `PerchSort/Assets/Scripts/Core/Rules.cs`
- `PerchSort/Assets/Scripts/Core/GameSession.cs`
- `PerchSort/Assets/Scripts/Core/PerchSort.Core.asmdef`：`noEngineReferences: true`
- `PerchSort/Assets/Tests/EditMode/PerchSort.Core.EditModeTests.asmdef`：引用 Core，使用 TestAssemblies 的 NUnit
- `PerchSort/ProjectSettings/ProjectVersion.txt`
- `PerchSort/ProjectSettings/EditorSettings.asset`
- `PerchSort/ProjectSettings/ProjectSettings.asset`
- `PerchSort/Packages/manifest.json`、`packages-lock.json` 以及官方 2D 模板自带的其他 ProjectSettings、Scenes、Settings、Welcome 资源

本次补齐或修改：

- `PerchSort/Assets/Tests/EditMode/CoreTests.cs`：30 个验收用例
- `scripts/run_tests.sh`：补充 XML 中通过数须等于总数的检查
- `PerchSort/Assets/Scripts.meta`、`PerchSort/Assets/Scripts/Core.meta`、Core 下所有 `.cs.meta` / `.asmdef.meta`：Unity 本次导入生成
- `PerchSort/Assets/Tests.meta`、`PerchSort/Assets/Tests/EditMode.meta`、EditMode 下 `.cs.meta` / `.asmdef.meta`：Unity 本次导入生成
- `tasks/T1_report.md`

验收产物：

- `build/test-results.xml`
- `build/editmode-tests.log`
- `build/create-project.log`：中断前项目创建的原有记录

## 运行方法与结果

在仓库根目录执行：

```bash
cd /Users/apple/Documents/projects/game
bash scripts/run_tests.sh
```

默认使用 `/Applications/Unity/Hub/Editor/6000.6.7f1/Unity.app/Contents/MacOS/Unity`，可用 `UNITY_EDITOR` 环境变量指定可执行文件。脚本以 `-batchmode -nographics -runTests -testPlatform EditMode` 运行；不传 `-quit`，由 Unity Test Runner 在完成后退出。

本次实测结果（2026-10-31，台北时间约 13:08）：

- 脚本退出码 **0**；XML：`result="Passed" total="30" passed="30" failed="0" inconclusive="0" skipped="0"`。
- 测试执行耗时约 **3.04 秒**，不含 Unity 启动和导入。
- 随机种子 **20261031**：**1000 个序列、32,286 次移动、50 次收集**，逐步撤销后所有内容、元数据及稳定哈希均还原。
- `build/editmode-tests.log` 包含 `Test run completed. Exiting with code 0 (Ok).`，无 `error CS…`、`Compilation failed` 或脚本编译失败记录。
- Core 源码无任何 UnityEngine 引用，独立 asmdef 的 `noEngineReferences` 为 true；实际 Unity 导入和编译成功。

## 后续调用约定与已知问题

`Rules.TryApply` 成功时原地修改传入的 `BoardState`；失败时状态不变，只有成功返回时才读取 `MoveResult`。鸟列表从左向右，最后一项可移动；非收集移动返回 `Collected=false`、收集索引及种类为 `-1`。`Undo()` 和 `Restart()` 会替换 `GameSession.State`，表现层应重新读取当前状态引用。

当前范围无已知功能问题。Unity 日志有许可证令牌更新不可用的非阻塞提示，本次导入、编译及测试正常完成；正式界面、关卡生成、美术与移动端打包属于后续任务。
