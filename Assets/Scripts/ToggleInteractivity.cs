using UnityEngine;
using UnityEngine.UI;

public class ToggleInteractivity : MonoBehaviour
{
    private Toggle toggle;

    private void Start()
    {
        // Get the Toggle component attached to the GameObject
        toggle = GetComponent<Toggle>();

        // Subscribe to the onValueChanged event of the Toggle
        toggle.onValueChanged.AddListener(OnToggleValueChanged);
    }

    private void OnToggleValueChanged(bool isOn)
    {
        // Check if the Toggle is turned on
        if (isOn)
        {
            // Disable interactivity when the Toggle is on
            toggle.interactable = false;
        }
        else
        {
            // Enable interactivity when the Toggle is off
            toggle.interactable = true;
        }
    }
}