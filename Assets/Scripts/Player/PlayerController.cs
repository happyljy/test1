using UnityEngine;

namespace PlaneWar
{
    public class PlayerController : MonoBehaviour
    {
        [Header("移动")]
        [SerializeField] private Transform visualRoot;
        [SerializeField] private float followSharpness = 18f;
        [SerializeField] private float maxTilt = 14f;

        [Header("活动区域限制 (按屏幕比例)")]
        [Tooltip("底部留白，防止飞机贴到底部UI。0.5表示底部至少留出0.5个世界单位。")]
        [SerializeField] private float bottomPadding = 0.8f;

        [Tooltip("左右两侧留白。0.3表示左右各留出0.3个世界单位。")]
        [SerializeField] private float sidePadding = 0.4f;

        [Tooltip("飞机允许到达的最高屏幕比例 (0~1)。0.7表示只能飞到屏幕从下往上70%的高度，不能逼近敌机出生的顶部。")]
        [Range(0.1f, 1f)]
        [SerializeField] private float topScreenRatio = 0.65f;

        [Header("射击")]
        [SerializeField] private Transform muzzle;
        [SerializeField] private GameObject bulletPrefab;
        [SerializeField] private float fireInterval = 0.13f;
        [SerializeField] private float bulletSpeed = 17f;

        [Header("视觉")]
        [SerializeField] private SpriteRenderer bodyRenderer;
        [SerializeField] private GameObject shieldVisual;
        [SerializeField] private float blinkInterval = 0.07f;

        private Camera _cam;
        private Vector3 _targetPos;
        private Vector3 _dragOffset;
        private bool _dragging;
        private float _fireTimer;

        private float _invincibleTimer;
        private float _blinkTimer;
        private bool _bodyVisible = true;

        private bool _shieldActive;
        private float _shieldTimer;

        // 边界缓存
        private float _minX, _maxX, _minY, _maxY;

        public bool IsInvincible => _invincibleTimer > 0f || _shieldActive;

        void Awake() { _cam = Camera.main; }

        void OnEnable()
        {
            if (_cam == null) _cam = Camera.main;
            RecalcBounds();
        }

        public void ResetPlayer()
        {
            gameObject.SetActive(true);

            if (_cam == null) _cam = Camera.main;

            transform.position = new Vector3(0f, -3.4f, 0f);
            _targetPos = transform.position;
            _dragging = false;
            _fireTimer = 0.25f;

            _invincibleTimer = 0f;
            _blinkTimer = 0f;
            _bodyVisible = true;
            if (bodyRenderer != null) bodyRenderer.enabled = true;

            _shieldActive = false;
            _shieldTimer = 0f;
            if (shieldVisual != null) shieldVisual.SetActive(false);
            UIManager.Instance?.HideShieldTimer();

            if (visualRoot != null) visualRoot.localRotation = Quaternion.identity;

            RecalcBounds();
        }

        /// <summary>
        /// 根据当前相机和比例设置，重新计算玩家可移动的矩形区域
        /// </summary>
        public void RecalcBounds()
        {
            if (_cam == null) return;

            float halfH = _cam.orthographicSize;
            float halfW = halfH * _cam.aspect;
            Vector3 c = _cam.transform.position;

            // 左右边界
            _minX = c.x - halfW + sidePadding;
            _maxX = c.x + halfW - sidePadding;

            // 下边界：屏幕底部 + 底部留白
            _minY = c.y - halfH + bottomPadding;

            // 上边界：按屏幕比例限制（例如只能走到屏幕高度的 65%）
            float screenTop = c.y + halfH;
            float screenBottom = c.y - halfH;
            _maxY = screenBottom + (screenTop - screenBottom) * topScreenRatio;
        }

        void Update()
        {
            if (GameManager.Instance == null || !GameManager.Instance.IsPlaying) return;

            HandleInput();
            HandleMovement();
            HandleShooting();
            HandleTimers();
        }

        private void HandleInput()
        {
            Vector2 pointer = Vector2.zero;
            bool down = false, held = false, up = false;

            if (Input.touchCount > 0)
            {
                Touch t = Input.GetTouch(0);
                pointer = t.position;
                down = t.phase == TouchPhase.Began;
                held = t.phase == TouchPhase.Moved || t.phase == TouchPhase.Stationary;
                up = t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled;
            }
            else
            {
                pointer = Input.mousePosition;
                down = Input.GetMouseButtonDown(0);
                held = Input.GetMouseButton(0);
                up = Input.GetMouseButtonUp(0);
            }

            if (_cam == null) _cam = Camera.main;
            if (_cam == null) return;

            Vector3 world = _cam.ScreenToWorldPoint(new Vector3(pointer.x, pointer.y, 0f));
            world.z = transform.position.z;

            if (down)
            {
                _dragging = true;
                // 抓取时计算偏移，避免飞机瞬移到手指位置
                _dragOffset = transform.position - world;
                // 限制拖拽偏移，防止手指拖太远导致飞机瞬移
                _dragOffset.x = Mathf.Clamp(_dragOffset.x, -2f, 2f);
                _dragOffset.y = Mathf.Clamp(_dragOffset.y, -2f, 2f);
            }

            if (_dragging && held)
            {
                Vector3 target = world + _dragOffset;

                // 预先钳制目标位置，防止 target 飞出屏幕外导致后续计算浪费
                target.x = Mathf.Clamp(target.x, _minX, _maxX);
                target.y = Mathf.Clamp(target.y, _minY, _maxY);
                target.z = transform.position.z;

                _targetPos = target;
            }

            if (up) _dragging = false;
        }

        private void HandleMovement()
        {
            // 指数平滑，帧率无关，手感顺滑
            float k = 1f - Mathf.Exp(-followSharpness * Time.deltaTime);
            Vector3 next = Vector3.Lerp(transform.position, _targetPos, k);

            // ★ 二次保险：无论怎么Lerp，最终位置绝对不允许超出活动区
            next.x = Mathf.Clamp(next.x, _minX, _maxX);
            next.y = Mathf.Clamp(next.y, _minY, _maxY);
            next.z = transform.position.z;

            transform.position = next;

            // 机身倾斜（视觉反馈）
            if (visualRoot != null)
            {
                float dx = _targetPos.x - transform.position.x;
                float targetTilt = Mathf.Clamp(dx * 42f, -maxTilt, maxTilt);

                float curZ = visualRoot.localEulerAngles.z;
                if (curZ > 180f) curZ -= 360f;

                float newZ = Mathf.Lerp(curZ, targetTilt, 1f - Mathf.Exp(-11f * Time.deltaTime));
                visualRoot.localRotation = Quaternion.Euler(0f, 0f, newZ);
            }
        }

        private void HandleShooting()
        {
            _fireTimer -= Time.deltaTime;
            if (_fireTimer > 0f) return;

            _fireTimer = fireInterval;
            Fire();
        }

        private void Fire()
        {
            if (bulletPrefab == null) return;

            Vector3 origin = muzzle != null
                ? muzzle.position
                : transform.position + Vector3.up * 0.8f;

            var go = Pool.Instance.Spawn(bulletPrefab, origin, Quaternion.identity);
            var bullet = go.GetComponent<Bullet>();
            if (bullet != null) bullet.Init(Vector2.up, bulletSpeed);
        }

        private void HandleTimers()
        {
            if (_invincibleTimer > 0f)
            {
                _invincibleTimer -= Time.deltaTime;
                _blinkTimer -= Time.deltaTime;

                if (_blinkTimer <= 0f)
                {
                    _blinkTimer = blinkInterval;
                    _bodyVisible = !_bodyVisible;
                    if (bodyRenderer != null) bodyRenderer.enabled = _bodyVisible;
                }

                if (_invincibleTimer <= 0f)
                {
                    _bodyVisible = true;
                    if (bodyRenderer != null) bodyRenderer.enabled = true;
                }
            }

            if (_shieldActive)
            {
                _shieldTimer -= Time.deltaTime;
                UIManager.Instance?.SetShieldTimer(Mathf.Max(0f, _shieldTimer));

                if (_shieldTimer <= 0f)
                {
                    _shieldActive = false;
                    if (shieldVisual != null) shieldVisual.SetActive(false);
                    UIManager.Instance?.HideShieldTimer();
                }
            }
        }

        public bool TakeHit()
        {
            if (GameManager.Instance == null || !GameManager.Instance.IsPlaying) return false;
            if (IsInvincible) return false;

            FXSpawner.Instance?.SpawnHit(transform.position);
            GameManager.Instance.OnPlayerHit();
            return true;
        }

        public void BeginInvincible(float duration)
        {
            _invincibleTimer = duration;
            _blinkTimer = 0f;
        }

        public void ActivateShield(float duration)
        {
            _shieldActive = true;
            _shieldTimer = duration;

            _invincibleTimer = 0f;
            _bodyVisible = true;
            if (bodyRenderer != null) bodyRenderer.enabled = true;

            if (shieldVisual != null) shieldVisual.SetActive(true);
            UIManager.Instance?.SetShieldTimer(duration);
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (GameManager.Instance == null || !GameManager.Instance.IsPlaying) return;
            if (IsInvincible) return;

            var enemy = other.GetComponentInParent<EnemyBase>();
            if (enemy != null && enemy.IsAlive)
            {
                enemy.TakeDamage(99999);
                TakeHit();
            }
        }
    }
}