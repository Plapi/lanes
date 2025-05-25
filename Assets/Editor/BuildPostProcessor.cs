//
//  Copyright (c) 2022 Tenjin. All rights reserved.
//

using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;

#if UNITY_IOS
using UnityEditor.iOS.Xcode;
using UnityEditor.iOS.Xcode.Extensions;
#endif

public class BuildPostProcessor : MonoBehaviour
{

    [MenuItem("Assets/Tenjin/Export Unity Package")]
    public static void ExportTenjinUnityPackage()
    {
        string exportedFileName = "TenjinUnityPackage.unitypackage";
        string assetsPath = "Assets";
        List<string> tenjinAssets = new List<string>();

        // Editor files
        tenjinAssets.Add(assetsPath + "/Editor/BuildPostProcessor.cs");
        tenjinAssets.Add(assetsPath + "/Editor/Dependencies.xml");
        tenjinAssets.Add(assetsPath + "/Tenjin/Scripts/Editor/TenjinAssetSelector.cs");
        tenjinAssets.Add(assetsPath + "/Tenjin/Scripts/Editor/TenjinEditorPrefs.cs");
        tenjinAssets.Add(assetsPath + "/Tenjin/Scripts/Editor/TenjinPackager.cs");

        // Gradle Templates
        tenjinAssets.Add(assetsPath + "/Plugins/Android/GradleTemplates/m2repository.gradle");

        // iOS dependencies
        tenjinAssets.Add(assetsPath + "/Plugins/iOS/GADUAdNetworkExtras.h");
        tenjinAssets.Add(assetsPath + "/Plugins/iOS/TenjinSDK.h");
        tenjinAssets.Add(assetsPath + "/Plugins/iOS/TenjinSDK.xcframework.zip");
        tenjinAssets.Add(assetsPath + "/Plugins/iOS/TenjinUnityInterface.h");
        tenjinAssets.Add(assetsPath + "/Plugins/iOS/TenjinUnityInterface.mm");

        // Tenjin files
        tenjinAssets.Add(assetsPath + "/Tenjin/Scripts/AndroidTenjin.cs");
        tenjinAssets.Add(assetsPath + "/Tenjin/Scripts/AppStoreType.cs");
        tenjinAssets.Add(assetsPath + "/Tenjin/Scripts/BaseTenjin.cs");
        tenjinAssets.Add(assetsPath + "/Tenjin/Scripts/CspManager.cs");
        tenjinAssets.Add(assetsPath + "/Tenjin/Scripts/DebugTenjin.cs");
        tenjinAssets.Add(assetsPath + "/Tenjin/Scripts/iOSTenjin.cs");
        tenjinAssets.Add(assetsPath + "/Tenjin/Scripts/Tenjin.cs");

        // Integration scripts
        tenjinAssets.Add(assetsPath + "/Tenjin/Scripts/TenjinAdMobIntegration.cs");
        tenjinAssets.Add(assetsPath + "/Tenjin/Scripts/TenjinAppLovinIntegration.cs");
        tenjinAssets.Add(assetsPath + "/Tenjin/Scripts/TenjinCASIntegration.cs");
        tenjinAssets.Add(assetsPath + "/Tenjin/Scripts/TenjinHyperBidIntegration.cs");
        tenjinAssets.Add(assetsPath + "/Tenjin/Scripts/TenjinIronSourceIntegration.cs");
        tenjinAssets.Add(assetsPath + "/Tenjin/Scripts/TenjinTopOnIntegration.cs");
        tenjinAssets.Add(assetsPath + "/Tenjin/Scripts/TenjinTradPlusIntegration.cs");

        // Tenjin prefab
        tenjinAssets.Add(assetsPath + "/Tenjin/tenjin.unitypackage.manifest");

        // Export package
        AssetDatabase.ExportPackage(
            tenjinAssets.ToArray(),
            exportedFileName,
            ExportPackageOptions.IncludeDependencies | ExportPackageOptions.Interactive);
    }
        
    [PostProcessBuild]
    public static void OnPostprocessBuild(BuildTarget buildTarget, string path)
    {
        if (buildTarget == BuildTarget.iOS)
        {
            BuildiOS(path);
        }
        else if (buildTarget == BuildTarget.Android)
        {
            BuildAndroid(path);
        }
    }

    private static void BuildAndroid(string path = "")
    {
        Debug.Log("TenjinSDK: Starting Android Build");
    }

    private static void BuildiOS(string path = "")
    {
#if UNITY_IOS
        Debug.Log("TenjinSDK: Starting iOS Build");

        string projectPath = Path.Combine(path, "Unity-iPhone.xcodeproj/project.pbxproj");
        PBXProject project = new PBXProject();
        project.ReadFromFile(projectPath);

#if UNITY_2019_3_OR_NEWER
        string buildTarget = project.GetUnityFrameworkTargetGuid();
#else
        string buildTarget = project.TargetGuidByName("Unity-iPhone");
#endif

        AddFrameworksToProject(project, buildTarget);
        AddLinkerFlags(project, buildTarget);
        UpdatePlist(path);

        File.WriteAllText(projectPath, project.WriteToString());
#endif  
    }

#if UNITY_IOS
    [PostProcessBuild(50)]
    public static void OnPostProcessBuild(BuildTarget buildTarget, string path)
    {
        if (buildTarget == BuildTarget.iOS)
        {
            EmbedSignFramework(path);
        }
    }

    public static void EmbedSignFramework(string path)
    {
        string projPath = PBXProject.GetPBXProjectPath(path);
        if (!File.Exists(projPath))
        {
            Debug.LogError("Project file does not exist: " + projPath);
            return;
        }
        
        PBXProject proj = new PBXProject();
        proj.ReadFromString(File.ReadAllText(projPath));

        // Get the target GUID
        string unityFrameworkTargetGuid = proj.GetUnityFrameworkTargetGuid();
        string targetGuid = proj.GetUnityMainTargetGuid();

        string zipPathInUnity = "Assets/Plugins/iOS/TenjinSDK.xcframework.zip";
        string extractionPath = Path.Combine(path, "Frameworks");
        string frameworkPath = Path.Combine(extractionPath, "TenjinSDK.xcframework");

        if (Directory.Exists(frameworkPath))
        {
            Directory.Delete(frameworkPath, true);
        }

        try
        {
            ZipFile.ExtractToDirectory(zipPathInUnity, extractionPath);

            // Delete --MACOSX metadata folder
            string macosxMetaFolder = Path.Combine(extractionPath, "__MACOSX");
            if (Directory.Exists(macosxMetaFolder))
            {
                Directory.Delete(macosxMetaFolder, true);
            }
        }
        catch (IOException e)
        {
            Debug.LogError("Failed to extract zip file: " + e.Message);
            return;
        }

        // Add the .xcframework to the Xcode project and embed it in the main target
        AddFrameworkToXcodeProject(proj, targetGuid, unityFrameworkTargetGuid, frameworkPath);
        
        File.WriteAllText(projPath, proj.WriteToString());
    }

    private static void AddFrameworkToXcodeProject(PBXProject proj, string targetGuid, string unityFrameworkTargetGuid, string frameworkPath)
    {
        string fileGuid = proj.AddFile(frameworkPath, "Frameworks/TenjinSDK.xcframework");
        proj.AddFileToEmbedFrameworks(targetGuid, fileGuid);

        string fileGuidForUnityFramework = proj.AddFile(frameworkPath, "Frameworks/TenjinSDK.xcframework");
        proj.AddFileToBuildSection(targetGuid, proj.GetFrameworksBuildPhaseByTarget(targetGuid), fileGuid);
        proj.AddFileToBuildSection(unityFrameworkTargetGuid, proj.GetFrameworksBuildPhaseByTarget(unityFrameworkTargetGuid), fileGuidForUnityFramework);
    }

    private static void AddFrameworksToProject(PBXProject project, string buildTarget)
    {
        List<string> frameworks = new List<string>
        {
            "AdServices.framework",
            "AdSupport.framework",
            "AppTrackingTransparency.framework",
            "StoreKit.framework"
        };

        foreach (string framework in frameworks)
        {
            Debug.Log("TenjinSDK: Adding framework: " + framework);
            project.AddFrameworkToProject(buildTarget, framework, true);
        }
    }

    private static void AddLinkerFlags(PBXProject project, string buildTarget)
    {
        Debug.Log("TenjinSDK: Adding -ObjC flag to other linker flags (OTHER_LDFLAGS)");
        project.AddBuildProperty(buildTarget, "OTHER_LDFLAGS", "-ObjC");
    }

    private static void UpdatePlist(string path)
    {
        string plistPath = Path.Combine(path, "Info.plist");
        PlistDocument plist = new PlistDocument();
            
        plist.ReadFromFile(plistPath);

        plist.root.SetString("NSUserTrackingUsageDescription", 
                "We request to track data to enhance ad performance and user experience. Your privacy is respected.");

        File.WriteAllText(plistPath, plist.WriteToString());
    }

#endif
}
