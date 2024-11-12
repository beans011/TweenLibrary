using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TweenManager : MonoBehaviour
{
    private static TweenManager _instance;
    public static TweenManager Instance
    {
        get 
        { 
            if ( _instance == null )
            {
                GameObject manager = new GameObject("TweenManager");
                _instance = manager.AddComponent<TweenManager>();
            }

            return _instance;
        }
    }

    private Dictionary<string, ITween> _activeTweens = new Dictionary<string, ITween>();

    private void Update()
    {
        foreach (var pair in _activeTweens.ToList()) 
        { 
            ITween tween = pair.Value;
            tween.Update();

            if (tween.IsComplete && !tween.WasKilled) 
            { 
                if (tween.onComplete != null)
                {
                    tween.onComplete.Invoke();
                    tween.onComplete = null;
                }

                RemoveTween(pair.Key);
            }

            if (tween.WasKilled)
            {
                RemoveTween(pair.Key);
            }
        }
    }

    public void AddTween<T>(ITween tween)
    {
        if (_activeTweens.ContainsKey(tween.Identifier)) 
        {
            _activeTweens[tween.Identifier].OnCompleteKill();
        }

        _activeTweens[tween.Identifier] = tween;
    }

    public void RemoveTween(string identifier) 
    { 
        _activeTweens.Remove(identifier);
    }

    #region static helper methods

    public static Tween<float> TweenSpriteAlpha(GameObject gameObject, float startAlpha, float endAlpha, float duration)
    { 
        SpriteRenderer spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        string identifier = $"{spriteRenderer.GetInstanceID()}_Alpha";

        Tween<float> tween = new Tween<float>(gameObject, identifier, startAlpha, endAlpha, duration, value =>
        {
            Color color = spriteRenderer.color;
            color.a = value;
            spriteRenderer.color = color;
        });

        return tween;
    }

    //overload method for tweeni sprite alpha for a group of sprites
    public static Tween<float> TweenSpriteAlpha(SpriteRenderer spriteRenderer, float startAlpha, float endAlpha, float duration)
    {
        string identifier = $"{spriteRenderer.GetInstanceID()}_Alpha";

        Tween<float> tween = new Tween<float>(spriteRenderer, identifier, startAlpha, endAlpha, duration, value =>
        {
            Color color = spriteRenderer.color;
            color.a = value;
            spriteRenderer.color = color;
        });

        return tween;
    }

    public static Tween<Vector3> TweenScale(GameObject gameObject, Vector3 startScale, Vector3 endScale, float duration)
    {
        string identifer = $"{gameObject.transform.GetInstanceID()}_Scale";
        Transform transform = gameObject.transform;

        Tween<Vector3> tween = new Tween<Vector3>(gameObject, identifer, startScale, endScale, duration, value =>
        {
            transform.localScale = value;
        });

        return tween;
    }

    public static Tween<float> TweenFloat(Func<float> getFloatToTween, Action<float> setFloatToTween, float endValue, float duration)
    {
        string identifier = $"{getFloatToTween.Target.GetHashCode()}_Float";
        object target = getFloatToTween.Target;

        float startValue = getFloatToTween();

        Tween<float> tween = new Tween<float>(target, identifier, startValue, endValue, duration, value =>
        { 
            setFloatToTween(value);
        });

        return tween;
    }
    #endregion
}
