using System;
using UnityEditor;

namespace Puerts.Editor.PluginManagement
{
    public enum BackendType
    {
        NodeJS,
        QuickJS,
        V8,
        V8_Debug,
        None
    }

    public class BackendStorage
    {
        internal static void SetCurrentBackendType(BackendType type) 
        {
            EditorPrefs.SetString("PuerTS_PluginManagement_CurrentBackend", type.ToString());
        }
        internal static BackendType GetCurrentBackendType() 
        {
            string typeStr = EditorPrefs.GetString("PuerTS_PluginManagement_CurrentBackend");
            BackendType ret;
            if (typeStr == string.Empty || typeStr == null || !Enum.TryParse(typeStr, out ret))
            {
                return BackendType.NodeJS;
            }
            return ret;
        }
    }
}