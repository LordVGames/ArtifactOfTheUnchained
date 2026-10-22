using MonoDetour;
using MonoDetour.HookGen;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
[assembly: MonoDetourTargets(typeof(HealthComponent))]
namespace ArtifactOfTheUnchained.ArtifactHooks;


internal static class MonoDetour
{
    internal static void SetupArtifact()
    {
        Mdh.RoR2.HealthComponent.TakeDamageProcess.Postfix(HealthComponent_TakeDamageProcess_Artifact);
    }


    private static void HealthComponent_TakeDamageProcess_Artifact(HealthComponent self, ref DamageInfo damageInfo)
    {
        if (!Main.IsArtifactEnabled)
        {
            return;
        }

        Log.DebugDamageInfo(damageInfo, true);
        HookMethods.HealthComponent_TakeDamage(damageInfo);
        Log.DebugDamageInfo(damageInfo, false);
    }




    internal static void SetupArtifactless()
    {
        Mdh.RoR2.HealthComponent.TakeDamageProcess.Postfix(HealthComponent_TakeDamageProcess_Artifactless);
    }

    private static void HealthComponent_TakeDamageProcess_Artifactless(HealthComponent self, ref DamageInfo damageInfo)
    {
        Log.DebugDamageInfo(damageInfo, true);
        HookMethods.HealthComponent_TakeDamage(damageInfo);
        Log.DebugDamageInfo(damageInfo, false);
    }
}