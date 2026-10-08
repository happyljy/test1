using UnityEngine;

namespace PlaneWar
{
    public class Bullet : MonoBehaviour
    {
        public enum Side { Player, Enemy }

        [SerializeField] private Side side = Side.Player;
        [SerializeField] private int damage = 1;
        [SerializeField] private float speed = 14f;
        [SerializeField] private float lifetime = 4f;
        [SerializeField] private float outOfScreenMargin = 0.2f; // 出屏容差

        private Vector2 direction = Vector2.up;
        private float timer;
        private Camera _cam;

        public Side BulletSide => side;

        void Awake()
        {
            _cam = Camera.main;
        }

        void OnEnable()
        {
            timer = lifetime;
            direction = Vector2.up;
            transform.rotation = Quaternion.identity;
            if (_cam == null) _cam = Camera.main;
        }

        public void Init(Vector2 dir, float spd = -1f)
        {
            direction = dir.sqrMagnitude > 0.001f ? dir.normalized : Vector2.up;
            if (spd > 0f) speed = spd;

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        void Update()
        {
            if (Time.deltaTime <= 0f) return;

            transform.position += (Vector3)(direction * (speed * Time.deltaTime));

            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                Pool.Instance.Despawn(gameObject);
                return;
            }

            // ★ 新增：屏幕边界检测，飞出屏幕立刻回收
            if (_cam != null)
            {
                Vector3 viewportPos = _cam.WorldToViewportPoint(transform.position);
                // 玩家子弹往上飞，y > 1 + margin 时消失
                if (side == Side.Player && viewportPos.y > 1f + outOfScreenMargin)
                {
                    Pool.Instance.Despawn(gameObject);
                    return;
                }
                // 敌人子弹往下飞，y < -margin 或飞出左右边界时消失
                if (side == Side.Enemy && (viewportPos.y < -outOfScreenMargin || viewportPos.x < -outOfScreenMargin || viewportPos.x > 1f + outOfScreenMargin))
                {
                    Pool.Instance.Despawn(gameObject);
                    return;
                }
            }
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (side == Side.Player)
            {
                var enemy = other.GetComponentInParent<EnemyBase>();
                if (enemy != null && enemy.IsAlive)
                {
                    enemy.TakeDamage(damage);
                    Pool.Instance.Despawn(gameObject);
                }
            }
            else
            {
                var player = other.GetComponentInParent<PlayerController>();
                if (player != null && player.TakeHit())
                    Pool.Instance.Despawn(gameObject);
            }
        }
    }
}