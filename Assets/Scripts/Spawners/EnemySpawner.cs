using UnityEngine;

namespace PlaneWar
{
    public class EnemySpawner : MonoBehaviour
    {
        [Header("预制体")]
        [SerializeField] private GameObject tankPrefab;
        [SerializeField] private GameObject planePrefab;

        [Header("节奏")]
        [SerializeField] private float startInterval = 1.6f;
        [SerializeField] private float minInterval = 0.55f;
        [SerializeField] private float difficultyRamp = 50f;
        [SerializeField] private float firstDelay = 0.8f;

        [Header("位置")]
        [SerializeField] private float spawnY = 7.2f;
        [SerializeField] private float spawnXRange = 2.6f;

        private bool _running;
        private float _elapsed;
        private float _timer;

        public void Begin()
        {
            _running = true;
            _elapsed = 0f;
            _timer = firstDelay;
        }

        public void Stop() => _running = false;

        void Update()
        {
            if (!_running) return;
            if (GameManager.Instance == null || !GameManager.Instance.IsPlaying) return;
            if (Time.deltaTime <= 0f) return;

            _elapsed += Time.deltaTime;
            _timer -= Time.deltaTime;
            if (_timer > 0f) return;

            float d = Mathf.Clamp01(_elapsed / difficultyRamp);
            _timer = Mathf.Lerp(startInterval, minInterval, d);

            SpawnOne(d);
        }

        private void SpawnOne(float difficulty)
        {
            bool isTank = Random.value < Mathf.Lerp(0.15f, 0.32f, difficulty);
            var prefab = isTank ? tankPrefab : planePrefab;
            if (prefab == null) return;

            float x = Random.Range(-spawnXRange, spawnXRange);
            Pool.Instance.Spawn(prefab, new Vector3(x, spawnY, 0f), Quaternion.identity);
        }
    }
}