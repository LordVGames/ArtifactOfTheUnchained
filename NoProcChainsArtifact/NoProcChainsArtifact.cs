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
using Mono.Cecil.Cil;
using MonoMod.Cil;
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
        public const string PluginVersion = "1.1.0";
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
            AllowShurikenProcs = config.Bind<bool>(ArtifactName, "Allow Shurikens to proc items", false, "Should damage from Shurikens be allowed to proc your on-hit items?");
            AllowEgoProcs = config.Bind<bool>(ArtifactName, "Allow Egocentrism to proc items", false, "Should damage from Egocentrism be allowed to proc your on-hit items?");
            AllowFireworkProcs = config.Bind<bool>(ArtifactName, "Allow Fireworks to proc items", false, "Should damage from Fireworks be allowed to proc your on-hit items?");
            AllowGloopProcs = config.Bind<bool>(ArtifactName, "Allow Genesis Loop to proc items", true, "Should damage from Genesis Loop be allowed to proc your on-hit items?");
            AllowAspectPassiveProcs = config.Bind<bool>(ArtifactName, "Allow elite passives to proc items", true, "Should damage from perfected & malachite elites' passive attacks be allowed to proc your on-hit items? Twisted's passive can proc, but is internally seen as razorwire, so as far as I know I'm unable to let it proc with the artifact on.");
            AllowProcCrits = config.Bind<bool>(ArtifactName, "Allow item procs to crit", true, "Should damage from item procs be allowed to crit?");
        }
        public override void Hooks()
        {
            On.RoR2.HealthComponent.TakeDamage += HealthComponent_OnTakeDamage;
            // need to IL patch some vanilla survivor skills so they actually have an inflictor and can proc items
            IL.RoR2.Orbs.GenericDamageOrb.OnArrival += IL_GenericDamageOrb_OnArrival;
            IL.EntityStates.Huntress.HuntressWeapon.ThrowGlaive.FireOrbGlaive += IL_Huntress_ThrowGlaive_FireOrbGlaive;
            IL.EntityStates.Bandit2.StealthMode.FireSmokebomb += IL_Bandit2_StealthMode_FireSmokebomb;
            IL.EntityStates.Toolbot.ToolbotDashImpact.OnEnter += IL_Toolbot_ToolbotDashImpact_OnEnter;
        }

        private void IL_Huntress_ThrowGlaive_FireOrbGlaive(ILContext il)
        {
            ILCursor c = new(il);
            if (c.TryGotoNext(MoveType.After,
                    x => x.MatchCall<EntityStates.EntityState>("get_gameObject"),
                    x => x.MatchStfld<RoR2.Orbs.LightningOrb>("attacker")
                ))
            {
                c.Emit(OpCodes.Ldloc_0);
                c.Emit(OpCodes.Ldarg_0);
                c.Emit<EntityStates.EntityState>(OpCodes.Call, "get_gameObject");
                c.Emit<RoR2.Orbs.LightningOrb>(OpCodes.Stfld, "inflictor");

                Log.Warning($"cursor is {c}");
                Log.Warning($"il is {il}");
            }
            else
            {
                Log.Error("COULD NOT IL HOOK HUNTRESS FIREORBGLAIVE!");
                Log.Error($"cursor is {c}");
                Log.Error($"il is {il}");
            }
        }

        private void IL_GenericDamageOrb_OnArrival(ILContext il)
        {
            // this is for huntress' primaries
            // this also affects plasma shrimp but i don't think this hurts anything since it adds itself to the proc mask anyways
            ILCursor c = new(il);
            if (c.TryGotoNext(MoveType.After,
                    x => x.MatchLdloc(1),
                    x => x.MatchLdnull(),
                    x => x.MatchStfld<DamageInfo>("inflictor")
                ))
            {
                // replacing null with attacker to be set as inflictor
                c.Index -= 2;
                c.Remove();
                c.Emit(OpCodes.Ldarg_0);
                c.Emit<RoR2.Orbs.GenericDamageOrb>(OpCodes.Ldfld, "attacker");
            }
            else
            {
                Log.Error("COULD NOT IL HOOK GENERICDAMAGEORB_ONARRIVAL!");
                Log.Error($"cursor is {c}");
                Log.Error($"il is {il}");
            }
        }

        private void IL_Toolbot_ToolbotDashImpact_OnEnter(ILContext il)
        {
            // the second hit from the dash impact doesn't have an inflictor but the first hit does???
            ILCursor c = new(il);
            if (c.TryGotoNext(MoveType.After,
                    x => x.MatchCall<EntityStates.EntityState>("get_gameObject"),
                    x => x.MatchStfld<DamageInfo>("attacker")
                ))
            {
                c.Emit(OpCodes.Dup);
                c.Emit(OpCodes.Ldarg_0);
                c.Emit<EntityStates.EntityState>(OpCodes.Call, "get_gameObject");
                c.Emit<DamageInfo>(OpCodes.Stfld, "inflictor");
            }
            else
            {
                Log.Error("COULD NOT IL HOOK MUL-T DASH IMPACT!");
                Log.Error($"cursor is {c}");
                Log.Error($"il is {il}");
            }
        }

        private void IL_Bandit2_StealthMode_FireSmokebomb(ILContext il)
        {
            ILLabel label = null;
            ILCursor c = new(il);
            if (c.TryGotoNext(MoveType.After,
                    x => x.MatchLdarg(0),
                    x => x.MatchCallOrCallvirt<EntityStates.EntityState>("get_isAuthority"),
                    x => x.MatchBrfalse(out label),
                    x => x.MatchNewobj<BlastAttack>()
                ))
            {
                // pretty much the game's IL for setting the blastattack's attacker but for inflictor
                c.Emit(OpCodes.Dup);
                c.Emit(OpCodes.Ldarg_0);
                c.Emit<EntityStates.EntityState>(OpCodes.Call, "get_gameObject");
                c.Emit<BlastAttack>(OpCodes.Stfld, "inflictor");
                return;
            }
            else
            {
                Log.Error("COULD NOT IL HOOK BANDIT SMOKEBOMB!");
                Log.Error($"cursor is {c}");
                Log.Error($"il is {il}");
            }
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

        private bool IsInflictorFromItem(string inflictorName)
        {
            return inflictorName switch
            {
                // doing it via strings is dumb i know but loading the prefab and checking against directly that didn't work
                // also im assuming checking the whole string is faster than .Contains with the actual inflictor name but idk
                // items
                "IcicleAura(Clone)" or
                "DaggerProjectile(Clone)" or
                "RunicMeteorStrikeImpact(Clone)" => true,
                "FireworkProjectile(Clone)" => !AllowFireworkProcs.Value,
                "ShurikenProjectile(Clone)" => !AllowShurikenProcs.Value,
                // ego and gloop get separate configs because gloop proccing gives it a cool niche and ego prolly too busted with proc chains
                "LunarSunProjectile(Clone)" => !AllowEgoProcs.Value,
                "VagrantNovaItemBodyAttachment(Clone)" => !AllowGloopProcs.Value,

                // equipments
                "GoldGatController(Clone)" or
                "MissileProjectile(Clone)" or
                "BeamSphere(Clone)" or
                "MeteorStorm(Clone)" or
                "Sawmerang(Clone)" or
                "VendingMachineProjectile(Clone)" or
                "FireballVehicle(Clone)" => !AllowEquipmentProcs.Value,

                // aspects
                "LunarMissileProjectile(Clone)" or
                "PoisonOrbProjectile(Clone)" or
                "PoisonStakeProjectile(Clone)" => !AllowAspectPassiveProcs.Value,

                _ => false,
            };
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

            //LogDamageInfo(damageInfo);
            if (damageInfo.procChainMask.mask > 0)
            {
                // stunning doesn't work if proc coefficient is 0 which means electric boomerang needs to be checked for or it'll never stun
                if (damageInfo.inflictor && damageInfo.inflictor.ToString() == "StunAndPierceBoomerang(Clone) (UnityEngine.GameObject)")
                {
                    // no clue if setting the coefficient to this low of a decimal will affect performance or not
                    damageInfo.procCoefficient = 0.001f;
                    orig(self, damageInfo);
                    return;
                }
                if (!AllowProcCrits.Value)
                {
                    damageInfo.crit = false;
                }

                damageInfo.procCoefficient = 0;
                orig(self, damageInfo);
                return;
            }
            else if (damageInfo.inflictor == null)
            {
                /*
                 * these survivor attacks don't have an inflictor for some reason:
                 * acrid epidemic's spreading hits (it's here since i couldn't find a good way to il hook and set inflictor)
                 * ss2 executioner's special
                 * nemesis enforcer's minigun-stance secondary
                 * miner's secondary & third abilities
                 */
                switch (damageInfo.attacker.name)
                {
                    case "CrocoBody(Clone)":
                        if (damageInfo.damageType.damageTypeCombined == 4096)
                        {
                            orig(self, damageInfo);
                            return;
                        }
                        break;
                    case "Executioner2Body(Clone)":
                        if (damageInfo.damageType.damageTypeCombined == 393216)
                        {
                            orig(self, damageInfo);
                            return;
                        }
                        break;
                    case "NemesisEnforcerBody(Clone)":
                        if (damageInfo.damageType.damageTypeCombined == 131104)
                        {
                            orig(self, damageInfo);
                            return;
                        }
                        break;
                    case "MinerBody(Clone)":
                        switch (damageInfo.damageType.damageTypeCombined)
                        {
                            case 131104:
                            case 131072:
                                orig(self, damageInfo);
                                return;
                        }
                        break;
                    default:
                        break;
                }
                /*
                 * these damage sources don't work properly with 0 coefficient:
                 * blood shrines (no damage)
                 * glacial elite death explosions (no freeze)
                 * capacitor works properly with 0 coefficient it just doesn't have an inflictor
                 */
                if ((AllowEquipmentProcs.Value && damageInfo.damageType.damageTypeCombined == 131104)
                    || (damageInfo.procCoefficient == 0.75 && damageInfo.damageType.damageTypeCombined == 131328)
                    || damageInfo.attacker.name == "ShrineBlood(Clone)")
                {
                    orig(self, damageInfo);
                    return;
                }
                // checking for no crit procs here might affect survivor abilities
                if (!AllowProcCrits.Value)
                {
                    damageInfo.crit = false;
                }

                damageInfo.procCoefficient = 0;
                orig(self, damageInfo);
                return;
            }
            else if (IsInflictorFromItem(damageInfo.inflictor.name))
            {
                damageInfo.procCoefficient = 0;
            }

            orig(self, damageInfo);
            return;
        }
    }
}