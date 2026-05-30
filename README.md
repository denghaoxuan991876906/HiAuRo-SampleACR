# HiAuRo-SampleACR

HiAuRo ACR 开发模板项目——克隆之后改类名和技能 ID 就能开始写你的职业循环。

## 前置要求

- **.NET 10.0 SDK**（[下载](https://dotnet.microsoft.com/download/dotnet/10.0)）
- **Dalamud SDK 15.0**（安装 XIVLauncherCN 后自动配置）
- **IDE**：Rider / VS2022+ / VS Code（推荐 Rider）

> 本项目面向 **Windows** 平台编译和运行。WSL 下开发需要通过 `cmd.exe /c` 转发构建命令。

## 项目结构

```
HiAuRo-SampleACR/
├── GlobalUsings.cs          # 全局 using：HiAuRo + OmenTools + Dalamud
├── SampleACR.csproj         # 项目文件（引用 HiAuRo.Sdk + HiAuRo.Helper）
├── SampleACR.sln            # 解决方案
│
├── Helper/                  # [Git Submodule] 全职业数据辅助库
│   └── HiAuRo.Helper/       # WAR/BRD/SAM/... 等 20 职业的静态 Helper
│
└── SampleJob/               # ← 你的 ACR 代码都在这里
    ├── SampleEntry.cs         # ① 入口类：实现 IRotationEntry（框架从这里加载）
    ├── SampleEventControl.cs  # ② 事件回调：IRotationEventHandler（战斗/阶段/游戏事件）
    ├── SamlpeSettings.cs     # ③ 设置类：继承 AcrSettings（自动持久化为 JSON）
    │
    ├── SoltResolver/          # ④ 技能槽位（写循环的地方）
    │   ├── SampleAlways.cs      # SlotMode.Always 示例（不限窗口，随时执行）
    │   ├── GCD/
    │   │   └── SampleGCD.cs     # SlotMode.Gcd 示例（GCD 冷却好时执行）
    │   └── Ability/
    │       └── SampleAbility.cs # SlotMode.OffGcd 示例（能力技窗口执行）
    │
    └── UI/
        └── SampleUI.cs        # ⑤ 声明式 UI 注册（QT 开关、热键、悬浮窗）
```

### 各文件职责

| 文件 | 必须？ | 说明 |
|------|-------|------|
| `SampleEntry.cs` | 必须 | HiAuRo 框架通过 `IRotationEntry` 发现 ACR。在这里注册 SlotResolver、EventHandler、Settings |
| `SampleEventControl.cs` | 推荐 | 12 个事件回调（OnBattleUpdate/OnGameEvent/OnPhaseChanged 等），ACR 所有非技能逻辑写在这里 |
| `SamlpeSettings.cs` | 推荐 | 继承 `AcrSettings`，公开属性自动保存到 JSON |
| `SoltResolver/*.cs` | 必须 | 每个技能一个 Resolver 类，实现 `Check()` + `Build()` |
| `SampleUI.cs` | 推荐 | 声明式注册 QT/BuiltinQt/Hotkey/Tab 到 HiAuRo 的 Web 前端 |

## 快速开始

### 1. 克隆项目（含 submodule）

```bash
git clone --recurse-submodules https://github.com/denghaoxuan991876906/HiAuRo-SampleACR.git
cd HiAuRo-SampleACR
```

如果已经克隆了主仓库但缺少 Helper：

```bash
git submodule update --init --recursive
```

### 2. 构建

**Windows：**

```bash
dotnet build SampleACR.sln -c Debug
```

**WSL2（通过 Windows dotnet）：**

```bash
cmd.exe /c "dotnet build D:\DalamudPlugins\HiAuRo-SampleACR\SampleACR.sln -c Debug"
```

构建成功后，DLL 输出在 `SampleJob/bin/Debug/net10.0-windows/`。

### 3. 部署到 HiAuRo

将编译好的 DLL 放到 HiAuRo 插件配置目录的 `ACR/<你的作者名>/` 下。HiAuRo 启动时会自动发现并加载。

### 4. 更新 SDK 版本

`SampleACR.csproj` 中的 `HiAuRo.Sdk` 版本号需要与 HiAuRo 框架版本匹配。检查 HiAuRo 的当前版本后更新：

```xml
<PackageReference Include="HiAuRo.Sdk" Version="0.1.XX">
    <ExcludeAssets>runtime</ExcludeAssets>
</PackageReference>
```

## 创建你自己的 ACR

### 第一步：复制并重命名

```
SampleJob/ → BRDJob/（以吟游诗人为例）
  SamlpeSettings.cs → BRDSettings.cs
  SampleEntry.cs    → BRDEntry.cs
  SampleEventControl.cs → BRDEventControl.cs
  SoltResolver/     → SlotResolvers/（可以改回正确拼写）
  UI/               → 保留
```

### 第二步：改 IRotationEntry

```csharp
public class BRDEntry : IRotationEntry, ISettingsProvider<BRDSettings>
{
    public string AuthorName => "你的名字";
    public IEnumerable<Jobs> TargetJobs => [Jobs.BRD]; // ← 改成你的职业

    public Rotation? Build(string settingFolder)
    {
        var rot = new Rotation
        {
            TargetJob = Jobs.BRD,
            MinLevel = 1,
            MaxLevel = 100,
            Description = "吟游诗人 PvE ACR",
            SlotResolvers =
            [
                new() { Resolver = new BRD_GCD_强力射击(), Mode = SlotMode.Gcd },
                new() { Resolver = new BRD_OffGcd_失血箭(), Mode = SlotMode.OffGcd },
                new() { Resolver = new BRD_OffGcd_侧风诱导箭(), Mode = SlotMode.OffGcd },
            ],
            EventHandler = new BRDEventControl(),
        };
        return rot;
    }

    public IRotationUI? GetRotationUI() => new BRDUI();
    // ...
}
```

### 第三步：写 SlotResolver

每个技能一个类：

```csharp
public class BRD_GCD_强力射击 : ISlotResolver
{
    public int Check()
    {
        if (!Data.Combat.InCombat) return -1;
        if (Data.Me.Object == null) return -1;
        // 目标身上有 DoT 时允许用爆发射击代替
        if (AuraHelper.HasTargetAura(1200) && SpellHelper.CanUseSpell(16495))
            return 0;  // 风蚀在 → 别打强力射击，让爆发射击来
        return 0;
    }

    public void Build(Slot slot)
    {
        slot.Add(new Spell(97, SpellTargetType.Target)); // 强力射击
    }
}
```

### 第四步：在 EventHandler 中处理复杂逻辑

```csharp
public class BRDEventControl : IRotationEventHandler
{
    private int _songTimer;

    public void OnBattleUpdate(int battleTimeMs)
    {
        _songTimer = (int)(AuraHelper.GetAuraTimeLeft(Data.Me.Object, 118) / 1000);
    }

    public void OnPreCombat()
    {
        // 非战斗时唱歌
        if (!AuraHelper.HasSelfAura(118) && SpellHelper.CanUseSpell(3558))
            SlotHelper.Enqueue(new Slot(new Spell(3558, SpellTargetType.Self)));
    }
}
```

## 核心概念速览

### SlotResolver：Check / Build 分离

```
Check()  ──→  判断技能能不能用   →  ≥0=能用, <0=跳过
Build()  ──→  组装执行单元       →  往 Slot 里 Add Spell
```

**Check 做判断，Build 做组装。不要在 Build 里写条件判断，不要在 SlotResolver 里手动释放技能。**

### SlotMode：控制执行窗口

| Mode | 何时 Build | 用途 |
|------|-----------|------|
| `Gcd` | GCD 冷却完毕时 | 普通 GCD 技能 |
| `OffGcd` | 能力技插入窗口 | oGCD 技能 |
| `Always` | Check 通过就执行 | 疾跑、康复、Idle 占位 |

### SlotResolvers 的顺序决定优先级

```csharp
SlotResolvers = [
    new() { Resolver = new 优先级最高的技能(), Mode = SlotMode.Gcd },
    new() { Resolver = new 次优先的技能(),     Mode = SlotMode.Gcd },
    new() { Resolver = new 兜底的技能(),       Mode = SlotMode.Gcd },
];
```

排在前面的先 Check，先通过就先执行。

### Data 静态类：你的数据入口

```csharp
Data.Me          // 自己：Object/ClassJob/CurrentLevel/IsMoving/...
Data.Target      // 目标：Current/Focus/MouseOver/Soft/Previous
Data.Party       // 队伍：All/Tanks/Healers/Dps/...（自动分类，每帧刷新一次）
Data.Objects     // 周围对象：Enemies/Allies/Party/Pets（框架已过滤不可攻击目标）
Data.Combat      // 战斗状态：InCombat/IsCasting/AbilityCountInGcd/...
Data.FactState   // 事实轴：当前阶段/事件预测/前向扫描（仅副本运行时可用）
```

### 常用工具类

| 工具类 | 用途 | 关键方法 |
|--------|------|---------|
| `SpellHelper` | 技能就绪检查 | `CanUseSpell(id)`, `IsReadyWithCanCast(id, target)` |
| `AuraHelper` | Buff/Debuff 检测 | `HasSelfAura(id)`, `HasTargetAura(id)`, `GetAuraTimeLeft()` |
| `ComboHelper` | 连击状态 | `LastComboSpellId`, `WasLastCombo(id)` |
| `GCDHelper` | GCD 时间 | `IsGCDReady()`, `CanUseOffGcd()` |
| `TargetHelper` | 目标信息 | `GetNearbyEnemyCount()`, `IsBehind()`, `IsFlanking()` |
| `QTHelper` | QT 开关 | `IsEnabled(id)`, `IsEnabled(BuiltinQt.Burst)` |
| `SpellExtension` | Spell 扩展 | `IsUnlock()`, `IsReadyWithCanCast()`, `CoolDownInGCDs(n)` |
| `SlotHelper` | 手动提交技能 | `Execute(slot)`（立即）, `Enqueue(slot)`（排队） |

### HiAuRo.Helper：职业专属快速访问

所有 20 个战斗职业都有静态 Helper，例如：

```csharp
WARHelper.Has原初的解放        // 战士
BRDHelper.HasStraightShotReady // 诗人
SAMHelper.IsTargetDotOk        // 武士
BLMHelper.HasEnochian          // 黑魔
DRGHelper.HasPowerSurge        // 龙骑
```

## 事件回调一览

实现 `IRotationEventHandler`，全部有默认空实现，只覆写你需要的：

| 回调 | 触发时机 | 典型用途 |
|------|---------|---------|
| `OnPreCombat()` | 非战斗每帧 | 远敏唱歌、T 切姿态 |
| `OnResetBattle()` | 脱战 | 重置计数器、清缓存 |
| `OnNoTarget()` | 战斗中无目标 | 转阶段处理 |
| `OnBattleUpdate(int ms)` | 战斗中每帧 | **最常用**：更新 DoT 计时、Buff 追踪 |
| `OnSpellCastSuccess(Slot, Spell)` | 读条成功判定 | 滑步记录 |
| `BeforeSpell(Slot, Spell)` | 技能释放前 | 最后资源检查 |
| `AfterSpell(Slot, Spell)` | 技能释放后 | 状态变更记录 |
| `OnTerritoryChanged()` | 切图/进副本 | 重置副本状态 |
| `OnGameEvent(ITriggerCondParams)` | 游戏事件 | Boss 读条、点名、Buff 变化、连线等 |
| `OnPhaseChanged(string, string)` | 事实轴阶段切换 | 副本阶段策略 |

## 实战技巧：来自真实 ACR 的模式

以下模式来自生产级黑魔 ACR（[嗨呀/黑魔](https://github.com/denghaoxuan991876906/MyACR)），展示了 SampleACR 模板的实际运用方式。

### 技巧 1：用 CheckResult 枚举让 Check 返回值有语义

```csharp
// 定义你职业专属的拒绝原因枚举
public enum CheckResult
{
    等级不足 = -100,   技能未解锁 = -77,
    移动中 = -99,      技能未就绪 = -1,
    冷却中 = -2,       资源不足 = -3,
    QT关闭 = -4,       状态不符 = -5,
    目标无效 = -7,     最近已用 = -8,
}
```

**好处**：调试面板会显示中文拒绝原因，一眼就知道技能为什么不放。

### 技巧 2：守卫子句风格（最干净的 Check 写法）

```csharp
public class 耀星 : ISlotResolver
{
    public int Check()
    {
        // 条件检查按"成本从低到高"排列，不满足立即返回拒绝原因
        if (HelperRuntime.GetCurrentLevel() < 100)     return (int)CheckResult.等级不足;
        if (!BLMHelper.火状态)                           return (int)CheckResult.状态不符;
        if (BLMHelper.耀星层数 != 6)                     return (int)CheckResult.资源不足;
        if (Data.Me.IsMoving && !BLMHelper.可瞬发)       return (int)CheckResult.移动中;
        // 最后返回技能 ID 作为正数值（表示可用）
        return (int)BLMHelper.耀星;
    }

    public void Build(Slot slot)
    {
        slot.Add(new Spell { Id = BLMHelper.耀星, TargetType = SpellTargetType.Target });
    }
}
```

**规律**：
- `Check()` 用守卫子句：条件不满足 → `return` 负值
- `Build()` 不做任何判断，只组装 `Slot`
- 几乎不写 `if-else`，全部是 `if (...) return;`

### 技巧 3：BattleData 单例——把所有可变状态集中管理

```csharp
public class BLM_BattleData
{
    public static readonly BLM_BattleData Instance = new();

    public uint 前一gcd { get; set; }         // 上次放的 GCD 技能
    public int 已回复蓝量 { get; set; }       // 冰阶段回蓝模拟
    public bool 需要即刻 { get; set; }        // 触发瞬发队列
    public bool 能六火四 { get; set; } = true; // 够 MP 打 6 个火四吗

    public void Reset()
    {
        前一gcd = 0;
        已回复蓝量 = 0;
        需要即刻 = false;
        能六火四 = true;
    }
}
```

**好处**：不再到处散落 `static` 字段。`OnResetBattle()` 调用一次 `Reset()` 全部归零。

### 技巧 4：在 AfterSpell 中追踪复杂状态

黑魔需要在冰阶段手动模拟 MP 回复（游戏 API 不直接暴露每跳回蓝量），所以在 `AfterSpell` 中根据技能类型累加：

```csharp
public void AfterSpell(Slot slot, Spell spell)
{
    var bd = BLM_BattleData.Instance;

    if (spell.Id is BLMHelper.冰澈 or BLMHelper.玄冰)
        bd.已回复蓝量 += 2500;     // 冰三后每跳 2500 MP

    if (spell.Id is BLMHelper.耀星)
        bd.火阶段已放耀星 = true;  // 标记该火阶段已放过耀星

    if (spell.Id is BLMHelper.魔泉)
        bd.能六火四 = true;          // 魔泉重置回蓝后重新计算
}
```

### 技巧 5：资源溢出预防（防止浪费通晓层数）

```csharp
public class 异言 : ISlotResolver
{
    public int Check()
    {
        if (BLMHelper.通晓数 <= 0) return (int)CheckResult.资源不足;

        if (BLMHelper.通晓数 >= 3) return (int)BLMHelper.异言;  // 3 层：马上泄

        // 2 层但详述快转好 → 留着等详述，避免溢出
        if (BLMHelper.通晓数 >= 2
            && CooldownHelper.GetCooldownRemaining(BLMHelper.详述) < 10000)
            return (int)CheckResult.状态不符;

        return (int)CheckResult.资源不足;
    }
}
```

### 技巧 6：在 OnBattleUpdate 中写阶段切换逻辑

```csharp
public void OnBattleUpdate(int battleTimeMs)
{
    var status = BLMHelper.冰火状态(); // 1=冰 2=火 0=无

    if (status != bd.上次冰火状态)
    {
        // 火 → 冰：重置阶段数据
        if (bd.上次冰火状态 == 2 && status == 1)
        {
            bd.已回复蓝量 = 0;
            bd.火阶段已放耀星 = false;
        }

        // 冰 → 火：计算六火四可行性
        if (bd.上次冰火状态 == 1 && status == 2)
        {
            var iceHearts = (int)BLMHelper.冰针数;
            var mpForSix = Math.Min(6, iceHearts) * 800     // 有心火四 = 800 MP
                         + Math.Max(0, 6 - iceHearts) * 1600; // 无心火四 = 1600 MP
            bd.能六火四 = bd.已回复蓝量 >= mpForSix + 800;   // 还得留绝望的 800
        }
        bd.上次冰火状态 = status;
    }
}
```

### 技巧 7：自动瞬发队列——发呆时自己激发即刻

```csharp
// BattleData 中的"发呆"检测
public static bool 在发呆()
{
    if (!Data.Combat.InCombat) return false;     // 已脱战
    if (HelperRuntime.GetGCDCooldown() > 0) return false; // GCD 没转好
    return true;                                  // 战斗中 GCD 好 = 该放技能了
}

// OnBattleUpdate 中触发瞬发
if (在发呆()) bd.需要即刻 = true;

// 即刻三连 Resolver（SlotMode.Always）
public class 即刻三连 : ISlotResolver
{
    public int Check()
    {
        if (!BLM_BattleData.Instance.需要即刻) return (int)CheckResult.状态不符;
        if (BLMHelper.可瞬发) return (int)CheckResult.状态不符; // 已经是瞬发
        if (CooldownHelper.GetCharges(BLMHelper.三连咏唱) >= 1) return 1;
        if (CooldownHelper.GetCooldownRemaining(BLMHelper.即可咏唱) <= 0) return 1;
        return (int)CheckResult.技能未就绪;
    }

    public void Build(Slot slot)
    {
        BLM_BattleData.Instance.需要即刻 = false;
        // 优先三连，其次即刻
        var id = CooldownHelper.GetCharges(BLMHelper.三连咏唱) >= 1
            ? BLMHelper.三连咏唱 : BLMHelper.即可咏唱;
        slot.Add(new Spell(id, SpellTargetType.Self));
    }
}
```

**关键**：`SlotMode.Always` 让这个 Resolver 在任何时刻都能触发，不受 GCD/oGCD 窗口限制。

### 技巧 8：QT 开关字符串常量管理

```csharp
// UI/QTKey.cs
public static class QTKey
{
    public const string 三连 = "三连";
    public const string 墨泉 = "墨泉";
    public const string 高级循环 = "高级循环";
    public const string 减少火悖论 = "减少火悖论";
    // 内置 QT 使用 BuiltinQt 枚举，自定义 QT 用字符串
}

// 在 Check 中使用
if (!QTHelper.IsEnabled(QTKey.AOE)) return (int)CheckResult.QT关闭;
if (QTHelper.IsEnabled(QTKey.高级循环)) { /* 特化逻辑 */ }

// 在 UI 中注册
builder.AddQtToggle(QTKey.三连, true);  // id + 默认值
builder.AddQtToggle(QTKey.墨泉, true);
builder.AddQtToggle(QTKey.高级循环, false);
```

### 技巧 9：设置绑定——IAcrUiBuilder ref 模式

```csharp
// Settings 用 Singleton 方便 Resolver 中访问
public class BLM_Setting : AcrSettings
{
    public bool test1 = false;
    public int test2 = 0;
    public static BLM_Setting Instance { get; set; } = new();
}

// UI 中使用 ref 绑定
public void RegisterControls(IAcrUiBuilder builder)
{
    var boolVal = BLM_Setting.Instance.test1;
    if (builder.AddCheckbox("开关测试", ref boolVal))  // ref 自动追踪变化
    {
        BLM_Setting.Instance.test1 = boolVal;
        BLM_Setting.Instance.Save();                    // 手动保存到 JSON
    }
}
```

### 技巧 10：直接输出到插件目录（开发效率翻倍）

修改 `.csproj` 让 Debug 构建直接输出到 HiAuRo 的 ACR 加载目录，省去手动拷贝步骤：

```xml
<PropertyGroup Condition=" '$(Configuration)' == 'Debug' ">
  <OutputPath>$(AppData)\XIVLauncherCN\pluginConfigs\HiAuRo\ACR\嗨呀\</OutputPath>
</PropertyGroup>
```

改代码 → F6 构建 → 在游戏中切一下职业 → 新代码生效。开发循环缩短到秒级。

### 技巧 11：职业 Helper 惯例

```csharp
public class BLMHelper
{
    // ① 技能 ID 常量
    public const uint 火四 = 3577;
    public const uint 异言 = 16507;

    // ② Buff ID 常量
    public const uint 三连buff = 1211;
    public const uint 雷云buff = 3870;

    // ③ 简单 Buff 检测（薄封装 HelperRuntime）
    public static bool Has三连 => HelperRuntime.HasStatus(三连buff);
    public static bool Has雷云 => HelperRuntime.HasStatus(雷云buff);

    // ④ 复合快捷属性
    public static bool 可瞬发 => Has三连 || Has即刻;
    public static bool 群怪模式 => HelperRuntime.GetEnemyCountNearTarget(5) >= 3;

    // ⑤ 带逻辑的辅助方法
    public static bool 补dot()
    {
        // 检查目标身上 DoT 剩余时间，需要等级感知 ID 切换
        var time = HelperRuntime.GetStatusTimeLeftOnTarget(3871);
        return time < 3f;
    }

    // ⑥ 量谱访问（类型安全）
    public static BLMGauge Gauge => HelperRuntime.GetGauge<BLMGauge>();
    public static bool 火状态 => Gauge.InAstralFire;
    public static uint 通晓数 => Gauge.PolyglotStacks;
}
```

## 调试

HiAuRo 的 Web UI 内置调试面板，会实时显示每个 SlotResolver 的：
- Check 返回值
- 是否通过窗口检查
- 是否被执行 Build

如果某个技能不放，检查三点：
1. Check 返回值 (应 ≥ 0)
2. SlotMode 窗口是否匹配
3. 是否被优先级更高的技能抢走了执行机会

## 更多资源

- **[ACR 作者完整上手指南](../HiAuRo/doc/ACR_AUTHOR_GUIDE.md)** — 接口详解、高级特性（Opener/SlotSequence/Trigger/TargetResolver）、实战技巧
- **[ACR 设置系统设计](../HiAuRo/doc/ACR_SETTINGS_DESIGN.md)** — Settings 持久化、UI 绑定原理
- **[HiAuRo.Helper](https://github.com/denghaoxuan991876906/HiAuRo.Helper)** — 全职业数据辅助库（clone 本项目时已作为 submodule 引入）
