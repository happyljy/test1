using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

namespace PlaneWar.EditorTools
{
    public static class SceneBuilder
    {
        [MenuItem("Tools/Build Plane War Scene")]
        public static void BuildScene()
        {
            // 1. 清理可能已存在的旧对象
            DestroyIfExists("GameManager");
            DestroyIfExists("Pool");
            DestroyIfExists("FXSpawner");
            DestroyIfExists("EnemySpawner");
            DestroyIfExists("PropSpawner");
            DestroyIfExists("Player");
            DestroyIfExists("Canvas");

            // 2. 创建场景管理器对象
            var poolGO = CreateObject("Pool", Vector3.zero);
            var pool = poolGO.AddComponent<Pool>();

            var gmGO = CreateObject("GameManager", Vector3.zero);
            var gm = gmGO.AddComponent<GameManager>();

            var fxGO = CreateObject("FXSpawner", Vector3.zero);
            var fx = fxGO.AddComponent<FXSpawner>();

            var enemySpawnerGO = CreateObject("EnemySpawner", Vector3.zero);
            var enemySpawner = enemySpawnerGO.AddComponent<EnemySpawner>();

            var propSpawnerGO = CreateObject("PropSpawner", Vector3.zero);
            var propSpawner = propSpawnerGO.AddComponent<PropSpawner>();

            // 3. 创建玩家
            var playerGO = CreateObject("Player", new Vector3(0, -3.4f, 0));
            playerGO.AddComponent<KinematicBody>();
            var col = playerGO.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.25f;
            var player = playerGO.AddComponent<PlayerController>();

            // 玩家子物体
            var bodyGO = CreateObject("Body", Vector3.zero);
            bodyGO.transform.SetParent(playerGO.transform);
            bodyGO.transform.localPosition = Vector3.zero;
            var bodySr = bodyGO.AddComponent<SpriteRenderer>();
            bodySr.sortingOrder = 10;

            var muzzleGO = CreateObject("Muzzle", new Vector3(0, 0.8f, 0));
            muzzleGO.transform.SetParent(playerGO.transform);
            muzzleGO.transform.localPosition = new Vector3(0, 0.8f, 0);

            var shieldGO = CreateObject("ShieldVisual", Vector3.zero);
            shieldGO.transform.SetParent(playerGO.transform);
            shieldGO.transform.localPosition = Vector3.zero;
            var shieldSr = shieldGO.AddComponent<SpriteRenderer>();
            shieldSr.sortingOrder = 15;
            shieldGO.SetActive(false);

            // 4. 创建 Canvas 及 UI
            var canvasGO = CreateObject("Canvas", Vector3.zero);
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGO.AddComponent<GraphicRaycaster>();
            var uiManager = canvasGO.AddComponent<UIManager>();

            // 创建四个面板
            var startPanel = CreatePanel("StartPanel", canvasGO.transform, true);
            var hudPanel = CreatePanel("HUDPanel", canvasGO.transform, false);
            var pausePanel = CreatePanel("PausePanel", canvasGO.transform, false);
            var gameOverPanel = CreatePanel("GameOverPanel", canvasGO.transform, false);

            // 简单生成 UI 按钮和文本占位（你后续可以自己替换美术）
            var btnStart = CreateButton("BtnStart", startPanel.transform, "开始");
            var btnPause = CreateButton("BtnPause", hudPanel.transform, "暂停");
            var btnResume = CreateButton("BtnResume", pausePanel.transform, "继续");
            var btnRestartFromPause = CreateButton("BtnRestartFromPause", pausePanel.transform, "重开");
            var btnBackToMenu = CreateButton("BtnBackToMenu", pausePanel.transform, "主菜单");
            var btnRestart = CreateButton("BtnRestart", gameOverPanel.transform, "重新开始");

            var scoreText = CreateText("ScoreText", hudPanel.transform, "0", 72, Color.yellow);
            var startBestText = CreateText("StartBestText", startPanel.transform, "0", 60, Color.cyan);
            var finalScoreText = CreateText("FinalScoreText", gameOverPanel.transform, "0", 90, Color.yellow);
            var finalBestText = CreateText("FinalBestText", gameOverPanel.transform, "0", 70, Color.cyan);

            var shieldTimerRoot = CreatePanel("ShieldTimerRoot", hudPanel.transform, false);
            var shieldTimerText = CreateText("ShieldText", shieldTimerRoot.transform, "3.0s", 48, Color.cyan);

            // 5. 通过 SerializedObject 为脚本的私有 [SerializeField] 字段赋值
            // --- 赋值 GameManager ---
            var soGM = new SerializedObject(gm);
            soGM.FindProperty("player").objectReferenceValue = player;
            soGM.FindProperty("enemySpawner").objectReferenceValue = enemySpawner;
            soGM.FindProperty("propSpawner").objectReferenceValue = propSpawner;
            soGM.ApplyModifiedProperties();

            // --- 赋值 PlayerController ---
            var soPlayer = new SerializedObject(player);
            soPlayer.FindProperty("visualRoot").objectReferenceValue = bodyGO.transform;
            soPlayer.FindProperty("muzzle").objectReferenceValue = muzzleGO.transform;
            soPlayer.FindProperty("bodyRenderer").objectReferenceValue = bodySr;
            soPlayer.FindProperty("shieldVisual").objectReferenceValue = shieldGO;
            soPlayer.ApplyModifiedProperties();

            // --- 赋值 UIManager ---
            var soUI = new SerializedObject(uiManager);
            soUI.FindProperty("startPanel").objectReferenceValue = startPanel;
            soUI.FindProperty("hudPanel").objectReferenceValue = hudPanel;
            soUI.FindProperty("pausePanel").objectReferenceValue = pausePanel;
            soUI.FindProperty("gameOverPanel").objectReferenceValue = gameOverPanel;
            soUI.FindProperty("scoreText").objectReferenceValue = scoreText;
            soUI.FindProperty("startBestText").objectReferenceValue = startBestText;
            soUI.FindProperty("shieldTimerRoot").objectReferenceValue = shieldTimerRoot;
            soUI.FindProperty("shieldTimerText").objectReferenceValue = shieldTimerText;
            soUI.FindProperty("finalScoreText").objectReferenceValue = finalScoreText;
            soUI.FindProperty("finalBestText").objectReferenceValue = finalBestText;
            soUI.FindProperty("btnStart").objectReferenceValue = btnStart;
            soUI.FindProperty("btnPause").objectReferenceValue = btnPause;
            soUI.FindProperty("btnResume").objectReferenceValue = btnResume;
            soUI.FindProperty("btnRestartFromPause").objectReferenceValue = btnRestartFromPause;
            soUI.FindProperty("btnBackToMenu").objectReferenceValue = btnBackToMenu;
            soUI.FindProperty("btnRestart").objectReferenceValue = btnRestart;
            soUI.ApplyModifiedProperties();

            // --- 赋值 FXSpawner (假设你已经建好了预制体，未建则留空) ---
            var soFx = new SerializedObject(fx);
            // 这里由你自己手动拖入，或者用代码加载预制体：
            // var explosionPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/FX_Explosion.prefab");
            // soFx.FindProperty("explosionPrefab").objectReferenceValue = explosionPrefab;
            soFx.ApplyModifiedProperties();

            // --- 赋值 EnemySpawner ---
            var soEnemy = new SerializedObject(enemySpawner);
            // var tankPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemy_Tank.prefab");
            // var planePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemy_Plane.prefab");
            // soEnemy.FindProperty("tankPrefab").objectReferenceValue = tankPrefab;
            // soEnemy.FindProperty("planePrefab").objectReferenceValue = planePrefab;
            soEnemy.ApplyModifiedProperties();

            // --- 赋值 PropSpawner ---
            var soProp = new SerializedObject(propSpawner);
            // var propLife = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Prop_Life.prefab");
            // var propShield = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Prop_Shield.prefab");
            // var propBomb = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Prop_Bomb.prefab");
            soProp.ApplyModifiedProperties();

            Debug.Log("<color=green>[SceneBuilder] 场景基础结构搭建完成！请检查 Inspector 并手动补充预制体引用。</color>");
        }

        // ================= 辅助方法 =================

        private static void DestroyIfExists(string name)
        {
            var go = GameObject.Find(name);
            if (go != null) Object.DestroyImmediate(go);
        }

        private static GameObject CreateObject(string name, Vector3 pos)
        {
            var go = new GameObject(name);
            go.transform.position = pos;
            Undo.RegisterCreatedObjectUndo(go, "Build Scene");
            return go;
        }

        private static GameObject CreatePanel(string name, Transform parent, bool active)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
            go.SetActive(active);
            return go;
        }

        private static Button CreateButton(string name, Transform parent, string label)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(300, 100);
            rect.anchoredPosition = new Vector2(0, Random.Range(-200, 200)); // 稍微错开，避免重叠

            var img = go.AddComponent<Image>();
            img.color = new Color(0.2f, 0.6f, 1f, 0.8f); // 蓝色底

            var btn = go.AddComponent<Button>();

            // 添加文本
            var textGo = new GameObject("Text");
            textGo.transform.SetParent(go.transform, false);
            var textRect = textGo.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            var tmp = textGo.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontSize = 40;

            return btn;
        }

        private static TextMeshProUGUI CreateText(string name, Transform parent, string defaultText, float fontSize, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(400, 100);
            rect.anchoredPosition = new Vector2(0, Random.Range(-300, 300));

            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = defaultText;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;

            return tmp;
        }
    }
}