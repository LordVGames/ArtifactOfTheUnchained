using System;
using System.Runtime.CompilerServices;
using RiskOfOptions.Options;
using RiskOfOptions;
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
                    new StringInputFieldOption(
                        ConfigOptions.ItemProcNerfBodyBlacklist
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
                    new CheckBoxOption(
                        ConfigOptions.PreventAllItemChaining
                    )
                );
            }
        }
    }
}