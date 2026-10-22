using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using RoR2;
using SS2;
using HarmonyLib;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;
using R2API;
using MonoDetour.HookGen;
using SS2.Items;
using MonoDetour;
using MonoDetour.Cil;
[assembly: MonoDetourTargets(typeof(RelicOfForce.ForceHitToken))]
namespace ArtifactOfTheUnchained;


internal static class ModSupport
{
    internal static class Starstorm2
    {
        private static bool? _modexists;
        internal static bool ModIsRunning
        {
            get
            {
                _modexists ??= BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(SS2Main.GUID);
                return (bool)_modexists;
            }
        }


        internal static bool IsBetaEnabled
        {
            [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
            get
            {
                return ModIsRunning && SS2Config.enableBeta;
            }
        }


        internal static int EtherealsCount
        {
            [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
            get
            {
                return (ModIsRunning && IsBetaEnabled) ? EtherealBehavior.instance.etherealsCompleted : 0;
            }
        }


        internal static class RelicOfForceDamageSourceFix
        {
            internal static void Setup()
            {
                if (!ModIsRunning)
                {
                    return;
                }


                Mdh.SS2.Items.RelicOfForce.ForceHitToken.RelicForceDelayedHits.ILHookMoveNext(RemoveDamageSource);
            }


            [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
            private static void RemoveDamageSource(ILManipulationInfo info)
            {
                ILWeaver w = new(info);
                w.MatchRelaxed(
                    x => x.MatchCallOrCallvirt<HealthComponent>("TakeDamage") && w.SetCurrentTo(x)
                ).ThrowIfFailure()
                .InsertBeforeCurrent(
                    w.CreateDelegateCall((DamageInfo damageInfo) =>
                    {
                        damageInfo.damageType.damageSource = DamageSource.NoneSpecified;
                        return damageInfo;
                    })
                );
            }
        }
    }
}