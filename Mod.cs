using HarmonyLib;
using PeterHan.PLib.Core;
using PeterHan.PLib.Options;

namespace DarkMatterSuit
{
    public class Mod : KMod.UserMod2
    {
        public override void OnLoad(Harmony harmony)
        {
            base.OnLoad(harmony); // PatchAll
            PUtil.InitLibrary(false);
            new POptions().RegisterOptions(this, typeof(ModConfig));
            ModConfig.Instance = POptions.ReadSettings<ModConfig>() ?? new ModConfig();
        }
    }
}
