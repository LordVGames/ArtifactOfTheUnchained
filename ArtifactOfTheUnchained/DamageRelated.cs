using System;
using System.Collections;
using RoR2; 
using R2API;
using static ArtifactOfTheUnchainedMod.Main;

namespace ArtifactOfTheUnchainedMod
{
    internal static class DamageRelated
    {
        public static ModdedProcType ProccedByItem;
        public static ModdedProcType ProccedByProc;

        internal static void HealthComponent_TakeDamage(DamageInfo damageInfo)
        {
            if (!damageInfo.attacker)
            {
                return;
            }
            if (damageInfo.damageType.IsDamageSourceSkillBased)
            {
                return;
            }


            int procsInChain = GetProcCountInChain(damageInfo.procChainMask);
            if (procsInChain > 0)
            {
                if (damageInfo.procChainMask.HasModdedProc(ProccedByProc))
                {
                    NerfProcFromProc(damageInfo, procsInChain);
                    // don't want to be doing the proc chain nerf AND the item proc nerf
                    return;
                }
                else
                {
                    damageInfo.procChainMask.AddModdedProc(ProccedByProc);
                }
            }


            if (damageInfo.procChainMask.HasModdedProc(ProccedByItem))
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
            else
            {
                damageInfo.procCoefficient *= ConfigOptions.ProcChainCoefficientNerfToPercent.Value;
                Log.NerfedProc(NerfLoggingMessages.ProcChainCoefficientNerf);
            }

            damageInfo.damage *= ConfigOptions.ProcChainDamageNerfToPercent.Value;
            Log.NerfedProc(NerfLoggingMessages.ProcChainDamageNerf);
        }

        private static void NerfProcFromItem(DamageInfo damageInfo)
        {
            damageInfo.procCoefficient *= ConfigOptions.ProcFromItemCoefficientNerfToPercent.Value;
            Log.NerfedProc(NerfLoggingMessages.ProcFrontItemDamageNerf);

            damageInfo.damage *= ConfigOptions.ProcFromItemDamageNerfToPercent.Value;
            Log.NerfedProc(NerfLoggingMessages.ProcFrontItemCoefficientNerf);
        }



        internal static int GetProcCountInChain(ProcChainMask procChainMask)
        {
            int numberOfProcsInChain = 0;
            // i don't really know bitwise stuff yet so this probably isn't that good performance-wise
            // but then again this first for loop is also kinda based on the game's procchainmask AppendToStringBuilder code so
            for (ProcType procType = 0; procType < ProcType.Count; procType += 1U)
            {
                if (procChainMask.HasProc(procType))
                {
                    numberOfProcsInChain++;
                }
            }

            BitArray moddedMask = ProcTypeAPI.GetModdedMask(procChainMask);
            // i starts at 2 not 0 because 0 and 1 are always our "ProccedBy" proc types so they need to be skipped
            for (int i = 2; i < moddedMask.Count; i++)
            {
                if (moddedMask.Get(i))
                {
                    numberOfProcsInChain++;
                }
            }
            return numberOfProcsInChain;
        }
        internal static void DebugLogDamageInfo(DamageInfo damageInfo, bool loggingBeforeChanges)
        {
#if DEBUG
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