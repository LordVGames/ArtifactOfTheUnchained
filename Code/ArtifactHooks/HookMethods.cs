using R2API;
using RoR2;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static ArtifactOfTheUnchained.Main;
using static ArtifactOfTheUnchained.ConfigOptions;
namespace ArtifactOfTheUnchained.ArtifactHooks;


internal static class HookMethods
{
    internal static void HealthComponent_TakeDamage(DamageInfo damageInfo)
    {
        if (damageInfo.attacker == null || damageInfo.damageType.damageType.HasFlag(DamageType.DoT))
        {
            return;
        }
        if (damageInfo.damageType.IsDamageSourceSkillBased)
        {
            damageInfo.procChainMask.AddModdedProc(ModProcTypes.ProccedBySkill);
            return;
        }
        if (ProcNerfing.PreventAllItemChaining.Value)
        {
            Log.Debug(NerfLoggingMessages.ProcNotFromSkillBlocked);
            damageInfo.procCoefficient = 0;
            return;
        }


        /*
         * Important info:
         * We don't return when adding a proctype so that if multiple need to be applied they actually will
         * There's no return on item & equipment nerfs because you can't get both
         * And there IS a return on proc chain nerfs because it's posible to be marked for proc chain nerfs and other nerfs
         * 
         * Walkthrough of the process for how the code handles a proc chain:
         * ATG from skill:
         * * Proc chain mark is applied for any future procs in the chain
         * Polylute from that ATG:
         * * Proc chain nerf mark gets detected, nerfs get applied
         * 
         * Another walkthrough:
         * Use damaging equipment:
         * * It procs something, equipment proc mark applied to proc
         * Proc from an equipment hits:
         * * Nerfed based on equipment proc settings, proc chain mark is applied for any future procs in the chain
         * Proc from a proc from an equipment:
         * * Proc chain mark is detected before equipment proc mark, so the proc chain nerfs takes effect instead of the equipment proc nerfs.
         * 
         * ^^^^^^ This process is pretty much the same if it comes from an item
         */

        if (damageInfo.procChainMask.mask > 0)
        {
            int procsInChain = GetProcCountInChain(damageInfo.procChainMask);
            if (procsInChain > 0)
            {
                if (ProcNerfing.FromProc.ProcChainAmountLimit.Value == 0)
                {
                    Log.Debug(NerfLoggingMessages.ProcBlockedForZeroLimit);
                    damageInfo.procCoefficient = 0;
                    return;
                }
                if (damageInfo.procChainMask.HasModdedProc(ModProcTypes.ProccedByProc))
                {
                    NerfProcFromProc(damageInfo, procsInChain);
                    return;
                }
                else
                {
                    damageInfo.procChainMask.AddModdedProc(ModProcTypes.ProccedByProc);
                    // no return here in case a proc from an item/equipment lands so that the nerf from those can be applied THEN the next proc will be nerfed for chaining
                }


                if (ModSupport.Starstorm2.ModIsRunning && ModSupport.Starstorm2.IsBetaEnabled)
                {
                    int ss2EtherealsCount = ModSupport.Starstorm2.EtherealsCount;
                    if (ss2EtherealsCount > 0)
                    {
                        float etherealDamageMult = 1 - (ProcNerfing.FromProc.NerfProcDamagePerEthereal.Value * ss2EtherealsCount);
                        damageInfo.damage *= etherealDamageMult;
                    }
                }
            }
        }


        if (damageInfo.procChainMask.HasModdedProc(ModProcTypes.ProccedByEquipment))
        {
            NerfProcFromEquipment(damageInfo);
        }
        else if (damageInfo.damageType.damageSource == DamageSource.Equipment)
        {
            damageInfo.procChainMask.AddModdedProc(ModProcTypes.ProccedByEquipment);
        }
        else if (damageInfo.procChainMask.HasModdedProc(ModProcTypes.ProccedByItem))
        {
            if (!IsAttackerBlacklisted(damageInfo.attacker))
            {
                NerfProcFromItem(damageInfo);
            }
        }
        else if (!damageInfo.procChainMask.HasModdedProc(ModProcTypes.ProccedByProc))
        {
            damageInfo.procChainMask.AddModdedProc(ModProcTypes.ProccedByItem);
        }
    }

    private static void NerfProcFromProc(DamageInfo damageInfo, int procsInChain)
    {
        if (procsInChain > ProcNerfing.FromProc.ProcChainAmountLimit.Value && ProcNerfing.FromProc.ProcChainAmountLimit.Value != -1)
        {
            damageInfo.procCoefficient = 0;
            Log.Debug(NerfLoggingMessages.ProcChainBlocked);
        }
        else if (ProcNerfing.FromProc.ProcChainCoefficientNerfToPercent.Value != 1)
        {
            damageInfo.procCoefficient *= ProcNerfing.FromProc.ProcChainCoefficientNerfToPercent.Value;
            Log.Debug(NerfLoggingMessages.ProcChainCoefficientNerf);
        }

        if (ProcNerfing.FromProc.ProcChainDamageNerfToPercent.Value != 1)
        {
            damageInfo.damage *= ProcNerfing.FromProc.ProcChainDamageNerfToPercent.Value;
            Log.Debug(NerfLoggingMessages.ProcChainDamageNerf);
        }
    }

    private static void NerfProcFromItem(DamageInfo damageInfo)
    {
        if (ProcNerfing.FromItem.ProcFromItemCoefficientNerfToPercent.Value != 1)
        {
            damageInfo.procCoefficient *= ProcNerfing.FromItem.ProcFromItemCoefficientNerfToPercent.Value;
            Log.Debug(NerfLoggingMessages.ProcFromItemCoefficientNerf);
        }

        if (ProcNerfing.FromItem.ProcFromItemDamageNerfToPercent.Value != 1)
        {
            damageInfo.damage *= ProcNerfing.FromItem.ProcFromItemDamageNerfToPercent.Value;
            Log.Debug(NerfLoggingMessages.ProcFromItemDamageNerf);
        }
    }

    private static void NerfProcFromEquipment(DamageInfo damageInfo)
    {
        if (ProcNerfing.FromEquipment.ProcFromEquipmentCoefficientNerfToPercent.Value != 1)
        {
            damageInfo.procCoefficient *= ProcNerfing.FromEquipment.ProcFromEquipmentCoefficientNerfToPercent.Value;
            Log.Debug(NerfLoggingMessages.ProcFromEquipmentDamageNerf);
        }

        if (ProcNerfing.FromEquipment.ProcFromEquipmentDamageNerfToPercent.Value != 1)
        {
            damageInfo.damage *= ProcNerfing.FromEquipment.ProcFromEquipmentDamageNerfToPercent.Value;
            Log.Debug(NerfLoggingMessages.ProcFromEquipmentCoefficientNerf);
        }
    }



    internal static int GetProcCountInChain(ProcChainMask procChainMask)
    {
        int numberOfProcsInChain = 0;
        // i don't really know bitwise stuff yet so this probably isn't that good
        // but then again this first for loop is also kinda based on the game's procchainmask AppendToStringBuilder code so
        for (ProcType procType = 0; procType < ProcType.Count; procType += 1U)
        {
            if (procChainMask.HasProc(procType) && !ModProcTypes.FakeProcTypes.Contains(procType))
            {
                numberOfProcsInChain++;
            }
        }

        BitArray moddedMask = ProcTypeAPI.GetModdedMask(procChainMask);
        for (int i = 0; i < moddedMask.Count; i++)
        {
            if (moddedMask.Get(i))
            {
                numberOfProcsInChain++;
            }
        }
        numberOfProcsInChain = RemoveMarkProcsFromProcCount(procChainMask, numberOfProcsInChain);
        Log.Debug($"numberOfProcsInChain is {numberOfProcsInChain}");

        return numberOfProcsInChain;
    }
    private static int RemoveMarkProcsFromProcCount(ProcChainMask procChainMask, int numberOfProcsInChain)
    {
        if (procChainMask.HasModdedProc(ModProcTypes.ProccedByEquipment))
        {
            numberOfProcsInChain--;
        }
        if (procChainMask.HasModdedProc(ModProcTypes.ProccedByItem))
        {
            numberOfProcsInChain--;
        }
        if (procChainMask.HasModdedProc(ModProcTypes.ProccedByProc))
        {
            numberOfProcsInChain--;
        }
        return numberOfProcsInChain;
    }
}
