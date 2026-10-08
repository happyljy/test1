using UnityEngine;

namespace PlaneWar
{
    public class PropSpawner : MonoBehaviour
    {
        public static PropSpawner Instance { get; private set; }

        [SerializeField] private GameObject[] propPrefabs;
        [SerializeField] private float minInterval = 9f;
        [SerializeField] private float maxInterval = 15f;
        [SerializeField] private float spawnY = 6.8f;
        [SerializeField] private float spawnXRange = 2.4f;

        private bool _running;
        private float _timer;

        void Awake() => Instance = this;

        public void Begin()
        {
            _running = true;
            _timer = Random.Range(minInterval, maxInterval);
        }

        public void Stop() => _running = false;

        void Update()
        {
            if (!_running) return;
            if (GameManager.Instance == null || !GameManager.Instance.IsPlaying) return;
            if (Time.deltaTime <= 0f) return;

            _timer -= Time.deltaTime;
            if (_timer > 0f) return;

            _timer = Random.Range(minInterval, maxInterval);
            SpawnAt(new Vector3(Random.Range(-spawnXRange, spawnXRange), spawnY, 0f));
        }

        public void SpawnRandomAt(Vector3 position) => SpawnAt(position);

        private void SpawnAt(Vector3 position)
        {
            if (propPrefabs == null || propPrefabs.Length == 0) return;

            var prefab = propPrefabs[Random.Range(0, propPrefabs.Length)];
            if (prefab == null) return;

            Pool.Instance.Spawn(prefab, position, Quaternion.identity);
        }
    }
}