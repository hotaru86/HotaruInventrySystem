/*
Copyright (c) 2024 hotaru86
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/
using System.Collections.Generic;
using System;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDKBase;
using UnityEditor;
using System.Linq;

namespace hotarunohikari.HotaruInventrySystem.Component
{
    [Serializable]
    public class HISGroup
    {
        public string groupName;
        public List<GameObject> objects;
        public List<bool> defaultState;
        public bool isSaved = true;

        public void RemoveObjectAt(int index)
        {
            objects.RemoveAt(index);
            defaultState.RemoveAt(index);
        }

        public void RemoveObject(GameObject objToRemove)
        {
            int index = objects.IndexOf(objToRemove);
            if(index >= 0 && index < objects.Count)
            {
                objects.RemoveAt(index);
                defaultState.RemoveAt(index);
            }
        }
    }
    public class HotaruInventrySystem_Menu : MonoBehaviour, IEditorOnly
    {
        private VRCAvatarDescriptor _targetAvatar = null;
        public VRCAvatarDescriptor targetAvatar
        {
            get
            {
                _targetAvatar = GetComponentInParent<VRCAvatarDescriptor>();
                return _targetAvatar;
            }
            set
            {
                //何かが代入されても、常に親をtargetAvatarに保持する(再取得)
                _targetAvatar = GetComponentInParent<VRCAvatarDescriptor>();
            }
        }
        
        public List<HISGroup> groups = new List<HISGroup>();

        public void AddGroup(HISGroup newGroup)
        {
            groups.Add(newGroup);
        }


        public void RemoveGroupAt(int index)
        {
            groups.RemoveAt(index);
        }

        public void RemoveGroup(HISGroup groupToRemove)
        {
            groups.Remove(groupToRemove);
        }

        public void PreProcess()
        {
            CheckAndDeleteNull();
            UniqueifyGroupName();
        }

        void CheckAndDeleteNull()
        {
            List<int> indexToDelete = new List<int>();

            //group内にnullオブジェクトがあれば削除
            foreach (HISGroup group in groups)
            {
                if (group == null) continue;
                for(int i=0; i<group.objects.Count; i++)
                {
                    if(group.objects[i] == null)
                    {
                        indexToDelete.Insert(0,i);
                    }
                }
                foreach(int i in indexToDelete)
                {
                    group.RemoveObjectAt(i);
                }
            }

            //オブジェクトの入っていないグループがあれば削除
            indexToDelete = new List<int>();
            for(int i=0; i<groups.Count; i++)
            {
                if (groups[i].objects.Count==0)
                {
                    indexToDelete.Insert(0, i);
                }
            }
            foreach(int i in indexToDelete)
            {
                RemoveGroupAt(i);
            }
        }

        void UniqueifyGroupName()
        {
            Dictionary<string, int> nameCount = new Dictionary<string, int>();
            for(int i=0; i< groups.Count; i++)
            {
                string str = groups[i].groupName;
                if (!nameCount.ContainsKey(str))
                {
                    nameCount[str] = 1;
                }
                else
                {
                    string uniqueStr;
                    do
                    {
                        uniqueStr = $"{str}_{nameCount[str]}";
                    } while (nameCount.ContainsKey(uniqueStr));

                    //ユニークな名前で上書き
                    groups[i].groupName = uniqueStr;
                    nameCount[uniqueStr] = 1;
                }
            }
        }

        //コンポーネントアタッチ時にtargetAvatarを設定
        private void Reset()
        {
            targetAvatar = transform.GetComponentInParent<VRCAvatarDescriptor>();
        }
    }

}
