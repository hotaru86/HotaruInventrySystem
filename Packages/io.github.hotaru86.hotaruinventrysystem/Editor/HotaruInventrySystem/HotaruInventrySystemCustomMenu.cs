using Newtonsoft.Json.Converters;
using System;
using System.Collections;
using System.Collections.Generic;
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
                EditorUtility.DisplayDialog("HotaruInventrySystem", "オブジェクトを選択されていません。\nオブジェクトを選択した状態で、追加してください。", "OK");
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
                    return;
                }
            }

            HotaruInventrySystemWindow.AddGameObjectAsNewGroup();
        }
    }
}
