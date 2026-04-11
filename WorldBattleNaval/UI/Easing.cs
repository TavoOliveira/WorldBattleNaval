namespace WorldBattleNaval.UI;

public static class Easing
{
    public static float Linear(float t) => t;

    public static float EaseIn(float t) => t * t;

    public static float EaseOut(float t) => 1f - (1f - t) * (1f - t);

    public static float EaseInOut(float t) =>
        t < 0.5f ? 2f * t * t : 1f - (-2f * t + 2f) * (-2f * t + 2f) / 2f;
}
