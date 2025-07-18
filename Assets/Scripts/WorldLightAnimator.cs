using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[RequireComponent(typeof(Image))]
public class WorldLightAnimator : MonoBehaviour
{
    [Header("Setting")]
    [SerializeField] private Gradient gredient;
    [SerializeField] private float loopTime;

    [Header("Debug")]
    [SerializeField] private Image imageComponent;
    [SerializeField] private float currentTime;

    // Start is called before the first frame update
    void Start()
    {
        //èâä˙âª
        imageComponent = GetComponent<Image>();
        currentTime = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
        currentTime += Time.deltaTime;

        imageComponent.color = gredient.Evaluate(((currentTime % loopTime) / loopTime));
    }
}
