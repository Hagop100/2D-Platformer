using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Death : MonoBehaviour
{

    [SerializeField] private PlayerController player;

    private Vector2 playerPosition;

    private void Start()
    {
        playerPosition = player.transform.position;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        player.transform.position = playerPosition;
    }
}
