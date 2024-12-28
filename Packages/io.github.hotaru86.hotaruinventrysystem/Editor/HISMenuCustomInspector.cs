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
            //Play���[�h���Ȃǂ�Avatar���擾�ł��Ȃ��Ȃ����ꍇ�ɉ������Ȃ�
            try
            {
                targetMenu.targetAvatar = EditorGUILayout.ObjectField("�ΏۃA�o�^�[", targetMenu.targetAvatar, typeof(VRCAvatarDescriptor), true) as VRCAvatarDescriptor;
            }catch(System.Exception e)
            {
                return;
            }

            if (targetMenu.groups.Count == 0)
            {
                GUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField("�I�u�W�F�N�g�O���[�v������܂���B\n�O���[�v�̒ǉ��́A�I�u�W�F�N�g��(����)�I������\n�E�N���b�N->HotaruInventrySystem�ɒǉ�", new GUIStyle(GUI.skin.label) { wordWrap = true });
                GUILayout.EndVertical();
                return;
            }

            // ���X�g�̊e�v�f��\���E�ҏW
            for (int i = 0; i < targetMenu.groups.Count; i++)
            {
                EditorGUI.indentLevel++;
                DrawGroup(targetMenu.groups[i]);
                EditorGUI.indentLevel--;
            }

            // �ύX��ۑ�
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
                GUILayout.Label("�O���[�v��", new GUIStyle(GUI.skin.label));
                Vector2 labelSize = new GUIStyle(GUI.skin.label).CalcSize(new GUIContent("�O���[�v��"));

                group.groupName = EditorGUILayout.TextField("", group.groupName, GUILayout.Width(totalWidth * 0.7f - labelSize.x));
                GUILayout.Space(20);
                // �폜�{�^��
                if (GUILayout.Button("�폜", GUILayout.Width(50)))
                {
                    Undo.RecordObject(targetMenu, "Remove HISGroup");
                    targetMenu.RemoveGroup(group);
                    EditorUtility.SetDirty(targetMenu);
                }
                EditorGUILayout.EndHorizontal();



                //���o��
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("�ΏۃI�u�W�F�N�g", GUILayout.Width(objectFieldWidth));
                GUILayout.Label("", centerStyle, GUILayout.Width(defaultStateToggleWidth));
                GUILayout.Label("������Ԃ�", centerStyle, GUILayout.Width(defaultLabelWidth));
                GUILayout.Label("�{�^����������", centerStyle, GUILayout.Width(animatedLabelWidth));
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
                    string defaultText = group.defaultState[i] ? "�\��" : "��\��";
                    GUILayout.Label(defaultText, centerStyle, GUILayout.Width(defaultLabelWidth));
                    string animatedText = !group.defaultState[i] ? "�\��" : "��\��";
                    GUILayout.Label(animatedText, centerStyle, GUILayout.Width(animatedLabelWidth));
                    EditorGUILayout.EndHorizontal();

                    GUILayout.Space(10);
                }
                GUILayout.EndVertical();
            }
            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUIStyle style = new GUIStyle(GUI.skin.label); style.wordWrap = true;
            EditorGUILayout.LabelField("�O���[�v�̒ǉ��́A�I�u�W�F�N�g��(����)�I������\n�E�N���b�N->HotaruInventrySystem�ɒǉ�", style);
            GUILayout.EndVertical();
        }
    }

}
#endif