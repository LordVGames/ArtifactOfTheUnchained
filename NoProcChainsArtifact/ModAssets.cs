using System.IO;
using UnityEngine;

namespace NoProcChainsArtifact
{
    public static class ModAssets
    {
        public static AssetBundle AssetBundle;
        public const string BundleName = "unchainedartifacticons";

        public static string AssetBundlePath
        {
            get
            {
                return Path.Combine(Path.GetDirectoryName(NoProcChainsArtifact.PluginInfo.Location), BundleName);
            }
        }

        public static void Init()
        {
            AssetBundle = AssetBundle.LoadFromFile(AssetBundlePath);
        }
    }
}
