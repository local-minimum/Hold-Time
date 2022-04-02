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

    Vector3 referencePosition;

    private void Awake()
    {
        referencePosition = transform.localPosition;
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
        transform.localPosition = Vector3.Lerp(transform.localPosition, referencePosition + lookAhead, easing);
    }
}
