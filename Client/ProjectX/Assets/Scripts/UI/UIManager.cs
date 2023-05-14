using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UIManager : Singleton<UIManager>
{
    [SerializeField] GameObject MainUI;
    [SerializeField] MessageBox MessageBox;
    [SerializeField] Toast      Toast;

    public Stack<UIBase> UIStack;

    // Start is called before the first frame update
    protected override void Awake()
    {
        base.Awake();
        UIStack = new Stack<UIBase>();
    }

    private void Update()
    {
        if (true == Input.GetKeyDown(KeyCode.Escape))
            PopStack();
    }

    public void PushStack(UIBase gameObject)
    {
        lock (UIStack)
            UIStack.Push(gameObject);
    }

    public void PopStack()
    {
        lock (UIStack)
        {
            if (0 == UIStack.Count)
                return;

            var UI = UIStack.Pop();
            UI.Close();
        }
    }

    public void ShowMessageBox(string message, UnityAction enterEvent)
    {
        var item = Instantiate(MessageBox.gameObject, MainUI.transform);
        var messageBox = item.GetComponent<MessageBox>();

        messageBox.Open(message, enterEvent);
        PushStack(messageBox);
    }

    public void ShowToast(string message, bool isDestroy)
    {
        var item = Instantiate(Toast.gameObject, MainUI.transform);
        var messageBox = item.GetComponent<Toast>();

        messageBox.Open(message, isDestroy);
    }
}
