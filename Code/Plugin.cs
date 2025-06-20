using System;
using System.Reflection;
using System.Linq;
using BepInEx;
using R2API;
using R2API.ContentManagement;
using RoR2;

namespace ArtifactOfTheUnchainedMod
{
    [BepInDependency(LanguageAPI.PluginGUID, BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency(ProcTypeAPI.PluginGUID, BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency(R2APIContentManager.PluginGUID, BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency(DamageSourceForEnemies.Plugin.PluginGUID, BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency(DamageSourceForEquipment.Plugin.PluginGUID, BepInDependency.DependencyFlags.HardDependency)]

    [BepInDependency(RiskOfOptions.PluginInfo.PLUGIN_GUID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(SS2.SS2Main.GUID, BepInDependency.DependencyFlags.SoftDependency)]

    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public static PluginInfo PluginInfo { get; private set; }
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "LordVGames";
        public const string PluginName = "ArtifactOfTheUnchained";
        public const string PluginVersion = "2.5.1";

        public void Awake()
        {
            PluginInfo = Info;
            Log.Init(Logger);
            Assets.Init();
            DamageRelated.ProccedByItem = ProcTypeAPI.ReserveProcType();
            DamageRelated.ProccedByProc = ProcTypeAPI.ReserveProcType();
            DamageRelated.ProccedByEquipment = ProcTypeAPI.ReserveProcType();
            // no way there'll be someone that changes the language midgame right????
            RoR2Application.onLoad += Main.SetupLanguageSpecificStrings;
            var ArtifactTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(ArtifactBase)));
            foreach (var artifactType in ArtifactTypes)
            {
                ArtifactBase artifact = (ArtifactBase)Activator.CreateInstance(artifactType);
                ConfigOptions.BindConfigEntries(Config, artifact);

                if (ConfigOptions.ArtifactlessMode.Value)
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
}