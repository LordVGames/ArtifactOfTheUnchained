using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using RoR2;
using RiskOfOptions;
using RiskOfOptions.Options;
using SS2;
using HarmonyLib;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;
using R2API;

namespace ArtifactOfTheUnchainedMod
{
    internal static class ModSupport
    {
        internal static class RiskOfOptions
        {
            private static bool? _modexists;
            internal static bool ModIsRunning
            {
                get
                {
                    _modexists ??= BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(PluginInfo.PLUGIN_GUID);
                    return (bool)_modexists;
                }
            }

            [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
            internal static void AddOptions()
            {
                ModSettingsManager.SetModIcon(Assets.AssetBundle.LoadAsset<Sprite>("RoOIcon.png"));
                ModSettingsManager.SetModDescription("Adds a highly configurable artifact that lets you customize how proc chains and procs caused by items/equipments should be nerfed.");


                ModSettingsManager.AddOption(
                    new CheckBoxOption(
                        ConfigOptions.EnableDebugLogging
                    )
                );
                ModSettingsManager.AddOption(
                    new CheckBoxOption(
                        ConfigOptions.ArtifactlessMode,
                        true
                    )
                );
                ModSettingsManager.AddOption(
                    new StringInputFieldOption(
                        ConfigOptions.ItemProcNerfBodyBlacklist
                    )
                );
                ModSettingsManager.AddOption(
                    new FloatFieldOption(
                        ConfigOptions.NerfProcDamagePerEthereal
                    )
                );


                ModSettingsManager.AddOption(
                    new FloatFieldOption(
                        ConfigOptions.ProcChainDamageNerfToPercent
                    )
                );
                ModSettingsManager.AddOption(
                    new FloatFieldOption(
                        ConfigOptions.ProcChainCoefficientNerfToPercent
                    )
                );
                ModSettingsManager.AddOption(
                    new IntFieldOption(
                        ConfigOptions.ProcChainAmountLimit
                    )
                );


                ModSettingsManager.AddOption(
                    new FloatFieldOption(
                        ConfigOptions.ProcFromItemDamageNerfToPercent
                    )
                );
                ModSettingsManager.AddOption(
                    new FloatFieldOption(
                        ConfigOptions.ProcFromItemCoefficientNerfToPercent
                    )
                );


                ModSettingsManager.AddOption(
                    new FloatFieldOption(
                        ConfigOptions.ProcFromEquipmentDamageNerfToPercent
                    )
                );
                ModSettingsManager.AddOption(
                    new FloatFieldOption(
                        ConfigOptions.ProcFromEquipmentCoefficientNerfToPercent
                    )
                );


                ModSettingsManager.AddOption(
                    new CheckBoxOption(
                        ConfigOptions.PreventAllItemChaining
                    )
                );
            }
        }

        internal static class Starstorm2
        {
            private static bool? _modexists;
            internal static bool ModIsRunning
            {
                get
                {
                    _modexists ??= BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(SS2Main.GUID);
                    return (bool)_modexists;
                }
            }
            private static bool? _isBetaVersion;
            internal static bool IsBetaVersion
            {
                get
                {
                    if (_isBetaVersion == null)
                    {
                        if (BepInEx.Bootstrap.Chainloader.PluginInfos.TryGetValue(SS2Main.GUID, out var pluginInfo))
                        {
                            // will probably come back to bite me later but it's the easiest way rn
                            // and yes the beta's version is one behind the thunderstore's version thankfully
                            if (pluginInfo.Metadata.Version.ToString() == "0.6.16")
                            {
                                _isBetaVersion = true;
                            }
                            else
                            {
                                _isBetaVersion = false;
                            }
                        }
                        else
                        {
                            _isBetaVersion = false;
                        }
                    }
                    return (bool)_isBetaVersion;
                }
            }

            [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
            internal static void HandleRelicOfForceDamageSource(DamageInfo damageInfo)
            {
                if (damageInfo.HasModdedDamageType(SS2.Items.RelicOfForce.relicForceDamageType))
                {
                    damageInfo.damageType.damageSource = DamageSource.NoneSpecified;
                }
            }

            internal static int EtherealsCount
            {
                [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
                get
                {
                    return (ModIsRunning && IsBetaVersion) ? EtherealBehavior.instance.etherealsCompleted : 0;
                }
            }
        }
    }
}