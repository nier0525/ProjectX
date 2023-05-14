using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Singleton<T> : MonoBehaviour where T : class
{
    public static T Instance;

    protected virtual void Awake()
    {
        if (null == Instance)
            Instance = GetComponent<T>();
    }
}
