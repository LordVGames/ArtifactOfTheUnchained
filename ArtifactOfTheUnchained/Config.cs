using BepInEx.Configuration;

namespace ArtifactOfTheUnchainedMod
{
    public static class ConfigOptions
    {
        public static ConfigEntry<bool> ServersideMode;

        public static ConfigEntry<int> ProcChainAmountLimit;
        public static ConfigEntry<float> ProcChainDamageNerfToPercent;
        public static ConfigEntry<float> ProcChainCoefficientNerfToPercent;

        public static ConfigEntry<bool> AllowEquipmentProcs;
        public static ConfigEntry<bool> AllowSawmerangProcs;
        public static ConfigEntry<bool> AllowElectricBoomerangProcs;
        public static ConfigEntry<bool> AllowGenericMissileProcs;
        public static ConfigEntry<bool> AllowFireworkProcs;
        public static ConfigEntry<bool> AllowShurikenProcs;
        public static ConfigEntry<bool> AllowEgoProcs;
        public static ConfigEntry<bool> AllowGloopProcs;
        public static ConfigEntry<bool> AllowAspectProcs;
        public static ConfigEntry<bool> AllowProcCrits;

        public static void BindConfigEntries(ConfigFile Config, ArtifactBase artifactInstance)
        {
            ServersideMode = Config.Bind<bool>(
                artifactInstance.ArtifactName,
                "Run serverside without artifact", false,
                "Should the mod run server-side only? This means the effects of the artifact will always on without needing to activate it."
            );

            ProcChainAmountLimit = Config.Bind<int>(
                artifactInstance.ArtifactName,
                "Proc chaining limit", 1,
                "The limit for how many times items can proc each other. 0 means item procs cannot chain into any other items, i.e. an atg hit will never chain into anytihng else. Set to -1 for vanilla behavior."
            );
            ProcChainDamageNerfToPercent = Config.Bind<float>(
                artifactInstance.ArtifactName,
                "Nerf chained procs damage", 0.2f,
                "Should damage from chained procs (i.e. polylute from an atg) be reduced to a percent of what they would normally do? I.E. 0.2 is 20% of the normal damage, and 1 is vanilla behavior as there is no change."
            );
            ProcChainCoefficientNerfToPercent = Config.Bind<float>(
                artifactInstance.ArtifactName,
                "Nerf chained procs coefficient", 1,
                "Should the proc coefficients for chained procs (i.e. polylute from an atg) be reduced to a percent of what they would normally are? I.E. 0.1 is 10% of the normal proc coefficient, and 1 is vanilla behavior as there is no change."
            );



            AllowEquipmentProcs = Config.Bind<bool>(
                artifactInstance.ArtifactName,
                "Allow equipments to proc items", true,
                "Should damage from equipments be allowed to proc your on-hit items? This works for all vanilla equipment except for Forgive Me Please and a few that have their own config option."
            );
            AllowSawmerangProcs = Config.Bind<bool>(
                artifactInstance.ArtifactName,
                "Allow Sawmerang to proc", true,
                "Should damage from Sawmerang be allowed to proc your on-hit items? WARNING: This will also disable the equipment's built-in bleed on hit!\nThis setting is completely separate from the general equipment procs setting, meaning you can let sawmerang proc while preventing all other equipments from proccing."
            );
            AllowElectricBoomerangProcs = Config.Bind<bool>(
                artifactInstance.ArtifactName,
                "Allow Electric Boomerang to proc", true,
                "Should damage from Electric Boomerang be allowed to proc your on-hit items? WARNING: This will also disable the item's built-in stun on hit!"
            );
            AllowGenericMissileProcs = Config.Bind<bool>(
                artifactInstance.ArtifactName,
                "Allow Missiles to proc", true,
                "Should damage from any missiles that aren't ATG missiles be allowed to proc your on-hit items? WARNING: This counts for Starstorm 2's Armed Backpack alongside Disposable Missile Launcher!"
            );
            AllowFireworkProcs = Config.Bind<bool>(
                artifactInstance.ArtifactName,
                "Allow Fireworks to proc items", true,
                "Should damage from Fireworks be allowed to proc your on-hit items?"
            );
            AllowShurikenProcs = Config.Bind<bool>(
                artifactInstance.ArtifactName,
                "Allow Shurikens to proc items", true,
                "Should damage from Shurikens be allowed to proc your on-hit items?"
            );
            AllowEgoProcs = Config.Bind<bool>(
                artifactInstance.ArtifactName,
                "Allow Egocentrism to proc items", true,
                "Should damage from Egocentrism be allowed to proc your on-hit items?"
            );
            AllowGloopProcs = Config.Bind<bool>(
                artifactInstance.ArtifactName,
                "Allow Genesis Loop to proc items", true,
                "Should damage from Genesis Loop be allowed to proc your on-hit items?"
            );
            AllowAspectProcs = Config.Bind<bool>(
                artifactInstance.ArtifactName,
                "Allow attacks from elite aspects to proc items", true,
                "Should damage from malachite, perfected, gilded, and twisted elites' aspect attacks be allowed to proc your on-hit items? WARNING: Disabling this will also disable those attacks applying an aspect's guranteed debuff such as malachite's healing disable!"
            );
            AllowProcCrits = Config.Bind<bool>(
                artifactInstance.ArtifactName,
                "Allow item procs to crit", true,
                "Should damage from item procs be allowed to crit?"
            );
        }
    }
}
