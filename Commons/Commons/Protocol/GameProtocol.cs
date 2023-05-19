
public enum GameProtocol : ushort
{
    NONE = 0,
    S_ERROR_CODE,

    C_LOGIN,
    S_LOGIN,

    C_WORLD_SELECT,
    S_WORLD_SELECT,

    S_ACCOUNT_INFO,

    C_HEART_BEAT = 19000,
    S_HEART_BEAT,

    MAX = 20000
}