using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace WorldBattleNaval.UI;

public class TweenGroup
{
    private readonly List<TweenEntry> tweens = [];
    private readonly float duration;
    private readonly Func<float, float> forwardEasing;
    private readonly Func<float, float> reverseEasing;

    private float progress;

    public bool IsPlaying { get; private set; }
    public bool IsForward { get; private set; }
    public float Progress => progress;

    public Action? OnComplete { get; set; }

    public TweenGroup(float duration,
        Func<float, float>? forwardEasing = null,
        Func<float, float>? reverseEasing = null)
    {
        this.duration = duration;
        this.forwardEasing = forwardEasing ?? Easing.EaseOut;
        this.reverseEasing = reverseEasing ?? Easing.EaseIn;
    }

    public TweenGroup Add(Action<float> setter, float from, float to, bool snapToInt = false)
    {
        tweens.Add(new TweenEntry(setter, from, to, snapToInt));
        return this;
    }

    public void PlayForward()
    {
        IsForward = true;
        IsPlaying = true;
    }

    public void PlayReverse()
    {
        IsForward = false;
        IsPlaying = true;
    }

    public void Toggle()
    {
        if (IsForward)
            PlayReverse();
        else
            PlayForward();
    }

    public void Update(GameTime gameTime)
    {
        if (!IsPlaying) return;

        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        float speed = dt / duration;

        progress = IsForward
            ? MathHelper.Clamp(progress + speed, 0f, 1f)
            : MathHelper.Clamp(progress - speed, 0f, 1f);

        if (progress is >= 1f or <= 0f)
        {
            var endValue = progress >= 1f ? 1f : 0f;
            foreach (var tween in tweens)
                tween.Apply(endValue);

            IsPlaying = false;
            OnComplete?.Invoke();
            return;
        }

        var eased = IsForward
            ? forwardEasing(progress)
            : reverseEasing(progress);

        foreach (var tween in tweens)
            tween.Apply(eased);
    }

    private class TweenEntry(Action<float> setter, float from, float to, bool snapToInt)
    {
        public void Apply(float eased)
        {
            var value = from + (to - from) * eased;
            if (snapToInt)
                setter((float)Math.Round(value));
            else
                setter(value);
        }
    }
}
