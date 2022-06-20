using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace Slipways.Modding
{
    /// <summary>
    /// Offers options and handles the process of exporting a mod bundle for Slipways.
    /// </summary>
    public class ExportBundleWindow : EditorWindow
    {
        // === Constants

        public const string SourceFolder = "Assets/Bundle/Contents";
        public const string BundleName = "bundle.unity3d";
        
        // === Data

        BundleExport _export;
        BuildSettings _settings;

        List<string> _foundFilesInfo;

        GUIStyle _richLabel;
        
        // === Menu items

        [MenuItem("Slipways/Export bundle...")]
        public static void Open()
        {
            ExportBundleWindow window = (ExportBundleWindow)GetWindow(typeof(ExportBundleWindow));
            window.Initialize();
            window.ShowModal();
        }

        [MenuItem("Slipways/Quick export with same settings")]
        public static void QuickExport() {
            ExportBundleWindow window = (ExportBundleWindow)GetWindow(typeof(ExportBundleWindow));
            window.Initialize();
            window.Close();
            try {
                window._export.PerformBuild(window._settings);
                ShowDone();
            } catch (BuildFailedException) {
                // swallow, Unity already logged it
            }
        }
        
        // === Initialization

        void Initialize()
        {
            _richLabel = new GUIStyle(EditorStyles.label) {richText = true};
            // window stuff
            minSize = new Vector2(400, 500);
            titleContent = new GUIContent("Bundle Export");
            // load settings
            _settings = Resources.Load<BuildSettings>("BuildSettings");
            // start the export
            _export = new BundleExport(new BundleExport.Settings {
                SourceFolder = SourceFolder
            });
            _export.PrepareForBuild();
            _foundFilesInfo = _export.DescribeAssets().ToList();
        }
        
        // === GUI
        
        public void OnGUI() {
            if (_settings == null) return;
            EditorGUILayout.LabelField("1. Make sure all your assets are in Assets/Bundle/Contents.", EditorStyles.boldLabel);
            if (_foundFilesInfo.Count > 0) {
                EditorGUILayout.LabelField("Found the following assets:");
                string text = string.Join("\n", _foundFilesInfo);
                EditorGUILayout.HelpBox(text.Trim(), MessageType.Info);
            } else {
                EditorGUILayout.HelpBox("No Slipways-compatible assets found.", MessageType.Warning);
            }
            EditorGUILayout.Separator();
            // ---
            EditorGUILayout.LabelField("2. Choose where to save the bundle:", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(_settings.target, EditorStyles.whiteLabel);
            if (GUILayout.Button("Change", GUILayout.Width(100))) {
                PickNewDestination();
            }
            GUILayout.EndHorizontal();
            EditorGUILayout.LabelField("Extensions will be automatically added for different versions of the bundle.");
            EditorGUILayout.Separator();
            // ---
            EditorGUILayout.LabelField("3. Click the button below to export your bundle.", EditorStyles.boldLabel);
            GUI.enabled = _settings.target != ""; 
            if (GUILayout.Button("Export!")) {
                Build();
            }
            GUI.enabled = true;
        }
        
        // === Operations

        void PickNewDestination() {
            string dir = null, file = null;
            try {
                if (!string.IsNullOrEmpty(_settings.target)) {
                    dir = Path.GetDirectoryName(_settings.target);
                }
            } finally {
                dir ??= "";
            }
            try {
                if (!string.IsNullOrEmpty(_settings.target)) {
                    file = Path.GetFileName(_settings.target);
                }
            } finally {
                file ??= "bundle";
            }
            string newTarget = EditorUtility.SaveFilePanel("Choose where to save the bundle files", dir, file, "");
            if (!string.IsNullOrEmpty(newTarget)) {
                Regex chopExtensions = new Regex(@"\..*$");
                newTarget = chopExtensions.Replace(newTarget, "");
                _settings.target = newTarget;
            }
        }
        
        void Build() {
            EditorUtility.SetDirty(_settings);
            AssetDatabase.SaveAssetIfDirty(_settings);
            try {
                _export.PerformBuild(_settings);
                ShowDone();
            } catch (BuildFailedException) {
                // swallow, Unity already logged it
            } finally {
                Close();
            }
        }

        static void ShowDone() {
            EditorUtility.DisplayDialog("Export complete", "Your bundle exported successfully.", "Okay!");
        }
    }
}