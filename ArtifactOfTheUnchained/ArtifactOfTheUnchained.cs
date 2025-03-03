using System;
using UnityEngine;
using BepInEx.Configuration;
using RoR2;

namespace ArtifactOfTheUnchainedMod
{
    public class ArtifactOfTheUnchained : ArtifactBase
    {
        public override string ArtifactLangTokenName => "UNCHAINED";
        public override string ArtifactName => "Artifact of the Unchained";
        public override string ArtifactDescription => "Nerfs proc chains and procs caused by items such as fireworks, all depending on how you've configured the mod.";
        public override Sprite ArtifactEnabledIcon => Assets.AssetBundle.LoadAsset<Sprite>("NoProcChainsArtifactIcon_Enabled.png");
        public override Sprite ArtifactDisabledIcon => Assets.AssetBundle.LoadAsset<Sprite>("NoProcChainsArtifactIcon_Disabled.png");
        public override void Init(ConfigFile config)
        {
            CreateLang();
            CreateArtifact();
            Hooks();
            if (ModSupport.RiskOfOptions.ModIsRunning)
            {
                ModSupport.RiskOfOptions.AddOptions();
            }
        }
        public override void Hooks()
        {
            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage_Artifact;
        }

        private void HealthComponent_TakeDamage_Artifact(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            if (!ArtifactEnabled)
            {
                orig(self, damageInfo);
                return;
            }
            DamageRelated.DebugLogDamageInfo(damageInfo, true);
            DamageRelated.HealthComponent_TakeDamage(damageInfo);
            orig(self, damageInfo);
            DamageRelated.DebugLogDamageInfo(damageInfo, false);
            return;
        }
    }

}
