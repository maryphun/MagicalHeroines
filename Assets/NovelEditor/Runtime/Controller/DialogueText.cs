using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Cysharp.Threading.Tasks;
using System.Threading;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using Assets.SimpleLocalization.Scripts;

namespace NovelEditor
{
    /// <summary>
    /// セリフのテキスト表示を管理するクラス
    /// </summary>
    [RequireComponent(typeof(TextMeshProUGUI))]
    internal class DialogueText : MonoBehaviour
    {
        TextMeshProUGUI tmpro;
        internal int textSpeed = 6;
        public bool IsStop = false;
        string nowText;

        TMP_FontAsset defaultFont;
        float defaultFontSize;
        Color defaultFontColor;

        public bool canFlush { get; private set; }

        void Awake()
        {
            tmpro = GetComponent<TextMeshProUGUI>();
            //再生時のフォントを保存
            defaultFont = tmpro.font;
            defaultFontSize = tmpro.fontSize;
            defaultFontColor = tmpro.color;
        }

        /// <summary>
        /// テキストを1文字ずつ再生
        /// </summary>
        /// <param name="data">次のセリフのデータ</param>
        /// <param name="token">使用するCancellationToken</param>
        /// <returns>終了したか</returns>
        internal async UniTask<bool> textUpdate(NovelData.ParagraphData.Dialogue data, CancellationToken token)
        {
            UpdateFont(data);
            //再生が終わったら通知
            if (data.localizationID != string.Empty && LocalizationManager.HasKey(data.localizationID))
            {
                return await PlayText(LocalizationManager.Localize(data.localizationID), token);
            }
            else
            {
                Debug.Log(data.localizationID + " does not exist");
                return await PlayText(data.text, token);
            }
        }

        /// <summary>
        /// ゲーム開始時のフォントに戻す
        /// </summary>
        internal void SetDefaultFont()
        {
            //tmpro.font = defaultFont;
            tmpro.fontSize = defaultFontSize;
            tmpro.color = defaultFontColor;
        }

        /// <summary>
        /// テキストを削除する
        /// </summary>
        internal void DeleteText()
        {
            tmpro.text = "";
        }

        /// <summary>
        /// 次のセリフに合わせてフォントを変更する
        /// </summary>
        /// <param name="data">次のセリフのデータ</param>
        internal void UpdateFont(NovelData.ParagraphData.Dialogue data)
        {
            if (data.changeFont)
            {
                tmpro.color = data.fontColor;
                tmpro.fontSize = data.fontSize;

                if (data.font != null)
                    tmpro.font = data.font;
            }
        }

        /// <summary>
        /// リッチテキストタグを含むセリフを1文字ずつ再生する
        /// </summary>
        /// <param name="text">再生するtext</param>
        /// <param name="token">CancellationToken使用する</param>
        private async UniTask<bool> PlayText(string text, CancellationToken token)
        {
            canFlush = false;
            tmpro.text = "";
            nowText = text;

            List<string> words = SplitText(text);

            int wordCnt = 0;
            int msecPerCharacter = 250 / textSpeed;

            try
            {
                while (wordCnt < words.Count)
                {
                    canFlush = wordCnt > 0;
                    await UniTask.Delay(msecPerCharacter, cancellationToken: token);

                    // msecPerCharacter の後は必ず 1 文字を追加する
                    int addCount = 1;

                    // 処理落ちが発生した時は追いつける
                    for (int msecDeltaTime = (int)(Time.deltaTime * 1000.0f) - msecPerCharacter; msecDeltaTime > msecPerCharacter; msecDeltaTime -= msecPerCharacter)
                    {
                        addCount++;
                    }

#if DEBUG_INFO
                    Debug.Log("Character add count : " + addCount);
#endif

                    // ダイアログに文字を追加する
                    for (int i = 0; i < addCount; ++i)
                    {
                        if (wordCnt >= words.Count)
                        {
                            break;
                        }

                        tmpro.text += words[wordCnt];
                        tmpro.richText = true;
                        wordCnt++;
                    }

                    await UniTask.WaitUntil(() => !IsStop);
                }
            }
            catch (OperationCanceledException)
            { }


            return true;
        }

        /// <summary>
        /// 再生途中のテキストを全て表示する
        /// </summary>
        internal void FlushText()
        {
            tmpro.text = nowText;
        }

        /// <summary>
        /// 文字を1文字ずつ分ける。リッチテキストタグを1文字として数える
        /// </summary>
        /// <param name="text">使用するtext</param>
        /// <returns>分割した文字のリスト</returns>
        List<string> SplitText(string text)
        {
            List<string> words = new List<string>();

            foreach (string str in text.Split('<'))
            {
                string[] split = str.Split('>');

                int i = 0;
                if (split.Length == 2)
                {
                    words.Add('<' + split[0] + '>');
                    i = 1;
                }
                split[i] = split[i].Replace("&lt;", "<");
                split[i] = split[i].Replace("&gt;", ">");
                words.AddRange(split[i].Select(c => c.ToString()));
            }
            return words;
        }
    }

}
