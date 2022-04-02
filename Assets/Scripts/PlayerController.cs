using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    float jumpForceMax = 2500f;

    [SerializeField]
    float jumpForceMin = 600f;

    [SerializeField, Range(0, 4)]
    float maxChargeTime = 2f;

    [SerializeField]
    Image jumpAimImage;

    [SerializeField, Range(0, 45f)]
    float maxJumpDegrees = 20f;

    [SerializeField, Range(0, 10f)]
    float angleLoopDuration = 3f;

    Rigidbody2D rb;    
    bool grounded = false;
    bool jumping = false;

    float jumpStart;
    Vector3 lastStableGround;

    float jumpDegrees = 0;
    float jumpDegreesChangeDirection = 1;

    private void Start()
    {
        lastStableGround = transform.position;
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        KillLayer.OnPlayerKilled += ReviveAtCommonGround;
    }

    private void OnDisable()
    {
        KillLayer.OnPlayerKilled -= ReviveAtCommonGround;
    }

    private void Update()
    {
        if (!grounded) return;

        if (!jumping && Gamepad.current.rightShoulder.isPressed)
        {
            jumping = true;
            jumpStart = Time.realtimeSinceStartup;

        } else if (jumping && !Gamepad.current.rightShoulder.isPressed)
        {
            jumping = false;
            float angle = Mathf.Deg2Rad * jumpDegrees;
            float force = Mathf.Lerp(jumpForceMin, jumpForceMax, (Time.realtimeSinceStartup - jumpStart) / maxChargeTime);
            rb.AddForce(new Vector2(-Mathf.Sin(angle), Mathf.Cos(angle)) * force, ForceMode2D.Impulse);

        } else if (!jumping)
        {            
            jumpDegrees += jumpDegreesChangeDirection * Time.deltaTime * maxJumpDegrees / angleLoopDuration;
            
            if (jumpDegrees > maxJumpDegrees)
            {
                jumpDegrees = maxJumpDegrees;
                jumpDegreesChangeDirection = -1;
            } else if (jumpDegrees < -maxJumpDegrees)
            {
                jumpDegrees = -maxJumpDegrees;
                jumpDegreesChangeDirection = 1;
            }

            jumpAimImage.transform.rotation = Quaternion.Euler(0, 0, jumpDegrees);
        }
    }

    public void ReviveAtCommonGround()
    {
        transform.position = lastStableGround;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!grounded && collision.gameObject.tag == "StableGround")
        {
            Vector2 collisionCenter = Vector2.zero;
            for (int i = 0; i<collision.contacts.Length; i++)
            {
                collisionCenter += collision.contacts[i].point;
            }
            collisionCenter /= collision.contacts.Length;
            if (collisionCenter.y < transform.position.y)
            {
                StartCoroutine(AttemptSetStableGround(transform.position));
            }
        }
    }

    IEnumerator<WaitForSeconds> AttemptSetStableGround(Vector3 position)
    {
        yield return new WaitForSeconds(0.5f);
        if (Vector3.Magnitude(transform.position - position) < 0.1f)
        {
            lastStableGround = transform.position + Vector3.up * 0.001f;
            grounded = true;
            jumpAimImage.enabled = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        grounded = false;
        jumpAimImage.enabled = false;
    }
}
