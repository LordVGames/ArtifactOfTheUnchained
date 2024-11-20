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
using UnityEngine.AddressableAssets;

namespace NoProcChainsArtifact
{
    [BepInDependency(LanguageAPI.PluginGUID, BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency(R2APIContentManager.PluginGUID, BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency(RiskOfOptions.PluginInfo.PLUGIN_GUID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class NoProcChainsArtifact : BaseUnityPlugin
    {
        public static PluginInfo PluginInfo { get; private set; }
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "LordVGames";
        public const string PluginName = "NoProcChainsArtifact";
        public const string PluginVersion = "1.1.4";
        public List<ArtifactBase> Artifacts = [];

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
        public static ConfigEntry<bool> AllowSawmerangProcs;
        public static ConfigEntry<bool> AllowElectricBoomerangProcs;
        public static ConfigEntry<bool> AllowFireworkProcs;
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
        public override void Init(ConfigFile config)
        {
            CreateLang();
            CreateConfig(config);
            CreateArtifact();
            Hooks();
            if (RiskOfOptionsSupport.ModIsRunning)
            {
                RiskOfOptionsSupport.AddOptions();
            }
        }
        private void CreateConfig(ConfigFile config)
        {
            AllowEquipmentProcs = config.Bind<bool>(ArtifactName, "Allow equipments to proc items", false, "Should damage from equipments be allowed to proc your on-hit items?");
            AllowSawmerangProcs = config.Bind<bool>(ArtifactName, "Allow Sawmerang to proc", true, "Should damage from Sawmerang be allowed to proc your on-hit items? WARNING: This will also disable the equipment's built-in bleed on hit!\nThis setting is completely separate from the general equipment procs setting, meaning you can let sawmerang proc while preventing all other equipments from proccing.");
            AllowElectricBoomerangProcs = config.Bind<bool>(ArtifactName, "Allow Electric Boomerang to proc", true, "Should damage from Electric Boomerang be allowed to proc your on-hit items? WARNING: This will also disable the item's built-in stun on hit!");
            AllowGloopProcs = config.Bind<bool>(ArtifactName, "Allow Genesis Loop to proc items", true, "Should damage from Genesis Loop be allowed to proc your on-hit items?");
            AllowShurikenProcs = config.Bind<bool>(ArtifactName, "Allow Shurikens to proc items", false, "Should damage from Shurikens be allowed to proc your on-hit items?");
            AllowEgoProcs = config.Bind<bool>(ArtifactName, "Allow Egocentrism to proc items", false, "Should damage from Egocentrism be allowed to proc your on-hit items?");
            AllowFireworkProcs = config.Bind<bool>(ArtifactName, "Allow Fireworks to proc items", false, "Should damage from Fireworks be allowed to proc your on-hit items?");
            AllowAspectPassiveProcs = config.Bind<bool>(ArtifactName, "Allow elite passives to proc items", true, "Should damage from perfected & malachite elites' passive attacks be allowed to proc your on-hit items? Twisted's passive can proc, but is internally seen as razorwire, so as far as I know I'm unable to let it proc with the artifact on.");
            AllowProcCrits = config.Bind<bool>(ArtifactName, "Allow item procs to crit", true, "Should damage from item procs be allowed to crit?");
        }
        public override void Hooks()
        {
            On.RoR2.HealthComponent.TakeDamage += HealthComponent_OnTakeDamage;
            // need to IL patch some vanilla survivor skills so they actually have an inflictor and can proc items
            ILHooks.SetupILHooks();
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
            Log.Debug($"procChainMask.mask is {damageInfo.procChainMask.mask}");
            Log.Debug($"procCoefficient is {damageInfo.procCoefficient}");
            Log.Debug($"rejected is {damageInfo.rejected}");
        }

        private void SetCoefficientIfFromItem(DamageInfo damageInfo)
        {
            switch (damageInfo.inflictor.name)
            {
                #region items
                case "IcicleAura(Clone)":
                case "DaggerProjectile(Clone)":
                case "RunicMeteorStrikeImpact(Clone)":
                // ss2
                case "LampBulletPlayer(Clone)":
                // enemiesreturns
                case "IfritPylonPlayerBody(Clone)":
                // sivscontent
                case "MushroomWard(Clone)":
                case "ThunderAuraStrike(Clone)":
                case "LunarShockwave(Clone)":
                    damageInfo.procCoefficient = 0;
                    break;

                case "FireworkProjectile(Clone)":
                    if (!AllowFireworkProcs.Value)
                    {
                        damageInfo.procCoefficient = 0;
                    }
                    break;
                case "ShurikenProjectile(Clone)":
                    if (!AllowShurikenProcs.Value)
                    {
                        damageInfo.procCoefficient = 0;
                    }
                    break;
                // ego and gloop get separate configs because gloop proccing gives it a cool niche and ego prolly too busted with proc chains
                case "LunarSunProjectile(Clone)":
                    if (!AllowEgoProcs.Value)
                    {
                        damageInfo.procCoefficient = 0;
                    }
                    break;
                case "VagrantNovaItemBodyAttachment(Clone)":
                    if (!AllowGloopProcs.Value)
                    {
                        damageInfo.procCoefficient = 0;
                    }
                    break;
                #endregion

                #region equipments
                case "GoldGatController(Clone)":
                case "MissileProjectile(Clone)":
                case "BeamSphere(Clone)":
                case "MeteorStorm(Clone)":
                case "VendingMachineProjectile(Clone)":
                case "FireballVehicle(Clone)":
                    if (!AllowEquipmentProcs.Value)
                    {
                        damageInfo.procCoefficient = 0;
                    }
                    break;
                /*
                 * i would give sawmerang a really low proc coeff but bleed duration scales off of that too
                 * so even though it's guranteed to apply it does one tick of damage then all bleed stacks goes away
                 * its either all procs or no procs
                 * this also applies to electric boomerang's stun
                 * thanks hopoo (in general) /s
                 */
                case "Sawmerang(Clone)":
                    if (!AllowSawmerangProcs.Value)
                    {
                        damageInfo.procCoefficient = 0;
                    }
                    break;
                    
                #region aspects
                case "LunarMissileProjectile(Clone)":
                case "PoisonOrbProjectile(Clone)":
                case "PoisonStakeProjectile(Clone)":
                    if (!AllowAspectPassiveProcs.Value)
                    {
                        // this also makes malachite debuff & perfected cripple not apply
                        damageInfo.procCoefficient = 0;
                    }
                    break;
                #endregion
                #endregion
            }
        }

        private bool IsDamageBrokenWithoutInflictor(DamageInfo damageInfo)
        {
            /*
             * these survivor attacks don't have an inflictor and break because of the artifact:
             * acrid epidemic's spreading hits (it's here since i couldn't find a good way to il hook and set inflictor)
             * ss2 executioner's special
             * nemesis enforcer's minigun-stance secondary
             * miner's secondary & third abilities
             * 
             * also checking for sulfur pods here since they don't debuff you with no proc coefficient
             * yet aqueducts tar pots still work
             * and also blood shrines because they deal no damage with 0 coefficient (????)
            */
            switch (damageInfo.attacker.name)
            {
                case "CrocoBody(Clone)":
                    if (damageInfo.damageType.damageTypeCombined == 4096)
                    {
                        return true;
                    }
                    break;
                case "Executioner2Body(Clone)":
                    if (damageInfo.damageType.damageTypeCombined == 393216)
                    {
                        return true;
                    }
                    break;
                case "NemesisEnforcerBody(Clone)":
                    if (damageInfo.damageType.damageTypeCombined == 131104)
                    {
                        return true;
                    }
                    break;
                case "MinerBody(Clone)":
                    switch (damageInfo.damageType.damageTypeCombined)
                    {
                        case 131104:
                        case 131072:
                            return true;
                    }
                    break;
                case "SulfurPodBody(Clone)":
                case "ShrineBlood(Clone)":
                    return true;
            }
            switch (damageInfo.damageType.damageTypeCombined)
            {
                // capacitor has no inflictor so even when config allows it it still won't proc
                case 131104:
                    if (AllowEquipmentProcs.Value)
                    {
                        return true;
                    }
                    break;
                // glacial death bubble won't with with 0 coefficient
                case 131328:
                    if (damageInfo.procCoefficient == 0.75)
                    {
                        return true;
                    }
                    break;
            }
            return false;
        }

        private void HealthComponent_OnTakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            // ty old InfiniteProcChains mod for a reference on how to do this stuff

            if (!ArtifactEnabled)
            {
                orig(self, damageInfo);
                return;
            }
            // void fog and probably other things have no inflictor nor attacker and can cause errors if not checked for
            if (damageInfo.inflictor == null && damageInfo.attacker == null)
            {
                orig(self, damageInfo);
                return;
            }

            /*
             *  if a survivor shoots with hitscan, the inflictor and the attacker are both the survivor and the proc chain mask is 0
             *  if a survivor fires a unique projectile from a skill, the inflictor isn't the survivor but the attacker is and the proc chain mask is still 0
             *  any item procs add to the proc chain mask when they hit, so when the procchainmask is not 0 it's guranteed to be from an item hit
             *  some item procs (polylute) and some equips (capacitor) can have no inflictor, but some still do (atg, missle launcher)
             *  missile launcher has a missle inflictor like atg, but has no procchainmask since it doesn't add itself to the mask
             */
#if DEBUG
            LogDamageInfo(damageInfo);
#endif
            if (damageInfo.procChainMask.mask > 0)
            {
#if DEBUG
                Log.Warning($"ITEM TRYING TO PROC CHAIN DETECTED");
#endif
                if (!AllowProcCrits.Value)
                {
                    damageInfo.crit = false;
                }
                // letting electric boomerang proc and thus do its stun
                if (damageInfo.procChainMask.mask == 8388608 && AllowElectricBoomerangProcs.Value)
                {
                    orig(self, damageInfo);
                    return;
                }

                damageInfo.procCoefficient = 0;
                orig(self, damageInfo);
                return;
            }
            else if (damageInfo.inflictor == null)
            {
                if (IsDamageBrokenWithoutInflictor(damageInfo))
                {
                    orig(self, damageInfo);
                    return;
                }
#if DEBUG
                Log.Warning("INFLICTORLESS DAMAGE DETECTED");
#endif
                // checking for no crit procs here might affect survivor abilities
                if (!AllowProcCrits.Value)
                {
                    damageInfo.crit = false;
                }

                damageInfo.procCoefficient = 0;
                orig(self, damageInfo);
                return;
            }
            SetCoefficientIfFromItem(damageInfo);
#if DEBUG
            if (damageInfo.procCoefficient > 0.1)
            {
                Log.Warning("DAMAGE FROM SPECIFIC ITEM DETECTED");
            }
#endif

            orig(self, damageInfo);
            return;
        }
    }
}