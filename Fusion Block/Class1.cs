using Harmony;
using Nuterra.BlockInjector;
using System;
using System.Reflection;
using UnityEngine;
using System.Collections.Generic;

namespace FusionBlock
{
    public static class Class1
    {
        public static void CreateBlocks()
        {
            {
                new BlockPrefabBuilder("GSOBlock_111")
                    .SetBlockID(98341)
                    .SetName("Fusion Block").SetDescription("Press this up to another of its kind and it will merge the two techs!\n"+
                    "Both techs must be facing the same direction, and the block must be oriented to lock together like puzzle pieces. "+
                    "Once that's done, it will turn fuse into a standard block!")
                    .SetMass(0.5f).SetHP(125).SetGrade(1)
                    .SetSize(IntVector3.one, BlockPrefabBuilder.AttachmentPoints.Bottom)
                    .SetPrice(144)
                    .SetFaction(FactionSubTypes.GSO).SetCategory(BlockCategories.Accessories)
                    .SetIcon(GameObjectJSON.SpriteFromImage(GameObjectJSON.ImageFromFile(Properties.Resources.fusionblock_png)))
                    .SetModel(GameObjectJSON.MeshFromData(Properties.Resources.Fusion_Block), true, GameObjectJSON.GetObjectFromGameResources<Material>("GSO_Main"))
                    .AddComponent<ModuleFuseHalf>(out ModuleFuseHalf moduleFuse)
                    .RegisterLater();
                moduleFuse.ModelForwardPairing = -1;
                moduleFuse.ModelForwardSignificance = true;
                moduleFuse.MakeSubstitiute = true;
                moduleFuse.SubstituteType = BlockTypes.GSOBlock_111;
                moduleFuse.JoinOffset = Vector3.zero;

                CustomRecipe.RegisterRecipe(
                    new CustomRecipe.RecipeInput[] {
                        new CustomRecipe.RecipeInput(0), // Fibrewood
                        new CustomRecipe.RecipeInput(4), // Plumbite
                        new CustomRecipe.RecipeInput(2, 2) // Rubber Jelly
                    }, new CustomRecipe.RecipeOutput[] {
                        new CustomRecipe.RecipeOutput(98341)
                    });
            }
            {
                new BlockPrefabBuilder("GSOBlock_111")
                    .SetBlockID(98342)
                    .SetName("Fusion Bolt").SetDescription("Press this up to another of its kind and it will merge the two techs!\n" +
                    "Both techs must be facing the same direction, and the block must be facing eachother. " +
                    "Once that's done, it will turn fuse into an exploding bolt!\n\nExploding may still occur while unfused")
                    .SetMass(0.125f).SetHP(50).SetGrade(3)
                    .SetSize(IntVector3.one, BlockPrefabBuilder.AttachmentPoints.Bottom)
                    .SetPrice(324)
                    .SetFaction(FactionSubTypes.GSO).SetCategory(BlockCategories.Accessories)
                    .SetIcon(GameObjectJSON.SpriteFromImage(GameObjectJSON.ImageFromFile(Properties.Resources.fusionbolt_png)))
                    .SetModel(GameObjectJSON.MeshFromData(Properties.Resources.Fusion_Bolt), true, GameObjectJSON.GetObjectFromGameResources<Material>("GSO_Main"))
                    .AddComponent<ModuleFuseHalf>(out ModuleFuseHalf moduleFuse)
                    .AddComponent<ModuleDetachableLink>()
                    .RegisterLater();
                moduleFuse.ModelForwardSignificance = false;
                moduleFuse.MakeSubstitiute = true;
                moduleFuse.SubstituteType = BlockTypes.GSO_Exploder_A_111;
                moduleFuse.JoinOffset = Vector3.zero;

                CustomRecipe.RegisterRecipe(
                    new CustomRecipe.RecipeInput[] {
                        new CustomRecipe.RecipeInput(1), // Fibron Chunk
                        new CustomRecipe.RecipeInput(9), // Carbius Brick
                        new CustomRecipe.RecipeInput(2, 2) // Rubber Jelly
                    }, new CustomRecipe.RecipeOutput[] {
                        new CustomRecipe.RecipeOutput(98342)
                    });

            }
            {
                new BlockPrefabBuilder("BF_Block_111")
                    .SetBlockID(98343)
                    .SetName("Fusion Plate").SetDescription("Press this up to another of its kind and it will merge the two techs!\n" +
                    "Both techs must be facing the same direction, and the block must be facing eachother. " +
                    "Once that's done, the halves will melt away and the techs will be merged!")
                    .SetMass(0.45f).SetHP(150).SetGrade(1)
                    .SetSize(IntVector3.one, BlockPrefabBuilder.AttachmentPoints.Bottom)
                    .SetPrice(312)
                    .SetFaction(FactionSubTypes.BF).SetCategory(BlockCategories.Accessories)
                    .SetIcon(GameObjectJSON.SpriteFromImage(GameObjectJSON.ImageFromFile(Properties.Resources.fusionplate_png)))
                    .SetModel(GameObjectJSON.MeshFromData(Properties.Resources.Fusion_Plate), true, GameObjectJSON.GetObjectFromGameResources<Material>("BF_Main"))
                    .AddComponent<ModuleFuseHalf>(out ModuleFuseHalf moduleFuse)
                    .RegisterLater();
                moduleFuse.ModelForwardSignificance = false;
                moduleFuse.MakeSubstitiute = false;
                moduleFuse.JoinOffset = Vector3.down;

                CustomRecipe.RegisterRecipe(
                    new CustomRecipe.RecipeInput[] {
                        new CustomRecipe.RecipeInput(32), // Luxian Crystal
                        new CustomRecipe.RecipeInput(43), // Fibre Plating
                        new CustomRecipe.RecipeInput(2, 2) // Rubber Jelly
                    }, new CustomRecipe.RecipeOutput[] {
                        new CustomRecipe.RecipeOutput(98343)
                    }, NameOfFabricator:"bffab");
            }
        }

        public class ModuleFuseHalf : Module
        {
            // (1 or -1) Should the front of the block need to be the back for the other? Model specific
            public float ModelForwardPairing = 1;
            // Does what direction the two halves are facing actually matter
            public bool ModelForwardSignificance = false;


            // Substitute block, the result of meging two halves
            public bool MakeSubstitiute = true;
            public BlockTypes SubstituteType = BlockTypes.GSOBlock_111;

            // How to offset the tech being joined. Move it down in a block?
            public Vector3 JoinOffset = Vector3.zero;

            public void Update()
            {
                if (block.tank != null) // On Tech
                {
                    var results = Physics.OverlapSphere(transform.position + transform.TransformDirection(JoinOffset * 0.5f), 0.4f, ManVisible.inst.VisiblePickerMask);
                    for (int I = 0; I < results.Length; I++)
                    {
                        ModuleFuseHalf other =  results[I].GetComponentInParent<ModuleFuseHalf>();
                        if (other != null && // Exists
                            other != this && // Not this
                            other.block.tank != null && // On tech
                            other.block.BlockType == block.BlockType && // Same type
                            other.block.tank.rbody.mass <= block.tank.rbody.mass) // Superiority
                        {
                            //Console.WriteLine($"Two fusionblocks have been located");
                            AttemptMerge(other);
                            return;
                        }
                    }
                }
            }

            public void AttemptMerge(ModuleFuseHalf other)
            {
                // Dot products are basically "Hey how much in this vector is this vector?" 
                // Magnitude relevance of two vectors based on how alike their directions are. 
                // Parallel are A.mag x B.mag, parallel but reverse is -(A.mag x B.mag), right-angle vectors are just 0.
                // Here dot products are being used to estimate how alike these 1-length directions are to one another.
                var a = ModelForwardSignificance ? Vector3.Dot(other.transform.forward, ModelForwardPairing * transform.forward) : 1f; // If it matters, are the blocks facing the right way
                var b = Vector3.Dot(other.transform.up, -transform.up); // Both blocks must be facing upwards at eachother
                var c = Vector3.Dot(other.block.tank.transform.forward, block.tank.transform.forward); // Both techs must be vertically relevant.
                var d = Vector3.Dot(other.block.tank.transform.up, block.tank.transform.up); // Both techs must be in the same general direction. I probably could just use quaternion math.

                //Console.WriteLine($"Testing if sacred ritual may commense:\n Block forward matching = {a}\n Block upward matching = {b}\n Tank forward matching = {c}\n Tank upward matching = {d}");
                if (a > 0.85f &&
                    b > 0.85f &&
                    c > 0.85f &&
                    d > 0.85f)
                {
                    Console.WriteLine($"Commencing the sacred ritual:\n Block forward matching = {a}\n Block upward matching = {b}\n Tank forward matching = {c}\n Tank upward matching = {d}"); //Commence the sacred ritual

                    Tank tankA = block.tank, tankB = other.block.tank; // Tank cache. Because they would be lost without it
                    if (Singleton.playerTank == tankB) // If player is tankB it's going to null
                        Singleton.SetPlayerTankInternal(tankA); // Change it to this tank, because this one is the merge host
                    Vector3 cachedMergePos = block.cachedLocalPosition; // Where to put the substitute block
                    OrthoRotation cachedMergeRot = block.cachedLocalRotation; // How to put the substitute block
                    List<TankBlock> array = GetSafeBlockStep(other.block); // Iterate the other tech's blocks to get a way to add them all

                    block.tank.blockman.Detach(block, true, false, false); // Remove this block
                    other.block.tank.blockman.Detach(other.block, true, false, false); // Remove that block

                    Quaternion hecku; // Fricken' detest quaternions, not going to try and figure out all that inverse transformation frick

                    hecku = tankA.transform.rotation; // Yeah that's right get shunned
                    tankB.transform.rotation = tankA.transform.rotation;  // They are supposed to be in the same direction anyways so this should be fine
                    tankB.transform.position += transform.position - other.transform.position - (transform.TransformDirection(JoinOffset)); // Move the tech by the offset of the two blocks, and join offset

                    tankB.blockman.Disintegrate(false, false); // Melt that bad boy
                    if (MakeSubstitiute) // Does this have a block to go between or is this one of those glue kinds of fusing 
                    {
                        TankBlock mergedBlock = ManSpawn.inst.SpawnBlock(SubstituteType, Vector3.zero, Quaternion.identity); // Create that substitute
                        mergedBlock.SetSkinIndex(block.GetSkinIndex()); // Set that skin so it is pretty
                        tankA.blockman.AddBlockToTech(mergedBlock, cachedMergePos, cachedMergeRot); // Put that block where it belongs
                    }

                    foreach (TankBlock sb in array) // Iterate from the array back up there from that tech, but now on this tech
                    {
                        tankA.blockman.AddBlockToTech(sb, new IntVector3(tankA.transform.InverseTransformPoint(sb.transform.position)), sb.cachedLocalRotation); // Add the block, using the block's rotation from memory, and abusing IntVector3's rounding, because I don't trust myself
                    }
                    tankA.transform.rotation = hecku; // Ok you can come back now



                    block.damage.Explode(false); // Explode this
                    other.block.damage.Explode(false); // Explode that
                    block.visible.RemoveFromGame(); // Rid of this
                    other.block.visible.RemoveFromGame(); // Rid of that
                }
            }

            static List<TankBlock> GetSafeBlockStep(TankBlock StartBlock)
            {
                List<TankBlock> tankBlocks = new List<TankBlock>();
                RecursiveBlockStep(StartBlock, tankBlocks); // Iterate
                return tankBlocks;
            }

            static void RecursiveBlockStep(TankBlock CurrentBlock, List<TankBlock> List)
            {
                List<TankBlock> tempList = new List<TankBlock>(); // Temporary list for not a very good reason
                foreach (TankBlock Block in CurrentBlock.ConnectedBlocksByAP) 
                {
                    if (Block == null || List.Contains(Block))
                    {
                        continue; // Skip blocks which do not exist, or already have been added
                    }
                    List.Add(Block);
                    tempList.Add(Block);
                }
                foreach (TankBlock Block in tempList)
                {
                    RecursiveBlockStep(Block, List); // Continue the process until there are none left
                }
            }
        }
    }
}