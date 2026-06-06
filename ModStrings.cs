namespace DarkMatterSuit
{
    // 中英双语字符串。按游戏当前语言返回对应文本；配置面板用 STRINGS key（PLib 会查 Strings），
    // 这些 key 的值在 RegisterConfigStrings() 里按语言填入（Db.Initialize 时调用）。
    public static class ModStrings
    {
        private static bool? _zh;
        private static bool IsZh
        {
            get
            {
                if (_zh == null)
                {
                    string code = Localization.GetCurrentLanguageCode() ?? "";
                    _zh = code.ToLowerInvariant().Contains("zh");
                }
                return _zh.Value;
            }
        }

        private static string L(string en, string zh) => IsZh ? zh : en;

        // —— 装备字符串（运行时按语言取值）——
        public static string NAME => L("Dark Matter Suit", "暗物质太空服");

        public static string DESC => L(
            "This suit continuously condenses dark matter from the surrounding space and annihilates it, " +
            "producing all the energy and matter its wearer needs: oxygen, thrust, thermal balance, even metabolism itself. " +
            "It takes only a handful of sand as a condensation core — it never belonged to this material world.",
            "这件服装从周围空间中持续凝聚暗物质并使其湮灭，产生的能量与物质足以供应穿戴者的一切所需：" +
            "氧气、推力、热平衡，乃至代谢本身。制造它只需一捧沙子作为凝聚核——它本来就不属于这个物质世界。");

        public static string EFFECT => L(
            "Infinite oxygen & fuel, flight, +1000 to all attributes, all skills unlocked, and freezes all survival needs.",
            "无限氧气与燃料、飞行能力、全属性 +1000、全技能解锁，并冻结一切生存需求。");

        public static string RECIPE_DESC => L(
            "Condensed from dark matter, using a handful of sand as the core.",
            "以一捧沙子为凝聚核，凝聚空间暗物质成型。");

        // —— 配置面板 STRINGS key（[Option] 引用，必须是编译期常量）——
        public const string CAT_SURVIVAL = "STRINGS.DARKMATTER.CAT.SURVIVAL";
        public const string CAT_ABILITY = "STRINGS.DARKMATTER.CAT.ABILITY";
        public const string CAT_MOVE = "STRINGS.DARKMATTER.CAT.MOVE";
        public const string CAT_DIG = "STRINGS.DARKMATTER.CAT.DIG";

        public const string OPT_OXY = "STRINGS.DARKMATTER.OPT.OXY";
        public const string OPT_OXY_TIP = "STRINGS.DARKMATTER.OPT.OXY_TIP";
        public const string OPT_NEEDS = "STRINGS.DARKMATTER.OPT.NEEDS";
        public const string OPT_NEEDS_TIP = "STRINGS.DARKMATTER.OPT.NEEDS_TIP";
        public const string OPT_SLEEP = "STRINGS.DARKMATTER.OPT.SLEEP";
        public const string OPT_SLEEP_TIP = "STRINGS.DARKMATTER.OPT.SLEEP_TIP";
        public const string OPT_ENV = "STRINGS.DARKMATTER.OPT.ENV";
        public const string OPT_ENV_TIP = "STRINGS.DARKMATTER.OPT.ENV_TIP";
        public const string OPT_REGEN = "STRINGS.DARKMATTER.OPT.REGEN";
        public const string OPT_REGEN_TIP = "STRINGS.DARKMATTER.OPT.REGEN_TIP";
        public const string OPT_BIONIC = "STRINGS.DARKMATTER.OPT.BIONIC";
        public const string OPT_BIONIC_TIP = "STRINGS.DARKMATTER.OPT.BIONIC_TIP";
        public const string OPT_ATTR = "STRINGS.DARKMATTER.OPT.ATTR";
        public const string OPT_ATTR_TIP = "STRINGS.DARKMATTER.OPT.ATTR_TIP";
        public const string OPT_SKILLS = "STRINGS.DARKMATTER.OPT.SKILLS";
        public const string OPT_SKILLS_TIP = "STRINGS.DARKMATTER.OPT.SKILLS_TIP";
        public const string OPT_RESEARCH = "STRINGS.DARKMATTER.OPT.RESEARCH";
        public const string OPT_RESEARCH_TIP = "STRINGS.DARKMATTER.OPT.RESEARCH_TIP";
        public const string OPT_NOUNEQUIP = "STRINGS.DARKMATTER.OPT.NOUNEQUIP";
        public const string OPT_NOUNEQUIP_TIP = "STRINGS.DARKMATTER.OPT.NOUNEQUIP_TIP";
        public const string OPT_FLIGHT = "STRINGS.DARKMATTER.OPT.FLIGHT";
        public const string OPT_FLIGHT_TIP = "STRINGS.DARKMATTER.OPT.FLIGHT_TIP";
        public const string OPT_FLYLIQUID = "STRINGS.DARKMATTER.OPT.FLYLIQUID";
        public const string OPT_FLYLIQUID_TIP = "STRINGS.DARKMATTER.OPT.FLYLIQUID_TIP";
        public const string OPT_FLYSPEED = "STRINGS.DARKMATTER.OPT.FLYSPEED";
        public const string OPT_FLYSPEED_TIP = "STRINGS.DARKMATTER.OPT.FLYSPEED_TIP";
        public const string OPT_CHAINDIG = "STRINGS.DARKMATTER.OPT.CHAINDIG";
        public const string OPT_CHAINDIG_TIP = "STRINGS.DARKMATTER.OPT.CHAINDIG_TIP";

        // Db.Initialize 时把配置面板字符串按当前语言填入 Strings（PLib 渲染时查这些 key）
        public static void RegisterConfigStrings()
        {
            Strings.Add(CAT_SURVIVAL, L("Survival", "维生"));
            Strings.Add(CAT_ABILITY, L("Ability", "能力"));
            Strings.Add(CAT_MOVE, L("Movement", "移动"));
            Strings.Add(CAT_DIG, L("Digging", "挖掘"));

            Strings.Add(OPT_OXY, L("Infinite Oxygen & Fuel", "无限氧气与燃料"));
            Strings.Add(OPT_OXY_TIP, L("Continuously refill the oxygen tank and jet fuel", "持续回满氧气罐与喷气燃料"));
            Strings.Add(OPT_NEEDS, L("Freeze Survival Needs", "冻结生存需求"));
            Strings.Add(OPT_NEEDS_TIP, L("Stamina / calories / bladder / stress no longer worsen", "体力 / 卡路里 / 膀胱 / 压力不再恶化"));
            Strings.Add(OPT_SLEEP, L("No Sleep Needed", "不需要睡眠"));
            Strings.Add(OPT_SLEEP_TIP, L("Ignore the sleep schedule block; work around the clock", "无视作息表就寝时段，全天候工作"));
            Strings.Add(OPT_ENV, L("Environment Protection", "环境保护与免疫"));
            Strings.Add(OPT_ENV_TIP, L("Insulation, max scalding/freezing thresholds, immune to wet/eardrum debuffs", "隔热、烫伤/冻伤阈值拉满、湿身/爆耳等 debuff 免疫"));
            Strings.Add(OPT_REGEN, L("Slow Regeneration", "缓慢回血"));
            Strings.Add(OPT_REGEN_TIP, L("Continuously restore health", "持续恢复生命值"));
            Strings.Add(OPT_BIONIC, L("Bionic Duplicant Support", "适配仿生复制人"));
            Strings.Add(OPT_BIONIC_TIP, L("For bionic wearers: refill battery / oxygen tank / oil, clear gunk", "仿生人穿戴时补满电池/氧气罐/机油、清零淤泥"));
            Strings.Add(OPT_ATTR, L("All-Attribute Bonus", "全属性加成"));
            Strings.Add(OPT_ATTR_TIP, L("Bonus added to all 12 base attributes (0 = off)", "全部 12 项基础属性加成值（0 = 关闭）"));
            Strings.Add(OPT_SKILLS, L("Unlock All Skills", "全技能解锁"));
            Strings.Add(OPT_SKILLS_TIP, L("Wearer is treated as having every skill perk (no morale expectation increase)", "穿戴者视为拥有全部技能 perk（不增加士气期望）"));
            Strings.Add(OPT_RESEARCH, L("Unlock All Research", "科技全开"));
            Strings.Add(OPT_RESEARCH_TIP, L("Complete all research once when the suit is equipped", "穿上服时一次性完成全部研究"));
            Strings.Add(OPT_NOUNEQUIP, L("Never Remove", "永不脱下"));
            Strings.Add(OPT_NOUNEQUIP_TIP, L("Not forced off at suit checkpoints", "经过太空服检查点不被强制脱衣"));
            Strings.Add(OPT_FLIGHT, L("Flight", "飞行"));
            Strings.Add(OPT_FLIGHT_TIP, L("Can fly through air / vacuum", "可在空中/真空飞行"));
            Strings.Add(OPT_FLYLIQUID, L("Fly in Liquid", "水中飞行"));
            Strings.Add(OPT_FLYLIQUID_TIP, L("Can fly into and through liquids", "可飞入并穿行液体"));
            Strings.Add(OPT_FLYSPEED, L("Flight Speed", "飞行速度"));
            Strings.Add(OPT_FLYSPEED_TIP, L("Hover movement speed (vanilla is 7)", "飞行（hover）移动速度（原版 7）"));
            Strings.Add(OPT_CHAINDIG, L("Chain Dig Radius", "连锁挖掘半径"));
            Strings.Add(OPT_CHAINDIG_TIP, L("Radius of marked cells also dug when finishing one (0 = off)", "完成一格挖掘时连带挖掉周围已标记格的半径（0 = 关闭）"));
        }
    }
}
