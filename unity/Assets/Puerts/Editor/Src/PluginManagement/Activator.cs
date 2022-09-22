#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Puerts.Editor.PluginManagement
{
    public enum BackendType
    {
        NodeJS,
        QuickJS,
        V8_Debug,
        V8,
        None
    }

    [UnityEditor.InitializeOnLoad]
    public class Activator
    {
        protected static void SetStoragedBackendType(BackendType type) 
        {
            EditorPrefs.SetString("PuerTS_PluginManagement_CurrentBackend", type.ToString());
        }
        protected static BackendType GetStoragedBackendType() 
        {
            string typeStr = EditorPrefs.GetString("PuerTS_PluginManagement_CurrentBackend");
            BackendType ret;
            if (typeStr == string.Empty || typeStr == null || !Enum.TryParse(typeStr, out ret))
            {
                return BackendType.NodeJS;
            }
            return ret;
        }


        public enum PluginRuntimeType
        {
            Editor,
            Runtime,
            ALL
        }

        private const string MENU_PATH = "PuerTS/Select Backend/";
        private const string PuertsPluginFolder = "Plugins";
        private const UnityEditor.BuildTarget INVALID_BUILD_TARGET = (UnityEditor.BuildTarget)(-1);
        static Activator()
        {
            BuildTargetToPlatformName.Add(UnityEditor.BuildTarget.StandaloneOSX, "macOS");
            BuildTargetToPlatformName.Add(UnityEditor.BuildTarget.StandaloneWindows, "x86");
            BuildTargetToPlatformName.Add(UnityEditor.BuildTarget.StandaloneWindows64, "x86_64");
            var curEditorEngineType = GetStoragedBackendType();
            activatePluginByEngineType(curEditorEngineType, false);
            UnityEditor.EditorApplication.delayCall += delayUpdateMenu;

        }

        protected class PluginInfo 
        {
            public string Backend;
            public string Platform;
            public string Architecture;
        }
        protected static PluginInfo getPluginInfoFromAssetPath(string assetPath)
        {
            var splitPath = splitAssetPathRelativeToPlugin(assetPath);
            if (splitPath == null) return null;

            PluginInfo pi = new PluginInfo();

            pi.Backend = splitPath[0];

            var pluginBasename = splitPath[splitPath.Length - 1].Split('.')[0];
            var pluginExtname = splitPath[splitPath.Length - 1].Split('.')[1];

            var split1 = splitPath[1];
            switch (split1) 
            {
                case "x86":
                case "x86_64":
                    pi.Architecture = split1;
                    if (pluginExtname == "so") 
                        pi.Platform = "Linux";
                    else 
                        pi.Platform = "Windows";
                    break;
                default:
                    pi.Platform = split1;
                    pi.Architecture = splitPath[2];
                    break;
            }
            return pi;
        }
        // private static ScriptEngineType ttype;

        // private static void delayUpdateMenu()
        // {
        //     var curEditorEngineType = TsProjDevUserSetting.getAsset().editorEngineType;
        //     ttype = curEditorEngineType;
        //     UnityEngine.Debug.LogError("enginetype"+ttype.ToString());
        //     activatePluginByEngineType(ttype, false);
        //     updateMenuItemState(ttype);

        // }
        private static void delayUpdateMenu()
        {
            var curBackEndType = GetStoragedBackendType();

            updateMenuItemState(curBackEndType);

        }
        public static bool activatePluginsForEditor(BackendType targetType)
        {
            var importers = getPuertsPluginImporters(
                new UnityEditor.BuildTarget[] { 
                    UnityEditor.BuildTarget.StandaloneWindows, 
                    UnityEditor.BuildTarget.StandaloneOSX 
                }
            );
            var hadActivated = false;

            List<UnityEditor.PluginImporter> ActiveList = new List<UnityEditor.PluginImporter>();
            List<UnityEditor.PluginImporter> DeactiveList = new List<UnityEditor.PluginImporter>();


            foreach (var pluginImporter in importers)
            {
                PluginInfo pi = getPluginInfoFromAssetPath(pluginImporter.assetPath);
                var editorCPU = string.Empty;
                var editorOS = string.Empty;

                if (pi.Platform != "macOS" && pi.Platform != "Windows") continue;
                editorOS = pi.Platform;
                if (pi.Platform == "macOS") {
                    editorOS = "OSX";
                }
                editorCPU = pi.Architecture;
                
                if (
                    !string.IsNullOrEmpty(editorOS)
                    && pi.Backend == targetType.ToString()
                )
                {
                    hadActivated = true;
                    pluginImporter.SetEditorData("CPU", editorCPU);
                    pluginImporter.SetEditorData("OS", editorOS);
                    ActiveList.Add(pluginImporter);
                } 
                else 
                {
                    DeactiveList.Add(pluginImporter);
                }
            }

            // deactive plugins that not match the target
            foreach (var pluginImporter in DeactiveList)
            {
                var AssetChanged = false;
                if (pluginImporter.GetCompatibleWithAnyPlatform())
                {
                    pluginImporter.SetCompatibleWithAnyPlatform(false);
    #if UNITY_2019_1_OR_NEWER
                    pluginImporter.isPreloaded = false;
    #endif
                    AssetChanged = true;
                }
                AssetChanged |= pluginImporter.GetCompatibleWithEditor() != false;
                pluginImporter.SetCompatibleWithEditor(false);
    #if UNITY_2019_1_OR_NEWER
                pluginImporter.isPreloaded = false;
    #endif

                if (AssetChanged)
                {
                    UnityEditor.AssetDatabase.ImportAsset(pluginImporter.assetPath);
                }
            }
            foreach (var pluginImporter in ActiveList)
            {
                var AssetChanged = false;
                if (pluginImporter.GetCompatibleWithAnyPlatform())
                {
                    pluginImporter.SetCompatibleWithAnyPlatform(false);
    #if UNITY_2019_1_OR_NEWER
                    pluginImporter.isPreloaded = false;
    #endif
                    AssetChanged = true;
                }
                AssetChanged |= pluginImporter.GetCompatibleWithEditor() != true;
                pluginImporter.SetCompatibleWithEditor(true);
    #if UNITY_2019_1_OR_NEWER
                pluginImporter.isPreloaded = true;
    #endif

                if (AssetChanged)
                {
                    UnityEditor.AssetDatabase.ImportAsset(pluginImporter.assetPath);
                }
            }


            if (hadActivated)
            {
                UnityEngine.Debug.Log("[Puerts] Plugins successfully activated for " + targetType.ToString() + " in Editor.");
            }
            else
            {
                UnityEngine.Debug.LogWarning("[Puerts] Plugins fail activated for " + targetType.ToString() + " in Editor. maybe there is no " + targetType.ToString() + " plugin existing");
            }
            return hadActivated;
        }
        public static void activatePluginsForDeployment(UnityEditor.BuildTarget target, BackendType type, bool activate)
        {
            var importers = getPuertsPluginImporters(new UnityEditor.BuildTarget[] { target });
            foreach (var pluginImporter in importers)
            {
                PluginInfo pi = getPluginInfoFromAssetPath(pluginImporter.assetPath);
                if (pi == null) continue;

                switch (pi.Platform)
                {
                    case "iOS":
                    case "tvOS":
                    case "PS4":
                    case "PS5":
                    case "XboxOne":
                    case "Stadia":
                    case "XboxSeriesX":
                    case "XboxOneGC":
                        break;

                    case "Android":
                        if (pi.Architecture == "armeabi-v7a")
                        {
                            pluginImporter.SetPlatformData(UnityEditor.BuildTarget.Android, "CPU", "ARMv7");
                        }
                        else if (pi.Architecture == "arm64-v8a")
                        {
                            pluginImporter.SetPlatformData(UnityEditor.BuildTarget.Android, "CPU", "ARM64");
                        }
                        else if (pi.Architecture == "x86")
                        {
                            pluginImporter.SetPlatformData(UnityEditor.BuildTarget.Android, "CPU", "x86");
                        }
                        else
                        {
                            UnityEngine.Debug.Log("[Puerts]: Architecture not found: " + pi.Architecture);
                        }
                        break;

                    case "Linux":
                        if (pi.Architecture != "x86" && pi.Architecture != "x86_64")
                        {
                            UnityEngine.Debug.Log("[Puerts]: Architecture not found: " + pi.Architecture);
                            continue;
                        }
                        setStandalonePlatformData(pluginImporter, pi.Platform, pi.Architecture);
                        break;

                    case "macOS":
                        setStandalonePlatformData(pluginImporter, pi.Platform, pi.Architecture);
                        break;

                    // case "WSA":
                    //     pi.Architecture = splitPath[1];
                    //     engineType = splitPath[2];

                    //     pluginImporter.SetPlatformData(UnityEditor.BuildTarget.WSAPlayer, "SDK", "AnySDK");

                    //     if (pi.Architecture == "WSA_UWP_Win32")
                    //     {
                    //         pluginImporter.SetPlatformData(UnityEditor.BuildTarget.WSAPlayer, "CPU", "X86");
                    //     }
                    //     else if (pi.Architecture == "WSA_UWP_x64")
                    //     {
                    //         pluginImporter.SetPlatformData(UnityEditor.BuildTarget.WSAPlayer, "CPU", "X64");
                    //     }
                    //     else if (pi.Architecture == "WSA_UWP_ARM")
                    //     {
                    //         pluginImporter.SetPlatformData(UnityEditor.BuildTarget.WSAPlayer, "CPU", "ARM");
                    //     }
                    //     else if (pi.Architecture == "WSA_UWP_ARM64")
                    //     {
                    //         pluginImporter.SetPlatformData(UnityEditor.BuildTarget.WSAPlayer, "CPU", "ARM64");
                    //     }
                    //     break;

                    case "Windows":
                        if (pi.Architecture != "x86" && pi.Architecture != "x86_64")
                        {
                            UnityEngine.Debug.Log("[Puerts]: Architecture not found: " + pi.Architecture);
                            continue;
                        }
                        setStandalonePlatformData(pluginImporter, pi.Platform, pi.Architecture);
                        break;
                    // case "Switch":
                    //     pi.Architecture = splitPath[1];
                    //     engineType = splitPath[2];

                    //     if (SwitchBuildTarget == INVALID_BUILD_TARGET)
                    //     {
                    //         continue;
                    //     }

                    //     if (pi.Architecture != "NX32" && pi.Architecture != "NX64")
                    //     {
                    //         UnityEngine.Debug.Log("[Puerts]: Architecture not found: " + pi.Architecture);
                    //         continue;
                    //     }
                    //     break;

                    default:
                        UnityEngine.Debug.Log("[Puerts]: Unknown platform: " + pi.Platform);
                        continue;
                }

                var AssetChanged = false;
                if (pluginImporter.GetCompatibleWithAnyPlatform())
                {
                    pluginImporter.SetCompatibleWithAnyPlatform(false);
                    AssetChanged = true;
                }

                var bActivate = true;
                if (pi.Backend != type.ToString())
                {
                    bActivate = false;
                }

                bool isCompatibleWithPlatform = bActivate && activate;
                if (!bActivate && target == UnityEditor.BuildTarget.WSAPlayer)
                {
                    AssetChanged = true;
                }
                else
                {
                    AssetChanged |= pluginImporter.GetCompatibleWithPlatform(target) != isCompatibleWithPlatform;
                }

                pluginImporter.SetCompatibleWithPlatform(target, isCompatibleWithPlatform);

                if (AssetChanged)
                {
                    pluginImporter.SaveAndReimport();
                }
            }
        }
        public static Dictionary<UnityEditor.BuildTarget, string> BuildTargetToPlatformName = new Dictionary<UnityEditor.BuildTarget, string>();

        // returns the name of the folder that contains plugins for a specific target
        private static string getBuildTargetPlatformName(UnityEditor.BuildTarget target)
        {
            if (BuildTargetToPlatformName.ContainsKey(target))
            {
                return BuildTargetToPlatformName[target];
            }
            return target.ToString();
        }
        private static void setStandalonePlatformData(UnityEditor.PluginImporter pluginImporter, string platformName, string architecture)
        {
            var isLinux = platformName == "Linux";
            var isWindows = platformName == "Windows";
            var isMac = platformName == "Mac";
            var isX86 = architecture == "x86";
            var isX64 = architecture == "x86_64";

    #if !UNITY_2019_2_OR_NEWER
            pluginImporter.SetPlatformData(UnityEditor.BuildTarget.StandaloneLinux, "CPU", isLinux && isX86 ? "x86" : "None");
            pluginImporter.SetPlatformData(UnityEditor.BuildTarget.StandaloneLinuxUniversal, "CPU", !isLinux ? "None" : isX86 ? "x86" : isX64 ? "x86_64" : "None");
    #endif
            pluginImporter.SetPlatformData(UnityEditor.BuildTarget.StandaloneLinux64, "CPU", isLinux && isX64 ? "x86_64" : "None");
            pluginImporter.SetPlatformData(UnityEditor.BuildTarget.StandaloneWindows, "CPU", isWindows && isX86 ? "AnyCPU" : "None");
            pluginImporter.SetPlatformData(UnityEditor.BuildTarget.StandaloneWindows64, "CPU", isWindows && isX64 ? "AnyCPU" : "None");
            pluginImporter.SetPlatformData(UnityEditor.BuildTarget.StandaloneOSX, "CPU", isMac ? "AnyCPU" : "None");
        }
        private static string[] splitAssetPathRelativeToPlugin(string path)
        {
            var indexOfPluginFolder = path.IndexOf(PuertsPluginFolder, System.StringComparison.OrdinalIgnoreCase);
            if (indexOfPluginFolder == -1)
            {
                return null;
            }

            return path.Substring(indexOfPluginFolder + PuertsPluginFolder.Length + 1).Split('/');
        }
        public static List<UnityEditor.PluginImporter> getPuertsPluginImporters(UnityEditor.BuildTarget[] targetPlatforms)

        {

            UnityEditor.PluginImporter[] pluginImporters = UnityEditor.PluginImporter.GetAllImporters();

            List<UnityEditor.PluginImporter> puertsPlugins = new List<UnityEditor.PluginImporter>();
            string filterPath = "package/Plugins";


            foreach (var pluginImporter in pluginImporters)
            {
                if (targetPlatforms != null)
                {
                    for (int i = 0; i < targetPlatforms.Length; i++)
                    {
                        string platformName = getBuildTargetPlatformName(targetPlatforms[i]);

                        if (
                            pluginImporter.assetPath.Contains(filterPath) && 
                            pluginImporter.assetPath.Contains(platformName)
                        )
                        {
                            puertsPlugins.Add(pluginImporter);
                        }
                    }
                }
                else
                {
                    if (pluginImporter.assetPath.Contains(filterPath))
                    {
                        puertsPlugins.Add(pluginImporter);
                    }
                }


            }
            return puertsPlugins;
        }

        private static void activatePluginByEngineType(BackendType type, bool updateMenu = true)
        {
            if (UnityEngine.Application.isPlaying)
            {
                Debug.LogWarning("[Puerts] can not switch PuerTS backend in play mode");
                return;
            }
            if (type == BackendType.None) return;
            var activatedBackend = activatePluginsForEditor(type);
            if (activatedBackend)
            {
                SetStoragedBackendType(type);
                if (updateMenu) updateMenuItemState(type);
            }

        }










        private static void updateMenuItemState(BackendType type)
        {
            SetChecked(BackendType.NodeJS, type);
            SetChecked(BackendType.QuickJS, type);
            SetChecked(BackendType.V8_Debug, type);
            SetChecked(BackendType.V8, type);
        }
        private static void SetChecked(BackendType menuType, BackendType targetType)
        {

            UnityEditor.Menu.SetChecked(MENU_PATH + menuType, menuType == targetType);
        }
        [UnityEditor.MenuItem(MENU_PATH + "DisableAll", false)]
        public static void disableAllPuertsPlugin()
        {
            Activator.deactivateAllPlugins();
            EditorBackEndTypeSetting.setCurEditorEngineType(BackendType.None);
            updateMenuItemState(BackendType.None);
        }
        /// <summary>
        /// puerts插件，所有平台禁用
        /// </summary>
        public static void deactivateAllPlugins()
        {
            var importers = getPuertsPluginImporters(null);
            var platforms = new BuildTarget[] {
                BuildTarget.Android,
                BuildTarget.iOS,
                BuildTarget.WebGL,
                BuildTarget.StandaloneWindows,
                BuildTarget.StandaloneWindows64,
                BuildTarget.StandaloneOSX,
                BuildTarget.StandaloneLinux64
            };
            foreach (var pluginImporter in importers)
            {
                //var assetPath = pluginImporter.assetPath;
                var isChanged = false;

                var isAny = pluginImporter.GetCompatibleWithAnyPlatform();
                if (isAny)
                {
                    pluginImporter.SetCompatibleWithAnyPlatform(false);
                }
                var isForEditor = pluginImporter.GetCompatibleWithEditor();
                if (isForEditor)
                {
                    pluginImporter.SetCompatibleWithEditor(false);

                }
                for (int i = 0; i < platforms.Length; i++)
                {
                    var platformTarget = platforms[i];
                    if (pluginImporter.GetCompatibleWithPlatform(platformTarget))
                    {
                        pluginImporter.SetCompatibleWithPlatform(platformTarget, false);
                        isChanged = true;
                    }
                }


                if (isChanged)
                {
    #if UNITY_2019_1_OR_NEWER
                    pluginImporter.isPreloaded = false;
    #endif
                    pluginImporter.SaveAndReimport();
                }

            }

        }
        /// <summary>
        /// 所有puerts插件在编辑器平台禁用
        /// </summary>
        private static void deactivateEditorPlugins()
            updateMenuItemState(BackendType.None);
        }
        [UnityEditor.MenuItem(MENU_PATH + "NodeJS", false)]
        public static void ActivateNodeJS()
        {
            activatePluginByEngineType(BackendType.NodeJS);
        }
        [UnityEditor.MenuItem(MENU_PATH + "QuickJS")]
        public static void ActivateQuickJS()
        {
            activatePluginByEngineType(BackendType.QuickJS);
        }
        [UnityEditor.MenuItem(MENU_PATH + "V8_Debug")]
        public static void ActivateV8_Debug()
        {
            activatePluginByEngineType(BackendType.V8_Debug);
        }
        [UnityEditor.MenuItem(MENU_PATH + "V8")]
        public static void ActivateV8()
        {
            activatePluginByEngineType(BackendType.V8);
        }
    }
}
#endif