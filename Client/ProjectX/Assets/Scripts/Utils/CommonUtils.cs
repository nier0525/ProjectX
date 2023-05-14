using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommonUtils : MonoBehaviour
{
    public static void ForceDestroy(string reason = null)
    {
#if UNITY_EDITOR
        if (null != reason)
            Debug.Log(reason);
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
