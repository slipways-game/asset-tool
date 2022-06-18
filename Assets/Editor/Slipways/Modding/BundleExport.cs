using System.Collections.Generic;
using System.IO;
using System.Linq;
using Slipways.General.Modding.API;
using UnityEditor;

namespace Slipways.Modding
{
    /// <summary>
    /// Actual implementation of the bundle export process.
    /// </summary>
    public class BundleExport
    {
        // === Settings

        public class Settings {
            public string SourceFolder { get; set; }
            public string BundleName { get; set; }
        }
        
        public Settings CurrentSettings { get; }
        
        // === Data

        List<string> _scannedAssets;
        
        // === Constructors

        public BundleExport(Settings settings) {
            CurrentSettings = settings;
        }
        
        // === External API

        public void PrepareForBuild() {
            _scannedAssets = GetRelevantAssets().ToList();
            AssignAssetsToBundle(_scannedAssets, CurrentSettings.BundleName);
        }

        public void PerformBuild(BuildSettings settings) {
            BuildForPlatform(settings, BuildPlatform.Windows);
            BuildForPlatform(settings, BuildPlatform.Mac);
        }

        public void BuildForPlatform(BuildSettings settings, BuildPlatform platform) {
            string outputFolder = settings.TargetFolder;
            string builtName = CurrentSettings.BundleName;
            string targetName = $"{settings.TargetFile}.{platform.BundleExtension}";
            BuildPipeline.BuildAssetBundles(outputFolder, 
                BuildAssetBundleOptions.None,
                platform.UnityTarget);
            RenameAndDeleteGarbage(outputFolder, builtName, targetName);
        }

        // === Describing the build
        
        public IEnumerable<string> DescribeAssets() {
            var groups = _scannedAssets.GroupBy(SlipwaysAssetName);
            List<string> descriptions = new List<string>();
            foreach (var grp in groups) {
                if (grp.Key == null) continue;
                descriptions.Add($"{grp.Key} ({grp.Count()} item(s))");
            }
            descriptions.Sort();
            return descriptions;
        }

        public string SlipwaysAssetName(string path) {
            string filename = Path.GetFileName(path);
            if (string.IsNullOrEmpty(filename)) return null;
            var rak = RecognizedAssetKind.All.FirstOrDefault(rak => filename.StartsWith(rak.FilePrefix));
            return rak?.DisplayName;
        }
        
        // === Internals

        public IEnumerable<string> GetRelevantAssets()
        {
            Queue<string> dirs = new Queue<string>();
            dirs.Enqueue(CurrentSettings.SourceFolder);
            string fullDir = Path.GetFullPath("Assets").Replace("\\", "/");
            while (dirs.Count > 0) {
                string current = dirs.Dequeue();
                foreach (var subDir in Directory.GetDirectories(current)) {
                    dirs.Enqueue(subDir);
                }
                foreach (var file in Directory.GetFiles(current)) {
                    if (file.EndsWith(".meta")) continue;
                    string fullPath = Path.GetFullPath(file).Replace("\\", "/");
                    yield return fullPath.Substring(fullDir.Length + 1);
                }
            }
        }

        public void AssignAssetsToBundle(IEnumerable<string> paths, string bundleName) {
            foreach (var path in paths) {
                AssignAssetToBundle(path, bundleName);
            }
        }
        public void AssignAssetToBundle(string assetPath, string bundleName) {
            var importer = AssetImporter.GetAtPath("Assets/" + assetPath);
            importer.assetBundleName = bundleName;
        }

        public void RenameAndDeleteGarbage(string targetFolder, string builtName, string targetName) {
            string fullBuilt = Path.Combine(targetFolder, builtName);
            string fullTarget = Path.Combine(targetFolder, targetName);
            // copy to final name
            File.Copy(fullBuilt, fullTarget, true);
            // delete leftovers
            List<string> manifestFiles = Directory.GetFiles(targetFolder)
                .Where(file => file.EndsWith(".manifest"))
                .ToList();
            foreach (string manifestFile in manifestFiles) {
                string correspondingFile = manifestFile.Replace(".manifest", "");
                File.Delete(manifestFile);
                File.Delete(correspondingFile);
            }
        }
    }
}