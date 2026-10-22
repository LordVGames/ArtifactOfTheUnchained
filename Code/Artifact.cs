using RoR2;
using R2API;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static RoR2.Skills.SkillFamily;
namespace ArtifactOfTheUnchained;


public static class Artifact
{
    public static ArtifactDef Unchained = ScriptableObject.CreateInstance<ArtifactDef>();


    internal static void SetupArtifact()
    {
        Unchained.nameToken = "ARTIFACT_UNCHAINED_NAME";
        Unchained.descriptionToken = "ARTIFACT_UNCHAINED_DESC";
        Unchained.cachedName = "Artifact of the Unchained";
#pragma warning disable CS0618 // Type or member is obsolete
        Unchained.smallIconSelectedSprite = Assets.AssetBundle.LoadAsset<Sprite>("NoProcChainsArtifactIcon_Enabled.png");
        Unchained.smallIconDeselectedSprite = Assets.AssetBundle.LoadAsset<Sprite>("NoProcChainsArtifactIcon_Disabled.png");
#pragma warning restore CS0618 // Type or member is obsolete
        ContentAddition.AddArtifactDef(Unchained);
    }
}