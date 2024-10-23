using UnityEngine;
using BepInEx;
using RoR2;
using R2API;
using R2API.ContentManagement;
using ExamplePlugin;
using BepInEx.Configuration;
using System.Collections.Generic;
using System.Reflection;
using System;
using System.Linq;

namespace NoProcChainsArtifact
{
    [BepInDependency(LanguageAPI.PluginGUID, BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency(R2APIContentManager.PluginGUID, BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency(RiskOfOptions.PluginInfo.PLUGIN_GUID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class NoProcChainsArtifact : BaseUnityPlugin
    {
        public static BepInEx.PluginInfo PluginInfo { get; private set; }
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "LordVGames";
        public const string PluginName = "NoProcChainsArtifact";
        public const string PluginVersion = "1.0.0";
        public List<ArtifactBase> Artifacts = new List<ArtifactBase>();

        public void Awake()
        {
            PluginInfo = Info;
            Log.Init(Logger);
            ModAssets.Init();

            var ArtifactTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(ArtifactBase)));
            foreach (var artifactType in ArtifactTypes)
            {
                ArtifactBase artifact = (ArtifactBase)Activator.CreateInstance(artifactType);
                artifact.Init(Config);
            }
        }
    }

    public class ArtifactOfTheUnchained : ArtifactBase
    {
        public static ConfigEntry<bool> AllowEquipmentProcs;
        public static ConfigEntry<bool> AllowShurikenProcs;
        public static ConfigEntry<bool> AllowEgoProcs;
        public static ConfigEntry<bool> AllowGloopProcs;
        public static ConfigEntry<bool> AllowAspectPassiveProcs;
        public static ConfigEntry<bool> AllowProcCrits;
        public override string ArtifactLangTokenName => "NO_PROC_CHAINS";
        public override string ArtifactName => "Artifact of the Unchained";
        public override string ArtifactDescription => "Almost all item effects cannot proc your on-hit item effects for you.";
        public override Sprite ArtifactEnabledIcon => ModAssets.AssetBundle.LoadAsset<Sprite>("NoProcChainsArtifactIcon_Enabled.png");
        public override Sprite ArtifactDisabledIcon => ModAssets.AssetBundle.LoadAsset<Sprite>("NoProcChainsArtifactIcon_Disabled.png");
        //public static GameObject missileProjectileGameObject;
        //public static GameObject sawmerangProjectileGameObject;
        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateArtifact();
            Hooks();
            if (RiskOfOptionsSupport.ModIsRunning)
            {
                RiskOfOptionsSupport.AddOptions();
            }

            /*missileProjectileGameObject = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/MissileProjectile.prefab").WaitForCompletion();
            sawmerangProjectileGameObject = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/Sawmerang.prefab").WaitForCompletion();*/
        }
        private void CreateConfig(ConfigFile config)
        {
            AllowEquipmentProcs = config.Bind<bool>(ArtifactName, "Allow equipments to proc items", false, "Should damage from equipments be allowed to proc your on-hit items?");
            AllowShurikenProcs = config.Bind<bool>(ArtifactName, "Allow Shurikens to proc items", false, "Should damage from Shurikens be allowed to proc your on-hit items?");
            AllowEgoProcs = config.Bind<bool>(ArtifactName, "Allow Egocentrism to proc items", false, "Should damage from Egocentrism be allowed to proc your on-hit items?");
            AllowGloopProcs = config.Bind<bool>(ArtifactName, "Allow Genesis Loop to proc items", true, "Should damage from Genesis Loop be allowed to proc your on-hit items?");
            AllowAspectPassiveProcs = config.Bind<bool>(ArtifactName, "Allow elite passives to proc items", true, "Should damage from perfected & malachite elites' passive attacks be allowed to proc your on-hit items? Twisted's passive can proc, but is internally seen as razorwire, so as far as I know I'm unable to let it proc with the artifact on.");
            AllowProcCrits = config.Bind<bool>(ArtifactName, "Allow item procs to crit", true, "Should damage from item procs be allowed to crit?");
        }
        public override void Hooks()
        {
            // ty old InfiniteProcChains mod for a reference on how to do this stuff
            On.RoR2.HealthComponent.TakeDamage += HealthComponent_OnTakeDamage;
        }

        private void LogDamageInfo(DamageInfo damageInfo)
        {
            Log.Debug("");
            Log.Debug("");
            Log.Debug("");
            Log.Debug($"attacker is {damageInfo.attacker}");
            Log.Debug($"canRejectForce is {damageInfo.canRejectForce}");
            Log.Debug($"crit is {damageInfo.crit}");
            Log.Debug($"damage is {damageInfo.damage}");
            Log.Debug($"damageColorIndex is {damageInfo.damageColorIndex}");
            Log.Debug($"damageType is {damageInfo.damageType}");
            Log.Debug($"damageType.damageType is {damageInfo.damageType.damageType}");
            Log.Debug($"damageType.damageTypeCombined is {damageInfo.damageType.damageTypeCombined}");
            Log.Debug($"damageType.damageTypeExtended is {damageInfo.damageType.damageTypeExtended}");
            Log.Debug($"delayedDamageSecondHalf is {damageInfo.delayedDamageSecondHalf}");
            Log.Debug($"dotIndex is {damageInfo.dotIndex}");
            Log.Debug($"force is {damageInfo.force}");
            Log.Debug($"inflictor is {damageInfo.inflictor}");
            Log.Debug($"position is {damageInfo.position}");
            Log.Debug($"procChainMask is {damageInfo.procChainMask}");
            Log.Debug($"procCoefficient is {damageInfo.procCoefficient}");
            Log.Debug($"rejected is {damageInfo.rejected}");
        }

        private bool IsInflictorFromItem(string inflictor)
        {
            switch (inflictor)
            {
                // doing it via strings is dumb i know but loading the prefab and checking against directly that didn't work
                // also im assuming checking the whole string is faster than .Contains with the actual inflictor name but idk

                // items
                case "IcicleAura(Clone) (UnityEngine.GameObject)":
                case "DaggerProjectile(Clone) (UnityEngine.GameObject)":
                case "RunicMeteorStrikeImpact(Clone) (UnityEngine.GameObject)":
                    return true;
                case "ShurikenProjectile(Clone) (UnityEngine.GameObject)":
                    return !AllowShurikenProcs.Value;
                // ego and gloop get separate configs because gloop proccing gives it a cool niche and ego prolly too busted with proc chains
                case "LunarSunProjectile(Clone) (UnityEngine.GameObject)":
                    return !AllowEgoProcs.Value; // returns false if config allows procs, vice versa if not
                case "VagrantNovaItemBodyAttachment(Clone) (UnityEngine.GameObject)":
                    return !AllowGloopProcs.Value;

                // equipments
                case "GoldGatController(Clone) (UnityEngine.GameObject)":
                case "MissileProjectile(Clone) (UnityEngine.GameObject)":
                case "MeteorStorm(Clone) (UnityEngine.GameObject)":
                case "Sawmerang(Clone) (UnityEngine.GameObject)":
                case "VendingMachineProjectile(Clone) (UnityEngine.GameObject)":
                case "FireballVehicle(Clone) (UnityEngine.GameObject)":
                    return !AllowEquipmentProcs.Value;

                // aspects
                case "LunarMissileProjectile(Clone) (UnityEngine.GameObject)":
                case "PoisonOrbProjectile(Clone) (UnityEngine.GameObject)":
                case "PoisonStakeProjectile(Clone) (UnityEngine.GameObject)":
                    return !AllowAspectPassiveProcs.Value;

                default:
                    return false;
            }
        }

        private void HealthComponent_OnTakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            if (!ArtifactEnabled)
            {
                orig(self, damageInfo);
                return;
            }

            /*
             *  if a survivor shoots with hitscan, the inflictor is the attacker and the proc chain mask is 0
             *  if a survivor fires a unique projectile from a skill, the inflictor isn't the attacker and the proc chain mask is still 0
             *  any item procs add to the proc chain mask, so when it's not 0 it's guranteed to be from an item hit that might be trying to chain
             *  some item procs (polylute) and some equips (capacitor) can have no inflictor, but some still do (atg, missle launcher)
             *  missile launcher has a missle inflictor like atg, but no procchainmask since it's an initial attack unlike atg
             */

            //LogDamageInfo(damageInfo);
            if (damageInfo.procChainMask.mask > 0)
            {
                damageInfo.procCoefficient = 0;
                if (!AllowProcCrits.Value)
                {
                    damageInfo.crit = false;
                }
            }
            else if (damageInfo.inflictor == null)
            {
                // checking for royal capacitor here since it has no inflictor
                if (AllowEquipmentProcs.Value && damageInfo.damageType.damageTypeCombined == 131104)
                {
                    orig(self, damageInfo);
                    return;
                }
                // checking for no crit procs here might be bad but uh fuck around and find out i guess
                if (!AllowProcCrits.Value)
                {
                    damageInfo.crit = false;
                }
                damageInfo.procCoefficient = 0;
            }
            else if (IsInflictorFromItem(damageInfo.inflictor.ToString()))
            {
                damageInfo.procCoefficient = 0;
            }
            orig(self, damageInfo);
            return;
        }
    }
}