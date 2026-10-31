# 小鸟归巢：同行依据版规则（唯一规则权威）

规则版本4 · 2026-10-31 · DESIGNED / PARTIALLY_IMPLEMENTED_T21。先完成[50项证据矩阵](../MECHANIC_EVIDENCE.md)后重订；本页不是现行v2代码说明。每项玩法对应来源/用户约束；稳定ID、事务/排序是工程协议E，不伪称竞品算法。**T21 first-level implementation is authorized; full v4 remains incomplete.**

## 1. 采用与退出

| 合同 | 采用内容 | 同行依据与迁移差异 |
| --- | --- | --- |
| R01 固定盘面/数量 | 全40关固定，每关统一容量N∈{4,6,8}，K种各N只；02–40为K+2枝/两空，01按§2.1专属教学profile | USER固定；SRC-SIMPLE明确4单位/每色4/K+2两空；SRC-BOOP明确6/8层。我方把单位移为鸟，N扩至同类来源已有6/8，不能说某个BirdSort原作每种就是这些值 |
| R02 搬移 | 点源再点目标，只取露端连续同种；空/同种端且有余量；Count=min(run,free) | SRC-LEAF支持鸟端群/接收条件；SRC-SIMPLE明确截断规则、一次操作一步。不同来源组合公开标示，不冒充Leaf也明确部分搬移 |
| R03 问号蛋 | 固定隐藏物种，前鸟移开露端后原位孵化；没有种类纹样预告 | SRC-QUEST问号蛋与前鸟触发；固定隐藏数据遵USER，原作是否随机未知；锁run属于E协议；相邻蛋精确边界未核，移出当前配置 |
| R04 完成与失枝 | 同枝N只已孵化同种完整群自动飞回家，同时该完成枝退出后续整理 | SRC-BIRD2明确完成群飞走失去枝；完整群数N按R01配置。没有目标等待、睡眠冻结、满组箱事件 |
| R05 撤销与重开 | 免费无限Undo/Restart；Undo退回最近一次Transfer或AddBranch及其全部事件/步数；Restart固定初态 | SRC-LEAF支持回退/重开，SRC-SIMPLE明确无限免费Undo和回退步数；整快照协议见DATA_SAVE |
| R07 额外普通枝 | 每当前局至多一条额外空枝，容量等于本关N；免费、非强制；精确事务见§8 | SRC-WSWEB网页明确每关一空容器；N容量是本项目鸟主题适配；免费为项目非商业化配置，Undo/持久化为E协议 |
| R06 提示/成绩 | 免费不限次Hint，给当前已验证胜利路线的合法首步；记录最佳操作数，不扣Undo星级 | SRC-SIMPLE的无限免费解路径/最佳步数；SRC-LEAF少步高分。不要求提示证明最优，不编三星阈值或终身3次配额 |

本版是有出处的排序规则迁移组合，不是任何一款竞品的全套复制。未进入有效规则：前二目标队列/未轮到睡眠/累计或指定种箱/0–2单鸟篮/混容量短枝；提前告知蛋种类、首次接鸟后任意搬空退枝、风闸、Undo扣星及终身3提示全部撤回。其旧实现/观察只能在CODE_MAP和历史记录中阅读，不能按旧字段补回。

额外普通枝已由R07/§8定义完整采用合同；逐次扩高、只进不出容器、金币与皮肤商店仍见§7。固定初盘不等于禁止局内扩空间；本40基础配置及原见证均不使用AddBranch。

## 2. R01 棋盘/信息与术语

`Branch`为固定位置；`Token`为稳定ID、species、form；`form=bird|egg`。每枝cap=N，species为0..K-1。N是该关统一完整群大小，四鸟/六鸟/八鸟都不直接套现行代码固定六。初始每种恰N，包含蛋；初始没有完整同种满枝、没有可见外露蛋、禁止相邻蛋；02–40恰两根空枝，01仅按§2.1例外。右列只视觉镜像，数组始终墙→通道，末项外露；初始枝号S1..SP对应数组下标0..P-1，不按字符串排序S10/S2；R07可在固定下标P追加S(P+1)，不改变原ID/数组顺序。

枝状态只有`Available`与`Retired`。初始全部Available；只有R04的完成事件才变Retired，普通Transfer使枝为空仍Available。退役枝保留ID/位置和不可用标记，不删数组，不让后面枝挤位。没有Unused/Armed，也没有定时折断/鸟受伤。

问号蛋的species在固定数据里，玩家孵化前看不到，UI不能用颜色/花纹/轮廓提前透露。非外露鸟种按实际盘面呈现；不得从隐藏数据给玩家预览完整鸟种表。持久化数据知道身份属于E需求，不表示玩家知道。Undo会恢复壳，玩家记忆不可清除，不声称回退能重置人的知识。

### 2.1 Adopted L01 tutorial profile (project content decision)

For schema4/rules4 campaign bird-home-peer-v4, levelId=1/worldId=A alone, derive profile l01-two-flock-v1: N=4, K=2, P=3, all bird form, zero empty branches; exact arrays are S1=[A,A,A], S2=[B,B,B,A], S3=[B]. The complete witness is in [FIXED_01_10](../levels/FIXED_01_10.md#关卡01). Species A=0/B=1; token IDs follow DATA_SAVE. Do not add a runtime profile flag to bypass validation.

Levels02–40 retain standard-v4: K=3..8, P=K+2 and two empty branches. Do not generalize K2/P3/no-empty to other levels, campaigns or arbitrary layouts. Other R02–R07 semantics, uniform N, conservation and completion-only retirement remain unchanged. R07 uses base P=3 here, so its optional extra branch is S4, capacity4; neither two-transfer witness needs it.

The unique legal first Transfer is S2→S1 A1, immediately Home A/Retire S1. Either S2→S3 B3 or S3→S2 B1 then completes the level. Staggered supported visual positions do not change logical IDs/array order. One upper receiving nest is presentation, never an order queue.

## 3. R02 点选与合法搬移

前置：稳定Idle/Selected，源、目标不同且均Available；源非空，末token是已孵化鸟。run从源末端向内统计连续同species的bird，遇异种或egg停止。目标必须未满、为空或末端是相同species的bird；蛋不能当同种接收端。

合法时先从前态一次锁定run与Count=min(run,N-targetCount)，取源最后Count个token的原相对顺序，删除该切片后append至目标；Count≥1。逻辑顺序不能随动画先飞谁而逆转。例源`[B:id0,A:id1,A:id2]`→目标`[A:id3]`搬2后为`[A:id3,A:id1,A:id2]`，不是id2/id1倒序。

一次成功Transfer操作数+1；飞几只、孵几颗、完整群回家/退枝均不另计。四座局合法部分移动例：源`[B,A,A]`、目标`[B,A,A]`（合计A4/B2，其他枝补B2），只搬A1，源`[B,A]`目标`[B,A,A,A]`，目标混种不完成。不能再用总A超过N的局部例冒充完整关。

点同源/空白取消；非法目标不改变棋盘、操作数或历史，只取消选中并短反馈。来源是空/蛋/Retired时不能启动搬移。动画中不排队缓存Transfer。

## 4. R03/R04 一笔操作的稳定化（E事务协议）

1. 检查前置并锁run/Count；按切片原序完成逻辑Transfer。
2. 只检查本次受影响源/目标的外露蛋；未遮挡且成为末项的egg原位改bird，保持同ID/species。蛋不额外生鸟；本笔锁定数量不追加新孵鸟。
3. 按固定shelves下标检查受影响枝：若恰N个bird且species全同，则登记该种Home、清该枝，置Retired，输出Home→Retire事件；无目标资格限制/睡眠/箱循环。每种只可Home一次；全局守恒保证没有同种另一组。
4. 检查终态：每种现存token数+已Home该种N=N；Retired无token，Available空枝不退役。所有K种Home且无剩余token则Won，否则继续Idle，或合法Transfer集合为空时NoMoves。
5. 候选和完整Undo快照按DATA_SAVE原子确认，成功后再按事件演出；动画仅消费既定结果，不能由落地回调决定规则/奖励。

相邻蛋的原作精确触发未核，本版配置禁止相邻egg，不以“工程确定化”名义补造其玩法；未来有明确证据后另评审。一般孤立蛋仍严格来自前鸟移走触发；当前没有盖箱交互，不补造箱遮蛋流程。

同一玩家动作可以同时令来源Hatch与目标Home；在本版统一容量时（包括R07额外枝），搬出后的来源长度小于N，不能凭空在同一步完成第二群。事件按固定枝下标记录，Home后紧随该枝Retire。只涉及已决定事件，不存在目标队列刷新自动扫描。程序重入、后台、Undo与Hint的并发边界是E协议，见DATA_SAVE/INTERACTION。

## 5. R05 终局、回退与固定性

Won为全部种类Home且无token；不以“看起来每枝单色”提前完成不满群。NoMoves只是当前无合法Transfer，不等于完整搜索证明全局无解；可选R07加枝或免费Undo/Restart，不扣生命/体力/金币。没有倒计时或最大操作数失败。

Undo恢复最近一个Transfer或AddBranch前的全部token、蛋形态、枝状态、Home集合、操作数和辅助状态；包括已飞群与退役枝。例末只补成完整A→A Home+该枝Retired，Undo后A回枝并Available。**不执行“空就退役”二次归一化**。Restart清本局历史并从同hash固定资源复建，重开不能抽蛋种或改盘。退出恢复按存档身份，不从动画坐标回放。

## 6. R06 帮助与成绩

提示免费不限次且不自动走子；只在当前稳定stateKey/事务序号仍匹配、返回一条完整合法胜利路径时高亮其首步。没有必要伪证最优首步；非最优解也是可验证解。搜索预算耗尽/取消/失效返回Unknown、不扣任何次数/资源，也不声称无解；后台只保留一个搜索，结果过期丢弃。预算是可配置工程上限，不是玩家体力或费用。

操作数从0开始，每Transfer+1；Undo恢复前操作数；Restart重新0。每关分别保留无加枝路径与有加枝路径的最小成功通关操作数（E公平比较记录，不新增奖励）；按Won时extraBranchAdded分类，Undo完全撤回AddBranch后恢复无加枝类别。Hint/Undo/Restart不触发星级罚则；星级数值/金币/经验阈值没有充分证据，目前不计、不发、不展示假星。旧星槽资源尺寸可留历史，但不是要求新版必须亮三颗。

## 7. 有据待参功能与严格边界

| 功能 | 第一方已有事实 | 本轮有效状态 |
| --- | --- | --- |
| ExtraBranch | SRC-QUEST/SRC-BIRD2/SRC-AZURA有额外枝；SRC-WSWEB明确每关一空容器 | DESIGNED：已进入R07/§8；免费一条N座空枝，非逐格扩高；不得再标为全部参数待证 |
| ReserveGrow | SRC-BOOP备用瓶逐次加高 | PENDING_PARAMETERS：初高/增量/上限/收费未知；不得自写+1座、一天3次等 |
| Collector | SRC-BOOP只可进不可出 | PENDING_PARAMETERS：色绑定/完整后处理未知；不加入正式40 |
| Skins/Coins | SRC-LEAF/SRC-QUEST/SRC-AZURA外观解锁/金币 | RESEARCH_ONLY：有功能依据，无完整价格/奖励表；世界地图先按通关解锁，不伪造金币经济 |
| Stars | 同行有少步高分，具体星阈值未定 | RESEARCH_ONLY；不保旧Undo扣星公式 |

今后若要激活仍待参/研究的功能，先补可定位同行规则/参数证据并评审组合，再改本页、schema、UI和证书；不得让开发AI按名字猜规则。世界地图和主题解锁是用户明确要求，唯一合同见[MAP_PROGRESSION](../MAP_PROGRESSION.md)，与以上未定经济分开。

## 8. R07 ExtraBranch：完整局内加枝合同

**依据和项目决定分层**：SRC-WSWEB是[WaterSort.in浏览器游戏说明](https://www.watersort.in/play)自身的一手文字，Controls写每关加一个空容器、Restart重建同关，Objective写普通容器四单位；本轮没有操作实测，也不借网站品牌证明Galaxy Android应用同样规则。将“同关普通容量的一条空容器”移植为一条N座树枝，N沿R01；不是声称来源有六/八座鸟枝。免费且无广告/金币/IAP是本项目范围配置，与既有免费帮助一致；以下ID、Undo、输入锁和保存边界为E集成协议，不冒充竞品细节。

- **额度和初态**：每次Start/Restart的固定初态只含本关profile规定的P枝（L01为3，02–40为K+2），`extraBranchAdded=false`。点击AddBranch将其变true，在下标P追加唯一`S(P+1)`（例如P5时S6），容量N、Available、tokens空；不生鸟、不换种、不洗牌、不改变原S1..SP位置。当前局不可能有第二条额外枝；退役/搬空不返还额度。
- **入口**：仅Idle/Selected/NoMoves、未Won、未加枝时可用；Selected先取消选中。动画、提交中、暂停/设置、SaveError期间禁止且不缓存；NoMoves模态有同一动作入口，不能穿透调用HUD。点一次即进入Resolving，重复请求拒绝。无货币、广告、确认付费或等待门槛。
- **事务和步数**：AddBranch自身是一笔可撤销、可保存的玩家动作，操作数不增加（操作数只计Transfer）；生成BranchAdded事件，先原子确认再演出。它不触发Hatch/Home，不改变已归巢种。加入后重新检查合法Transfer集合并恢复Idle/NoMoves；未知不称无解。
- **后续规则**：新枝与普通枝同样接收和移出；普通搬空保持Available，N只同种bird完成则Home后Retired；该枝即使Retired仍保留在数组和快照，不可再补一枝。胜利条件仍为全部种类Home且无token，不要求所有空枝退役。
- **Undo/Restart/续局**：历史记录区分Transfer与AddBranch，严格后进先出；先撤回加枝之后的移动，才可撤回AddBranch，撤回时移除当时仍空的末枝并恢复false及原操作数。不可删掉仍承载鸟的枝。Undo回退加枝恢复可用资格，是回退当前路径而非购买第二条；没有累计消耗账本。Restart清历史/末枝、重建同hash初盘并恢复false；退出续局必须保留true及枝内容/退役/历史，不能靠重进刷枝。
- **Hint与成绩**：Hint只搜索当前真实枝集合上的Transfer，不把AddBranch放进隐藏搜索动作，也不代玩家消费帮助。无结果按Unknown或已证明无解区分，可提示手动加枝但不能声称一定救活。AddBranch/Undo/Restart使旧Hint失效。成绩以当前成功路径是否含AddBranch分类；两类各记最少Transfer，不加星/金币/倍率。
- **身份与证明**：辅助协议`assistPolicyVersion=1`以及`extraBranchAdded`必须写入完整快照、精确stateKey、Undo及证书身份，不能只凭基础contentHash复用辅助状态。仅L01初态按§2.1修订，其余39不变；472次主路线Transfer见证继续只证明无AddBranch子集，不证明新动作、保存或UI已实现。
- **显示与验收**：新枝预留位置/图标/触控见UI_SPEC，BranchAdded表现见INTERACTION AN09，保存约束见DATA_SAVE，反例见VALIDATION V19–V26。Original assets remain preserved; new production slices/layout require internal technical and independent visual QA. T21 first-level implementation is authorized; style/QA authority remains UI_SPEC §4.1.20蛋关盲玩公平仍PENDING，额外枝不替代该验收。
