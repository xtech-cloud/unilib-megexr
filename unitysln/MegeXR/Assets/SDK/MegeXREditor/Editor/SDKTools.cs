using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public static class SDKTools
{
    private static string srcPath = Path.Combine(Path.GetFullPath("."), "../../sdk");
    private static string destPath = Path.Combine(Path.GetFullPath("."), "Assets/3rd");

    [MenuItem("MegeXR/Import/Dummy")]
    public static void ImportDummySDK()
    {
        importFolder(Path.Combine(srcPath, "_dummy"), Path.Combine(destPath, "_dummy"));

        AssetDatabase.Refresh();

        PlayerSettings.gpuSkinning = false;
        PlayerSettings.virtualRealitySupported = false;
        PlayerSettings.stereoRenderingPath  = StereoRenderingPath.MultiPass;
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel21;
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
    }
    
    [MenuItem("MegeXR/Clean/Dummy")]
    public static void CleanDummySDK()
    {
        delFolder(Path.Combine(destPath, "_dummy"));

        AssetDatabase.Refresh();
    }

    [MenuItem("MegeXR/Import/PicoVR")]
    public static void ImportPicoVRSDK()
    {
        importFolder(Path.Combine(srcPath, "_picovr"), Path.Combine(destPath, "_picovr"));
        importAndroidManifest("_picovr");

        AssetDatabase.Refresh();

        PlayerSettings.gpuSkinning = false;
        PlayerSettings.virtualRealitySupported = false;
        PlayerSettings.stereoRenderingPath  = StereoRenderingPath.MultiPass;
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel21;
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
    }

    [MenuItem("MegeXR/Clean/PicoVR")]
    public static void CleanPicoVRSDK()
    {
        delFolder(Path.Combine(destPath, "_picovr"));
        delAndroidManifest();

        AssetDatabase.Refresh();
    }

    
    [MenuItem("MegeXR/Import/IdealensVR")]
    public static void ImportIdealensVRSDK()
    {
        importFolder(Path.Combine(srcPath, "_idealens"), Path.Combine(destPath, "_idealens"));
        importAndroidManifest("_idealens");

        AssetDatabase.Refresh();

        PlayerSettings.gpuSkinning = false;
        PlayerSettings.virtualRealitySupported = false;
        PlayerSettings.stereoRenderingPath  = StereoRenderingPath.MultiPass;
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel21;
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
    }

    [MenuItem("MegeXR/Clean/IdealensVR")]
    public static void CleanIdealensVRSDK()
    {
        delFolder(Path.Combine(destPath, "_idealens"));
        delAndroidManifest();

        AssetDatabase.Refresh();
    }


    [MenuItem("MegeXR/Import/SkyworthVR -S801")]
    public static void ImportSkyworthVR_S801_SDK()
    {
        importFolder(Path.Combine(srcPath, "_skyworth"), Path.Combine(destPath, "_skyworth"));
        importAndroidManifest("_skyworth");

        AssetDatabase.Refresh();

        PlayerSettings.gpuSkinning = false;
        PlayerSettings.virtualRealitySupported = false;
        PlayerSettings.stereoRenderingPath  = StereoRenderingPath.MultiPass;
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel21;
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
    }

    [MenuItem("MegeXR/Import/SkyworthVR -V901")]
    public static void ImportSkyworthVR_V901_SDK()
    {
        importFolder(Path.Combine(srcPath, "_skyworth"), Path.Combine(destPath, "_skyworth"));
        importAndroidManifest("_skyworth");

        AssetDatabase.Refresh();

        PlayerSettings.gpuSkinning = false;
        PlayerSettings.virtualRealitySupported = true;
        PlayerSettings.stereoRenderingPath  = StereoRenderingPath.SinglePass;
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel21;
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
    }

    [MenuItem("MegeXR/Clean/SkyworthVR")]
    public static void CleanSkyworthVRSDK()
    {
        delFolder(Path.Combine(destPath, "_skyworth"));
        delAndroidManifest();

        AssetDatabase.Refresh();
    }

     [MenuItem("MegeXR/Import/SteamVR")]
    public static void ImportSteamVRSDK()
    {
        importFolder(Path.Combine(srcPath, "_steam"), Path.Combine(destPath, "_steam"));

        AssetDatabase.Refresh();

        PlayerSettings.gpuSkinning = true;
        PlayerSettings.virtualRealitySupported = true;
        PlayerSettings.stereoRenderingPath  = StereoRenderingPath.SinglePass;
    }

    [MenuItem("MegeXR/Clean/SteamVR")]
    public static void CleanSteamVRSDK()
    {
        delFolder(Path.Combine(destPath, "_steam"));

        AssetDatabase.Refresh();
    }


    private static void cleanSymbol()
    {
        setSymbol("");
    }

    private static void setSymbol(string _symbol)
    {
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, _symbol);
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, _symbol);
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, _symbol);
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.WebGL, _symbol);
    }

    private static void importFolder(string _from, string _to)
    {
        if (!Directory.Exists(_from))
            return;

        if (!Directory.Exists(_to))
            Directory.CreateDirectory(_to);

        copyFiles(_from, _to);

        string[] sourceFolders = Directory.GetDirectories(_from);

        foreach (string sourceFolder in sourceFolders)
        {
            string destFolder = _to + "\\" + sourceFolder.Substring(_from.Length + 1);

            importFolder(sourceFolder, destFolder);
        }
    }

    private static void importAndroidManifest(string _sdkName)
    {
        string from = Path.Combine(srcPath, string.Format("AndroidManifest/{0}/AndroidManifest.xml", _sdkName));
        string to = Path.Combine(Path.GetFullPath("."), "Assets/Plugins/Android/AndroidManifest.xml");
        if (!File.Exists(from))
            return;

        Directory.CreateDirectory(Path.Combine(Path.GetFullPath("."), "Assets/Plugins"));
        Directory.CreateDirectory(Path.Combine(Path.GetFullPath("."), "Assets/Plugins/Android"));

        File.Copy(from, to, true);
    }

    private static void copyFiles(string _sourceDirectory, string _destDirectory)
    {
        string[] sourceFilePaths = Directory.GetFiles(_sourceDirectory);

        foreach (string sourceFilePath in sourceFilePaths)
        {
            string destFilePath = _destDirectory + "\\" + sourceFilePath.Substring(_sourceDirectory.Length + 1);
            File.Copy(sourceFilePath, destFilePath, true);
        }
    }


    private static void cleanFolder(string _target, string _source)
    {
        DirectoryInfo target = new DirectoryInfo(_target);
        DirectoryInfo source = new DirectoryInfo(_source);

        List<string> files = parseFolder(target.FullName, source.FullName, _source);

        foreach (string file in files)
        {
            if (File.Exists(file))
                File.Delete(file);
        }

        Debug.Log(string.Format("Delete {0} files", files.Count));
    }

    private static void delFolder(string _path)
    {
        Debug.Log("delete folder: " + _path);
        if(!Directory.Exists(_path))
            return;

        DirectoryInfo d = new DirectoryInfo(_path);
        FileSystemInfo[] fsinfos = d.GetFileSystemInfos();
        foreach (FileSystemInfo fsinfo in fsinfos)
        {
            if (fsinfo is DirectoryInfo)
            {
                delFolder(fsinfo.FullName);
            }
            else
            {
                File.Delete(fsinfo.FullName);
            }
        }
        Directory.Delete(_path);
    }

    private static void delAndroidManifest()
    {
        string file = Path.Combine(Path.GetFullPath("."), "Assets/Plugins/Android/AndroidManifest.xml");
        if(File.Exists(file))
            File.Delete(file);
    }

    private static List<string> parseFolder(string _target, string _source, string _now)
    {
        List<string> files = new List<string>();

        if (!Directory.Exists(_source))
            return files;

        if (!Directory.Exists(_target))
            return files;

        DirectoryInfo d = new DirectoryInfo(_now);
        FileSystemInfo[] fsinfos = d.GetFileSystemInfos();
        foreach (FileSystemInfo fsinfo in fsinfos)
        {
            if (fsinfo is DirectoryInfo)
            {
                files.AddRange(parseFolder(_target, _source, fsinfo.FullName));
            }
            else
            {
                files.Add(fsinfo.FullName.Replace(_source, _target));
            }
        }
        return files;

    }
}

