# 第11–20关：固定候选与纸面完整回放

2026-10-31 · DESIGNED / INDEPENDENT_REPLAY_PASS / PENDING_GRAYBOX。这是本批**固定初态唯一权威**；[设计卡](../LEVEL_DESIGN.md)只拥有教学/节奏，[模拟入口](../LEVEL_SIMULATIONS.md)拥有记号与证据边界，[RULES](../spec/RULES.md)拥有算法。仅Markdown候选，未修改游戏关卡JSON。

数组末项外露，`蛋(A)`是一只固定A种蛋，普通`E`始终是E鸟。`A3`精确等于三只A，不是随机数量；展开原序分配token ID。未列机关不存在。Complete/Sleep与Complete/Home表示本次首次成组六只、完成数+1；AutoHome不重复加数。表内接收临时枝事件包含首次Armed或已Armed再次接收，具体三态已在完整快照标出。

每条主路/对照路由独立文档模型逐步校验合法性、锁run/Count、稳定化、每种六只守恒及全空终态；不是项目Core/Unity/最优/真人证书。下面数值是路线长度，不是solverMinMoves。固定内容不得运行时生成、换色或换seed。

## 关卡11

K4/P6；Q=`A,B,C,D`。单蛋阻断run；本步B2离开，孵C不追飞。

| 固定ID | 容量 | 固定初始数组（墙→露端） | 类型/条件 |
| --- | --- | --- | --- |
| S1 | 6 | `A3 · 蛋(C) · B2` | 普通枝 |
| S2 | 6 | `B4 · C2` | 普通枝 |
| S3 | 6 | `C3 · A3` | 普通枝 |
| S4 | 6 | `D3` | 普通枝 |
| S5 | 6 | `D3` | 普通枝 |
| S6 | 6 | `∅` | 普通枝 |

### 主路线（7步，非最优证明）

| 步 | 玩家动作/自动事件顺序 | 完成数 / 已归巢 / 当前 | 稳定后全部容器 |
| --- | --- | --- | --- |
| 1 | Move S1→S6 B2；Hatch C@S1；run=2,Count=2 | 0 / 无 / AB | S1=[A3 · C]；S2=[B4 · C2]；S3=[C3 · A3]；S4=[D3]；S5=[D3]；S6=[B2] |
| 2 | Move S2→S1 C2；run=2,Count=2 | 0 / 无 / AB | S1=[A3 · C3]；S2=[B4]；S3=[C3 · A3]；S4=[D3]；S5=[D3]；S6=[B2] |
| 3 | Move S6→S2 B2；Complete/Home B@S2；run=2,Count=2 | 1 / B / AC | S1=[A3 · C3]；S2=[∅]；S3=[C3 · A3]；S4=[D3]；S5=[D3]；S6=[∅] |
| 4 | Move S3→S6 A3；run=3,Count=3 | 1 / B / AC | S1=[A3 · C3]；S2=[∅]；S3=[C3]；S4=[D3]；S5=[D3]；S6=[A3] |
| 5 | Move S1→S3 C3；Complete/Home C@S3；run=3,Count=3 | 2 / BC / AD | S1=[A3]；S2=[∅]；S3=[∅]；S4=[D3]；S5=[D3]；S6=[A3] |
| 6 | Move S6→S1 A3；Complete/Home A@S1；run=3,Count=3 | 3 / ABC / D | S1=[∅]；S2=[∅]；S3=[∅]；S4=[D3]；S5=[D3]；S6=[∅] |
| 7 | Move S4→S5 D3；Complete/Home D@S5；run=3,Count=3 | 4 / ABCD / 全部完成 | S1=[∅]；S2=[∅]；S3=[∅]；S4=[∅]；S5=[∅]；S6=[∅] |


### 对照路线1（6步，非最优证明）

| 步 | 玩家动作/自动事件顺序 | 完成数 / 已归巢 / 当前 | 稳定后全部容器 |
| --- | --- | --- | --- |
| 1 | Move S4→S5 D3；Complete/Sleep D@S5；run=3,Count=3 | 1 / 无 / AB | S1=[A3 · 蛋(C) · B2]；S2=[B4 · C2]；S3=[C3 · A3]；S4=[∅]；S5=[D6][睡眠]；S6=[∅] |
| 2 | Move S2→S4 C2；run=2,Count=2 | 1 / 无 / AB | S1=[A3 · 蛋(C) · B2]；S2=[B4]；S3=[C3 · A3]；S4=[C2]；S5=[D6][睡眠]；S6=[∅] |
| 3 | Move S1→S2 B2；Hatch C@S1；Complete/Home B@S2；run=2,Count=2 | 2 / B / AC | S1=[A3 · C]；S2=[∅]；S3=[C3 · A3]；S4=[C2]；S5=[D6][睡眠]；S6=[∅] |
| 4 | Move S1→S4 C1；run=1,Count=1 | 2 / B / AC | S1=[A3]；S2=[∅]；S3=[C3 · A3]；S4=[C3]；S5=[D6][睡眠]；S6=[∅] |
| 5 | Move S3→S1 A3；Complete/Home A@S1；AutoHome D@S5；run=3,Count=3 | 3 / ABD / C | S1=[∅]；S2=[∅]；S3=[C3]；S4=[C3]；S5=[∅]；S6=[∅] |
| 6 | Move S3→S4 C3；Complete/Home C@S4；run=3,Count=3 | 4 / ABCD / 全部完成 | S1=[∅]；S2=[∅]；S3=[∅]；S4=[∅]；S5=[∅]；S6=[∅] |


终态：全部目标已收集；全部容器无token，临时枝若启用已退役；物种每步守恒。项目最优证明/Unity/真人/设备/实际Undo存档均未验证。

## 关卡12

K4/P6；Q=`B,D,A,C`。B蛋先更早腾枝，A蛋先改变中间空间；两条见证都是八步，不称唯一最优。

| 固定ID | 容量 | 固定初始数组（墙→露端） | 类型/条件 |
| --- | --- | --- | --- |
| S1 | 6 | `A2 · 蛋(B) · C2` | 普通枝 |
| S2 | 6 | `C2 · 蛋(A) · D` | 普通枝 |
| S3 | 6 | `B5` | 普通枝 |
| S4 | 6 | `A3 · D3` | 普通枝 |
| S5 | 6 | `D2 · C2` | 普通枝 |
| S6 | 6 | `∅` | 普通枝 |

### 主路线（8步，非最优证明）

| 步 | 玩家动作/自动事件顺序 | 完成数 / 已归巢 / 当前 | 稳定后全部容器 |
| --- | --- | --- | --- |
| 1 | Move S1→S6 C2；Hatch B@S1；run=2,Count=2 | 0 / 无 / BD | S1=[A2 · B]；S2=[C2 · 蛋(A) · D]；S3=[B5]；S4=[A3 · D3]；S5=[D2 · C2]；S6=[C2] |
| 2 | Move S1→S3 B1；Complete/Home B@S3；run=1,Count=1 | 1 / B / DA | S1=[A2]；S2=[C2 · 蛋(A) · D]；S3=[∅]；S4=[A3 · D3]；S5=[D2 · C2]；S6=[C2] |
| 3 | Move S5→S6 C2；run=2,Count=2 | 1 / B / DA | S1=[A2]；S2=[C2 · 蛋(A) · D]；S3=[∅]；S4=[A3 · D3]；S5=[D2]；S6=[C4] |
| 4 | Move S2→S5 D1；Hatch A@S2；run=1,Count=1 | 1 / B / DA | S1=[A2]；S2=[C2 · A]；S3=[∅]；S4=[A3 · D3]；S5=[D3]；S6=[C4] |
| 5 | Move S4→S5 D3；Complete/Home D@S5；run=3,Count=3 | 2 / BD / AC | S1=[A2]；S2=[C2 · A]；S3=[∅]；S4=[A3]；S5=[∅]；S6=[C4] |
| 6 | Move S2→S4 A1；run=1,Count=1 | 2 / BD / AC | S1=[A2]；S2=[C2]；S3=[∅]；S4=[A4]；S5=[∅]；S6=[C4] |
| 7 | Move S1→S4 A2；Complete/Home A@S4；run=2,Count=2 | 3 / BDA / C | S1=[∅]；S2=[C2]；S3=[∅]；S4=[∅]；S5=[∅]；S6=[C4] |
| 8 | Move S2→S6 C2；Complete/Home C@S6；run=2,Count=2 | 4 / BDAC / 全部完成 | S1=[∅]；S2=[∅]；S3=[∅]；S4=[∅]；S5=[∅]；S6=[∅] |


### 对照路线1（8步，非最优证明）

| 步 | 玩家动作/自动事件顺序 | 完成数 / 已归巢 / 当前 | 稳定后全部容器 |
| --- | --- | --- | --- |
| 1 | Move S2→S6 D1；Hatch A@S2；run=1,Count=1 | 0 / 无 / BD | S1=[A2 · 蛋(B) · C2]；S2=[C2 · A]；S3=[B5]；S4=[A3 · D3]；S5=[D2 · C2]；S6=[D] |
| 2 | Move S4→S6 D3；run=3,Count=3 | 0 / 无 / BD | S1=[A2 · 蛋(B) · C2]；S2=[C2 · A]；S3=[B5]；S4=[A3]；S5=[D2 · C2]；S6=[D4] |
| 3 | Move S2→S4 A1；run=1,Count=1 | 0 / 无 / BD | S1=[A2 · 蛋(B) · C2]；S2=[C2]；S3=[B5]；S4=[A4]；S5=[D2 · C2]；S6=[D4] |
| 4 | Move S1→S2 C2；Hatch B@S1；run=2,Count=2 | 0 / 无 / BD | S1=[A2 · B]；S2=[C4]；S3=[B5]；S4=[A4]；S5=[D2 · C2]；S6=[D4] |
| 5 | Move S1→S3 B1；Complete/Home B@S3；run=1,Count=1 | 1 / B / DA | S1=[A2]；S2=[C4]；S3=[∅]；S4=[A4]；S5=[D2 · C2]；S6=[D4] |
| 6 | Move S5→S2 C2；Complete/Sleep C@S2；run=2,Count=2 | 2 / B / DA | S1=[A2]；S2=[C6][睡眠]；S3=[∅]；S4=[A4]；S5=[D2]；S6=[D4] |
| 7 | Move S5→S6 D2；Complete/Home D@S6；AutoHome C@S2；run=2,Count=2 | 3 / BDC / A | S1=[A2]；S2=[∅]；S3=[∅]；S4=[A4]；S5=[∅]；S6=[∅] |
| 8 | Move S1→S4 A2；Complete/Home A@S4；run=2,Count=2 | 4 / BDAC / 全部完成 | S1=[∅]；S2=[∅]；S3=[∅]；S4=[∅]；S5=[∅]；S6=[∅] |


终态：全部目标已收集；全部容器无token，临时枝若启用已退役；物种每步守恒。项目最优证明/Unity/真人/设备/实际Undo存档均未验证。

## 关卡13

K4/P6；Q=`C,A,D,B`。连续两蛋一次只孵外侧；先解另一蛋改变睡眠时机。

| 固定ID | 容量 | 固定初始数组（墙→露端） | 类型/条件 |
| --- | --- | --- | --- |
| S1 | 6 | `蛋(A) · 蛋(B) · D2` | 普通枝 |
| S2 | 6 | `B5` | 普通枝 |
| S3 | 6 | `A2 · 蛋(C) · D` | 普通枝 |
| S4 | 6 | `C5` | 普通枝 |
| S5 | 6 | `A3 · D3` | 普通枝 |
| S6 | 6 | `∅` | 普通枝 |

### 主路线（7步，非最优证明）

| 步 | 玩家动作/自动事件顺序 | 完成数 / 已归巢 / 当前 | 稳定后全部容器 |
| --- | --- | --- | --- |
| 1 | Move S1→S6 D2；Hatch B@S1；run=2,Count=2 | 0 / 无 / CA | S1=[蛋(A) · B]；S2=[B5]；S3=[A2 · 蛋(C) · D]；S4=[C5]；S5=[A3 · D3]；S6=[D2] |
| 2 | Move S1→S2 B1；Hatch A@S1；Complete/Sleep B@S2；run=1,Count=1 | 1 / 无 / CA | S1=[A]；S2=[B6][睡眠]；S3=[A2 · 蛋(C) · D]；S4=[C5]；S5=[A3 · D3]；S6=[D2] |
| 3 | Move S3→S6 D1；Hatch C@S3；run=1,Count=1 | 1 / 无 / CA | S1=[A]；S2=[B6][睡眠]；S3=[A2 · C]；S4=[C5]；S5=[A3 · D3]；S6=[D3] |
| 4 | Move S3→S4 C1；Complete/Home C@S4；run=1,Count=1 | 2 / C / AD | S1=[A]；S2=[B6][睡眠]；S3=[A2]；S4=[∅]；S5=[A3 · D3]；S6=[D3] |
| 5 | Move S5→S6 D3；Complete/Home D@S6；AutoHome B@S2；run=3,Count=3 | 3 / CDB / A | S1=[A]；S2=[∅]；S3=[A2]；S4=[∅]；S5=[A3]；S6=[∅] |
| 6 | Move S1→S3 A1；run=1,Count=1 | 3 / CDB / A | S1=[∅]；S2=[∅]；S3=[A3]；S4=[∅]；S5=[A3]；S6=[∅] |
| 7 | Move S5→S3 A3；Complete/Home A@S3；run=3,Count=3 | 4 / CADB / 全部完成 | S1=[∅]；S2=[∅]；S3=[∅]；S4=[∅]；S5=[∅]；S6=[∅] |


### 对照路线1（7步，非最优证明）

| 步 | 玩家动作/自动事件顺序 | 完成数 / 已归巢 / 当前 | 稳定后全部容器 |
| --- | --- | --- | --- |
| 1 | Move S3→S6 D1；Hatch C@S3；run=1,Count=1 | 0 / 无 / CA | S1=[蛋(A) · 蛋(B) · D2]；S2=[B5]；S3=[A2 · C]；S4=[C5]；S5=[A3 · D3]；S6=[D] |
| 2 | Move S3→S4 C1；Complete/Home C@S4；run=1,Count=1 | 1 / C / AD | S1=[蛋(A) · 蛋(B) · D2]；S2=[B5]；S3=[A2]；S4=[∅]；S5=[A3 · D3]；S6=[D] |
| 3 | Move S1→S6 D2；Hatch B@S1；run=2,Count=2 | 1 / C / AD | S1=[蛋(A) · B]；S2=[B5]；S3=[A2]；S4=[∅]；S5=[A3 · D3]；S6=[D3] |
| 4 | Move S1→S2 B1；Hatch A@S1；Complete/Sleep B@S2；run=1,Count=1 | 2 / C / AD | S1=[A]；S2=[B6][睡眠]；S3=[A2]；S4=[∅]；S5=[A3 · D3]；S6=[D3] |
| 5 | Move S5→S6 D3；Complete/Home D@S6；AutoHome B@S2；run=3,Count=3 | 3 / CDB / A | S1=[A]；S2=[∅]；S3=[A2]；S4=[∅]；S5=[A3]；S6=[∅] |
| 6 | Move S1→S3 A1；run=1,Count=1 | 3 / CDB / A | S1=[∅]；S2=[∅]；S3=[A3]；S4=[∅]；S5=[A3]；S6=[∅] |
| 7 | Move S5→S3 A3；Complete/Home A@S3；run=3,Count=3 | 4 / CADB / 全部完成 | S1=[∅]；S2=[∅]；S3=[∅]；S4=[∅]；S5=[∅]；S6=[∅] |


终态：全部目标已收集；全部容器无token，临时枝若启用已退役；物种每步守恒。项目最优证明/Unity/真人/设备/实际Undo存档均未验证。

## 关卡14

K4/P6；Q=`D,B,C,A`。短枝存C4、长枝留D汇合；另一顺序有截断但总步同为九。

| 固定ID | 容量 | 固定初始数组（墙→露端） | 类型/条件 |
| --- | --- | --- | --- |
| S1 | 6 | `A · 蛋(D) · C4` | 普通枝 |
| S2 | 6 | `C · 蛋(B) · A4` | 普通枝 |
| S3 | 6 | `D5 · A` | 普通枝 |
| S4 | 6 | `B5 · C` | 普通枝 |
| S5 | 6 | `∅` | 普通枝 |
| S6 | 4 | `∅` | 普通枝 |

### 主路线（9步，非最优证明）

| 步 | 玩家动作/自动事件顺序 | 完成数 / 已归巢 / 当前 | 稳定后全部容器 |
| --- | --- | --- | --- |
| 1 | Move S1→S6 C4；Hatch D@S1；run=4,Count=4 | 0 / 无 / DB | S1=[A · D]；S2=[C · 蛋(B) · A4]；S3=[D5 · A]；S4=[B5 · C]；S5=[∅]；S6=[C4] |
| 2 | Move S1→S5 D1；run=1,Count=1 | 0 / 无 / DB | S1=[A]；S2=[C · 蛋(B) · A4]；S3=[D5 · A]；S4=[B5 · C]；S5=[D]；S6=[C4] |
| 3 | Move S3→S1 A1；run=1,Count=1 | 0 / 无 / DB | S1=[A2]；S2=[C · 蛋(B) · A4]；S3=[D5]；S4=[B5 · C]；S5=[D]；S6=[C4] |
| 4 | Move S3→S5 D5；Complete/Home D@S5；run=5,Count=5 | 1 / D / BC | S1=[A2]；S2=[C · 蛋(B) · A4]；S3=[∅]；S4=[B5 · C]；S5=[∅]；S6=[C4] |
| 5 | Move S2→S1 A4；Hatch B@S2；Complete/Sleep A@S1；run=4,Count=4 | 2 / D / BC | S1=[A6][睡眠]；S2=[C · B]；S3=[∅]；S4=[B5 · C]；S5=[∅]；S6=[C4] |
| 6 | Move S4→S3 C1；run=1,Count=1 | 2 / D / BC | S1=[A6][睡眠]；S2=[C · B]；S3=[C]；S4=[B5]；S5=[∅]；S6=[C4] |
| 7 | Move S2→S4 B1；Complete/Home B@S4；AutoHome A@S1；run=1,Count=1 | 3 / DBA / C | S1=[∅]；S2=[C]；S3=[C]；S4=[∅]；S5=[∅]；S6=[C4] |
| 8 | Move S2→S3 C1；run=1,Count=1 | 3 / DBA / C | S1=[∅]；S2=[∅]；S3=[C2]；S4=[∅]；S5=[∅]；S6=[C4] |
| 9 | Move S6→S3 C4；Complete/Home C@S3；run=4,Count=4 | 4 / DBCA / 全部完成 | S1=[∅]；S2=[∅]；S3=[∅]；S4=[∅]；S5=[∅]；S6=[∅] |


### 对照路线1（9步，非最优证明）

| 步 | 玩家动作/自动事件顺序 | 完成数 / 已归巢 / 当前 | 稳定后全部容器 |
| --- | --- | --- | --- |
| 1 | Move S1→S5 C4；Hatch D@S1；run=4,Count=4 | 0 / 无 / DB | S1=[A · D]；S2=[C · 蛋(B) · A4]；S3=[D5 · A]；S4=[B5 · C]；S5=[C4]；S6=[∅] |
| 2 | Move S1→S6 D1；run=1,Count=1 | 0 / 无 / DB | S1=[A]；S2=[C · 蛋(B) · A4]；S3=[D5 · A]；S4=[B5 · C]；S5=[C4]；S6=[D] |
| 3 | Move S3→S1 A1；run=1,Count=1 | 0 / 无 / DB | S1=[A2]；S2=[C · 蛋(B) · A4]；S3=[D5]；S4=[B5 · C]；S5=[C4]；S6=[D] |
| 4 | Move S3→S6 D3；run=5,Count=3 | 0 / 无 / DB | S1=[A2]；S2=[C · 蛋(B) · A4]；S3=[D2]；S4=[B5 · C]；S5=[C4]；S6=[D4] |
| 5 | Move S6→S3 D4；Complete/Home D@S3；run=4,Count=4 | 1 / D / BC | S1=[A2]；S2=[C · 蛋(B) · A4]；S3=[∅]；S4=[B5 · C]；S5=[C4]；S6=[∅] |
| 6 | Move S2→S1 A4；Hatch B@S2；Complete/Sleep A@S1；run=4,Count=4 | 2 / D / BC | S1=[A6][睡眠]；S2=[C · B]；S3=[∅]；S4=[B5 · C]；S5=[C4]；S6=[∅] |
| 7 | Move S4→S5 C1；run=1,Count=1 | 2 / D / BC | S1=[A6][睡眠]；S2=[C · B]；S3=[∅]；S4=[B5]；S5=[C5]；S6=[∅] |
| 8 | Move S2→S4 B1；Complete/Home B@S4；AutoHome A@S1；run=1,Count=1 | 3 / DBA / C | S1=[∅]；S2=[C]；S3=[∅]；S4=[∅]；S5=[C5]；S6=[∅] |
| 9 | Move S2→S5 C1；Complete/Home C@S5；run=1,Count=1 | 4 / DBCA / 全部完成 | S1=[∅]；S2=[∅]；S3=[∅]；S4=[∅]；S5=[∅]；S6=[∅] |


终态：全部目标已收集；全部容器无token，临时枝若启用已退役；物种每步守恒。项目最优证明/Unity/真人/设备/实际Undo存档均未验证。

## 关卡15

K3/P5；Q=`A,C,B`。两个简单一鸟堵头，K3舒缓；先B或先C都是七步。

| 固定ID | 容量 | 固定初始数组（墙→露端） | 类型/条件 |
| --- | --- | --- | --- |
| S1 | 6 | `A4 · 蛋(C) · B` | 普通枝 |
| S2 | 6 | `B4 · 蛋(A) · C` | 普通枝 |
| S3 | 6 | `C3 · A` | 普通枝 |
| S4 | 6 | `B` | 普通枝 |
| S5 | 6 | `C` | 普通枝 |

### 主路线（7步，非最优证明）

| 步 | 玩家动作/自动事件顺序 | 完成数 / 已归巢 / 当前 | 稳定后全部容器 |
| --- | --- | --- | --- |
| 1 | Move S1→S4 B1；Hatch C@S1；run=1,Count=1 | 0 / 无 / AC | S1=[A4 · C]；S2=[B4 · 蛋(A) · C]；S3=[C3 · A]；S4=[B2]；S5=[C] |
| 2 | Move S2→S5 C1；Hatch A@S2；run=1,Count=1 | 0 / 无 / AC | S1=[A4 · C]；S2=[B4 · A]；S3=[C3 · A]；S4=[B2]；S5=[C2] |
| 3 | Move S3→S2 A1；run=1,Count=1 | 0 / 无 / AC | S1=[A4 · C]；S2=[B4 · A2]；S3=[C3]；S4=[B2]；S5=[C2] |
| 4 | Move S1→S5 C1；run=1,Count=1 | 0 / 无 / AC | S1=[A4]；S2=[B4 · A2]；S3=[C3]；S4=[B2]；S5=[C3] |
| 5 | Move S3→S5 C3；Complete/Home C@S5；run=3,Count=3 | 1 / C / AB | S1=[A4]；S2=[B4 · A2]；S3=[∅]；S4=[B2]；S5=[∅] |
| 6 | Move S2→S1 A2；Complete/Home A@S1；run=2,Count=2 | 2 / AC / B | S1=[∅]；S2=[B4]；S3=[∅]；S4=[B2]；S5=[∅] |
| 7 | Move S4→S2 B2；Complete/Home B@S2；run=2,Count=2 | 3 / ACB / 全部完成 | S1=[∅]；S2=[∅]；S3=[∅]；S4=[∅]；S5=[∅] |


### 对照路线1（7步，非最优证明）

| 步 | 玩家动作/自动事件顺序 | 完成数 / 已归巢 / 当前 | 稳定后全部容器 |
| --- | --- | --- | --- |
| 1 | Move S2→S5 C1；Hatch A@S2；run=1,Count=1 | 0 / 无 / AC | S1=[A4 · 蛋(C) · B]；S2=[B4 · A]；S3=[C3 · A]；S4=[B]；S5=[C2] |
| 2 | Move S1→S4 B1；Hatch C@S1；run=1,Count=1 | 0 / 无 / AC | S1=[A4 · C]；S2=[B4 · A]；S3=[C3 · A]；S4=[B2]；S5=[C2] |
| 3 | Move S3→S2 A1；run=1,Count=1 | 0 / 无 / AC | S1=[A4 · C]；S2=[B4 · A2]；S3=[C3]；S4=[B2]；S5=[C2] |
| 4 | Move S1→S5 C1；run=1,Count=1 | 0 / 无 / AC | S1=[A4]；S2=[B4 · A2]；S3=[C3]；S4=[B2]；S5=[C3] |
| 5 | Move S3→S5 C3；Complete/Home C@S5；run=3,Count=3 | 1 / C / AB | S1=[A4]；S2=[B4 · A2]；S3=[∅]；S4=[B2]；S5=[∅] |
| 6 | Move S2→S1 A2；Complete/Home A@S1；run=2,Count=2 | 2 / AC / B | S1=[∅]；S2=[B4]；S3=[∅]；S4=[B2]；S5=[∅] |
| 7 | Move S4→S2 B2；Complete/Home B@S2；run=2,Count=2 | 3 / ACB / 全部完成 | S1=[∅]；S2=[∅]；S3=[∅]；S4=[∅]；S5=[∅] |


终态：全部目标已收集；全部容器无token，临时枝若启用已退役；物种每步守恒。项目最优证明/Unity/真人/设备/实际Undo存档均未验证。

## 关卡16

K4/P6；Q=`A,D,B,C`。首满A才开计数箱，无A藏箱自锁。

| 固定ID | 容量 | 固定初始数组（墙→露端） | 类型/条件 |
| --- | --- | --- | --- |
| S1 | 6 | `B3 · A3` | 普通枝 |
| S2 | 6 | `A3` | 普通枝 |
| S3 | 6 | `C3 · B3` | 普通枝；盖箱 RevealAfter=1 |
| S4 | 6 | `D3 · C3` | 普通枝 |
| S5 | 6 | `D3` | 普通枝 |
| S6 | 6 | `∅` | 普通枝 |

### 主路线（4步，非最优证明）

| 步 | 玩家动作/自动事件顺序 | 完成数 / 已归巢 / 当前 | 稳定后全部容器 |
| --- | --- | --- | --- |
| 1 | Move S1→S2 A3；Complete/Home A@S2；Reveal S3；run=3,Count=3 | 1 / A / DB | S1=[B3]；S2=[∅]；S3=[C3 · B3]；S4=[D3 · C3]；S5=[D3]；S6=[∅] |
| 2 | Move S3→S1 B3；Complete/Home B@S1；run=3,Count=3 | 2 / AB / DC | S1=[∅]；S2=[∅]；S3=[C3]；S4=[D3 · C3]；S5=[D3]；S6=[∅] |
| 3 | Move S4→S3 C3；Complete/Home C@S3；run=3,Count=3 | 3 / ABC / D | S1=[∅]；S2=[∅]；S3=[∅]；S4=[D3]；S5=[D3]；S6=[∅] |
| 4 | Move S5→S4 D3；Complete/Home D@S4；run=3,Count=3 | 4 / ADBC / 全部完成 | S1=[∅]；S2=[∅]；S3=[∅]；S4=[∅]；S5=[∅]；S6=[∅] |


终态：全部目标已收集；全部容器无token，临时枝若启用已退役；物种每步守恒。项目最优证明/Unity/真人/设备/实际Undo存档均未验证。

## 关卡17

K4/P6；Q=`B,D,C,A`。B先完成不触发D种类箱，D完成才开。

| 固定ID | 容量 | 固定初始数组（墙→露端） | 类型/条件 |
| --- | --- | --- | --- |
| S1 | 6 | `A3 · B3` | 普通枝 |
| S2 | 6 | `B3` | 普通枝 |
| S3 | 6 | `C3 · A3` | 普通枝；盖箱 RevealSpecies=D |
| S4 | 6 | `D3 · C3` | 普通枝 |
| S5 | 6 | `D3` | 普通枝 |
| S6 | 6 | `∅` | 普通枝 |

### 主路线（5步，非最优证明）

| 步 | 玩家动作/自动事件顺序 | 完成数 / 已归巢 / 当前 | 稳定后全部容器 |
| --- | --- | --- | --- |
| 1 | Move S1→S2 B3；Complete/Home B@S2；run=3,Count=3 | 1 / B / DC | S1=[A3]；S2=[∅]；S3=[C3 · A3][盖箱]；S4=[D3 · C3]；S5=[D3]；S6=[∅] |
| 2 | Move S4→S6 C3；run=3,Count=3 | 1 / B / DC | S1=[A3]；S2=[∅]；S3=[C3 · A3][盖箱]；S4=[D3]；S5=[D3]；S6=[C3] |
| 3 | Move S5→S4 D3；Complete/Home D@S4；Reveal S3；run=3,Count=3 | 2 / BD / CA | S1=[A3]；S2=[∅]；S3=[C3 · A3]；S4=[∅]；S5=[∅]；S6=[C3] |
| 4 | Move S3→S1 A3；Complete/Home A@S1；run=3,Count=3 | 3 / BDA / C | S1=[∅]；S2=[∅]；S3=[C3]；S4=[∅]；S5=[∅]；S6=[C3] |
| 5 | Move S6→S3 C3；Complete/Home C@S3；run=3,Count=3 | 4 / BDCA / 全部完成 | S1=[∅]；S2=[∅]；S3=[∅]；S4=[∅]；S5=[∅]；S6=[∅] |


终态：全部目标已收集；全部容器无token，临时枝若启用已退役；物种每步守恒。项目最优证明/Unity/真人/设备/实际Undo存档均未验证。

## 关卡18

K5/P7；Q=`A,B,D,C,E`。D先睡眠便满足两箱，本轮按固定数组先揭两箱；之后归巢不重计。

| 固定ID | 容量 | 固定初始数组（墙→露端） | 类型/条件 |
| --- | --- | --- | --- |
| S1 | 6 | `A3 · D3` | 普通枝 |
| S2 | 6 | `D3` | 普通枝 |
| S3 | 6 | `B3 · A3` | 普通枝；盖箱 RevealSpecies=D |
| S4 | 6 | `E3 · B3` | 普通枝；盖箱 RevealAfter=1 |
| S5 | 6 | `C3 · E3` | 普通枝 |
| S6 | 6 | `C3` | 普通枝 |
| S7 | 6 | `∅` | 普通枝 |

### 主路线（5步，非最优证明）

| 步 | 玩家动作/自动事件顺序 | 完成数 / 已归巢 / 当前 | 稳定后全部容器 |
| --- | --- | --- | --- |
| 1 | Move S1→S2 D3；Complete/Sleep D@S2；Reveal S3；Reveal S4；run=3,Count=3 | 1 / 无 / AB | S1=[A3]；S2=[D6][睡眠]；S3=[B3 · A3]；S4=[E3 · B3]；S5=[C3 · E3]；S6=[C3]；S7=[∅] |
| 2 | Move S3→S1 A3；Complete/Home A@S1；AutoHome D@S2；run=3,Count=3 | 2 / AD / BC | S1=[∅]；S2=[∅]；S3=[B3]；S4=[E3 · B3]；S5=[C3 · E3]；S6=[C3]；S7=[∅] |
| 3 | Move S4→S3 B3；Complete/Home B@S3；run=3,Count=3 | 3 / ABD / CE | S1=[∅]；S2=[∅]；S3=[∅]；S4=[E3]；S5=[C3 · E3]；S6=[C3]；S7=[∅] |
| 4 | Move S5→S4 E3；Complete/Home E@S4；run=3,Count=3 | 4 / ABDE / C | S1=[∅]；S2=[∅]；S3=[∅]；S4=[∅]；S5=[C3]；S6=[C3]；S7=[∅] |
| 5 | Move S6→S5 C3；Complete/Home C@S5；run=3,Count=3 | 5 / ABDCE / 全部完成 | S1=[∅]；S2=[∅]；S3=[∅]；S4=[∅]；S5=[∅]；S6=[∅]；S7=[∅] |


终态：全部目标已收集；全部容器无token，临时枝若启用已退役；物种每步守恒。项目最优证明/Unity/真人/设备/实际Undo存档均未验证。

## 关卡19

K5/P7；Q=`C,A,E,B,D`。开箱不孵内层蛋；B3移走才孵一颗A，再搬A才孵下一颗。

| 固定ID | 容量 | 固定初始数组（墙→露端） | 类型/条件 |
| --- | --- | --- | --- |
| S1 | 6 | `B3 · C3` | 普通枝 |
| S2 | 6 | `C3` | 普通枝 |
| S3 | 6 | `蛋(A)2 · B3` | 普通枝；盖箱 RevealAfter=1 |
| S4 | 6 | `A4 · D2` | 普通枝 |
| S5 | 6 | `D4 · E2` | 普通枝 |
| S6 | 6 | `E4` | 普通枝 |
| S7 | 6 | `∅` | 普通枝 |

### 主路线（7步，非最优证明）

| 步 | 玩家动作/自动事件顺序 | 完成数 / 已归巢 / 当前 | 稳定后全部容器 |
| --- | --- | --- | --- |
| 1 | Move S1→S2 C3；Complete/Home C@S2；Reveal S3；run=3,Count=3 | 1 / C / AE | S1=[B3]；S2=[∅]；S3=[蛋(A)2 · B3]；S4=[A4 · D2]；S5=[D4 · E2]；S6=[E4]；S7=[∅] |
| 2 | Move S3→S1 B3；Hatch A@S3；Complete/Sleep B@S1；run=3,Count=3 | 2 / C / AE | S1=[B6][睡眠]；S2=[∅]；S3=[蛋(A) · A]；S4=[A4 · D2]；S5=[D4 · E2]；S6=[E4]；S7=[∅] |
| 3 | Move S3→S2 A1；Hatch A@S3；run=1,Count=1 | 2 / C / AE | S1=[B6][睡眠]；S2=[A]；S3=[A]；S4=[A4 · D2]；S5=[D4 · E2]；S6=[E4]；S7=[∅] |
| 4 | Move S3→S2 A1；run=1,Count=1 | 2 / C / AE | S1=[B6][睡眠]；S2=[A2]；S3=[∅]；S4=[A4 · D2]；S5=[D4 · E2]；S6=[E4]；S7=[∅] |
| 5 | Move S5→S6 E2；Complete/Home E@S6；AutoHome B@S1；run=2,Count=2 | 3 / CEB / AD | S1=[∅]；S2=[A2]；S3=[∅]；S4=[A4 · D2]；S5=[D4]；S6=[∅]；S7=[∅] |
| 6 | Move S4→S5 D2；Complete/Home D@S5；run=2,Count=2 | 4 / CEBD / A | S1=[∅]；S2=[A2]；S3=[∅]；S4=[A4]；S5=[∅]；S6=[∅]；S7=[∅] |
| 7 | Move S4→S2 A4；Complete/Home A@S2；run=4,Count=4 | 5 / CAEBD / 全部完成 | S1=[∅]；S2=[∅]；S3=[∅]；S4=[∅]；S5=[∅]；S6=[∅]；S7=[∅] |


终态：全部目标已收集；全部容器无token，临时枝若启用已退役；物种每步守恒。项目最优证明/Unity/真人/设备/实际Undo存档均未验证。

## 关卡20

K4/P6；Q=`D,A,C,B`。唯一机关为箱，开箱C1先合C3，再接第三段C2；与16线性四段不同，舒缓分叉汇合。

| 固定ID | 容量 | 固定初始数组（墙→露端） | 类型/条件 |
| --- | --- | --- | --- |
| S1 | 6 | `C3 · D3` | 普通枝 |
| S2 | 6 | `D3` | 普通枝 |
| S3 | 6 | `B5 · C` | 普通枝；盖箱 RevealAfter=1 |
| S4 | 6 | `A3 · B` | 普通枝 |
| S5 | 6 | `A3 · C2` | 普通枝 |
| S6 | 6 | `∅` | 普通枝 |

### 主路线（5步，非最优证明）

| 步 | 玩家动作/自动事件顺序 | 完成数 / 已归巢 / 当前 | 稳定后全部容器 |
| --- | --- | --- | --- |
| 1 | Move S1→S2 D3；Complete/Home D@S2；Reveal S3；run=3,Count=3 | 1 / D / AC | S1=[C3]；S2=[∅]；S3=[B5 · C]；S4=[A3 · B]；S5=[A3 · C2]；S6=[∅] |
| 2 | Move S3→S1 C1；run=1,Count=1 | 1 / D / AC | S1=[C4]；S2=[∅]；S3=[B5]；S4=[A3 · B]；S5=[A3 · C2]；S6=[∅] |
| 3 | Move S5→S1 C2；Complete/Home C@S1；run=2,Count=2 | 2 / DC / AB | S1=[∅]；S2=[∅]；S3=[B5]；S4=[A3 · B]；S5=[A3]；S6=[∅] |
| 4 | Move S4→S3 B1；Complete/Home B@S3；run=1,Count=1 | 3 / DCB / A | S1=[∅]；S2=[∅]；S3=[∅]；S4=[A3]；S5=[A3]；S6=[∅] |
| 5 | Move S5→S4 A3；Complete/Home A@S4；run=3,Count=3 | 4 / DACB / 全部完成 | S1=[∅]；S2=[∅]；S3=[∅]；S4=[∅]；S5=[∅]；S6=[∅] |


终态：全部目标已收集；全部容器无token，临时枝若启用已退役；物种每步守恒。项目最优证明/Unity/真人/设备/实际Undo存档均未验证。
