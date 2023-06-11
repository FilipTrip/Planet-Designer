using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace EasyPoolKit.Editor
{
    [CustomEditor(typeof(RecyclableGOPoolKit))]
    public class RecyclableGOPoolManagerInspector : UnityEditor.Editor
    {
        private readonly HashSet<string> _poolIdsSet = new HashSet<string>();

        public override void OnInspectorGUI()
        {
            if (!EditorApplication.isPlaying)
            {
                EditorGUILayout.HelpBox("Available only on runtime.", MessageType.Info);
                return;
            }

            var poolMgr = target as RecyclableGOPoolKit;

            if (poolMgr)
            {
                EditorGUILayout.LabelField("RecyclableGOPoolManager");
                
                var poolsInfo = poolMgr.GetPoolsInfo();
                
                EditorGUILayout.LabelField("GameObject Pool Count", poolsInfo.Count.ToString());

                for (int i = 0; i < poolsInfo.Count; i++)
                {
                    var poolInfo = poolsInfo[i];
                    DrawGameObjectPool(poolInfo);
                }
            }

            Repaint();
        }

        private void DrawGameObjectPool(RecyclablePoolInfo poolInfo)
        {
            var ifLastOpened = _poolIdsSet.Contains(poolInfo.PoolId);
            var ifCurOpened = EditorGUILayout.Foldout(ifLastOpened, poolInfo.PoolId);

            if (ifCurOpened != ifLastOpened)
            {
                if (ifCurOpened)
                {
                    _poolIdsSet.Add(poolInfo.PoolId);
                }
                else
                {
                    _poolIdsSet.Remove(poolInfo.PoolId);
                }
            }

            if (ifCurOpened)
            {
                EditorGUILayout.BeginVertical("box");
                {
                    EditorGUILayout.LabelField("PoolId", poolInfo.PoolId);
                    EditorGUILayout.LabelField("ReferenceType", poolInfo.ReferenceType.ToString());
                    EditorGUILayout.LabelField("InitCreateCount", poolInfo.InitCreateCount.HasValue ? poolInfo.InitCreateCount.Value.ToString() : "-");
                    EditorGUILayout.LabelField("ReachMaxLimitType", poolInfo.ReachMaxLimitType.ToString());
                    if (poolInfo.ReachMaxLimitType != PoolReachMaxLimitType.Default)
                    {
                        EditorGUILayout.LabelField("MaxSpawnCount", poolInfo.MaxSpawnCount.HasValue ? poolInfo.MaxSpawnCount.Value.ToString() : "-");
                    }
                    EditorGUILayout.LabelField("DespawnDestroyType", poolInfo.DespawnDestroyType.ToString());
                    if (poolInfo.DespawnDestroyType == PoolDespawnDestroyType.DestroyToLimit)
                    {
                        EditorGUILayout.LabelField("MaxDespawnCount", poolInfo.MaxDespawnCount.HasValue ? poolInfo.MaxDespawnCount.Value.ToString() : "-");
                    }

                    EditorGUILayout.LabelField("ClearType", poolInfo.ClearType.ToString());
                    EditorGUILayout.LabelField("AutoClearTime", poolInfo.AutoClearTime.ToString());
                    EditorGUILayout.LabelField("IfIgnoreTimeScale", poolInfo.IfIgnoreTimeScale.ToString());
                    EditorGUILayout.LabelField("CachedObjectCount", poolInfo.CachedObjectCount.ToString());
                    EditorGUILayout.LabelField("UsedObjectCount", poolInfo.UsedObjectCount.ToString());
                    EditorGUILayout.LabelField("TotalObjectCount", poolInfo.TotalObjectCount.ToString());

                    if (GUILayout.Button("ClearUnusedObjects(Safe)"))
                    {
                        if (poolInfo.ExtraInfo is RecyclableGameObjectPool pool)
                        {
                            pool.ClearUnusedObjects();
                        }
                    }
                    
                    if (GUILayout.Button("ClearPool(Unsafe)"))
                    {
                        if (poolInfo.ExtraInfo is RecyclableGameObjectPool pool)
                        {
                            pool.ClearAll();
                        }
                    }
                    
                    //TODO Draw all Objects
                }
                EditorGUILayout.EndVertical();

                EditorGUILayout.Separator();
            }
        }
    }
}