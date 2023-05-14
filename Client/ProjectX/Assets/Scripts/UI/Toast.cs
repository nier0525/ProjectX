using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Toast : MonoBehaviour
{
    [SerializeField] Image  BackGround;
    [SerializeField] Text   Message;
    private bool isDestroy;

    public void Open(string message, bool isDestroy)
    {
        Message.text = message;
        this.isDestroy = isDestroy;

        StartCoroutine(LateClose());
    }

    private IEnumerator LateClose()
    {
        var moving = GetComponent<MoveText>();        

        while (true)
        {
            if (1 <= moving.LoopCount)
                break;
            yield return null;
        }        

        if (true == isDestroy)
            CommonUtils.ForceDestroy();

        Destroy(gameObject);
        yield return null;
    }
}
