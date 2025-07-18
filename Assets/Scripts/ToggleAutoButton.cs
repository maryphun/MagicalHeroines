using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ToggleAutoButton : MonoBehaviour
{
    [Header("Setting")]
    [SerializeField] private Sprite toggleONIcon;
    [SerializeField] private Sprite toggleOFFIcon;

    [Header("References")]    
    [SerializeField] private NovelEditor.NovelPlayer references;

    public void OnClick()
    {
        bool isToggleOn = references.ToggleAutoPlay();

        if (isToggleOn)
        {
            GetComponent<Button>().image.sprite = toggleONIcon;
        }
        else
        {
            GetComponent<Button>().image.sprite = toggleOFFIcon;
        }
    }

    public void ForceStopAutoPlay()
    {
        if (references.IsAutoPlaying)
        {
            OnClick();
        }
    }
}
