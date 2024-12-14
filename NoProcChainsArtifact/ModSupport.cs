using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using RiskOfOptions.Options;
using RiskOfOptions;
using RoR2;
using SS2;
using UnityEngine;

namespace NoProcChainsArtifact
{
    internal static class ModSupport
    {
        public static class RiskOfOptions
        {
            private static bool? _modexists;
            public static bool ModIsRunning
            {
                get
                {
                    if (_modexists == null)
                    {
                        _modexists = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(PluginInfo.PLUGIN_GUID);
                    }
                    return (bool)_modexists;
                }
            }

            [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
            public static void AddOptions()
            {
                ModSettingsManager.SetModIcon(Assets.AssetBundle.LoadAsset<Sprite>("RoOIcon.png"));
                ModSettingsManager.SetModDescription("Adds an artifact that prevents your items from proccing your on-hit items for you.");

                ModSettingsManager.AddOption(
                    new CheckBoxOption(
                        ArtifactOfTheUnchained.AllowEquipmentProcs
                    )
                );
                ModSettingsManager.AddOption(
                    new CheckBoxOption(
                        ArtifactOfTheUnchained.AllowSawmerangProcs
                    )
                );
                ModSettingsManager.AddOption(
                    new CheckBoxOption(
                        ArtifactOfTheUnchained.AllowElectricBoomerangProcs
                    )
                );
                ModSettingsManager.AddOption(
                    new CheckBoxOption(
                        ArtifactOfTheUnchained.AllowGenericMissileProcs
                    )
                );
                ModSettingsManager.AddOption(
                    new CheckBoxOption(
                        ArtifactOfTheUnchained.AllowFireworkProcs
                    )
                );
                ModSettingsManager.AddOption(
                    new CheckBoxOption(
                        ArtifactOfTheUnchained.AllowShurikenProcs
                    )
                );
                ModSettingsManager.AddOption(
                    new CheckBoxOption(
                        ArtifactOfTheUnchained.AllowEgoProcs
                    )
                );
                ModSettingsManager.AddOption(
                    new CheckBoxOption(
                        ArtifactOfTheUnchained.AllowGloopProcs
                    )
                );
                ModSettingsManager.AddOption(
                    new CheckBoxOption(
                        ArtifactOfTheUnchained.AllowAspectProcs
                    )
                );
                ModSettingsManager.AddOption(
                    new CheckBoxOption(
                        ArtifactOfTheUnchained.AllowProcCrits
                    )
                );
            }
        }

        internal static class Starstorm2
        {
            private static bool? _modexists;
            public static bool ModIsRunning
            {
                get
                {
                    if (_modexists == null)
                    {
                        _modexists = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(SS2Main.GUID);
                    }
                    return (bool)_modexists;
                }
            }

            internal static ItemDef ChirrFlowerItemDef
            {
                get
                {
                    if (ModIsRunning)
                    {
                        return SS2Content.Items.FlowerTurret;
                    }
                    return null;
                }
            }
        }
    }
}
