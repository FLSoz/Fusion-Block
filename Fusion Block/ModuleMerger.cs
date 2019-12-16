using System;
using System.Collections.Generic;
using UnityEngine;

namespace FusionBlock
{
    public static partial class Class1
    {
        public class ModuleFuseHalf : Module
        {
            // (1 or -1) Should the front of the block need to be the back for the other? Model specific
            public float ModelForwardPairing = 1;
            // Does what direction the two halves are facing actually matter
            public bool ModelForwardSignificance = false;
            public bool Separator = false;


            // Substitute block, the result of meging two halves
            public bool MakeSubstitiute = true;
            public BlockTypes SubstituteType = BlockTypes.GSOBlock_111;

            // How to offset the tech being joined. Move it down in a block?
            public Vector3 JoinOffset = Vector3.zero;

            void OnPool()
            {
                if (Separator)
                {
                    block.AttachEvent.Subscribe(OnAttach);
                    block.DetachEvent.Subscribe(OnDetach);
                }
            }

            void OnSpawn()
            {
                Detonated = false;
                blockA = null;
                blockB = null;
                cachedBlockAOffset = Vector3.zero;
                cachedBlockBOffset = Vector3.zero;
                cachedWorldPos = Vector3.zero;
                cachedSplitRot = OrthoRotation.identity;
                cachedWorldRot = Quaternion.identity;

            }

            void OnAttach()
            {
                block.tank.control.explosiveBoltDetonateEvent.Subscribe(Detonate);
            }
            void OnDetach()
            {
                block.tank.control.explosiveBoltDetonateEvent.Unsubscribe(Detonate);
            }

            public void Update()
            {
                if (Detonated)
                {
                    Detonated = false;
                    return;
                }
                if (block.tank != null) // On Tech
                {
                    if (Separator)
                    {
                        return;
                    }
                    else
                    {
                        var results = Physics.OverlapSphere(transform.position + transform.TransformDirection(JoinOffset * 0.5f), 0.4f, ManVisible.inst.VisiblePickerMask);
                        for (int I = 0; I < results.Length; I++)
                        {
                            ModuleFuseHalf other = results[I].GetComponentInParent<ModuleFuseHalf>();
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

                    Quaternion hecku; // Not going to try and figure out all that inverse transformation frick

                    hecku = tankA.transform.rotation; // Yeah that's right get shunned
                    tankB.transform.rotation = tankA.transform.rotation;  // They are supposed to be in the same direction anyways so this should be fine
                    tankB.transform.position += transform.position - other.transform.position + (transform.TransformDirection(JoinOffset)); // Move the tech by the offset of the two blocks, and join offset

                    tankB.blockman.Disintegrate(false, false); // Melt that bad boy
                    if (MakeSubstitiute) // Does this have a block to go between or is this one of those glue kinds of fusing 
                    {
                        TankBlock mergedBlock = ManSpawn.inst.SpawnBlock(SubstituteType, Vector3.zero, Quaternion.identity); // Create that substitute
                        mergedBlock.SetSkinIndex(block.GetSkinIndex()); // Set that skin so it is pretty
                        mergedBlock.visible.damageable.InitHealth(block.visible.damageable.Health + other.block.visible.damageable.Health); // Set the health to both of the halves
                        tankA.blockman.AddBlockToTech(mergedBlock, cachedMergePos, cachedMergeRot); // Put that block where it belongs
                    }

                    List<TankBlock> retry = new List<TankBlock>();

                    foreach (TankBlock sb in array) // Iterate from the array back up there from that tech, but now on this tech
                    {
                        // Add the block, using the block's rotation from memory, and just use the block's positions
                        if (!tankA.blockman.AddBlockToTech(sb, block.cachedLocalPosition + sb.cachedLocalPosition - other.block.cachedLocalPosition + block.cachedLocalRotation * JoinOffset, sb.cachedLocalRotation)) /*new IntVector3(tankA.transform.InverseTransformPoint(sb.transform.position))*/
                            retry.Add(sb); // If it didn't attach, try again after
                    }

                    int retryCount = 2; // How many times to retry attaching
                    while (retryCount != 0 && retry.Count != 0) // While there are things left, and can retry
                    {
                        retryCount--; // Spend a token
                        int iter = 0; // Start the iterator
                        while (retry.Count > iter) // Go through the elements
                        {
                            var sb = retry[iter];
                            if (!tankA.blockman.AddBlockToTech(sb, block.cachedLocalPosition + sb.cachedLocalPosition - other.block.cachedLocalPosition + block.cachedLocalRotation * JoinOffset, sb.cachedLocalRotation))
                                iter++; // Skip
                            else
                                retry.RemoveAt(iter); // Move elements down, keep placement
                        }
                    }

                    tankA.transform.rotation = hecku; // Ok you can come back now



                    block.damage.Explode(false); // Explode this
                    other.block.damage.Explode(false); // Explode that
                    block.visible.RemoveFromGame(); // Rid of this
                    other.block.visible.RemoveFromGame(); // Rid of that
                }
            }

            TankBlock blockA, blockB;
            Vector3 cachedBlockAOffset, cachedBlockBOffset, cachedWorldPos;
            OrthoRotation cachedSplitRot;
            Quaternion cachedWorldRot;
            bool Detonated;

            private void Detonate(TechSplitNamer obj)
            {
                if (base.block.tank.beam.IsActive)
                {
                    base.block.tank.beam.EnableBeam(false, false, false);
                }
                blockA = block.ConnectedBlocksByAP[0]; blockB = block.ConnectedBlocksByAP[1];
                cachedWorldPos = block.trans.position;
                cachedSplitRot = block.cachedLocalRotation; cachedWorldRot = block.trans.rotation;
                if (blockA != null)
                    cachedBlockAOffset = block.cachedLocalPosition - blockA.cachedLocalPosition;
                if (blockB != null)
                    cachedBlockBOffset = block.cachedLocalPosition - blockB.cachedLocalPosition;

                if (blockB == null)
                    blockA.AttachEvent.Subscribe(AfterDetonate);
                else
                    blockB.AttachEvent.Subscribe(AfterDetonate);
                Singleton.Manager<ManLooseBlocks>.inst.HostDetachBlock(base.block, false, true);
                block.damage.Explode(false); // Explode this
                Detonated = true;
            }

            private void AfterDetonate()
            {
                if (!Detonated)
                {
                    Console.WriteLine("FusionBlock.ModuleMerger.AfterDetonate() has been called more than once!");
                }
                Detonated = false;

                if (blockB == null)
                    blockA.AttachEvent.Unsubscribe(AfterDetonate);
                else
                    blockB.AttachEvent.Unsubscribe(AfterDetonate);

                TankBlock halfBlockA = ManSpawn.inst.SpawnBlock(SubstituteType, cachedWorldPos, cachedWorldRot); // Create substitute #1
                TankBlock halfBlockB = ManSpawn.inst.SpawnBlock(SubstituteType, cachedWorldPos, cachedWorldRot * Quaternion.Euler(180, 0, 0)); // Create substitute #2
                halfBlockA.SetSkinIndex(block.GetSkinIndex());
                halfBlockB.SetSkinIndex(block.GetSkinIndex()); // Set that skins so they are pretty
                halfBlockA.visible.damageable.InitHealth(block.visible.damageable.Health / 2);
                halfBlockB.visible.damageable.InitHealth(block.visible.damageable.Health / 2); // Set the healths to halves of the whole

                block.visible.RemoveFromGame(); // Rid of this

                if (blockA != null && blockA.tank != null)
                    blockA.tank.blockman.AddBlockToTech(halfBlockA, blockA.cachedLocalPosition + cachedBlockAOffset, cachedSplitRot); // Put that block where it belongs
                if (blockB != null && blockB.tank != null)
                    blockB.tank.blockman.AddBlockToTech(halfBlockB, blockB.cachedLocalPosition + cachedBlockBOffset, new OrthoRotation(cachedSplitRot * Quaternion.Euler(180, 0, 0))); // Put that other block where it belongs
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
