using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System.Threading;
using System;
using TMPro;
using Assets.SimpleLocalization.Scripts;

namespace NovelEditor
{
    /// <summary>
    /// 選択肢のボタンのコンポーネント
    /// </summary>
    [RequireComponent(typeof(Button))]
    internal class ChoiceButton : MonoBehaviour
    {
        bool _choiced = false;
        Button _button;

        /// <summary>
        /// ボタンにデータとテキストをセットし、選択を待つ
        /// </summary>
        /// <param name="data">ボタンに設定する選択肢のデータ</param>
        /// <param name="token">使用するCancellationToken</param>
        internal async UniTask<NovelData.ChoiceData> SetChoice(NovelData.ChoiceData data, CancellationToken token)
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(Clicked);

            // ローカライズ化されているかを確認
            string text = data.text;
            if (data.localizeID != string.Empty && LocalizationManager.HasKey(data.localizeID))
            {
                text = LocalizationManager.Localize(data.localizeID);
            }
            GetComponentInChildren<TextMeshProUGUI>().text = text;
            try
            {
                await UniTask.WaitUntil(() => _choiced, cancellationToken: token);
            }
            catch
            {
                return null;
            }

            return data;
        }

        void Clicked()
        {
            _choiced = true;
        }
    }
}

