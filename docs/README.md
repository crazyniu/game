# 文档入口：给零上下文AI的最少阅读路径

先读根[README](../README.md)了解当前/目标，再读[AGENTS](../AGENTS.md)确定工作地点、审核与资源门槛。当前实现为v2；新蛋/临时枝是设计稿，未落库/未实机验证。本次仅整理/评审文档和桌面玩法推演，不授权代码或素材实施；未来实施必须另获明确授权，规范中的MUST是设计合同而非执行许可。

## 1. 状态与规范词

- `IMPLEMENTED_V2`：现行源码/数据可定位的事实，是否本轮测试另记。
- `DESIGNED`：新版规范合同，MUST定义实施目标，不代表已实现。
- `PENDING_GRAYBOX`：新机制或具体关卡还需新Core/实机/真人验证，不能写“已可解/好玩”。
- `OPTIONAL`：后续候选，未进入首版必做；不得无批准混入代码或宣传。

状态表示实施与证据维度，可并列；比如DESIGNED+PENDING_GRAYBOX。`MUST`强制；`SHOULD`建议，偏离必须在报告解释；`MAY/OPTIONAL`可选。没有关键词的确定性规则表也视为MUST。本设计冲突先停相关实施并报告，不能猜一套新规则，独立工作可继续。

## 2. 每个概念唯一权威

| 文件 | 拥有的内容 | 不拥有/不覆盖 |
| --- | --- | --- |
| [PRD](PRD.md) | 产品目标、范围、六类玩法、课程与取舍 | 不重复状态机/字段算法 |
| [RULES](spec/RULES.md) | 名词、合法动作、孵化/临时枝/满组/箱/目标、稳定化顺序与回退 | 不决定文件写入时点/动画帧 |
| [LEVEL_DESIGN](LEVEL_DESIGN.md) | 40关课程/关键决策/差异/节奏，三个夹具 | 不重复附录初态或规则算法 |
| [LEVEL_SIMULATIONS](LEVEL_SIMULATIONS.md)与其四份固定附录 | 40份完整固定初态、60条完整胜利纸面回放、反例/多样性证据边界 | 非运行资源、最优/项目Core或真人证书 |
| [INTERACTION_ANIMATION](spec/INTERACTION_ANIMATION.md) | UI输入状态/动画阶段、时长建议、表现资源门槛 | 不由动画回调改规则；既有视觉服从UI_SPEC最新修订 |
| [DATA_SAVE](spec/DATA_SAVE.md) | 字段/ID/版本/hash、原子提交时机、存档/升级/Undo持久化 | 不改变合法移动/完成条件 |
| [VALIDATION](spec/VALIDATION.md) | 证明状态、证书、Given/When/Then、阶段门槛/真人与设备验证 | 不把预期或历史记录写成当前通过 |
| [CODE_MAP](spec/CODE_MAP.md) | 已实现基线/差异、代码阅读、迁移依赖 | 不把旧代码限制当新版产品意图 |
| [SORTING_RESEARCH](SORTING_RESEARCH.md) | 第一方来源、事实/未验证/推断、采纳拒绝 | 官方宣传不当独立实测 |
| [UI_SPEC](UI_SPEC.md) | 静态尺寸/坐标/样式、缩放安全区、设备触控验收目标，单值键UI-U/D/S/V | 已批准与代码事实/新增目标分开；时间/输入归INTERACTION |
| [ANIMATION_PROMPTS](spec/ANIMATION_PROMPTS.md) | 分镜/未来获授权实现/验收的可复制模板AP01..10 | 模板不授权执行；引用权威数值不另立新值 |
| [RELEASE_CHECKLIST](RELEASE_CHECKLIST.md) | 发布前身份/工具链/平台检查 | 旧截图/测试数量不是新版验收替代 |

历史v2设计已在[archive](archive/DESIGN_V2.md)，UI叠加修订已在[UI历史](archive/UI_SPEC_HISTORY.md)，旧[DESIGN入口](DESIGN.md)只做兼容导航。历史任务报告不是当前规范，不与主线并列。归档里的“当时里程碑/分工/规则”不覆盖当前用户指令、AGENTS或上表。

## 3. 按任务只读必要集合

| 要做什么 | 最少阅读集合（按顺序） |
| --- | --- |
| 理解产品/评审方向 | PRD→研究取舍→40关各章代表→对应固定回放 |
| 实现Core/修规则 | RULES全篇→DATA_SAVE§2–4→CODE_MAP→VALIDATION关键用例 |
| 制作关卡 | RULES→LEVEL_DESIGN对应卡→LEVEL_SIMULATIONS对应10关附录→DATA_SAVE静态schema→VALIDATION证明/公平性 |
| 做动画/UI | INTERACTION_ANIMATION→UI_SPEC对应UI-D行→ANIMATION_PROMPTS所需动作→RULES事件顺序→CODE_MAP资源依赖（另需实施授权） |
| 做存档/迁移 | DATA_SAVE全篇→RULES事务/Undo→VALIDATION恢复用例 |
| 跑测试/验收 | AGENTS→VALIDATION→任务对应规范→具体关卡hash/证书 |
| 查已实现与计划差异 | CODE_MAP→对应源码；不要先翻几十篇历史任务 |

## 4. 交付规则

每次变更报告MUST写规则/内容身份、实际改动、验证命令与输出、失败/跳过/未知、剩余边界。文档阶段只完成链接/概念/数量/推演审查；代码阶段要补真实新Core、Unity和设备证据。现40关已有完整固定文档布局及独立回放，但无项目求解证书/运行导入，必须继续写未落库、项目验证未运行；禁止把纸面见证写成最优或真人已验收。
