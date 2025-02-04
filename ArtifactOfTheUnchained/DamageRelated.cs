using RoR2; 
using R2API;

namespace ArtifactOfTheUnchainedMod
{
    internal static class DamageRelated
    {
        public static ModdedProcType ProccedByItem;

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

            bool isProcChainPastLimit = IsProcChainPastLimit(damageInfo.procChainMask);
            if (isProcChainPastLimit || ConfigOptions.PreventAllItemChaining.Value)
            {
                damageInfo.procCoefficient = 0;
            }
            if (damageInfo.procChainMask.HasModdedProc(ProccedByItem))
            {
                if (Main.AllowLoggingNerfs && ConfigOptions.ProcChainDamageNerfToPercent.Value != 1)
                {
                    Log.Info(Language.GetStringFormatted("ARTIFACT_UNCHAINED_PROC_DAMAGE_NERF"));
                }
                damageInfo.damage *= ConfigOptions.ProcChainDamageNerfToPercent.Value;
                if (!isProcChainPastLimit)
                {
                    if (Main.AllowLoggingNerfs && ConfigOptions.ProcChainCoefficientNerfToPercent.Value != 1)
                    {
                        Log.Info(Language.GetStringFormatted("ARTIFACT_UNCHAINED_PROC_COEFFICIENT_NERF"));
                    }
                    damageInfo.procCoefficient *= ConfigOptions.ProcChainCoefficientNerfToPercent.Value;
                }
                return;
            }
            damageInfo.procChainMask.AddModdedProc(ProccedByItem);
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

        internal static bool IsProcChainPastLimit(ProcChainMask procChainMask)
        {
            // -1 is for those who configured for vanilla behavior
            if (ConfigOptions.ProcChainAmountLimit.Value == -1)
            {
                return false;
            }
            Log.Debug($"IsProcChainPastLimit: ConfigOptions.ProcChainAmountLimit.Value is {ConfigOptions.ProcChainAmountLimit.Value}");
            return GetProcCountInChain(procChainMask) > ConfigOptions.ProcChainAmountLimit.Value;
        }

        internal static int GetProcCountInChain(ProcChainMask procChainMask)
        {
            int numberOfProcsInChain = 0;
            // i don't really know bitwise stuff yet so this probably isn't that good performance-wise
            // but then again this first for loop is also kinda based on the game's procchainmask AppendToStringBuilder code
            for (ProcType procType = 0; procType < ProcType.Count; procType += 1U)
            {
                if (procChainMask.HasProc(procType))
                {
                    numberOfProcsInChain++;
                }
            }
            var moddedMask = ProcTypeAPI.GetModdedMask(procChainMask);
            for (int i = 1; i < moddedMask.Count; i++)
            {
                // i starts at 1 not 0 because 0 is always our ProccedByItem proc type
                if (moddedMask.Get(i))
                {
                    numberOfProcsInChain++;
                }
            }
            Log.Debug($"numberOfProcsInChain is {numberOfProcsInChain}");
            return numberOfProcsInChain;
        }
    }
}