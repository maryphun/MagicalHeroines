using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas)), RequireComponent(typeof(CanvasScaler))]
public class RegisterCanvas : MonoBehaviour
{
    void Awake()
    {
        CanvasReferencer.Instance.RegisterCanvas(GetComponent<Canvas>());
    }
}
