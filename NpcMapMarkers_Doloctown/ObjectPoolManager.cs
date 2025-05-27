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
                Debug.Log($"[NavigationPool] 重置旧池 panel={panel.GetInstanceID()}");
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

            Debug.LogWarning($"[NavigationPool] 尝试获取未初始化的对象池 panel={panel.GetInstanceID()}");
            return null;
        }

        /// <summary>
        /// 创建一个新的对象池
        /// </summary>
        private static ObjectPool<DolocNavigationButton> CreatePool(CityMapPanel panel)
        {
            var parent = Traverse.Create(panel).Field<Transform>("tipRoot").Value;
            var prefab = DolocAPI.GetAsset<GameObject>(DolocGameAssets.UI_ELEMENT_MAP_MISSION_TIP);
            if (!prefab)
            {
                Debug.LogError("[NavigationPool] 获取 prefab 失败！");
                return null;
            }
            var pool = new ObjectPool<DolocNavigationButton>(prefab, parent, false, false);
            pool.OnRecycle += tip => tip.iconMaterial = LocMaterials.UI_MAT_DEFAULT;
            pool.RecycleAll();
            pool.CheckCount(10);
            Debug.Log($"[NavigationPool] 创建新池 panel={panel.GetInstanceID()}");
            return pool;
        }
    }
}
