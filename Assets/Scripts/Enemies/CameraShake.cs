using UnityEngine;
using System.Collections;

namespace PlaneWar
{
    public class CameraShake : MonoBehaviour
    {
        public static CameraShake Instance { get; private set; }

        [Header("全局震动倍率 (可在 Inspector 调节)")]
        [Range(0f, 2f)]
        [SerializeField] private float globalMultiplier = 1f;

        private Vector3 _originalPos;
        private Coroutine _shakeRoutine;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            _originalPos = transform.localPosition;
        }

        /// <summary>触发屏幕震动</summary>
        /// <param name="duration">持续时间</param>
        /// <param name="magnitude">震动幅度（建议 0.05 ~ 0.2）</param>
        public void Shake(float duration = 0.2f, float magnitude = 0.1f)
        {
            if (_shakeRoutine != null) StopCoroutine(_shakeRoutine);
            _shakeRoutine = StartCoroutine(ShakeRoutine(duration, magnitude * globalMultiplier));
        }

        private IEnumerator ShakeRoutine(float duration, float magnitude)
        {
            float elapsed = 0f;
            // 柏林噪声的随机种子，保证每次震动轨迹不同
            float seed = Random.Range(0f, 100f);

            while (elapsed < duration)
            {
                // 使用 unscaledDeltaTime，这样暂停时也能正常震动（如果不需要暂停震动可改为 deltaTime）
                elapsed += Time.unscaledDeltaTime;

                // 衰减系数：从 1 逐渐变为 0
                float fade = 1f - (elapsed / duration);
                // 使用 SmoothStep 让衰减更平滑
                fade = Mathf.SmoothStep(0f, 1f, fade);

                // 利用柏林噪声生成平滑的偏移量（而不是纯随机）
                float x = (Mathf.PerlinNoise(seed, elapsed * 20f) - 0.5f) * 2f * magnitude * fade;
                float y = (Mathf.PerlinNoise(seed + 1f, elapsed * 20f) - 0.5f) * 2f * magnitude * fade;

                transform.localPosition = new Vector3(_originalPos.x + x, _originalPos.y + y, _originalPos.z);

                yield return null;
            }

            // 震动结束，复位
            transform.localPosition = _originalPos;
            _shakeRoutine = null;
        }
    }
}