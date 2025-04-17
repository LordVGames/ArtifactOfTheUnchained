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
                ModSettingsManager.SetModDescription("Adds an artifact that prevents your items from proccing your on-hit items for you.");


                ModSettingsManager.AddOption(
                    new StringInputFieldOption(
                        ConfigOptions.ItemProcNerfBodyBlacklist
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

            [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
            internal static void HandleRelicOfForceDamageSource(DamageInfo damageInfo)
            {
                if (damageInfo.HasModdedDamageType(SS2.Items.RelicOfForce.relicForceDamageType))
                {
                    damageInfo.damageType.damageSource = DamageSource.NoneSpecified;
                }
            }
        }
    }
}