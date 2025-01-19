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


    //Example of how to use with some functions

    //TweenManager.TweenScale(gameObject, _startScale, Vector3.one* 1.25f, 0.75f)
    //            .SetEase(EaseType.EaseInOutBack)
    //            .SetPingPong(-1)
    //            .SetOnPercentCompleted(0.5f, OnHalfWay);

    #region static helper methods
    #region misc methods
    //method to gradually change a float 
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

    #region rendering methods
    //method to change opacity of object
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

    //overload method for tween sprite alpha for a group of sprites
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
    #endregion

    #region transform methods
    //method to change scale of object 
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

    //method to move an object via transform using vector 3
    public static Tween<Vector3> TransformMove3D(GameObject gameObject, Vector3 startPosition, Vector3 endPosition, float duration)
    {
        string identifier = $"{gameObject.transform.GetInstanceID()}_Move";
        Transform transform = gameObject.transform;

        Tween<Vector3> tween = new Tween<Vector3>(transform, identifier, startPosition, endPosition, duration, value =>
        {
            transform.position = value;
        });

        return tween;
    }

    //method to move an object via transform using vector 2
    public static Tween<Vector2> TransformMove2D(GameObject gameObject, Vector2 startPosition, Vector3 endPosition, float duration)
    {
        string identifier = $"{gameObject.transform.GetInstanceID()}_Move";
        Transform transform = gameObject.transform;

        Tween<Vector2> tween = new Tween<Vector2>(transform, identifier, startPosition, endPosition, duration, value =>
        {
            transform.position = value;
        });

        return tween;
    }

    //method to move an object on x axis via transform using float
    public static Tween<float> TransformMoveX(GameObject gameObject, float startX, float endX, float duration)
    {
        string identifier = $"{gameObject.transform.GetInstanceID()}_MoveX";
        Transform transform = gameObject.transform;

        Tween<float> tween = new Tween<float>(transform, identifier, startX, endX, duration, value =>
        {
            Vector3 position = transform.position;
            position.x = value;
            transform.position = position;
        });

        return tween;
    }

    //method to move an object on y axis via transform using float
    public static Tween<float> TransformMoveY(GameObject gameObject, float startY, float endY, float duration)
    {
        string identifier = $"{gameObject.transform.GetInstanceID()}_MoveY";
        Transform transform = gameObject.transform;

        Tween<float> tween = new Tween<float>(transform, identifier, startY, endY, duration, value =>
        {
            Vector3 position = transform.position;
            position.y = value;
            transform.position = position;
        });

        return tween;
    }

    //method to move an object on z axis via transform using float 
    public static Tween<float> TransformMoveZ(GameObject gameObject, float startZ, float endZ, float duration)
    {
        string identifier = $"{gameObject.transform.GetInstanceID()}_MoveZ";
        Transform transform = gameObject.transform;

        Tween<float> tween = new Tween<float>(transform, identifier, startZ, endZ, duration, value =>
        {
            Vector3 position = transform.position;
            position.z = value;
            transform.position = position;
        });

        return tween;
    }

    //method to rotate an object using float that converts into euler angles
    public static Tween<float> TransformRotate(GameObject gameObject, float startAngle, float endAngle, float duration)
    {
        string identifier = $"{gameObject.transform.GetInstanceID()}_Rotate";
        Transform transform = gameObject.transform;

        Tween<float> tween = new Tween<float>(transform, identifier, startAngle, endAngle, duration, value =>
        {
            Vector3 eulerAngles = transform.eulerAngles;
            eulerAngles.y = value;
            transform.eulerAngles = eulerAngles;
        });

        return tween;
    }

    //method to make object look at target position using vector 3
    public static Tween<Vector3> TransformLookAt(GameObject gameObject, Vector3 targetPosition, float duration)
    {
        string identifier = $"{gameObject.transform.GetInstanceID()}_LookAt";
        Transform transform = gameObject.transform;

        Vector3 startDirection = transform.forward;
        Vector3 targetDirection = (targetPosition - transform.position).normalized;

        Tween<Vector3> tween = new Tween<Vector3>(transform, identifier, startDirection, targetDirection, duration, value =>
        {
            Quaternion lookRotation = Quaternion.LookRotation(value);
            transform.rotation = lookRotation;
        });

        return tween;
    }

    //method to scale the object using vector 3
    public static Tween<Vector3> TransformScale(GameObject gameObject, Vector3 startScale, Vector3 endScale, float duration)
    {
        string identifier = $"{gameObject.transform.GetInstanceID()}_Scale";
        Transform transform = gameObject.transform;

        Tween<Vector3> tween = new Tween<Vector3>(transform, identifier, startScale, endScale, duration, value =>
        {
            transform.localScale = value;
        });

        return tween;
    }

    //method to scale x value of game object using float
    public static Tween<float> TransformScaleX(GameObject gameObject, float startX, float endX, float duration)
    {
        string identifier = $"{gameObject.transform.GetInstanceID()}_ScaleX";
        Transform transform = gameObject.transform;

        Tween<float> tween = new Tween<float>(transform, identifier, startX, endX, duration, value =>
        {
            Vector3 scale = transform.localScale;
            scale.x = value;
            transform.localScale = scale;
        });

        return tween;
    }

    //method to scale y value of game object using float
    public static Tween<float> TransformScaleY(GameObject gameObject, float startY, float endY, float duration)
    {
        string identifier = $"{gameObject.transform.GetInstanceID()}_ScaleY";
        Transform transform = gameObject.transform;

        Tween<float> tween = new Tween<float>(transform, identifier, startY, endY, duration, value =>
        {
            Vector3 scale = transform.localScale;
            scale.y = value;
            transform.localScale = scale;
        });

        return tween;
    }

    //method to scale z value of game object using float
    public static Tween<float> TransformScaleZ(GameObject gameObject, float startZ, float endZ, float duration)
    {
        string identifier = $"{gameObject.transform.GetInstanceID()}_ScaleZ";
        Transform transform = gameObject.transform;

        Tween<float> tween = new Tween<float>(transform, identifier, startZ, endZ, duration, value =>
        {
            Vector3 scale = transform.localScale;
            scale.z = value;
            transform.localScale = scale;
        });

        return tween;
    }
    #endregion

    #region 3d rigidbody methods
    //method to move a 3d rigidbody using vector 3 
    public static Tween<Vector3> RigidBody3DMove(GameObject gameObject, Vector3 startPosition, Vector3 endPosition, float duration)
    {
        Rigidbody rigidbody = gameObject.GetComponent<Rigidbody>();
        string identifier = $"{rigidbody.GetInstanceID()}_Move";
        
        Tween<Vector3> tween = new Tween<Vector3>(rigidbody, identifier, startPosition, endPosition, duration, value =>
        {
            rigidbody.MovePosition(value);
        });

        return tween;
    }

    //method to move a 3d rigibody on x axis using float
    public static Tween<float> RigidBody3DMoveX(GameObject gameObject, float startX, float endX, float duration)
    {
        Rigidbody rigidbody = gameObject.GetComponent<Rigidbody>();
        string identifier = $"{rigidbody.GetInstanceID()}_MoveX";

        Tween<float> tween = new Tween<float>(rigidbody, identifier, startX, endX, duration, value =>
        {
            Vector3 position = rigidbody.position;
            position.x = value;
            rigidbody.MovePosition(position);
        });

        return tween;
    }

    //method to move a 3d rigidbody on y axis using float
    public static Tween<float> RigidBody3DMoveY(GameObject gameObject, float startY, float endY, float duration)
    {
        Rigidbody rigidbody = gameObject.GetComponent<Rigidbody>();
        string identifier = $"{rigidbody.GetInstanceID()}_MoveY";

        Tween<float> tween = new Tween<float>(rigidbody, identifier, startY, endY, duration, value =>
        {
            Vector3 position = rigidbody.position;
            position.y = value;
            rigidbody.MovePosition(position);
        });

        return tween;
    }

    //method to move a 3d rigidbody on z axis using float
    public static Tween<float> RigidBody3DMoveZ(GameObject gameObject, float startZ, float endZ, float duration)
    {
        Rigidbody rigidbody = gameObject.GetComponent<Rigidbody>();
        string identifier = $"{rigidbody.GetInstanceID()}_MoveZ";

        Tween<float> tween = new Tween<float>(rigidbody, identifier, startZ, endZ, duration, value =>
        {
            Vector3 position = rigidbody.position;
            position.z = value;
            rigidbody.MovePosition(position);
        });

        return tween;
    }

    //method to rotate a 3d rigidbody using a float that converts into euler angles
    public static Tween<float> RigidBody3DRotate(GameObject gameObject, float startAngle, float endAngle, float duration)
    {
        Rigidbody rigidbody = gameObject.GetComponent<Rigidbody>();
        string identifier = $"{rigidbody.GetInstanceID()}_Rotate";

        Tween<float> tween = new Tween<float>(rigidbody, identifier, startAngle, endAngle, duration, value =>
        {
            Vector3 eulerAngles = rigidbody.rotation.eulerAngles;
            eulerAngles.y = value;
            rigidbody.MoveRotation(Quaternion.Euler(eulerAngles));
        });

        return tween;
    }

    //method to make object rotate to look at target position using vector 3
    public static Tween<Vector3> RigidBody3DLookAt(GameObject gameObject, Vector3 targetPosition, float duration)
    {
        Rigidbody rigidbody = gameObject.GetComponent<Rigidbody>();
        string identifier = $"{rigidbody.GetInstanceID()}_LookAt";

        Vector3 startDirection = rigidbody.rotation * Vector3.forward;
        Vector3 targetDirection = (targetPosition - rigidbody.position).normalized;

        Tween<Vector3> tween = new Tween<Vector3>(rigidbody, identifier, startDirection, targetDirection, duration, value =>
        {
            Quaternion lookRotation = Quaternion.LookRotation(value);
            rigidbody.MoveRotation(lookRotation);
        });

        return tween;
    }
    #endregion

    #region 2d rigidbody methods
    //method to move a 2d rigidbody using vector 2
    public static Tween<Vector2> RigidBody2DMove(GameObject gameObject, Vector3 startPosition, Vector3 endPosition, float duration)
    {
        Rigidbody2D rigidbody = gameObject.GetComponent<Rigidbody2D>();
        string identifier = $"{rigidbody.GetInstanceID()}_Move";

        Tween<Vector2> tween = new Tween<Vector2>(rigidbody, identifier, startPosition, endPosition, duration, value =>
        {
            rigidbody.MovePosition(value);
        });

        return tween;
    }

    //method to move a 2d rigibody on x axis using float
    public static Tween<float> RigidBody2DMoveX(GameObject gameObject, float startX, float endX, float duration)
    {
        Rigidbody2D rigidbody = gameObject.GetComponent<Rigidbody2D>();
        string identifier = $"{rigidbody.GetInstanceID()}_MoveX";

        Tween<float> tween = new Tween<float>(rigidbody, identifier, startX, endX, duration, value =>
        {
            Vector3 position = rigidbody.position;
            position.x = value;
            rigidbody.MovePosition(position);
        });

        return tween;
    }

    //method to move a 2d rigidbody on y axis using float
    public static Tween<float> RigidBody2DMoveY(GameObject gameObject, float startY, float endY, float duration)
    {
        Rigidbody2D rigidbody = gameObject.GetComponent<Rigidbody2D>();
        string identifier = $"{rigidbody.GetInstanceID()}_MoveY";

        Tween<float> tween = new Tween<float>(rigidbody, identifier, startY, endY, duration, value =>
        {
            Vector3 position = rigidbody.position;
            position.y = value;
            rigidbody.MovePosition(position);
        });

        return tween;
    }

    //method to rotate a 2d rigidbody using a float that converts into euler angles
    public static Tween<float> RigidBody2DRotate(GameObject gameObject, float startAngle, float endAngle, float duration)
    {
        Rigidbody2D rigidbody = gameObject.GetComponent<Rigidbody2D>();
        string identifier = $"{rigidbody.GetInstanceID()}_Rotate";

        Tween<float> tween = new Tween<float>(rigidbody, identifier, startAngle, endAngle, duration, value =>
        {
            rigidbody.MoveRotation(value);
        });

        return tween;
    }
    #endregion

    #region UI Image methods
    //method to change colour of a ui image
    public static Tween<Color> ImageColor(GameObject gameObject, Color startColor, Color endColor, float duration)
    {
        UnityEngine.UI.Image image = gameObject.GetComponent<UnityEngine.UI.Image>();
        string identifier = $"{image.GetInstanceID()}_Color";

        Tween<Color> tween = new Tween<Color>(image, identifier, startColor, endColor, duration, value =>
        {
            image.color = value;
        });

        return tween;
    }
    
    //method to change alpha of ui image
    public static Tween<float> ImageFade(GameObject gameObject, float startAlpha, float endAlpha, float duration)
    {
        UnityEngine.UI.Image image = gameObject.GetComponent<UnityEngine.UI.Image>();
        string identifier = $"{image.GetInstanceID()}_Fade";

        Tween<float> tween = new Tween<float>(image, identifier, startAlpha, endAlpha, duration, value =>
        {
            Color color = image.color;
            color.a = value;
            image.color = color;
        });

        return tween;
    }
    #endregion
    #endregion
}
