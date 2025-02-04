using BepInEx.Configuration;

namespace ArtifactOfTheUnchainedMod
{
    public static class ConfigOptions
    {
        public static ConfigEntry<bool> ArtifactlessMdde;

        public static ConfigEntry<int> ProcChainAmountLimit;
        public static ConfigEntry<float> ProcChainDamageNerfToPercent;
        public static ConfigEntry<float> ProcChainCoefficientNerfToPercent;

        public static ConfigEntry<bool> PreventAllItemChaining;

        public static void BindConfigEntries(ConfigFile Config, ArtifactBase artifactInstance)
        {
            ArtifactlessMdde = Config.Bind<bool>(
                artifactInstance.ArtifactName,
                "Run without requiring artifact", false,
                "Should the effects of the artifact always be on without needing to activate it?"
            );

            ProcChainAmountLimit = Config.Bind<int>(
                artifactInstance.ArtifactName,
                "Proc chaining limit", 0,
                "The limit for how many times items can proc each other. 0 means item procs cannot chain into any other items, i.e. an atg hit will never chain into anytihng else. Set to -1 for vanilla behavior."
            );
            ProcChainDamageNerfToPercent = Config.Bind<float>(
                artifactInstance.ArtifactName,
                "Nerf chained procs damage", 0.025f,
                "Should damage from chained procs (i.e. polylute from an atg) be reduced to a percent of what they would normally do? I.E. 0.2 is 20% of the normal damage, 0.02 is 2%, and 1 is vanilla behavior as there is no change."
            );
            ProcChainCoefficientNerfToPercent = Config.Bind<float>(
                artifactInstance.ArtifactName,
                "Nerf chained procs coefficient", 1,
                "Should the proc coefficients for chained procs (i.e. polylute from an atg) be reduced to a percent of what they would normally are? I.E. 0.1 is 10% of the normal proc coefficient, and 1 is vanilla behavior as there is no change."
            );

            PreventAllItemChaining = Config.Bind<bool>(
                artifactInstance.ArtifactName,
                "Prevent ALL items from proccing other items", false,
                "Should not just procs from other procs but item procs from ANY item be blocked? This means that ONLY damage from skills can proc your items."
            );
        }
    }
}
