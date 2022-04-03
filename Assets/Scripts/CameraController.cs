using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    Rigidbody2D playerBody;

    [SerializeField]
    float easing = 0.2f;

    [SerializeField]
    float lookAheadX = 1f;

    [SerializeField]
    float lookAheadY = 0.5f;

    [SerializeField]
    float stillThreshold = 0.1f;

    [SerializeField]
    float shakeMagnitude = 0.1f;

    [SerializeField]
    float shakeDuration = 0.5f;

    Vector3 shake = Vector3.zero;

    Vector3 referencePosition;

    private void Awake()
    {
        referencePosition = transform.localPosition;
    }

    private void OnEnable()
    {
        KillLayer.OnPlayerKilled += HandlePlayerKilled;
    }

    private void OnDisable()
    {
        KillLayer.OnPlayerKilled -= HandlePlayerKilled;
    }

    private void HandlePlayerKilled()
    {
        StartCoroutine(Shaker());
    }

    IEnumerator<WaitForSeconds> Shaker()
    {
        float t0 = Time.timeSinceLevelLoad;
        float duration = 0;
        while (duration < shakeDuration)
        {
            shake = new Vector3(Random.Range(-shakeMagnitude, shakeMagnitude), Random.Range(-shakeMagnitude, shakeMagnitude), 0);
            yield return new WaitForSeconds(0.02f);
            duration = Time.timeSinceLevelLoad - t0;
        }
        shake = Vector3.zero;

    }

    private void Update()
    {
        var velocity = playerBody.velocity;
        var lookAhead = new Vector3(velocity.x, velocity.y);

        if (lookAhead.y < -stillThreshold)
        {
            lookAhead.y = -lookAheadY;
        } else if (lookAhead.y > stillThreshold)
        {
            lookAhead.y = lookAheadY;
        } else
        {
            lookAhead.y = 0;
        }
        if (lookAhead.x < -stillThreshold)
        {
            lookAhead.x = -lookAheadX;
        } else if (lookAhead.x > stillThreshold)
        {
            lookAhead.x = lookAheadX;
        } else
        {
            lookAhead.x = 0;
        }
        transform.localPosition = Vector3.Lerp(transform.localPosition, referencePosition + lookAhead, easing) + shake;
    }
}
