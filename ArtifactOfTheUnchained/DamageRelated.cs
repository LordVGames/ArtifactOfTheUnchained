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
            if (!damageInfo.attacker || damageInfo.damageType.damageType.HasFlag(DamageType.DoT))
            {
                return;
            }
            if (ModSupport.Starstorm2.ModIsRunning)
            {
                // relic of force procs on literally every hit including skills so this HAS to happen super early
                ModSupport.Starstorm2.HandleRelicOfForceDamageSource(damageInfo);
            }
            if (damageInfo.damageType.IsDamageSourceSkillBased)
            {
                return;
            }



            if (ConfigOptions.PreventAllItemChaining.Value)
            {
                Log.NerfedProc(NerfLoggingMessages.ProcNotFromSkillBlocked);
                damageInfo.procCoefficient = 0;
                return;
            }
            
            /*
             * Important info:
             * We don't return when adding a proctype so that if multiple need to be applied they will
             * There's no return on item & equipment nerfs because you can't get both
             * And there IS a return on proc chain nerfs because it's posible to be marked for proc chain nerfs and other nerfs
             * 
             * Also let's visualize some scenarios for future reference:
             * ATG from skill:
             * Gets marked for proc chain nerfs, that's it
             * Polylute from that ATG:
             * Proc chain nerf mark gets detected, nerfs applied, and that's it
             * Proc from an equipment that later procs something else:
             * Initial proc hits, equipment proc mark applied, equipment marked proc hits and equipment proc nerf is applied, then the proc chain mark is also applied. Chained proc happens, proc chain mark detected, nerfed for proc chaining and that's it
             * ^^^ This process is pretty much the same if it comes from an item
             */

            if (damageInfo.procChainMask.mask > 0)
            {
                int procsInChain = GetProcCountInChain(damageInfo.procChainMask);
                if (procsInChain > 0)
                {
                    if (damageInfo.procChainMask.HasModdedProc(ProccedByProc))
                    {
                        NerfProcFromProc(damageInfo, procsInChain);
                        return;
                    }
                    else
                    {
                        damageInfo.procChainMask.AddModdedProc(ProccedByProc);
                        return;
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
            else
            {
                damageInfo.procChainMask.AddModdedProc(ProccedByItem);
            }
        }

        private static void NerfProcFromProc(DamageInfo damageInfo, int procsInChain)
        {
            if (procsInChain > ConfigOptions.ProcChainAmountLimit.Value && ConfigOptions.ProcChainAmountLimit.Value != -1)
            {
                damageInfo.procCoefficient = 0;
                Log.NerfedProc(NerfLoggingMessages.ProcChainBlocked);
            }
            else if (ConfigOptions.ProcChainCoefficientNerfToPercent.Value != 1)
            {
                damageInfo.procCoefficient *= ConfigOptions.ProcChainCoefficientNerfToPercent.Value;
                Log.NerfedProc(NerfLoggingMessages.ProcChainCoefficientNerf);
            }

            if (ConfigOptions.ProcChainDamageNerfToPercent.Value != 1)
            {
                damageInfo.damage *= ConfigOptions.ProcChainDamageNerfToPercent.Value;
                Log.NerfedProc(NerfLoggingMessages.ProcChainDamageNerf);
            }
        }

        private static void NerfProcFromItem(DamageInfo damageInfo)
        {
            if (ConfigOptions.ProcFromItemCoefficientNerfToPercent.Value != 1)
            {
                damageInfo.procCoefficient *= ConfigOptions.ProcFromItemCoefficientNerfToPercent.Value;
                Log.NerfedProc(NerfLoggingMessages.ProcFromItemCoefficientNerf);
            }

            if (ConfigOptions.ProcFromItemDamageNerfToPercent.Value != 1)
            {
                damageInfo.damage *= ConfigOptions.ProcFromItemDamageNerfToPercent.Value;
                Log.NerfedProc(NerfLoggingMessages.ProcFromItemDamageNerf);
            }
        }

        private static void NerfProcFromEquipment(DamageInfo damageInfo)
        {
            if (ConfigOptions.ProcFromEquipmentCoefficientNerfToPercent.Value != 1)
            {
                damageInfo.procCoefficient *= ConfigOptions.ProcFromEquipmentCoefficientNerfToPercent.Value;
                Log.NerfedProc(NerfLoggingMessages.ProcFromEquipmentDamageNerf);
            }

            if (ConfigOptions.ProcFromEquipmentDamageNerfToPercent.Value != 1)
            {
                damageInfo.damage *= ConfigOptions.ProcFromEquipmentDamageNerfToPercent.Value;
                Log.NerfedProc(NerfLoggingMessages.ProcFromEquipmentCoefficientNerf);
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
#if DEBUG
            if (damageInfo.inflictor != null && damageInfo.inflictor.name == "DotController(Clone)")
            {
                return;
            }
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
            //Log.Debug($"damageType is {damageInfo.damageType}");
            Log.Debug($"inflictor is {damageInfo.inflictor}");
            Log.Debug($"procChainMask is {damageInfo.procChainMask}");
            Log.Debug($"procCoefficient is {damageInfo.procCoefficient}");
#endif
        }
    }
}