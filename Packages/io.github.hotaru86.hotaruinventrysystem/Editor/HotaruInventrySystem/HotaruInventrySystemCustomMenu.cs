/*
Copyright (c) 2023 hotaru86
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace hotarunohikari.HotaruInventrySystem
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

            //呼び出されるたびにcountを1増やす
            //countが選択オブジェクト数と一致したとき(最後のオブジェクトについての処理の時)のみ実行
            count++;
            if (count < Selection.gameObjects.Length) return;
            count = 0;

            foreach (GameObject obj in Selection.gameObjects)
            {
                //複数選択時に同一オブジェクトを登録しないように、登録済みオブジェクトがあればキャンセル
                //何度も登録してしまわないよう、ここでチェックする
                if (HotaruInventrySystemWindow.IsContainGameObjectInGroup(obj))
                {
                    EditorUtility.DisplayDialog("HotaruInventrySystem", "すでに登録済みのオブジェクトが選択されています。", "OK");
                    HotaruInventrySystemWindow.OpenWindow();
                    return;
                }
            }

            HotaruInventrySystemWindow.AddGameObjectAsNewGroup();
        }
    }
}
#endif
