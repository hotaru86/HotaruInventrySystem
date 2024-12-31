/*
Copyright (c) 2024 hotaru86
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/
#if UNITY_EDITOR
using nadena.dev.modular_avatar.core;
using nadena.dev.ndmf;
using UnityEngine;
using System;
using UnityEditor.Animations;
using UnityEditor;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using System.IO;
using hotarunohikari.HotaruInventrySystem.Component;
using hotarunohikari.HotaruInventrySystem.Editor;
using UnityEditor.Build.Reporting;
using UnityEditor.Build;
using UnityEditor.Callbacks;

[assembly: ExportsPlugin(typeof(HotaruInventrySystemPlugin))]
namespace hotarunohikari.HotaruInventrySystem.Editor
{
    public class HotaruInventrySystemPlugin : Plugin<HotaruInventrySystemPlugin>
    {
        public override string QualifiedName => "io.github.hotaru86.hotaruinventrysystem";
        public override string DisplayName => "Hotaru Inventry System";
        protected override void Configure()
        {
            InPhase(BuildPhase.Generating).BeforePlugin("nadena.dev.modular-avatar").Run("io.github.hotaru86.hotaruinventrysystem", ctx =>
            {
                var targetMenu = ctx.AvatarRootTransform.GetComponentInChildren<HotaruInventrySystem_Menu>();
                if (targetMenu != null)
                {
                    targetMenu.PreProcess();
                    SetupAnimationsToAvatar(targetMenu);
                }

            });
        }

        static string mainPath = "Assets/HotaruInventrySystem";
        static string assetsPath = mainPath + "/tmp";

        static string timeStampedPath = "";
        private void SetupAnimationsToAvatar(HotaruInventrySystem_Menu targetMenu)
        {
            //�A�j���[�V�����ۑ���̃t�H���_���Ȃ���΍쐬
            if (!Directory.Exists(assetsPath))
                Directory.CreateDirectory(assetsPath);

            string timeStamp = DateTime.Now.ToString("yyyy-MM-dd-HHmmss");
            timeStampedPath = $"{assetsPath}/{timeStamp}";
            if (!Directory.Exists(timeStampedPath))
                Directory.CreateDirectory(timeStampedPath);
            Debug.Log("HIS_dayo1");
            // AnimatorController�̍쐬
            Debug.Log($"targetMenu:{targetMenu != null}");
            Debug.Log($"targetMenu.targetAvatar:{targetMenu.targetAvatar != null}");
            AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath($"{timeStampedPath}/{timeStamp}_{targetMenu.targetAvatar.name}.controller");
            Debug.Log("HIS_dayo2");
            //��U�S���C���[���폜
            for (int i = 0; i < controller.layers.Length; i++)
            {
                controller.RemoveLayer(i);
            }
            Debug.Log("HIS_dayo3");
            for (int i = 0; i < targetMenu.groups.Count; i++)
            {
                var group = targetMenu.groups[i];
                //���݂̃I�u�W�F�N�g�O���[�v���������C���[

                //�X�e�[�g�}�V���̍쐬
                AnimatorStateMachine stateMachine = new AnimatorStateMachine();
                stateMachine.name = group.groupName;
                //����H
                //stateMachine.hideFlags = HideFlags.HideInHierarchy;
                //���C���[�̍쐬
                AnimatorControllerLayer layer = new AnimatorControllerLayer
                {
                    name = group.groupName,
                    defaultWeight = 1,
                    stateMachine = stateMachine
                };
                controller.AddLayer(layer);

                AssetDatabase.AddObjectToAsset(layer.stateMachine, controller);
                EditorUtility.SetDirty(layer.stateMachine);

                // �p�����[�^�̍쐬
                AnimatorControllerParameter onParameter = new AnimatorControllerParameter
                {
                    name = GetParameterNameFromGroupName(group.groupName),
                    type = AnimatorControllerParameterType.Bool,
                    defaultBool = false,
                };
                controller.AddParameter(onParameter);

                // �X�e�[�g�̍쐬
                AnimatorState defaultState = layer.stateMachine.AddState(group.groupName + "_Default");
                defaultState.motion = CreateAnimationClip(group, true);
                EditorUtility.SetDirty(defaultState);

                AnimatorState animatedState = layer.stateMachine.AddState(group.groupName + "_Animated");
                animatedState.motion = CreateAnimationClip(group, false);
                EditorUtility.SetDirty(animatedState);

                // �g�����W�V�����̍쐬
                AnimatorStateTransition defToAnimTrans = defaultState.AddTransition(animatedState);
                defToAnimTrans.AddCondition(AnimatorConditionMode.If, 0, onParameter.name);
                defToAnimTrans.hasExitTime = false;
                defToAnimTrans.exitTime = 0;
                defToAnimTrans.duration = 0;
                EditorUtility.SetDirty(defToAnimTrans);

                AnimatorStateTransition animToDefTrans = animatedState.AddTransition(defaultState);
                animToDefTrans.AddCondition(AnimatorConditionMode.IfNot, 0, onParameter.name);
                animToDefTrans.hasExitTime = false;
                animToDefTrans.exitTime = 0;
                animToDefTrans.duration = 0;
                EditorUtility.SetDirty(animToDefTrans);
            }

            EditorUtility.SetDirty(controller);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            //MA��ExpressionMenu�̐ݒ�
            //�V�K�I�u�W�F�N�g���A�o�^�[�̎q�ɒu���A������Component��ǉ����Ă���
            GameObject targetObj = new GameObject($"{timeStamp}_HotaruInventrySystem");
            targetObj.transform.parent = targetMenu.targetAvatar.transform;
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

            for (int i = 0; i < targetMenu.groups.Count; i++)
            {
                var group = targetMenu.groups[i];
                subExMenu.controls.Add(new VRCExpressionsMenu.Control()
                {
                    name = group.groupName,
                    type = VRCExpressionsMenu.Control.ControlType.Toggle,
                    parameter = new VRCExpressionsMenu.Control.Parameter() { name = GetParameterNameFromGroupName(group.groupName) },
                    value = 1.0f,
                    subParameters = Array.Empty<VRCExpressionsMenu.Control.Parameter>(),
                });

                MAParam.parameters.Add(new ParameterConfig()
                {
                    nameOrPrefix = GetParameterNameFromGroupName(group.groupName),
                    syncType = ParameterSyncType.Bool,
                    //�����l!
                    defaultValue = 0,
                    saved = group.isSaved,
                    internalParameter = true,
                });
            }

            AssetDatabase.CreateAsset(rootExMenu, $"{timeStampedPath}/{timeStamp}_rootExMenu.asset");
            AssetDatabase.CreateAsset(subExMenu, $"{timeStampedPath}/{timeStamp}_subExMenu.asset");

            EditorUtility.SetDirty(rootExMenu);
            EditorUtility.SetDirty(subExMenu);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();


            //MergeAnimator�̐ݒ�
            var MAMergeAnim = targetObj.AddComponent<ModularAvatarMergeAnimator>();
            MAMergeAnim.animator = controller;
            MAMergeAnim.layerType = VRCAvatarDescriptor.AnimLayerType.FX;
            MAMergeAnim.pathMode = MergeAnimatorPathMode.Absolute;
            MAMergeAnim.matchAvatarWriteDefaults = false;

            //MenuInstaller�̐ݒ�
            var MAMenuInst = targetObj.AddComponent<ModularAvatarMenuInstaller>();
            MAMenuInst.menuToAppend = rootExMenu;

            EditorUtility.SetDirty(MAMergeAnim);
            EditorUtility.SetDirty(MAMenuInst);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();


            AnimationClip CreateAnimationClip(HISGroup group, bool isDefault)
            {
                AnimationClip clip = new AnimationClip();
                clip.name = $"{group.groupName}_{(isDefault ? "Default" : "Animated")}";

                //�O���[�v���̊e�I�u�W�F�N�g�ɑ΂��āA�L�[�t���[�����쐬���A1�̃N���b�v�ɂ���
                for (int i = 0; i < group.objects.Count; i++)
                {
                    GameObject obj = group.objects[i];
                    string relativePath = GetRelativePathFromParent(targetMenu.targetAvatar.transform, obj.transform);
                    var curve = new AnimationCurve();
                    bool state = !(group.defaultState[i] ^ isDefault);
                    curve.AddKey(0f, state ? 1 : 0);
                    curve.AddKey(1f / clip.frameRate, state ? 1 : 0);
                    clip.SetCurve(relativePath, typeof(GameObject), "m_IsActive", curve);
                }
                AssetDatabase.CreateAsset(clip, $"{timeStampedPath}/{timeStamp}_{clip.name}.anim");

                return clip;
            }
        }


        public static void DeleteTmpAssets()
        {
            if (Directory.Exists(assetsPath))
            {
                Directory.Delete(assetsPath, true);
                AssetDatabase.Refresh();
                Debug.Log("HIS_�t�H���_�폜");
            }
        }

        static string parameterNamePrefix = "HIS_";
        static string parameterNameSuffix = "_ON";
        public static string GetParameterNameFromGroupName(string groupName)
        {
            return parameterNamePrefix + groupName + parameterNameSuffix;
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
    }

    [InitializeOnLoad]
    public class ProcessEndHandler : IPostprocessBuildWithReport
    {
        // �r���h�I�����̗D��x
        public int callbackOrder => 10;

        static ProcessEndHandler()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                // Play���[�h�I����ɏ���
                HotaruInventrySystemPlugin.DeleteTmpAssets();
            }
        }

        //���삵�Ă��Ȃ�
        public void OnPostprocessBuild(BuildReport report)
        {
            // �r���h�I����̏�����ɏ���
            HotaruInventrySystemPlugin.DeleteTmpAssets();
        }
    }
}

#endif