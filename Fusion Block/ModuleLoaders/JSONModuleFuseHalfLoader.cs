using System;
using Newtonsoft.Json.Linq;

namespace FusionBlock.ModuleLoaders
{
    public class JSONModuleFuseHalfLoader : JSONModuleLoader
    {
        internal static Logger logger;
        internal static void ConfigureLogger(Logger.TargetConfig targetConfig)
        {
            logger = new Logger("ModuleFuseHalfLoader", targetConfig);
            logger.Info("Logger setup");
        }

        public override bool CreateModuleForBlock(int blockID, ModdedBlockDefinition def, TankBlock block, JToken data)
        {
            logger.Trace(data.ToString());
            if (data.Type == JTokenType.Object)
            {
                JObject obj = (JObject)data;
                try
                {
                    ModuleFuseHalf fuser = base.GetOrAddComponent<ModuleFuseHalf>(block);
                    fuser.ModelForwardPairing = CustomParser.LenientTryParseFloat(obj, "ModelForwardPairing", fuser.ModelForwardPairing);
                    fuser.ModelForwardSignificance = CustomParser.LenientTryParseBool(obj, "ModelForwardSignificance", fuser.ModelForwardSignificance);
                    fuser.MakeSubstitiute = CustomParser.LenientTryParseBool(obj, "MakeSubstitute", fuser.MakeSubstitiute);
                    
                    fuser.SubstituteType = (BlockTypes)CustomParser.LenientTryParseInt(obj, "SubstituteID", (int) fuser.SubstituteType);
                    fuser.SubstituteID = (int) fuser.SubstituteType;
                    JObject name = base.TryGetObject(obj, "SubstituteName");
                    if (name != null)
                    {
                        fuser.SubstituteName = name.ToString();
                    }

                    fuser.Separator = CustomParser.LenientTryParseBool(obj, "Separator", fuser.Separator);
                    if (fuser.Separator)
                    {
                        logger.Trace("Block is a Separator!");
                    }
                    fuser.JoinOffset = CustomParser.LenientTryParseVector3(obj, "JoinOffset", fuser.JoinOffset);

                    if (CustomParser.LenientTryParseBool(obj, "AddDetachableLink", false))
                    {
                        base.GetOrAddComponent<ModuleDetachableLink>(block);
                    }

                    return true;
                }
                catch (Exception e)
                {
                    logger.Error(e);
                    logger.Error("Destroying added ModuleFuseHalf");
                    ModuleFuseHalf failedModule = block.GetComponent<ModuleFuseHalf>();
                    if (failedModule != null)
                    {
                        UnityEngine.GameObject.Destroy(failedModule);
                    }
                    return false;
                }
            }
            return false;
        }

        public override string GetModuleKey()
        {
            return "ModuleFuseHalf";
        }
    }
}
