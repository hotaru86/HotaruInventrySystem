/*
Copyright (c) 2024 hotaru86
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/
#if UNITY_EDITOR
using hotarunohikari.HotaruInventrySystem.Component;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace hotarunohikari.HotaruInventrySystem.Editor
{
    public class HotaruInventrySystemCustomMenu : MonoBehaviour
    {
        //同時に選択したかどうかの識別のためのカウント変数
        static int count = 0;

        [MenuItem("GameObject/HotaruInventrySystemに追加", false, 49)]
        private static void AddObjectGroup(MenuCommand menuCommand)
        {
            if(Selection.gameObjects.Length == 0)
            {
                EditorUtility.DisplayDialog("HotaruInventrySystem", "オブジェクトが選択されていません。\nオブジェクトを選択した状態で、追加してください。", "OK");
                HotaruInventrySystemWindow.OpenWindow();
                return;
            }

            //オブジェクト複数選択時に1度だけ処理を行うためのロジック
            //呼び出されるたびにcountを1増やし、countが選択オブジェクト数と一致したとき(最後のオブジェクトについての処理の時)のみ実行
            count++;
            if (count < Selection.gameObjects.Length) return;
            count = 0;

            //選択されたオブジェクト群をHISGroupに変換
            HISGroup newGroup = new HISGroup {
                groupName = Selection.gameObjects[0].name,
                objects = Selection.gameObjects.ToList(),
                defaultState = Enumerable.Repeat(true, Selection.gameObjects.Length).ToList(),
            };

            HotaruInventrySystem_Menu menu = GetHISMenuWithSelectionObjects();
            if(menu == null)
            {
                EditorUtility.DisplayDialog("HotaruInventrySystem", "アバターの子でないオブジェクトが選択されています。\n一つのアバターの中にあるオブジェクトのみを選択してください。", "OK");
                HotaruInventrySystemWindow.OpenWindow();
                return;
            }

            //同一オブジェクトチェック
            HashSet<GameObject> newObjectsSet = newGroup.objects.ToHashSet();
            foreach (HISGroup group in menu.groups)
            {
                if (group.objects.Any(x => newObjectsSet.Contains(x)))
                {
                    EditorUtility.DisplayDialog("HotaruInventrySystem", "同一オブジェクトが複数グループに含まれています。", "OK");
                    return;
                }
            }

            menu.AddGroup(newGroup);
            HotaruInventrySystemWindow.OpenWindow();
        }

        //Selection.gameObjectsの共通の親であるアバター、の子であるHISMenuを返す。なければ作って返す。
        static HotaruInventrySystem_Menu GetHISMenuWithSelectionObjects()
        {
            //そもそもアバターがなければnullを返す
            VRCAvatarDescriptor avatar = Selection.gameObjects[0].GetComponentInParent<VRCAvatarDescriptor>();
            if(avatar == null) return null;

            foreach(GameObject obj in Selection.gameObjects)
            {
                //選択オブジェクトが1つでも同じアバターの子になければnullを返す。
                if (!obj.transform.IsChildOf(avatar.transform)) return null;
                //選択オブジェクトがアバター自体でもnullを返す
                if(obj == avatar.gameObject) return null;
            }

            //既存Menuがあれば返す
            HotaruInventrySystem_Menu existMenu = avatar.GetComponentInChildren<HotaruInventrySystem_Menu>();
            if (existMenu) return existMenu;

            //既存Menuなければ作って返す
            GameObject newMenuObj = new GameObject() { name = "Hotaru Inventry System" };
            newMenuObj.transform.parent = avatar.transform;
            HotaruInventrySystem_Menu newMenu = newMenuObj.AddComponent<HotaruInventrySystem_Menu>();

            //デフォルトアイコンを設定
            string defaultIconPath = "Packages/io.github.hotaru86.hotaruinventrysystem/Resources/Tofu_icon128.png";
            Texture2D defaultIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(defaultIconPath);
            if (defaultIcon != null)
            {
                newMenu.menuIcon = defaultIcon;
            }else{
                Debug.LogWarning($"Default icon not found at {defaultIconPath}. Please set a default icon in the script.");
            }
            return newMenu;
        }
    }
}
#endif
