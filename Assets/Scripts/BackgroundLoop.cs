using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundLoop : MonoBehaviour
{
    [Header("Setting")]
    [SerializeField] private float startPosition = 0.0f;
    [SerializeField] private float endPosition = 0.0f;
    [SerializeField] private float speed = 0.0f;

    [Header("Debug")]
    [SerializeField] private float delta;

    RectTransform rect;

    // Start is called before the first frame update
    void Start()
    {
        rect = GetComponent<RectTransform>();

        rect.localPosition = new Vector3(rect.localPosition.x, startPosition, rect.localPosition.z);
        delta = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
        delta += speed * Time.deltaTime;
        rect.localPosition = new Vector3(rect.localPosition.x, Mathf.Min(startPosition + delta, endPosition), rect.localPosition.z);

        if (startPosition + delta >= endPosition)
        {
            // reset
            rect.localPosition = new Vector3(rect.localPosition.x, startPosition, rect.localPosition.z);
            delta = 0.0f;
        }
    }
}
