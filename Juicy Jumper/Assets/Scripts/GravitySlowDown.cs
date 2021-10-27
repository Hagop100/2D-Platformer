using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravitySlowDown : MonoBehaviour
{
    [SerializeField] private float timeSpeed;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Time.timeScale = timeSpeed;
    }
}
