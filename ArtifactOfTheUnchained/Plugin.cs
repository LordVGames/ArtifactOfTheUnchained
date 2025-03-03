using System;
using System.Reflection;
using System.Linq;
using BepInEx;
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
        public const string PluginVersion = "2.3.1";

        public void Awake()
        {
            PluginInfo = Info;
            Log.Init(Logger);
            Assets.Init();
            // no way there'll be someone that changes the language midgzame right????
            Main.SetupLanguageSpecificStrings();
            DamageRelated.ProccedByItem = ProcTypeAPI.ReserveProcType();
            DamageRelated.ProccedByProc = ProcTypeAPI.ReserveProcType();

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