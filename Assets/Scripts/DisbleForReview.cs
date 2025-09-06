using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisbleForReview : MonoBehaviour
{
#if STEAM_REVIEW
    void Start()
    {
        gameObject.SetActive(false);
    }
#endif
}
