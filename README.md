# 小鸟归巢 · Perch Sort

Unity **6000.6.7f1** 竖屏离线排序益智游戏：把外露连续同种小鸟搬到空枝/同种端，凑齐六只并按当前鸟巢目标回家。

**当前代码是v2，已有40关。新版PRD是设计评审稿，蛋与临时栖枝尚未实现，新版40关也未落库或完成可玩性验证。** 本轮只整理文档，不改变游戏和已验收UI。

## 从这里开始

| 文档 | 用途 |
| --- | --- |
| [主PRD](docs/PRD.md) | 产品目标、统一规则、蛋与临时枝、交互/动画、恢复与验收 |
| [40关逐关玩法](docs/LEVEL_DESIGN.md) | 每关参数、怎么玩、关键取舍、与相邻关差异、舒缓节奏、三个灰盒 |
| [同类游戏研究](docs/SORTING_RESEARCH.md) | Water/Ball/Bird/Nut/Goods第一方来源，采纳与不采纳理由 |
| [现行实现基线](docs/DESIGN.md) | v2实际规则、技术与历史演进；不把新版规划当已完成 |
| [UI规范](docs/UI_SPEC.md) | 既有视觉按文件内部最新修订；本轮不重画 |
| [协作入口](AGENTS.md) | 机器分工、文档优先级、Git审核与验证要求 |
| [发布清单](docs/RELEASE_CHECKLIST.md) | 发布前要求，含待清零占位；不是已上架结论 |
| [本轮交付/验证](tasks/T16_report.md) | 实际改动、检查证据、未验证边界与提交门槛 |

## 当前实现与新版方向

现行v2：普通枝六座、短枝四/五座（仅中转）、单座篮、双激活目标、非当前满组睡眠冻结、目标补位自动归巢、计数/种类箱、无限撤销/重开、本地进度与提示。40关均使用JSON version2；生成器以环形分层布局为主，存在换种类等价题。

新版设计保留自动归巢的流畅体验，轮换经典腾位、短枝/篮周转、探蛋、条件救援、临时空间和目标连锁六类决策；每关最多两类特殊机制、每五关舒缓。新增重点是**纹样可预知物种的蛋**和**首次接鸟后再次腾空才收起的临时短枝**。无广告/内购/体力/随机洗牌；主动放飞与风闸仅后续实验。

## 工程与运行

- `PerchSort/`：用固定版本Unity Hub打开；入口场景`PerchSort/Assets/Scenes/Game.unity`。
- `Assets/Scripts/Core`（工程内）：纯C#逻辑、求解、生成、JSON；`View/UI/Audio`负责表现/流程/声音。
- `PerchSort/Assets/Resources/Levels/levels.json`：现行40关，**不要用旧生成器覆盖新版设计或声称生成了新关**。
- `art/`、`audio_src/`：源资源；`tasks/`为历史需求与验收；`build/`为忽略的本地产物。

开发在251，Mac用于Unity编译/测试/截图；Mac原检出路径`/Users/apple/Documents/projects/game`。先读[AGENTS](AGENTS.md)。运行脚本前确认本工程没有被Unity Editor占用，不强杀用户编辑器。脚本会重建场景，先保留已有改动；发布账号、包名和平台模块仍有待办。

```bash
bash scripts/run_tests.sh
bash scripts/screenshots.sh
```

上面是后续实现验收命令，本轮文档修改未执行。`run_tests.sh`历史最低门槛是EditMode101、PlayMode11；最新历史报告记载163个EditMode通过、PlayMode14通过/8个Explicit跳过，应如实记录跳过，不能写22/22通过。每关新内容还要重新求解、绑定hash、无帮助实机路径与真人观察，旧40关测试不为新版背书。

## 协作与范围

T12内容扩量仍暂停；本轮只为新版40关定义内容。新增规则、关卡数据、存档schema或视觉实施需另行评审；Core与动画分离，原始美术不覆盖。按既有流程由Claude审核并提交，Codex不自行提交/推送；不自动合并、部署、发布。
