/*
Copyright (c) 2024 hotaru86
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/
#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using hotarunohikari.HotaruInventrySystem.Component;

namespace hotarunohikari.HotaruInventrySystem.Editor
{

    public class HotaruInventrySystemWindow : EditorWindow
    {
        // ゲームオブジェクトグループを表すクラス
        [System.Serializable]
        public class HISGroup
        {
            public string groupName;
            public GameObject[] objects;
            public bool defaultState = true;
        }
        private static HotaruInventrySystem_Menu targetMenu;

        public static void OnTargetAvatarChanged(VRCAvatarDescriptor oldTarget, VRCAvatarDescriptor newTarget)
        {
            objectGroups.Clear();
        }

        private static List<HISGroup> objectGroups = new List<HISGroup>();

        static string appDisplayName = "Hotaru Inventry System";

        [MenuItem("Tools/HotaruInventrySystem")]
        public static void OpenWindow()
        {
            HotaruInventrySystemWindow window = GetWindow<HotaruInventrySystemWindow>();
            window.titleContent = new GUIContent(appDisplayName);
            window.Show();
        }

        private void OnGUI()
        {
            UpdateTargetMenu();
            HISMenuCustomInspector.DrawHISGUI(targetMenu);
        }

        void UpdateTargetMenu()
        {
            //選択オブジェクトがアバターに属するなら、そのアバターの子であるHISMenuを取得
            if(Selection.gameObjects.Length > 0)
            {
                var avatar = Selection.gameObjects[0].GetComponentInParent<VRCAvatarDescriptor>();
                if (avatar != null)
                {
                    targetMenu = avatar.GetComponentInChildren<HotaruInventrySystem_Menu>();
                }
            }
            //選択オブジェクトがアバターに属していないか、アバター下にHISMenuがなければ、シーン内の0番目Menuを取得する。
            if (targetMenu == null)
            {
                HotaruInventrySystem_Menu[] menus = FindObjectsOfType<HotaruInventrySystem_Menu>();
                if (menus.Length >= 1)
                {
                    targetMenu = menus[0];
                }
            }
        }
        
    }
}
#endif
