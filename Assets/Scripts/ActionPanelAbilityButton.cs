using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ���t�@�����X�擾�p
public class ActionPanelAbilityButton : MonoBehaviour
{
    [Header("References")]
    [SerializeField] public TMPro.TMP_Text abilityName;
    [SerializeField] public UnityEngine.UI.Image abilityIcon;
    [SerializeField] public TMPro.TMP_Text abilityContextText; // �⑫
    [SerializeField] public UnityEngine.UI.Button abilityButton;
}
