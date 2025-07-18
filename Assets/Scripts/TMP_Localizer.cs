using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Assets.SimpleLocalization.Scripts;

[RequireComponent(typeof(TMP_Text))]
public class TMP_Localizer : MonoBehaviour
{
    public string LocalizationKey;
    public string Extra;

    public void Start()
    {
        Localize();
        LocalizationManager.LocalizationChanged += Localize;
    }

    public void OnDestroy()
    {
        LocalizationManager.LocalizationChanged -= Localize;
    }

    private void Localize()
    {
        GetComponent<TMP_Text>().SetText(LocalizationManager.Localize(LocalizationKey) + Extra);

        if (GetComponent<TMP_Text>().font != DynamicFont.Instance.GetFont())
        {
            GetComponent<TMP_Text>().font = DynamicFont.Instance.GetFont();
        }
    }
}
