using System.Collections.Generic;
using UnityEngine;

namespace PlaneWar
{
    public class Pool : MonoBehaviour
    {
        public static Pool Instance { get; private set; }

        [System.Serializable]
        public class Entry
        {
            public GameObject prefab;
            [Min(0)] public int prewarm = 16;
        }

        [SerializeField] private Entry[] entries;

        private readonly Dictionary<GameObject, Queue<GameObject>> _pools = new();
        private readonly Dictionary<GameObject, GameObject> _sourceOf = new();

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            foreach (var e in entries)
            {
                if (e.prefab == null) continue;
                var q = new Queue<GameObject>();
                for (int i = 0; i < e.prewarm; i++) q.Enqueue(CreateInstance(e.prefab));
                _pools[e.prefab] = q;
            }
        }

        private GameObject CreateInstance(GameObject prefab)
        {
            var go = Instantiate(prefab, transform);
            go.SetActive(false);
            _sourceOf[go] = prefab;
            return go;
        }

        public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            if (prefab == null) return null;

            if (!_pools.TryGetValue(prefab, out var q))
            {
                q = new Queue<GameObject>();
                _pools[prefab] = q;
            }

            var go = q.Count > 0 ? q.Dequeue() : CreateInstance(prefab);
            go.transform.SetPositionAndRotation(position, rotation);
            go.SetActive(true);
            return go;
        }

        public void Despawn(GameObject go)
        {
            if (go == null) return;
            if (!go.activeSelf) return;
            if (!_sourceOf.TryGetValue(go, out var prefab)) { Destroy(go); return; }

            go.SetActive(false);
            _pools[prefab].Enqueue(go);
        }
    }
}