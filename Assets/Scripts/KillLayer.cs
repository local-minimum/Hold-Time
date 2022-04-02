using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void PlayerKilledEvent();

public class KillLayer : MonoBehaviour
{
    public static event PlayerKilledEvent OnPlayerKilled;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var player = collision.gameObject.GetComponent<PlayerController>();
        if (player != null)
        {
            OnPlayerKilled?.Invoke();
        }
    }
}
