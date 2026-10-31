# 第21–30关：固定候选与纸面完整回放

2026-10-31 · DESIGNED / INDEPENDENT_REPLAY_PASS / PENDING_GRAYBOX。这是本批**固定初态唯一权威**；[设计卡](../LEVEL_DESIGN.md)只拥有教学/节奏，[模拟入口](../LEVEL_SIMULATIONS.md)拥有记号与证据边界，[RULES](../spec/RULES.md)拥有算法。仅Markdown候选，未修改游戏关卡JSON。

数组末项外露，`蛋(A)`是一只固定A种蛋，普通`E`始终是E鸟。`A3`精确等于三只A，不是随机数量；展开原序分配token ID。未列机关不存在。Complete/Sleep与Complete/Home表示本次首次成组六只、完成数+1；AutoHome不重复加数。表内接收临时枝事件包含首次Armed或已Armed再次接收，具体三态已在完整快照标出。

每条主路/对照路由独立文档模型逐步校验合法性、锁run/Count、稳定化、每种六只守恒及全空终态；不是项目Core/Unity/最优/真人证书。下面数值是路线长度，不是solverMinMoves。固定内容不得运行时生成、换色或换seed。

## 关卡21

K4/P6；Q=`A,B,D,C`。临时枝首次借用，先归B再安全回流A退役。

| 固定ID | 容量 | 固定初始数组（墙→露端） | 类型/条件 |
| --- | --- | --- | --- |
| S1 | 6 | `B3 · A3` | 普通枝 |
| S2 | 6 | `A3` | 普通枝 |
| S3 | 6 | `C3 · B3` | 普通枝 |
| S4 | 6 | `D3 · C3` | 普通枝 |
| S5 | 6 | `D3` | 普通枝 |
| S6 | 4 | `∅` | 临时Unused |

### 主路线（5步，非最优证明）

| 步 | 玩家动作/自动事件顺序 | 完成数 / 已归巢 / 当前 | 稳定后全部容器 |
| --- | --- | --- | --- |
| 1 | Move S1→S6 A3；接收临时枝 S6；run=3,Count=3 | 0 / 无 / AB | S1=[B3]；S2=[A3]；S3=[C3 · B3]；S4=[D3 · C3]；S5=[D3]；S6=[A3][Armed] |
| 2 | Move S3→S1 B3；Complete/Home B@S1；run=3,Count=3 | 1 / B / AD | S1=[∅]；S2=[A3]；S3=[C3]；S4=[D3 · C3]；S5=[D3]；S6=[A3][Armed] |
| 3 | Move S6→S2 A3；Complete/Home A@S2；Retire S6；run=3,Count=3 | 2 / AB / DC | S1=[∅]；S2=[∅]；S3=[C3]；S4=[D3 · C3]；S5=[D3]；S6=[∅][Retired] |
| 4 | Move S4→S3 C3；Complete/Home C@S3；run=3,Count=3 | 3 / ABC / D | S1=[∅]；S2=[∅]；S3=[∅]；S4=[D3]；S5=[D3]；S6=[∅][Retired] |
| 5 | Move S5→S4 D3；Complete/Home D@S4；run=3,Count=3 | 4 / ABDC / 全部完成 | S1=[∅]；S2=[∅]；S3=[∅]；S4=[∅]；S5=[∅]；S6=[∅][Retired] |


终态：全部目标已收集；全部容器无token，临时枝若启用已退役；物种每步守恒。项目最优证明/Unity/真人/设备/实际Undo存档均未验证。

## 关卡22

K4/P6；Q=`B,A,C,D`。部分转出留一只，同种再次接收；普通短枝不消失。

| 固定ID | 容量 | 固定初始数组（墙→露端） | 类型/条件 |
| --- | --- | --- | --- |
| S1 | 6 | `D3 · A3` | 普通枝 |
| S2 | 6 | `B3 · A` | 普通枝 |
| S3 | 6 | `C3 · B3` | 普通枝 |
| S4 | 6 | `C3 · D3` | 普通枝 |
| S5 | 4 | `A2` | 普通枝 |
| S6 | 4 | `∅` | 临时Unused |

### 主路线（8步，非最优证明）

| 步 | 玩家动作/自动事件顺序 | 完成数 / 已归巢 / 当前 | 稳定后全部容器 |
| --- | --- | --- | --- |
| 1 | Move S1→S6 A3；接收临时枝 S6；run=3,Count=3 | 0 / 无 / BA | S1=[D3]；S2=[B3 · A]；S3=[C3 · B3]；S4=[C3 · D3]；S5=[A2]；S6=[A3][Armed] |
| 2 | Move S6→S5 A2；run=3,Count=2 | 0 / 无 / BA | S1=[D3]；S2=[B3 · A]；S3=[C3 · B3]；S4=[C3 · D3]；S5=[A4]；S6=[A][Armed] |
| 3 | Move S2→S6 A1；接收临时枝 S6；run=1,Count=1 | 0 / 无 / BA | S1=[D3]；S2=[B3]；S3=[C3 · B3]；S4=[C3 · D3]；S5=[A4]；S6=[A2][Armed] |
| 4 | Move S4→S1 D3；Complete/Sleep D@S1；run=3,Count=3 | 1 / 无 / BA | S1=[D6][睡眠]；S2=[B3]；S3=[C3 · B3]；S4=[C3]；S5=[A4]；S6=[A2][Armed] |
| 5 | Move S3→S2 B3；Complete/Home B@S2；run=3,Count=3 | 2 / B / AC | S1=[D6][睡眠]；S2=[∅]；S3=[C3]；S4=[C3]；S5=[A4]；S6=[A2][Armed] |
| 6 | Move S4→S3 C3；Complete/Home C@S3；AutoHome D@S1；run=3,Count=3 | 3 / BCD / A | S1=[∅]；S2=[∅]；S3=[∅]；S4=[∅]；S5=[A4]；S6=[A2][Armed] |
| 7 | Move S5→S1 A4；run=4,Count=4 | 3 / BCD / A | S1=[A4]；S2=[∅]；S3=[∅]；S4=[∅]；S5=[∅]；S6=[A2][Armed] |
| 8 | Move S6→S1 A2；Complete/Home A@S1；Retire S6；run=2,Count=2 | 4 / BACD / 全部完成 | S1=[∅]；S2=[∅]；S3=[∅]；S4=[∅]；S5=[∅]；S6=[∅][Retired] |


终态：全部目标已收集；全部容器无token，临时枝若启用已退役；物种每步守恒。项目最优证明/Unity/真人/设备/实际Undo存档均未验证。

## 关卡23

K4/P6；Q=`C,D,A,B`。普通短枝可清空后换种复用，临时枝全出就退役。

| 固定ID | 容量 | 固定初始数组（墙→露端） | 类型/条件 |
| --- | --- | --- | --- |
| S1 | 6 | `B3 · C3` | 普通枝 |
| S2 | 6 | `C3` | 普通枝 |
| S3 | 6 | `A3 · D3` | 普通枝 |
| S4 | 6 | `D3 · B3` | 普通枝 |
| S5 | 5 | `A3` | 普通枝 |
| S6 | 4 | `∅` | 临时Unused |

### 主路线（7步，非最优证明）

| 步 | 玩家动作/自动事件顺序 | 完成数 / 已归巢 / 当前 | 稳定后全部容器 |
| --- | --- | --- | --- |
| 1 | Move S5→S6 A3；接收临时枝 S6；run=3,Count=3 | 0 / 无 / CD | S1=[B3 · C3]；S2=[C3]；S3=[A3 · D3]；S4=[D3 · B3]；S5=[∅]；S6=[A3][Armed] |
| 2 | Move S1→S5 C3；run=3,Count=3 | 0 / 无 / CD | S1=[B3]；S2=[C3]；S3=[A3 · D3]；S4=[D3 · B3]；S5=[C3]；S6=[A3][Armed] |
| 3 | Move S5→S2 C3；Complete/Home C@S2；run=3,Count=3 | 1 / C / DA | S1=[B3]；S2=[∅]；S3=[A3 · D3]；S4=[D3 · B3]；S5=[∅]；S6=[A3][Armed] |
| 4 | Move S4→S5 B3；run=3,Count=3 | 1 / C / DA | S1=[B3]；S2=[∅]；S3=[A3 · D3]；S4=[D3]；S5=[B3]；S6=[A3][Armed] |
| 5 | Move S5→S1 B3；Complete/Sleep B@S1；run=3,Count=3 | 2 / C / DA | S1=[B6][睡眠]；S2=[∅]；S3=[A3 · D3]；S4=[D3]；S5=[∅]；S6=[A3][Armed] |
| 6 | Move S3→S4 D3；Complete/Home D@S4；AutoHome B@S1；run=3,Count=3 | 3 / CDB / A | S1=[∅]；S2=[∅]；S3=[A3]；S4=[∅]；S5=[∅]；S6=[A3][Armed] |
| 7 | Move S6→S3 A3；Complete/Home A@S3；Retire S6；run=3,Count=3 | 4 / CDAB / 全部完成 | S1=[∅]；S2=[∅]；S3=[∅]；S4=[∅]；S5=[∅]；S6=[∅][Retired] |


终态：全部目标已收集；全部容器无token，临时枝若启用已退役；物种每步守恒。项目最优证明/Unity/真人/设备/实际Undo存档均未验证。

## 关卡24

K4/P6；Q=`D,B,A,C`。双篮同时单鸟暂存，临时枝接一段+一只再回流。

| 固定ID | 容量 | 固定初始数组（墙→露端） | 类型/条件 |
| --- | --- | --- | --- |
| S1 | 6 | `A3 · D2 · B` | 普通枝 |
| S2 | 6 | `D4 · C` | 普通枝 |
| S3 | 6 | `B5 · A` | 普通枝 |
| S4 | 6 | `C5 · A` | 普通枝 |
| S5 | 6 | `A` | 普通枝 |
| S6 | 5 | `∅` | 临时Unused |
| T1 | 1 | `∅` | 篮 |
| T2 | 1 | `∅` | 篮 |

### 主路线（10步，非最优证明）

| 步 | 玩家动作/自动事件顺序 | 完成数 / 已归巢 / 当前 | 稳定后全部容器 |
| --- | --- | --- | --- |
| 1 | Move S1→T1 B1；run=1,Count=1 | 0 / 无 / DB | S1=[A3 · D2]；S2=[D4 · C]；S3=[B5 · A]；S4=[C5 · A]；S5=[A]；S6=[∅][Unused]；T1=[B]；T2=[∅] |
| 2 | Move S2→T2 C1；run=1,Count=1 | 0 / 无 / DB | S1=[A3 · D2]；S2=[D4]；S3=[B5 · A]；S4=[C5 · A]；S5=[A]；S6=[∅][Unused]；T1=[B]；T2=[C] |
| 3 | Move S1→S2 D2；Complete/Home D@S2；run=2,Count=2 | 1 / D / BA | S1=[A3]；S2=[∅]；S3=[B5 · A]；S4=[C5 · A]；S5=[A]；S6=[∅][Unused]；T1=[B]；T2=[C] |
| 4 | Move S1→S6 A3；接收临时枝 S6；run=3,Count=3 | 1 / D / BA | S1=[∅]；S2=[∅]；S3=[B5 · A]；S4=[C5 · A]；S5=[A]；S6=[A3][Armed]；T1=[B]；T2=[C] |
| 5 | Move S3→S6 A1；接收临时枝 S6；run=1,Count=1 | 1 / D / BA | S1=[∅]；S2=[∅]；S3=[B5]；S4=[C5 · A]；S5=[A]；S6=[A4][Armed]；T1=[B]；T2=[C] |
| 6 | Move T1→S3 B1；Complete/Home B@S3；run=1,Count=1 | 2 / DB / AC | S1=[∅]；S2=[∅]；S3=[∅]；S4=[C5 · A]；S5=[A]；S6=[A4][Armed]；T1=[∅]；T2=[C] |
| 7 | Move S4→S1 A1；run=1,Count=1 | 2 / DB / AC | S1=[A]；S2=[∅]；S3=[∅]；S4=[C5]；S5=[A]；S6=[A4][Armed]；T1=[∅]；T2=[C] |
| 8 | Move S5→S1 A1；run=1,Count=1 | 2 / DB / AC | S1=[A2]；S2=[∅]；S3=[∅]；S4=[C5]；S5=[∅]；S6=[A4][Armed]；T1=[∅]；T2=[C] |
| 9 | Move S6→S1 A4；Complete/Home A@S1；Retire S6；run=4,Count=4 | 3 / DBA / C | S1=[∅]；S2=[∅]；S3=[∅]；S4=[C5]；S5=[∅]；S6=[∅][Retired]；T1=[∅]；T2=[C] |
| 10 | Move T2→S4 C1；Complete/Home C@S4；run=1,Count=1 | 4 / DBAC / 全部完成 | S1=[∅]；S2=[∅]；S3=[∅]；S4=[∅]；S5=[∅]；S6=[∅][Retired]；T1=[∅]；T2=[∅] |


终态：全部目标已收集；全部容器无token，临时枝若启用已退役；物种每步守恒。项目最优证明/Unity/真人/设备/实际Undo存档均未验证。

## 关卡25

K3/P5；Q=`A,B,C`。丢四座临时枝，换六座长枝清空；反驳永远留一只。

| 固定ID | 容量 | 固定初始数组（墙→露端） | 类型/条件 |
| --- | --- | --- | --- |
| S1 | 6 | `A5` | 普通枝 |
| S2 | 6 | `B3 · A` | 普通枝 |
| S3 | 6 | `C3 · B3` | 普通枝 |
| S4 | 6 | `C3` | 普通枝 |
| S5 | 4 | `∅` | 临时Unused |

### 主路线（4步，非最优证明）

| 步 | 玩家动作/自动事件顺序 | 完成数 / 已归巢 / 当前 | 稳定后全部容器 |
| --- | --- | --- | --- |
| 1 | Move S2→S5 A1；接收临时枝 S5；run=1,Count=1 | 0 / 无 / AB | S1=[A5]；S2=[B3]；S3=[C3 · B3]；S4=[C3]；S5=[A][Armed] |
| 2 | Move S5→S1 A1；Complete/Home A@S1；Retire S5；run=1,Count=1 | 1 / A / BC | S1=[∅]；S2=[B3]；S3=[C3 · B3]；S4=[C3]；S5=[∅][Retired] |
| 3 | Move S3→S2 B3；Complete/Home B@S2；run=3,Count=3 | 2 / AB / C | S1=[∅]；S2=[∅]；S3=[C3]；S4=[C3]；S5=[∅][Retired] |
| 4 | Move S4→S3 C3；Complete/Home C@S3；run=3,Count=3 | 3 / ABC / 全部完成 | S1=[∅]；S2=[∅]；S3=[∅]；S4=[∅]；S5=[∅][Retired] |


终态：全部目标已收集；全部容器无token，临时枝若启用已退役；物种每步守恒。项目最优证明/Unity/真人/设备/实际Undo存档均未验证。

## 关卡26

K5/P7；Q=`A,C,E,B,D`。只接一次且第二蛋前退役；不用临时枝五步更短，生命周期可选示范。

| 固定ID | 容量 | 固定初始数组（墙→露端） | 类型/条件 |
| --- | --- | --- | --- |
| S1 | 6 | `B4 · 蛋(B) · A` | 普通枝 |
| S2 | 6 | `C4 · 蛋(C) · D` | 普通枝 |
| S3 | 6 | `A5` | 普通枝 |
| S4 | 6 | `D5` | 普通枝 |
| S5 | 6 | `E5 · C` | 普通枝 |
| S6 | 6 | `B · E` | 普通枝 |
| S7 | 4 | `∅` | 临时Unused |

### 主路线（6步，非最优证明）

| 步 | 玩家动作/自动事件顺序 | 完成数 / 已归巢 / 当前 | 稳定后全部容器 |
| --- | --- | --- | --- |
| 1 | Move S1→S7 A1；接收临时枝 S7；Hatch B@S1；run=1,Count=1 | 0 / 无 / AC | S1=[B5]；S2=[C4 · 蛋(C) · D]；S3=[A5]；S4=[D5]；S5=[E5 · C]；S6=[B · E]；S7=[A][Armed] |
| 2 | Move S7→S3 A1；Complete/Home A@S3；Retire S7；run=1,Count=1 | 1 / A / CE | S1=[B5]；S2=[C4 · 蛋(C) · D]；S3=[∅]；S4=[D5]；S5=[E5 · C]；S6=[B · E]；S7=[∅][Retired] |
| 3 | Move S2→S4 D1；Hatch C@S2；Complete/Sleep D@S4；run=1,Count=1 | 2 / A / CE | S1=[B5]；S2=[C5]；S3=[∅]；S4=[D6][睡眠]；S5=[E5 · C]；S6=[B · E]；S7=[∅][Retired] |
| 4 | Move S5→S2 C1；Complete/Home C@S2；run=1,Count=1 | 3 / AC / EB | S1=[B5]；S2=[∅]；S3=[∅]；S4=[D6][睡眠]；S5=[E5]；S6=[B · E]；S7=[∅][Retired] |
| 5 | Move S6→S5 E1；Complete/Home E@S5；AutoHome D@S4；run=1,Count=1 | 4 / ACED / B | S1=[B5]；S2=[∅]；S3=[∅]；S4=[∅]；S5=[∅]；S6=[B]；S7=[∅][Retired] |
| 6 | Move S6→S1 B1；Complete/Home B@S1；run=1,Count=1 | 5 / ACEBD / 全部完成 | S1=[∅]；S2=[∅]；S3=[∅]；S4=[∅]；S5=[∅]；S6=[∅]；S7=[∅][Retired] |


### 对照路线1（5步，非最优证明）

| 步 | 玩家动作/自动事件顺序 | 完成数 / 已归巢 / 当前 | 稳定后全部容器 |
| --- | --- | --- | --- |
| 1 | Move S1→S3 A1；Hatch B@S1；Complete/Home A@S3；run=1,Count=1 | 1 / A / CE | S1=[B5]；S2=[C4 · 蛋(C) · D]；S3=[∅]；S4=[D5]；S5=[E5 · C]；S6=[B · E]；S7=[∅][Unused] |
| 2 | Move S2→S4 D1；Hatch C@S2；Complete/Sleep D@S4；run=1,Count=1 | 2 / A / CE | S1=[B5]；S2=[C5]；S3=[∅]；S4=[D6][睡眠]；S5=[E5 · C]；S6=[B · E]；S7=[∅][Unused] |
| 3 | Move S5→S2 C1；Complete/Home C@S2；run=1,Count=1 | 3 / AC / EB | S1=[B5]；S2=[∅]；S3=[∅]；S4=[D6][睡眠]；S5=[E5]；S6=[B · E]；S7=[∅][Unused] |
| 4 | Move S6→S5 E1；Complete/Home E@S5；AutoHome D@S4；run=1,Count=1 | 4 / ACED / B | S1=[B5]；S2=[∅]；S3=[∅]；S4=[∅]；S5=[∅]；S6=[B]；S7=[∅][Unused] |
| 5 | Move S6→S1 B1；Complete/Home B@S1；run=1,Count=1 | 5 / ACEBD / 全部完成 | S1=[∅]；S2=[∅]；S3=[∅]；S4=[∅]；S5=[∅]；S6=[∅]；S7=[∅][Unused] |


终态：全部目标已收集；全部容器无token，临时枝若启用已退役；物种每步守恒。项目最优证明/Unity/真人/设备/实际Undo存档均未验证。

## 关卡27

K5/P7；Q=`B,D,A,E,C`。第一满组A睡眠计数1；B使计数2开箱，E等待接回再退役。

| 固定ID | 容量 | 固定初始数组（墙→露端） | 类型/条件 |
| --- | --- | --- | --- |
| S1 | 6 | `B5 · A` | 普通枝 |
| S2 | 6 | `D5 · B` | 普通枝 |
| S3 | 6 | `E5 · D` | 普通枝；盖箱 RevealAfter=2 |
| S4 | 6 | `A5` | 普通枝 |
| S5 | 6 | `C5 · E` | 普通枝 |
| S6 | 6 | `C` | 普通枝 |
| S7 | 5 | `∅` | 临时Unused |

### 主路线（6步，非最优证明）

| 步 | 玩家动作/自动事件顺序 | 完成数 / 已归巢 / 当前 | 稳定后全部容器 |
| --- | --- | --- | --- |
| 1 | Move S1→S4 A1；Complete/Sleep A@S4；run=1,Count=1 | 1 / 无 / BD | S1=[B5]；S2=[D5 · B]；S3=[E5 · D][盖箱]；S4=[A6][睡眠]；S5=[C5 · E]；S6=[C]；S7=[∅][Unused] |
| 2 | Move S5→S7 E1；接收临时枝 S7；run=1,Count=1 | 1 / 无 / BD | S1=[B5]；S2=[D5 · B]；S3=[E5 · D][盖箱]；S4=[A6][睡眠]；S5=[C5]；S6=[C]；S7=[E][Armed] |
| 3 | Move S2→S1 B1；Complete/Home B@S1；AutoHome A@S4；Reveal S3；run=1,Count=1 | 2 / BA / DE | S1=[∅]；S2=[D5]；S3=[E5 · D]；S4=[∅]；S5=[C5]；S6=[C]；S7=[E][Armed] |
| 4 | Move S3→S2 D1；Complete/Home D@S2；run=1,Count=1 | 3 / BDA / EC | S1=[∅]；S2=[∅]；S3=[E5]；S4=[∅]；S5=[C5]；S6=[C]；S7=[E][Armed] |
| 5 | Move S7→S3 E1；Complete/Home E@S3；Retire S7；run=1,Count=1 | 4 / BDAE / C | S1=[∅]；S2=[∅]；S3=[∅]；S4=[∅]；S5=[C5]；S6=[C]；S7=[∅][Retired] |
| 6 | Move S6→S5 C1；Complete/Home C@S5；run=1,Count=1 | 5 / BDAEC / 全部完成 | S1=[∅]；S2=[∅]；S3=[∅]；S4=[∅]；S5=[∅]；S6=[∅]；S7=[∅][Retired] |


终态：全部目标已收集；全部容器无token，临时枝若启用已退役；物种每步守恒。项目最优证明/Unity/真人/设备/实际Undo存档均未验证。

## 关卡28

K5/P7；Q=`C,E,A,D,B`。选对暂存对象：先睡A、临时存C六步；存A八步。旧不要早睡A结论撤销。

| 固定ID | 容量 | 固定初始数组（墙→露端） | 类型/条件 |
| --- | --- | --- | --- |
| S1 | 6 | `B3 · A3` | 普通枝 |
| S2 | 6 | `E3 · C3` | 普通枝 |
| S3 | 6 | `A3` | 普通枝 |
| S4 | 6 | `D3 · E3` | 普通枝 |
| S5 | 6 | `C3 · D3` | 普通枝 |
| S6 | 6 | `B3` | 普通枝 |
| S7 | 4 | `∅` | 临时Unused |

### 主路线（6步，非最优证明）

| 步 | 玩家动作/自动事件顺序 | 完成数 / 已归巢 / 当前 | 稳定后全部容器 |
| --- | --- | --- | --- |
| 1 | Move S1→S3 A3；Complete/Sleep A@S3；run=3,Count=3 | 1 / 无 / CE | S1=[B3]；S2=[E3 · C3]；S3=[A6][睡眠]；S4=[D3 · E3]；S5=[C3 · D3]；S6=[B3]；S7=[∅][Unused] |
| 2 | Move S2→S7 C3；接收临时枝 S7；run=3,Count=3 | 1 / 无 / CE | S1=[B3]；S2=[E3]；S3=[A6][睡眠]；S4=[D3 · E3]；S5=[C3 · D3]；S6=[B3]；S7=[C3][Armed] |
| 3 | Move S4→S2 E3；Complete/Home E@S2；AutoHome A@S3；run=3,Count=3 | 2 / EA / CD | S1=[B3]；S2=[∅]；S3=[∅]；S4=[D3]；S5=[C3 · D3]；S6=[B3]；S7=[C3][Armed] |
| 4 | Move S5→S4 D3；Complete/Home D@S4；run=3,Count=3 | 3 / EAD / CB | S1=[B3]；S2=[∅]；S3=[∅]；S4=[∅]；S5=[C3]；S6=[B3]；S7=[C3][Armed] |
| 5 | Move S7→S5 C3；Complete/Home C@S5；Retire S7；run=3,Count=3 | 4 / CEAD / B | S1=[B3]；S2=[∅]；S3=[∅]；S4=[∅]；S5=[∅]；S6=[B3]；S7=[∅][Retired] |
| 6 | Move S1→S6 B3；Complete/Home B@S6；run=3,Count=3 | 5 / CEADB / 全部完成 | S1=[∅]；S2=[∅]；S3=[∅]；S4=[∅]；S5=[∅]；S6=[∅]；S7=[∅][Retired] |


### 对照路线1（8步，非最优证明）

| 步 | 玩家动作/自动事件顺序 | 完成数 / 已归巢 / 当前 | 稳定后全部容器 |
| --- | --- | --- | --- |
| 1 | Move S3→S7 A3；接收临时枝 S7；run=3,Count=3 | 0 / 无 / CE | S1=[B3 · A3]；S2=[E3 · C3]；S3=[∅]；S4=[D3 · E3]；S5=[C3 · D3]；S6=[B3]；S7=[A3][Armed] |
| 2 | Move S2→S3 C3；run=3,Count=3 | 0 / 无 / CE | S1=[B3 · A3]；S2=[E3]；S3=[C3]；S4=[D3 · E3]；S5=[C3 · D3]；S6=[B3]；S7=[A3][Armed] |
| 3 | Move S4→S2 E3；Complete/Home E@S2；run=3,Count=3 | 1 / E / CA | S1=[B3 · A3]；S2=[∅]；S3=[C3]；S4=[D3]；S5=[C3 · D3]；S6=[B3]；S7=[A3][Armed] |
| 4 | Move S7→S2 A3；Retire S7；run=3,Count=3 | 1 / E / CA | S1=[B3 · A3]；S2=[A3]；S3=[C3]；S4=[D3]；S5=[C3 · D3]；S6=[B3]；S7=[∅][Retired] |
| 5 | Move S1→S2 A3；Complete/Home A@S2；run=3,Count=3 | 2 / EA / CD | S1=[B3]；S2=[∅]；S3=[C3]；S4=[D3]；S5=[C3 · D3]；S6=[B3]；S7=[∅][Retired] |
| 6 | Move S5→S4 D3；Complete/Home D@S4；run=3,Count=3 | 3 / EAD / CB | S1=[B3]；S2=[∅]；S3=[C3]；S4=[∅]；S5=[C3]；S6=[B3]；S7=[∅][Retired] |
| 7 | Move S5→S3 C3；Complete/Home C@S3；run=3,Count=3 | 4 / CEAD / B | S1=[B3]；S2=[∅]；S3=[∅]；S4=[∅]；S5=[∅]；S6=[B3]；S7=[∅][Retired] |
| 8 | Move S1→S6 B3；Complete/Home B@S6；run=3,Count=3 | 5 / CEADB / 全部完成 | S1=[∅]；S2=[∅]；S3=[∅]；S4=[∅]；S5=[∅]；S6=[∅]；S7=[∅][Retired] |


终态：全部目标已收集；全部容器无token，临时枝若启用已退役；物种每步守恒。项目最优证明/Unity/真人/设备/实际Undo存档均未验证。

## 关卡29

K5/P7；Q=`D,A,E,C,B`。同篮先A后E复用，释放三个固定蛋。

| 固定ID | 容量 | 固定初始数组（墙→露端） | 类型/条件 |
| --- | --- | --- | --- |
| S1 | 6 | `D4 · 蛋(D) · A` | 普通枝 |
| S2 | 6 | `A4 · 蛋(A) · E` | 普通枝 |
| S3 | 6 | `E4 · 蛋(E) · C` | 普通枝 |
| S4 | 6 | `C5 · B` | 普通枝 |
| S5 | 6 | `B5 · D` | 普通枝 |
| S6 | 6 | `∅` | 普通枝 |
| S7 | 6 | `∅` | 普通枝 |
| T1 | 1 | `∅` | 篮 |

### 主路线（10步，非最优证明）

| 步 | 玩家动作/自动事件顺序 | 完成数 / 已归巢 / 当前 | 稳定后全部容器 |
| --- | --- | --- | --- |
| 1 | Move S1→T1 A1；Hatch D@S1；run=1,Count=1 | 0 / 无 / DA | S1=[D5]；S2=[A4 · 蛋(A) · E]；S3=[E4 · 蛋(E) · C]；S4=[C5 · B]；S5=[B5 · D]；S6=[∅]；S7=[∅]；T1=[A] |
| 2 | Move S5→S1 D1；Complete/Home D@S1；run=1,Count=1 | 1 / D / AE | S1=[∅]；S2=[A4 · 蛋(A) · E]；S3=[E4 · 蛋(E) · C]；S4=[C5 · B]；S5=[B5]；S6=[∅]；S7=[∅]；T1=[A] |
| 3 | Move T1→S1 A1；run=1,Count=1 | 1 / D / AE | S1=[A]；S2=[A4 · 蛋(A) · E]；S3=[E4 · 蛋(E) · C]；S4=[C5 · B]；S5=[B5]；S6=[∅]；S7=[∅]；T1=[∅] |
| 4 | Move S2→T1 E1；Hatch A@S2；run=1,Count=1 | 1 / D / AE | S1=[A]；S2=[A5]；S3=[E4 · 蛋(E) · C]；S4=[C5 · B]；S5=[B5]；S6=[∅]；S7=[∅]；T1=[E] |
| 5 | Move S1→S2 A1；Complete/Home A@S2；run=1,Count=1 | 2 / DA / EC | S1=[∅]；S2=[∅]；S3=[E4 · 蛋(E) · C]；S4=[C5 · B]；S5=[B5]；S6=[∅]；S7=[∅]；T1=[E] |
| 6 | Move T1→S1 E1；run=1,Count=1 | 2 / DA / EC | S1=[E]；S2=[∅]；S3=[E4 · 蛋(E) · C]；S4=[C5 · B]；S5=[B5]；S6=[∅]；S7=[∅]；T1=[∅] |
| 7 | Move S3→S6 C1；Hatch E@S3；run=1,Count=1 | 2 / DA / EC | S1=[E]；S2=[∅]；S3=[E5]；S4=[C5 · B]；S5=[B5]；S6=[C]；S7=[∅]；T1=[∅] |
| 8 | Move S1→S3 E1；Complete/Home E@S3；run=1,Count=1 | 3 / DAE / CB | S1=[∅]；S2=[∅]；S3=[∅]；S4=[C5 · B]；S5=[B5]；S6=[C]；S7=[∅]；T1=[∅] |
| 9 | Move S4→S5 B1；Complete/Home B@S5；run=1,Count=1 | 4 / DAEB / C | S1=[∅]；S2=[∅]；S3=[∅]；S4=[C5]；S5=[∅]；S6=[C]；S7=[∅]；T1=[∅] |
| 10 | Move S6→S4 C1；Complete/Home C@S4；run=1,Count=1 | 5 / DAECB / 全部完成 | S1=[∅]；S2=[∅]；S3=[∅]；S4=[∅]；S5=[∅]；S6=[∅]；S7=[∅]；T1=[∅] |


终态：全部目标已收集；全部容器无token，临时枝若启用已退役；物种每步守恒。项目最优证明/Unity/真人/设备/实际Undo存档均未验证。

## 关卡30

K5/P7；Q=`E,B,A,D,C`。无特殊大run舒缓环；复用基础骨架，不宣称独特新机制。

| 固定ID | 容量 | 固定初始数组（墙→露端） | 类型/条件 |
| --- | --- | --- | --- |
| S1 | 6 | `E3 · B3` | 普通枝 |
| S2 | 6 | `B3 · A3` | 普通枝 |
| S3 | 6 | `A3 · D3` | 普通枝 |
| S4 | 6 | `D3 · C3` | 普通枝 |
| S5 | 6 | `C3 · E3` | 普通枝 |
| S6 | 6 | `∅` | 普通枝 |
| S7 | 6 | `∅` | 普通枝 |

### 主路线（6步，非最优证明）

| 步 | 玩家动作/自动事件顺序 | 完成数 / 已归巢 / 当前 | 稳定后全部容器 |
| --- | --- | --- | --- |
| 1 | Move S1→S6 B3；run=3,Count=3 | 0 / 无 / EB | S1=[E3]；S2=[B3 · A3]；S3=[A3 · D3]；S4=[D3 · C3]；S5=[C3 · E3]；S6=[B3]；S7=[∅] |
| 2 | Move S5→S1 E3；Complete/Home E@S1；run=3,Count=3 | 1 / E / BA | S1=[∅]；S2=[B3 · A3]；S3=[A3 · D3]；S4=[D3 · C3]；S5=[C3]；S6=[B3]；S7=[∅] |
| 3 | Move S4→S5 C3；Complete/Sleep C@S5；run=3,Count=3 | 2 / E / BA | S1=[∅]；S2=[B3 · A3]；S3=[A3 · D3]；S4=[D3]；S5=[C6][睡眠]；S6=[B3]；S7=[∅] |
| 4 | Move S3→S4 D3；Complete/Sleep D@S4；run=3,Count=3 | 3 / E / BA | S1=[∅]；S2=[B3 · A3]；S3=[A3]；S4=[D6][睡眠]；S5=[C6][睡眠]；S6=[B3]；S7=[∅] |
| 5 | Move S2→S3 A3；Complete/Home A@S3；AutoHome D@S4；AutoHome C@S5；run=3,Count=3 | 4 / EADC / B | S1=[∅]；S2=[B3]；S3=[∅]；S4=[∅]；S5=[∅]；S6=[B3]；S7=[∅] |
| 6 | Move S6→S2 B3；Complete/Home B@S2；run=3,Count=3 | 5 / EBADC / 全部完成 | S1=[∅]；S2=[∅]；S3=[∅]；S4=[∅]；S5=[∅]；S6=[∅]；S7=[∅] |


### 对照路线1（6步，非最优证明）

| 步 | 玩家动作/自动事件顺序 | 完成数 / 已归巢 / 当前 | 稳定后全部容器 |
| --- | --- | --- | --- |
| 1 | Move S5→S6 E3；run=3,Count=3 | 0 / 无 / EB | S1=[E3 · B3]；S2=[B3 · A3]；S3=[A3 · D3]；S4=[D3 · C3]；S5=[C3]；S6=[E3]；S7=[∅] |
| 2 | Move S4→S5 C3；Complete/Sleep C@S5；run=3,Count=3 | 1 / 无 / EB | S1=[E3 · B3]；S2=[B3 · A3]；S3=[A3 · D3]；S4=[D3]；S5=[C6][睡眠]；S6=[E3]；S7=[∅] |
| 3 | Move S3→S4 D3；Complete/Sleep D@S4；run=3,Count=3 | 2 / 无 / EB | S1=[E3 · B3]；S2=[B3 · A3]；S3=[A3]；S4=[D6][睡眠]；S5=[C6][睡眠]；S6=[E3]；S7=[∅] |
| 4 | Move S2→S3 A3；Complete/Sleep A@S3；run=3,Count=3 | 3 / 无 / EB | S1=[E3 · B3]；S2=[B3]；S3=[A6][睡眠]；S4=[D6][睡眠]；S5=[C6][睡眠]；S6=[E3]；S7=[∅] |
| 5 | Move S1→S2 B3；Complete/Home B@S2；AutoHome A@S3；AutoHome D@S4；AutoHome C@S5；run=3,Count=3 | 4 / BADC / E | S1=[E3]；S2=[∅]；S3=[∅]；S4=[∅]；S5=[∅]；S6=[E3]；S7=[∅] |
| 6 | Move S6→S1 E3；Complete/Home E@S1；run=3,Count=3 | 5 / EBADC / 全部完成 | S1=[∅]；S2=[∅]；S3=[∅]；S4=[∅]；S5=[∅]；S6=[∅]；S7=[∅] |


终态：全部目标已收集；全部容器无token，临时枝若启用已退役；物种每步守恒。项目最优证明/Unity/真人/设备/实际Undo存档均未验证。
