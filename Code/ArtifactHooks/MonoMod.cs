using System;
using System.Collections.Generic;
using System.Text;
using RoR2;
namespace ArtifactOfTheUnchained.ArtifactHooks;


internal static class MonoMod
{
    internal static void HealthComponent_TakeDamage_Artifact(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
    {
        if (!Main.IsArtifactEnabled)
        {
            orig(self, damageInfo);
            return;
        }

        Log.DebugDamageInfo(damageInfo, true);
        HookMethods.HealthComponent_TakeDamage(damageInfo);
        orig(self, damageInfo);
        Log.DebugDamageInfo(damageInfo, false);
        return;
    }


    internal static void HealthComponent_TakeDamage_Artifactless(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
    {
        Log.DebugDamageInfo(damageInfo, true);
        HookMethods.HealthComponent_TakeDamage(damageInfo);
        orig(self, damageInfo);
        Log.DebugDamageInfo(damageInfo, false);
        return;
    }
}
