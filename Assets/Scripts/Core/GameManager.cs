using System;
using System.Collections;
using UnityEngine;

namespace PlaneWar
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("规则")]
        [SerializeField] private int startLives = 3;
        [SerializeField] private int maxLives = 5;
        [SerializeField] private float invincibleAfterHit = 1.5f;

        [Header("场景引用")]
        [SerializeField] private PlayerController player;
        [SerializeField] private EnemySpawner enemySpawner;
        [SerializeField] private PropSpawner propSpawner;

        public GameState State { get; private set; } = GameState.Ready;
        public int Score { get; private set; }
        public int Lives { get; private set; }
        public int BestScore { get; private set; }
        public bool IsPlaying => State == GameState.Playing;
        public PlayerController Player => player;

        public event Action<int> ScoreChanged;
        public event Action<int> LivesChanged;
        public event Action<GameState> StateChanged;

        private const string BestKey = "PlaneWar.BestScore";
        private Coroutine _freezeRoutine;
        private Coroutine _hitStopRoutine;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            BestScore = PlayerPrefs.GetInt(BestKey, 0);
        }

        void Start()
        {
            Time.timeScale = 1f;
            if (player != null) player.gameObject.SetActive(false);
            enemySpawner?.Stop();
            propSpawner?.Stop();
            ChangeState(GameState.Ready);
        }

        private void ChangeState(GameState next)
        {
            State = next;
            StateChanged?.Invoke(next);
        }

        public void StartGame()
        {
            if (_freezeRoutine != null) { StopCoroutine(_freezeRoutine); _freezeRoutine = null; }
            if (_hitStopRoutine != null) { StopCoroutine(_hitStopRoutine); _hitStopRoutine = null; }
            Time.timeScale = 1f;

            Score = 0;
            Lives = startLives;
            ScoreChanged?.Invoke(Score);
            LivesChanged?.Invoke(Lives);

            ClearField();

            if (player != null) player.ResetPlayer();
            enemySpawner?.Begin();
            propSpawner?.Begin();

            ChangeState(GameState.Playing);
        }

        public void PauseGame()
        {
            if (State != GameState.Playing) return;
            Time.timeScale = 0f;
            ChangeState(GameState.Paused);
        }

        public void ResumeGame()
        {
            if (State != GameState.Paused) return;
            Time.timeScale = 1f;
            ChangeState(GameState.Playing);
        }

        public void BackToMenu()
        {
            if (_freezeRoutine != null) { StopCoroutine(_freezeRoutine); _freezeRoutine = null; }
            if (_hitStopRoutine != null) { StopCoroutine(_hitStopRoutine); _hitStopRoutine = null; }
            Time.timeScale = 1f;

            ClearField();
            enemySpawner?.Stop();
            propSpawner?.Stop();
            if (player != null) player.gameObject.SetActive(false);

            ChangeState(GameState.Ready);
        }

        public void GameOver()
        {
            if (State == GameState.GameOver) return;

            enemySpawner?.Stop();
            propSpawner?.Stop();

            if (player != null)
            {
                FXSpawner.Instance?.SpawnExplosion(player.transform.position, 1.8f);
                player.gameObject.SetActive(false);
            }

            ChangeState(GameState.GameOver);

            Time.timeScale = 0.25f;
            _freezeRoutine = StartCoroutine(FreezeAfterRealSeconds(0.55f));
        }

        private IEnumerator FreezeAfterRealSeconds(float realSeconds)
        {
            float t = 0f;
            while (t < realSeconds)
            {
                t += Time.unscaledDeltaTime;
                yield return null;
            }
            Time.timeScale = 0f;
            _freezeRoutine = null;
        }

        public void AddScore(int amount)
        {
            if (State != GameState.Playing) return;
            Score += amount;
            ScoreChanged?.Invoke(Score);

            if (Score > BestScore)
            {
                BestScore = Score;
                PlayerPrefs.SetInt(BestKey, BestScore);
                PlayerPrefs.Save();
            }
        }

        public void OnPlayerHit()
        {
            if (State != GameState.Playing) return;
            Lives = Mathf.Max(0, Lives - 1);
            LivesChanged?.Invoke(Lives);

            if (Lives <= 0) GameOver();
            else player?.BeginInvincible(invincibleAfterHit);
            CameraShake.Instance?.Shake(0.2f, 0.2f); // 屏幕震动
        }

        public void HealLife()
        {
            if (State != GameState.Playing) return;
            Lives = Mathf.Min(Lives + 1, maxLives);
            LivesChanged?.Invoke(Lives);
        }

        public void UseBomb()
        {
            if (State != GameState.Playing) return;

            var enemies = FindObjectsByType<EnemyBase>(FindObjectsSortMode.None);
            foreach (var e in enemies)
                if (e != null && e.IsAlive) e.TakeDamage(99999);

            FXSpawner.Instance?.SpawnExplosion(Vector3.zero, 6f);
            CameraShake.Instance?.Shake(0.3f, 0.3f); // 炸弹全屏震动
        }

        public void DoHitStop(float duration = 0.04f)
        {
            if (State != GameState.Playing) return; // 只在游戏进行中触发顿帧
            if (_hitStopRoutine != null) StopCoroutine(_hitStopRoutine);
            _hitStopRoutine = StartCoroutine(HitStopRoutine(duration));
        }

        private IEnumerator HitStopRoutine(float duration)
        {
            Time.timeScale = 0.05f; // 顿帧
            yield return new WaitForSecondsRealtime(duration);
            Time.timeScale = 1f;    // 恢复
            _hitStopRoutine = null;
        }

        private void ClearField()
        {
            if (Pool.Instance == null) return;

            foreach (var b in FindObjectsByType<Bullet>(FindObjectsSortMode.None))
                Pool.Instance.Despawn(b.gameObject);

            foreach (var e in FindObjectsByType<EnemyBase>(FindObjectsSortMode.None))
                Pool.Instance.Despawn(e.gameObject);

            foreach (var p in FindObjectsByType<Prop>(FindObjectsSortMode.None))
                Pool.Instance.Despawn(p.gameObject);

            foreach (var fx in FindObjectsByType<Explosion>(FindObjectsSortMode.None))
                Pool.Instance.Despawn(fx.gameObject);
        }
    }
}