using HarmonyLib;
using FusionBlock.ModuleLoaders;

namespace FusionBlock
{
    public class FusionBlocksMod : ModBase
    {
        internal const string HarmonyID = "aceba1.fusionblock";
        internal static Harmony harmony = new Harmony(HarmonyID);
        internal static bool Inited = false;

        public override bool HasEarlyInit()
        {
            return true;
        }

        public override void EarlyInit()
        {
            if (!Inited)
            {
                Logger.TargetConfig targetConfig = new Logger.TargetConfig() {
                    filename = "FusionBlocks",
                    layout = "${longdate} | ${level:uppercase=true:padding=-5:alignmentOnTruncation=left} | ${logger:shortName=true} | ${message}  ${exception}"
                };

                JSONModuleFuseHalfLoader.ConfigureLogger(targetConfig);
                ModuleFuseHalf.ConfigureLogger(targetConfig);
                Inited = true;
            }
        }

        public override void Init()
        {
            JSONBlockLoader.RegisterModuleLoader(new JSONModuleFuseHalfLoader());
            harmony.PatchAll();
        }

        public override void DeInit()
        {
            harmony.UnpatchAll(HarmonyID);
        }
    }
}