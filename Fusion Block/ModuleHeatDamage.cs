using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FusionBlock
{
    /*
    class ModuleHeatDamage
    {
        //static class FlamethrowerPatch
        //{
        //    static void Prefix(Visible other)
        //    {
        //        var heat = other.gameObject.GetComponent<ModuleHEAT>();
        //        if (!heat)
        //        {
        //            heat = other.gameObject.AddComponent<ModuleHEAT>();
        //        }
        //        heat.ChangeEnergy += 100 * Time.deltaTime;
        //    }
        //}

        internal class ModuleHEAT : Module
        {
            static DamageMultiplierTable damageMultiplierTable = typeof(ManDamage).GetField("m_DamageMultiplierTable", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public).GetValue(ManDamage.inst) as DamageMultiplierTable;
            const float multiplierEffect = 0.6f; // Multiply damage multiplier by this amount (Needs effect or offset to make sure 0 is not 0)
            const float multiplierOffset = 0.4f; // Reposition the total damage multiplier (1 - multiplierEffect)
            const float ThermalDischarge = 50f; // How much energy to turn in to damage
            const float ThermalDrain = 10;
            const float BaseUnit = 100f;
            static Color color = new Color(1f, 0.7f, 0.5f);

            public float ThermalEnergy;
            public float ChangeEnergy;
            public float ThermalConductivity;
            public float ThermalCapacity;
            public float Density;
            public float Space;
            public bool Active = true;
            List<ModuleHEAT> blocks;

            MeshRenderer[] _renderers;

            void ChangeEmission(Color color)
            {
                foreach (MeshRenderer renderer in _renderers)
                    if (renderer != null) renderer.material.color = color;
            }

            public ModuleHEAT()
            {
                if (block == null)
                {
                    Console.WriteLine("Added ModuleHEAT to a block that has not been fully instanced!");
                    Component.DestroyImmediate(this);
                    return;
                }
                blocks = new List<ModuleHEAT>();
                float multiplier = damageMultiplierTable.GetDamageMultiplier(ManDamage.DamageType.Fire, block.visible.damageable.DamageableType) * multiplierEffect + multiplierOffset;
                _renderers = GetComponentsInChildren<MeshRenderer>(true);
                ThermalEnergy = 0f;
                ChangeEnergy = 0f;
                Space = block.filledCells.Length;
                Density = block.m_DefaultMass / Space;
                ThermalCapacity = Density * (1f / multiplier) * BaseUnit;
                ThermalConductivity = Density * multiplier;
            }

            public void Update()
            {
                ThermalEnergy += ChangeEnergy - ThermalDrain * Space;
                ChangeEnergy = 0;
                ChangeEmission(Color.Lerp(Color.white, color, ThermalEnergy / ThermalCapacity));

                if (ThermalEnergy > ThermalCapacity)
                {
                    float discharge = Mathf.Ceil((ThermalEnergy - ThermalCapacity) / ThermalDischarge) * ThermalDischarge;
                    ThermalEnergy -= discharge;
                    ManDamage.inst.DealDamage(block.visible.damageable, discharge, ManDamage.DamageType.Fire, null);
                }
            }

            public void FixedUpdate()
            {
                if (!block.IsAttached || ThermalEnergy <= 0)
                {
                    Active = false;
                    ChangeEmission(Color.white);
                    Component.Destroy(this);
                    return;
                }
                if (UnityEngine.Random.Range(0, 3) == 0 && ThermalEnergy > ThermalDischarge * ThermalConductivity)
                {
                    blocks.Clear();
                    foreach (TankBlock attached in block.ConnectedBlocksByAP)
                    {
                        if (attached == null) continue;
                        var HOT = attached.gameObject.GetComponent<ModuleHEAT>();
                        if (!HOT)
                        {
                            HOT = attached.gameObject.AddComponent<ModuleHEAT>();
                        }
                        else if (!HOT.Active) continue;
                        blocks.Add(HOT);
                    }
                    foreach (ModuleHEAT HOT in blocks)
                    {
                        if (ThermalEnergy / Density > HOT.ThermalEnergy / HOT.Density)
                        {
                            HOT.ChangeEnergy += ThermalDischarge * ThermalConductivity;
                            ChangeEnergy -= ThermalDischarge * ThermalConductivity;
                        }
                    }
                }
            }
        }
    }
    */
}
