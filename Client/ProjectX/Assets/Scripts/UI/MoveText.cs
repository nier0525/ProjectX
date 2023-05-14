using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MoveText : MonoBehaviour
{
    [SerializeField] RectTransform BackGround;
    [SerializeField] RectTransform Text;
    [SerializeField] int    Speed;
    [SerializeField] bool   Loop;

    [HideInInspector] public int LoopCount;
    private Vector2 startPosition;
    private float   endPositionX;    

    private void Start()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(Text);

        var textWidth = Text.GetComponent<Text>().preferredWidth;
        Text.sizeDelta = new Vector2(textWidth + 100, Text.rect.height);

        var textHalf = Text.rect.width / 2 + (BackGround.rect.width / 2);

        endPositionX    = Text.anchoredPosition.x + textHalf;
        startPosition   = new Vector2(-endPositionX, Text.anchoredPosition.y);
        Text.anchoredPosition = startPosition;

        LoopCount = 0;
        StartCoroutine(TryMoveText());
    }

    private IEnumerator TryMoveText()
    {
        while (true)
        {
            Text.Translate(Vector2.right * Speed * Time.deltaTime);

            if (true == IsEnd())
            {
                ++LoopCount;
                if (true == Loop)
                    Text.anchoredPosition = startPosition;
                else
                    break;
            }
            yield return null;
        }
        yield return null;
    }

    public bool IsEnd()
    {
        return endPositionX < Text.anchoredPosition.x;
    }
}
