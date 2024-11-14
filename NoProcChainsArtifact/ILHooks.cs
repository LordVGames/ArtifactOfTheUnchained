using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;

namespace NoProcChainsArtifact
{
    internal static class ILHooks
    {
        internal static void SetupILHooks()
        {
            IL.RoR2.Orbs.GenericDamageOrb.OnArrival += IL_GenericDamageOrb_OnArrival;
            IL.EntityStates.Huntress.HuntressWeapon.ThrowGlaive.FireOrbGlaive += IL_Huntress_ThrowGlaive_FireOrbGlaive;
            IL.EntityStates.Bandit2.StealthMode.FireSmokebomb += IL_Bandit2_StealthMode_FireSmokebomb;
            IL.EntityStates.Toolbot.ToolbotDashImpact.OnEnter += IL_Toolbot_ToolbotDashImpact_OnEnter;
        }

        private static void IL_Huntress_ThrowGlaive_FireOrbGlaive(ILContext il)
        {
            ILCursor c = new(il);
            if (c.TryGotoNext(MoveType.After,
                    x => x.MatchCall<EntityStates.EntityState>("get_gameObject"),
                    x => x.MatchStfld<RoR2.Orbs.LightningOrb>("attacker")
                ))
            {
                c.Emit(OpCodes.Ldloc_0);
                c.Emit(OpCodes.Ldarg_0);
                c.Emit<EntityStates.EntityState>(OpCodes.Call, "get_gameObject");
                c.Emit<RoR2.Orbs.LightningOrb>(OpCodes.Stfld, "inflictor");

                Log.Warning($"cursor is {c}");
                Log.Warning($"il is {il}");
            }
            else
            {
                Log.Error("COULD NOT IL HOOK HUNTRESS FIREORBGLAIVE!");
                Log.Error($"cursor is {c}");
                Log.Error($"il is {il}");
            }
        }

        private static void IL_GenericDamageOrb_OnArrival(ILContext il)
        {
            // this is for huntress' primaries
            // this also affects plasma shrimp but i don't think this hurts anything since it adds itself to the proc mask anyways
            ILCursor c = new(il);
            if (c.TryGotoNext(MoveType.After,
                    x => x.MatchLdloc(1),
                    x => x.MatchLdnull(),
                    x => x.MatchStfld<DamageInfo>("inflictor")
                ))
            {
                // replacing null with attacker to be set as inflictor
                c.Index -= 2;
                c.Remove();
                c.Emit(OpCodes.Ldarg_0);
                c.Emit<RoR2.Orbs.GenericDamageOrb>(OpCodes.Ldfld, "attacker");
            }
            else
            {
                Log.Error("COULD NOT IL HOOK GENERICDAMAGEORB_ONARRIVAL!");
                Log.Error($"cursor is {c}");
                Log.Error($"il is {il}");
            }
        }

        private static void IL_Toolbot_ToolbotDashImpact_OnEnter(ILContext il)
        {
            // the second hit from the dash impact doesn't have an inflictor but the first hit does???
            ILCursor c = new(il);
            if (c.TryGotoNext(MoveType.After,
                    x => x.MatchCall<EntityStates.EntityState>("get_gameObject"),
                    x => x.MatchStfld<DamageInfo>("attacker")
                ))
            {
                c.Emit(OpCodes.Dup);
                c.Emit(OpCodes.Ldarg_0);
                c.Emit<EntityStates.EntityState>(OpCodes.Call, "get_gameObject");
                c.Emit<DamageInfo>(OpCodes.Stfld, "inflictor");
            }
            else
            {
                Log.Error("COULD NOT IL HOOK MUL-T DASH IMPACT!");
                Log.Error($"cursor is {c}");
                Log.Error($"il is {il}");
            }
        }

        private static void IL_Bandit2_StealthMode_FireSmokebomb(ILContext il)
        {
            ILLabel label = null;
            ILCursor c = new(il);
            if (c.TryGotoNext(MoveType.After,
                    x => x.MatchLdarg(0),
                    x => x.MatchCallOrCallvirt<EntityStates.EntityState>("get_isAuthority"),
                    x => x.MatchBrfalse(out label),
                    x => x.MatchNewobj<BlastAttack>()
                ))
            {
                // pretty much the game's IL for setting the blastattack's attacker but for inflictor
                c.Emit(OpCodes.Dup);
                c.Emit(OpCodes.Ldarg_0);
                c.Emit<EntityStates.EntityState>(OpCodes.Call, "get_gameObject");
                c.Emit<BlastAttack>(OpCodes.Stfld, "inflictor");
                return;
            }
            else
            {
                Log.Error("COULD NOT IL HOOK BANDIT SMOKEBOMB!");
                Log.Error($"cursor is {c}");
                Log.Error($"il is {il}");
            }
        }
    }
}