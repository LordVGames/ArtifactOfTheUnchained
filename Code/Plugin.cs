using BepInEx;
using MonoDetour.HookGen;
using R2API;
using R2API.ContentManagement;
using RoR2;
using static ArtifactOfTheUnchained.ConfigOptions.ForModItself;
namespace ArtifactOfTheUnchained;


[BepInDependency(LanguageAPI.PluginGUID, BepInDependency.DependencyFlags.HardDependency)]
[BepInDependency(ProcTypeAPI.PluginGUID, BepInDependency.DependencyFlags.HardDependency)]
[BepInDependency(R2APIContentManager.PluginGUID, BepInDependency.DependencyFlags.HardDependency)]
[BepInDependency(DamageSourceForEnemies.Plugin.Id, BepInDependency.DependencyFlags.HardDependency)]
[BepInDependency(DamageSourceForEquipment.Plugin.Id, BepInDependency.DependencyFlags.HardDependency)]
[BepInDependency(SS2.SS2Main.GUID, BepInDependency.DependencyFlags.SoftDependency)]
[BepInAutoPlugin]
public partial class Plugin : BaseUnityPlugin
{
    public static PluginInfo PluginInfo { get; private set; }


    public void Awake()
    {
        PluginInfo = Info;
        Log.Init(Logger);
        ConfigOptions.BindAllConfigOptions(Config);
        Assets.Init();
        ModProcTypes.SetupProcTypes();
        // separate hooks for the setting to save a tiny bit of performance where i can
        if (ArtifactlessMode.Value)
        {
            if (HookTypeToUse.Value == HookType.MonoMod)
            {
                On.RoR2.HealthComponent.TakeDamage += ArtifactHooks.MonoMod.HealthComponent_TakeDamage_Artifactless;
            }
            else
            {
                ArtifactHooks.MonoDetour.SetupArtifactless();
            }
        }
        else
        {
            Artifact.SetupArtifact();
            if (HookTypeToUse.Value == HookType.MonoMod)
            {
                On.RoR2.HealthComponent.TakeDamage += ArtifactHooks.MonoMod.HealthComponent_TakeDamage_Artifact;
            }
            else
            {
                ArtifactHooks.MonoDetour.SetupArtifact();
            }
        }
        if (ModSupport.Starstorm2.ModIsRunning)
        {
            ModSupport.Starstorm2.RelicOfForceDamageSourceFix.Setup();
        }
        // no way there'll be someone that changes the language midgame right????
        RoR2Application.onLoad += Main.SetupLanguageSpecificStrings;
    }
}