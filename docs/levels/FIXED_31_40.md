> **T30 current-engine scope (2026-11-01):** [GODOT_MIGRATION](../GODOT_MIGRATION.md) owns active implementation, cloud-first testing and gaps. Unity/C#/LitMotion/PlayerPrefs/Canvas/asmdef commands, APIs, dimensions and test results retained below are **legacy evidence/reference, not active Godot instructions or pass claims**. Engine-independent rule/content/asset-approval requirements remain; current engine mappings are in the migration matrix. Other feature expansion is stopped during migration. Native save version1 is isolated from Unity save6 and design-only economy save7.

# 第31–40关：v4固定布局与独立纸面回放

2026-10-31 · FIXED_CANDIDATE_V4 / INDEPENDENT_REPLAY_PASS。**本页已完全替换旧R4内容；旧目标队列、睡眠、条件箱、纹样预告蛋、临时枝任意清空退役及篮/异容量配置均不适用。** 本页不是运行JSON、最优证书或Unity记录。

规则唯一归[RULES](../spec/RULES.md)，来源键见[MECHANIC_EVIDENCE](../MECHANIC_EVIDENCE.md)，课程归[LEVEL_DESIGN](../LEVEL_DESIGN.md)，证据边界见[LEVEL_SIMULATIONS](../LEVEL_SIMULATIONS.md)。

每关N为所有枝统一容量、每种总数N；P=K+2，开局恰两空枝。数组墙→外露端；普通字母是鸟。`?(A)`表示策划固定为A的蛋，**玩家只看见?，不能读出A**；表/回放展示的是全信息分析视图，不证明盲玩公平。所有初始数组、鸟种/形态/位置固定，无运行随机；v4正式初态禁止相邻蛋，未证实的连蛋精确孵化不属于有效玩法。普通搬空不退役；完整同种N群自动归巢并使所在枝Retired；不使用额外枝也能走下列路径。

## 关卡31

错开的同类；世界D 雪松林；N=6，K=5，P=7；所有枝相同容量；每种含蛋共6token；开局两空。

来源：SRC-LEAF/SRC-SIMPLE（端部、接收与移动）；SRC-BIRD2（完成飞走失枝）；容量SRC-BOOP。

| 固定ID | 容量 | 策划完整初始数组 | 玩家初始可见数组 |
| --- | --- | --- | --- |
| S1 | 6 | `A · D2 · A3` | `A · D2 · A3` |
| S2 | 6 | `B · C2 · B3` | `B · C2 · B3` |
| S3 | 6 | `C · E · B2 · C2` | `C · E · B2 · C2` |
| S4 | 6 | `A2 · D4` | `A2 · D4` |
| S5 | 6 | `E · C · E4` | `E · C · E4` |
| S6 | 6 | `∅` | `∅` |
| S7 | 6 | `∅` | `∅` |

本关固定结构：15个初始连续段、0颗问号蛋。主路13步，仅存在性见证。首次归巢在第6步，之后该枝不再参与排序。本见证未发生容量截断，不证明所有路线都不会发生。

### 完整合法见证

| 步 | 玩家动作和自动事件 | 已归巢种类 | 稳定后所有位置（分析视图） |
| --- | --- | --- | --- |
| 1 | Move S2→S6 B3（run=3，Count=3） | 无 | S1=[A · D2 · A3]；S2=[B · C2]；S3=[C · E · B2 · C2]；S4=[A2 · D4]；S5=[E · C · E4]；S6=[B3]；S7=[∅] |
| 2 | Move S3→S2 C2（run=2，Count=2） | 无 | S1=[A · D2 · A3]；S2=[B · C4]；S3=[C · E · B2]；S4=[A2 · D4]；S5=[E · C · E4]；S6=[B3]；S7=[∅] |
| 3 | Move S3→S6 B2（run=2，Count=2） | 无 | S1=[A · D2 · A3]；S2=[B · C4]；S3=[C · E]；S4=[A2 · D4]；S5=[E · C · E4]；S6=[B5]；S7=[∅] |
| 4 | Move S5→S3 E4（run=4，Count=4） | 无 | S1=[A · D2 · A3]；S2=[B · C4]；S3=[C · E5]；S4=[A2 · D4]；S5=[E · C]；S6=[B5]；S7=[∅] |
| 5 | Move S2→S5 C4（run=4，Count=4） | 无 | S1=[A · D2 · A3]；S2=[B]；S3=[C · E5]；S4=[A2 · D4]；S5=[E · C5]；S6=[B5]；S7=[∅] |
| 6 | Move S2→S6 B1（run=1，Count=1）；Home B@S6；Retire S6 | B | S1=[A · D2 · A3]；S2=[∅]；S3=[C · E5]；S4=[A2 · D4]；S5=[E · C5]；S6=[∅][Retired]；S7=[∅] |
| 7 | Move S3→S2 E5（run=5，Count=5） | B | S1=[A · D2 · A3]；S2=[E5]；S3=[C]；S4=[A2 · D4]；S5=[E · C5]；S6=[∅][Retired]；S7=[∅] |
| 8 | Move S5→S3 C5（run=5，Count=5）；Home C@S3；Retire S3 | B,C | S1=[A · D2 · A3]；S2=[E5]；S3=[∅][Retired]；S4=[A2 · D4]；S5=[E]；S6=[∅][Retired]；S7=[∅] |
| 9 | Move S2→S5 E5（run=5，Count=5）；Home E@S5；Retire S5 | B,C,E | S1=[A · D2 · A3]；S2=[∅]；S3=[∅][Retired]；S4=[A2 · D4]；S5=[∅][Retired]；S6=[∅][Retired]；S7=[∅] |
| 10 | Move S4→S2 D4（run=4，Count=4） | B,C,E | S1=[A · D2 · A3]；S2=[D4]；S3=[∅][Retired]；S4=[A2]；S5=[∅][Retired]；S6=[∅][Retired]；S7=[∅] |
| 11 | Move S1→S4 A3（run=3，Count=3） | B,C,E | S1=[A · D2]；S2=[D4]；S3=[∅][Retired]；S4=[A5]；S5=[∅][Retired]；S6=[∅][Retired]；S7=[∅] |
| 12 | Move S1→S2 D2（run=2，Count=2）；Home D@S2；Retire S2 | B,C,D,E | S1=[A]；S2=[∅][Retired]；S3=[∅][Retired]；S4=[A5]；S5=[∅][Retired]；S6=[∅][Retired]；S7=[∅] |
| 13 | Move S1→S4 A1（run=1，Count=1）；Home A@S4；Retire S4 | A,B,C,D,E | S1=[∅]；S2=[∅][Retired]；S3=[∅][Retired]；S4=[∅][Retired]；S5=[∅][Retired]；S6=[∅][Retired]；S7=[∅] |

终态：全部K种归巢、所有token为空、恰K条完成枝Retired，最初及中途普通搬空枝不因空而退休；每种现存token+已收集数量在每步均等于N。全程未使用扩枝、加高、提示、广告或付费。

验收边界：该路径由独立文档模型按完整species状态复算，非项目Core/Unity、非最优、非实际Undo/存档/音画或真人通过。含蛋关还须可见信息公平性验证；无此证据不得宣称玩家无需试错就能知道下一步。

## 关卡32

双蛋回流；世界D 雪松林；N=6，K=5，P=7；所有枝相同容量；每种含蛋共6token；开局两空。

来源：SRC-LEAF/SRC-SIMPLE（端部、接收与移动）；SRC-BIRD2（完成飞走失枝）；容量SRC-BOOP；SRC-QUEST（?蛋）。

| 固定ID | 容量 | 策划完整初始数组 | 玩家初始可见数组 |
| --- | --- | --- | --- |
| S1 | 6 | `A3 · D3` | `A3 · D3` |
| S2 | 6 | `B2 · ?(B) · E · C · D` | `B2 · ? · E · C · D` |
| S3 | 6 | `C · D · B2 · E · C` | `C · D · B2 · E · C` |
| S4 | 6 | `A3 · C2 · E` | `A3 · C2 · E` |
| S5 | 6 | `E · B · ?(D) · C · E2` | `E · B · ? · C · E2` |
| S6 | 6 | `∅` | `∅` |
| S7 | 6 | `∅` | `∅` |

本关固定结构：20个初始连续段、2颗问号蛋。主路17步，仅存在性见证。首次归巢在第8步，之后该枝不再参与排序。本见证未发生容量截断，不证明所有路线都不会发生。孵化见第9,14步；未孵化种类对玩家隐藏。

### 完整合法见证

| 步 | 玩家动作和自动事件 | 已归巢种类 | 稳定后所有位置（分析视图） |
| --- | --- | --- | --- |
| 1 | Move S1→S6 D3（run=3，Count=3） | 无 | S1=[A3]；S2=[B2 · ?(B) · E · C · D]；S3=[C · D · B2 · E · C]；S4=[A3 · C2 · E]；S5=[E · B · ?(D) · C · E2]；S6=[D3]；S7=[∅] |
| 2 | Move S2→S6 D1（run=1，Count=1） | 无 | S1=[A3]；S2=[B2 · ?(B) · E · C]；S3=[C · D · B2 · E · C]；S4=[A3 · C2 · E]；S5=[E · B · ?(D) · C · E2]；S6=[D4]；S7=[∅] |
| 3 | Move S3→S2 C1（run=1，Count=1） | 无 | S1=[A3]；S2=[B2 · ?(B) · E · C2]；S3=[C · D · B2 · E]；S4=[A3 · C2 · E]；S5=[E · B · ?(D) · C · E2]；S6=[D4]；S7=[∅] |
| 4 | Move S4→S3 E1（run=1，Count=1） | 无 | S1=[A3]；S2=[B2 · ?(B) · E · C2]；S3=[C · D · B2 · E2]；S4=[A3 · C2]；S5=[E · B · ?(D) · C · E2]；S6=[D4]；S7=[∅] |
| 5 | Move S2→S7 C2（run=2，Count=2） | 无 | S1=[A3]；S2=[B2 · ?(B) · E]；S3=[C · D · B2 · E2]；S4=[A3 · C2]；S5=[E · B · ?(D) · C · E2]；S6=[D4]；S7=[C2] |
| 6 | Move S3→S2 E2（run=2，Count=2） | 无 | S1=[A3]；S2=[B2 · ?(B) · E3]；S3=[C · D · B2]；S4=[A3 · C2]；S5=[E · B · ?(D) · C · E2]；S6=[D4]；S7=[C2] |
| 7 | Move S4→S7 C2（run=2，Count=2） | 无 | S1=[A3]；S2=[B2 · ?(B) · E3]；S3=[C · D · B2]；S4=[A3]；S5=[E · B · ?(D) · C · E2]；S6=[D4]；S7=[C4] |
| 8 | Move S1→S4 A3（run=3，Count=3）；Home A@S4；Retire S4 | A | S1=[∅]；S2=[B2 · ?(B) · E3]；S3=[C · D · B2]；S4=[∅][Retired]；S5=[E · B · ?(D) · C · E2]；S6=[D4]；S7=[C4] |
| 9 | Move S2→S1 E3（run=3，Count=3）；Hatch S2 → B | A | S1=[E3]；S2=[B3]；S3=[C · D · B2]；S4=[∅][Retired]；S5=[E · B · ?(D) · C · E2]；S6=[D4]；S7=[C4] |
| 10 | Move S3→S2 B2（run=2，Count=2） | A | S1=[E3]；S2=[B5]；S3=[C · D]；S4=[∅][Retired]；S5=[E · B · ?(D) · C · E2]；S6=[D4]；S7=[C4] |
| 11 | Move S3→S6 D1（run=1，Count=1） | A | S1=[E3]；S2=[B5]；S3=[C]；S4=[∅][Retired]；S5=[E · B · ?(D) · C · E2]；S6=[D5]；S7=[C4] |
| 12 | Move S3→S7 C1（run=1，Count=1） | A | S1=[E3]；S2=[B5]；S3=[∅]；S4=[∅][Retired]；S5=[E · B · ?(D) · C · E2]；S6=[D5]；S7=[C5] |
| 13 | Move S5→S1 E2（run=2，Count=2） | A | S1=[E5]；S2=[B5]；S3=[∅]；S4=[∅][Retired]；S5=[E · B · ?(D) · C]；S6=[D5]；S7=[C5] |
| 14 | Move S5→S7 C1（run=1，Count=1）；Hatch S5 → D；Home C@S7；Retire S7 | A,C | S1=[E5]；S2=[B5]；S3=[∅]；S4=[∅][Retired]；S5=[E · B · D]；S6=[D5]；S7=[∅][Retired] |
| 15 | Move S5→S6 D1（run=1，Count=1）；Home D@S6；Retire S6 | A,C,D | S1=[E5]；S2=[B5]；S3=[∅]；S4=[∅][Retired]；S5=[E · B]；S6=[∅][Retired]；S7=[∅][Retired] |
| 16 | Move S5→S2 B1（run=1，Count=1）；Home B@S2；Retire S2 | A,B,C,D | S1=[E5]；S2=[∅][Retired]；S3=[∅]；S4=[∅][Retired]；S5=[E]；S6=[∅][Retired]；S7=[∅][Retired] |
| 17 | Move S1→S5 E5（run=5，Count=5）；Home E@S5；Retire S5 | A,B,C,D,E | S1=[∅]；S2=[∅][Retired]；S3=[∅]；S4=[∅][Retired]；S5=[∅][Retired]；S6=[∅][Retired]；S7=[∅][Retired] |

终态：全部K种归巢、所有token为空、恰K条完成枝Retired，最初及中途普通搬空枝不因空而退休；每种现存token+已收集数量在每步均等于N。全程未使用扩枝、加高、提示、广告或付费。

验收边界：该路径由独立文档模型按完整species状态复算，非项目Core/Unity、非最优、非实际Undo/存档/音画或真人通过。含蛋关还须可见信息公平性验证；无此证据不得宣称玩家无需试错就能知道下一步。

## 关卡33

八座首次汇合；世界D 雪松林；N=8，K=5，P=7；所有枝相同容量；每种含蛋共8token；开局两空。

来源：SRC-LEAF/SRC-SIMPLE（端部、接收与移动）；SRC-BIRD2（完成飞走失枝）；容量SRC-BOOP。

| 固定ID | 容量 | 策划完整初始数组 | 玩家初始可见数组 |
| --- | --- | --- | --- |
| S1 | 8 | `A5 · B · C2` | `A5 · B · C2` |
| S2 | 8 | `B · D · B2 · E · A2 · C` | `B · D · B2 · E · A2 · C` |
| S3 | 8 | `C2 · B2 · E3 · C` | `C2 · B2 · E3 · C` |
| S4 | 8 | `D · B · C2 · D3 · B` | `D · B · C2 · D3 · B` |
| S5 | 8 | `E4 · D3 · A` | `E4 · D3 · A` |
| S6 | 8 | `∅` | `∅` |
| S7 | 8 | `∅` | `∅` |

本关固定结构：21个初始连续段、0颗问号蛋。主路20步，仅存在性见证。首次归巢在第7步，之后该枝不再参与排序。本见证未发生容量截断，不证明所有路线都不会发生。

### 完整合法见证

| 步 | 玩家动作和自动事件 | 已归巢种类 | 稳定后所有位置（分析视图） |
| --- | --- | --- | --- |
| 1 | Move S1→S6 C2（run=2，Count=2） | 无 | S1=[A5 · B]；S2=[B · D · B2 · E · A2 · C]；S3=[C2 · B2 · E3 · C]；S4=[D · B · C2 · D3 · B]；S5=[E4 · D3 · A]；S6=[C2]；S7=[∅] |
| 2 | Move S2→S6 C1（run=1，Count=1） | 无 | S1=[A5 · B]；S2=[B · D · B2 · E · A2]；S3=[C2 · B2 · E3 · C]；S4=[D · B · C2 · D3 · B]；S5=[E4 · D3 · A]；S6=[C3]；S7=[∅] |
| 3 | Move S3→S6 C1（run=1，Count=1） | 无 | S1=[A5 · B]；S2=[B · D · B2 · E · A2]；S3=[C2 · B2 · E3]；S4=[D · B · C2 · D3 · B]；S5=[E4 · D3 · A]；S6=[C4]；S7=[∅] |
| 4 | Move S4→S1 B1（run=1，Count=1） | 无 | S1=[A5 · B2]；S2=[B · D · B2 · E · A2]；S3=[C2 · B2 · E3]；S4=[D · B · C2 · D3]；S5=[E4 · D3 · A]；S6=[C4]；S7=[∅] |
| 5 | Move S5→S2 A1（run=1，Count=1） | 无 | S1=[A5 · B2]；S2=[B · D · B2 · E · A3]；S3=[C2 · B2 · E3]；S4=[D · B · C2 · D3]；S5=[E4 · D3]；S6=[C4]；S7=[∅] |
| 6 | Move S1→S7 B2（run=2，Count=2） | 无 | S1=[A5]；S2=[B · D · B2 · E · A3]；S3=[C2 · B2 · E3]；S4=[D · B · C2 · D3]；S5=[E4 · D3]；S6=[C4]；S7=[B2] |
| 7 | Move S2→S1 A3（run=3，Count=3）；Home A@S1；Retire S1 | A | S1=[∅][Retired]；S2=[B · D · B2 · E]；S3=[C2 · B2 · E3]；S4=[D · B · C2 · D3]；S5=[E4 · D3]；S6=[C4]；S7=[B2] |
| 8 | Move S2→S3 E1（run=1，Count=1） | A | S1=[∅][Retired]；S2=[B · D · B2]；S3=[C2 · B2 · E4]；S4=[D · B · C2 · D3]；S5=[E4 · D3]；S6=[C4]；S7=[B2] |
| 9 | Move S2→S7 B2（run=2，Count=2） | A | S1=[∅][Retired]；S2=[B · D]；S3=[C2 · B2 · E4]；S4=[D · B · C2 · D3]；S5=[E4 · D3]；S6=[C4]；S7=[B4] |
| 10 | Move S2→S4 D1（run=1，Count=1） | A | S1=[∅][Retired]；S2=[B]；S3=[C2 · B2 · E4]；S4=[D · B · C2 · D4]；S5=[E4 · D3]；S6=[C4]；S7=[B4] |
| 11 | Move S2→S7 B1（run=1，Count=1） | A | S1=[∅][Retired]；S2=[∅]；S3=[C2 · B2 · E4]；S4=[D · B · C2 · D4]；S5=[E4 · D3]；S6=[C4]；S7=[B5] |
| 12 | Move S3→S2 E4（run=4，Count=4） | A | S1=[∅][Retired]；S2=[E4]；S3=[C2 · B2]；S4=[D · B · C2 · D4]；S5=[E4 · D3]；S6=[C4]；S7=[B5] |
| 13 | Move S3→S7 B2（run=2，Count=2） | A | S1=[∅][Retired]；S2=[E4]；S3=[C2]；S4=[D · B · C2 · D4]；S5=[E4 · D3]；S6=[C4]；S7=[B7] |
| 14 | Move S3→S6 C2（run=2，Count=2） | A | S1=[∅][Retired]；S2=[E4]；S3=[∅]；S4=[D · B · C2 · D4]；S5=[E4 · D3]；S6=[C6]；S7=[B7] |
| 15 | Move S4→S3 D4（run=4，Count=4） | A | S1=[∅][Retired]；S2=[E4]；S3=[D4]；S4=[D · B · C2]；S5=[E4 · D3]；S6=[C6]；S7=[B7] |
| 16 | Move S4→S6 C2（run=2，Count=2）；Home C@S6；Retire S6 | A,C | S1=[∅][Retired]；S2=[E4]；S3=[D4]；S4=[D · B]；S5=[E4 · D3]；S6=[∅][Retired]；S7=[B7] |
| 17 | Move S4→S7 B1（run=1，Count=1）；Home B@S7；Retire S7 | A,B,C | S1=[∅][Retired]；S2=[E4]；S3=[D4]；S4=[D]；S5=[E4 · D3]；S6=[∅][Retired]；S7=[∅][Retired] |
| 18 | Move S3→S4 D4（run=4，Count=4） | A,B,C | S1=[∅][Retired]；S2=[E4]；S3=[∅]；S4=[D5]；S5=[E4 · D3]；S6=[∅][Retired]；S7=[∅][Retired] |
| 19 | Move S5→S4 D3（run=3，Count=3）；Home D@S4；Retire S4 | A,B,C,D | S1=[∅][Retired]；S2=[E4]；S3=[∅]；S4=[∅][Retired]；S5=[E4]；S6=[∅][Retired]；S7=[∅][Retired] |
| 20 | Move S2→S5 E4（run=4，Count=4）；Home E@S5；Retire S5 | A,B,C,D,E | S1=[∅][Retired]；S2=[∅]；S3=[∅]；S4=[∅][Retired]；S5=[∅][Retired]；S6=[∅][Retired]；S7=[∅][Retired] |

终态：全部K种归巢、所有token为空、恰K条完成枝Retired，最初及中途普通搬空枝不因空而退休；每种现存token+已收集数量在每步均等于N。全程未使用扩枝、加高、提示、广告或付费。

验收边界：该路径由独立文档模型按完整species状态复算，非项目Core/Unity、非最优、非实际Undo/存档/音画或真人通过。含蛋关还须可见信息公平性验证；无此证据不得宣称玩家无需试错就能知道下一步。

## 关卡34

八座单蛋挑战；世界D 雪松林；N=8，K=5，P=7；所有枝相同容量；每种含蛋共8token；开局两空。

来源：SRC-LEAF/SRC-SIMPLE（端部、接收与移动）；SRC-BIRD2（完成飞走失枝）；容量SRC-BOOP；SRC-QUEST（?蛋）。

| 固定ID | 容量 | 策划完整初始数组 | 玩家初始可见数组 |
| --- | --- | --- | --- |
| S1 | 8 | `A · E2 · D2 · E · A2` | `A · E2 · D2 · E · A2` |
| S2 | 8 | `B4 · ?(B) · B · D · B` | `B4 · ? · B · D · B` |
| S3 | 8 | `C4 · D · E · C2` | `C4 · D · E · C2` |
| S4 | 8 | `A · D3 · E3 · A` | `A · D3 · E3 · A` |
| S5 | 8 | `D · A · E · B · A2 · C2` | `D · A · E · B · A2 · C2` |
| S6 | 8 | `∅` | `∅` |
| S7 | 8 | `∅` | `∅` |

本关固定结构：24个初始连续段、1颗问号蛋。主路23步，仅存在性见证。首次归巢在第18步，之后该枝不再参与排序。本见证未发生容量截断，不证明所有路线都不会发生。孵化见第12步；未孵化种类对玩家隐藏。

### 完整合法见证

| 步 | 玩家动作和自动事件 | 已归巢种类 | 稳定后所有位置（分析视图） |
| --- | --- | --- | --- |
| 1 | Move S1→S6 A2（run=2，Count=2） | 无 | S1=[A · E2 · D2 · E]；S2=[B4 · ?(B) · B · D · B]；S3=[C4 · D · E · C2]；S4=[A · D3 · E3 · A]；S5=[D · A · E · B · A2 · C2]；S6=[A2]；S7=[∅] |
| 2 | Move S4→S6 A1（run=1，Count=1） | 无 | S1=[A · E2 · D2 · E]；S2=[B4 · ?(B) · B · D · B]；S3=[C4 · D · E · C2]；S4=[A · D3 · E3]；S5=[D · A · E · B · A2 · C2]；S6=[A3]；S7=[∅] |
| 3 | Move S1→S4 E1（run=1，Count=1） | 无 | S1=[A · E2 · D2]；S2=[B4 · ?(B) · B · D · B]；S3=[C4 · D · E · C2]；S4=[A · D3 · E4]；S5=[D · A · E · B · A2 · C2]；S6=[A3]；S7=[∅] |
| 4 | Move S1→S7 D2（run=2，Count=2） | 无 | S1=[A · E2]；S2=[B4 · ?(B) · B · D · B]；S3=[C4 · D · E · C2]；S4=[A · D3 · E4]；S5=[D · A · E · B · A2 · C2]；S6=[A3]；S7=[D2] |
| 5 | Move S4→S1 E4（run=4，Count=4） | 无 | S1=[A · E6]；S2=[B4 · ?(B) · B · D · B]；S3=[C4 · D · E · C2]；S4=[A · D3]；S5=[D · A · E · B · A2 · C2]；S6=[A3]；S7=[D2] |
| 6 | Move S4→S7 D3（run=3，Count=3） | 无 | S1=[A · E6]；S2=[B4 · ?(B) · B · D · B]；S3=[C4 · D · E · C2]；S4=[A]；S5=[D · A · E · B · A2 · C2]；S6=[A3]；S7=[D5] |
| 7 | Move S4→S6 A1（run=1，Count=1） | 无 | S1=[A · E6]；S2=[B4 · ?(B) · B · D · B]；S3=[C4 · D · E · C2]；S4=[∅]；S5=[D · A · E · B · A2 · C2]；S6=[A4]；S7=[D5] |
| 8 | Move S1→S4 E6（run=6，Count=6） | 无 | S1=[A]；S2=[B4 · ?(B) · B · D · B]；S3=[C4 · D · E · C2]；S4=[E6]；S5=[D · A · E · B · A2 · C2]；S6=[A4]；S7=[D5] |
| 9 | Move S1→S6 A1（run=1，Count=1） | 无 | S1=[∅]；S2=[B4 · ?(B) · B · D · B]；S3=[C4 · D · E · C2]；S4=[E6]；S5=[D · A · E · B · A2 · C2]；S6=[A5]；S7=[D5] |
| 10 | Move S2→S1 B1（run=1，Count=1） | 无 | S1=[B]；S2=[B4 · ?(B) · B · D]；S3=[C4 · D · E · C2]；S4=[E6]；S5=[D · A · E · B · A2 · C2]；S6=[A5]；S7=[D5] |
| 11 | Move S2→S7 D1（run=1，Count=1） | 无 | S1=[B]；S2=[B4 · ?(B) · B]；S3=[C4 · D · E · C2]；S4=[E6]；S5=[D · A · E · B · A2 · C2]；S6=[A5]；S7=[D6] |
| 12 | Move S2→S1 B1（run=1，Count=1）；Hatch S2 → B | 无 | S1=[B2]；S2=[B5]；S3=[C4 · D · E · C2]；S4=[E6]；S5=[D · A · E · B · A2 · C2]；S6=[A5]；S7=[D6] |
| 13 | Move S1→S2 B2（run=2，Count=2） | 无 | S1=[∅]；S2=[B7]；S3=[C4 · D · E · C2]；S4=[E6]；S5=[D · A · E · B · A2 · C2]；S6=[A5]；S7=[D6] |
| 14 | Move S3→S1 C2（run=2，Count=2） | 无 | S1=[C2]；S2=[B7]；S3=[C4 · D · E]；S4=[E6]；S5=[D · A · E · B · A2 · C2]；S6=[A5]；S7=[D6] |
| 15 | Move S3→S4 E1（run=1，Count=1） | 无 | S1=[C2]；S2=[B7]；S3=[C4 · D]；S4=[E7]；S5=[D · A · E · B · A2 · C2]；S6=[A5]；S7=[D6] |
| 16 | Move S3→S7 D1（run=1，Count=1） | 无 | S1=[C2]；S2=[B7]；S3=[C4]；S4=[E7]；S5=[D · A · E · B · A2 · C2]；S6=[A5]；S7=[D7] |
| 17 | Move S1→S3 C2（run=2，Count=2） | 无 | S1=[∅]；S2=[B7]；S3=[C6]；S4=[E7]；S5=[D · A · E · B · A2 · C2]；S6=[A5]；S7=[D7] |
| 18 | Move S5→S3 C2（run=2，Count=2）；Home C@S3；Retire S3 | C | S1=[∅]；S2=[B7]；S3=[∅][Retired]；S4=[E7]；S5=[D · A · E · B · A2]；S6=[A5]；S7=[D7] |
| 19 | Move S5→S6 A2（run=2，Count=2） | C | S1=[∅]；S2=[B7]；S3=[∅][Retired]；S4=[E7]；S5=[D · A · E · B]；S6=[A7]；S7=[D7] |
| 20 | Move S5→S2 B1（run=1，Count=1）；Home B@S2；Retire S2 | B,C | S1=[∅]；S2=[∅][Retired]；S3=[∅][Retired]；S4=[E7]；S5=[D · A · E]；S6=[A7]；S7=[D7] |
| 21 | Move S5→S4 E1（run=1，Count=1）；Home E@S4；Retire S4 | B,C,E | S1=[∅]；S2=[∅][Retired]；S3=[∅][Retired]；S4=[∅][Retired]；S5=[D · A]；S6=[A7]；S7=[D7] |
| 22 | Move S5→S6 A1（run=1，Count=1）；Home A@S6；Retire S6 | A,B,C,E | S1=[∅]；S2=[∅][Retired]；S3=[∅][Retired]；S4=[∅][Retired]；S5=[D]；S6=[∅][Retired]；S7=[D7] |
| 23 | Move S5→S7 D1（run=1，Count=1）；Home D@S7；Retire S7 | A,B,C,D,E | S1=[∅]；S2=[∅][Retired]；S3=[∅][Retired]；S4=[∅][Retired]；S5=[∅]；S6=[∅][Retired]；S7=[∅][Retired] |

终态：全部K种归巢、所有token为空、恰K条完成枝Retired，最初及中途普通搬空枝不因空而退休；每种现存token+已收集数量在每步均等于N。全程未使用扩枝、加高、提示、广告或付费。

验收边界：该路径由独立文档模型按完整species状态复算，非项目Core/Unity、非最优、非实际Undo/存档/音画或真人通过。含蛋关还须可见信息公平性验证；无此证据不得宣称玩家无需试错就能知道下一步。

## 关卡35

雪松林歇脚；世界D 雪松林；N=6，K=4，P=6；所有枝相同容量；每种含蛋共6token；开局两空。

来源：SRC-LEAF/SRC-SIMPLE（端部、接收与移动）；SRC-BIRD2（完成飞走失枝）；容量SRC-BOOP。

| 固定ID | 容量 | 策划完整初始数组 | 玩家初始可见数组 |
| --- | --- | --- | --- |
| S1 | 6 | `D3 · A3` | `D3 · A3` |
| S2 | 6 | `B · C · A3 · B` | `B · C · A3 · B` |
| S3 | 6 | `C3 · B · C2` | `C3 · B · C2` |
| S4 | 6 | `D · B3 · D2` | `D · B3 · D2` |
| S5 | 6 | `∅` | `∅` |
| S6 | 6 | `∅` | `∅` |

本关固定结构：12个初始连续段、0颗问号蛋。主路9步，仅存在性见证。首次归巢在第4步，之后该枝不再参与排序。本见证未发生容量截断，不证明所有路线都不会发生。

### 完整合法见证

| 步 | 玩家动作和自动事件 | 已归巢种类 | 稳定后所有位置（分析视图） |
| --- | --- | --- | --- |
| 1 | Move S1→S5 A3（run=3，Count=3） | 无 | S1=[D3]；S2=[B · C · A3 · B]；S3=[C3 · B · C2]；S4=[D · B3 · D2]；S5=[A3]；S6=[∅] |
| 2 | Move S4→S1 D2（run=2，Count=2） | 无 | S1=[D5]；S2=[B · C · A3 · B]；S3=[C3 · B · C2]；S4=[D · B3]；S5=[A3]；S6=[∅] |
| 3 | Move S2→S4 B1（run=1，Count=1） | 无 | S1=[D5]；S2=[B · C · A3]；S3=[C3 · B · C2]；S4=[D · B4]；S5=[A3]；S6=[∅] |
| 4 | Move S2→S5 A3（run=3，Count=3）；Home A@S5；Retire S5 | A | S1=[D5]；S2=[B · C]；S3=[C3 · B · C2]；S4=[D · B4]；S5=[∅][Retired]；S6=[∅] |
| 5 | Move S3→S2 C2（run=2，Count=2） | A | S1=[D5]；S2=[B · C3]；S3=[C3 · B]；S4=[D · B4]；S5=[∅][Retired]；S6=[∅] |
| 6 | Move S3→S4 B1（run=1，Count=1） | A | S1=[D5]；S2=[B · C3]；S3=[C3]；S4=[D · B5]；S5=[∅][Retired]；S6=[∅] |
| 7 | Move S2→S3 C3（run=3，Count=3）；Home C@S3；Retire S3 | A,C | S1=[D5]；S2=[B]；S3=[∅][Retired]；S4=[D · B5]；S5=[∅][Retired]；S6=[∅] |
| 8 | Move S4→S2 B5（run=5，Count=5）；Home B@S2；Retire S2 | A,B,C | S1=[D5]；S2=[∅][Retired]；S3=[∅][Retired]；S4=[D]；S5=[∅][Retired]；S6=[∅] |
| 9 | Move S1→S4 D5（run=5，Count=5）；Home D@S4；Retire S4 | A,B,C,D | S1=[∅]；S2=[∅][Retired]；S3=[∅][Retired]；S4=[∅][Retired]；S5=[∅][Retired]；S6=[∅] |

终态：全部K种归巢、所有token为空、恰K条完成枝Retired，最初及中途普通搬空枝不因空而退休；每种现存token+已收集数量在每步均等于N。全程未使用扩枝、加高、提示、广告或付费。

验收边界：该路径由独立文档模型按完整species状态复算，非项目Core/Unity、非最优、非实际Undo/存档/音画或真人通过。含蛋关还须可见信息公平性验证；无此证据不得宣称玩家无需试错就能知道下一步。

## 关卡36

八座双揭示；世界D 雪松林；N=8，K=5，P=7；所有枝相同容量；每种含蛋共8token；开局两空。

来源：SRC-LEAF/SRC-SIMPLE（端部、接收与移动）；SRC-BIRD2（完成飞走失枝）；容量SRC-BOOP；SRC-QUEST（?蛋）。

| 固定ID | 容量 | 策划完整初始数组 | 玩家初始可见数组 |
| --- | --- | --- | --- |
| S1 | 8 | `A6 · ?(C) · A` | `A6 · ? · A` |
| S2 | 8 | `B4 · E3 · B` | `B4 · E3 · B` |
| S3 | 8 | `C · A · C2 · D3 · E` | `C · A · C2 · D3 · E` |
| S4 | 8 | `C3 · D5` | `C3 · D5` |
| S5 | 8 | `E2 · B · ?(B) · B · E2 · C` | `E2 · B · ? · B · E2 · C` |
| S6 | 8 | `∅` | `∅` |
| S7 | 8 | `∅` | `∅` |

本关固定结构：19个初始连续段、2颗问号蛋。主路18步，仅存在性见证。首次归巢在第8步，之后该枝不再参与排序。本见证未发生容量截断，不证明所有路线都不会发生。孵化见第1,13步；未孵化种类对玩家隐藏。

### 完整合法见证

| 步 | 玩家动作和自动事件 | 已归巢种类 | 稳定后所有位置（分析视图） |
| --- | --- | --- | --- |
| 1 | Move S1→S6 A1（run=1，Count=1）；Hatch S1 → C | 无 | S1=[A6 · C]；S2=[B4 · E3 · B]；S3=[C · A · C2 · D3 · E]；S4=[C3 · D5]；S5=[E2 · B · ?(B) · B · E2 · C]；S6=[A]；S7=[∅] |
| 2 | Move S5→S1 C1（run=1，Count=1） | 无 | S1=[A6 · C2]；S2=[B4 · E3 · B]；S3=[C · A · C2 · D3 · E]；S4=[C3 · D5]；S5=[E2 · B · ?(B) · B · E2]；S6=[A]；S7=[∅] |
| 3 | Move S3→S5 E1（run=1，Count=1） | 无 | S1=[A6 · C2]；S2=[B4 · E3 · B]；S3=[C · A · C2 · D3]；S4=[C3 · D5]；S5=[E2 · B · ?(B) · B · E3]；S6=[A]；S7=[∅] |
| 4 | Move S1→S7 C2（run=2，Count=2） | 无 | S1=[A6]；S2=[B4 · E3 · B]；S3=[C · A · C2 · D3]；S4=[C3 · D5]；S5=[E2 · B · ?(B) · B · E3]；S6=[A]；S7=[C2] |
| 5 | Move S1→S6 A6（run=6，Count=6） | 无 | S1=[∅]；S2=[B4 · E3 · B]；S3=[C · A · C2 · D3]；S4=[C3 · D5]；S5=[E2 · B · ?(B) · B · E3]；S6=[A7]；S7=[C2] |
| 6 | Move S3→S1 D3（run=3，Count=3） | 无 | S1=[D3]；S2=[B4 · E3 · B]；S3=[C · A · C2]；S4=[C3 · D5]；S5=[E2 · B · ?(B) · B · E3]；S6=[A7]；S7=[C2] |
| 7 | Move S3→S7 C2（run=2，Count=2） | 无 | S1=[D3]；S2=[B4 · E3 · B]；S3=[C · A]；S4=[C3 · D5]；S5=[E2 · B · ?(B) · B · E3]；S6=[A7]；S7=[C4] |
| 8 | Move S3→S6 A1（run=1，Count=1）；Home A@S6；Retire S6 | A | S1=[D3]；S2=[B4 · E3 · B]；S3=[C]；S4=[C3 · D5]；S5=[E2 · B · ?(B) · B · E3]；S6=[∅][Retired]；S7=[C4] |
| 9 | Move S3→S7 C1（run=1，Count=1） | A | S1=[D3]；S2=[B4 · E3 · B]；S3=[∅]；S4=[C3 · D5]；S5=[E2 · B · ?(B) · B · E3]；S6=[∅][Retired]；S7=[C5] |
| 10 | Move S4→S1 D5（run=5，Count=5）；Home D@S1；Retire S1 | A,D | S1=[∅][Retired]；S2=[B4 · E3 · B]；S3=[∅]；S4=[C3]；S5=[E2 · B · ?(B) · B · E3]；S6=[∅][Retired]；S7=[C5] |
| 11 | Move S4→S7 C3（run=3，Count=3）；Home C@S7；Retire S7 | A,C,D | S1=[∅][Retired]；S2=[B4 · E3 · B]；S3=[∅]；S4=[∅]；S5=[E2 · B · ?(B) · B · E3]；S6=[∅][Retired]；S7=[∅][Retired] |
| 12 | Move S5→S3 E3（run=3，Count=3） | A,C,D | S1=[∅][Retired]；S2=[B4 · E3 · B]；S3=[E3]；S4=[∅]；S5=[E2 · B · ?(B) · B]；S6=[∅][Retired]；S7=[∅][Retired] |
| 13 | Move S5→S4 B1（run=1，Count=1）；Hatch S5 → B | A,C,D | S1=[∅][Retired]；S2=[B4 · E3 · B]；S3=[E3]；S4=[B]；S5=[E2 · B2]；S6=[∅][Retired]；S7=[∅][Retired] |
| 14 | Move S2→S4 B1（run=1，Count=1） | A,C,D | S1=[∅][Retired]；S2=[B4 · E3]；S3=[E3]；S4=[B2]；S5=[E2 · B2]；S6=[∅][Retired]；S7=[∅][Retired] |
| 15 | Move S2→S3 E3（run=3，Count=3） | A,C,D | S1=[∅][Retired]；S2=[B4]；S3=[E6]；S4=[B2]；S5=[E2 · B2]；S6=[∅][Retired]；S7=[∅][Retired] |
| 16 | Move S2→S4 B4（run=4，Count=4） | A,C,D | S1=[∅][Retired]；S2=[∅]；S3=[E6]；S4=[B6]；S5=[E2 · B2]；S6=[∅][Retired]；S7=[∅][Retired] |
| 17 | Move S5→S4 B2（run=2，Count=2）；Home B@S4；Retire S4 | A,B,C,D | S1=[∅][Retired]；S2=[∅]；S3=[E6]；S4=[∅][Retired]；S5=[E2]；S6=[∅][Retired]；S7=[∅][Retired] |
| 18 | Move S3→S5 E6（run=6，Count=6）；Home E@S5；Retire S5 | A,B,C,D,E | S1=[∅][Retired]；S2=[∅]；S3=[∅]；S4=[∅][Retired]；S5=[∅][Retired]；S6=[∅][Retired]；S7=[∅][Retired] |

终态：全部K种归巢、所有token为空、恰K条完成枝Retired，最初及中途普通搬空枝不因空而退休；每种现存token+已收集数量在每步均等于N。全程未使用扩枝、加高、提示、广告或付费。

验收边界：该路径由独立文档模型按完整species状态复算，非项目Core/Unity、非最优、非实际Undo/存档/音画或真人通过。含蛋关还须可见信息公平性验证；无此证据不得宣称玩家无需试错就能知道下一步。

## 关卡37

六种同场；世界D 雪松林；N=8，K=6，P=8；所有枝相同容量；每种含蛋共8token；开局两空。

来源：SRC-LEAF/SRC-SIMPLE（端部、接收与移动）；SRC-BIRD2（完成飞走失枝）；容量SRC-BOOP。

| 固定ID | 容量 | 策划完整初始数组 | 玩家初始可见数组 |
| --- | --- | --- | --- |
| S1 | 8 | `A · F2 · A3 · E2` | `A · F2 · A3 · E2` |
| S2 | 8 | `B6 · D2` | `B6 · D2` |
| S3 | 8 | `C5 · D2 · C` | `C5 · D2 · C` |
| S4 | 8 | `B2 · A2 · C2 · D2` | `B2 · A2 · C2 · D2` |
| S5 | 8 | `D2 · E6` | `D2 · E6` |
| S6 | 8 | `A2 · F6` | `A2 · F6` |
| S7 | 8 | `∅` | `∅` |
| S8 | 8 | `∅` | `∅` |

本关固定结构：17个初始连续段、0颗问号蛋。主路13步，仅存在性见证。首次归巢在第2步，之后该枝不再参与排序。本见证未发生容量截断，不证明所有路线都不会发生。

### 完整合法见证

| 步 | 玩家动作和自动事件 | 已归巢种类 | 稳定后所有位置（分析视图） |
| --- | --- | --- | --- |
| 1 | Move S1→S7 E2（run=2，Count=2） | 无 | S1=[A · F2 · A3]；S2=[B6 · D2]；S3=[C5 · D2 · C]；S4=[B2 · A2 · C2 · D2]；S5=[D2 · E6]；S6=[A2 · F6]；S7=[E2]；S8=[∅] |
| 2 | Move S5→S7 E6（run=6，Count=6）；Home E@S7；Retire S7 | E | S1=[A · F2 · A3]；S2=[B6 · D2]；S3=[C5 · D2 · C]；S4=[B2 · A2 · C2 · D2]；S5=[D2]；S6=[A2 · F6]；S7=[∅][Retired]；S8=[∅] |
| 3 | Move S2→S5 D2（run=2，Count=2） | E | S1=[A · F2 · A3]；S2=[B6]；S3=[C5 · D2 · C]；S4=[B2 · A2 · C2 · D2]；S5=[D4]；S6=[A2 · F6]；S7=[∅][Retired]；S8=[∅] |
| 4 | Move S4→S5 D2（run=2，Count=2） | E | S1=[A · F2 · A3]；S2=[B6]；S3=[C5 · D2 · C]；S4=[B2 · A2 · C2]；S5=[D6]；S6=[A2 · F6]；S7=[∅][Retired]；S8=[∅] |
| 5 | Move S3→S4 C1（run=1，Count=1） | E | S1=[A · F2 · A3]；S2=[B6]；S3=[C5 · D2]；S4=[B2 · A2 · C3]；S5=[D6]；S6=[A2 · F6]；S7=[∅][Retired]；S8=[∅] |
| 6 | Move S3→S5 D2（run=2，Count=2）；Home D@S5；Retire S5 | D,E | S1=[A · F2 · A3]；S2=[B6]；S3=[C5]；S4=[B2 · A2 · C3]；S5=[∅][Retired]；S6=[A2 · F6]；S7=[∅][Retired]；S8=[∅] |
| 7 | Move S4→S3 C3（run=3，Count=3）；Home C@S3；Retire S3 | C,D,E | S1=[A · F2 · A3]；S2=[B6]；S3=[∅][Retired]；S4=[B2 · A2]；S5=[∅][Retired]；S6=[A2 · F6]；S7=[∅][Retired]；S8=[∅] |
| 8 | Move S4→S1 A2（run=2，Count=2） | C,D,E | S1=[A · F2 · A5]；S2=[B6]；S3=[∅][Retired]；S4=[B2]；S5=[∅][Retired]；S6=[A2 · F6]；S7=[∅][Retired]；S8=[∅] |
| 9 | Move S2→S4 B6（run=6，Count=6）；Home B@S4；Retire S4 | B,C,D,E | S1=[A · F2 · A5]；S2=[∅]；S3=[∅][Retired]；S4=[∅][Retired]；S5=[∅][Retired]；S6=[A2 · F6]；S7=[∅][Retired]；S8=[∅] |
| 10 | Move S6→S2 F6（run=6，Count=6） | B,C,D,E | S1=[A · F2 · A5]；S2=[F6]；S3=[∅][Retired]；S4=[∅][Retired]；S5=[∅][Retired]；S6=[A2]；S7=[∅][Retired]；S8=[∅] |
| 11 | Move S1→S6 A5（run=5，Count=5） | B,C,D,E | S1=[A · F2]；S2=[F6]；S3=[∅][Retired]；S4=[∅][Retired]；S5=[∅][Retired]；S6=[A7]；S7=[∅][Retired]；S8=[∅] |
| 12 | Move S1→S2 F2（run=2，Count=2）；Home F@S2；Retire S2 | B,C,D,E,F | S1=[A]；S2=[∅][Retired]；S3=[∅][Retired]；S4=[∅][Retired]；S5=[∅][Retired]；S6=[A7]；S7=[∅][Retired]；S8=[∅] |
| 13 | Move S1→S6 A1（run=1，Count=1）；Home A@S6；Retire S6 | A,B,C,D,E,F | S1=[∅]；S2=[∅][Retired]；S3=[∅][Retired]；S4=[∅][Retired]；S5=[∅][Retired]；S6=[∅][Retired]；S7=[∅][Retired]；S8=[∅] |

终态：全部K种归巢、所有token为空、恰K条完成枝Retired，最初及中途普通搬空枝不因空而退休；每种现存token+已收集数量在每步均等于N。全程未使用扩枝、加高、提示、广告或付费。

验收边界：该路径由独立文档模型按完整species状态复算，非项目Core/Unity、非最优、非实际Undo/存档/音画或真人通过。含蛋关还须可见信息公平性验证；无此证据不得宣称玩家无需试错就能知道下一步。

## 关卡38

长群深层练习；世界D 雪松林；N=6，K=6，P=8；所有枝相同容量；每种含蛋共6token；开局两空。

来源：SRC-LEAF/SRC-SIMPLE（端部、接收与移动）；SRC-BIRD2（完成飞走失枝）；容量SRC-BOOP。

| 固定ID | 容量 | 策划完整初始数组 | 玩家初始可见数组 |
| --- | --- | --- | --- |
| S1 | 6 | `A3 · F · A · E` | `A3 · F · A · E` |
| S2 | 6 | `B2 · E · F2 · B` | `B2 · E · F2 · B` |
| S3 | 6 | `C4 · E · C` | `C4 · E · C` |
| S4 | 6 | `D3 · B · D2` | `D3 · B · D2` |
| S5 | 6 | `E2 · C · A · F · E` | `E2 · C · A · F · E` |
| S6 | 6 | `A · F2 · D · B2` | `A · F2 · D · B2` |
| S7 | 6 | `∅` | `∅` |
| S8 | 6 | `∅` | `∅` |

本关固定结构：23个初始连续段、0颗问号蛋。主路22步，仅存在性见证。首次归巢在第10步，之后该枝不再参与排序。本见证未发生容量截断，不证明所有路线都不会发生。

### 完整合法见证

| 步 | 玩家动作和自动事件 | 已归巢种类 | 稳定后所有位置（分析视图） |
| --- | --- | --- | --- |
| 1 | Move S1→S7 E1（run=1，Count=1） | 无 | S1=[A3 · F · A]；S2=[B2 · E · F2 · B]；S3=[C4 · E · C]；S4=[D3 · B · D2]；S5=[E2 · C · A · F · E]；S6=[A · F2 · D · B2]；S7=[E]；S8=[∅] |
| 2 | Move S5→S7 E1（run=1，Count=1） | 无 | S1=[A3 · F · A]；S2=[B2 · E · F2 · B]；S3=[C4 · E · C]；S4=[D3 · B · D2]；S5=[E2 · C · A · F]；S6=[A · F2 · D · B2]；S7=[E2]；S8=[∅] |
| 3 | Move S1→S8 A1（run=1，Count=1） | 无 | S1=[A3 · F]；S2=[B2 · E · F2 · B]；S3=[C4 · E · C]；S4=[D3 · B · D2]；S5=[E2 · C · A · F]；S6=[A · F2 · D · B2]；S7=[E2]；S8=[A] |
| 4 | Move S1→S5 F1（run=1，Count=1） | 无 | S1=[A3]；S2=[B2 · E · F2 · B]；S3=[C4 · E · C]；S4=[D3 · B · D2]；S5=[E2 · C · A · F2]；S6=[A · F2 · D · B2]；S7=[E2]；S8=[A] |
| 5 | Move S1→S8 A3（run=3，Count=3） | 无 | S1=[∅]；S2=[B2 · E · F2 · B]；S3=[C4 · E · C]；S4=[D3 · B · D2]；S5=[E2 · C · A · F2]；S6=[A · F2 · D · B2]；S7=[E2]；S8=[A4] |
| 6 | Move S2→S1 B1（run=1，Count=1） | 无 | S1=[B]；S2=[B2 · E · F2]；S3=[C4 · E · C]；S4=[D3 · B · D2]；S5=[E2 · C · A · F2]；S6=[A · F2 · D · B2]；S7=[E2]；S8=[A4] |
| 7 | Move S6→S1 B2（run=2，Count=2） | 无 | S1=[B3]；S2=[B2 · E · F2]；S3=[C4 · E · C]；S4=[D3 · B · D2]；S5=[E2 · C · A · F2]；S6=[A · F2 · D]；S7=[E2]；S8=[A4] |
| 8 | Move S4→S6 D2（run=2，Count=2） | 无 | S1=[B3]；S2=[B2 · E · F2]；S3=[C4 · E · C]；S4=[D3 · B]；S5=[E2 · C · A · F2]；S6=[A · F2 · D3]；S7=[E2]；S8=[A4] |
| 9 | Move S4→S1 B1（run=1，Count=1） | 无 | S1=[B4]；S2=[B2 · E · F2]；S3=[C4 · E · C]；S4=[D3]；S5=[E2 · C · A · F2]；S6=[A · F2 · D3]；S7=[E2]；S8=[A4] |
| 10 | Move S6→S4 D3（run=3，Count=3）；Home D@S4；Retire S4 | D | S1=[B4]；S2=[B2 · E · F2]；S3=[C4 · E · C]；S4=[∅][Retired]；S5=[E2 · C · A · F2]；S6=[A · F2]；S7=[E2]；S8=[A4] |
| 11 | Move S2→S6 F2（run=2，Count=2） | D | S1=[B4]；S2=[B2 · E]；S3=[C4 · E · C]；S4=[∅][Retired]；S5=[E2 · C · A · F2]；S6=[A · F4]；S7=[E2]；S8=[A4] |
| 12 | Move S2→S7 E1（run=1，Count=1） | D | S1=[B4]；S2=[B2]；S3=[C4 · E · C]；S4=[∅][Retired]；S5=[E2 · C · A · F2]；S6=[A · F4]；S7=[E3]；S8=[A4] |
| 13 | Move S1→S2 B4（run=4，Count=4）；Home B@S2；Retire S2 | B,D | S1=[∅]；S2=[∅][Retired]；S3=[C4 · E · C]；S4=[∅][Retired]；S5=[E2 · C · A · F2]；S6=[A · F4]；S7=[E3]；S8=[A4] |
| 14 | Move S3→S1 C1（run=1，Count=1） | B,D | S1=[C]；S2=[∅][Retired]；S3=[C4 · E]；S4=[∅][Retired]；S5=[E2 · C · A · F2]；S6=[A · F4]；S7=[E3]；S8=[A4] |
| 15 | Move S3→S7 E1（run=1，Count=1） | B,D | S1=[C]；S2=[∅][Retired]；S3=[C4]；S4=[∅][Retired]；S5=[E2 · C · A · F2]；S6=[A · F4]；S7=[E4]；S8=[A4] |
| 16 | Move S1→S3 C1（run=1，Count=1） | B,D | S1=[∅]；S2=[∅][Retired]；S3=[C5]；S4=[∅][Retired]；S5=[E2 · C · A · F2]；S6=[A · F4]；S7=[E4]；S8=[A4] |
| 17 | Move S5→S1 F2（run=2，Count=2） | B,D | S1=[F2]；S2=[∅][Retired]；S3=[C5]；S4=[∅][Retired]；S5=[E2 · C · A]；S6=[A · F4]；S7=[E4]；S8=[A4] |
| 18 | Move S5→S8 A1（run=1，Count=1） | B,D | S1=[F2]；S2=[∅][Retired]；S3=[C5]；S4=[∅][Retired]；S5=[E2 · C]；S6=[A · F4]；S7=[E4]；S8=[A5] |
| 19 | Move S5→S3 C1（run=1，Count=1）；Home C@S3；Retire S3 | B,C,D | S1=[F2]；S2=[∅][Retired]；S3=[∅][Retired]；S4=[∅][Retired]；S5=[E2]；S6=[A · F4]；S7=[E4]；S8=[A5] |
| 20 | Move S5→S7 E2（run=2，Count=2）；Home E@S7；Retire S7 | B,C,D,E | S1=[F2]；S2=[∅][Retired]；S3=[∅][Retired]；S4=[∅][Retired]；S5=[∅]；S6=[A · F4]；S7=[∅][Retired]；S8=[A5] |
| 21 | Move S6→S1 F4（run=4，Count=4）；Home F@S1；Retire S1 | B,C,D,E,F | S1=[∅][Retired]；S2=[∅][Retired]；S3=[∅][Retired]；S4=[∅][Retired]；S5=[∅]；S6=[A]；S7=[∅][Retired]；S8=[A5] |
| 22 | Move S6→S8 A1（run=1，Count=1）；Home A@S8；Retire S8 | A,B,C,D,E,F | S1=[∅][Retired]；S2=[∅][Retired]；S3=[∅][Retired]；S4=[∅][Retired]；S5=[∅]；S6=[∅]；S7=[∅][Retired]；S8=[∅][Retired] |

终态：全部K种归巢、所有token为空、恰K条完成枝Retired，最初及中途普通搬空枝不因空而退休；每种现存token+已收集数量在每步均等于N。全程未使用扩枝、加高、提示、广告或付费。

验收边界：该路径由独立文档模型按完整species状态复算，非项目Core/Unity、非最优、非实际Undo/存档/音画或真人通过。含蛋关还须可见信息公平性验证；无此证据不得宣称玩家无需试错就能知道下一步。

## 关卡39

双揭示挑战；世界D 雪松林；N=8，K=6，P=8；所有枝相同容量；每种含蛋共8token；开局两空。

来源：SRC-LEAF/SRC-SIMPLE（端部、接收与移动）；SRC-BIRD2（完成飞走失枝）；容量SRC-BOOP；SRC-QUEST（?蛋）。

| 固定ID | 容量 | 策划完整初始数组 | 玩家初始可见数组 |
| --- | --- | --- | --- |
| S1 | 8 | `E2 · ?(F) · A5` | `E2 · ? · A5` |
| S2 | 8 | `F3 · B · C2 · B2` | `F3 · B · C2 · B2` |
| S3 | 8 | `C2 · F · C2 · B2 · C` | `C2 · F · C2 · B2 · C` |
| S4 | 8 | `D · E2 · A · D4` | `D · E2 · A · D4` |
| S5 | 8 | `D · ?(D) · A · E4 · D` | `D · ? · A · E4 · D` |
| S6 | 8 | `C · A · B3 · F3` | `C · A · B3 · F3` |
| S7 | 8 | `∅` | `∅` |
| S8 | 8 | `∅` | `∅` |

本关固定结构：25个初始连续段、2颗问号蛋。主路23步，仅存在性见证。首次归巢在第11步，之后该枝不再参与排序。本见证未发生容量截断，不证明所有路线都不会发生。孵化见第1,22步；未孵化种类对玩家隐藏。

### 完整合法见证

| 步 | 玩家动作和自动事件 | 已归巢种类 | 稳定后所有位置（分析视图） |
| --- | --- | --- | --- |
| 1 | Move S1→S7 A5（run=5，Count=5）；Hatch S1 → F | 无 | S1=[E2 · F]；S2=[F3 · B · C2 · B2]；S3=[C2 · F · C2 · B2 · C]；S4=[D · E2 · A · D4]；S5=[D · ?(D) · A · E4 · D]；S6=[C · A · B3 · F3]；S7=[A5]；S8=[∅] |
| 2 | Move S6→S1 F3（run=3，Count=3） | 无 | S1=[E2 · F4]；S2=[F3 · B · C2 · B2]；S3=[C2 · F · C2 · B2 · C]；S4=[D · E2 · A · D4]；S5=[D · ?(D) · A · E4 · D]；S6=[C · A · B3]；S7=[A5]；S8=[∅] |
| 3 | Move S2→S6 B2（run=2，Count=2） | 无 | S1=[E2 · F4]；S2=[F3 · B · C2]；S3=[C2 · F · C2 · B2 · C]；S4=[D · E2 · A · D4]；S5=[D · ?(D) · A · E4 · D]；S6=[C · A · B5]；S7=[A5]；S8=[∅] |
| 4 | Move S3→S2 C1（run=1，Count=1） | 无 | S1=[E2 · F4]；S2=[F3 · B · C3]；S3=[C2 · F · C2 · B2]；S4=[D · E2 · A · D4]；S5=[D · ?(D) · A · E4 · D]；S6=[C · A · B5]；S7=[A5]；S8=[∅] |
| 5 | Move S2→S8 C3（run=3，Count=3） | 无 | S1=[E2 · F4]；S2=[F3 · B]；S3=[C2 · F · C2 · B2]；S4=[D · E2 · A · D4]；S5=[D · ?(D) · A · E4 · D]；S6=[C · A · B5]；S7=[A5]；S8=[C3] |
| 6 | Move S3→S2 B2（run=2，Count=2） | 无 | S1=[E2 · F4]；S2=[F3 · B3]；S3=[C2 · F · C2]；S4=[D · E2 · A · D4]；S5=[D · ?(D) · A · E4 · D]；S6=[C · A · B5]；S7=[A5]；S8=[C3] |
| 7 | Move S3→S8 C2（run=2，Count=2） | 无 | S1=[E2 · F4]；S2=[F3 · B3]；S3=[C2 · F]；S4=[D · E2 · A · D4]；S5=[D · ?(D) · A · E4 · D]；S6=[C · A · B5]；S7=[A5]；S8=[C5] |
| 8 | Move S3→S1 F1（run=1，Count=1） | 无 | S1=[E2 · F5]；S2=[F3 · B3]；S3=[C2]；S4=[D · E2 · A · D4]；S5=[D · ?(D) · A · E4 · D]；S6=[C · A · B5]；S7=[A5]；S8=[C5] |
| 9 | Move S3→S8 C2（run=2，Count=2） | 无 | S1=[E2 · F5]；S2=[F3 · B3]；S3=[∅]；S4=[D · E2 · A · D4]；S5=[D · ?(D) · A · E4 · D]；S6=[C · A · B5]；S7=[A5]；S8=[C7] |
| 10 | Move S2→S3 B3（run=3，Count=3） | 无 | S1=[E2 · F5]；S2=[F3]；S3=[B3]；S4=[D · E2 · A · D4]；S5=[D · ?(D) · A · E4 · D]；S6=[C · A · B5]；S7=[A5]；S8=[C7] |
| 11 | Move S1→S2 F5（run=5，Count=5）；Home F@S2；Retire S2 | F | S1=[E2]；S2=[∅][Retired]；S3=[B3]；S4=[D · E2 · A · D4]；S5=[D · ?(D) · A · E4 · D]；S6=[C · A · B5]；S7=[A5]；S8=[C7] |
| 12 | Move S6→S3 B5（run=5，Count=5）；Home B@S3；Retire S3 | B,F | S1=[E2]；S2=[∅][Retired]；S3=[∅][Retired]；S4=[D · E2 · A · D4]；S5=[D · ?(D) · A · E4 · D]；S6=[C · A]；S7=[A5]；S8=[C7] |
| 13 | Move S6→S7 A1（run=1，Count=1） | B,F | S1=[E2]；S2=[∅][Retired]；S3=[∅][Retired]；S4=[D · E2 · A · D4]；S5=[D · ?(D) · A · E4 · D]；S6=[C]；S7=[A6]；S8=[C7] |
| 14 | Move S6→S8 C1（run=1，Count=1）；Home C@S8；Retire S8 | B,C,F | S1=[E2]；S2=[∅][Retired]；S3=[∅][Retired]；S4=[D · E2 · A · D4]；S5=[D · ?(D) · A · E4 · D]；S6=[∅]；S7=[A6]；S8=[∅][Retired] |
| 15 | Move S4→S6 D4（run=4，Count=4） | B,C,F | S1=[E2]；S2=[∅][Retired]；S3=[∅][Retired]；S4=[D · E2 · A]；S5=[D · ?(D) · A · E4 · D]；S6=[D4]；S7=[A6]；S8=[∅][Retired] |
| 16 | Move S4→S7 A1（run=1，Count=1） | B,C,F | S1=[E2]；S2=[∅][Retired]；S3=[∅][Retired]；S4=[D · E2]；S5=[D · ?(D) · A · E4 · D]；S6=[D4]；S7=[A7]；S8=[∅][Retired] |
| 17 | Move S1→S4 E2（run=2，Count=2） | B,C,F | S1=[∅]；S2=[∅][Retired]；S3=[∅][Retired]；S4=[D · E4]；S5=[D · ?(D) · A · E4 · D]；S6=[D4]；S7=[A7]；S8=[∅][Retired] |
| 18 | Move S5→S6 D1（run=1，Count=1） | B,C,F | S1=[∅]；S2=[∅][Retired]；S3=[∅][Retired]；S4=[D · E4]；S5=[D · ?(D) · A · E4]；S6=[D5]；S7=[A7]；S8=[∅][Retired] |
| 19 | Move S4→S1 E4（run=4，Count=4） | B,C,F | S1=[E4]；S2=[∅][Retired]；S3=[∅][Retired]；S4=[D]；S5=[D · ?(D) · A · E4]；S6=[D5]；S7=[A7]；S8=[∅][Retired] |
| 20 | Move S4→S6 D1（run=1，Count=1） | B,C,F | S1=[E4]；S2=[∅][Retired]；S3=[∅][Retired]；S4=[∅]；S5=[D · ?(D) · A · E4]；S6=[D6]；S7=[A7]；S8=[∅][Retired] |
| 21 | Move S5→S1 E4（run=4，Count=4）；Home E@S1；Retire S1 | B,C,E,F | S1=[∅][Retired]；S2=[∅][Retired]；S3=[∅][Retired]；S4=[∅]；S5=[D · ?(D) · A]；S6=[D6]；S7=[A7]；S8=[∅][Retired] |
| 22 | Move S5→S7 A1（run=1，Count=1）；Hatch S5 → D；Home A@S7；Retire S7 | A,B,C,E,F | S1=[∅][Retired]；S2=[∅][Retired]；S3=[∅][Retired]；S4=[∅]；S5=[D2]；S6=[D6]；S7=[∅][Retired]；S8=[∅][Retired] |
| 23 | Move S5→S6 D2（run=2，Count=2）；Home D@S6；Retire S6 | A,B,C,D,E,F | S1=[∅][Retired]；S2=[∅][Retired]；S3=[∅][Retired]；S4=[∅]；S5=[∅]；S6=[∅][Retired]；S7=[∅][Retired]；S8=[∅][Retired] |

终态：全部K种归巢、所有token为空、恰K条完成枝Retired，最初及中途普通搬空枝不因空而退休；每种现存token+已收集数量在每步均等于N。全程未使用扩枝、加高、提示、广告或付费。

验收边界：该路径由独立文档模型按完整species状态复算，非项目Core/Unity、非最优、非实际Undo/存档/音画或真人通过。含蛋关还须可见信息公平性验证；无此证据不得宣称玩家无需试错就能知道下一步。

## 关卡40

雪松林归巢终章；世界D 雪松林；N=8，K=4，P=6；所有枝相同容量；每种含蛋共8token；开局两空。

来源：SRC-LEAF/SRC-SIMPLE（端部、接收与移动）；SRC-BIRD2（完成飞走失枝）；容量SRC-BOOP。

| 固定ID | 容量 | 策划完整初始数组 | 玩家初始可见数组 |
| --- | --- | --- | --- |
| S1 | 8 | `A · D2 · A2 · B · A2` | `A · D2 · A2 · B · A2` |
| S2 | 8 | `C2 · B6` | `C2 · B6` |
| S3 | 8 | `C · B · A · C5` | `C · B · A · C5` |
| S4 | 8 | `D5 · A2 · D` | `D5 · A2 · D` |
| S5 | 8 | `∅` | `∅` |
| S6 | 8 | `∅` | `∅` |

本关固定结构：14个初始连续段、0颗问号蛋。主路12步，仅存在性见证。首次归巢在第9步，之后该枝不再参与排序。本见证未发生容量截断，不证明所有路线都不会发生。

### 完整合法见证

| 步 | 玩家动作和自动事件 | 已归巢种类 | 稳定后所有位置（分析视图） |
| --- | --- | --- | --- |
| 1 | Move S2→S5 B6（run=6，Count=6） | 无 | S1=[A · D2 · A2 · B · A2]；S2=[C2]；S3=[C · B · A · C5]；S4=[D5 · A2 · D]；S5=[B6]；S6=[∅] |
| 2 | Move S3→S2 C5（run=5，Count=5） | 无 | S1=[A · D2 · A2 · B · A2]；S2=[C7]；S3=[C · B · A]；S4=[D5 · A2 · D]；S5=[B6]；S6=[∅] |
| 3 | Move S1→S3 A2（run=2，Count=2） | 无 | S1=[A · D2 · A2 · B]；S2=[C7]；S3=[C · B · A3]；S4=[D5 · A2 · D]；S5=[B6]；S6=[∅] |
| 4 | Move S1→S5 B1（run=1，Count=1） | 无 | S1=[A · D2 · A2]；S2=[C7]；S3=[C · B · A3]；S4=[D5 · A2 · D]；S5=[B7]；S6=[∅] |
| 5 | Move S1→S3 A2（run=2，Count=2） | 无 | S1=[A · D2]；S2=[C7]；S3=[C · B · A5]；S4=[D5 · A2 · D]；S5=[B7]；S6=[∅] |
| 6 | Move S4→S1 D1（run=1，Count=1） | 无 | S1=[A · D3]；S2=[C7]；S3=[C · B · A5]；S4=[D5 · A2]；S5=[B7]；S6=[∅] |
| 7 | Move S1→S6 D3（run=3，Count=3） | 无 | S1=[A]；S2=[C7]；S3=[C · B · A5]；S4=[D5 · A2]；S5=[B7]；S6=[D3] |
| 8 | Move S3→S1 A5（run=5，Count=5） | 无 | S1=[A6]；S2=[C7]；S3=[C · B]；S4=[D5 · A2]；S5=[B7]；S6=[D3] |
| 9 | Move S3→S5 B1（run=1，Count=1）；Home B@S5；Retire S5 | B | S1=[A6]；S2=[C7]；S3=[C]；S4=[D5 · A2]；S5=[∅][Retired]；S6=[D3] |
| 10 | Move S2→S3 C7（run=7，Count=7）；Home C@S3；Retire S3 | B,C | S1=[A6]；S2=[∅]；S3=[∅][Retired]；S4=[D5 · A2]；S5=[∅][Retired]；S6=[D3] |
| 11 | Move S4→S1 A2（run=2，Count=2）；Home A@S1；Retire S1 | A,B,C | S1=[∅][Retired]；S2=[∅]；S3=[∅][Retired]；S4=[D5]；S5=[∅][Retired]；S6=[D3] |
| 12 | Move S4→S6 D5（run=5，Count=5）；Home D@S6；Retire S6 | A,B,C,D | S1=[∅][Retired]；S2=[∅]；S3=[∅][Retired]；S4=[∅]；S5=[∅][Retired]；S6=[∅][Retired] |

终态：全部K种归巢、所有token为空、恰K条完成枝Retired，最初及中途普通搬空枝不因空而退休；每种现存token+已收集数量在每步均等于N。全程未使用扩枝、加高、提示、广告或付费。

验收边界：该路径由独立文档模型按完整species状态复算，非项目Core/Unity、非最优、非实际Undo/存档/音画或真人通过。含蛋关还须可见信息公平性验证；无此证据不得宣称玩家无需试错就能知道下一步。
