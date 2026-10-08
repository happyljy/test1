using UnityEngine;

namespace PlaneWar
{
    public static class Easing
    {
        // 弹性缩放（受击、UI弹出）
        public static float EaseOutBack(float t)
        {
            float c1 = 1.70158f;
            float c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
        }

        // 二次缓出（平滑减速）
        public static float EaseOutQuad(float t)
        {
            return 1f - (1f - t) * (1f - t);
        }

        // 二次缓入缓出
        public static float EaseInOutQuad(float t)
        {
            return t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;
        }

        // 弹簧效果（敌人受击）
        public static float EaseOutElastic(float t)
        {
            if (t == 0f) return 0f;
            if (t == 1f) return 1f;
            float c4 = (2f * Mathf.PI) / 3f;
            return Mathf.Pow(2f, -10f * t) * Mathf.Sin((t * 10f - 0.75f) * c4) + 1f;
        }
    }
}