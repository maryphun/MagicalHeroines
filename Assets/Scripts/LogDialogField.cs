using UnityEngine;

public class LogDialogField : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMPro.TMP_Text _name;
    [SerializeField] private TMPro.TMP_Text _dialog;
    [SerializeField] private UnityEngine.UI.Image _sprite;

    public TMPro.TMP_Text Name { get { return _name; } }
    public TMPro.TMP_Text Dialog { get { return _dialog; } }
    public UnityEngine.UI.Image Sprite { get { return _sprite; } }
}
