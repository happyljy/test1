using UnityEngine;

namespace PlaneWar
{
    public abstract class EnemyBase : MonoBehaviour
    {
        [Header("数值")]
        [SerializeField] protected int maxHp = 3;
        [SerializeField] protected int scoreValue = 100;

        [Header("掉落")]
        [SerializeField] protected bool canDropProp = true;
        [Range(0f, 1f)][SerializeField] protected float dropChance = 0.18f;

        [Header("受击反馈")]
        [SerializeField] protected float flashTime = 0.09f;
        [SerializeField] protected float hitPunchScale = 1.18f;

        protected int Hp;
        protected Collider2D col;

        private SpriteRenderer[] _renderers;
        private Color[] _baseColors;
        private Vector3 _baseScale;
        private float _flashTimer;
        private bool _dying;
        private Coroutine _punchRoutine;

        public bool IsAlive => Hp > 0 && !_dying;

        protected virtual void Awake()
        {
            col = GetComponent<Collider2D>();

            _renderers = GetComponentsInChildren<SpriteRenderer>(true);
            _baseColors = new Color[_renderers.Length];
            for (int i = 0; i < _renderers.Length; i++)
                _baseColors[i] = _renderers[i].color;

            _baseScale = transform.localScale;
        }

        protected virtual void OnEnable()
        {
            Hp = maxHp;
            _dying = false;
            _flashTimer = 0f;
            _punchRoutine = null;
            transform.localScale = _baseScale;
            RestoreColors();
            if (col != null) col.enabled = true;

            OnSpawned();
        }

        protected virtual void Update()
        {
            if (Time.deltaTime <= 0f) return;   // 暂停时完全冻结

            if (_flashTimer > 0f)
            {
                _flashTimer -= Time.deltaTime;
                if (_flashTimer <= 0f) RestoreColors();
            }
            Tick();
        }

        protected abstract void Tick();
        protected virtual void OnSpawned() { }

        public virtual void TakeDamage(int amount)
        {
            if (!IsAlive) return;

            Hp -= amount;

            // 白闪
            _flashTimer = flashTime;
            for (int i = 0; i < _renderers.Length; i++)
                _renderers[i].color = Color.white;

            // ★ 启动弹性受击反馈（协程接管缩放）
            if (_punchRoutine != null) StopCoroutine(_punchRoutine);
            _punchRoutine = StartCoroutine(HitPunch());

            GameManager.Instance?.DoHitStop(0.03f);
            OnDamaged();

            if (Hp <= 0) Die();
        }

        protected virtual void OnDamaged() { }

        protected virtual void Die()
        {
            if (_dying) return;
            _dying = true;

            if (col != null) col.enabled = false;
            if (_punchRoutine != null) StopCoroutine(_punchRoutine); // 死亡时停止回弹协程

            GameManager.Instance?.AddScore(scoreValue);
            FXSpawner.Instance?.SpawnExplosion(transform.position, 1f);

            if (canDropProp && Random.value < dropChance)
                PropSpawner.Instance?.SpawnRandomAt(transform.position);

            Pool.Instance.Despawn(gameObject);
        }

        protected void RestoreColors()
        {
            for (int i = 0; i < _renderers.Length; i++)
                if (_renderers[i] != null) _renderers[i].color = _baseColors[i];
        }

        private System.Collections.IEnumerator HitPunch()
        {
            float elapsed = 0f;
            float duration = 0.25f;
            Vector3 originalScale = _baseScale;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                // 利用 EaseOutElastic 制造弹簧回弹效果
                float scaleMul = Mathf.Lerp(1f, hitPunchScale, Easing.EaseOutElastic(t));
                transform.localScale = originalScale * scaleMul;
                yield return null;
            }
            transform.localScale = originalScale;
            _punchRoutine = null;
        }

        protected bool IsOutOfScreen(float margin = 0.25f)
        {
            var cam = Camera.main;
            if (cam == null) return false;
            Vector3 v = cam.WorldToViewportPoint(transform.position);
            return v.y < -margin || v.y > 1f + margin || v.x < -margin || v.x > 1f + margin;
        }
    }
}