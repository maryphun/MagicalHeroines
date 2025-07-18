using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NayutaEnemyScript : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] public Battler target;

    private void Start()
    {
        target = null; 
    }
}
