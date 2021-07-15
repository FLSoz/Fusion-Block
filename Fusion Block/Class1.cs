using HarmonyLib;
using Nuterra.BlockInjector;
using System;
using System.Reflection;
using UnityEngine;
using System.Collections.Generic;

namespace FusionBlock
{
    public static partial class Class1
    {
        static Color rf_ExplosionColor = new Color(0f, 0.6f, 1f);
        static PhysicMaterial boxmat = new PhysicMaterial() { bounciness = 0f, dynamicFriction = 0.7f, staticFriction = 0.8f };
        static void AddBoxCollider(GameObject parent, Vector3 position, Vector3 rotation, Vector3 size)
        {
            var obj = new GameObject("m_BoxCollider" + parent.transform.childCount.ToString());
            obj.transform.parent = parent.transform;
            var box = obj.AddComponent<BoxCollider>();
            box.size = size;
            box.sharedMaterial = boxmat;
            obj.transform.localPosition = position;
            obj.transform.localRotation = Quaternion.Euler(rotation);
            obj.layer = Globals.inst.layerTank;
        }
        static void AddRoditeRecipeList() =>
            RecipeManager.inst.recipeTable.m_RecipeLists.Add(Reactors.ModuleReactorLoader.RecipeList);

        public static void CreateBlocks()
        {
            Harmony mod = new Harmony("aceba1.fusionblock");
            mod.PatchAll(System.Reflection.Assembly.GetExecutingAssembly());

            Texture2D rtex1 = GameObjectJSON.ImageFromFile(Properties.Resources.reactor_tex_1),
                      rtex2 = GameObjectJSON.ImageFromFile(Properties.Resources.reactor_tex_2),
                      rtex3 = GameObjectJSON.ImageFromFile(Properties.Resources.reactor_tex_3);
            Material gso_main = GameObjectJSON.GetObjectFromGameResources<Material>("GSO_Main"), bf_main = GameObjectJSON.GetObjectFromGameResources<Material>("BF_Main"), rf_main = GameObjectJSON.MaterialFromShader().SetTexturesToMaterial(rtex1, rtex2, rtex3, false), rf_glow = GameObjectJSON.MaterialFromShader("Legacy Shaders/Particles/Additive").SetTexturesToMaterial(rtex3);


            {
                new BlockPrefabBuilder("GSOBlock_111")
                    .SetBlockID(98341)
                    .SetName("Fusion Block").SetDescription("<i>(Vanilla-safe when merged)</i> Press this up to another of its kind and it will merge the two techs!\n" +
                    "Both techs must be facing the same direction, and the block must be oriented to lock together like puzzle pieces. "+
                    "Once that's done, it will fuse into a standard block!")
                    .SetMass(0.5f).SetHP(125).SetGrade(1)
                    .SetSize(IntVector3.one, BlockPrefabBuilder.AttachmentPoints.Bottom)
                    .SetPrice(144)
                    .SetFaction(FactionSubTypes.GSO).SetCategory(BlockCategories.Accessories)
                    .SetIcon(GameObjectJSON.SpriteFromImage(GameObjectJSON.ImageFromFile(Properties.Resources.fusionblock)))
                    .SetModel(GameObjectJSON.MeshFromData(Properties.Resources.Fusion_Block), true, gso_main)
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
                    .SetName("Fusion Bolt").SetDescription("<i>(Vanilla-safe when merged)</i> Press this up to another of its kind and it will merge the two techs!\n" +
                    "Both techs must be facing the same direction, and the block must be facing eachother. " +
                    "Once that's done, it will fuse into an exploding bolt!\n\nExploding may still occur while unfused")
                    .SetMass(0.125f).SetHP(50).SetGrade(3)
                    .SetSize(IntVector3.one, BlockPrefabBuilder.AttachmentPoints.Bottom)
                    .SetPrice(324)
                    .SetFaction(FactionSubTypes.GSO).SetCategory(BlockCategories.Accessories)
                    .SetIcon(GameObjectJSON.SpriteFromImage(GameObjectJSON.ImageFromFile(Properties.Resources.fusionbolt)))
                    .SetModel(GameObjectJSON.MeshFromData(Properties.Resources.Fusion_Bolt), true, gso_main)
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
                    .SetName("Fusion Plate").SetDescription("<i>(Vanilla-safe when merged)</i> Press this up to another of its kind and it will merge the two techs!\n" +
                    "Both techs must be facing the same direction, and the block must be facing eachother. " +
                    "Once that's done, the halves will melt away and the techs will be merged!")
                    .SetMass(0.45f).SetHP(150).SetGrade(1)
                    .SetSize(IntVector3.one, BlockPrefabBuilder.AttachmentPoints.Bottom)
                    .SetPrice(312)
                    .SetFaction(FactionSubTypes.BF).SetCategory(BlockCategories.Accessories)
                    .SetIcon(GameObjectJSON.SpriteFromImage(GameObjectJSON.ImageFromFile(Properties.Resources.fusionplate)))
                    .SetModel(GameObjectJSON.MeshFromData(Properties.Resources.Fusion_Plate), true, bf_main)
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
                    }, NameOfFabricator: "bffab");
            }
            {
                new BlockPrefabBuilder("HEBlock_111")
                    .SetBlockID(98350)
                    .SetName("Reactor Passive Blast Block").SetDescription("A dense heat-resistent block, that can permit the flow of rodius through all APs on it.")
                    .SetMass(3f).SetHP(400).SetGrade(2)
                    .SetSize(IntVector3.one, BlockPrefabBuilder.AttachmentPoints.All)
                    .SetPrice(400)
                    .SetFaction(FactionSubTypes.HE).SetCategory(BlockCategories.Base)
                    .SetIcon(GameObjectJSON.SpriteFromImage(GameObjectJSON.ImageFromFile(Properties.Resources.reactor_blast_block_passive)))
                    .SetModel(GameObjectJSON.MeshFromData(Properties.Resources.ReactorBlastBlockPassive), true, rf_main)
                    .AddComponent(out Reactors.ModuleReactorHolder moduleReactor)
                    .SetDamageableType(ManDamage.DamageableType.Rock)
                    .RegisterLater();
                moduleReactor.ThisCapacity = 0.5f;
            }
            {
                new BlockPrefabBuilder("HEBlock_111")
                    .SetBlockID(98356)
                    .SetName("Reactor Passive Cell Block").SetDescription("A dense heat-resistent block, which can hold more rodius within it than a passive blast block. Optimal for storage.")
                    .SetMass(6f).SetHP(400).SetGrade(2)
                    .SetSize(new IntVector3(1, 2, 1))
                    .SetAPsManual(new Vector3[] { new Vector3(0, -0.5f, 0f), new Vector3(0, 1.5f, 0f) })
                    .SetPrice(1200)
                    .SetFaction(FactionSubTypes.HE).SetCategory(BlockCategories.Base)
                    .SetIcon(GameObjectJSON.SpriteFromImage(GameObjectJSON.ImageFromFile(Properties.Resources.reactor_cell)))
                    .SetModel(GameObjectJSON.MeshFromData(Properties.Resources.ReactorCellPassive), true, rf_main)
                    .AddComponent(out Reactors.ModuleReactorHolder moduleReactor)
                    .SetDamageableType(ManDamage.DamageableType.Rock)
                    .RegisterLater();
                moduleReactor.ThisCapacity = 4f;
            }
            {
                new BlockPrefabBuilder("HEBlock_111")
                    .SetBlockID(98351)
                    .SetName("Reactor Blast Block").SetDescription("A dense heat-resistent block that can widthstand the jets radiated by a reactor ring. Or a flamethrower.")
                    .SetMass(3f).SetHP(550).SetGrade(2)
                    .SetSize(IntVector3.one, BlockPrefabBuilder.AttachmentPoints.All)
                    .SetPrice(400)
                    .SetFaction(FactionSubTypes.HE).SetCategory(BlockCategories.Base)
                    .SetIcon(GameObjectJSON.SpriteFromImage(GameObjectJSON.ImageFromFile(Properties.Resources.reactor_blast_block)))
                    .SetModel(GameObjectJSON.MeshFromData(Properties.Resources.ReactorBlastBlock), true, rf_main)
                    .SetDamageableType(ManDamage.DamageableType.Rock)
                    .RegisterLater();
            }
            {
                var coolerPrefab = new BlockPrefabBuilder("HEBlock_111")
                    .SetBlockID(98353)
                    .SetName("Reactor Cooler").SetDescription("It's a spinny!")
                    .SetMass(1f).SetHP(500).SetGrade(2)
                    .SetSize(IntVector3.one, BlockPrefabBuilder.AttachmentPoints.Bottom)
                    .SetPrice(400)
                    .SetFaction(FactionSubTypes.HE).SetCategory(BlockCategories.Accessories)
                    .SetIcon(GameObjectJSON.SpriteFromImage(GameObjectJSON.ImageFromFile(Properties.Resources.reactor_cooler)))
                    .SetModel(GameObjectJSON.MeshFromData(Properties.Resources.ReactorCooler), true, rf_main)
                    .AddComponent(out Reactors.ModuleReactorCooler moduleCooler)
                    .SetDamageableType(ManDamage.DamageableType.Rock);

                var fan = new GameObject("m_Spinner");
                fan.AddComponent<MeshFilter>().sharedMesh = GameObjectJSON.MeshFromData(Properties.Resources.ReactorCoolerFan);
                fan.AddComponent<MeshRenderer>().sharedMaterial = rf_main;
                fan.layer = Globals.inst.layerTank;
                var spinner = fan.AddComponent<Spinner>();
                var tS = typeof(Spinner);
                tS.GetField("m_Speed", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).SetValue(spinner, 5f);
                tS.GetField("m_AutoSpin", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).SetValue(spinner, true);
                tS.GetField("m_SpinUpTime", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).SetValue(spinner, 0.75f);
                spinner.m_RotationAxis = new Axis(Axis.AxisType.Y);
                spinner.m_SteerAxis = new Axis(Axis.AxisType.Y);
                fan.transform.parent = coolerPrefab.Prefab.transform;
                fan.transform.localPosition = Vector3.zero;

                coolerPrefab.RegisterLater();
            }
            {
                var mesh = GameObjectJSON.MeshFromData(Properties.Resources.ReactorRingIV);
                var ringIV = new BlockPrefabBuilder("HE_Battery_211")
                    .SetBlockID(98354)
                    .SetName("Reactor Ring IV").SetDescription("This is a rodite generator. It pulls rodite from the connected passive blocks and produces electrical energy. It also lets off jets of heat from the spinning core, which may melt your tech")
                    .SetMass(8f).SetHP(1000).SetGrade(2)
                    .SetSizeManual(new IntVector3[] 
                    { 
                        new IntVector3(0,0,0),new IntVector3(0,0,1),new IntVector3(0,0,2),new IntVector3(1,0,2),
                        new IntVector3(2,0,2),new IntVector3(2,0,1),new IntVector3(2,0,0),new IntVector3(1,0,0),
                    }, new Vector3[]
                    {
                        new Vector3(1f,-.5f,0f),new Vector3(2f,-.5f,1f),new Vector3(1f,-.5f,2f),new Vector3(0f,-.5f,1f),
                        new Vector3(1f,.5f,0f),new Vector3(2f,.5f,1f),new Vector3(1f,.5f,2f),new Vector3(0f,.5f,1f),

                        new Vector3(1f,0f,-0.5f),new Vector3(2.5f,0f,1f),new Vector3(1f,0f,2.5f),new Vector3(-.5f,0f,1f),
                    })
                    .SetPrice(400)
                    .SetFaction(FactionSubTypes.HE).SetCategory(BlockCategories.Base)
                    .SetIcon(GameObjectJSON.SpriteFromImage(GameObjectJSON.ImageFromFile(Properties.Resources.reactor_ring_iv)))
                    .SetModel(mesh, false, rf_main)
                    .AddComponent(out Reactors.ModuleReactorRing moduleEnergy)
                    .SetDamageableType(ManDamage.DamageableType.Rock)
                    .SetCenterOfMass(new Vector3(1f, 0f, 1f));

                AddBoxCollider(ringIV.Prefab, new Vector3(1f, 0f, -.05f), Vector3.zero, new Vector3(1.15f, 1f, .7f));
                AddBoxCollider(ringIV.Prefab, new Vector3(1f, 0f, 2.05f), Vector3.zero, new Vector3(1.15f, 1f, .7f));
                AddBoxCollider(ringIV.Prefab, new Vector3(-.05f, 0f, 1f), Vector3.zero, new Vector3(.7f, 1f, 1.15f));
                AddBoxCollider(ringIV.Prefab, new Vector3(2.05f, 0f, 1f), Vector3.zero, new Vector3(.7f, 1f, 1.15f));
                AddBoxCollider(ringIV.Prefab, new Vector3(1.73f, 0f, 0.27f), new Vector3(0, -45, 0), new Vector3(1.15f, 1f, .7f));
                AddBoxCollider(ringIV.Prefab, new Vector3(0.27f, 0f, 1.73f), new Vector3(0, -45, 0), new Vector3(1.15f, 1f, .7f));
                AddBoxCollider(ringIV.Prefab, new Vector3(0.27f, 0f, 0.27f), new Vector3(0, 45, 0), new Vector3(1.15f, 1f, .7f));
                AddBoxCollider(ringIV.Prefab, new Vector3(1.73f, 0f, 1.73f), new Vector3(0, 45, 0), new Vector3(1.15f, 1f, .7f));

                moduleEnergy.ThisCapacity = 1f;
                moduleEnergy.UseAPIDs = false;
                moduleEnergy.DrainPerSecond = 0.04f;
                moduleEnergy.JetCapsule1 = new Vector3(1f, 0.2f, 1f);
                moduleEnergy.JetCapsule2 = new Vector3(1f, -0.2f, 1f);
                moduleEnergy.JetCapsuleRad = 1f;
                moduleEnergy.JetCapsuleDamage = 45f;
                Component.DestroyImmediate(ringIV.Prefab.GetComponentInChildren<EnergyGauge>());
                var power = ringIV.Prefab.GetComponent<ModuleEnergyStore>();
                power.m_Capacity = 400;
                power.m_AcceptRemoteCharge = false;

                var daage = ringIV.Prefab.GetComponent<ModuleDamage>();
                daage.deathExplosion = Transform.Instantiate(daage.deathExplosion);
                var damage = daage.deathExplosion.GetComponent<Explosion>();
                damage.m_MaxDamageStrength = 1400;
                damage.m_EffectRadius = 8;
                damage.m_EffectRadiusMaxStrength = 6;
                var particles = daage.deathExplosion.GetComponentsInChildren<ParticleSystem>();
                foreach (var particle in particles)
                {
                    var m = particle.main;
                    m.startSpeedMultiplier *= 2f;
                    m.startSizeMultiplier *= 3f;
                    m.startColor = rf_ExplosionColor;
                }

                var jet = new GameObject("BlastJet");
                jet.AddComponent<MeshFilter>().sharedMesh = GameObjectJSON.MeshFromData(Properties.Resources.ReactorRingIVJet);
                jet.AddComponent<MeshRenderer>().sharedMaterial = rf_glow;
                jet.transform.parent = ringIV.Prefab.transform;
                jet.transform.localPosition = new Vector3(1f, 0f, 1f);

                ringIV.RegisterLater();
            }
            {
                var mesh = GameObjectJSON.MeshFromData(Properties.Resources.ReactorRingX);
                var ringX = new BlockPrefabBuilder("HE_Battery_211")
                    .SetBlockID(98355)
                    .SetName("Reactor Ring X").SetDescription("This is a more powerful rodite generator. It pulls rodite from the connected passive blocks and produces electrical energy. It also lets off jets of heat from the spinning core, which may melt your tech")
                    .SetMass(16f).SetHP(2000).SetGrade(2)
                    .SetSizeManual(new IntVector3[]
                    {
                        new IntVector3(0,0,0),new IntVector3(0,0,1),new IntVector3(0,0,2),new IntVector3(1,0,2),
                        new IntVector3(2,0,2),new IntVector3(2,0,1),new IntVector3(2,0,0),new IntVector3(1,0,0),

                        new IntVector3(0,1,0),new IntVector3(0,1,1),new IntVector3(0,1,2),new IntVector3(1,1,2),
                        new IntVector3(2,1,2),new IntVector3(2,1,1),new IntVector3(2,1,0),new IntVector3(1,1,0),
                    }, new Vector3[]
                    {
                        new Vector3(1f,-.5f,0f),new Vector3(2f,-.5f,1f),new Vector3(1f,-.5f,2f),new Vector3(0f,-.5f,1f),
                        new Vector3(1f,1.5f,0f),new Vector3(2f,1.5f,1f),new Vector3(1f,1.5f,2f),new Vector3(0f,1.5f,1f),

                        new Vector3(1f,0f,-0.5f),new Vector3(2.5f,0f,1f),new Vector3(1f,0f,2.5f),new Vector3(-.5f,0f,1f),
                        new Vector3(1f,1f,-0.5f),new Vector3(2.5f,1f,1f),new Vector3(1f,1f,2.5f),new Vector3(-.5f,1f,1f),
                    })
                    .SetPrice(400)
                    .SetFaction(FactionSubTypes.HE).SetCategory(BlockCategories.Base)
                    .SetIcon(GameObjectJSON.SpriteFromImage(GameObjectJSON.ImageFromFile(Properties.Resources.reactor_ring_x)))
                    .SetModel(mesh, false, rf_main)
                    .AddComponent(out Reactors.ModuleReactorRing moduleReactor)
                    .SetDamageableType(ManDamage.DamageableType.Rock)
                    .SetCenterOfMass(new Vector3(1f, 0.5f, 1f));

                AddBoxCollider(ringX.Prefab, new Vector3(1f, 0.5f, -.50f), Vector3.zero, new Vector3(1.15f, 2f, .7f));
                AddBoxCollider(ringX.Prefab, new Vector3(1f, 0.5f, 2.05f), Vector3.zero, new Vector3(1.15f, 2f, .7f));
                AddBoxCollider(ringX.Prefab, new Vector3(-.05f, 0.5f, 1f), Vector3.zero, new Vector3(.7f, 2f, 1.15f));
                AddBoxCollider(ringX.Prefab, new Vector3(2.05f, 0.5f, 1f), Vector3.zero, new Vector3(.7f, 2f, 1.15f));
                AddBoxCollider(ringX.Prefab, new Vector3(1.73f, 0f, 0.27f), new Vector3(0, -45, 0), new Vector3(1.15f, 2f, .7f));
                AddBoxCollider(ringX.Prefab, new Vector3(0.27f, 0f, 1.73f), new Vector3(0, -45, 0), new Vector3(1.15f, 2f, .7f));
                AddBoxCollider(ringX.Prefab, new Vector3(0.27f, 0f, 0.27f), new Vector3(0, 45, 0), new Vector3(1.15f, 2f, .7f));
                AddBoxCollider(ringX.Prefab, new Vector3(1.73f, 0f, 1.73f), new Vector3(0, 45, 0), new Vector3(1.15f, 2f, .7f));

                moduleReactor.ThisCapacity = 1f;
                moduleReactor.DrainPerSecond = 0.1f;
                moduleReactor.JetCapsule1 = new Vector3(1f, 1.2f, 1f);
                moduleReactor.JetCapsule2 = new Vector3(1f, -0.2f, 1f);
                moduleReactor.JetCapsuleRad = 1f;
                moduleReactor.JetCapsuleDamage = 60f;
                Component.DestroyImmediate(ringX.Prefab.GetComponentInChildren<EnergyGauge>());
                var power = ringX.Prefab.GetComponent<ModuleEnergyStore>();
                power.m_Capacity = 1000;
                power.m_AcceptRemoteCharge = false;

                var daage = ringX.Prefab.GetComponent<ModuleDamage>();
                daage.deathExplosion = Transform.Instantiate(daage.deathExplosion);
                var damage = daage.deathExplosion.GetComponent<Explosion>();
                damage.m_MaxDamageStrength = 2500;
                damage.m_EffectRadius = 12;
                damage.m_EffectRadiusMaxStrength = 10;
                var particles = daage.deathExplosion.GetComponentsInChildren<ParticleSystem>();
                foreach (var particle in particles)
                {
                    var m = particle.main;
                    m.startSpeedMultiplier *= 3f;
                    m.startSizeMultiplier *= 4f;
                    m.startColor = rf_ExplosionColor;
                }

                var jet = new GameObject("BlastJet");
                jet.AddComponent<MeshFilter>().sharedMesh = GameObjectJSON.MeshFromData(Properties.Resources.ReactorRingXJet);
                jet.AddComponent<MeshRenderer>().sharedMaterial = rf_glow;
                jet.transform.parent = ringX.Prefab.transform;
                jet.transform.localPosition = new Vector3(1f, 0f, 1f);

                ringX.RegisterLater();
            }
            {
                var mesh = GameObjectJSON.MeshFromData(Properties.Resources.ReactorLoaderPassive);
                var loaderPrefab = new BlockPrefabBuilder("GSO_Generator_211")//"BF_PlasmaFurnace_333")
                    .SetBlockID(98352)
                    .SetName("Reactor Resource Loader").SetDescription("This is the rodite loader for any reactor setup on a tech.\n<b>There is only one passive AP on the bottom of this block.</b> Attach this to another passive AP to power ring generators with rodite ore or capsules.")
                    .SetMass(3f).SetHP(400).SetGrade(2)
                    .SetSize(new IntVector3(1, 2, 1))
                    .SetAPsManual(new Vector3[]
                    {
                        new Vector3(0,-0.5f,0),
                        new Vector3(0,0,-0.5f),
                        new Vector3(0,0,0.5f),
                        new Vector3(-0.5f,0,0),
                        new Vector3(0.5f,0,0),
                    })
                    .SetPrice(400)
                    .SetFaction(FactionSubTypes.HE).SetCategory(BlockCategories.Base)
                    .SetIcon(GameObjectJSON.SpriteFromImage(GameObjectJSON.ImageFromFile(Properties.Resources.reactor_loader)))
                    .SetModel(mesh, false, rf_main)
                    .AddComponent(out Reactors.ModuleReactorLoader moduleReactor)
                    .SetDamageableType(ManDamage.DamageableType.Rock)
                    .SetCenterOfMass(Vector3.zero);

                AddBoxCollider(loaderPrefab.Prefab, new Vector3(0f, -0.175f, 0f), Vector3.zero, new Vector3(1f, 0.65f, 1f));
                AddBoxCollider(loaderPrefab.Prefab, new Vector3(0.28f, 0.5f, 0.28f), Vector3.zero, new Vector3(.2f, 1.6f, .2f));
                AddBoxCollider(loaderPrefab.Prefab, new Vector3(0.28f, 0.5f, -0.28f), Vector3.zero, new Vector3(.2f, 1.6f, .2f));
                AddBoxCollider(loaderPrefab.Prefab, new Vector3(-0.28f, 0.5f, -0.28f), Vector3.zero, new Vector3(.2f, 1.6f, .2f));
                AddBoxCollider(loaderPrefab.Prefab, new Vector3(-0.28f, 0.5f, 0.28f), Vector3.zero, new Vector3(.2f, 1.6f, .2f));

                moduleReactor.ThisCapacity = 2.5f;
                moduleReactor.APIDs = new int[] { 0 };
                moduleReactor.UseAPIDs = true;
                ModuleItemConsume consume = loaderPrefab.Prefab.GetComponent<ModuleItemConsume>();
                Component.DestroyImmediate(loaderPrefab.Prefab.GetComponent<ModuleAnchor>());
                //loaderPrefab.Prefab.transform.Find("BF_PlasmaFurnace_333").localPosition = new Vector3(0, 0, 0);

                Type MIC = typeof(ModuleItemConsume);
                BindingFlags BF = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic;
                FieldInfo //NTBA = MIC.GetField("m_NeedsToBeAnchored", BF),
                    CS = MIC.GetField("m_Consume", BF),
                    IP = MIC.GetField("m_Input", BF),
                    EM = MIC.GetField("m_EnergyMultiplier", BF),
                    OBP = typeof(ModuleItemHolder).GetField("m_OverrideBasePositons", BF),
                    RLN = typeof(ModuleRecipeProvider).GetField("m_RecipeListNames", BF);
                try
                {
                    RLN.SetValue(loaderPrefab.Prefab.GetComponent<ModuleRecipeProvider>(), new RecipeManager.RecipeNameWrapper[] { new RecipeManager.RecipeNameWrapper() { name = Reactors.ModuleReactorLoader.RecipeName } });
                    //NTBA.SetValue(consume, false);
                    EM.SetValue(consume, 0f);
                    (IP.GetValue(consume) as ModuleItemHolder.StackHandle).localPos = new Vector3(0, 0.37f, 0);
                    (CS.GetValue(consume) as ModuleItemHolder.StackHandle).localPos = new Vector3(0, -0.5f, 0);
                    Vector3[] overrideBasePositions = OBP.GetValue(loaderPrefab.Prefab.GetComponent<ModuleItemHolder>()) as Vector3[];
                    overrideBasePositions[0] = new Vector3(0, 0.37f, 0);
                    overrideBasePositions[1] = new Vector3(0, -0.5f, 0);
                }
                catch (Exception E) { do { Console.WriteLine(E); E = E.InnerException; } while (E.InnerException != null); }
                loaderPrefab.RegisterLater();
                BlockLoader.DelayAfterSingleton(AddRoditeRecipeList);
            }
            {
                new BlockPrefabBuilder("BF_Block_111")
                    .SetBlockID(98344)
                    .SetName("Reusable Fusion Bolt").SetDescription("Press this up to another of its kind and it will merge the two techs!\n" +
                    "Both techs must be facing the same direction, and the block must be facing eachother. " +
                    "Once that's done, the halves join together to form one tech, and can be separated")
                    .SetMass(0.6f).SetHP(200).SetGrade(1)
                    .SetSize(IntVector3.one, BlockPrefabBuilder.AttachmentPoints.Bottom)
                    .SetPrice(624)
                    .SetFaction(FactionSubTypes.BF).SetCategory(BlockCategories.Accessories)
                    .SetIcon(GameObjectJSON.SpriteFromImage(GameObjectJSON.ImageFromFile(Properties.Resources.ionicboltopen)))
                    .SetModel(GameObjectJSON.MeshFromData(Properties.Resources.Ionic_Bolt_Open), true, bf_main)
                    .AddComponent(out ModuleFuseHalf moduleFuse)
                    .RegisterLater();
                moduleFuse.ModelForwardSignificance = false;
                moduleFuse.MakeSubstitiute = true;
                moduleFuse.SubstituteType = (BlockTypes)98345;
                moduleFuse.JoinOffset = Vector3.zero;

                CustomRecipe.RegisterRecipe(
                    new CustomRecipe.RecipeInput[] {
                        new CustomRecipe.RecipeInput(32,2), // Luxian Crystal
                        new CustomRecipe.RecipeInput(43,2), // Fibre Plating
                        new CustomRecipe.RecipeInput(2, 4) // Rubber Jelly
                    }, new CustomRecipe.RecipeOutput[] {
                        new CustomRecipe.RecipeOutput(98344)
                    }, NameOfFabricator: "bffab");
            }
            {
                new BlockPrefabBuilder("BF_Block_111")
                    .SetBlockID(98345)
                    .SetName("Reusable Fusion Bolt Pair").SetDescription("This is a joined pair of Resusable Fusion Bolts. They can be separated like a normal bolt")
                    .SetMass(1.2f).SetHP(400).SetGrade(1)
                    .SetSize(IntVector3.one)
                    .SetAPsManual(new Vector3[] {
                        Vector3.down * 0.5f,
                        Vector3.up * 0.5f
                        })
                    .SetPrice(1248)
                    .SetFaction(FactionSubTypes.BF).SetCategory(BlockCategories.Accessories)
                    .SetIcon(GameObjectJSON.SpriteFromImage(GameObjectJSON.ImageFromFile(Properties.Resources.ionicbolt)))
                    .SetModel(GameObjectJSON.MeshFromData(Properties.Resources.Ionic_Bolt), true, bf_main)
                    .AddComponent(out ModuleFuseHalf moduleFuse)
                    .AddComponent<ModuleDetachableLink>()
                    .RegisterLater();
                moduleFuse.Separator = true;
                moduleFuse.SubstituteType = (BlockTypes)98344;

                CustomRecipe.RegisterRecipe(
                    new CustomRecipe.RecipeInput[] {
                        new CustomRecipe.RecipeInput(32,4), // Luxian Crystal
                        new CustomRecipe.RecipeInput(43,4), // Fibre Plating
                        new CustomRecipe.RecipeInput(2, 8) // Rubber Jelly
                    }, new CustomRecipe.RecipeOutput[] {
                        new CustomRecipe.RecipeOutput(98345)
                    }, NameOfFabricator: "bffab");
            }
        }
    }
}