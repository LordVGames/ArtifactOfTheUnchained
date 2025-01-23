using System.Runtime.CompilerServices;
using RiskOfOptions.Options;
using RiskOfOptions;
using RoR2;
using SS2;
using UnityEngine;

namespace ArtifactOfTheUnchainedMod
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
                    new IntFieldOption(
                        ConfigOptions.ProcChainAmountLimit
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
                    new CheckBoxOption(
                        ConfigOptions.AllowEquipmentProcs
                    )
                );
                ModSettingsManager.AddOption(
                    new CheckBoxOption(
                        ConfigOptions.AllowSawmerangProcs
                    )
                );
                ModSettingsManager.AddOption(
                    new CheckBoxOption(
                        ConfigOptions.AllowElectricBoomerangProcs
                    )
                );
                ModSettingsManager.AddOption(
                    new CheckBoxOption(
                        ConfigOptions.AllowGenericMissileProcs
                    )
                );
                ModSettingsManager.AddOption(
                    new CheckBoxOption(
                        ConfigOptions.AllowFireworkProcs
                    )
                );
                ModSettingsManager.AddOption(
                    new CheckBoxOption(
                        ConfigOptions.AllowShurikenProcs
                    )
                );
                ModSettingsManager.AddOption(
                    new CheckBoxOption(
                        ConfigOptions.AllowEgoProcs
                    )
                );
                ModSettingsManager.AddOption(
                    new CheckBoxOption(
                        ConfigOptions.AllowGloopProcs
                    )
                );
                ModSettingsManager.AddOption(
                    new CheckBoxOption(
                        ConfigOptions.AllowAspectProcs
                    )
                );
                ModSettingsManager.AddOption(
                    new CheckBoxOption(
                        ConfigOptions.AllowProcCrits
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
