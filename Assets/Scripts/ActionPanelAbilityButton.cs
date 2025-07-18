using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// レファレンス取得用
public class ActionPanelAbilityButton : MonoBehaviour
{
    [Header("References")]
    [SerializeField] public TMPro.TMP_Text abilityName;
    [SerializeField] public UnityEngine.UI.Image abilityIcon;
    [SerializeField] public TMPro.TMP_Text abilityContextText; // 補足
    [SerializeField] public UnityEngine.UI.Button abilityButton;
}
