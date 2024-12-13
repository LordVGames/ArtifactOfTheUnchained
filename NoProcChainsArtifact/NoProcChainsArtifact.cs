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
        public static PluginInfo PluginInfo { get; private set; }
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "LordVGames";
        public const string PluginName = "NoProcChainsArtifact";
        public const string PluginVersion = "1.2.0";
        public List<ArtifactBase> Artifacts = [];

        public void Awake()
        {
            PluginInfo = Info;
            Log.Init(Logger);
            Assets.Init();

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
        public static ConfigEntry<bool> AllowGenericMissileProcs;
        public static ConfigEntry<bool> AllowFireworkProcs;
        public static ConfigEntry<bool> AllowShurikenProcs;
        public static ConfigEntry<bool> AllowEgoProcs;
        public static ConfigEntry<bool> AllowGloopProcs;
        public static ConfigEntry<bool> AllowAspectPassiveProcs;
        public static ConfigEntry<bool> AllowProcCrits;
        public override string ArtifactLangTokenName => "NO_PROC_CHAINS";
        public override string ArtifactName => "Artifact of the Unchained";
        public override string ArtifactDescription => "All item effects cannot proc your on-hit item effects for you.";
        public override Sprite ArtifactEnabledIcon => Assets.AssetBundle.LoadAsset<Sprite>("NoProcChainsArtifactIcon_Enabled.png");
        public override Sprite ArtifactDisabledIcon => Assets.AssetBundle.LoadAsset<Sprite>("NoProcChainsArtifactIcon_Disabled.png");
        public override void Init(ConfigFile config)
        {
            CreateLang();
            CreateConfig(config);
            CreateArtifact();
            Hooks();
            if (ModSupport.RiskOfOptions.ModIsRunning)
            {
                ModSupport.RiskOfOptions.AddOptions();
            }
        }
        private void CreateConfig(ConfigFile config)
        {
            AllowEquipmentProcs = config.Bind<bool>(ArtifactName, "Allow equipments to proc items", true, "Should damage from equipments be allowed to proc your on-hit items?");
            AllowSawmerangProcs = config.Bind<bool>(ArtifactName, "Allow Sawmerang to proc", true, "Should damage from Sawmerang be allowed to proc your on-hit items? WARNING: This will also disable the equipment's built-in bleed on hit!\nThis setting is completely separate from the general equipment procs setting, meaning you can let sawmerang proc while preventing all other equipments from proccing.");
            AllowElectricBoomerangProcs = config.Bind<bool>(ArtifactName, "Allow Electric Boomerang to proc", true, "Should damage from Electric Boomerang be allowed to proc your on-hit items? WARNING: This will also disable the item's built-in stun on hit!");
            AllowGenericMissileProcs = config.Bind<bool>(ArtifactName, "Allow Missiles to proc", false, "Should damage from any item that isn't ATG that fires missiles be allowed to proc your on-hit items? WARNING: This counts for Starstorm 2's Armed Backpack alongside Disposable Missile Launcher!");
            AllowGloopProcs = config.Bind<bool>(ArtifactName, "Allow Genesis Loop to proc items", true, "Should damage from Genesis Loop be allowed to proc your on-hit items?");
            AllowShurikenProcs = config.Bind<bool>(ArtifactName, "Allow Shurikens to proc items", false, "Should damage from Shurikens be allowed to proc your on-hit items?");
            AllowEgoProcs = config.Bind<bool>(ArtifactName, "Allow Egocentrism to proc items", false, "Should damage from Egocentrism be allowed to proc your on-hit items?");
            AllowFireworkProcs = config.Bind<bool>(ArtifactName, "Allow Fireworks to proc items", false, "Should damage from Fireworks be allowed to proc your on-hit items?");
            AllowAspectPassiveProcs = config.Bind<bool>(ArtifactName, "Allow elite passives to proc items", true, "Should damage from perfected & malachite elites' passive attacks be allowed to proc your on-hit items? Twisted's passive can proc, but is internally seen as razorwire, so as far as I know I'm unable to let it proc with the artifact on.");
            AllowProcCrits = config.Bind<bool>(ArtifactName, "Allow item procs to crit", true, "Should damage from item procs be allowed to crit?");
        }
        public override void Hooks()
        {
            On.RoR2.CharacterMaster.OnBodyDamaged += CharacterMaster_OnBodyDamaged;
        }

        private void LogDamageInfo(DamageInfo damageInfo)
        {
            Log.Debug("");
            Log.Debug("");
            Log.Debug("");
            Log.Debug($"damageInfo.damageType.damageSource is {damageInfo.damageType.damageSource}");
            Log.Debug($"damageInfo.damageType.IsDamageSourceSkillBased is {damageInfo.damageType.IsDamageSourceSkillBased}");
            Log.Debug($"attacker is {damageInfo.attacker}");
            Log.Debug($"canRejectForce is {damageInfo.canRejectForce}");
            Log.Debug($"crit is {damageInfo.crit}");
            Log.Debug($"damage is {damageInfo.damage}");
            Log.Debug($"damageColorIndex is {damageInfo.damageColorIndex}");
            Log.Debug($"damageType is {damageInfo.damageType}");
            Log.Debug($"damageType.damageType is {damageInfo.damageType.damageType}");
            Log.Debug($"damageType.damageTypeCombined is {damageInfo.damageType.damageTypeCombined}");
            Log.Debug($"damageType.damageTypeExtended is {damageInfo.damageType.damageTypeExtended}");

            Log.Debug($"dotIndex is {damageInfo.dotIndex}");
            Log.Debug($"force is {damageInfo.force}");
            Log.Debug($"inflictor is {damageInfo.inflictor}");
            Log.Debug($"position is {damageInfo.position}");
            Log.Debug($"procChainMask is {damageInfo.procChainMask}");
            Log.Debug($"procChainMask.mask is {damageInfo.procChainMask.mask}");
            Log.Debug($"procCoefficient is {damageInfo.procCoefficient}");
            Log.Debug($"rejected is {damageInfo.rejected}");
        }

        private bool SetConfiguredItemProcCoefficient(DamageInfo damageInfo)
        {
            bool isDamageFromConfiguredItem = false;
            switch (damageInfo.inflictor.name)
            {
                #region items
                case "StunAndPierceBoomerang(Clone)":
                    isDamageFromConfiguredItem = true;
                    if (!AllowFireworkProcs.Value)
                    {
                        damageInfo.procCoefficient = 0;
                        isDamageFromConfiguredItem = false;
                    }
                    break;
                case "FireworkProjectile(Clone)":
                    isDamageFromConfiguredItem = true;
                    if (!AllowFireworkProcs.Value)
                    {
                        damageInfo.procCoefficient = 0;
                    }
                    break;
                case "ShurikenProjectile(Clone)":
                    isDamageFromConfiguredItem = true;
                    if (!AllowShurikenProcs.Value)
                    {
                        damageInfo.procCoefficient = 0;
                    }
                    break;
                case "LunarSunProjectile(Clone)":
                    isDamageFromConfiguredItem = true;
                    if (!AllowEgoProcs.Value)
                    {
                        damageInfo.procCoefficient = 0;
                    }
                    break;
                case "VagrantNovaItemBodyAttachment(Clone)":
                    isDamageFromConfiguredItem = true;
                    if (!AllowGloopProcs.Value)
                    {
                        damageInfo.procCoefficient = 0;
                    }
                    break;
                #endregion

                #region equipments
                case "GoldGatController(Clone)":
                case "BeamSphere(Clone)":
                case "MeteorStorm(Clone)":
                case "VendingMachineProjectile(Clone)":
                case "FireballVehicle(Clone)":
                    isDamageFromConfiguredItem = true;
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
                 * thanks hopoo /s
                 */
                case "Sawmerang(Clone)":
                    isDamageFromConfiguredItem = true;
                    if (!AllowSawmerangProcs.Value)
                    {
                        damageInfo.procCoefficient = 0;
                    }
                    break;
                // atg triggers this but it can be checked for with the procchainmask
                // starstorm 2's armed backpack also triggers this but i can't check for it specifically
                case "MissileProjectile(Clone)":
                    isDamageFromConfiguredItem = true;
                    if (damageInfo.procChainMask.mask > 0)
                    {
                        damageInfo.procCoefficient = 0;
                    }
                    else if (!AllowGenericMissileProcs.Value)
                    {
                        damageInfo.procCoefficient = 0;
                    }
                    break;

                #region aspects
                case "LunarMissileProjectile(Clone)":
                case "PoisonOrbProjectile(Clone)":
                case "PoisonStakeProjectile(Clone)":
                case "BeadProjectileTrackingBomb(Clone)":
                    isDamageFromConfiguredItem = true;
                    if (!AllowAspectPassiveProcs.Value)
                    {
                        // this also makes malachite debuff & perfected cripple not apply
                        damageInfo.procCoefficient = 0;
                    }
                    break;
                #endregion
                #endregion
            }
            return isDamageFromConfiguredItem;
        }

        private bool IsDamageBrokenWithoutInflictor(DamageInfo damageInfo)
        {
            // glacial death bubble won't freeze with with 0 coefficient
            if (damageInfo.damageType.damageTypeCombined == 131328)
            {
                return true;
            }
            /*
             * blood shrines deal no damaage when set with 0 proc coefficient for some reason
             * sulfur pods also break since they don't debuff you with no proc coefficient
             * (yet aqueducts tar pots still tar you without coefficient)
            */
            switch (damageInfo.attacker.name)
            {
                case "SulfurPodBody(Clone)":
                case "ShrineBlood(Clone)":
                    return true;
            }
            return false;
        
        }

        private void CharacterMaster_OnBodyDamaged(On.RoR2.CharacterMaster.orig_OnBodyDamaged orig, CharacterMaster self, DamageReport damageReport)
        {
            orig(self, damageReport);
            // ty old InfiniteProcChains mod for a reference on how to do this stuff
            if (!ArtifactEnabled)
            {
                orig(self, damageReport);
                return;
            }
#if DEBUG
            LogDamageInfo(damageReport.damageInfo);
#endif
            // void fog and probably other things have no inflictor nor attacker and that can cause errors if not checked for
            //if (damageInfo.inflictor == null && damageInfo.attacker == null)
            if (!damageReport.attacker)
            {
                orig(self, damageReport);
                return;
            }
            // need to let nemesis commando gouge still proc, player or not
            if (damageReport.damageInfo.damageType.damageType.HasFlag(DamageType.DoT))
            {
                orig(self, damageReport);
                return;
            }
            // need to let electric boomerang proc and thus do its stun, even if it'll proc other items
            if (damageReport.damageInfo.procChainMask.mask == 8388608 && AllowElectricBoomerangProcs.Value)
            {
                if (!AllowProcCrits.Value)
                {
                    damageReport.damageInfo.crit = false;
                }
                orig(self, damageReport);
                return;
            }
            if (IsDamageBrokenWithoutInflictor(damageReport.damageInfo))
            {
                orig(self, damageReport);
                return;
            }
            if (damageReport.damageInfo.inflictor)
            {
                bool isDamageFromConfiguredItem = SetConfiguredItemProcCoefficient(damageReport.damageInfo);
                if (isDamageFromConfiguredItem)
                {
                    if (!AllowProcCrits.Value)
                    {
                        damageReport.damageInfo.crit = false;
                    }
                    // proc coefficient has or has not been dealt with based on the config, so don't bother continuing
                    orig(self, damageReport);
                    return;
                }
            }



            // players can only proc items from their skills
            if (damageReport.attackerMaster.playerCharacterMasterController)
            {
                if (!damageReport.damageInfo.damageType.IsDamageSourceSkillBased)
                {
                    damageReport.damageInfo.procCoefficient = 0;
                }
            }
            // you'd think the damagesource stuff would apply to monsters too, but all monster skills are considered "NoneSpecified"
            // i would sarcastically say "thanks gearbox" but i don't blame them for not going through every enemy attack and correctly setting each one
            // although the ss2 devs did it with their enemies so it's not like it's impossible
            else if (damageReport.damageInfo.procChainMask.mask > 0)
            {
                damageReport.damageInfo.procCoefficient = 0;
                orig(self, damageReport);
                return;
            }
            // chirr flower turret counts for this check but it should always be allowed to proc items to let tamed ground enemies do something against flying ones
            // but the flower turret's attack is really generic so it's kinda hard to distinguish it from other item procs
            else if (!damageReport.damageInfo.inflictor && ModSupport.Starstorm2.ChirrFlowerItemDef)
            {
                if (damageReport.attackerBody.inventory.GetItemCount(ModSupport.Starstorm2.ChirrFlowerItemDef) > 0)
                {
                    DamageTypeCombo genericDamageType = DamageType.Generic;
                    genericDamageType.damageTypeExtended = DamageTypeExtended.Generic;
                    genericDamageType.damageSource = DamageSource.NoneSpecified;
                    if (damageReport.damageInfo.damageType != genericDamageType)
                    {
                        damageReport.damageInfo.procCoefficient = 0;
                    }
                }
            }

            orig(self, damageReport);
            return;
        }
    }
}