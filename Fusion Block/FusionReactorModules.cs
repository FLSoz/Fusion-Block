using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FusionBlock
{
    public class Reactors
    {
        public class ModuleReactorHolder : Module
        {
            public bool UseAPIDs = false;
            public int[] APIDs;
            public float ThisCapacity = 3f;
            public float ThisCharge = 0f;
            internal float ChargeLeftToAdd = 0f;

            MeshRenderer[] renderers;
            float[] emissionClip;

            void OnPool()
            {
                block.AttachEvent.Subscribe(OnAttach);
                block.DetachEvent.Subscribe(OnDetach);
                block.serializeEvent.Subscribe(new Action<bool, TankPreset.BlockSpec>(OnSerialize));
                block.serializeTextEvent.Subscribe(new Action<bool, TankPreset.BlockSpec>(OnSerialize));

                renderers = GetComponentsInChildren<MeshRenderer>();
                emissionClip = new float[renderers.Length];
                for (int i = 0; i < renderers.Length; i++)
                {
                    if (renderers[i].gameObject.name[0] == '0' && float.TryParse(renderers[i].gameObject.name, out float offset))
                        emissionClip[i] = offset / 100f;
                    else
                        emissionClip[i] = 0f;
                }
            }

            void OnSerialize(bool saving, TankPreset.BlockSpec blockSpec)
            {
                if (saving)
                {
                    new ModuleReactorHolder.SerialData
                    {
                        charge = EnergyNetActive ? NetCharge / NetMaxCharge * ThisCapacity : ThisCharge,
                        remain = ChargeLeftToAdd
                    }.Store(blockSpec.saveState);
                }
                else
                {
                    ModuleReactorHolder.SerialData sd = SerialData<ModuleReactorHolder.SerialData>.Retrieve(blockSpec.saveState);
                    if (sd != null)
                    {
                        ThisCharge = sd.charge;
                        ChargeLeftToAdd = sd.remain;
                    }
                }
            }


            [Serializable]
            public class SerialData : SerialData<ModuleReactorHolder.SerialData>
            {
                public float charge;
                public float remain;
            }

            void OnSpawn()
            {
                Emission = 1f;
                sharedEnergyNet = null;
                block.SwapMaterialTime(true);
            }

            void OnRecycle()
            {
                ThisCharge = 0f;
            }

            internal virtual float EmissionCalc => Emission * 0.95f + NetCharge / NetMaxCharge * 0.05f;

            internal float Emission = 1f;
            void LateUpdate()
            {
                if (EnergyNetActive)
                {
                    Emission = EmissionCalc;
                }
                else
                {
                    Emission *= 0.95f + ThisCharge / ThisCapacity * 0.05f;
                }
                if (Emission > 0.05)
                {
                    ChangeEmission(Color.Lerp(Color.black, new Color(UnityEngine.Random.Range(0.7f, 1f), UnityEngine.Random.Range(0.7f, 1f), UnityEngine.Random.Range(0.7f, 1f)), Emission));
                }
                else
                {
                    ChangeEmission(Color.black);
                }
            }

            void OnAttach()
            {
                SharedEnergyNet.Validate(this);
            }

            void OnDetach()
            {
                sharedEnergyNet.Leave(this);
            }



            void ChangeEmission(Color color)
            {
                for (int i = 0; i < renderers.Length; i++)
                {
                    var renderer = renderers[i];
                    Color _color = color;

                    float offset = emissionClip[i];
                    float bofst = 1f - offset;
                    if (offset != 0f)
                    _color = new Color(Mathf.Clamp01((color.r - offset) / bofst), Mathf.Clamp01((color.g - offset) / bofst), Mathf.Clamp01((color.b - offset) / bofst));

                    if (renderer.gameObject.name == "BlastJet")
                    {
                        if (EnergyNetActive)
                            renderer.material.SetColor("_TintColor", _color);
                        else
                            renderer.material.SetColor("_TintColor", Color.black);
                    }
                    else renderer.material.SetColor("_EmissionColor", _color);
                }
            }

            public float NetCharge { get => sharedEnergyNet.NetCharge; set => sharedEnergyNet.NetCharge = value; }
            public float NetMaxCharge { get => sharedEnergyNet.NetMaxCap; }
            public int NetBlockCount => sharedEnergyNet.holders.Count;
            public bool EnergyNetActive => block.tank != null && sharedEnergyNet != null;

            SharedEnergyNet sharedEnergyNet;

            class SharedEnergyNet
            {
                public List<ModuleReactorHolder> holders = new List<ModuleReactorHolder>();
                public float NetCharge = 0f;
                public float NetMaxCap = 0f;

                static void AttemptJoin(ModuleReactorHolder subject, TankBlock other)
                {
                    if (other == null) return;
                    var moduleHolder = other.GetComponent<ModuleReactorHolder>();
                    if (moduleHolder == null || moduleHolder.sharedEnergyNet == null) return;
                    if (!moduleHolder.UseAPIDs)
                    {
                        moduleHolder.sharedEnergyNet.Join(subject);
                        return;
                    }
                    else
                    {
                        for (int i = 0; i < moduleHolder.APIDs.Length; i++)
                        {
                            var check = moduleHolder.block.ConnectedBlocksByAP[moduleHolder.APIDs[i]];
                            if (check == subject.block)
                            {
                                moduleHolder.sharedEnergyNet.Join(subject);
                                return;
                            }
                        }
                    }
                }

                public static SharedEnergyNet Validate(ModuleReactorHolder holder)
                {
                    if (holder.UseAPIDs)
                    {
                        for (int i = 0; i < holder.APIDs.Length; i++)
                        {
                            var other = holder.block.ConnectedBlocksByAP[holder.APIDs[i]];
                            AttemptJoin(holder, other);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < holder.block.attachPoints.Length; i++)
                        {
                            var other = holder.block.ConnectedBlocksByAP[i];
                            AttemptJoin(holder, other);
                        }
                    }
                    if (holder.sharedEnergyNet == null)
                    {
                        new SharedEnergyNet().Join(holder);
                    }
                    return holder.sharedEnergyNet;
                }

                public void Leave(ModuleReactorHolder holder)
                {
                    holder.sharedEnergyNet = null;
                    if (holders.Remove(holder))
                    {
                        NetMaxCap -= holder.ThisCapacity;
                        if (NetCharge > NetMaxCap)
                        {
                            holder.ThisCharge = NetCharge - NetMaxCap;
                            NetCharge = NetMaxCap;
                        }
                        Revalidate();
                    }
                }

                void Revalidate()
                {
                    if (holders.Count < 2) return;
                    var First = holders[0];
                    for (int i = holders.Count - 1; i >= 0; i--)
                    {
                        var holder = holders[i];
                        holder.sharedEnergyNet = null;
                        holder.ThisCharge = NetCharge / NetMaxCap * holder.ThisCapacity;
                    }
                    NetCharge = 0;
                    NetMaxCap = 0;
                    Join(First);
                    var old_holders = holders;
                    holders = new List<ModuleReactorHolder>() { First };
                    for (int i = 1; i < old_holders.Count; i++)
                    {
                        Validate(old_holders[i]);
                    }
                }

                public void Join(ModuleReactorHolder holder)
                {
                    if (holder.sharedEnergyNet != null) //MERGE
                    {
                        if (holder.sharedEnergyNet == this) return;
                        var otherNet = holder.sharedEnergyNet;
                        foreach (var block in otherNet.holders)
                        {
                            holders.Add(block);
                            block.sharedEnergyNet = this;
                        }
                        NetMaxCap += otherNet.NetMaxCap;
                        NetCharge += otherNet.NetCharge;
                        otherNet.holders.Clear(); // Another one bites the dust
                    }
                    else
                    {
                        holders.Add(holder);
                        NetMaxCap += holder.ThisCapacity;
                        NetCharge += holder.ThisCharge;
                        holder.ThisCharge = 0f;
                        holder.sharedEnergyNet = this;
                    }
                }
            }
        }

        public class ModuleReactorLoader : ModuleReactorHolder
        {
            internal const string RecipeName = "roditefusionreactorloader";
            internal static RecipeTable.RecipeList RecipeList = new RecipeTable.RecipeList()
            {
                m_Recipes = new List<RecipeTable.Recipe>() {
                    new RecipeTable.Recipe() { m_InputItems = new RecipeTable.Recipe.ItemSpec[] {
                        new RecipeTable.Recipe.ItemSpec(new ItemTypeInfo(ObjectTypes.Chunk, (int)ChunkTypes.RoditeOre), 1) },
                        m_OutputItems = new RecipeTable.Recipe.ItemSpec[0],
                        m_OutputType = RecipeTable.Recipe.OutputType.Energy, m_EnergyOutput = -1, m_BuildTimeSeconds = 20 },
                    new RecipeTable.Recipe() { m_InputItems = new RecipeTable.Recipe.ItemSpec[] {
                        new RecipeTable.Recipe.ItemSpec(new ItemTypeInfo(ObjectTypes.Chunk, (int)ChunkTypes.RodiusCapsule), 1) },
                        m_OutputItems = new RecipeTable.Recipe.ItemSpec[0],
                        m_OutputType = RecipeTable.Recipe.OutputType.Energy, m_EnergyOutput = -1, m_BuildTimeSeconds = 14 } },
                m_Name = RecipeName
            };

            ModuleItemHolder ItemHolder;
            ModuleItemConsume ItemConsumer;
            ModuleItemHolder.Stack Holder, Consumer;
            Visible Generating = null;

            internal override float EmissionCalc => Emission * 0.9f + (ItemConsumer.IsOperating ? 0.035f : 0f) + ChargeLeftToAdd * 0.065f;

            void OnPool()
            {
                ItemHolder = GetComponent<ModuleItemHolder>();
                ItemConsumer = GetComponent<ModuleItemConsume>();
                Holder = ItemHolder.GetStack(0);
                Consumer = ItemHolder.GetStack(1);
            }

            void OnSpawn()
            {
                ChargeLeftToAdd = 0f;
            }

            void Update()
            {
                if (EnergyNetActive)
                {
                    if (Input.GetKeyDown(KeyCode.O)) Console.WriteLine($"SharedEnergyNet (Loader {block.cachedLocalPosition}): {NetBlockCount} blocks, {NetCharge} / {NetMaxCharge} rods");
                    if (ChargeLeftToAdd > 0.01f)
                    {
                        float charge = Mathf.Min(ChargeLeftToAdd, NetMaxCharge - NetCharge);
                        ChargeLeftToAdd -= charge;
                        NetCharge += charge;
                    }
                    if (!Consumer.IsEmpty)
                    {
                        if (!Generating || Generating != Consumer.FirstItem)
                        {
                            Generating = Consumer.FirstItem;
                            ChargeLeftToAdd += 1f;
                        }
                    }
                    else if (Generating) Generating = null;
                    if (NetCharge + 1 > NetMaxCharge || ChargeLeftToAdd > 0.1f)
                    {
                        ItemHolder.OverrideStackCapacity(0);
                    }
                    else
                    {
                        ItemHolder.OverrideStackCapacity(1);
                    }
                }
            }
        }
        public class ModuleReactorRing : ModuleReactorHolder
        {
            ModuleEnergyStore power;
            public float DrainPerSecond = 0.04f;
            public const float PowerPerUnit = 12500;
            float m_g = 0f;

            internal override float EmissionCalc => Emission * 0.92f + m_g;

            void OnPool()
            {
                JetCapsuleColliders = new Collider[24];
                power = GetComponent<ModuleEnergyStore>();
                Jet = transform.Find("BlastJet");
            }

            void Update()
            {
                m_g = 0f;
                if (EnergyNetActive)
                {
                    float drain = Mathf.Clamp(NetCharge, 0f, DrainPerSecond * Time.deltaTime);
                    float charge = drain * PowerPerUnit;
                    if (charge > 0 && power.SpareCapacity > charge)
                    {
                        m_g = .08f;
                        power.AddEnergy(charge);
                        NetCharge -= drain;
                    }
                }
                if (Jet)
                {
                    Jet.Rotate(0f, JetSpin * Time.deltaTime, 0f, Space.Self);
                }
            }

            public Vector3 JetCapsule1 = Vector3.zero, JetCapsule2 = Vector3.zero;
            public float JetCapsuleRad = 1;
            public float JetCapsuleDamage = 25;
            Collider[] JetCapsuleColliders;
            static int JetDamageMask = Globals.inst.layerTank.mask | Globals.inst.layerScenery.mask;
            Transform Jet;
            const float JetSpin = 120f;

            void FixedUpdate()
            {
                if (m_g == 0f) return;
                int Collided = Physics.OverlapCapsuleNonAlloc(transform.TransformPoint(JetCapsule1), transform.TransformPoint(JetCapsule2), JetCapsuleRad, JetCapsuleColliders, JetDamageMask);
                for (int i = 0; i < Collided; i++)
                {
                    var visible = Visible.FindVisibleUpwards(JetCapsuleColliders[i]);
                    if (visible)
                    {
                        var damageable = visible.damageable;
                        if (damageable.DamageableType != ManDamage.DamageableType.Rock)
                        {
                            ManDamage.inst.DealDamage(damageable, JetCapsuleDamage * Time.deltaTime, ManDamage.DamageType.Fire, null);
                        }
                    }
                }
            }
        }
        public class ReactorHeat : Module
        {

        }
        public class ModuleReactorCooler : Module
        {

        }
    }
}
