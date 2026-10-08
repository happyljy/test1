using UnityEngine;
using UnityEngine.UI;

namespace PlaneWar
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("面板")]
        [SerializeField] private GameObject startPanel;
        [SerializeField] private GameObject hudPanel;
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private GameObject gameOverPanel;

        [Header("HUD - 像素数字组件")]
        [SerializeField] private PixelNumberDisplay scoreDisplay;  // 分数
        [SerializeField] private PixelNumberDisplay lifeDisplay;   // 生命
        [SerializeField] private PixelNumberDisplay shieldDisplay; // 护盾倒计时

        [Header("HUD - 其它")]
        [SerializeField] private Image[] lifeIcons;               // 生命图标（可选，与数字共存）
        [SerializeField] private GameObject shieldTimerRoot;      // 护盾显示根节点

        [Header("结算 - 像素数字组件")]
        [SerializeField] private PixelNumberDisplay finalScoreDisplay;
        [SerializeField] private PixelNumberDisplay finalBestDisplay;
        [SerializeField] private PixelNumberDisplay startBestDisplay;

        [Header("按钮")]
        [SerializeField] private Button btnStart;
        [SerializeField] private Button btnPause;
        [SerializeField] private Button btnResume;
        [SerializeField] private Button btnRestart;
        [SerializeField] private Button btnRestartFromPause;
        [SerializeField] private Button btnBackToMenu;

        // ★ 新增的两个退出按钮
        [SerializeField] private Button btnQuitStart;       // 开始界面退出
        [SerializeField] private Button btnQuitGameOver;    // 结束界面退出

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        void Start()
        {
            var gm = GameManager.Instance;
            if (gm == null) { Debug.LogError("[UIManager] 找不到 GameManager"); return; }

            gm.StateChanged += OnStateChanged;
            gm.ScoreChanged += OnScoreChanged;
            gm.LivesChanged += OnLivesChanged;

            if (btnStart != null) btnStart.onClick.AddListener(gm.StartGame);
            if (btnPause != null) btnPause.onClick.AddListener(gm.PauseGame);
            if (btnResume != null) btnResume.onClick.AddListener(gm.ResumeGame);
            if (btnRestart != null) btnRestart.onClick.AddListener(gm.StartGame);
            if (btnRestartFromPause != null) btnRestartFromPause.onClick.AddListener(gm.StartGame);
            if (btnBackToMenu != null) btnBackToMenu.onClick.AddListener(gm.BackToMenu);

            // ★ 新增：绑定退出游戏事件
            if (btnQuitStart != null) btnQuitStart.onClick.AddListener(QuitGame);
            if (btnQuitGameOver != null) btnQuitGameOver.onClick.AddListener(QuitGame);

            OnStateChanged(gm.State);
            OnScoreChanged(gm.Score);
            OnLivesChanged(gm.Lives);

            if (shieldTimerRoot != null) shieldTimerRoot.SetActive(false);
        }

        void OnDestroy()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;

            gm.StateChanged -= OnStateChanged;
            gm.ScoreChanged -= OnScoreChanged;
            gm.LivesChanged -= OnLivesChanged;
        }

        // 在 UIManager.cs 的 OnStateChanged 中：
        private void OnStateChanged(GameState state)
        {
            if (startPanel != null) startPanel.SetActive(state == GameState.Ready);
            if (hudPanel != null) hudPanel.SetActive(state == GameState.Playing || state == GameState.Paused);
            if (pausePanel != null) pausePanel.SetActive(state == GameState.Paused);
            if (gameOverPanel != null) gameOverPanel.SetActive(state == GameState.GameOver);

            // ★ 面板弹出缓动
            if (state == GameState.GameOver && gameOverPanel != null)
                StartCoroutine(PopPanel(gameOverPanel.transform));
            if (state == GameState.Paused && pausePanel != null)
                StartCoroutine(PopPanel(pausePanel.transform));

            if (state == GameState.GameOver)
            {
                var gm = GameManager.Instance;
                if (finalScoreDisplay != null) finalScoreDisplay.SetNumber(gm.Score.ToString());
                if (finalBestDisplay != null) finalBestDisplay.SetNumber(gm.BestScore.ToString());
            }

            if (state == GameState.Ready && startBestDisplay != null)
            {
                startBestDisplay.SetNumber(GameManager.Instance.BestScore.ToString());
            }
        }

        // 新增这个协程：
        private System.Collections.IEnumerator PopPanel(Transform panel)
        {
            panel.localScale = Vector3.zero;
            float elapsed = 0f;
            float duration = 0.3f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime; // 使用 unscaled 时间，暂停也能播动画
                float t = elapsed / duration;
                float scale = Easing.EaseOutBack(t);
                panel.localScale = new Vector3(scale, scale, 1f);
                yield return null;
            }
            panel.localScale = Vector3.one;
        }

        private void OnScoreChanged(int score)
        {
            if (scoreDisplay != null) scoreDisplay.SetNumber(score.ToString());
        }

        private void OnLivesChanged(int lives)
        {
            // 1. 更新图标
            if (lifeIcons != null)
            {
                for (int i = 0; i < lifeIcons.Length; i++)
                    if (lifeIcons[i] != null) lifeIcons[i].enabled = i < lives;
            }

            // 2. 更新像素数字生命
            if (lifeDisplay != null) lifeDisplay.SetNumber(lives.ToString());
        }

        /// <summary>更新护盾倒计时（只显示数字，不显示 s）</summary>
        public void SetShieldTimer(float remainSeconds)
        {
            if (shieldTimerRoot != null) shieldTimerRoot.SetActive(true);
            if (shieldDisplay != null)
            {
                // 直接传入 "3.0"，无需拼 "s"
                shieldDisplay.SetNumber(remainSeconds.ToString("0.0"));
            }
        }

        public void HideShieldTimer()
        {
            if (shieldTimerRoot != null) shieldTimerRoot.SetActive(false);
        }

        /// <summary>
        /// 退出游戏
        /// </summary>
        public void QuitGame()
        {
#if UNITY_EDITOR
            // 如果在 Unity 编辑器里运行，就停止播放
            UnityEditor.EditorApplication.isPlaying = false;
#else
            // 如果在打包好的游戏（Android/PC）里运行，就退出应用
            Application.Quit();
#endif
        }
    }
}