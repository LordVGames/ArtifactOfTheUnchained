using System.IO;
using UnityEngine;

namespace ArtifactOfTheUnchainedMod
{
    public static class Assets
    {
        public static AssetBundle AssetBundle;
        public const string BundleName = "unchainedartifacticons";

        public static string AssetBundlePath
        {
            get
            {
                return Path.Combine(Path.GetDirectoryName(Main.PluginInfo.Location), BundleName);
            }
        }

        public static void Init()
        {
            AssetBundle = AssetBundle.LoadFromFile(AssetBundlePath);
        }
    }
}