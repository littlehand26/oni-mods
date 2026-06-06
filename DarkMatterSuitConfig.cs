using System.Collections.Generic;
using Klei.AI;
using TUNING;
using UnityEngine;

namespace DarkMatterSuit
{
    // IEquipmentConfig 由游戏反射自动发现并注册（GeneratedEquipment.LoadGeneratedEquipment）
    public class DarkMatterSuitConfig : IEquipmentConfig
    {
        public const string ID = "DarkMatterAnnihilationSuit";
        public static readonly Tag TAG = new Tag(ID);
        // 穿戴者标记：供调度/睡眠相关 patch 快速判定（HashSet 查询）
        public static readonly Tag WEARER_TAG = new Tag("DarkMatterSuitWearer");

        private const string ANIM_ITEM = "suit_oxygen_kanim";   // 普通太空服（atmo），非喷气服
        private const string ANIM_BODY = "body_oxygen_kanim";
        private const string ANIM_HOVER = "anim_loco_hover_kanim";
        private static readonly Color32 DARK_MATTER_TINT = new Color32(130, 80, 190, 255); // 暗物质浅紫

        public EquipmentDef CreateEquipmentDef()
        {
            var modifiers = new List<AttributeModifier>();
            var cfg = ModConfig.Instance;

            // 全 12 项基础属性加成（值与开关由配置控制；0 = 不加）
            if (cfg.AttributeBonus > 0)
            {
                foreach (string attributeId in DUPLICANTSTATS.ALL_ATTRIBUTES)
                {
                    modifiers.Add(new AttributeModifier(attributeId, cfg.AttributeBonus, ModStrings.NAME));
                }
            }

            // 需求冻结（数值取明显超过基础速率的值）
            var amounts = Db.Get().Amounts;
            if (cfg.FreezeNeeds)
            {
                modifiers.Add(new AttributeModifier(amounts.Stamina.deltaAttribute.Id, 0.2f, ModStrings.NAME));
                modifiers.Add(new AttributeModifier(amounts.Calories.deltaAttribute.Id, 1700f, ModStrings.NAME));
                modifiers.Add(new AttributeModifier(amounts.Bladder.deltaAttribute.Id, -0.2f, ModStrings.NAME));
                modifiers.Add(new AttributeModifier(amounts.Stress.deltaAttribute.Id, -0.1f, ModStrings.NAME));
            }
            if (cfg.Regen)
            {
                modifiers.Add(new AttributeModifier(amounts.HitPoints.deltaAttribute.Id, 0.05f, ModStrings.NAME));
            }

            // 环境保护（铅服同级拉满）
            var attributes = Db.Get().Attributes;
            if (cfg.EnvironmentProtection)
            {
                modifiers.Add(new AttributeModifier(TUNING.EQUIPMENT.ATTRIBUTE_MOD_IDS.INSULATION, 50f, ModStrings.NAME));
                modifiers.Add(new AttributeModifier(TUNING.EQUIPMENT.ATTRIBUTE_MOD_IDS.THERMAL_CONDUCTIVITY_BARRIER, 0.3f, ModStrings.NAME));
                modifiers.Add(new AttributeModifier(attributes.ScaldingThreshold.Id, 9999f, ModStrings.NAME)); // 覆盖岩浆(1727°C)在内的一切热源
                modifiers.Add(new AttributeModifier(attributes.ScoldingThreshold.Id, -1000f, ModStrings.NAME));
                if (DlcManager.IsExpansion1Active())
                {
                    modifiers.Add(new AttributeModifier(attributes.RadiationResistance.Id, 1f, ModStrings.NAME));
                }
            }

            EquipmentDef def = EquipmentTemplates.CreateEquipmentDef(
                ID,
                TUNING.EQUIPMENT.SUITS.SLOT,
                SimHashes.Dirt,
                TUNING.EQUIPMENT.SUITS.ATMOSUIT_MASS,
                ANIM_ITEM,
                "",
                ANIM_BODY, // 太空服 body 覆盖外形
                6,
                modifiers,
                null,
                IsBody: true,
                EntityTemplates.CollisionShape.CIRCLE,
                0.325f, 0.325f,
                new Tag[] { GameTags.Suit, GameTags.Clothes, GameTags.PedestalDisplayable, GameTags.AirtightSuit });

            def.RecipeDescription = ModStrings.RECIPE_DESC;
            // 无 wornID、无 Durability 组件 → 永不磨损（PRD F7）

            if (cfg.EnvironmentProtection)
            {
                foreach (string effectId in new[] { "SoakingWet", "WetFeet", "ColdAir", "WarmAir", "PoppedEarDrums", "RecentlySlippedTracker" })
                {
                    def.EffectImmunites.Add(Db.Get().effects.Get(effectId));
                }
            }

            def.OnEquipCallBack = OnEquip;
            def.OnUnequipCallBack = OnUnequip;

            GeneratedBuildings.RegisterWithOverlay(OverlayScreen.SuitIDs, ID);
            return def;
        }

        public void DoPostConfigure(GameObject go)
        {
            SuitTank suitTank = go.AddComponent<SuitTank>();
            suitTank.element = "Oxygen";
            suitTank.capacity = 75f;
            suitTank.amount = 75f; // 出厂即满罐（OnSpawn 时转为 storage 气体）
            suitTank.elementTag = GameTags.Breathable;
            suitTank.SafeCellFlagsToIgnoreOnEquipped = (SafeCellQuery.SafeFlags)464;

            // 普通太空服无喷气背包：不挂 JetSuitTank；头盔无喷气（has_jets 默认 false）。
            // 飞行仍靠 OnEquip 设的 HasJetPack flag（独立于外观）。
            go.AddComponent<HelmetController>();

            KPrefabID prefabID = go.GetComponent<KPrefabID>();
            prefabID.AddTag(GameTags.Clothes);
            prefabID.AddTag(GameTags.PedestalDisplayable);
            prefabID.AddTag(GameTags.AirtightSuit);

            Storage storage = go.AddOrGet<Storage>();
            storage.SetDefaultStoredItemModifiers(Storage.StandardInsulatedStorage);
            storage.showInUI = true;

            go.AddOrGet<AtmoSuit>();
            go.AddComponent<SuitDiseaseHandler>();
            go.AddComponent<DarkMatterCore>();
        }

        // IEquipmentConfig 的旧接口成员（带默认实现），net472 目标下显式实现以避开 DIM 限制
        public string[] GetDlcIds() => null;

        private static void OnEquip(Equippable eq)
        {
            GameObject dupe = GetWearer(eq);
            if (dupe == null)
            {
                return;
            }
            if (ModConfig.Instance.Flight)
            {
                Navigator navigator = dupe.GetComponent<Navigator>();
                if (navigator != null)
                {
                    navigator.SetFlags(PathFinder.PotentialPath.Flags.HasJetPack); // 飞行
                }
                KAnimControllerBase anim = dupe.GetComponent<KAnimControllerBase>();
                if (anim != null)
                {
                    anim.AddAnimOverrides(Assets.GetAnim(ANIM_HOVER));
                }
            }
            // 暗物质染色：穿戴者整体染深紫，区别于喷气服外观（暗物质笼罩感）
            KBatchedAnimController tintAnim = dupe.GetComponent<KBatchedAnimController>();
            if (tintAnim != null)
            {
                tintAnim.TintColour = DARK_MATTER_TINT;
            }
            dupe.AddTag(GameTags.HasAirtightSuit);
            dupe.AddTag(WEARER_TAG);
        }

        private static void OnUnequip(Equippable eq)
        {
            if (eq.assignee == null)
            {
                return;
            }
            GameObject dupe = GetWearer(eq);
            if (dupe != null)
            {
                Navigator navigator = dupe.GetComponent<Navigator>();
                if (navigator != null)
                {
                    navigator.ClearFlags(PathFinder.PotentialPath.Flags.HasJetPack);
                }
                KAnimControllerBase anim = dupe.GetComponent<KAnimControllerBase>();
                if (anim != null)
                {
                    anim.RemoveAnimOverrides(Assets.GetAnim(ANIM_HOVER));
                }
                Effects effects = dupe.GetComponent<Effects>();
                if (effects != null && effects.HasEffect("SoiledSuit"))
                {
                    effects.Remove("SoiledSuit");
                }
                KBatchedAnimController tintAnim = dupe.GetComponent<KBatchedAnimController>();
                if (tintAnim != null)
                {
                    tintAnim.TintColour = Color.white; // 恢复原色
                }
                dupe.RemoveTag(GameTags.HasAirtightSuit);
                dupe.RemoveTag(WEARER_TAG);
            }
            // 与原版一致：脱下时倒掉罐内非氧气内容物
            Tag elementTag = eq.GetComponent<SuitTank>().elementTag;
            eq.GetComponent<Storage>().DropUnlessHasTag(elementTag);
        }

        private static GameObject GetWearer(Equippable eq)
        {
            Ownables owner = eq.assignee?.GetSoleOwner();
            if (owner == null)
            {
                return null;
            }
            return owner.GetComponent<MinionAssignablesProxy>().GetTargetGameObject();
        }
    }
}
