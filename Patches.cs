using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace DarkMatterSuit
{
    public static class Patches
    {
        // 字符串注册：Db.Initialize 在装备注册（Assets.CreatePrefabs → LegacyModMain.Load）之前完成，
        // Postfix 时机安全（与 EquipmentDef.Name 的 Strings.Get 查询时序兼容）
        [HarmonyPatch(typeof(Db), nameof(Db.Initialize))]
        public static class Db_Initialize_Patch
        {
            public static void Postfix()
            {
                // 配置面板字符串（按当前语言填入，供 PLib [Option] 的 STRINGS key 查询）
                ModStrings.RegisterConfigStrings();

                // 暗物质服（装备）字符串
                string prefix = "STRINGS.EQUIPMENT.PREFABS." + DarkMatterSuitConfig.ID.ToUpperInvariant() + ".";
                Strings.Add(new[] { prefix + "NAME", ModStrings.NAME });
                Strings.Add(new[] { prefix + "GENERICNAME", ModStrings.NAME });
                Strings.Add(new[] { prefix + "DESC", ModStrings.DESC });
                Strings.Add(new[] { prefix + "EFFECT", ModStrings.EFFECT });
                Strings.Add(new[] { prefix + "RECIPE_DESC", ModStrings.RECIPE_DESC });
            }
        }

        // 配方：工艺站，1 kg 沙子，5 秒（PRD F8）。
        // 真·零材料不可行：ComplexRecipeManager.DeriveRecipiesFromSource 无条件索引
        // ingredients[0]，空材料列表抛 IndexOutOfRangeException（已实测崩溃，见 docs/todo.md）
        [HarmonyPatch(typeof(CraftingTableConfig), "ConfigureRecipes")]
        public static class CraftingTable_ConfigureRecipes_Patch
        {
            public static void Postfix()
            {
                var input = new[]
                {
                    new ComplexRecipe.RecipeElement(SimHashes.Sand.CreateTag(), 1f)
                };
                var output = new[]
                {
                    new ComplexRecipe.RecipeElement(DarkMatterSuitConfig.TAG, 1f,
                        ComplexRecipe.RecipeElement.TemperatureOperation.AverageTemperature)
                };
                new ComplexRecipe(ComplexRecipeManager.MakeRecipeID(CraftingTableConfig.ID, input, output), input, output)
                {
                    time = 5f,
                    description = ModStrings.RECIPE_DESC,
                    nameDisplay = ComplexRecipe.RecipeNameDisplay.Result,
                    fabricators = new List<Tag> { CraftingTableConfig.ID },
                    sortOrder = 999,
                };
            }
        }

        // 不需要睡眠（其一）：作息表"就寝时段"对穿戴者失效。
        // 机制：UrgeMonitor 在 Schedulable.IsAllowed(Sleep)==true 时把睡眠 urge 阈值切到 100
        // （体力≤100 恒成立 → 到点必睡）；对穿戴者强制返回 false 后回到阈值 0，
        // 而体力被冻结在高位，睡眠 urge 永不触发。StaminaMonitor.ShouldExitSleep 同样
        // 读此方法 → 正在睡的穿戴者会被唤醒。仅拦截 Sleep 时段，其余时段走原逻辑。
        [HarmonyPatch(typeof(Schedulable), nameof(Schedulable.IsAllowed))]
        public static class Schedulable_IsAllowed_Patch
        {
            public static bool Prefix(Schedulable __instance, ScheduleBlockType schedule_block_type, ref bool __result)
            {
                if (ModConfig.Instance.NoSleep && schedule_block_type == Db.Get().ScheduleBlockTypes.Sleep && __instance.HasTag(DarkMatterSuitConfig.WEARER_TAG))
                {
                    __result = false;
                    return false;
                }
                return true;
            }
        }

        // 不需要睡眠（其二）：让穿戴者永远处于"工作时段"。
        // chore 的 IsScheduledTime 前置条件读 consumerState.scheduleBlock.IsAllowed(type)，
        // 是 scheduleBlock 的唯一外部读取方。给穿戴者替换为常驻原版 Worktime 组的日程块
        // （该组只允许 Work 类型），等价于全天候工作时段。
        // 注意不能置 null（?? true 全放行）：实测会把娱乐类 chore（放松/闲聊）一起放进来，
        // 其优先级类高于标准工作，穿戴者会被"放松"锁死不干活。
        [HarmonyPatch(typeof(ChoreConsumerState), nameof(ChoreConsumerState.Refresh))]
        public static class ChoreConsumerState_Refresh_Patch
        {
            private static ScheduleBlock alwaysWork;

            public static void Postfix(ChoreConsumerState __instance)
            {
                if (ModConfig.Instance.NoSleep
                    && __instance.scheduleBlock != null
                    && __instance.gameObject != null
                    && __instance.gameObject.HasTag(DarkMatterSuitConfig.WEARER_TAG))
                {
                    if (alwaysWork == null)
                    {
                        alwaysWork = new ScheduleBlock("DarkMatterAllWork", "Worktime");
                    }
                    __instance.scheduleBlock = alwaysWork;
                }
            }
        }

        // 自由行·水域（"不能去水里"）：原版把液体格一律判为悬浮无效
        // （FlyingValidator.UpdateCell: !Grid.IsSubstantialLiquid(cell)），而水体内部格子
        // Floor 也无效（脚下非实体）——水中对所有寻路都不可达。本 patch 给悬浮放开液体格：
        // 复刻原判定、仅去掉液体排除，穿戴者（持 HasJetPack flag）即可在水中飞行。
        // 用 exclude_jet_suit_blockers==true 这一复制人网格独有的构造指纹限定实例，
        // 不影响机器人网格。已知边界：穿原版喷气服的复制人同样获得水中飞行（NavGrid 全局共享）。
        [HarmonyPatch(typeof(GameNavGrids.FlyingValidator), nameof(GameNavGrids.FlyingValidator.UpdateCell))]
        public static class FlyingValidator_UpdateCell_Patch
        {
            private static readonly Func<NavTableValidator, int, CellOffset[], bool, bool> IsClearFn =
                AccessTools.MethodDelegate<Func<NavTableValidator, int, CellOffset[], bool, bool>>(
                    AccessTools.Method(typeof(NavTableValidator), "IsClear"));

            public static void Postfix(GameNavGrids.FlyingValidator __instance, int cell, NavTable nav_table,
                CellOffset[] bounding_offsets, bool ___exclude_floor, bool ___exclude_jet_suit_blockers,
                bool ___allow_door_traversal)
            {
                if (!ModConfig.Instance.FlyInLiquid || !___exclude_jet_suit_blockers)
                {
                    return; // 配置关闭水中飞行 / 非复制人网格的验证器实例
                }
                // 原方法对液体格已置 invalid，这里只补液体格的重新判定
                if (!Grid.IsWorldValidCell(Grid.CellAbove(cell)) || !Grid.IsSubstantialLiquid(cell))
                {
                    return;
                }
                bool valid = IsClearFn(__instance, cell, bounding_offsets, ___allow_door_traversal);
                if (valid && ___exclude_floor)
                {
                    int below = Grid.CellBelow(cell);
                    if (Grid.IsWorldValidCell(below))
                    {
                        valid = IsClearFn(__instance, below, bounding_offsets, ___allow_door_traversal);
                    }
                }
                if (valid && Grid.Objects[cell, 1] is GameObject go && go.HasTag(GameTags.JetSuitBlocker))
                {
                    valid = false;
                }
                if (valid)
                {
                    nav_table.SetValid(cell, NavType.Hover, true);
                }
            }
        }

        // 飞行提速：原版飞行(hover)速度固定 jetPackSpeed=7，比 +1000 属性的地面跑步慢很多。
        // 穿戴者飞行时把该字段提到 20（脱戴者恢复 7），每次 BeginTransition 按当前状态设。
        [HarmonyPatch(typeof(BipedTransitionLayer), "BeginTransition")]
        public static class BipedTransitionLayer_BeginTransition_Patch
        {
            private const float VANILLA_FLY_SPEED = 7f;

            private static readonly AccessTools.FieldRef<BipedTransitionLayer, float> JetPackSpeedRef =
                AccessTools.FieldRefAccess<BipedTransitionLayer, float>("jetPackSpeed");

            public static void Prefix(BipedTransitionLayer __instance, Navigator navigator)
            {
                bool isWearer = navigator != null && navigator.HasTag(DarkMatterSuitConfig.WEARER_TAG);
                JetPackSpeedRef(__instance) = isWearer ? ModConfig.Instance.FlySpeed : VANILLA_FLY_SPEED;
            }
        }

        // 快速连锁挖掘（分帧安全版）：穿戴者完成一格挖掘时，把半径内已标记待挖的格子
        // 入队，由每帧处理器每帧只挖少量。之前一帧直接 ApplyDamage 多达 288 格，导致
        // sim 的 cell 变化回调句柄失效崩溃（mismatched handle version）。分帧后同帧
        // 改动的 solid 格子大幅减少，不再触发句柄风暴；配合 +1000 挖掘与瞬移仍极快。
        internal static class ChainDig
        {
            private const int PER_FRAME = 8; // 每帧最多挖几格（保守，避免同帧批量）

            private static readonly Queue<int> queue = new Queue<int>();
            private static readonly HashSet<int> queued = new HashSet<int>();

            public static void EnqueueAround(int centerCell)
            {
                int radius = ModConfig.Instance.ChainDigRadius;
                if (radius <= 0)
                {
                    return; // 配置关闭连锁挖掘
                }
                Vector2I c = Grid.CellToXY(centerCell);
                for (int dy = -radius; dy <= radius; dy++)
                {
                    for (int dx = -radius; dx <= radius; dx++)
                    {
                        if (dx == 0 && dy == 0)
                        {
                            continue;
                        }
                        int cell = Grid.XYToCell(c.x + dx, c.y + dy);
                        if (Grid.IsValidCell(cell) && queued.Add(cell))
                        {
                            queue.Enqueue(cell);
                        }
                    }
                }
            }

            public static void Process()
            {
                int n = 0;
                while (queue.Count > 0 && n < PER_FRAME)
                {
                    int cell = queue.Dequeue();
                    queued.Remove(cell);
                    n++;
                    if (!Grid.IsValidCell(cell) || !Grid.Solid[cell])
                    {
                        continue;
                    }
                    if (Diggable.GetDiggable(cell) == null) // 只挖玩家已标记的
                    {
                        continue;
                    }
                    if (Grid.Element[cell].hardness == byte.MaxValue) // 中子素等不可挖
                    {
                        continue;
                    }
                    WorldDamage.Instance.ApplyDamage(cell, 1f, -1, WorldDamage.DamageType.Absolute);
                }
            }
        }

        [HarmonyPatch(typeof(Diggable), "OnWorkTick")]
        public static class Diggable_OnWorkTick_Patch
        {
            public static void Postfix(Diggable __instance, WorkerBase worker, bool __result)
            {
                // __result == true 表示本 tick 挖完了这格
                if (!__result || worker == null || !worker.HasTag(DarkMatterSuitConfig.WEARER_TAG))
                {
                    return;
                }
                ChainDig.EnqueueAround(Grid.PosToCell(__instance));
            }
        }

        // 每帧处理连锁挖掘队列（队列空时立即返回，开销可忽略）
        [HarmonyPatch(typeof(Game), "Update")]
        public static class Game_Update_ChainDig_Patch
        {
            public static void Postfix()
            {
                ChainDig.Process();
            }
        }

        // 永远穿着（PRD F7）：检查点的强制脱衣对本服失效。
        // 目标是 SuitMarker 的私有嵌套类，用 TargetMethod 定位
        [HarmonyPatch]
        public static class UnequipSuitReactable_InternalCanBegin_Patch
        {
            public static System.Reflection.MethodBase TargetMethod()
            {
                Type inner = AccessTools.Inner(typeof(SuitMarker), "UnequipSuitReactable");
                return AccessTools.Method(inner, "InternalCanBegin");
            }

            public static bool Prefix(GameObject newReactor, ref bool __result)
            {
                if (!ModConfig.Instance.NeverUnequip)
                {
                    return true;
                }
                try
                {
                    MinionIdentity identity = newReactor.GetComponent<MinionIdentity>();
                    Assignable suit = identity?.GetEquipment()?.GetAssignable(Db.Get().AssignableSlots.Suit);
                    if (suit != null && suit.PrefabID() == DarkMatterSuitConfig.TAG)
                    {
                        __result = false;
                        return false; // 跳过原判定：穿本服者不在检查点脱衣
                    }
                }
                catch (Exception e)
                {
                    // 降级：判定失败则放行原逻辑，最多退化为"会被脱衣"，不崩游戏
                    Debug.LogWarning("[DarkMatterSuit] KeepSuitOn patch degraded: " + e);
                }
                return true;
            }
        }

        // 免伤（真无敌）：所有伤害源都经 Health.Damage（坠落/过热/辐射/战斗等），
        // 穿戴者直接跳过扣血。
        [HarmonyPatch(typeof(Health), nameof(Health.Damage))]
        public static class Health_Damage_Patch
        {
            public static bool Prefix(Health __instance)
            {
                if (ModConfig.Instance.Invincible && __instance.gameObject.HasTag(DarkMatterSuitConfig.WEARER_TAG))
                {
                    return false; // 跳过扣血
                }
                return true;
            }
        }

        // 免疫疾病与病菌：患病统一经 Sicknesses.Infect，穿戴者直接跳过感染。
        [HarmonyPatch(typeof(Klei.AI.Sicknesses), nameof(Klei.AI.Sicknesses.Infect))]
        public static class Sicknesses_Infect_Patch
        {
            public static bool Prefix(Klei.AI.Sicknesses __instance)
            {
                if (ModConfig.Instance.DiseaseImmunity && __instance.gameObject.HasTag(DarkMatterSuitConfig.WEARER_TAG))
                {
                    return false; // 跳过感染
                }
                return true;
            }
        }
    }
}
