using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Slipways.General.Modding.API;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

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
        }
        
        public Settings CurrentSettings { get; }
        
        // === Data

        List<string> _scannedAssets;
        string _bundleName;
        
        // === Constructors

        public BundleExport(Settings settings) {
            CurrentSettings = settings;
        }
        
        // === External API

        public void PrepareForBuild() {
            _scannedAssets = GetRelevantAssets().ToList();
            _bundleName = Guid.NewGuid().ToString().Substring(24) + ".unity3d";
        }

        public void PerformBuild(BuildSettings settings) {
            //AssignAssetsToBundle(_scannedAssets, _bundleName);
            BuildForPlatform(settings, BuildPlatform.Windows);
            BuildForPlatform(settings, BuildPlatform.Mac);
        }

        public void BuildForPlatform(BuildSettings settings, BuildPlatform platform) {
            string outputFolder = settings.TargetFolder;
            string builtName = _bundleName;
            string targetName = $"{settings.TargetFile}.{platform.BundleExtension}";
            AssetBundleBuild abb = new AssetBundleBuild {
                assetBundleName = _bundleName,
                assetNames = _scannedAssets.Select(sa => "Assets/" + sa).ToArray()
            };
            AssetBundleManifest result = BuildPipeline.BuildAssetBundles(outputFolder,
                new[] {abb},
                BuildAssetBundleOptions.None,
                platform.UnityTarget);
            if (result != null) {
                RenameAndDeleteGarbage(outputFolder, builtName, targetName);
            } else {
                throw new BuildFailedException("Failed to create the bundle.");
            }
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

        public IEnumerable<string> GetIrrelevantAssets() {
            Queue<string> dirs = new Queue<string>();
            dirs.Enqueue("Assets");
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