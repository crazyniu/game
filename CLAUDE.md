# Perch Sort — 协作说明（Claude 读这里；Codex 读 AGENTS.md，内容一致）

Unity 6000.6.7f1 竖屏小鸟归类益智游戏（原创题材/美术/文案）。权威设计：`docs/DESIGN.md`；UI 规范：`docs/UI_SPEC.md`；上架清单：`docs/RELEASE_CHECKLIST.md`；历史任务与验收报告：`tasks/T*.md`（最新 T15b）。

## 机器与分工（2026-10-31 起）
| 机器 | 角色 |
|---|---|
| **251**（Linux，192.168.1.251，`~/Projects/game`） | 开发：Claude 拆任务/写验收/验收/决策，Codex 写代码、出图。**无 Unity Editor**，不能编译或跑测试。 |
| **Mac**（192.168.1.242，Orca 环境 `Mac-Image-Worker`，旧路径 `/Users/apple/Documents/projects/game`） | 只做测试：Unity 编译、EditMode/PlayMode 测试、截图。不在 Mac 上改代码。 |

旧流程中 Mac 上的 Claude 不再使用。

## Git 流程（两边同一个仓库 `origin = github.com/crazyniu/game`）
1. 251 从 `main` 开分支 `t<编号>-<简述>`，Codex 实现，Claude 审代码后提交并 push。
2. Mac：`git fetch && git checkout <分支> && git pull`，然后运行
   - `bash scripts/run_tests.sh`（会先 SceneBuilder 重建场景；EditMode ≥101、PlayMode ≥11 全绿）
   - `bash scripts/screenshots.sh`（截图在 `build/screens/flow/`）
   跑之前确认没有其它 Unity 进程。
3. 测试结果/截图回到 251：Mac 可达时用 Orca `--environment Mac-Image-Worker` 远程执行并读取；不可达时由用户手动跑并把输出/截图发过来。`build/` 已被 gitignore，不要提交测试产物。
4. 验收通过 → 合并到 `main` 并 push；不通过 → 带具体问题清单让 Codex 返工。
5. Mac 上若 Unity 改写了 `.meta`/`ProjectSettings`（例如 SceneBuilder、ReleaseSettings），把这些改动从 Mac 提交到同一分支 push，251 再 pull，避免两边分叉。

## 纪律
- Core（`Assets/Scripts/Core`）纯 C#、不引用 UnityEngine；逻辑与动画分离。改 Core 规则或关卡前先更新 `docs/DESIGN.md` 并经用户同意。
- 不改 `art/` 原件；新资源另出派生文件。
- 每个任务写 `tasks/T<n>_<name>.md`（需求+验收标准）和 `tasks/T<n>_report.md`（交付说明）。
- 保持原创：题材、美术、命名、数值、文案都自己定，不照搬任何现有游戏的素材或文字。
- T12 内容扩量暂停，关卡保持 40 关；T14 上架需用户提供账号/包名（当前 `com.todo.perchsort` 是占位）。
