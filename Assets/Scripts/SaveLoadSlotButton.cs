using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Reference—p
/// </summary>
public class SaveLoadSlotButton : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text slotText;
    [SerializeField] private TMP_Text dateText;

    [HideInInspector] public Button Button { get { return button; } }
    [HideInInspector] public TMP_Text SlotText { get { return slotText; } }
    [HideInInspector] public TMP_Text DateText { get { return dateText; } }
}
