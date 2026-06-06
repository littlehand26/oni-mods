using Newtonsoft.Json;
using PeterHan.PLib.Options;

namespace DarkMatterSuit
{
    // 游戏 MODS 界面的配置面板（PLib POptions 渲染）。各功能可手动开关 + 关键数值滑条。
    // [Option] 用 STRINGS key（PLib 会查 Strings），文本由 ModStrings.RegisterConfigStrings() 按语言填入。
    // 改动需重启游戏生效（[RestartRequired]，因属性/技能/科技在装备时一次性应用）。
    [RestartRequired]
    [JsonObject(MemberSerialization.OptIn)]
    [ConfigFile(SharedConfigLocation: true)]
    public class ModConfig
    {
        public static ModConfig Instance { get; set; } = new ModConfig();

        // —— 维生 ——
        [JsonProperty]
        [Option(ModStrings.OPT_OXY, ModStrings.OPT_OXY_TIP, ModStrings.CAT_SURVIVAL)]
        public bool InfiniteOxygenFuel { get; set; } = true;

        [JsonProperty]
        [Option(ModStrings.OPT_NEEDS, ModStrings.OPT_NEEDS_TIP, ModStrings.CAT_SURVIVAL)]
        public bool FreezeNeeds { get; set; } = true;

        [JsonProperty]
        [Option(ModStrings.OPT_SLEEP, ModStrings.OPT_SLEEP_TIP, ModStrings.CAT_SURVIVAL)]
        public bool NoSleep { get; set; } = true;

        [JsonProperty]
        [Option(ModStrings.OPT_ENV, ModStrings.OPT_ENV_TIP, ModStrings.CAT_SURVIVAL)]
        public bool EnvironmentProtection { get; set; } = true;

        [JsonProperty]
        [Option(ModStrings.OPT_REGEN, ModStrings.OPT_REGEN_TIP, ModStrings.CAT_SURVIVAL)]
        public bool Regen { get; set; } = true;

        [JsonProperty]
        [Option(ModStrings.OPT_BIONIC, ModStrings.OPT_BIONIC_TIP, ModStrings.CAT_SURVIVAL)]
        public bool BionicSupport { get; set; } = true;

        // —— 能力 ——
        [JsonProperty]
        [Option(ModStrings.OPT_ATTR, ModStrings.OPT_ATTR_TIP, ModStrings.CAT_ABILITY, Format = "G5")]
        [Limit(0, 10000)]
        public int AttributeBonus { get; set; } = 1000;

        [JsonProperty]
        [Option(ModStrings.OPT_SKILLS, ModStrings.OPT_SKILLS_TIP, ModStrings.CAT_ABILITY)]
        public bool AllSkills { get; set; } = true;

        [JsonProperty]
        [Option(ModStrings.OPT_RESEARCH, ModStrings.OPT_RESEARCH_TIP, ModStrings.CAT_ABILITY)]
        public bool UnlockResearch { get; set; } = true;

        [JsonProperty]
        [Option(ModStrings.OPT_NOUNEQUIP, ModStrings.OPT_NOUNEQUIP_TIP, ModStrings.CAT_ABILITY)]
        public bool NeverUnequip { get; set; } = true;

        // —— 移动 ——
        [JsonProperty]
        [Option(ModStrings.OPT_FLIGHT, ModStrings.OPT_FLIGHT_TIP, ModStrings.CAT_MOVE)]
        public bool Flight { get; set; } = true;

        [JsonProperty]
        [Option(ModStrings.OPT_FLYLIQUID, ModStrings.OPT_FLYLIQUID_TIP, ModStrings.CAT_MOVE)]
        public bool FlyInLiquid { get; set; } = true;

        [JsonProperty]
        [Option(ModStrings.OPT_FLYSPEED, ModStrings.OPT_FLYSPEED_TIP, ModStrings.CAT_MOVE)]
        [Limit(7, 50)]
        public int FlySpeed { get; set; } = 20;

        // —— 挖掘 ——
        [JsonProperty]
        [Option(ModStrings.OPT_CHAINDIG, ModStrings.OPT_CHAINDIG_TIP, ModStrings.CAT_DIG)]
        [Limit(0, 12)]
        public int ChainDigRadius { get; set; } = 8;
    }
}
