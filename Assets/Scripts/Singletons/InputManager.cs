using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : SingletonMonoBehaviour<InputManager>
{
    Dictionary<KeyCode, bool> isKeyDown = new Dictionary<KeyCode, bool>();

    // Start is called before the first frame update
    public void Initialize()
    {
        Debug.Log("Initialize Input manager");
        for (int i = 0; i <= ((int)KeyCode.Z); i++)
        {
            isKeyDown.Add(((KeyCode)i), false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i <= ((int)KeyCode.Z); i++)
        {
            if (isKeyDown[(KeyCode)i])
            {
                isKeyDown[(KeyCode)i] = false;
                Debug.Log(i.ToString() + "is cancelled");
            }
            else
            {
                if (Input.GetKeyDown((KeyCode)i))
                {
                    isKeyDown[(KeyCode)i] = true;
                    Debug.Log(i.ToString() + "is clicked");
                }
            }
        }
    }

    public bool GetIsKeyDown(KeyCode key)
    {
        Debug.Log(key.ToString() + "is checking and the result is " + isKeyDown[key].ToString());
        return isKeyDown[key];
    }
}
