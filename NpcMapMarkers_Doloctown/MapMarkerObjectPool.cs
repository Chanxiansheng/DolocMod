//using DolocTown.UI;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using UnityEngine;
//using UnityEngine.Pool;

//namespace NpcMapMarkers_Doloctown
//{
//    public static class MapMarkerObjectPool
//    {
//        public static ObjectPool<DolocNavigationButton> _pool;

//        public static  readonly HashSet<DolocNavigationButton> _active = new HashSet<DolocNavigationButton>();

//        public static DolocNavigationButton _prefab;
//        public static Transform _parent;

//        public static bool IsInitialized { get; private set; } = false;
//        public static void Init(GameObject prefabObj, Transform parent)
//        {
//            _prefab = prefabObj.GetComponent<DolocNavigationButton>();
//            _parent = parent;

//            _pool = new ObjectPool<DolocNavigationButton>(
//                createFunc: () =>
//                {
//                    var go = UnityEngine.Object.Instantiate(prefabObj, _parent);
//                    return go.GetComponent<DolocNavigationButton>();
//                },
//                actionOnGet: mapMarker =>
//                {
//                    mapMarker.gameObject.SetActive(true);
//                },
//                actionOnRelease: mapMarker =>
//                {
//                    // 源码中的 reset 逻辑
//                    //mapMarker.iconMaterial = LocMaterials.UI_MAT_DEFAULT; 
//                    mapMarker.gameObject.SetActive(false);
//                },
//                actionOnDestroy: mapMarker => UnityEngine.Object.Destroy(mapMarker.gameObject),

//                collectionCheck: false,
//                defaultCapacity: 10,
//                maxSize: 100
//            );

//            IsInitialized = true;  // <- 这里必须设置
//        }

//        public static DolocNavigationButton Get()
//        {
//            var mapMarker = _pool.Get();
//            mapMarker.Init();  // 👈 添加这一行，确保 rectTransform 被赋值
//            _active.Add(mapMarker);
//            return mapMarker;
//        }

//        public static void Release(DolocNavigationButton mapMarker)
//        {
//            if (_active.Remove(mapMarker))
//                _pool.Release(mapMarker);
//        }

//        public static void ReleaseAll()
//        {
//            // 拷贝一份，避免遍历时修改集合
//            var activeCopy = _active.ToList();  
//            foreach (var mapMarker in activeCopy)
//                _pool.Release(mapMarker);
//            _active.Clear();
//        }

//        public static void Clear()
//        {
//            ReleaseAll();
//            _pool?.Clear(); // 若支持
//            _pool = null;
//            _prefab = null;
//            _parent = null;
//            IsInitialized = false;
//        }
//    }
//}
