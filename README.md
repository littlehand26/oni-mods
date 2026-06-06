# 暗物质太空服 (Dark Matter Suit)

> An overpowered "god suit" mod for **Oxygen Not Included** — one suit that frees a duplicant from all survival needs, grants flight, maxes every attribute, unlocks all skills/research, and speed-digs. Every feature is toggleable in the in-game mod config (PLib).

一件缺氧（Oxygen Not Included）的「无敌太空服」Mod。穿上后复制人摆脱一切生存需求，并获得飞行、全属性、全技能、快速挖掘等能力。所有功能都能在游戏内 Mod 配置面板里单独开关。

## 功能

穿上暗物质太空服后，复制人获得：

**维生**
- 无限氧气与燃料（永不窒息、永不缺燃料）
- 冻结生存需求（体力 / 卡路里 / 膀胱 / 压力不再恶化）
- 不需要睡眠（无视作息表，全天候工作）
- 环境保护与免疫（隔热、烫伤阈值 +9999、冻伤、湿身 / 爆耳等 debuff 免疫）
- 缓慢回血
- **适配仿生复制人**（自动补满电池 / 氧气罐 / 机油、清零淤泥）

**能力**
- 全 12 项基础属性加成（默认 +1000，可调 0~10000）
- 全技能解锁（视为拥有全部 perk，且不增加士气期望）
- 科技全开（穿上时一次性完成全部研究）
- 永不脱下（经过太空服检查点不被强制脱衣）

**移动**
- 飞行（可在空中 / 真空飞行）
- 水中飞行（可飞入并穿行液体）
- 飞行提速（默认 20，原版 7）

**挖掘**
- 全属性加成带来的极速挖掘
- 连锁挖掘（完成一格时连带挖掉周围已标记的格子，分帧处理避免卡顿）

**外观**
- 普通太空服造型 + 暗物质紫色染色

## 获取方式

在**工艺站（Crafting Station）**用 1 kg 沙子制造（接近零成本）。

## 配置

游戏内 **MODS → 暗物质太空服 → 配置齿轮**，可单独开关每个功能、调整数值（属性加成、飞行速度、连锁挖掘半径等）。改动需重启游戏生效。

依赖 [PLib](https://github.com/peterhaneve/ONIMods)（配置面板由 PLib 提供）。

## 构建

需要 .NET SDK 与游戏的托管 DLL。

```bash
# 默认从 macOS Steam 路径取游戏 DLL，并部署到 Dev mod 目录
dotnet build -c Release

# 其他机器/路径用参数覆盖
dotnet build -c Release -p:GameLibsPath="<游戏 OxygenNotIncluded_Data/Managed 路径>"
```

- 目标框架 `net48`（匹配当前游戏自带的 0Harmony）
- 构建产物自动复制到 `mods/Dev/DarkMatterSuit/`（含 PLib.dll）

## 许可

MIT License，见 [LICENSE](LICENSE)。

外观复用了游戏原版太空服的动画资源；游戏素材版权归 Klei Entertainment 所有，本仓库仅含 Mod 源代码。
