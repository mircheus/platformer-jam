using UnityEngine;

namespace Minigames
{
    public enum EaseType
    {
        Linear,
        InQuad,
        OutQuad,
        InOutQuad,
        InCubic,
        OutCubic,
        InOutCubic,
        InSine,
        OutSine,
        InOutSine,
        InBack,
        OutBack,
        OutBounce
    }

    /// <summary>
    /// Лёгкий набор easing-функций (без внешних твин-библиотек).
    /// t ожидается в диапазоне [0..1], результат тоже нормализованный.
    /// </summary>
    public static class Easing
    {
        public static float Evaluate(EaseType type, float t)
        {
            t = Mathf.Clamp01(t);

            switch (type)
            {
                case EaseType.Linear:    return t;

                case EaseType.InQuad:    return t * t;
                case EaseType.OutQuad:   return 1f - (1f - t) * (1f - t);
                case EaseType.InOutQuad: return t < 0.5f
                                             ? 2f * t * t
                                             : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;

                case EaseType.InCubic:    return t * t * t;
                case EaseType.OutCubic:   return 1f - Mathf.Pow(1f - t, 3f);
                case EaseType.InOutCubic: return t < 0.5f
                                              ? 4f * t * t * t
                                              : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f;

                case EaseType.InSine:    return 1f - Mathf.Cos((t * Mathf.PI) / 2f);
                case EaseType.OutSine:   return Mathf.Sin((t * Mathf.PI) / 2f);
                case EaseType.InOutSine: return -(Mathf.Cos(Mathf.PI * t) - 1f) / 2f;

                case EaseType.InBack:
                {
                    const float c1 = 1.70158f;
                    const float c3 = c1 + 1f;
                    return c3 * t * t * t - c1 * t * t;
                }
                case EaseType.OutBack:
                {
                    const float c1 = 1.70158f;
                    const float c3 = c1 + 1f;
                    return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
                }

                case EaseType.OutBounce: return OutBounce(t);

                default: return t;
            }
        }

        private static float OutBounce(float t)
        {
            const float n1 = 7.5625f;
            const float d1 = 2.75f;

            if (t < 1f / d1)
            {
                return n1 * t * t;
            }

            if (t < 2f / d1)
            {
                t -= 1.5f / d1;
                return n1 * t * t + 0.75f;
            }

            if (t < 2.5f / d1)
            {
                t -= 2.25f / d1;
                return n1 * t * t + 0.9375f;
            }

            t -= 2.625f / d1;
            return n1 * t * t + 0.984375f;
        }
    }
}
