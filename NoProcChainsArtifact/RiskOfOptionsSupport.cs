using System.Runtime.CompilerServices;
using UnityEngine;
using RiskOfOptions;
using RiskOfOptions.Options;

namespace NoProcChainsArtifact
{
    public static class RiskOfOptionsSupport
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
            ModSettingsManager.SetModIcon(ModAssets.AssetBundle.LoadAsset<Sprite>("RoOIcon.png"));
            ModSettingsManager.SetModDescription("Adds an artifact that disables proc chains and prevents most items from starting a proc chain.");

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
