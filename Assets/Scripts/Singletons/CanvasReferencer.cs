using UnityEngine;
using UnityEngine.UI;

public class CanvasReferencer : SingletonMonoBehaviour<CanvasReferencer>
{
    public Canvas MainCanvas { get { return mainCanvas; } }
    [SerializeField] private Canvas mainCanvas;
    public CanvasScaler MainCanvasScaler { get { return mainCanvasScaler; } }
    [SerializeField] private CanvasScaler mainCanvasScaler;

    public void RegisterCanvas(Canvas reference)
    {
        mainCanvas = reference;
        mainCanvasScaler = reference.GetComponent<CanvasScaler>();

        if (ReferenceEquals(mainCanvasScaler, null))
        {
            Debug.LogWarning("CanvasScaler not found.");
        }
    }

    public float GetScaleFactor()
    {
        return mainCanvas.scaleFactor;
    }
    public Vector2 GetResolution()
    {
        return mainCanvasScaler.referenceResolution * mainCanvas.scaleFactor;
    }
}
