using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

enum JumpingState { NotJumping, LeftShoulder, RightShoulder, Keyboard };
enum JumpingTransition { NotJumping, StartJump, Jumping, EndJump };

public delegate void JumpEvent();


public class PlayerController : MonoBehaviour
{
    public static event JumpEvent OnJump;

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

    float jumpStart;
    Vector3 lastStableGround;

    float jumpDegrees = 0;
    float jumpDegreesChangeDirection = 1;
    bool alive = true;

    private void Start()
    {
        lastStableGround = transform.position;
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        KillLayer.OnPlayerKilled += ReviveAtCommonGround;
        LevelGoal.OnLevelDone += HandleLevelDone;
    }

    private void OnDisable()
    {
        KillLayer.OnPlayerKilled -= ReviveAtCommonGround;
        LevelGoal.OnLevelDone -= HandleLevelDone;
    }
    private void HandleLevelDone()
    {
        enabled = false;
        rb.velocity = Vector2.zero;
        rb.isKinematic = true;
    }

    JumpingState _jumping = JumpingState.NotJumping;
    private JumpingTransition CheckJumping()
    {
        switch (_jumping)
        {
            case JumpingState.NotJumping:
                if (Gamepad.current.rightShoulder.isPressed)
                {
                    _jumping = JumpingState.RightShoulder;
                    return JumpingTransition.StartJump;
                }
                else if (Gamepad.current.leftShoulder.isPressed)
                {
                    _jumping = JumpingState.LeftShoulder;
                    return JumpingTransition.StartJump;
                }
                return JumpingTransition.NotJumping;
            case JumpingState.LeftShoulder:
                if (!Gamepad.current.leftShoulder.isPressed)
                {
                    _jumping = JumpingState.NotJumping;
                    return JumpingTransition.EndJump;
                }
                return JumpingTransition.Jumping;
            case JumpingState.RightShoulder:
                if (!Gamepad.current.rightShoulder.isPressed)
                {
                    _jumping = JumpingState.NotJumping;
                    return JumpingTransition.EndJump;
                }
                return JumpingTransition.Jumping;
            default:
                return JumpingTransition.NotJumping;
        }
    }

    private void Update()
    {
        if (!grounded || !alive) return;

        switch (CheckJumping())
        {
            case JumpingTransition.Jumping:
                Color c = jumpAimImage.color;
                c.a = 0;
                jumpAimImage.color = c;
                break;
            case JumpingTransition.StartJump:
                jumpStart = Time.realtimeSinceStartup;
                break;
            case JumpingTransition.EndJump:
                float angle = Mathf.Deg2Rad * jumpDegrees;
                float force = Mathf.Lerp(jumpForceMin, jumpForceMax, (Time.realtimeSinceStartup - jumpStart) / maxChargeTime);
                rb.AddForce(new Vector2(-Mathf.Sin(angle), Mathf.Cos(angle)) * force, ForceMode2D.Impulse);
                OnJump?.Invoke();
                break;
            case JumpingTransition.NotJumping:
                c = jumpAimImage.color;
                c.a = 1;
                jumpAimImage.color = c;

                jumpDegrees += jumpDegreesChangeDirection * Time.deltaTime * maxJumpDegrees / angleLoopDuration;

                if (jumpDegrees > maxJumpDegrees)
                {
                    jumpDegrees = maxJumpDegrees;
                    jumpDegreesChangeDirection = -1;
                }
                else if (jumpDegrees < -maxJumpDegrees)
                {
                    jumpDegrees = -maxJumpDegrees;
                    jumpDegreesChangeDirection = 1;
                }

                jumpAimImage.transform.rotation = Quaternion.Euler(0, 0, jumpDegrees);
                break;
        }
    }

    public void ReviveAtCommonGround()
    {
        if (alive) StartCoroutine(Revive());
    }

    IEnumerator<WaitForSeconds> Revive() {
        alive = false;
        rb.isKinematic = true;
        rb.velocity = Vector2.zero;
        yield return new WaitForSeconds(0.5f);
        float start = Time.timeSinceLevelLoad;
        float progress = 0;
        while (progress < 1)
        {
            progress = (Time.timeSinceLevelLoad - start) / 1.5f;
            transform.position = Vector3.Lerp(transform.position, lastStableGround, 0.1f);
            yield return new WaitForSeconds(0.02f);
        }
        transform.position = lastStableGround;
        rb.isKinematic = false;
        alive = true;
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
