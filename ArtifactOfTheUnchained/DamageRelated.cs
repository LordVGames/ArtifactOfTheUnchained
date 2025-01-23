using RoR2;
using R2API;

namespace ArtifactOfTheUnchainedMod
{
    internal static class DamageRelated
    {
        public static ModdedProcType ProccedByItem;

        internal static void CharacterMaster_OnBodyDamaged(DamageReport damageReport)
        {
            // ty InfiniteProcChains mod for giving me a starting reference on how to do this stuff
#if DEBUG
            LogDamageInfo(damageReport.damageInfo);
#endif
            if (!IsDamageReportSafeToModify(damageReport))
            {
                return;
            }


            // these need to apply to both player and monster damage
            PreventProcChainingIfPastLimit(damageReport);
            // giga brain move: using a custom proc type in the chain in combination with damagesources to check if an item was procced by another item
            if (ProcTypeAPI.HasModdedProc(damageReport.damageInfo.procChainMask, ProccedByItem))
            {
                NerfDamageReportBasedOnConfig(damageReport);
                return;
            }

            if (damageReport.attackerMaster.playerCharacterMasterController)
            {
                // player damage not from a skill? must be from an item
                // we mark it so any further procs will have this mark that we check for above
                ProcTypeAPI.AddModdedProc(ref damageReport.damageInfo.procChainMask, ProccedByItem);
                return;
            }
            else
            {
                // monster damage needs to be handled separately because all monster skills (except ss2 ones) don't use damagesources
                HandleMonsterDamage(damageReport);
            }

            return;
        }

        private static void HandleMonsterDamage(DamageReport damageReport)
        {
            // this assumes proc chain limit has already been handled
            // and also that this damage isn't from a skill but every monster's skill counts as no skill
            // i hate this

            if (!damageReport.damageInfo.inflictor && damageReport.damageInfo.procChainMask.mask == 0)
            {
                // damage without an inflictor that has no mask? must be an item, we're safe to mark
                ProcTypeAPI.AddModdedProc(ref damageReport.damageInfo.procChainMask, ProccedByItem);
            }
            else if (damageReport.damageInfo.procChainMask.mask > 0)
            {
                if (GetVanillaProcCountInChain(damageReport.damageInfo.procChainMask) > 1)
                {
                    NerfDamageReportBasedOnConfig(damageReport);
                    return;
                }
            }
            else if (IsInflictorSpecificItem(damageReport.damageInfo.inflictor))
            {
                ProcTypeAPI.AddModdedProc(ref damageReport.damageInfo.procChainMask, ProccedByItem);
            }
        }



        internal static bool IsDamageReportSafeToModify(DamageReport damageReport)
        {
            // void fog
            if (!damageReport.attacker)
            {
                return false;
            }
            // void cradles (but only for clients???)
            if (!damageReport.attackerMaster)
            {
                return false;
            }
            // need to let ss2's nemesis commando gouge still proc things
            if (damageReport.damageInfo.damageType.damageType.HasFlag(DamageType.DoT))
            {
                return false;
            }
            if (IsDamageBrokenWithoutInflictor(damageReport.damageInfo))
            {
                return false;
            }
            // this needs to apply to both players and monsters
            else if (!ShouldAllowNormalConfigurableItemProcs(damageReport.damageInfo))
            {
                if (!ConfigOptions.AllowProcCrits.Value)
                {
                    damageReport.damageInfo.crit = false;
                }
                damageReport.damageInfo.procCoefficient *= ConfigOptions.ProcChainCoefficientNerfToPercent.Value;
                return false;
            }
            // leave player attacks alone
            // and monsters if they supported damagesources but they don't
            // TODO would be horrible but IL hooking to fix all monster skills???
            if (damageReport.damageInfo.damageType.IsDamageSourceSkillBased)
            {
                return false;
            }
            // i hate warbonds
            if (damageReport.damageInfo.inflictor)
            {
                if (damageReport.damageInfo.inflictor.name == "BossMissileProjectile(Clone)")
                {
                    damageReport.damageInfo.procCoefficient = 0;
                    return false;
                }
            }

            return true;
        }

        internal static bool IsDamageBrokenWithoutInflictor(DamageInfo damageInfo)
        {
            // glacial death bubble won't freeze with with 0 coefficient
            if (damageInfo.damageType.damageTypeCombined == 131328)
            {
                return true;
            }
            /*
             * blood shrines deal no damaage when set with 0 proc coefficient for some reason
             * sulfur pods also break since they don't debuff you with no proc coefficient
             * (yet aqueducts tar pots still tar you without coefficient)
            */
            switch (damageInfo.attacker.name)
            {
                case "SulfurPodBody(Clone)":
                case "ShrineBlood(Clone)":
                    return true;
            }
            return false;
        }

        /// <remarks>
        /// Even though it says "item" procs it's also for a lot of equipments
        /// </remarks>
        internal static bool ShouldAllowNormalConfigurableItemProcs(DamageInfo damageInfo)
        {
            if (!damageInfo.inflictor)
            {
                return true;
            }

            bool shouldAllowItemProcs = true;
            switch (damageInfo.inflictor.name)
            {
                #region items
                // electric boomerang needs to have a proc coefficient to make it's stun work
                case "StunAndPierceBoomerang(Clone)":
                    if (!ConfigOptions.AllowElectricBoomerangProcs.Value)
                    {
                        shouldAllowItemProcs = false;
                    }
                    break;
                case "FireworkProjectile(Clone)":
                    if (!ConfigOptions.AllowFireworkProcs.Value)
                    {
                        shouldAllowItemProcs = false;
                    }
                    break;
                case "ShurikenProjectile(Clone)":
                    if (!ConfigOptions.AllowShurikenProcs.Value)
                    {
                        shouldAllowItemProcs = false;
                    }
                    break;
                case "LunarSunProjectile(Clone)":
                    if (!ConfigOptions.AllowEgoProcs.Value)
                    {
                        shouldAllowItemProcs = false;
                    }
                    break;
                case "VagrantNovaItemBodyAttachment(Clone)":
                    if (!ConfigOptions.AllowGloopProcs.Value)
                    {
                        shouldAllowItemProcs = false;
                    }
                    break;
                #endregion

                #region equipments
                case "GoldGatController(Clone)":
                case "BeamSphere(Clone)":
                case "MeteorStorm(Clone)":
                case "VendingMachineProjectile(Clone)":
                case "FireballVehicle(Clone)":
                    if (!ConfigOptions.AllowEquipmentProcs.Value)
                    {
                        shouldAllowItemProcs = false;
                    }
                    break;
                /*
                 * i would give sawmerang a really low proc coeff but bleed duration scales off of that too
                 * so even though it's guranteed to apply it does one tick of damage then all bleed stacks goes away
                 * its either all procs or no procs
                 * this also applies to electric boomerang's stun
                 * thanks hopoo /s
                 */
                case "Sawmerang(Clone)":
                    if (!ConfigOptions.AllowSawmerangProcs.Value)
                    {
                        shouldAllowItemProcs = false;
                    }
                    break;
                // this is meant for disposable missile launcher but atg and starstorm 2's armed backpack also trigger this
                // atg can be checked for with the procchainmask but armed backpack can't check for it specifically
                case "MissileProjectile(Clone)":
                    if (damageInfo.procChainMask.mask == 0 && !ConfigOptions.AllowGenericMissileProcs.Value)
                    {
                        shouldAllowItemProcs = false;
                    }
                    break;

                #region aspects
                case "LunarMissileProjectile(Clone)":
                case "PoisonOrbProjectile(Clone)":
                case "PoisonStakeProjectile(Clone)":
                case "AffixAurelionitePreStrikeProjectile(Clone)":
                case "AffixAurelioniteCenterProjectile(Clone)":
                case "BeadProjectileTrackingBomb(Clone)":
                    if (!ConfigOptions.AllowAspectProcs.Value)
                    {
                        // if proc coefficient is 0 it makes malachite debuff & perfected cripple not apply
                        shouldAllowItemProcs = false;
                    }
                    break;
                    #endregion
                    #endregion
            }
            return shouldAllowItemProcs;
        }



        internal static void NerfDamageReportBasedOnConfig(DamageReport damageReport)
        {
            if (!ConfigOptions.AllowProcCrits.Value)
            {
                damageReport.damageInfo.crit = false;
            }
            //Log.Error($"damageReport.damageDealt was {damageReport.damageDealt}");
            damageReport.damageDealt *= ConfigOptions.ProcChainDamageNerfToPercent.Value;
            //Log.Error($"damageReport.damageDealt is now {damageReport.damageDealt}");
            damageReport.damageInfo.procCoefficient *= ConfigOptions.ProcChainCoefficientNerfToPercent.Value;
        }

        internal static int GetVanillaProcCountInChain(ProcChainMask procChainMask)
        {
            int numberOfProcsInChain = 0;
            // i don't really know bitwise stuff yet so this probably isn't that good performance-wise
            // but then again this is also kinda based on the game's procchainmask AppendToStringBuilder code
            for (ProcType procType = 0; procType < ProcType.Count; procType += 1U)
            {
                if (procChainMask.HasProc(procType))
                {
                    numberOfProcsInChain++;
                }
            }
            return numberOfProcsInChain;
        }

        internal static bool IsProcChainPastLimit(ProcChainMask procChainMask)
        {
            // != -1 is for those who configured for vanilla behavior
            return ConfigOptions.ProcChainAmountLimit.Value != -1 && GetVanillaProcCountInChain(procChainMask) > ConfigOptions.ProcChainAmountLimit.Value;
        }

        internal static void PreventProcChainingIfPastLimit(DamageReport damageReport)
        {
            if (damageReport.damageInfo.procChainMask.mask > 0)
            {
                if (IsProcChainPastLimit(damageReport.damageInfo.procChainMask))
                {
                    damageReport.damageInfo.procCoefficient = 0;
                }
            }
        }

        internal static bool IsInflictorSpecificItem(UnityEngine.GameObject inflictor)
        {
            switch (inflictor.name)
            {
                case "IcicleAura(Clone)":
                case "DaggerProjectile(Clone)":
                case "RunicMeteorStrikeImpact(Clone)":
                // ss2
                case "LampBulletPlayer(Clone)":
                // enemiesreturns
                case "IfritPylonPlayerBody(Clone)":
                // sivscontent
                case "MushroomWard(Clone)":
                case "ThunderAuraStrike(Clone)":
                case "LunarShockwave(Clone)":
                    return true;
            }
            return false;
        }



        internal static void LogDamageInfo(DamageInfo damageInfo)
        {
            Log.Debug("");
            Log.Debug("");
            Log.Debug("");
            Log.Debug($"damageInfo.damageType.damageSource is {damageInfo.damageType.damageSource}");
            Log.Debug($"damageInfo.damageType.IsDamageSourceSkillBased is {damageInfo.damageType.IsDamageSourceSkillBased}");
            Log.Debug($"attacker is {damageInfo.attacker}");
            Log.Debug($"canRejectForce is {damageInfo.canRejectForce}");
            Log.Debug($"crit is {damageInfo.crit}");
            Log.Debug($"damage is {damageInfo.damage}");
            Log.Debug($"damageColorIndex is {damageInfo.damageColorIndex}");
            Log.Debug($"damageType is {damageInfo.damageType}");
            Log.Debug($"damageType.damageType is {damageInfo.damageType.damageType}");
            Log.Debug($"damageType.damageTypeCombined is {damageInfo.damageType.damageTypeCombined}");
            Log.Debug($"damageType.damageTypeExtended is {damageInfo.damageType.damageTypeExtended}");

            Log.Debug($"dotIndex is {damageInfo.dotIndex}");
            Log.Debug($"force is {damageInfo.force}");
            Log.Debug($"inflictor is {damageInfo.inflictor}");
            Log.Debug($"position is {damageInfo.position}");
            Log.Debug($"procChainMask is {damageInfo.procChainMask}");
            Log.Debug($"procChainMask.mask is {damageInfo.procChainMask.mask}");
            Log.Debug($"procCoefficient is {damageInfo.procCoefficient}");
            Log.Debug($"rejected is {damageInfo.rejected}");
        }
    }
}
