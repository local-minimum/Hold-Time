using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void PlayerKilledEvent();

public class KillLayer : MonoBehaviour
{
    [SerializeField]
    Butterfly butterflyPrefab;

    [SerializeField]
    int minButterflies = 3;

    [SerializeField]
    int maxButterflies = 8;

    public static event PlayerKilledEvent OnPlayerKilled;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var player = collision.gameObject.GetComponent<PlayerController>();
        if (player != null && player.Alive)
        {
            OnPlayerKilled?.Invoke();
            int bflies = Random.Range(minButterflies, maxButterflies);
            for (int i = 0; i<bflies; i++)
            {
                var butter = Instantiate(butterflyPrefab);
                butter.transform.position = collision.ClosestPoint(transform.position);
            }
        }
    }
}
