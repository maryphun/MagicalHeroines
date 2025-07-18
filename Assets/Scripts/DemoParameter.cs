#if DEMO
// 体験版用管理クラス
public static class DemoParameter
{
    // DEFINE
    public const int EndChapter = 4; // 終了チャプター
    public const int CharacterLevelLimit = 8; // 体験版キャラレベル上限
    public const string DemoStartTextID = "Demo.Start";
    public const string DemoEndTextID = "Demo.End";

    // Variable
    public static bool isDemoEnded = false;
}
#endif