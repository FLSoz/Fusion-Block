using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine;
using System.Collections.Generic;
using FusionBlock.ModuleLoaders;
using NLog;
using LogManager;

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
                LogTarget target = TTLogManager.RegisterLoggingTarget("Fusion Blocks", new TargetConfig
                {
                    layout = "${longdate} | ${level:uppercase=true:padding=-5:alignmentOnTruncation=left} | ${logger:shortName=true} | ${message}  ${exception}"
                });
                JSONModuleFuseHalfLoader.ConfigureLogger(target);
                ModuleFuseHalf.ConfigureLogger(target);
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