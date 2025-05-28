using DolocTown.UI;
using System.Runtime.CompilerServices;
using RedSaw;
using UnityEngine;
using HarmonyLib;

namespace NpcMapMarkers_Doloctown
{
    public static class ObjectPoolManager
    {
        private static readonly ConditionalWeakTable<CityMapPanel, ObjectPool<DolocNavigationButton>> PoolsWeakTable = new ConditionalWeakTable<CityMapPanel, ObjectPool<DolocNavigationButton>>();

        /// <summary>
        /// 主动初始化/重建对象池。如果存在旧的，会先清空并移除再创建新池。
        /// </summary>
        public static void InitPool(CityMapPanel panel)
        {
            if (PoolsWeakTable.TryGetValue(panel, out var oldPool))
            {
                oldPool.RecycleAll();
                PoolsWeakTable.Remove(panel); // 移除旧引用
                ModLog.Logger.Log($"重置旧池 panel={panel.GetInstanceID()}");
            }
            var newPool = CreatePool(panel);
            PoolsWeakTable.Add(panel, newPool);
        }

        /// <summary>
        /// 获取对象池（如果不存在则返回 null，不会自动创建）
        /// </summary>
        public static ObjectPool<DolocNavigationButton> Get(CityMapPanel panel)
        {
            if (PoolsWeakTable.TryGetValue(panel, out var pool))
                return pool;

            ModLog.Logger.Log($"尝试获取未初始化的对象池 panel={panel.GetInstanceID()}", Debug.LogWarning);
            return null;
        }

        /// <summary>
        /// 创建一个新的对象池
        /// </summary>
        private static ObjectPool<DolocNavigationButton> CreatePool(CityMapPanel panel)
        {
            // 1. 获取预制体
            var prefab = DolocAPI.GetAsset<GameObject>(DolocGameAssets.UI_ELEMENT_MAP_MISSION_TIP);
            if (!prefab)
            {
                ModLog.Logger.Log("获取 prefab 失败！", Debug.LogError);
                return null;
            }

            // 2. 获取根节点
            var tipRoot = Traverse.Create(panel).Field<Transform>("tipRoot").Value;
            var parent = tipRoot.parent; // 获取 tipRoot 的父物体
            Transform markerRoot = parent.Find("markerRoot");
            if (!markerRoot)
            {
                var go = new GameObject("markerRoot");
                markerRoot = go.transform;
                markerRoot.SetParent(parent);

                markerRoot.localPosition = Vector3.zero;
                markerRoot.localRotation = Quaternion.identity;
                markerRoot.localScale = Vector3.one;

            }

            // 保证光标在前
            var cursor = parent.Find("cursor");
            if (cursor != null)
            {
                int cursorIndex = cursor.GetSiblingIndex();
                markerRoot.SetSiblingIndex(cursorIndex);
            }



            var pool = new ObjectPool<DolocNavigationButton>(prefab, markerRoot, false, false);

            pool.OnRecycle += tip => {
                tip.iconMaterial = LocMaterials.UI_MAT_DEFAULT;
                tip.gameObject.SetActive(false); // 回收时隐藏
            };

            pool.RecycleAll();
            pool.CheckCount(10);
            pool.ForEach(tip => tip.gameObject.SetActive(false));

            ModLog.Logger.Log($"创建新池 panel={panel.GetInstanceID()}");
            return pool;
        }
    }
}
