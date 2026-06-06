using System.Collections.Generic;
using Database;
using Klei.AI;
using UnityEngine;

namespace DarkMatterSuit
{
    // 挂在服 prefab 上的"暗物质湮灭核心"：
    // 1. 每秒回满氧气罐与燃料罐、清除罐内 CO2（PRD F2/F3）
    // 2. 穿戴期间授予全部技能 perk，脱下移除（PRD F10）
    public class DarkMatterCore : KMonoBehaviour, ISim1000ms
    {
#pragma warning disable 649 // [MyCmpReq] 字段由 KMonoBehaviour 框架运行时注入
        [MyCmpReq] private SuitTank suitTank;
        [MyCmpReq] private Storage storage;
#pragma warning restore 649

        private static readonly Tag CO2_TAG = SimHashes.CarbonDioxide.CreateTag();

        private GameObject wearer;
        // 只记录"穿戴时复制人尚未拥有"的 perk，脱下时只移除这些，
        // 避免 RemoveAdditionalSkillPerks 的 OnRemove 误伤精通技能授予的同名 perk（见 docs/todo.md）
        private readonly List<SkillPerk> grantedPerks = new List<SkillPerk>();

        private static readonly EventSystem.IntraObjectHandler<DarkMatterCore> OnEquippedDelegate =
            new EventSystem.IntraObjectHandler<DarkMatterCore>((component, data) => component.OnEquipped(data));

        private static readonly EventSystem.IntraObjectHandler<DarkMatterCore> OnUnequippedDelegate =
            new EventSystem.IntraObjectHandler<DarkMatterCore>((component, data) => component.OnUnequipped(data));

        protected override void OnPrefabInit()
        {
            base.OnPrefabInit();
            Subscribe((int)GameHashes.EquippedItemEquippable, OnEquippedDelegate);
            Subscribe((int)GameHashes.UnequippedItemEquippable, OnUnequippedDelegate);
        }

        private void OnEquipped(object data)
        {
            var equipment = (Equipment)data;
            wearer = equipment.GetComponent<MinionAssignablesProxy>().GetTargetGameObject();
            if (ModConfig.Instance.AllSkills)
            {
                GrantMissingPerks();
            }
            if (ModConfig.Instance.UnlockResearch)
            {
                UnlockAllResearch();
            }
        }

        // 科技全开：穿上服的瞬间，殖民地获得暗物质太空服蕴含的全部知识。
        // 幂等（已完成的跳过）；读档重新穿戴时再跑一遍也无害。
        private static void UnlockAllResearch()
        {
            if (Research.Instance == null)
            {
                return;
            }
            try
            {
                foreach (Tech tech in Db.Get().Techs.resources)
                {
                    TechInstance techInstance = Research.Instance.GetOrAdd(tech);
                    if (techInstance != null && !techInstance.IsComplete())
                    {
                        techInstance.Purchased();
                        Game.Instance.Trigger((int)GameHashes.ResearchComplete, tech);
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("[DarkMatterSuit] unlock research degraded: " + e);
            }
        }

        private void OnUnequipped(object data)
        {
            var equipment = (Equipment)data;
            if (wearer != null && !equipment.destroyed)
            {
                RemoveGrantedPerks();
            }
            wearer = null;
        }

        public void Sim1000ms(float dt)
        {
            var cfg = ModConfig.Instance;
            if (cfg.InfiniteOxygenFuel)
            {
                // 氧气回满（SuitTank 从 storage 扣氧）
                float o2 = storage.GetMassAvailable(suitTank.elementTag);
                if (o2 < suitTank.capacity)
                {
                    storage.AddGasChunk(SimHashes.Oxygen, suitTank.capacity - o2, 294.15f,
                        0, 0, keep_zero_mass: false, do_disease_transfer: false);
                }
                // 清除罐内积累的 CO2（密封服会把呼出的 CO2 存进 storage）
                if (storage.GetMassAvailable(CO2_TAG) > 0f)
                {
                    storage.ConsumeIgnoringDisease(CO2_TAG, float.MaxValue);
                }
            }

            // 读档后装备事件重放的兜底：穿戴中但 perk 清单为空时补授予
            if (cfg.AllSkills && wearer != null && grantedPerks.Count == 0)
            {
                GrantMissingPerks();
            }

            if (cfg.BionicSupport)
            {
                MaintainBionic();
            }
        }

        // 仿生复制人维生：仿生人不用普通氧气/卡路里/膀胱，而是电池/氧气罐/机油/淤泥。
        // 普通需求冻结(装备 delta 修正)对它无效，需单独维持：电池/氧气罐/机油补满、淤泥清零。
        private void MaintainBionic()
        {
            if (wearer == null || wearer.PrefabID() != BionicMinionConfig.ID)
            {
                return;
            }
            Database.Amounts amounts = Db.Get().Amounts;
            Fill(amounts.BionicInternalBattery); // 电池：用 debugSetValue 同步充电 storage
            Fill(amounts.BionicOxygenTank);      // 氧气罐：用 debugSetValue 同步 gas storage
            Fill(amounts.BionicOil);             // 机油
            Drain(amounts.BionicGunk);           // 淤泥清零
        }

        private void Fill(Amount amt)
        {
            AmountInstance inst = amt?.Lookup(wearer);
            if (inst == null)
            {
                return;
            }
            float max = inst.GetMax();
            if (amt.debugSetValue != null)
            {
                amt.debugSetValue(inst, max);
            }
            else
            {
                inst.value = max;
            }
        }

        private void Drain(Amount amt)
        {
            AmountInstance inst = amt?.Lookup(wearer);
            if (inst == null)
            {
                return;
            }
            if (amt.debugSetValue != null)
            {
                amt.debugSetValue(inst, 0f);
            }
            else
            {
                inst.value = 0f;
            }
        }

        private void GrantMissingPerks()
        {
            MinionResume resume = GetResume();
            if (resume == null)
            {
                return;
            }
            foreach (SkillPerk perk in Db.Get().SkillPerks.resources)
            {
                if (!resume.HasPerk(perk))
                {
                    resume.ApplyAdditionalSkillPerks(new[] { perk });
                    grantedPerks.Add(perk);
                }
            }
        }

        private void RemoveGrantedPerks()
        {
            MinionResume resume = GetResume();
            if (resume != null)
            {
                foreach (SkillPerk perk in grantedPerks)
                {
                    resume.RemoveAdditionalSkillPerks(new[] { perk });
                }
            }
            grantedPerks.Clear();
        }

        private MinionResume GetResume()
        {
            return wearer == null ? null : wearer.GetComponent<MinionResume>();
        }
    }
}
