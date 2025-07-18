using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShakeManager : SingletonMonoBehaviour<ShakeManager>
{
    public Coroutine ShakeObject(RectTransform rectTransform, float duration, float magnitude)
    {
        return StartCoroutine(ShakeObjectAnimation(rectTransform, duration, magnitude));
    }

    /// <summary>
    /// í‚é~
    /// </summary>
    public void StopObjectShake(Coroutine reference)
    {
        StopCoroutine(reference);
    }

    IEnumerator ShakeObjectAnimation(RectTransform rectTransform, float duration, float magnitude)
    {
        Vector2 originalPosition = rectTransform.position;

        for (float timeElapsed = 0.0f; timeElapsed < duration; timeElapsed+=Time.deltaTime)
        {
            Vector2 newPosition = Vector2.MoveTowards(originalPosition, originalPosition + new Vector2(Random.Range(-magnitude, magnitude), 
                                                      Random.Range(-magnitude, magnitude)), 
                                                      magnitude);

            rectTransform.position = newPosition;
            yield return null;
        }

        // ñﬂÇ…ñﬂÇ∑
        rectTransform.position = originalPosition;
    }
}
