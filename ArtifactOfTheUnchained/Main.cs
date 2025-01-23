using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine;
using BepInEx;
using BepInEx.Configuration;
using RoR2;
using R2API;
using R2API.Utils;
using R2API.ContentManagement;

namespace ArtifactOfTheUnchainedMod
{
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync)]
    [BepInDependency(LanguageAPI.PluginGUID, BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency(ProcTypeAPI.PluginGUID, BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency(R2APIContentManager.PluginGUID, BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency(RiskOfOptions.PluginInfo.PLUGIN_GUID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(SS2.SS2Main.GUID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class Main : BaseUnityPlugin
    {
        public static PluginInfo PluginInfo { get; private set; }
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "LordVGames";
        public const string PluginName = "ArtifactOfTheUnchained";
        public const string PluginVersion = "2.0.2";

        public List<ArtifactBase> Artifacts = [];
        public static string ArtifactDescription;

        public void Awake()
        {
            PluginInfo = Info;
            Log.Init(Logger);
            Assets.Init();

            DamageRelated.ProccedByItem = ProcTypeAPI.ReserveProcType();
            var ArtifactTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(ArtifactBase)));
            foreach (var artifactType in ArtifactTypes)
            {
                ArtifactBase artifact = (ArtifactBase)Activator.CreateInstance(artifactType);
                ConfigOptions.BindConfigEntries(Config, artifact);

                if (ConfigOptions.ServersideMode.Value)
                {
                    On.RoR2.CharacterMaster.OnBodyDamaged += CharacterMaster_OnBodyDamaged_Serverside;
                }
                else
                {
                    artifact.Init(Config);
                }
            }
        }

        private void CharacterMaster_OnBodyDamaged_Serverside(On.RoR2.CharacterMaster.orig_OnBodyDamaged orig, CharacterMaster self, DamageReport damageReport)
        {
            DamageRelated.CharacterMaster_OnBodyDamaged(damageReport);
            orig(self, damageReport);
            return;
        }
    }

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
            On.RoR2.CharacterMaster.OnBodyDamaged += CharacterMaster_OnBodyDamaged_Artifact;
        }

        private void CharacterMaster_OnBodyDamaged_Artifact(On.RoR2.CharacterMaster.orig_OnBodyDamaged orig, CharacterMaster self, DamageReport damageReport)
        {
            if (!ArtifactEnabled)
            {
                orig(self, damageReport);
                return;
            }
            DamageRelated.CharacterMaster_OnBodyDamaged(damageReport);
            orig(self, damageReport);
            return;
        }
    }
}