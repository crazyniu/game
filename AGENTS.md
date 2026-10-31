# Perch Sort — 协作说明（Codex 读这里；Claude 读 CLAUDE.md，内容一致）

Unity 6000.6.7f1 竖屏小鸟归类益智游戏（原创题材/美术/文案）。权威设计：`docs/DESIGN.md`；UI 规范：`docs/UI_SPEC.md`；上架清单：`docs/RELEASE_CHECKLIST.md`；历史任务与验收报告：`tasks/T*.md`（最新 T15b）。

## 机器与分工（2026-10-31 起）
| 机器 | 角色 |
|---|---|
| **251**（Linux，192.168.1.251，`~/Projects/game`） | 开发：Claude 拆任务/写验收/验收/决策，Codex 写代码、出图。**无 Unity Editor**，不能编译或跑测试。 |
| **Mac**（192.168.1.232，Orca 环境 `Mac-Image-Worker`，旧路径 `/Users/apple/Documents/projects/game`） | 只做测试：Unity 编译、EditMode/PlayMode 测试、截图。不在 Mac 上改代码。 |

旧流程中 Mac 上的 Claude 不再使用。

## Git 流程（两边同一个仓库 `origin = github.com/crazyniu/game`）
1. 251 从 `main` 开分支 `t<编号>-<简述>`，Codex 实现，Claude 审代码后提交并 push。
2. Mac：`git fetch && git checkout <分支> && git pull`，然后运行
   - `bash scripts/run_tests.sh`（会先 SceneBuilder 重建场景；EditMode ≥101、PlayMode ≥11 全绿）
   - `bash scripts/screenshots.sh`（截图在 `build/screens/flow/`）
   跑之前确认没有其它 Unity 进程。
3. 测试结果/截图回到 251：251 直接远程驱动 Mac（不可达时才请用户手动跑）：
   ```bash
   orca status --environment Mac-Image-Worker --json
   orca terminal create --environment Mac-Image-Worker --worktree path:/Users/apple/Documents/projects/game \
     --title Mac-Test --command "git fetch && git checkout <分支> && git pull --ff-only && bash scripts/run_tests.sh" --json
   orca terminal read --environment Mac-Image-Worker --terminal <handle> --json
   ```
   - 跑之前 Mac 上**不能开着 Unity 编辑器**打开本工程（否则 batchmode 秒退 exit 1），请用户退出编辑器，不要替用户强杀。
   - `run_tests.sh` 会让 SceneBuilder 重写 `Assets/Scenes/Game.unity`，仅 fileID 重新编号；若分支本身没改场景，测完 `git checkout -- PerchSort/Assets/Scenes/Game.unity` 丢弃。
   - PlayMode 当前 14/22 通过、8 个是代码里标注的有意 Skip（Level10/20/30/40 等延后项），不算失败。
   - 取截图到 251：Mac 上临时 `python3 -m http.server 8765 --bind 0.0.0.0 -d build/screens/flow &`，251 上 `curl` 到 `/tmp/perch-screens/flow/`，取完立即 kill（Mac→251 无免密 SSH）。
   Mac IP 变了导致连不上时，改 251 上 `~/.orca/orca-environments.json` 里的 `endpoint`（配对凭据不变）。`build/` 已被 gitignore，不要提交测试产物。
4. 验收通过 → 合并到 `main` 并 push；不通过 → 带具体问题清单让 Codex 返工。
5. Mac 上若 Unity 改写了 `.meta`/`ProjectSettings`（例如 SceneBuilder、ReleaseSettings），把这些改动从 Mac 提交到同一分支 push，251 再 pull，避免两边分叉。

## 纪律
- Core（`Assets/Scripts/Core`）纯 C#、不引用 UnityEngine；逻辑与动画分离。改 Core 规则或关卡前先更新 `docs/DESIGN.md` 并经用户同意。
- 不改 `art/` 原件；新资源另出派生文件。
- 每个任务写 `tasks/T<n>_<name>.md`（需求+验收标准）和 `tasks/T<n>_report.md`（交付说明）。
- 保持原创：题材、美术、命名、数值、文案都自己定，不照搬任何现有游戏的素材或文字。
- T12 内容扩量暂停，关卡保持 40 关；T14 上架需用户提供账号/包名（当前 `com.todo.perchsort` 是占位）。

## Codex 专用
- 只做协调者（Claude）派发的当前任务；有歧义用 `orca orchestration ask`，不要自行猜测扩大范围。
- 251 上无法运行 Unity：写完代码后自查编译风险（命名空间、asmdef 引用、序列化字段），在报告里列出需要在 Mac 上验证的点。
- 不提交 git，由 Claude 审完后提交。
