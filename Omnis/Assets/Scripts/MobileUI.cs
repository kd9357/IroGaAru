using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobileUI : MonoBehaviour {

    // Singleton properties
    public static MobileUI Instance;
    private static int _refCount;

    // Input States
    public static bool SwitchColor;
    public static bool Left;
    public static bool Right;
    public static bool Attack;
    public static bool Jump;

    // If not on phone, destory
    void Start()
    {
#if (UNITY_ANDROID || UNITY_IPHONE)
        ++_refCount;
        if (_refCount > 1) {
            DestroyImmediate(this.gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this.gameObject);
#else
        Destroy(this.gameObject);
#endif
    }

    void OnDestroy()
    {
        --_refCount;
        if (_refCount == 0)
            Instance = null;
    }

    public void SetSwitchColor(bool val)
    {
        SwitchColor = val;
    }

    public void SetLeft(bool val)
    {
        Left = val;
    }

    public void SetRight(bool val)
    {
        Right = val;
    }

    public void SetAttack(bool val)
    {
        Attack = val;
    }

    public void SetJump(bool val)
    {
        Jump = val;
    }

    public bool GetSwitchColor()
    {
        return SwitchColor;
    }

    public bool GetLeft()
    {
        return Left;
    }

    public bool GetRight()
    {
        return Right;
    }

    public bool GetAttack()
    {
        return Attack;
    }

    public bool GetJump()
    {
        return Jump;
    }
}
