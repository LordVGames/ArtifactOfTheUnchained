using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
namespace ArtifactOfTheUnchained;


public static class ModProcTypes
{
    public static ModdedProcType ProccedBySkill;
    public static ModdedProcType ProccedByProc;
    public static ModdedProcType ProccedByItem;
    public static ModdedProcType ProccedByEquipment;
    // some items have a proc type added to the chain even though they aren't actual proc items (i.e nkuhanas, razorwire, bands, etc)
    public static readonly ProcType[] FakeProcTypes = [ProcType.HealNova, ProcType.Thorns, ProcType.Rings];


    internal static void SetupProcTypes()
    {
        ProccedBySkill = ProcTypeAPI.ReserveProcType();
        ProccedByItem = ProcTypeAPI.ReserveProcType();
        ProccedByProc = ProcTypeAPI.ReserveProcType();
        ProccedByEquipment = ProcTypeAPI.ReserveProcType();
    }
}