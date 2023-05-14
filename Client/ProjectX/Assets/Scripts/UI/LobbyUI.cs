using Core;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    public enum LobbyUIState
    {
        Login,
        Character,
        Option,
    }

    [SerializeField] GameObject[] UI;
    private LobbyUIState state;
    
    [Header("Login Component")]
    [SerializeField] InputField ID;
    [SerializeField] InputField Password;
    
    private void Awake()
    {
        for (int i = 0; i < UI.Length; i++)
            UI[i].SetActive(false);

        state = LobbyUIState.Login;
        UI[(int)state].SetActive(true);
    }

    private void Start()
    {
        PacketUtil.RegisterCallback(GameProtocol.S_ACCOUNT_INFO, OnAccountInfo);
    }

    private void Update()
    {
        
    }

    // 서버에서 로그인에 성공한 경우 호출된다
    private void OnAccountInfo(PacketSession session, ArraySegment<byte> buffer)
    {
        var packet = PacketUtil.UnPack<AccountInfo>(buffer);
        var user = session as GameSession;

        user.AccountID = packet.accountID;
        StartCoroutine(OnLoginSuccess(packet));
    }

    private IEnumerator OnLoginSuccess(AccountInfo accountInfo)
    {
        // Character List Setting

        
        // Move Animantion
        var rect = GetCurrentUI().GetComponent<RectTransform>();
        var wait = new WaitForSeconds(0.001f);
        for (int i = 0; i < 600; ++i)
        {
            rect.anchoredPosition = new Vector2(0, rect.anchoredPosition.y + 2);
            yield return wait;
        }

        // Change UI
        FetchState(LobbyUIState.Character);
        rect.anchoredPosition = Vector2.zero;
        yield return null;
    }

    public void FetchState(LobbyUIState changeState)
    {
        UI[(int)state].SetActive(false);
        state = changeState;
        UI[(int)state].SetActive(true);
    }

    public GameObject GetCurrentUI()
    {
        return UI[(int)state];
    }

    public void SendToLogin(bool isSignUp)
    {
        if (true == string.IsNullOrEmpty(ID.text) || true == string.IsNullOrEmpty(Password.text))
            UIManager.Instance.ShowToast("Don't Blank To ID And Password", false);
        else
            Network.Instance.Send(GameProtocol.C_LOGIN, new LoginInfo() { id = ID.text, password = Password.text, isSignUp = isSignUp });
    }
}
