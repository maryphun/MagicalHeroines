using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// ボタンをショートカットキーに紐つける
/// </summary>
public class ButtonHotkey : MonoBehaviour
{
    [Header("Setting")]
    [SerializeField] private KeyCode key;
    [SerializeField] private bool isForDialogueSystem = false;
    [SerializeField] private bool onlyAllowOnce = false;

    [Header("References")]
    [SerializeField] private TMPro.TMP_Text text;
    [SerializeField] private UnityEngine.UI.Button button;
    [SerializeField] private EventSystem eventSystem;
    [SerializeField] private GraphicRaycaster raycaster;

    private void Start()
    {
        if (!ReferenceEquals(text, null)) text.text = key.ToString();

        if (ReferenceEquals(eventSystem, null))
        {
            eventSystem = FindObjectOfType<EventSystem>();
        }
    }

    private void Update()
    {
        if (button == null) // null check
        {
            enabled = false;
            return;
        }

        if (!button.IsInteractable()) return;
        if (!button.enabled) return;
        if (!button.gameObject.activeInHierarchy) return;
        if (!isForDialogueSystem && NovelSingletone.Instance.IsPlaying()) return;
        if (isForDialogueSystem && !NovelSingletone.Instance.IsPlaying()) return;

        if (Input.GetKeyDown(key) 
            || (Input.GetMouseButtonDown(1) && key == KeyCode.Escape)) // Esc は→クリックも対応
        {
            if (CheckIsButtonReachable())
            {
                button.onClick.Invoke();

                if (onlyAllowOnce)
                {
                    this.enabled = false;
                }
            }
        }
    }

    private bool CheckIsButtonReachable()
    {
        //Set up the new Pointer Event
        PointerEventData m_PointerEventData = new PointerEventData(eventSystem);
        //Set the Pointer Event Position to that of the mouse position
        m_PointerEventData.position = button.GetComponent<RectTransform>().position;

        //Create a list of Raycast Results
        List<RaycastResult> results = new List<RaycastResult>();

        //Raycast using the Graphics Raycaster and mouse click position
        raycaster.Raycast(m_PointerEventData, results);

        //For every result returned, output the name of the GameObject on the Canvas hit by the Ray
        if (results[0].gameObject == button.gameObject)
        {
            return true;
        }

        return false;
    }
}
