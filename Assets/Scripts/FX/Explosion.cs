using UnityEngine;

namespace PlaneWar
{
    public class Explosion : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer sr;
        [SerializeField] private float duration = 0.35f;
        [SerializeField] private float startScale = 0.35f;
        [SerializeField] private float endScale = 1.9f;

        [Header("»º¶¯ÇúÏß")]
        [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField] private AnimationCurve alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

        private float _t;
        private float _scaleMul = 1f;

        void OnEnable()
        {
            _t = 0f;
            _scaleMul = 1f;
            if (sr == null) sr = GetComponentInChildren<SpriteRenderer>();
            transform.localScale = Vector3.one * startScale;
            if (sr != null) sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 1f);
        }

        public void SetScaleMultiplier(float mul) => _scaleMul = Mathf.Max(0.01f, mul);

        void Update()
        {
            if (Time.deltaTime <= 0f) return;

            _t += Time.deltaTime;
            float k = Mathf.Clamp01(_t / duration);

            float s = Mathf.Lerp(startScale, endScale, scaleCurve.Evaluate(k)) * _scaleMul;
            transform.localScale = new Vector3(s, s, 1f);

            if (sr != null)
            {
                var c = sr.color;
                c.a = alphaCurve.Evaluate(k);
                sr.color = c;
            }

            if (k >= 1f) Pool.Instance.Despawn(gameObject);
        }
    }
}