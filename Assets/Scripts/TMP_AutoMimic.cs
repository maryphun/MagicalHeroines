using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_Text))]
public class TMP_AutoMimic : MonoBehaviour
{
    [SerializeField] TMP_Text mimicTarget;
    [HideInInspector] TMP_Text self;

    bool isInitialized = false;

    private void Start()
    {
        self = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        if (isInitialized)
        {
            self.text = mimicTarget.text;
            return;
        }
        isInitialized = true;
    }

    private void Update()
    {
        self.text = mimicTarget.text;
    }
}
