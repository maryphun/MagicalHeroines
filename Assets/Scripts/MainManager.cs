using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainManager : MonoBehaviour
{
    [SerializeField] private Canvas currentCanvas;

    // Start is called before the first frame update
    void Start()
    {
        UIManager.Instance.Initialize();
        InputManager.Instance.Initialize();
    }

    private void Update()
    {
#if DEBUG_MODE
        if (Input.GetKeyDown(KeyCode.Q))
        {
            UIManager.Instance.StartDialogueBox(currentCanvas);
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            UIManager.Instance.StopDialogueBox();
        }
#endif
    }
}
