using ArtifactOfTheUnchained.ArtifactHooks;
using BepInEx.Logging;
using RoR2;
namespace ArtifactOfTheUnchained;


internal static class Log
{
    private static ManualLogSource _logSource;

    internal static void Init(ManualLogSource logSource)
    {
        _logSource = logSource;
    }

    internal static void Debug(object data)
    {
        if (ConfigOptions.ForModItself.EnableDebugLogging.Value)
        {
            _logSource.LogDebug(data);
        }
    }
    internal static void Info(object data) => _logSource.LogInfo(data);
    internal static void Message(object data) => _logSource.LogMessage(data);
    internal static void Warning(object data) => _logSource.LogWarning(data);
    internal static void Error(object data) => _logSource.LogError(data);
    internal static void Fatal(object data) => _logSource.LogFatal(data);


    internal static void DebugDamageInfo(DamageInfo damageInfo, bool loggingBeforeChanges)
    {
        if (!ConfigOptions.ForModItself.EnableDebugLogging.Value)
        {
            return;
        }
        if (damageInfo.attacker == null)
        {
            return;
        }
        if (damageInfo.inflictor != null && damageInfo.inflictor.name == "DotController(Clone)")
        {
            return;
        }


        Debug("\n\n\n");
        if (loggingBeforeChanges)
        {
            Warning("BEFORE");
            Warning("BEFORE");
            Warning("BEFORE");
        }
        else
        {
            Warning("AFTER");
            Warning("AFTER");
            Warning("AFTER");
        }


        Debug($"damageSource is {damageInfo.damageType.damageSource}");
        Debug($"IsDamageSourceSkillBased is {damageInfo.damageType.IsDamageSourceSkillBased}");
        Debug($"attacker is {damageInfo.attacker}");
        Debug($"crit is {damageInfo.crit}");
        Debug($"damage is {damageInfo.damage}");
        if (ModSupport.Starstorm2.ModIsRunning && ModSupport.Starstorm2.IsBetaEnabled)
        {
            Debug($"ethereal damage mult is {1 - (ConfigOptions.ProcNerfing.FromProc.NerfProcDamagePerEthereal.Value * ModSupport.Starstorm2.EtherealsCount)}");
        }
        Debug($"damageType is {damageInfo.damageType}");
        Debug($"inflictor is {damageInfo.inflictor}");
        Debug($"procChainMask is {damageInfo.procChainMask}");
        Debug($"real proc chain count in mask is {HookMethods.GetProcCountInChain(damageInfo.procChainMask)}");
        Debug($"procCoefficient is {damageInfo.procCoefficient}");
    }
}