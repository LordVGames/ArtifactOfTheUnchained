using System;
using System.Collections;
using System.Linq;
using RoR2; 
using R2API;
using static ArtifactOfTheUnchainedMod.Main;

namespace ArtifactOfTheUnchainedMod
{
    internal static class DamageRelated
    {
        public static ModdedProcType ProccedByProc;
        public static ModdedProcType ProccedByItem;
        public static ModdedProcType ProccedByEquipment;
        // some items have a proc type added to the chain even though they aren't actual proc items (i.e nkuhanas, razorwire, bands, etc)
        private static readonly ProcType[] _fakeProcTypes = [ProcType.HealNova, ProcType.Thorns, ProcType.Rings];

        internal static void HealthComponent_TakeDamage(DamageInfo damageInfo)
        {
            if (damageInfo.attacker == null || damageInfo.damageType.damageType.HasFlag(DamageType.DoT))
            {
                return;
            }
            if (ModSupport.Starstorm2.ModIsRunning)
            {
                // relic of force procs on literally every hit including skills so this before then
                ModSupport.Starstorm2.HandleRelicOfForceDamageSource(damageInfo);


                if (ModSupport.Starstorm2.IsBetaVersion)
                {
                    // grabbing EtherealsCount from mod support only once to help a tiny bit on performance
                    int ss2EtherealsCount = ModSupport.Starstorm2.EtherealsCount;
                    if (ss2EtherealsCount > 0)
                    {
                        if (!damageInfo.damageType.IsDamageSourceSkillBased && damageInfo.damageType.damageSource != DamageSource.Equipment)
                        {
                            // i would filter for only player damage but that requires a GetComponent call
                            // and there's NO WAY i'm doing GetComponent in this part since this will run super often if it's true
                            // which means monster procs are gonna get nerfed too
                            // but monsters they get +100 levels for every ethereal level anyways so it's not really that much of a nerf in terms of damage for them
                            float etherealDamageMult = 1 - (ConfigOptions.NerfProcDamagePerEthereal.Value * ss2EtherealsCount);
                            damageInfo.damage *= etherealDamageMult;
                        }
                    }
                }
            }
            if (damageInfo.damageType.IsDamageSourceSkillBased)
            {
                return;
            }
            if (ConfigOptions.PreventAllItemChaining.Value)
            {
                Log.Debug(NerfLoggingMessages.ProcNotFromSkillBlocked);
                damageInfo.procCoefficient = 0;
                return;
            }


            /*
             * Important info:
             * We don't return when adding a proctype so that if multiple need to be applied they will
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
                    if (ConfigOptions.ProcChainAmountLimit.Value == 0)
                    {
                        Log.Debug(NerfLoggingMessages.ProcBlockedForZeroLimit);
                        damageInfo.procCoefficient = 0;
                        return;
                    }
                    if (damageInfo.procChainMask.HasModdedProc(ProccedByProc))
                    {
                        NerfProcFromProc(damageInfo, procsInChain);
                        return;
                    }
                    else
                    {
                        damageInfo.procChainMask.AddModdedProc(ProccedByProc);
                        // no return here in case a proc from an item/equipment lands so that the nerf from those can be applied THEN the next proc will be nerfed for chaining
                    }
                }
            }


            if (damageInfo.procChainMask.HasModdedProc(ProccedByEquipment))
            {
                NerfProcFromEquipment(damageInfo);
            }
            else if (damageInfo.damageType.damageSource == DamageSource.Equipment)
            {
                damageInfo.procChainMask.AddModdedProc(ProccedByEquipment);
            }
            else if (damageInfo.procChainMask.HasModdedProc(ProccedByItem))
            {
                if (!IsAttackerBlacklisted(damageInfo.attacker))
                {
                    NerfProcFromItem(damageInfo);
                }
            }
            else if (!damageInfo.procChainMask.HasModdedProc(ProccedByProc))
            {
                damageInfo.procChainMask.AddModdedProc(ProccedByItem);
            }
        }

        private static void NerfProcFromProc(DamageInfo damageInfo, int procsInChain)
        {
            if (procsInChain > ConfigOptions.ProcChainAmountLimit.Value && ConfigOptions.ProcChainAmountLimit.Value != -1)
            {
                damageInfo.procCoefficient = 0;
                Log.Debug(NerfLoggingMessages.ProcChainBlocked);
            }
            else if (ConfigOptions.ProcChainCoefficientNerfToPercent.Value != 1)
            {
                damageInfo.procCoefficient *= ConfigOptions.ProcChainCoefficientNerfToPercent.Value;
                Log.Debug(NerfLoggingMessages.ProcChainCoefficientNerf);
            }

            if (ConfigOptions.ProcChainDamageNerfToPercent.Value != 1)
            {
                damageInfo.damage *= ConfigOptions.ProcChainDamageNerfToPercent.Value;
                Log.Debug(NerfLoggingMessages.ProcChainDamageNerf);
            }
        }

        private static void NerfProcFromItem(DamageInfo damageInfo)
        {
            if (ConfigOptions.ProcFromItemCoefficientNerfToPercent.Value != 1)
            {
                damageInfo.procCoefficient *= ConfigOptions.ProcFromItemCoefficientNerfToPercent.Value;
                Log.Debug(NerfLoggingMessages.ProcFromItemCoefficientNerf);
            }

            if (ConfigOptions.ProcFromItemDamageNerfToPercent.Value != 1)
            {
                damageInfo.damage *= ConfigOptions.ProcFromItemDamageNerfToPercent.Value;
                Log.Debug(NerfLoggingMessages.ProcFromItemDamageNerf);
            }
        }

        private static void NerfProcFromEquipment(DamageInfo damageInfo)
        {
            if (ConfigOptions.ProcFromEquipmentCoefficientNerfToPercent.Value != 1)
            {
                damageInfo.procCoefficient *= ConfigOptions.ProcFromEquipmentCoefficientNerfToPercent.Value;
                Log.Debug(NerfLoggingMessages.ProcFromEquipmentDamageNerf);
            }

            if (ConfigOptions.ProcFromEquipmentDamageNerfToPercent.Value != 1)
            {
                damageInfo.damage *= ConfigOptions.ProcFromEquipmentDamageNerfToPercent.Value;
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
                if (procChainMask.HasProc(procType) && !_fakeProcTypes.Contains(procType))
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
            if (procChainMask.HasModdedProc(ProccedByEquipment))
            {
                numberOfProcsInChain--;
            }
            if (procChainMask.HasModdedProc(ProccedByItem))
            {
                numberOfProcsInChain--;
            }
            if (procChainMask.HasModdedProc(ProccedByProc))
            {
                numberOfProcsInChain--;
            }
            return numberOfProcsInChain;
        }
        internal static void DebugLogDamageInfo(DamageInfo damageInfo, bool loggingBeforeChanges)
        {
            if (!ConfigOptions.EnableDebugLogging.Value)
            {
                return;
            }
            if (damageInfo.inflictor != null && damageInfo.inflictor.name == "DotController(Clone)")
            {
                return;
            }


            Log.Debug("\n\n\n");
            if (loggingBeforeChanges)
            {
                Log.Warning("BEFORE");
                Log.Warning("BEFORE");
                Log.Warning("BEFORE");
            }
            else
            {
                Log.Warning("AFTER");
                Log.Warning("AFTER");
                Log.Warning("AFTER");
            }


            Log.Debug($"damageSource is {damageInfo.damageType.damageSource}");
            Log.Debug($"IsDamageSourceSkillBased is {damageInfo.damageType.IsDamageSourceSkillBased}");
            Log.Debug($"attacker is {damageInfo.attacker}");
            Log.Debug($"crit is {damageInfo.crit}");
            Log.Debug($"damage is {damageInfo.damage}");
            if (ModSupport.Starstorm2.ModIsRunning && ModSupport.Starstorm2.IsBetaVersion)
            {
                Log.Debug($"ethereal damage mult is {1 - (ConfigOptions.NerfProcDamagePerEthereal.Value * ModSupport.Starstorm2.EtherealsCount)}");
            }
            Log.Debug($"damageType is {damageInfo.damageType}");
            Log.Debug($"inflictor is {damageInfo.inflictor}");
            Log.Debug($"procChainMask is {damageInfo.procChainMask}");
            Log.Debug($"real proc chain count in mask is {GetProcCountInChain(damageInfo.procChainMask)}");
            Log.Debug($"procCoefficient is {damageInfo.procCoefficient}");
        }
    }
}