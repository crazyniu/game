# 小鸟归巢：数据模型、事务提交与存档迁移

版本：schema设计1.0 · 2026-10-31 · **DESIGNED / PENDING_GRAYBOX**，尚未实现。规则由[RULES](RULES.md)拥有，本文只定义字段、持久化和版本边界。

## 1. 版本与身份

| 字段 | 新版设计值/类型 | 含义 |
| --- | --- | --- |
| `schemaVersion` | int，3 | 关卡文件格式；当前文件为2，不能默默按新格式解释 |
| `rulesVersion` | int，3 | 自动归巢+纹样蛋+临时短枝的规则合同；手动放飞/风闸不在该版本 |
| `campaignId` | string，`bird-home-v3` | 新内容身份；旧40关归档为`perch-sort-v2`，同levelId不表示同题 |
| `catalogRevision` | int，从1递增 | 同campaign中内容更新，必须配合hash检查 |
| `levelId` | int，1..40 | 仅在campaign内唯一；文案“01”只是显示格式 |
| `contentHash` | 64个小写十六进制字符（256bit SHA-256） | 精确语义内容；种类、位置、蛋、临时枝、箱、目标均影响hash |
| `saveVersion` | int，5 | 新存档；现行进度v4不包含新规则状态 |
| `actionSequence` | 非负int64 | 本局已提交事务序号，Undo/Restart也递增，用于过期回调与幂等 |

每关是冻结的固定资源，不允许运行时生成/洗牌/随机变种/每次换seed。Replay、Restart与重新进入相同contentHash必须恢复同一初始token顺序、容量/位置、目标、蛋/箱/临时状态。离线策划工具输出只有审查冻结后才是关卡，未填数组禁止由运行时AI自由补齐。

这些值为本设计的实施目标，不是当前已落库状态。版本字段不从文件名/关卡号猜测；未知版本不得用默认值吞掉。

### 1.1 contentHash规范字节（MUST）

先完整校验类型、范围、唯一ID和鸟数，再补默认到固定字段集合；不得直接对用户原始JSON文本求hash。固定哈希根字段仅为`schemaVersion,rulesVersion,campaignId,levelId,targets,baskets,shelves`；root元数据`catalogRevision,contentHash,solverMinMoves`允许但不入hash，其余未知字段拒绝，防止新机制被静默忽略。

默认只允许：`temporary`省略→false、`crate`省略或null→null、根`baskets`省略→空数组；其余本表required字段缺失均拒绝。每Shelf规范字段固定为`id,side,capacity,temporary,tokens,crate`；每Token固定`id,species,form`；每Basket固定`id,tokens`。`form`不能省略，`crate`非null时恰一个允许条件字段。归一化后默认省略和显式值必须同hash。

标识字符串MUST匹配ASCII安全字符`[A-Za-z0-9][A-Za-z0-9._-]{0,63}`（位置额外满足S1..SP/T1..T2），枚举也只用本文给定ASCII值。禁止Unicode标识、控制字符、引号、反斜线，避免不同转义器输出不一致。对象键按ASCII码升序、无空白/换行/BOM、UTF-8编码；布尔/null严格小写；所有数值为整型十进制，无正号/前导零/负零/小数/指数。数组保留原顺序，MUST不排序token或shelves。

规范字节经SHA-256得64个小写hex字符。contentHash排除自身和证书/难度/seed/说明；同构布局仍可有不同hash，去同构不属于此算法。完整字节与真实SHA测试向量见§2.1。

## 2. 静态关卡字段（字段名大小写固定）

| 对象/字段 | 类型/约束 | 空缺/非法处理 |
| --- | --- | --- |
| `targets` | int[K]，恰覆盖0..K-1，K=3..8 | 不允许重复/缺种 |
| `shelves` | ShelfData[]，顺序固定，最多10枝 | 自动事件顺序按[RULES§1.1](RULES.md#11-棋盘与移动)下标，不按ID字符串排序；退役不删元素或重排 |
| `ShelfData.id` | string `S1`..`SP`，目录编号一一对应 | 唯一且非空 |
| `side` | `left`或`right` | 只影响显示镜像 |
| `capacity` | int，普通4/5/6；临时4/5 | 不按画图长度猜容量 |
| `temporary` | bool，缺省false | true时初始tokens必须空，crate必须null |
| `tokens` | BirdToken[]，外墙→通道，长度≤capacity | 正式开局不能可见完整组六鸟 |
| `BirdToken.id` | string，关内唯一稳定；例`L11-S1-03` | 孵化、移动、Undo不改ID，不用当前槽号重生成 |
| `species` | int，0..K-1，蛋也必填 | 不运行抽种逻辑 |
| `form` | `bird`或`egg` | 首版egg纹样映射species；不直接当可搬鸟 |
| `crate` | null或`{revealAfter:n}`或`{revealSpecies:s}` | 严格二选一；n=1..K，s在species范围；不能覆盖临时枝 |
| `baskets` | BasketData[]，数量0..2 | 初始均空，不含egg，不临时退役 |
| `BasketData.id` | `T1`/`T2`，唯一 | ID不随枝数变化而改变；旧整数索引要适配 |
| `solverMinMoves` | authoring为null，发布必须int≥1且证书Exact | 禁止用0/旧值占位冒充已证明 |

每种恰六token，含蛋、篮、箱下；正式篮初空，因此初始鸟全在枝上。相邻蛋允许、可见露端蛋正式开局拒绝；箱下露端蛋不算可见，开箱后规范化。箱下六只已孵化同种拒绝，箱下五Bird+一Egg不属于已完成群，需揭示并孵化后才完成。

### 2.1 完整合法小关卡与SHA-256测试向量

以下是完整**authoring**关卡（18只鸟、5枝、无初始完整群），独立固定schema测试夹具H01，**不对应新版主线01**；结构合法但`solverMinMoves=null`表示尚无新版最优证书，因此不是可发布关卡。它不是前版缺鸟的片段；levelId=1是该测试夹具的局部标识，禁止覆盖LEVEL_SIMULATIONS中的新01。为兼容规范测试，下面canonical bytes与hash保持不变。

```json
{
  "schemaVersion": 3,
  "rulesVersion": 3,
  "campaignId": "bird-home-v3",
  "levelId": 1,
  "targets": [
    0,
    1,
    2
  ],
  "baskets": [],
  "shelves": [
    {
      "id": "S1",
      "side": "left",
      "capacity": 6,
      "temporary": false,
      "tokens": [
        {
          "id": "L1-S1-01",
          "species": 0,
          "form": "bird"
        },
        {
          "id": "L1-S1-02",
          "species": 1,
          "form": "bird"
        },
        {
          "id": "L1-S1-03",
          "species": 1,
          "form": "bird"
        },
        {
          "id": "L1-S1-04",
          "species": 1,
          "form": "bird"
        },
        {
          "id": "L1-S1-05",
          "species": 1,
          "form": "bird"
        },
        {
          "id": "L1-S1-06",
          "species": 1,
          "form": "bird"
        }
      ],
      "crate": null
    },
    {
      "id": "S2",
      "side": "right",
      "capacity": 6,
      "temporary": false,
      "tokens": [
        {
          "id": "L1-S2-01",
          "species": 1,
          "form": "bird"
        },
        {
          "id": "L1-S2-02",
          "species": 2,
          "form": "bird"
        },
        {
          "id": "L1-S2-03",
          "species": 2,
          "form": "bird"
        },
        {
          "id": "L1-S2-04",
          "species": 2,
          "form": "bird"
        },
        {
          "id": "L1-S2-05",
          "species": 2,
          "form": "bird"
        },
        {
          "id": "L1-S2-06",
          "species": 2,
          "form": "bird"
        }
      ],
      "crate": null
    },
    {
      "id": "S3",
      "side": "left",
      "capacity": 6,
      "temporary": false,
      "tokens": [
        {
          "id": "L1-S3-01",
          "species": 2,
          "form": "bird"
        },
        {
          "id": "L1-S3-02",
          "species": 0,
          "form": "bird"
        },
        {
          "id": "L1-S3-03",
          "species": 0,
          "form": "bird"
        },
        {
          "id": "L1-S3-04",
          "species": 0,
          "form": "bird"
        },
        {
          "id": "L1-S3-05",
          "species": 0,
          "form": "bird"
        },
        {
          "id": "L1-S3-06",
          "species": 0,
          "form": "bird"
        }
      ],
      "crate": null
    },
    {
      "id": "S4",
      "side": "right",
      "capacity": 6,
      "temporary": false,
      "tokens": [],
      "crate": null
    },
    {
      "id": "S5",
      "side": "left",
      "capacity": 6,
      "temporary": false,
      "tokens": [],
      "crate": null
    }
  ],
  "catalogRevision": 1,
  "contentHash": "7b22db03e9ececf99c7fea9626a773ae26e60d77d3a78bdaa2ece385f80317de",
  "solverMinMoves": null
}
```

规范UTF-8字节如下（代码块边界和末尾换行不属于字节）：

```text
{"baskets":[],"campaignId":"bird-home-v3","levelId":1,"rulesVersion":3,"schemaVersion":3,"shelves":[{"capacity":6,"crate":null,"id":"S1","side":"left","temporary":false,"tokens":[{"form":"bird","id":"L1-S1-01","species":0},{"form":"bird","id":"L1-S1-02","species":1},{"form":"bird","id":"L1-S1-03","species":1},{"form":"bird","id":"L1-S1-04","species":1},{"form":"bird","id":"L1-S1-05","species":1},{"form":"bird","id":"L1-S1-06","species":1}]},{"capacity":6,"crate":null,"id":"S2","side":"right","temporary":false,"tokens":[{"form":"bird","id":"L1-S2-01","species":1},{"form":"bird","id":"L1-S2-02","species":2},{"form":"bird","id":"L1-S2-03","species":2},{"form":"bird","id":"L1-S2-04","species":2},{"form":"bird","id":"L1-S2-05","species":2},{"form":"bird","id":"L1-S2-06","species":2}]},{"capacity":6,"crate":null,"id":"S3","side":"left","temporary":false,"tokens":[{"form":"bird","id":"L1-S3-01","species":2},{"form":"bird","id":"L1-S3-02","species":0},{"form":"bird","id":"L1-S3-03","species":0},{"form":"bird","id":"L1-S3-04","species":0},{"form":"bird","id":"L1-S3-05","species":0},{"form":"bird","id":"L1-S3-06","species":0}]},{"capacity":6,"crate":null,"id":"S4","side":"right","temporary":false,"tokens":[]},{"capacity":6,"crate":null,"id":"S5","side":"left","temporary":false,"tokens":[]}],"targets":[0,1,2]}
```

- 字节长度：`1320`。
- SHA-256：`7b22db03e9ececf99c7fea9626a773ae26e60d77d3a78bdaa2ece385f80317de`。
- 等价默认测试：删去全部`temporary:false`、`crate:null`及根空`baskets`后，经§1.1补默认，必须得到完全相同字节/hash。
- 顺序反例：交换S1中两个同种token ID的数组位置，会改变hash；不能因species相同而排序或复用旧证书。

该向量仅用独立文档脚本计算hash与结构数量，不代表新Core/最优求解已实现。

## 3. 运行状态与Clone/键

| 运行字段 | 唯一来源与语义 |
| --- | --- |
| `tokensByLocation` | 移动后的有序token（ID/species/form），包含篮 |
| `temporaryState` | 普通枝为null；临时枝为`Unused/Armed/Retired`，单一枚举，不并列存armed/retired布尔冲突 |
| `Asleep` | 规则保存的枝状态，稳定态与完整同种且非激活一致 |
| `Crate` | 剩余箱条件；揭示后null，不另存矛盾revealed标志 |
| `Targets` | 静态队列引用/校验副本，不能因归巢删除原序列 |
| `CollectedTargets` | 已收集物种无重复；ActiveTargets由Targets减去集合后取前二，不另保存 |
| `CompletedCount` | 首次完成累计；稳定态应等于已收集种类数+睡眠完整群数，不因孵化/退役重复增加 |
| `Moves` | 本局已提交Transfer序列，用稳定location ID，记录实际Count供校验 |

Clone必须深拷贝全部可变列表、token形态、临时枚举、箱和规则计数。精确状态键含rulesVersion、contentHash、固定位置顺序/容量、所有token ID/species/form、临时态、睡眠/箱、目标与收集/完成。动画帧/声音/提示次数/星级不参与规则状态键；提示请求另绑定actionSequence。内容去同构不能重排运行状态。

## 4. 持久化事务：先提交稳定终态，再播放

**本文是事务保存时机唯一权威。** 规则稳定化与视觉稳定是两个阶段：新逻辑终态先原子保存，之后才演出其事件；输入仍锁到演出完成。不能混成“动画每飞一只存一次”或同时保留另一套“动画完才写”策略。

1. 锁输入，在上个稳定快照上构造候选状态，验证并执行一次Transfer及全部稳定化，得到终态和有序事件。
2. 候选Save包含终态、Undo历史和进度更新、递增actionSequence。写同目录`.tmp`，flush后原子替换主档，保留前一兼容档`.bak`。
3. **原子成功才提交内存候选并播放事件。** 写入异常进入§4.1 SaveError，旧确认State/步数/进度不变，保留同一候选供唯一重试策略；不得永久停在Resolving。
4. 从旧视图按事件演出终态；动画完同步规则状态并解锁/显示胜利。奖励/通关记录以事务号幂等，动画回调不再写第二次完成。
5. 正常后台或杀进程发生在原子提交后，重进直接显示已提交终态；不必重播半段动画。若在原子提交前异常强杀，恢复上一次成功快照。不得混合旧鸟和新枝状态。

### 4.1 SaveError与重试（唯一失败策略）

- 每次新候选使用`candidateSequence=lastCommittedSequence+1`，只有原子提交被确认后才增加已提交序号；失败重试不再+1，也不重新运行规则或重新构造另一个动作。
- SaveError保留不可变的旧稳定快照/原主档字节、同一候选完整字节/事件及From/To请求，取消视觉选择。尚未播放成功移动，旧棋盘和Undo历史可见；棋盘/Undo/Restart/Hint/Next锁定，但**重试、返回菜单、退出**可用。
- 替换抛异常若结果可能已执行，先读回主档：若字节完整等于候选且身份/序号校验一致，视为已成功，提交该候选并播放一次；若等于原主档，确定未提交；若两者均不匹配或暂无法读，保留不确定错误，不盲目重复写。
- “重试”先执行上述核验；确认原档未提交后，用**同一候选字节、同一candidateSequence**再原子写。成功→Presenting→正常稳定态；失败→SaveError。若先前其实成功，核验后只接续表现，不重复计步/领奖。
- “返回菜单”不写档、不计步；确定未提交时丢弃待重试候选，恢复旧稳定局并允许继续。结果仍不确定时允许返回**只读菜单**（核验重试/退出可用，不启动/覆盖任何对局），待存档恢复明确后才继续。退出也不覆盖任何档。
- Undo/Restart的写档失败用相同策略，旧board和原Undo栈不先弹出/清空。失败不能递增已提交序号、计数或提示消耗；不得通过关闭弹窗假装保存成功。

Core仍纯C#，不自己访问Unity路径/文件；UI/存档适配器组织候选提交，文件失败不污染Core已确认State。

新存档以**稳定board快照为恢复权威**，加完整Undo快照栈保证退出后仍可无限撤销；移动日志仅用于回放校验/诊断，不能覆盖不一致快照。每次Undo保存恢复后的board和弹栈结果为一个新事务，失败保留Undo前态。Restart保存初始board并清当前Undo栈/移动日志，仍保留最高成绩/已通关/提示消耗/设置；评级局计数/提示额度的首版政策唯一见[交互§3](INTERACTION_ANIMATION.md#3-信息表达与辅助)。

实现可用压缩快照或可证明等价的持久化delta减少体积，但不得静默截断可撤销历史。存档同时含campaign/rules/content身份、当前level、稳定状态、history、动作序号、进度与设置引用；不保存半途Transform坐标、协程时刻或选中视觉状态。

## 5. 旧档迁移与内容更新

| 输入情况 | 必须行为 |
| --- | --- |
| 旧进度v4 / rules2 | 先保留原文件只读迁移副本；归档到`legacyProgress[perch-sort-v2]`，保留通关/星/提示/当前关信息，不把旧Move用于新题；旧提示消耗仅留旧campaign归档，新campaign按交互政策重新计数 |
| 首次进新版campaign | 显示玩法更新说明；从新01关开始教学，旧成绩继续归档可读，不直接映射同号新关为已通关；旧未完局不伪装新版继续 |
| 新campaign内仅catalog修订，当前level hash不变 | 原样恢复快照、Undo历史与进度 |
| 同campaign当前level hash改变 | 保留该关历史最高成绩并标旧hash；丢弃该局运行快照/Undo到隔离备份，按新布局重开当前关并说明；未完成评级/提示消耗保留，避免升级刷资源 |
| 主档损坏/校验失败 | 尝试身份兼容且完整的bak；成功后记录恢复，失败保留原件并按对应campaign安全开局，不静默覆盖唯一损坏证据 |
| 未知save/schema/rules版本 | 明确“不支持此版本”并保留原件；不按空档解析、不自动降级、不覆盖；可返回菜单/退出，不能伪造成功恢复 |
| 新档未来版本但有旧兼容bak | 经明确恢复说明选择兼容bak，不丢弃未来版本原件；无兼容bak不强开覆盖 |

旧campaign默认不在新关卡格混排；是否提供独立旧关入口是OPTIONAL，不阻止保留数据。正式发布前冻结路径命名与catalog身份；公司名/产品名会影响Unity存档目录，不能只改包名不做升级验证。

## 6. 前置/后置与反例

- Given原子写成功、归巢动画播到第三只；When杀进程重进；Then恢复完整已提交终态，不重复收前三只、不将未播鸟返还。
- Given正在写tmp但尚未替换；When强杀；Then主档仍是前一完整状态，孤立tmp不当作新主档。
- Given旧levelId=11已通关；When迁移新campaign；Then旧11成绩保留归档，新11不因此自动标通关。
- GivenUndo恢复Unused空临时枝；When保存/加载；Then仍Unused，不运行正向空枝退役。
- Given相同levelId但contentHash不同；When请求恢复；Then走内容更新策略，不能重放旧From/To。

完整运行与文件故障测试按[VALIDATION](VALIDATION.md)举证；本文件不宣称新saveVersion5已经实现。
