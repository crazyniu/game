# T1 — Unity 项目骨架 + Core 逻辑层 + 测试

先阅读 `docs/DESIGN.md`（唯一权威规则）。你只在 `/Users/apple/Documents/projects/game` 内工作；**不要碰 `art/` 目录**（另一个任务在写）。

## 做什么
1. 用 Unity `/Applications/Unity/Hub/Editor/6000.6.7f1/Unity.app/Contents/MacOS/Unity` 的 `-batchmode -quit -createProject` 在 `/Users/apple/Documents/projects/game/PerchSort` 创建 2D 项目（若 2D 模板不可用，创建空项目并设置为 2D）。竖屏默认。加 `.gitignore`（Unity 标准：Library/ Temp/ Logs/ obj/ UserSettings/ *.csproj *.sln 等）。
2. `Assets/Scripts/Core`（asmdef `PerchSort.Core`，`noEngineReferences: true`），纯 C#：
   - `BoardState`：perches（每根栖木是 bird id 的有序列表）、容量、种类数。可克隆、可序列化、有稳定哈希。
   - `Move`（from, to）、`Rules.GetLegalMoves(state)`、`Rules.TryApply(state, move, out result)`；result 要包含：移动的 bird、目标栖木是否因此集齐（并返回被收集的栖木索引与种类）。
   - 集齐规则：栖木装满容量且全同种 → 立刻清空（收集）。
   - `Rules.IsWon`、`Rules.IsDeadlock`（未通关且无合法移动）。
   - `UndoStack`/`GameSession`：封装当前状态 + 无限撤销 + 重开；撤销必须能还原“被收集的栖木”。
   - 为后续扩展留位但**不实现**：栖木可有 `locked`/`extra` 标志字段（默认 false，不参与规则）。
3. `Assets/Tests/EditMode`（asmdef，引用 Core 和 NUnit）：覆盖——合法/非法移动（空来源、满目标、同位置）、仅右端可取、允许异种叠放、集齐清空、集齐后撤销还原、连续撤销到初始、通关、死局、克隆独立性、哈希一致性、随机局面下 apply+undo 往返不变式（至少 1000 个随机序列）。
4. 提供可重复运行的测试脚本 `scripts/run_tests.sh`（Unity batchmode `-runTests -testPlatform EditMode`，输出 XML 到 `build/test-results.xml`，失败时非零退出）。

## 验收标准（Claude 将亲自运行）
- `bash scripts/run_tests.sh` 退出码 0，XML 中无失败，用例数 ≥ 25。
- Core 目录没有任何 `using UnityEngine`。
- 项目能被 Unity 无错误导入（batchmode 日志无编译错误）。
- 代码简洁，不过度抽象，不写与规则无关的功能。

## 交付
完成后在 `tasks/T1_report.md` 写：做了什么、文件清单、如何运行测试、已知问题。不要提交 git。
