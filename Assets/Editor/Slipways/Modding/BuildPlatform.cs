using UnityEditor;

namespace Slipways.Modding {
    
    /// <summary>
    /// Platforms for which we're building the bundles.
    /// </summary>
    public class BuildPlatform {
        // === Values

        public static readonly BuildPlatform
            Windows = new BuildPlatform(BuildTarget.StandaloneWindows64, "Windows", "win"),
            Mac = new BuildPlatform(BuildTarget.StandaloneOSX, "Mac", "mac");
        
        // === Data
        
        public BuildTarget UnityTarget { get; }
        public string DisplayName { get; }
        public string BundleExtension { get; }

        // === Constructors
        
        BuildPlatform(BuildTarget unityTarget, string displayName, string bundleExtension) {
            UnityTarget = unityTarget;
            DisplayName = displayName;
            BundleExtension = bundleExtension;
        }
    }
}