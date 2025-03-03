using BepInEx.Configuration;

namespace ArtifactOfTheUnchainedMod
{
    public static class ConfigOptions
    {
        public static ConfigEntry<bool> ArtifactlessMode;
        public static ConfigEntry<string> ItemProcNerfBodyBlacklist;
        internal static void ItemProcNerfBodyBlacklist_SettingChanged(object sender, System.EventArgs e)
        {
            ItemProcNerfBodyBlacklistArray = ItemProcNerfBodyBlacklist.Value.Split(',');
        }
        internal static string[] ItemProcNerfBodyBlacklistArray;

        public static ConfigEntry<float> ProcFromItemDamageNerfToPercent;
        public static ConfigEntry<float> ProcFromItemCoefficientNerfToPercent;

        public static ConfigEntry<int> ProcChainAmountLimit;
        public static ConfigEntry<float> ProcChainDamageNerfToPercent;
        public static ConfigEntry<float> ProcChainCoefficientNerfToPercent;

        public static ConfigEntry<bool> PreventAllItemChaining;

        public static void BindConfigEntries(ConfigFile Config, ArtifactBase artifactInstance)
        {
            ArtifactlessMode = Config.Bind<bool>(
                artifactInstance.ArtifactName,
                "Run without requiring artifact", false,
                "Should the effects of the artifact always be on without needing to activate it?"
            );
            ItemProcNerfBodyBlacklist = Config.Bind<string>(
                artifactInstance.ArtifactName,
                "Blacklist for DamageSource-less characters", "",
                "Modded characters that have not been completely updated for the latest RoR2 version or rely on SeekersPatcher will be missing a \"DamageSource\" on their skills. "
                + "This causes the artifact to see damage from those modded characters as damage from items, thus nerfing any procs from those skills. "
                + "Add a survivor's internal body name (i.e CommandoBody) here, separating each one with a comma and no spaces, to add those survivors to a blacklist excluding them from damage nerfs for procs from items. "
                + "\n\nNOTE: This will make survivors added here stronger than other survivors, due to some of their damage not being nerfed, but it's better than consistently having barely any damage."
            );
            ItemProcNerfBodyBlacklist.SettingChanged += ItemProcNerfBodyBlacklist_SettingChanged;
            // SettingChanged doesn't happen on game startup so it's gotta be done manually here
            ItemProcNerfBodyBlacklistArray = ItemProcNerfBodyBlacklist.Value.Split(',');


            ProcFromItemDamageNerfToPercent = Config.Bind<float>(
                artifactInstance.ArtifactName,
                "Nerf damage for procs from items", 0.2f,
                "Should damage for procs caused by items (i.e. fireworks or royal capacitor) be nerfed to a percent of what it would normally be? I.E. 0.2 is 20% of the normal damage, 0.02 is 2%, and 1 is vanilla behavior as there is no change."
            );
            ProcFromItemCoefficientNerfToPercent = Config.Bind<float>(
                artifactInstance.ArtifactName,
                "Nerf proc coefficient for procs from items", 1,
                "Should the proc coefficient for procs caused by items (i.e. fireworks or royal capacitor) be nerfed to a percent of what it would normally be? I.E. 0.1 is 20% of the normal proc coefficient, and 1 is vanilla behavior as there is no change."
            );


            ProcChainAmountLimit = Config.Bind<int>(
                artifactInstance.ArtifactName,
                "Proc chaining limit", 1,
                "The limit for how many times on-hit proc items can proc other on-hit proc items. 1 means item procs cannot chain into any other items, i.e. an atg hit can proc your other on-hit proc items, then those procs will not proc anything else. Set to -1 for vanilla behavior."
            );
            ProcChainDamageNerfToPercent = Config.Bind<float>(
                artifactInstance.ArtifactName,
                "Nerf chained procs damage", 0.05f,
                "Should damage from chained procs (i.e. atg procced by a charged perforator hit) be reduced to a percent of what they would normally do? I.E. 0.2 is 20% of the normal damage, 0.02 is 2%, and 1 is vanilla behavior as there is no change."
            );
            ProcChainCoefficientNerfToPercent = Config.Bind<float>(
                artifactInstance.ArtifactName,
                "Nerf chained procs proc coefficient", 1,
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
