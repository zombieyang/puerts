using System;
using UnityEditor;
using UnityEngine;

namespace Puerts.Editor.PluginManagement
{
    [UnityEditor.InitializeOnLoad]
    internal class Menus
    {
        private const string MENU_PATH = "PuerTS/Select Backend/";

        static Menus()
        {
            UnityEditor.EditorApplication.delayCall += delayUpdateMenu;
        }
        private static void delayUpdateMenu()
        {
            var curBackEndType = BackendStorage.GetCurrentBackendType();

            updateMenuItemState(curBackEndType);

        }

        public static void updateMenuItemState(BackendType type)
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
            BackendStorage.SetCurrentBackendType(BackendType.None);
            updateMenuItemState(BackendType.None);
        }
        /// <summary>
        /// 所有puerts插件在编辑器平台禁用
        /// </summary>
        private static void deactivateEditorPlugins()
        {
            updateMenuItemState(BackendType.None);
        }
        [UnityEditor.MenuItem(MENU_PATH + "NodeJS", false)]
        public static void ActivateNodeJS()
        {
            Activator.activatePluginByBackendType(BackendType.NodeJS);
            updateMenuItemState(BackendType.NodeJS);
        }
        [UnityEditor.MenuItem(MENU_PATH + "QuickJS")]
        public static void ActivateQuickJS()
        {
            Activator.activatePluginByBackendType(BackendType.QuickJS);
            updateMenuItemState(BackendType.QuickJS);
        }
        [UnityEditor.MenuItem(MENU_PATH + "V8")]
        public static void ActivateV8()
        {
            Activator.activatePluginByBackendType(BackendType.V8);
            updateMenuItemState(BackendType.V8);
        }
        [UnityEditor.MenuItem(MENU_PATH + "V8_Debug")]
        public static void ActivateV8_Debug()
        {
            Activator.activatePluginByBackendType(BackendType.V8_Debug);
            updateMenuItemState(BackendType.V8_Debug);
        }
    }

}