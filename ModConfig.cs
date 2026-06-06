using Newtonsoft.Json;
using PeterHan.PLib.Options;

namespace DarkMatterSuit
{
    // 游戏 MODS 界面的配置面板（PLib POptions 渲染）。各功能可手动开关 + 关键数值滑条。
    // 改动需重启游戏生效（[RestartRequired]，因属性/技能/科技在装备时一次性应用）。
    [RestartRequired]
    [JsonObject(MemberSerialization.OptIn)]
    [ConfigFile(SharedConfigLocation: true)]
    public class ModConfig
    {
        public static ModConfig Instance { get; set; } = new ModConfig();

        // —— 维生 ——
        [JsonProperty]
        [Option("无限氧气与燃料", "持续回满氧气罐与喷气燃料", "维生")]
        public bool InfiniteOxygenFuel { get; set; } = true;

        [JsonProperty]
        [Option("冻结生存需求", "体力/卡路里/膀胱/压力不再恶化", "维生")]
        public bool FreezeNeeds { get; set; } = true;

        [JsonProperty]
        [Option("不需要睡眠", "无视作息表就寝时段，全天候工作", "维生")]
        public bool NoSleep { get; set; } = true;

        [JsonProperty]
        [Option("环境保护与免疫", "隔热、烫伤/冻伤阈值拉满、湿身/爆耳等 debuff 免疫", "维生")]
        public bool EnvironmentProtection { get; set; } = true;

        [JsonProperty]
        [Option("缓慢回血", "持续恢复生命值", "维生")]
        public bool Regen { get; set; } = true;

        [JsonProperty]
        [Option("适配仿生复制人", "仿生人穿戴时补满电池/氧气罐/机油、清零淤泥", "维生")]
        public bool BionicSupport { get; set; } = true;

        // —— 能力 ——
        [JsonProperty]
        [Option("全属性加成", "全部 12 项基础属性加成值（0 = 关闭）", "能力", Format = "G5")]
        [Limit(0, 10000)]
        public int AttributeBonus { get; set; } = 1000;

        [JsonProperty]
        [Option("全技能解锁", "穿戴者视为拥有全部技能 perk（不增加士气期望）", "能力")]
        public bool AllSkills { get; set; } = true;

        [JsonProperty]
        [Option("科技全开", "穿上服时一次性完成全部研究", "能力")]
        public bool UnlockResearch { get; set; } = true;

        [JsonProperty]
        [Option("永不脱下", "经过太空服检查点不被强制脱衣", "能力")]
        public bool NeverUnequip { get; set; } = true;

        // —— 移动 ——
        [JsonProperty]
        [Option("飞行", "可在空中/真空飞行（喷气服机制）", "移动")]
        public bool Flight { get; set; } = true;

        [JsonProperty]
        [Option("水中飞行", "可飞入并穿行液体", "移动")]
        public bool FlyInLiquid { get; set; } = true;

        [JsonProperty]
        [Option("飞行速度", "飞行（hover）移动速度（原版 7）", "移动")]
        [Limit(7, 50)]
        public int FlySpeed { get; set; } = 20;

        // —— 挖掘 ——
        [JsonProperty]
        [Option("连锁挖掘半径", "完成一格挖掘时连带挖掉周围已标记格的半径（0 = 关闭）", "挖掘")]
        [Limit(0, 12)]
        public int ChainDigRadius { get; set; } = 8;
    }
}
