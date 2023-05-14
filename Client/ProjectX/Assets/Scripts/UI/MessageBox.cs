using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MessageBox : UIBase
{
    [SerializeField] Text Message;
    [SerializeField] Button Enter;

    private UnityAction enterEvent;
    
    public void Open(string message, UnityAction enterEvent)
    {
        Message.text    = message;
        this.enterEvent = enterEvent;

        Enter.onClick.AddListener(Close);
    }

    public override void Close()
    {
        enterEvent.Invoke();
        Destroy(gameObject);
    }
}
