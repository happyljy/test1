using UnityEngine;

namespace PlaneWar
{
    public enum PropType { Life, Shield, Bomb }

    public class Prop : MonoBehaviour
    {
        [SerializeField] private PropType type = PropType.Life;
        [SerializeField] private float fallSpeed = 2.0f;
        [SerializeField] private float lifeTime = 14f;
        [SerializeField] private float swayAmplitude = 0.25f;
        [SerializeField] private float swayFrequency = 3.5f;

        public PropType Type => type;

        private float _timer;
        private float _baseX;
        private float _phase;

        void OnEnable()
        {
            _timer = lifeTime;
            _baseX = transform.position.x;
            _phase = Random.value * Mathf.PI * 2f;
        }

        void Update()
        {
            if (Time.deltaTime <= 0f) return;

            _timer -= Time.deltaTime;
            if (_timer <= 0f) { Pool.Instance.Despawn(gameObject); return; }

            var p = transform.position;
            p.y -= fallSpeed * Time.deltaTime;
            p.x = _baseX + Mathf.Sin((lifeTime - _timer) * swayFrequency + _phase) * swayAmplitude;
            transform.position = p;

            var cam = Camera.main;
            if (cam != null && cam.WorldToViewportPoint(transform.position).y < -0.2f)
                Pool.Instance.Despawn(gameObject);
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            var player = other.GetComponentInParent<PlayerController>();
            if (player == null) return;

            Apply(player);
            Pool.Instance.Despawn(gameObject);
        }

        private void Apply(PlayerController player)
        {
            switch (type)
            {
                case PropType.Life:
                    GameManager.Instance?.HealLife();
                    break;
                case PropType.Shield:
                    player.ActivateShield(3f);
                    break;
                case PropType.Bomb:
                    GameManager.Instance?.UseBomb();
                    break;
            }
        }
    }
}