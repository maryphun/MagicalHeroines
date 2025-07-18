// #define DEBUG_INFO

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System.Threading;
using System;

#if DEBUG_INFO
using System.Diagnostics;
#endif

namespace NovelEditor
{
    /// <summary>
    /// UIを管理するコンポーネント
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    internal class NovelUIManager : MonoBehaviour
    {
        CanvasGroup NovelCanvas;
        ImageManager imageManager;

        [SerializeField] Transform _charaTransform;
        [SerializeField] NovelBackGround _backGround;
        [SerializeField] DialogueImage _dialogueImage;
        [SerializeField] DialogueText _dialogueText;
        [SerializeField] NameText _nameText;
        [SerializeField] CanvasGroup UIparents;
        public bool canFlush => _dialogueText.canFlush;

        /// <summary>
        /// 初期化用関数
        /// </summary>
        /// <param name="charaFadeTime">キャラがディゾルブで切り替わるのにかかる時間</param>
        /// <param name="nonameDialogueSprite">名前なし用のセリフ枠のSprite</param>
        /// <param name="dialogueSprite">通常のセリフ枠のSprite</param>
        internal void Init(float charaFadeTime, Sprite nonameDialogueSprite, Sprite dialogueSprite)
        {
            NovelCanvas = GetComponent<CanvasGroup>();
            imageManager = new ImageManager(_charaTransform, _backGround, _dialogueImage, charaFadeTime);
            _dialogueImage.SetDialogueSprite(dialogueSprite, nonameDialogueSprite);
        }

        /// <summary>
        /// 立ち絵の情報を新しいデータに合わせてリセットする
        /// </summary>
        /// <param name="data">新しいデータの立ち絵の位置</param>
        /// <param name="isLoad">ロード後かどうか</param>
        internal void Reset(List<Image> data, bool isLoad)
        {
            imageManager.Init(data, isLoad);
            DeleteText();
            _dialogueText.SetDefaultFont();
            _nameText.SetDefaultFont();
        }

        /// <summary>
        /// 残りのセリフを一度に表示する
        /// </summary>
        internal void FlashText()
        {
            _dialogueText.FlushText();
        }

        /// <summary>
        /// テキストを初期化する
        /// </summary>
        internal void DeleteText()
        {
            //テキストを初期化
            _dialogueText.DeleteText();
            _nameText.DeleteText();
        }

        /// <summary>
        /// 立ち絵、背景を含む全てのUIを表示切り替え
        /// </summary>
        /// <param name="display">表示するかどうか</param>
        internal void SetDisplay(bool display)
        {
            if (display)
            {
                NovelCanvas.alpha = 1;
                NovelCanvas.interactable = true;
                NovelCanvas.blocksRaycasts = true;
            }
            else
            {
                NovelCanvas.alpha = 0;
                NovelCanvas.interactable = false;
                NovelCanvas.blocksRaycasts = false;
            }
        }

        /// <summary>
        /// UIの表示の設定
        /// </summary>
        /// <param name="display">UIを表示するかどうか</param>
        internal void SetUIDisplay(bool display)
        {
            if (display)
            {
                UIparents.alpha = 1;
                UIparents.interactable = true;
                UIparents.blocksRaycasts = true;
            }
            else
            {
                UIparents.alpha = 0;
                UIparents.interactable = false;
                UIparents.interactable = false;
            }
        }

        /// <summary>
        /// UIの表示の設定
        /// </summary>
        /// <param name="display">UIを表示するかどうか</param>
        internal async UniTask Fade(float finalAlpha, float time)
        {
            float alpha = 0;

            float alphaSpeed = 0.01f;
            if (time < 0.5)
            {
                alphaSpeed = 0.1f;
            }
            try
            {
                while (alpha != finalAlpha)
                {
                    NovelCanvas.alpha = finalAlpha;
                    await UniTask.Delay(TimeSpan.FromSeconds(time * alphaSpeed));
                    Mathf.MoveTowards(alpha, finalAlpha, alphaSpeed);

                    UnityEngine.Debug.Log("NovelUIManager : Fade");
                }
            }
            catch (OperationCanceledException)
            { }
        }



        /// <summary>
        /// UIの表示の設定
        /// </summary>
        /// <param name="display">UIを表示するかどうか</param>
        internal void SetUIAlpha(float alpha)
        {
            NovelCanvas.alpha = alpha;
        }

        /// <summary>
        /// UIをフェードアウトさせる
        /// </summary>
        /// <param name="time">フェードアウトにかかる時間</param>
        /// <param name="token">使用するCancellationToken</param>
        internal async UniTask<bool> FadeOut(float time, CancellationToken token)
        {
#if DEBUG_INFO
            Stopwatch stopwatch = Stopwatch.StartNew();
#endif

            float alpha = 1.0f;

            try
            {
#if DEBUG_INFO
                UnityEngine.Debug.Log("time = " + time);
                int count = 0;
#endif

                while (alpha > 0.0f)
                {
#if DEBUG_INFO
                    Stopwatch stopwatchLoop = Stopwatch.StartNew();
#endif

                    NovelCanvas.alpha = alpha;

                    // １フレームを待つ
                    await UniTask.NextFrame(token);

                    // より安定な結果が出せるようにループ内で毎回生成する
                    var fps = 1.0f / Time.deltaTime;

                    // １フレームの分を減らす (例 : FPS が 60 の時は、alpha を 31.25 回分を減らす必要がる)
                    alpha -= 1.0f / fps / time;

#if DEBUG_INFO
                    stopwatchLoop.Stop();
                    UnityEngine.Debug.Log("Elapsed (seconds) : " + (stopwatchLoop.ElapsedMilliseconds / 1000.0f) + ", count : " + count++ + ", alpha : " + alpha + ", fps : " + fps);
#endif
                }
            }
            catch (OperationCanceledException)
            { }

#if DEBUG_INFO
            stopwatch.Stop();
            UnityEngine.Debug.Log("Total elapsed time (seconds) : " + (stopwatch.ElapsedMilliseconds / 1000.0f));
#endif

            return true;
        }

        /// <summary>
        /// 次のテキストを設定し、1文字ずつ再生する
        /// </summary>
        /// <param name="data">次のセリフのデータ</param>
        /// <param name="token">使用するCancellationToken</param>
        internal async UniTask<bool> SetNextText(NovelData.ParagraphData.Dialogue data, CancellationToken token)
        {

            _nameText.UpdateNameText(data);
            return await _dialogueText.textUpdate(data, token);
        }

        /// <summary>
        /// 次のセリフの立ち絵、背景を設定する
        /// </summary>
        /// <param name="data">次のセリフのデータ</param>
        /// <param name="token">使用するCancellationToken</param>
        internal async UniTask<bool> SetNextImage(NovelData.ParagraphData.Dialogue data, CancellationToken token)
        {
            DeleteText();
            await imageManager.SetNextImage(data, data.Name != "", token);
            return true;
        }

        /// <summary>
        /// テキストの再生・停止を切り替える
        /// </summary>
        internal void SwitchStopText()
        {
            _dialogueText.IsStop = !_dialogueText.IsStop;
        }

        /// <summary>
        /// テキストの再生を一時停止する
        /// </summary>
        /// <param name="flag">停止するかどうか</param>
        internal void SetStopText(bool flag)
        {
            _dialogueText.IsStop = flag;
        }

        /// <summary>
        /// テキスト再生速度を設定する
        /// </summary>
        /// <param name="speed">新しい速度</param>
        internal void SetTextSpeed(int speed)
        {
            _dialogueText.textSpeed = speed;
        }

        /// <summary>
        /// 現在の背景を取得する
        /// </summary>
        internal Sprite GetNowBack()
        {
            return _backGround.image.sprite;
        }
    }
}
