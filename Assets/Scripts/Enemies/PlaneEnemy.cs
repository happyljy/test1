using UnityEngine;

namespace PlaneWar
{
    public class PlaneEnemy : EnemyBase
    {
        [Header("移动")]
        [SerializeField] private float moveSpeed = 3.2f;
        [SerializeField] private float swayAmplitude = 1.4f;
        [SerializeField] private float swayFrequency = 1.7f;
        [SerializeField] private float maxTilt = 22f;
      

        [Header("攻击")]
        [SerializeField] private GameObject bulletPrefab;
        [SerializeField] private float bulletSpeed = 7.5f;
        [SerializeField] private float fireInterval = 1.5f;
        [SerializeField] private int burstCount = 1;
        [SerializeField] private float burstGap = 0.1f;
        [SerializeField] private Transform firePoint;

        private float _baseX;
        private float _phaseOffset;
        private float _fireTimer;
        private float _burstTimer;
        private int _burstLeft;

        // 动态屏幕边界
        private float _minX, _maxX;
        private float _screenTopY;

        protected override void OnSpawned()
        {
            _baseX = transform.position.x;
            _phaseOffset = Random.value * Mathf.PI * 2f;
            _fireTimer = Random.Range(0.3f, fireInterval);
            _burstLeft = 0;
            _burstTimer = 0f;

            // 初始化时计算一次屏幕边界
            RecalcBounds();
        }

        private void RecalcBounds()
        {
            if (Camera.main == null) return;

            float halfH = Camera.main.orthographicSize;
            float halfW = halfH * Camera.main.aspect;
            Vector3 c = Camera.main.transform.position;

            // 左右边界留出一些间距，与玩家的 sidePadding 类似（玩家是 0.4f）
            _minX = c.x - halfW + 0.4f;
            _maxX = c.x + halfW - 0.4f;

            // 屏幕顶部世界坐标
            _screenTopY = c.y + halfH;
        }

        protected override void Tick()
        {
            Vector3 p = transform.position;
            float prevX = p.x;

            // 向下移动
            p.y -= moveSpeed * Time.deltaTime;

            // 动态计算左右摆动边界（和玩家一样限制在屏幕内）
            float targetX = _baseX + Mathf.Sin(Time.time * swayFrequency + _phaseOffset) * swayAmplitude;
            p.x = Mathf.Clamp(targetX, _minX, _maxX);

            transform.position = p;

            // 倾斜视觉
            float dx = p.x - prevX;
            float targetTilt = Mathf.Clamp(-dx / Mathf.Max(Time.deltaTime, 1e-4f) * 1.6f, -maxTilt, maxTilt);
            float curZ = transform.localEulerAngles.z;
            if (curZ > 180f) curZ -= 360f;
            float newZ = Mathf.Lerp(curZ, targetTilt, 1f - Mathf.Exp(-9f * Time.deltaTime));
            transform.localRotation = Quaternion.Euler(0f, 0f, newZ);

            // ★ 只有完全进入屏幕内部才允许开火
            bool isInsideScreen = transform.position.y <= _screenTopY;
            HandleFire(isInsideScreen);

            // 飞出屏幕回收
            if (IsOutOfScreen(0.35f))
            {
                Pool.Instance.Despawn(gameObject);
            }
        }

        private void HandleFire(bool canFire)
        {
            // 如果还没进入屏幕，不推进开火计时器，也不发射
            if (!canFire) return;

            if (_burstLeft > 0)
            {
                _burstTimer -= Time.deltaTime;
                if (_burstTimer <= 0f)
                {
                    FireOne();
                    _burstLeft--;
                    _burstTimer = burstGap;
                }
            }
            else
            {
                _fireTimer -= Time.deltaTime;
                if (_fireTimer <= 0f)
                {
                    _burstLeft = Mathf.Max(1, burstCount);
                    _burstTimer = 0f;
                    _fireTimer = fireInterval;
                }
            }
        }

        private void FireOne()
        {
            if (bulletPrefab == null) return;

            Vector3 origin = firePoint != null
                ? firePoint.position
                : transform.position + Vector3.down * 0.5f;

            var go = Pool.Instance.Spawn(bulletPrefab, origin, Quaternion.identity);
            var bullet = go.GetComponent<Bullet>();
            if (bullet != null) bullet.Init(Vector2.down, bulletSpeed);
        }
    }
}