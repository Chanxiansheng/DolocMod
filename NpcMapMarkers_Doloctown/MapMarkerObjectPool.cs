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
        public DolocNavigationButton markerPrefab;
        public Transform markerParent;

        private ObjectPool<DolocNavigationButton> _markerPool;

        void Awake()
        {
            _markerPool = new ObjectPool<DolocNavigationButton>(
                createFunc: () => Instantiate(markerPrefab, markerParent),

                actionOnGet: (marker) => marker.gameObject.SetActive(true),

                actionOnRelease: (marker) => marker.gameObject.SetActive(false),

                actionOnDestroy: (marker) => Destroy(marker.gameObject),

                collectionCheck: false,
                defaultCapacity: 20,
                maxSize: 100
            );
        }


    }
}
