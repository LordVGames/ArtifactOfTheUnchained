using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine;
using BepInEx;
using BepInEx.Configuration;
using RoR2;
using R2API;
using R2API.ContentManagement;
[assembly: HG.Reflection.SearchableAttribute.OptIn]

namespace ArtifactOfTheUnchainedMod
{
    [BepInDependency(LanguageAPI.PluginGUID, BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency(ProcTypeAPI.PluginGUID, BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency(R2APIContentManager.PluginGUID, BepInDependency.DependencyFlags.HardDependency)]
    // depending on DamageSourceForEnemies so i don't have to handle monster damage in a very unreliable and weird way
    [BepInDependency(DamageSourceForEnemies.Plugin.PluginGUID, BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency(RiskOfOptions.PluginInfo.PLUGIN_GUID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public static PluginInfo PluginInfo { get; private set; }
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "LordVGames";
        public const string PluginName = "ArtifactOfTheUnchained";
        public const string PluginVersion = "2.2.0";

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

                if (ConfigOptions.ArtifactlessMdde.Value)
                {
                    On.RoR2.HealthComponent.TakeDamage += Main.HealthComponent_TakeDamage_Artifactless;
                }
                else
                {
                    artifact.Init(Config);
                }
            }
        }
    }

    public static class Main
    {
        public static List<ArtifactBase> Artifacts = [];
        internal static bool AllowLoggingNerfs = false;

        public static void HealthComponent_TakeDamage_Artifactless(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            DamageRelated.DebugLogDamageInfo(damageInfo, true);
            DamageRelated.HealthComponent_TakeDamage(damageInfo);
            orig(self, damageInfo);
            DamageRelated.DebugLogDamageInfo(damageInfo, false);
            return;
        }

        [ConCommand(commandName = "unchained_toggle_logging_nerfs", flags = ConVarFlags.None)]
        internal static void CCToggleLoggingNerfs(ConCommandArgs args)
        {
            AllowLoggingNerfs = !AllowLoggingNerfs;
            string chatMessage;
            if (AllowLoggingNerfs)
            {
                chatMessage = Language.GetString("ARTIFACT_UNCHAINED_SANITY_CHECK_ENABLED");
            }
            else
            {
                chatMessage = Language.GetString("ARTIFACT_UNCHAINED_SANITY_CHECK_DISABLED");
            }
            Chat.AddMessage(chatMessage);
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