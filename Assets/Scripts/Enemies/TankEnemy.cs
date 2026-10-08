using UnityEngine;

namespace PlaneWar
{
    public class TankEnemy : EnemyBase
    {
        [Header("ÒÆ¶¯")]
        [SerializeField] private float descendSpeed = 1.1f;
        [SerializeField] private float stopY = 2.3f;
        [SerializeField] private float patrolSpeed = 0.75f;
        [SerializeField] private float patrolRange = 2.2f;

        [Header("¹¥»÷")]
        [SerializeField] private GameObject shellPrefab;
        [SerializeField] private float shellSpeed = 5.5f;
        [SerializeField] private float fireInterval = 2.2f;
        [SerializeField] private float aimAssist = 0.65f;
        [SerializeField] private Transform firePoint;

        private enum Phase { Descend, Patrol }

        private Phase _phase;
        private float _patrolCenterX;
        private int _patrolDir = 1;
        private float _fireTimer;

        protected override void OnSpawned()
        {
            _phase = Phase.Descend;
            _patrolCenterX = transform.position.x;
            _patrolDir = Random.value < 0.5f ? -1 : 1;
            _fireTimer = fireInterval * Random.Range(0.5f, 0.9f);
        }

        protected override void Tick()
        {
            switch (_phase)
            {
                case Phase.Descend:
                    transform.position += Vector3.down * (descendSpeed * Time.deltaTime);
                    if (transform.position.y <= stopY)
                    {
                        var p = transform.position; p.y = stopY; transform.position = p;
                        _phase = Phase.Patrol;
                    }
                    break;

                case Phase.Patrol:
                    Patrol();
                    break;
            }
        }

        private void Patrol()
        {
            var p = transform.position;

            p.x += _patrolDir * patrolSpeed * Time.deltaTime;

            if (p.x > _patrolCenterX + patrolRange) { p.x = _patrolCenterX + patrolRange; _patrolDir = -1; }
            if (p.x < _patrolCenterX - patrolRange) { p.x = _patrolCenterX - patrolRange; _patrolDir = 1; }

            p.y = Mathf.Lerp(p.y, stopY, 1f - Mathf.Exp(-6f * Time.deltaTime));
            transform.position = p;

            _fireTimer -= Time.deltaTime;
            if (_fireTimer <= 0f)
            {
                Fire();
                _fireTimer = fireInterval;
            }
        }

        private void Fire()
        {
            if (shellPrefab == null) return;

            Vector3 origin = firePoint != null
                ? firePoint.position
                : transform.position + Vector3.down * 0.6f;

            Vector2 dir = Vector2.down;
            var player = GameManager.Instance != null ? GameManager.Instance.Player : null;
            if (player != null && player.gameObject.activeSelf)
            {
                Vector2 toPlayer = ((Vector2)(player.transform.position - origin)).normalized;
                dir = Vector2.Lerp(Vector2.down, toPlayer, aimAssist).normalized;
            }

            var go = Pool.Instance.Spawn(shellPrefab, origin, Quaternion.identity);
            var bullet = go.GetComponent<Bullet>();
            if (bullet != null) bullet.Init(dir, shellSpeed);
        }
    }
}