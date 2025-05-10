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
            GUILayout.BeginVertical();
            GUILayout.Space(10);
            //Playモード時などにAvatarが取得できなくなった場合に何もしない
            try
            {
                targetMenu.targetAvatar = EditorGUILayout.ObjectField("対象アバター", targetMenu.targetAvatar, typeof(VRCAvatarDescriptor), true) as VRCAvatarDescriptor;
            }catch(System.Exception e)
            {
                return;
            }

            GUILayout.Space(10);
            targetMenu.showMenuSettings = EditorGUILayout.Foldout(targetMenu.showMenuSettings, "メニュー全体設定", true);
            if (targetMenu.showMenuSettings)
            {
                GUILayout.Space(10);
                using (new EditorGUILayout.VerticalScope())
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.Space(10);
                        float totalWidth = EditorGUIUtility.currentViewWidth;
                        float iconWidth = 100;
                        float labelWidth = totalWidth - iconWidth - 20;
                        targetMenu.menuIcon = (Texture2D)EditorGUILayout.ObjectField(
                            targetMenu.menuIcon,
                            typeof(Texture2D),
                            false,
                            GUILayout.Width(iconWidth),
                            GUILayout.Height(iconWidth)
                        );
                        using (new EditorGUILayout.VerticalScope())
                        {
                            GUILayout.Label("メニュー名", new GUIStyle(GUI.skin.label), GUILayout.Width(labelWidth));
                            targetMenu.menuName = EditorGUILayout.TextField(
                                "",
                                targetMenu.menuName,
                                GUILayout.Width(labelWidth - 10)
                            );
                        }
                    }
                }
            }
            GUILayout.Space(20);

            if (targetMenu.groups.Count == 0)
            {
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.LabelField("オブジェクトグループがありません。\nグループの追加は、オブジェクトを(複数)選択して\n右クリック->HotaruInventrySystemに追加", new GUIStyle(GUI.skin.label) { wordWrap = true });
                }
                return;
            }

            // リストの各要素を表示・編集
            for (int i = 0; i < targetMenu.groups.Count; i++)
            {
                using (new EditorGUI.IndentLevelScope())
                {
                    DrawGroup(targetMenu.groups[i]);
                }
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

                GUIStyle centerStyle = new GUIStyle(GUI.skin.label);
                centerStyle.alignment = TextAnchor.MiddleCenter;

                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    GUILayout.Space(10);
                    using (new EditorGUILayout.HorizontalScope())
                    {
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
                    }

                    GUILayout.Space(3);

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        GUILayout.Space(10);
                        GUILayout.Label("ワールド移動時に状態を保存する", new GUIStyle(GUI.skin.label));
                        group.isSaved = EditorGUILayout.Toggle("", group.isSaved);
                    }

                    GUILayout.Space(5);

                    //見出し
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        GUILayout.Label("対象オブジェクト", GUILayout.Width(objectFieldWidth));
                        GUILayout.Label("", centerStyle, GUILayout.Width(defaultStateToggleWidth));
                        GUILayout.Label("初期状態で", centerStyle, GUILayout.Width(defaultLabelWidth));
                        GUILayout.Label("ボタンを押すと", centerStyle, GUILayout.Width(animatedLabelWidth));
                    }
                    for (int i = 0; i < group.objects.Count; i++)
                    {
                    using (new EditorGUILayout.HorizontalScope())
                    {
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
                    }

                        GUILayout.Space(10);
                    }
                }
            }
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                GUIStyle style = new GUIStyle(GUI.skin.label); style.wordWrap = true;
                EditorGUILayout.LabelField("グループの追加は、オブジェクトを(複数)選択して\n右クリック->HotaruInventrySystemに追加", style);
            }
            
            GUILayout.EndVertical();
        }
    }

}
#endif