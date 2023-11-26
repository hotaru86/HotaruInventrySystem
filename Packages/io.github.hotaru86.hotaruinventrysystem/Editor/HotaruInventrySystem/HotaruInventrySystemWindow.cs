/*
Copyright (c) 2023 hotaru86
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace hotarunohikari.HotaruInventrySystem
{
    using nadena.dev.modular_avatar.core;
    using System;
    using System.Collections.Immutable;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using UnityEditor;
    using UnityEditor.Animations;
    using UnityEditor.Experimental.TerrainAPI;
    using UnityEditor.PackageManager.UI;
    using UnityEngine;
    using VRC.Core;
    using VRC.SDK3.Avatars.Components;
    using VRC.SDK3.Avatars.ScriptableObjects;

    public class HotaruInventrySystemWindow : EditorWindow
    {
        // ゲームオブジェクトグループを表すクラス
        [System.Serializable]
        public class GameObjectGroup
        {
            public string groupName;
            public GameObject[] objects;
            public bool defaultState = true;
        }
        private static VRCAvatarDescriptor _targetAvatar = null;
        private static VRCAvatarDescriptor targetAvatar
        {
            get { return _targetAvatar; }
            set
            {
                if(_targetAvatar != value)
                {
                    OnTargetAvatarChanged(targetAvatar, value);
                }
                _targetAvatar = value;
            }
        }

        public static void OnTargetAvatarChanged(VRCAvatarDescriptor oldTarget, VRCAvatarDescriptor newTarget)
        {
            objectGroups.Clear();
        }

        private static List<GameObjectGroup> objectGroups = new List<GameObjectGroup>();

        static string mainPath = "Assets/HotaruInventrySystem";
        static string assetsPath = mainPath + "/Generated";
        static string appDisplayName = "Hotaru Inventry System";
        static string targetAvatarDisplayName = "導入対象アバター";

        [MenuItem("Tools/HotaruInventrySystem")]
        public static void OpenWindow()
        {
            HotaruInventrySystemWindow window = GetWindow<HotaruInventrySystemWindow>();
            window.titleContent = new GUIContent(appDisplayName);
            window.Show();

            VRCAvatarDescriptor[] avatars = FindObjectsOfType<VRCAvatarDescriptor>();
            if(avatars.Length == 1)
            {
                targetAvatar = avatars[0];
            }
        }

        public static void SetTargetAvatarAutomatically()
        {
            //シーン内に1アバターならそれをtargetAvatarとする。
            //複数いるなら、選択中オブジェクトの親にあるアバターを選択
            if (!SetTargetAvatarWithOnlyOneAvatarInScene())
            {
                SetTargetAvatarWithParentOfSelectingObject();
            }

        }
        public static bool SetTargetAvatarWithOnlyOneAvatarInScene()
        {
            //Sceneに1つしかアバターがいなければそれを代入
            VRCAvatarDescriptor[] avatars = FindObjectsOfType<VRCAvatarDescriptor>();
            if (avatars.Length == 1)
            {
                targetAvatar = avatars[0];
                return true;
            }
            return false;
        }
        public static bool SetTargetAvatarWithParentOfSelectingObject()
        {
            VRCAvatarDescriptor parent = Selection.gameObjects[0].transform.GetComponentInParent<VRCAvatarDescriptor>();
            if (parent != null)
            {
                targetAvatar = parent;
                return true;
            }
            return false;
        }

        public static void AddGameObjectAsNewGroup()
        {
            //targetAvatarがnullのとき
            if (targetAvatar == null)
            {
                SetTargetAvatarAutomatically();
            }
            //それでもnullなら、警告を出す
            if (targetAvatar == null)
            {
                EditorUtility.DisplayDialog(appDisplayName, $"先に{targetAvatarDisplayName}を選択してください。", "OK");
                OpenWindow();
                return;
            }

            GameObjectGroup group = new GameObjectGroup();
            group.groupName = Selection.activeGameObject.name;
            group.objects = Selection.gameObjects;

            foreach(GameObject obj in group.objects)
            {
                //アバターの子でないオブジェクトが含まれていたらキャンセル
                if (!IsChildOfTargetAvatar(obj))
                {
                    EditorUtility.DisplayDialog(appDisplayName, "アバターの子でないオブジェクトは追加できません。", "OK");
                    return;
                }
            }
            objectGroups.Add(group);
            OpenWindow();
        }
        private Vector2 scrollPosition = Vector2.zero;
        private void OnGUI()
        {
            var titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 20,
            };
            GUILayout.Label(appDisplayName, titleStyle);
            GUILayout.Space(5);

            if (targetAvatar == null)
                SetTargetAvatarWithOnlyOneAvatarInScene();

            targetAvatar = EditorGUILayout.ObjectField(targetAvatarDisplayName, targetAvatar, typeof(VRCAvatarDescriptor), true) as VRCAvatarDescriptor;
            GUILayout.Space(5);
            GUILayout.Label("オブジェクトグループ", EditorStyles.boldLabel);
            //スクロール可能領域開始
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            // ゲームオブジェクトグループを表示
            for (int i = 0; i < objectGroups.Count; i++)
            {
                GUILayout.BeginVertical(EditorStyles.helpBox);

                objectGroups[i].groupName = EditorGUILayout.TextField("グループ名", objectGroups[i].groupName);

                // グループ内のオブジェクトを表示
                for (int j = 0; j < objectGroups[i].objects.Length; j++)
                {
                    objectGroups[i].objects[j] = (GameObject)EditorGUILayout.ObjectField("オブジェクト " + (j + 1), objectGroups[i].objects[j], typeof(GameObject), true);
                }

                objectGroups[i].defaultState = EditorGUILayout.Toggle("デフォルトでオンにする", objectGroups[i].defaultState);
                // グループ削除ボタン
                if (GUILayout.Button("このグループを削除"))
                {
                    objectGroups.RemoveAt(i);
                    return;
                }

                GUILayout.EndVertical();
                GUILayout.Space(5);
            }
            //スクロール可能領域終了
            GUILayout.EndScrollView();
            GUILayout.Space(10);
            // アニメーション作成ボタン
            if (GUILayout.Button("アニメーションを作成"))
            {
                //アニメーション保存先のフォルダがなければ作成
                if(!Directory.Exists(assetsPath))
                    Directory.CreateDirectory(assetsPath);
                if (isExistDoubledGroupName())
                {
                    EditorUtility.DisplayDialog(appDisplayName, $"複数の異なるグループに同一の名前が設定されています。\n各グループには異なる名前を登録してください。", "OK");
                    return;
                }
                CreateAnimations();
            }
            GUILayout.Space(10);
        }

        static string lastTimeStamp = "";
        static string timeStampedPath = "";
        private void CreateAnimations()
        {
            lastTimeStamp = GetTimeStamp();
            timeStampedPath = $"{assetsPath}/{lastTimeStamp}";
            if (!Directory.Exists(timeStampedPath))
                Directory.CreateDirectory(timeStampedPath);
            // AnimatorControllerの作成
            string timeStamp = GetTimeStamp();
            AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath($"{timeStampedPath}/{lastTimeStamp}_{targetAvatar.name}.controller");
            //一旦全レイヤーを削除
            for(int i=0; i<controller.layers.Length; i++)
            {
                controller.RemoveLayer(i);
            }
            for(int i=0; i<objectGroups.Count; i++)
            {
                var group = objectGroups[i];
                //現在のオブジェクトグループを扱うレイヤー
                
                //ステートマシンの作成
                AnimatorStateMachine stateMachine = new AnimatorStateMachine();
                stateMachine.name = group.groupName;
                //いる？
                //stateMachine.hideFlags = HideFlags.HideInHierarchy;
                //レイヤーの作成
                AnimatorControllerLayer layer = new AnimatorControllerLayer
                {
                    name = group.groupName,
                    defaultWeight = 1,
                    stateMachine = stateMachine
                };
                controller.AddLayer(layer);

                // パラメータの作成
                AnimatorControllerParameter onParameter = new AnimatorControllerParameter
                {
                    name = GetParameterNameFromGroupName(group.groupName),
                    type = AnimatorControllerParameterType.Bool,
                    defaultBool = group.defaultState
                };
                controller.AddParameter(onParameter);

                // ステートの作成
                AnimatorState onState = layer.stateMachine.AddState(group.groupName + "_ON");
                onState.motion = CreateAnimationClip(group, true);
                AnimatorState offState = layer.stateMachine.AddState(group.groupName + "_OFF");
                offState.motion = CreateAnimationClip(group, false);

                // トランジションの作成
                AnimatorStateTransition onToOffTrans = onState.AddTransition(offState);
                onToOffTrans.AddCondition(AnimatorConditionMode.IfNot, 0, onParameter.name);
                onToOffTrans.hasExitTime = false;
                onToOffTrans.exitTime = 0;
                onToOffTrans.duration = 0;
                AnimatorStateTransition offToOnTrans = offState.AddTransition(onState);
                offToOnTrans.AddCondition(AnimatorConditionMode.If, 0, onParameter.name);
                offToOnTrans.hasExitTime = false;
                offToOnTrans.exitTime = 0;
                offToOnTrans.duration = 0;
            }
            AssetDatabase.Refresh();

            //MAとExpressionMenuの設定
            //新規オブジェクトをアバターの子に置き、そこにComponentを追加していく
            GameObject targetObj = new GameObject($"{lastTimeStamp}_HotaruInventrySystem");
            targetObj.transform.parent = targetAvatar.transform;
            var MAParam = targetObj.AddComponent<ModularAvatarParameters>();

            VRCExpressionsMenu rootExMenu = new VRCExpressionsMenu();
            VRCExpressionsMenu subExMenu = new VRCExpressionsMenu();

            rootExMenu.controls.Add(new VRCExpressionsMenu.Control()
            {
                name = "HotaruInventrySystem",
                type = VRCExpressionsMenu.Control.ControlType.SubMenu,
                parameter = new VRCExpressionsMenu.Control.Parameter(),
                subMenu = subExMenu,
                subParameters = Array.Empty<VRCExpressionsMenu.Control.Parameter>()
            });

            for (int i = 0; i< objectGroups.Count; i++)
            {
                var group = objectGroups[i];
                subExMenu.controls.Add(new VRCExpressionsMenu.Control()
                {
                    name = group.groupName,
                    type = VRCExpressionsMenu.Control.ControlType.Toggle,
                    parameter = new VRCExpressionsMenu.Control.Parameter() { name = GetParameterNameFromGroupName(group.groupName)},
                    value = 1.0f,
                    subParameters = Array.Empty<VRCExpressionsMenu.Control.Parameter>()
                });

                MAParam.parameters.Add(new ParameterConfig()
                {
                    nameOrPrefix = GetParameterNameFromGroupName(group.groupName),
                    syncType = ParameterSyncType.Bool,
                    defaultValue = group.defaultState ? 1 : 0,
                    saved = true,
                    internalParameter = true,
                });
            }

            AssetDatabase.CreateAsset(rootExMenu, $"{timeStampedPath}/{lastTimeStamp}_rootExMenu.asset");
            AssetDatabase.CreateAsset(subExMenu, $"{timeStampedPath}/{lastTimeStamp}_subExMenu.asset");

            //MergeAnimatorの設定
            var MAMergeAnim = targetObj.AddComponent<ModularAvatarMergeAnimator>();
            MAMergeAnim.animator = controller;
            MAMergeAnim.layerType = VRCAvatarDescriptor.AnimLayerType.FX;
            MAMergeAnim.pathMode = MergeAnimatorPathMode.Absolute;
            MAMergeAnim.matchAvatarWriteDefaults = false;

            //MenuInstallerの設定
            var MAMenuInst = targetObj.AddComponent<ModularAvatarMenuInstaller>();
            MAMenuInst.menuToAppend = rootExMenu;
        }

        private AnimationClip CreateAnimationClip(GameObjectGroup group, bool state)
        {
            AnimationClip clip = new AnimationClip();
            clip.name = $"{group.groupName}_{(state ? "ON" : "OFF")}";

            //グループ内の各オブジェクトに対して、キーフレームを作成し、1つのクリップにする
            foreach(GameObject obj in group.objects)
            {
                string relativePath = GetRelativePathFromParent(targetAvatar.transform, obj.transform);
                var curve = new AnimationCurve();
                curve.AddKey(0f, state ? 1 : 0);
                curve.AddKey(1f / clip.frameRate, state ? 1 : 0);
                clip.SetCurve(relativePath, typeof(GameObject), "m_IsActive", curve);
            }
            AssetDatabase.CreateAsset(clip, $"{timeStampedPath}/{lastTimeStamp}_{clip.name}.anim");

            return clip;
        }

        //引数のオブジェクトが、すでにobjectGroupsのどれかに含まれているかを返す
        public static bool IsContainGameObjectInGroup(GameObject obj)
        {
            if(obj == null) return false;
            foreach(var group in objectGroups)
            {
                foreach(var item in group.objects)
                {
                    if(item == obj) return true;
                }
            }
            return false;
        }

        //引数のオブジェクトが、targetAvatarの子オブジェクトかどうかを返す
        public static bool IsChildOfTargetAvatar(GameObject obj)
        {
            return obj.transform.IsChildOf(targetAvatar.transform);
        }


        public static string GetRelativePathFromParent(Transform parent, Transform target)
        {
            if (target == null || parent == null) return "";
            if (!target.IsChildOf(parent)) return "";
            string result = "";
            while (target.name != parent.name)
            {
                result = target.name + "/" + result;
                target = target.parent;
            }
            return result.TrimEnd('/');
        }
        public static string GetTimeStamp()
        {
            return DateTime.Now.ToString("yyyy-MM-dd-HHmmss");
        }

        static string parameterNamePrefix = "HIS_";
        static string parameterNameSuffix = "_ON";
        public static string GetParameterNameFromGroupName(string groupName)
        {
            return parameterNamePrefix + groupName + parameterNameSuffix;
        }

        public static bool isExistDoubledGroupName()
        {
            HashSet<string> set = new HashSet<string>();
            foreach (GameObjectGroup group in objectGroups)
                set.Add(group.groupName);
            if (set.Count == objectGroups.Count)
                return false;
            return true;
        }
    }
}
