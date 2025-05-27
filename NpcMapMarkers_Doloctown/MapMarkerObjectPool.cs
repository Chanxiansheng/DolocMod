using DolocTown.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;

namespace NpcMapMarkers_Doloctown
{
    internal class MapMarkerObjectPool:MonoBehaviour
    {
        public static MapMarkerObjectPool Instance { get; private set; }

        private ObjectPool<DolocNavigationButton> _pool;

        private readonly HashSet<DolocNavigationButton> _active = new HashSet<DolocNavigationButton>();

        private DolocNavigationButton _prefab;
        private Transform _parent;

        void Awake()
        {
            Instance = this;
        }
        public void Init(GameObject prefabObj, Transform parent)
        {
            _prefab = prefabObj.GetComponent<DolocNavigationButton>();
            _parent = parent;

            _pool = new ObjectPool<DolocNavigationButton>(
                createFunc: () =>
                {
                    var go = Instantiate(prefabObj, _parent);
                    return go.GetComponent<DolocNavigationButton>();
                },
                actionOnGet: mapMarker =>
                {
                    mapMarker.gameObject.SetActive(true);
                },
                actionOnRelease: mapMarker =>
                {
                    // 源码中的 reset 逻辑
                    mapMarker.iconMaterial = LocMaterials.UI_MAT_DEFAULT; 
                    mapMarker.gameObject.SetActive(false);
                },
                actionOnDestroy: mapMarker => Destroy(mapMarker.gameObject),

                collectionCheck: false,
                defaultCapacity: 10,
                maxSize: 100
            );
        }

        public DolocNavigationButton Get()
        {
            var mapMarker = _pool.Get();
            _active.Add(mapMarker);
            return mapMarker;
        }

        public void Release(DolocNavigationButton mapMarker)
        {
            if (_active.Remove(mapMarker))
                _pool.Release(mapMarker);
        }

        public void ReleaseAll()
        {
            foreach (var mapMarker in _active)
                _pool.Release(mapMarker);
            _active.Clear();
        }


    }
}
