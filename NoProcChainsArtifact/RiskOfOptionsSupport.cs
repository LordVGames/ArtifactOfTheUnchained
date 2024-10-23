using UnityEngine;
using RiskOfOptions;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;

namespace NoProcChainsArtifact
{
    public static class RiskOfOptionsSupport
    {
        public const string PluginName = "com.rune580.riskofoptions";
        private static bool? _modexists;
        public static bool ModIsRunning
        {
            get
            {
                if (_modexists == null)
                {
                    _modexists = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(PluginName);
                }
                return (bool)_modexists;
            }
        }

        public static void AddOptions()
        {
            ModSettingsManager.SetModIcon(ModAssets.AssetBundle.LoadAsset<Sprite>("RoOIcon.png"));
            ModSettingsManager.SetModDescription("Adds some chat messages that you can configure the contents of, along with when they appear in your runs.");

            ModSettingsManager.AddOption(
                new CheckBoxOption(
                    ArtifactOfTheUnchained.AllowEquipmentProcs
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
                    ArtifactOfTheUnchained.AllowAspectPassiveProcs
                )
            );
            ModSettingsManager.AddOption(
                new CheckBoxOption(
                    ArtifactOfTheUnchained.AllowProcCrits
                )
            );
        }
    }
}
