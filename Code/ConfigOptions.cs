using BepInEx.Configuration;
using MiscFixes.Modules;
namespace ArtifactOfTheUnchained;


public static class ConfigOptions
{
    public static class ForModItself
    {
        public enum HookType
        {
            MonoMod = 0,
            MonoDetour
        }

        private const string _sectionName = "For mod itself";
        public static ConfigEntry<bool> EnableDebugLogging;
        public static ConfigEntry<bool> ArtifactlessMode;
        public static ConfigEntry<HookType> HookTypeToUse;
        public static ConfigEntry<string> ItemProcNerfBodyBlacklist;


        internal static void BindConfigOptions(ConfigFile config)
        {
            EnableDebugLogging = config.BindOption<bool>(
                _sectionName,
                "Enable debug logging",
                "",
                false
            );
            ArtifactlessMode = config.BindOption<bool>(
                _sectionName,
                "Run without requiring artifact",
                "Should the effects of the artifact always be on without needing to activate it?",
                false,
                Extensions.ConfigFlags.RestartRequired
            );
            HookTypeToUse = config.BindOption<HookType>(
                _sectionName,
                "Hook Type To Use",
                "Doesn't matter too much, but hooking via MonoDetour may lead to better performance while potentially having less compatibility with other mods (i doubt it but i dont know yet)",
                HookType.MonoMod,
                Extensions.ConfigFlags.RestartRequired
            );
            ItemProcNerfBodyBlacklist = config.BindOption<string>(
                _sectionName,
                "Blacklist for DamageSource-less characters",
                "Modded characters that have not been completely updated for the latest RoR2 version or rely on SeekersPatcher will be missing a \"DamageSource\" on their skills. "
                + "This causes the artifact to see damage from those modded characters as damage from items, thus nerfing any procs from those skills. "
                + "Add a survivor's internal body name (i.e CommandoBody) here, separating each one with a comma and no spaces, to add those survivors to a blacklist excluding them from damage nerfs for procs from items. "
                + "\n\nNOTE: This will make survivors added here stronger than other survivors, due to some of their damage not being nerfed, but it's better than consistently having barely any damage.",
                ""
            );
            ItemProcNerfBodyBlacklist.SettingChanged += ItemProcNerfBodyBlacklist_SettingChanged;
            // SettingChanged doesn't happen on game startup so it's gotta be done manually here
            ItemProcNerfBodyBlacklistArray = ItemProcNerfBodyBlacklist.Value.Split(',');
        }
        internal static void ItemProcNerfBodyBlacklist_SettingChanged(object sender, System.EventArgs e)
        {
            ItemProcNerfBodyBlacklistArray = ItemProcNerfBodyBlacklist.Value.Split(',');
        }
        internal static string[] ItemProcNerfBodyBlacklistArray;
    }

    
    public static class ProcNerfing
    {
        public static class FromProc
        {
            private const string _sectionName = "Procs From Procs";
            public static ConfigEntry<float> ProcChainDamageNerfToPercent;
            public static ConfigEntry<float> ProcChainCoefficientNerfToPercent;
            public static ConfigEntry<int> ProcChainAmountLimit;
            public static ConfigEntry<float> NerfProcDamagePerEthereal;


            internal static void BindConfigOptions(ConfigFile config)
            {
                ProcChainDamageNerfToPercent = config.BindOptionSlider<float>(
                    _sectionName,
                    "Nerf damage",
                    "Should damage from chained procs (i.e. atg procced by a charged perforator hit) be reduced to a percent of what they would normally do? I.E. 0.4 is 40% of the normal damage, 0.04 is 4%, and 1 is vanilla behavior as there is no change.",
                    0.4f
                );
                ProcChainCoefficientNerfToPercent = config.BindOptionSlider<float>(
                    _sectionName,
                    "Nerf proc coefficient",
                    "Should the proc coefficients for chained procs (i.e. polylute from an atg) be reduced to a percent of what they would normally are? I.E. 0.1 is 10% of the normal proc coefficient, and 1 is vanilla behavior as there is no change.",
                    1
                );
                ProcChainAmountLimit = config.BindOptionSlider<int>(
                    _sectionName,
                    "Proc chaining limit",
                    "The limit for how many times on-hit proc items can proc other on-hit proc items. 1 means item procs cannot chain into any other items, i.e. an atg hit can proc your other on-hit proc items, then those procs will not proc anything else. Set to -1 for vanilla behavior.",
                    1
                );
                NerfProcDamagePerEthereal = config.BindOptionSlider<float>(
                    _sectionName,
                    "Nerf proc damage per ethereal level",
                    "(For Starstorm 2 beta content) Nerfs ALL proc damage based on how many ethereal teleporters you've completed. Because this applies to EVERY proc anything does, it will apply multiple times in a proc chain.\nFormula: 1 - (This config's value * # of Ethereal teleporters completed)",
                    0.07f
                );
            }
        }


        public static class FromItem
        {
            private const string _sectionName = "Procs From Items";
            public static ConfigEntry<float> ProcFromItemDamageNerfToPercent;
            public static ConfigEntry<float> ProcFromItemCoefficientNerfToPercent;


            internal static void BindConfigOptions(ConfigFile config)
            {
                ProcFromItemDamageNerfToPercent = config.BindOptionSlider<float>(
                    _sectionName,
                    "Nerf damage",
                    "Should damage for procs from items (i.e. fireworks or royal capacitor) be nerfed to a percent of what it would normally be? I.E. 0.15 is 15% of the normal damage, 0.05 is 5%, and 1 is vanilla behavior as there is no change.",
                    0.15f
                );
                ProcFromItemCoefficientNerfToPercent = config.BindOptionSlider<float>(
                    _sectionName,
                    "Nerf proc coefficient",
                    "Should the proc coefficient for procs from items (i.e. fireworks or royal capacitor) be nerfed to a percent of what it would normally be? I.E. 0.1 is 10% of the normal proc coefficient, and 1 is vanilla behavior as there is no change.",
                    1
                );
            }
        }


        public static class FromEquipment
        {
            private const string _sectionName = "Procs From Equipment";
            public static ConfigEntry<float> ProcFromEquipmentDamageNerfToPercent;
            public static ConfigEntry<float> ProcFromEquipmentCoefficientNerfToPercent;


            internal static void BindConfigOptions(ConfigFile config)
            {
                ProcFromEquipmentDamageNerfToPercent = config.BindOptionSlider<float>(
                    _sectionName,
                    "Nerf damage",
                    "Should damage for procs from equipments (i.e. sawmerang or royal capacitor) be nerfed to a percent of what it would normally be? I.E. 0.2 is 20% of the normal damage, 0.02 is 2%, and 1 is vanilla behavior as there is no change.",
                    0.25f
                );
                ProcFromEquipmentCoefficientNerfToPercent = config.BindOptionSlider<float>(
                    _sectionName,
                    "Nerf proc coefficient",
                    "Should the proc coefficient for procs from equipments (i.e. sawmerang or royal capacitor) be nerfed to a percent of what it would normally be? I.E. 0.1 is 10% of the normal proc coefficient, and 1 is vanilla behavior as there is no change.",
                    1
                );
            }
        }




        private const string _sectionName = "General Proc Chain Nerfs";
        public static ConfigEntry<bool> PreventAllItemChaining;


        internal static void BindAllConfigOptions(ConfigFile config)
        {
            PreventAllItemChaining = config.BindOption<bool>(
                _sectionName,
                "Prevent ALL items from proccing other items",
                "Should not just procs from other procs but item procs from ANY item be blocked? This means that ONLY damage from skills can proc your items.",
                false
            );
            FromProc.BindConfigOptions(config);
            FromItem.BindConfigOptions(config);
            FromEquipment.BindConfigOptions(config);
        }
    }




    internal static void BindAllConfigOptions(ConfigFile config)
    {
        ForModItself.BindConfigOptions(config);
        ProcNerfing.BindAllConfigOptions(config);
    }
}