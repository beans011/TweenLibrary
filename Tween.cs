using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tween<T> : ITween
{
    private T _startValue;
    private T _endValue;
    private float _duration;
    private Action<T> _onTweenUpdate;
    private float _elapsedTime = 0f;

    private float _delayElaspedTime = 0f;

    private int _loopsCompleted = 0;
    private bool _reverse = false;
    private bool _pingPong = false;
    private int _loopCount = 1;

    private float _percentThreshold = -1f;

    private Action _onUpdate;
    private Action _onPercentCompleted;
    private EaseType _easeType = EaseType.Linear;

    public Tween(object target, string identifier, T startValue, T endValue, float duration, Action<T> onTweenUpdate)
    {
        Target = target;
        Identifier = identifier;
        _startValue = startValue;
        _endValue = endValue;
        _duration = duration;
        _onTweenUpdate = onTweenUpdate;

        TweenManager.Instance.AddTween<T>(this);
    }

    public object Target { get; private set; }

    public bool IsComplete { get; private set; }

    public bool WasKilled { get; private set; }

    public bool IsPaused { get; private set; }

    public bool IgnoreTimeScale { get; private set; }

    public string Identifier { get; private set; }

    public float DelayTime { get; private set; }

    public Action onComplete { get; set; }

    public void Update()
    {
        if (!IsPaused) 
        {
            if (IgnoreTimeScale)
            {
                _delayElaspedTime += Time.unscaledDeltaTime;
            }
            else
            {
                _delayElaspedTime += Time.deltaTime;
            }

            if (_delayElaspedTime >= DelayTime)
            {
                if (IsComplete)
                {
                    return;
                }

                if (IsTargetDestroyed())
                {
                    FullKill();
                    return;
                }

                if (IgnoreTimeScale)
                {
                    _elapsedTime += Time.unscaledDeltaTime;
                }
                else
                {
                    _elapsedTime += Time.deltaTime;
                }

                _elapsedTime += Time.deltaTime;
                float t = _elapsedTime / _duration;
                float easedT = Ease(_easeType, t);

                T currentValue;

                if (_reverse)
                {
                    currentValue = Interpolate(_endValue, _startValue, easedT);
                }
                else
                {
                    currentValue = Interpolate(_startValue, _endValue, easedT);
                }

                _onUpdate?.Invoke();
                _onTweenUpdate?.Invoke(currentValue);

                if (_percentThreshold >= 0f && t >= _percentThreshold)
                {
                    _onPercentCompleted?.Invoke();
                    _percentThreshold = -1f;
                }

                if (_elapsedTime >= _duration)
                {
                    _loopsCompleted++;
                    IsComplete = true;

                    if (_pingPong)
                    {
                        _reverse = !_reverse;
                    }

                    if (_loopCount > 0 && _loopsCompleted >= _loopCount)
                    {
                        OnCompleteKill();
                        IsComplete = true;
                    }
                }
            }
        }     
    }

    public T Interpolate(T start, T end, float t)
    {
        if (start is float startFloat && end is float endFloat) 
        {
            return (T)(object)Mathf.LerpUnclamped(startFloat, endFloat, t);
        }

        if (start is Vector3 startVector3 &&  end is Vector3 endVector3)
        {
            return (T)(object)Vector3.LerpUnclamped(startVector3, endVector3, t);
        }

        if (start is Vector2 startVector2 && end is Vector2 endVector2)
        {
            return (T)(object)Vector2.LerpUnclamped(startVector2, endVector2, t);
        }

        if (start is Color startColor && end is Color endColor)
        {
            return (T)(object)Color.Lerp(startColor, endColor, t);
        }  

        if (start is int startInt && end is int endInt)
        {
            return (T)(object)Mathf.LerpUnclamped(startInt, endInt, t);
        }

        throw new NotImplementedException($"Interpoaltion for type {typeof(T)} is not implemented");
    }

    public bool IsTargetDestroyed()
    {
        if (Target is MonoBehaviour mono && mono ==  null)
        {
            return true;
        }

        if (Target is GameObject go && go == null)
        {
            return true;
        }

        if (Target is Delegate del && del.Target == null) 
        {
            return true;
        }

        return false;
    }
    public void FullKill()
    {
        OnCompleteKill();
        WasKilled = true;
        onComplete = null;
    }
    public void OnCompleteKill()
    {
        IsComplete = true;

        _onUpdate = null;
        _onTweenUpdate = null;
        _onPercentCompleted = null;
    }

    public void Pause()
    {
        IsPaused = true;
    }

    public void Resume()
    {
        IsPaused = false;
    }

    public Tween<T> SetEase(EaseType easeType)
    {
        _easeType = easeType;
        return this;
    }

    public Tween<T> SetPingPong(int loopCount = 1)
    {
        _loopCount = loopCount;
        _pingPong = true;
        return this;
    }

    public Tween<T> SetOnComplete(Action onComplete)
    {
        this.onComplete = onComplete;
        return this;
    }

    public Tween<T> SetIgnoreTimeScale()
    {
        IgnoreTimeScale = true;
        return this;
    }

    public Tween<T> SetOnUpdate(Action onUpdate) 
    { 
        _onUpdate = onUpdate;
        return this;
    }

    public Tween<T> SetOnPercentCompleted(float percentCompleted, Action onPercentCompleted) 
    { 
        _percentThreshold = Mathf.Clamp01(percentCompleted);
        _onPercentCompleted = onPercentCompleted;
        return this;
    }

    public Tween<T> SetStartDelay(float delayTime) 
    {
        DelayTime = delayTime;
        return this;
    }

    #region Ease calculations
    // Linear
    public static float Linear(float t)
    {
        return t;
    }

    // Quadratic
    public static float EaseInQuad(float t)
    {
        return t * t;
    }

    public static float EaseOutQuad(float t)
    {
        return t * (2 - t);
    }

    public static float EaseInOutQuad(float t)
    {
        return t < 0.5f ? 2 * t * t : -1 + (4 - 2 * t) * t;
    }

    // Cubic
    public static float EaseInCubic(float t)
    {
        return t * t * t;
    }

    public static float EaseOutCubic(float t)
    {
        t--;
        return 1 + t * t * t;
    }

    public static float EaseInOutCubic(float t)
    {
        if (t < 0.5f)
            return 4 * t * t * t;
        t--;
        return 1 + t * 2 * t * t;
    }

    // Quartic
    public static float EaseInQuart(float t)
    {
        return t * t * t * t;
    }

    public static float EaseOutQuart(float t)
    {
        t--;
        return 1 - t * t * t * t;
    }

    public static float EaseInOutQuart(float t)
    {
        if (t < 0.5f)
            return 8 * t * t * t * t;
        t--;
        return 1 - 8 * t * t * t * t;
    }

    // Quintic
    public static float EaseInQuint(float t)
    {
        return t * t * t * t * t;
    }

    public static float EaseOutQuint(float t)
    {
        t--;
        return 1 + t * t * t * t * t;
    }

    public static float EaseInOutQuint(float t)
    {
        if (t < 0.5f)
            return 16 * t * t * t * t * t;
        t--;
        return 1 + 16 * t * t * t * t * t;
    }

    // Sine
    public static float EaseInSine(float t)
    {
        return 1 - Mathf.Cos(t * Mathf.PI * 0.5f);
    }

    public static float EaseOutSine(float t)
    {
        return Mathf.Sin(t * Mathf.PI * 0.5f);
    }

    public static float EaseInOutSine(float t)
    {
        return 0.5f * (1 - Mathf.Cos(Mathf.PI * t));
    }

    // Exponential
    public static float EaseInExpo(float t)
    {
        return t == 0 ? 0 : Mathf.Pow(2, 10 * (t - 1));
    }

    public static float EaseOutExpo(float t)
    {
        return t == 1 ? 1 : 1 - Mathf.Pow(2, -10 * t);
    }

    public static float EaseInOutExpo(float t)
    {
        if (t == 0) return 0;
        if (t == 1) return 1;
        if (t < 0.5f) return Mathf.Pow(2, 20 * t - 10) * 0.5f;
        return (2 - Mathf.Pow(2, -20 * t + 10)) * 0.5f;
    }

    // Circular
    public static float EaseInCirc(float t)
    {
        return 1 - Mathf.Sqrt(1 - t * t);
    }

    public static float EaseOutCirc(float t)
    {
        t--;
        return Mathf.Sqrt(1 - t * t);
    }

    public static float EaseInOutCirc(float t)
    {
        if (t < 0.5f)
            return (1 - Mathf.Sqrt(1 - 4 * t * t)) * 0.5f;
        t = 2 * t - 2;
        return (Mathf.Sqrt(1 - t * t) + 1) * 0.5f;
    }

    // Back
    public static float EaseInBack(float t)
    {
        const float s = 1.70158f;
        return t * t * ((s + 1) * t - s);
    }

    public static float EaseOutBack(float t)
    {
        const float s = 1.70158f;
        t--;
        return t * t * ((s + 1) * t + s) + 1;
    }

    public static float EaseInOutBack(float t)
    {
        const float s = 1.70158f * 1.525f;
        if (t < 0.5f)
            return (t * t * ((s + 1) * 2 * t - s)) * 0.5f;
        t = 2 * t - 2;
        return (t * t * ((s + 1) * t + s) + 2) * 0.5f;
    }

    // Elastic
    public static float EaseInElastic(float t)
    {
        return Mathf.Sin(13 * Mathf.PI * 0.5f * t) * Mathf.Pow(2, 10 * (t - 1));
    }

    public static float EaseOutElastic(float t)
    {
        return Mathf.Sin(-13 * Mathf.PI * 0.5f * (t + 1)) * Mathf.Pow(2, -10 * t) + 1;
    }

    public static float EaseInOutElastic(float t)
    {
        if (t < 0.5f)
            return 0.5f * Mathf.Sin(13 * Mathf.PI * t) * Mathf.Pow(2, 20 * t - 10);
        return 0.5f * (Mathf.Sin(-13 * Mathf.PI * t) * Mathf.Pow(2, -20 * t + 10) + 2);
    }

    // Bounce
    public static float EaseInBounce(float t)
    {
        return 1 - EaseOutBounce(1 - t);
    }

    public static float EaseOutBounce(float t)
    {
        if (t < 1 / 2.75f)
            return 7.5625f * t * t;
        else if (t < 2 / 2.75f)
        {
            t -= 1.5f / 2.75f;
            return 7.5625f * t * t + 0.75f;
        }
        else if (t < 2.5f / 2.75f)
        {
            t -= 2.25f / 2.75f;
            return 7.5625f * t * t + 0.9375f;
        }
        else
        {
            t -= 2.625f / 2.75f;
            return 7.5625f * t * t + 0.984375f;
        }
    }

    public static float EaseInOutBounce(float t)
    {
        if (t < 0.5f)
            return EaseInBounce(t * 2) * 0.5f;
        return EaseOutBounce(t * 2 - 1) * 0.5f + 0.5f;
    }
    #endregion

    #region Ease enum and Helper
    public static float Ease(EaseType equation, float t)
    {
        switch (equation)
        {
            case EaseType.Linear: return Linear(t);
            case EaseType.EaseInQuad: return EaseInQuad(t);
            case EaseType.EaseOutQuad: return EaseOutQuad(t);
            case EaseType.EaseInOutQuad: return EaseInOutQuad(t);
            case EaseType.EaseInCubic: return EaseInCubic(t);
            case EaseType.EaseOutCubic: return EaseOutCubic(t);
            case EaseType.EaseInOutCubic: return EaseInOutCubic(t);
            case EaseType.EaseInQuart: return EaseInQuart(t);
            case EaseType.EaseOutQuart: return EaseOutQuart(t);
            case EaseType.EaseInOutQuart: return EaseInOutQuart(t);
            case EaseType.EaseInQuint: return EaseInQuint(t);
            case EaseType.EaseOutQuint: return EaseOutQuint(t);
            case EaseType.EaseInOutQuint: return EaseInOutQuint(t);
            case EaseType.EaseInSine: return EaseInSine(t);
            case EaseType.EaseOutSine: return EaseOutSine(t);
            case EaseType.EaseInOutSine: return EaseInOutSine(t);
            case EaseType.EaseInExpo: return EaseInExpo(t);
            case EaseType.EaseOutExpo: return EaseOutExpo(t);
            case EaseType.EaseInOutExpo: return EaseInOutExpo(t);
            case EaseType.EaseInCirc: return EaseInCirc(t);
            case EaseType.EaseOutCirc: return EaseOutCirc(t);
            case EaseType.EaseInOutCirc: return EaseInOutCirc(t);
            case EaseType.EaseInBack: return EaseInBack(t);
            case EaseType.EaseOutBack: return EaseOutBack(t);
            case EaseType.EaseInOutBack: return EaseInOutBack(t);
            case EaseType.EaseInElastic: return EaseInElastic(t);
            case EaseType.EaseOutElastic: return EaseOutElastic(t);
            case EaseType.EaseInOutElastic: return EaseInOutElastic(t);
            case EaseType.EaseInBounce: return EaseInBounce(t);
            case EaseType.EaseOutBounce: return EaseOutBounce(t);
            case EaseType.EaseInOutBounce: return EaseInOutBounce(t);
            default: throw new ArgumentOutOfRangeException(nameof(equation), equation, null);
        }
    }
}

public enum EaseType
{
    // Linear
    Linear,

    // Quadratic
    EaseInQuad,
    EaseOutQuad,
    EaseInOutQuad,

    // Cubic
    EaseInCubic,
    EaseOutCubic,
    EaseInOutCubic,

    // Quartic
    EaseInQuart,
    EaseOutQuart,
    EaseInOutQuart,

    // Quintic
    EaseInQuint,
    EaseOutQuint,
    EaseInOutQuint,

    // Sine
    EaseInSine,
    EaseOutSine,
    EaseInOutSine,

    // Exponential
    EaseInExpo,
    EaseOutExpo,
    EaseInOutExpo,

    // Circular
    EaseInCirc,
    EaseOutCirc,
    EaseInOutCirc,

    // Back
    EaseInBack,
    EaseOutBack,
    EaseInOutBack,

    // Elastic
    EaseInElastic,
    EaseOutElastic,
    EaseInOutElastic,

    // Bounce
    EaseInBounce,
    EaseOutBounce,
    EaseInOutBounce
}
#endregion