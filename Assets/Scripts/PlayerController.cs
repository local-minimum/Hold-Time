using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

enum JumpingTransition { NotJumping, StartJump, Jumping, EndJump };
enum GroundStates { NONE, Stable, Unstable };
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
    HandPointer jumpAim;

    [SerializeField, Range(0, 45f)]
    float maxJumpDegrees = 20f;

    [SerializeField, Range(0, 10f)]
    float angleLoopDuration = 3f;

    Rigidbody2D rb;
    bool grounded = false;

    float jumpStart;
    Vector3 lastStableGround;

    Animator anim;
    float jumpDegrees = 0;
    float jumpDegreesChangeDirection = 1;
    bool alive = true;

    public bool Alive
    {
        get
        {
            return alive;
        }
    }

    public bool Grounded
    {
        get
        {
            return grounded;
        }
    }

    private void Start()
    {
        lastStableGround = transform.position;
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
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
                _jumping = SimpleUnifiedInput.Jump;
                if (_jumping == JumpingState.NotJumping) return JumpingTransition.NotJumping;
                return JumpingTransition.StartJump;
            default:
                if (!SimpleUnifiedInput.CheckJump(_jumping))
                {
                    _jumping = JumpingState.NotJumping;
                    return JumpingTransition.EndJump;
                }
                return JumpingTransition.Jumping;
        }
    }

    private void Update()
    {
        if (!grounded || !alive) return;

        switch (CheckJumping())
        {
            case JumpingTransition.Jumping:
                anim.ResetTrigger("Jump");
                anim.SetTrigger("StartJumping");
                break;

            case JumpingTransition.StartJump:
                jumpStart = Time.realtimeSinceStartup;
                break;

            case JumpingTransition.EndJump:
                anim.ResetTrigger("StartJumping");
                anim.SetTrigger("Jump");

                float angle = Mathf.Deg2Rad * jumpDegrees;
                float forcePercent = Mathf.Clamp01((Time.realtimeSinceStartup - jumpStart) / maxChargeTime);                
                float force = Mathf.Lerp(jumpForceMin, jumpForceMax, forcePercent);

                rb.AddForce(new Vector2(-Mathf.Sin(angle), Mathf.Cos(angle)) * force, ForceMode2D.Impulse);
                OnJump?.Invoke();
                break;

            case JumpingTransition.NotJumping:                
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

                jumpAim.SetDegrees(jumpDegrees);
                break;
        }
    }

    public void ReviveAtCommonGround()
    {
        if (alive) StartCoroutine(Revive());
    }

    IEnumerator<WaitForSeconds> Revive() {
        alive = false;
        float start = Time.timeSinceLevelLoad;
        float progress = 0;        
        anim.SetTrigger("Splatter");
        while (progress < 1)
        {
            rb.velocity *= 0.2f;
            yield return new WaitForSeconds(0.2f);
            progress = (Time.timeSinceLevelLoad - start) / 0.5f;
        }
        start = Time.timeSinceLevelLoad;
        progress = 0;
        rb.velocity = Vector2.zero;
        rb.isKinematic = true;
        while (progress < 1)
        {            
            transform.position = Vector3.Lerp(transform.position, lastStableGround, 0.1f);
            yield return new WaitForSeconds(0.02f);
            progress = (Time.timeSinceLevelLoad - start) / 1.5f;
        }
        transform.position = lastStableGround;
        rb.isKinematic = false;
        alive = true;
    }

    bool testingGround = false;

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!grounded && !testingGround)
        {
            var groundState = GroundStates.NONE;
            if (collision.gameObject.tag == "StableGround") groundState = GroundStates.Stable;
            if (collision.gameObject.tag == "UnstableGround") groundState = GroundStates.Unstable;
            if (groundState == GroundStates.NONE) return;

            Vector2 collisionCenter = Vector2.zero;
            for (int i = 0; i<collision.contacts.Length; i++)
            {
                collisionCenter += collision.contacts[i].point;
            }
            collisionCenter /= collision.contacts.Length;
            if (collisionCenter.y < transform.position.y)
            {
                StartCoroutine(AttemptSetStableGround(transform.position, groundState));
            }
        }
    }

    IEnumerator<WaitForSeconds> AttemptSetStableGround(Vector3 position, GroundStates state)
    {
        testingGround = true;
        yield return new WaitForSeconds(0.5f);
        if (Vector3.Magnitude(transform.position - position) < 0.1f)
        {
            if (state == GroundStates.Stable) lastStableGround = transform.position + Vector3.up * 0.001f;
            grounded = true;
        }
        testingGround = false;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        grounded = false;
    }
}
