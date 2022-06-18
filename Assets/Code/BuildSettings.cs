using System;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Stores settings between builds.
/// </summary>
[CreateAssetMenu]
public class BuildSettings : ScriptableObject {
    // === Data

    public string target;

    public string TargetFolder => Path.GetDirectoryName(target);
    public string TargetFile => Path.GetFileName(target);
}
