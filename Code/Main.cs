using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;
using RoR2;
using UnityEngine;

namespace ArtifactOfTheUnchainedMod
{
    public static class Main
    {
        internal static class NerfLoggingMessages
        {
            internal static string ProcNotFromSkillBlocked;
            internal static string ProcBlockedForZeroLimit;

            internal static string ProcChainDamageNerf;
            internal static string ProcChainCoefficientNerf;
            internal static string ProcChainBlocked;

            internal static string ProcFromItemDamageNerf;
            internal static string ProcFromItemCoefficientNerf;

            internal static string ProcFromEquipmentDamageNerf;
            internal static string ProcFromEquipmentCoefficientNerf;
        }

        public static List<ArtifactBase> Artifacts = [];



        public static void HealthComponent_TakeDamage_Artifactless(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            DamageRelated.DebugLogDamageInfo(damageInfo, true);
            DamageRelated.HealthComponent_TakeDamage(damageInfo);
            orig(self, damageInfo);
            DamageRelated.DebugLogDamageInfo(damageInfo, false);
            return;
        }



        internal static void SetupLanguageSpecificStrings()
        {
            NerfLoggingMessages.ProcNotFromSkillBlocked = Language.GetStringFormatted("ARTIFACT_UNCHAINED_NO_ITEM_PROC");
            NerfLoggingMessages.ProcBlockedForZeroLimit = Language.GetStringFormatted("ARTIFACT_UNCHAINED_");

            NerfLoggingMessages.ProcChainDamageNerf = Language.GetStringFormatted("ARTIFACT_UNCHAINED_PROC_CHAIN_DAMAGE_NERF");
            NerfLoggingMessages.ProcChainCoefficientNerf = Language.GetStringFormatted("ARTIFACT_UNCHAINED_PROC_CHAIN_COEFFICIENT_NERF");
            NerfLoggingMessages.ProcChainBlocked = Language.GetStringFormatted("ARTIFACT_UNCHAINED_PROC_CHAIN_BLOCKED");

            NerfLoggingMessages.ProcFromItemCoefficientNerf = Language.GetStringFormatted("ARTIFACT_UNCHAINED_ITEM_PROC_COEFFICIENT_NERF");
            NerfLoggingMessages.ProcFromItemDamageNerf = Language.GetStringFormatted("ARTIFACT_UNCHAINED_ITEM_PROC_DAMAGE_NERF");

            NerfLoggingMessages.ProcFromEquipmentCoefficientNerf = Language.GetStringFormatted("ARTIFACT_UNCHAINED_EQUIPMENT_PROC_DAMAGE_NERF");
            NerfLoggingMessages.ProcFromEquipmentDamageNerf = Language.GetStringFormatted("ARTIFACT_UNCHAINED_EQUIPMENT_PROC_COEFFICIENT_NERF");
        }

        internal static bool IsAttackerBlacklisted(GameObject attackerGameObject)
        {
            if (ConfigOptions.ItemProcNerfBodyBlacklist.Value.IsNullOrWhiteSpace())
            {
                return false;
            }
            // idk if this check is needed but as they say "just nullcheck shit man"
            if (attackerGameObject.name.IsNullOrWhiteSpace())
            {
                return false;
            }

            // i don't like using Substring & Contains here but afaik there's no better way
            string attackerBodyName = attackerGameObject.name.Substring(0, attackerGameObject.name.Length - 7);
            if (ConfigOptions.ItemProcNerfBodyBlacklistArray.Contains(attackerBodyName))
            {
                return true;
            }
            return false;
        }
    }
}