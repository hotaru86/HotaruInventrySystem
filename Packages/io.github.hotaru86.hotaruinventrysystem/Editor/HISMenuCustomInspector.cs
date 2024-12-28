/*
Copyright (c) 2024 hotaru86
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using hotarunohikari.HotaruInventrySystem.Component;
using VRC.SDK3.Avatars.Components;

namespace hotarunohikari.HotaruInventrySystem.Editor
{
    [CustomEditor(typeof(HotaruInventrySystem_Menu))]
    public class HISMenuCustomInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            HotaruInventrySystem_Menu targetMenu = (HotaruInventrySystem_Menu)target;
            EditorGUILayout.LabelField("Hotaru Inventry System", EditorStyles.boldLabel);

            DrawHISGUI(targetMenu);

        }

        public static void DrawHISGUI(HotaruInventrySystem_Menu targetMenu)
        {
            //Playモード時などにAvatarが取得できなくなった場合に何もしない
            try
            {
                targetMenu.targetAvatar = EditorGUILayout.ObjectField("対象アバター", targetMenu.targetAvatar, typeof(VRCAvatarDescriptor), true) as VRCAvatarDescriptor;
            }catch(System.Exception e)
            {
                return;
            }

            if (targetMenu.groups.Count == 0)
            {
                GUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField("オブジェクトグループがありません。\nグループの追加は、オブジェクトを(複数)選択して\n右クリック->HotaruInventrySystemに追加", new GUIStyle(GUI.skin.label) { wordWrap = true });
                GUILayout.EndVertical();
                return;
            }

            // リストの各要素を表示・編集
            for (int i = 0; i < targetMenu.groups.Count; i++)
            {
                EditorGUI.indentLevel++;
                DrawGroup(targetMenu.groups[i]);
                EditorGUI.indentLevel--;
            }

            // 変更を保存
            if (GUI.changed)
            {
                EditorUtility.SetDirty(targetMenu);
            }


            void DrawGroup(HISGroup group)
            {
                float totalWidth = EditorGUIUtility.currentViewWidth;
                float objectFieldWidth = totalWidth * 0.5f;
                float defaultStateToggleWidth = 30;
                float defaultLabelWidth = totalWidth * 0.15f;
                float animatedLabelWidth = totalWidth * 0.15f;

                GUIStyle centerStyle = new GUIStyle(GUI.skin.label); centerStyle.alignment = TextAnchor.MiddleCenter;


                GUILayout.BeginVertical(EditorStyles.helpBox);
                GUILayout.Space(10);
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("グループ名", new GUIStyle(GUI.skin.label));
                Vector2 labelSize = new GUIStyle(GUI.skin.label).CalcSize(new GUIContent("グループ名"));

                group.groupName = EditorGUILayout.TextField("", group.groupName, GUILayout.Width(totalWidth * 0.7f - labelSize.x));
                GUILayout.Space(20);
                // 削除ボタン
                if (GUILayout.Button("削除", GUILayout.Width(50)))
                {
                    Undo.RecordObject(targetMenu, "Remove HISGroup");
                    targetMenu.RemoveGroup(group);
                    EditorUtility.SetDirty(targetMenu);
                }
                EditorGUILayout.EndHorizontal();



                //見出し
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("対象オブジェクト", GUILayout.Width(objectFieldWidth));
                GUILayout.Label("", centerStyle, GUILayout.Width(defaultStateToggleWidth));
                GUILayout.Label("初期状態で", centerStyle, GUILayout.Width(defaultLabelWidth));
                GUILayout.Label("ボタンを押すと", centerStyle, GUILayout.Width(animatedLabelWidth));
                EditorGUILayout.EndHorizontal();
                for (int i = 0; i < group.objects.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    group.objects[i] = EditorGUILayout.ObjectField(
                            group.objects[i],
                            typeof(GameObject),
                            true,
                            GUILayout.Width(objectFieldWidth)
                        ) as GameObject;
                    group.defaultState[i] = EditorGUILayout.Toggle(
                            "",
                            group.defaultState[i],
                            GUILayout.Width(defaultStateToggleWidth)
                        );
                    string defaultText = group.defaultState[i] ? "表示" : "非表示";
                    GUILayout.Label(defaultText, centerStyle, GUILayout.Width(defaultLabelWidth));
                    string animatedText = !group.defaultState[i] ? "表示" : "非表示";
                    GUILayout.Label(animatedText, centerStyle, GUILayout.Width(animatedLabelWidth));
                    EditorGUILayout.EndHorizontal();

                    GUILayout.Space(10);
                }
                GUILayout.EndVertical();
            }
            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUIStyle style = new GUIStyle(GUI.skin.label); style.wordWrap = true;
            EditorGUILayout.LabelField("グループの追加は、オブジェクトを(複数)選択して\n右クリック->HotaruInventrySystemに追加", style);
            GUILayout.EndVertical();
        }
    }

}
#endif