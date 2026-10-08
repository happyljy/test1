using UnityEngine;

namespace PlaneWar
{
    public class FXSpawner : MonoBehaviour
    {
        public static FXSpawner Instance { get; private set; }

        [SerializeField] private GameObject explosionPrefab;
        [SerializeField] private GameObject hitSparkPrefab;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void SpawnExplosion(Vector3 pos, float scale = 1f)
        {
            if (explosionPrefab == null) return;

            var go = Pool.Instance.Spawn(explosionPrefab, pos, Quaternion.identity);
            var fx = go.GetComponent<Explosion>();
            if (fx != null) fx.SetScaleMultiplier(scale);
        }

        public void SpawnHit(Vector3 pos)
        {
            if (hitSparkPrefab == null) return;
            Pool.Instance.Spawn(hitSparkPrefab, pos, Quaternion.identity);
        }
    }
}