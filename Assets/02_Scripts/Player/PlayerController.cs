using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Animator anim;
    private Rigidbody rb;

    [SerializeField] private float moveForce = 10.0f;
    [SerializeField] private float jumpForce = 8.0f;
    [SerializeField] private float moveSpeed = 5.0f;
    [SerializeField] private float rotateSpeed = 5.0f;
    [SerializeField] private Transform mainCamera;
    [SerializeField] private Vector3 groundCheckDis = new Vector3(0.01f, 0.1f, 0.01f);

    private Vector3 inputDir;
    private Vector3 moveDir;
    private Vector3 horiVelo;


    private bool isJumpKeyPressed = false;

    private bool isMoving = false;
    private bool isIdle = true;
    private bool isJumped = false;
    private bool isGround = false;
    private bool isFirstFrameForGrounded = false;
    private Coroutine delayForJump;
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
    }
    private void Start()
    {
        StartCoroutine(CheckCrashCo());
    }
    void Update()
    {
        GroundCheck();
        inputDir = new Vector3(Managers.Input.InputX, 0, Managers.Input.InputY).normalized;
        if (Managers.Input.IsJumpPressed) JumpControl();
    }
    private void FixedUpdate()
    {
        MoveControl();
        AnimChangeIdle();
        LookRotate();
    }
    private IEnumerator CheckCrashCo()
    {
        while (true)
        {
            yield return new WaitForSeconds(1.0f);
            if (transform.position.y < -1.0f) Managers.Game.LoadPlayScene();
        }
    }
    private void JumpControl()
    {
        if (!isJumped && isGround)
        {
            isJumped = true;
            isMoving = false;
            isIdle = false;
            anim.CrossFade("Jump", 0.0f, 0, 0.0f);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }
    private void MoveControl()
    {
        moveDir = Quaternion.Euler(0.0f, mainCamera.eulerAngles.y, 0.0f) * inputDir;
        horiVelo = new Vector3(rb.velocity.x, 0.0f, rb.velocity.z);
        if (horiVelo.magnitude < moveSpeed)
        {
            rb.AddForce(moveDir * moveForce, ForceMode.Force);
        }
    }
    private void AnimChangeIdle()
    {
        if (!isJumped && rb.velocity.magnitude < 0.1f && !isIdle && isGround)
        {
            isIdle = true;
            isMoving = false;
            anim.CrossFade("Idle", 0.2f, 0, 0.0f);
        }
        if (!isJumped && horiVelo.magnitude > 0.1f && !isMoving && isGround)
        {
            isIdle = false;
            isMoving = true;
            anim.CrossFade("Walk", 0.1f);
        }
    }
    private void LookRotate()
    {
        //rb.MoveRotation(Quaternion.Euler(0.0f, mainCamera.eulerAngles.y, 0.0f));
        if (moveDir != Vector3.zero)
        {
            Quaternion lookRatation = Quaternion.LookRotation(moveDir, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRatation, Time.fixedDeltaTime * rotateSpeed);
        }
    }

    private void GroundCheck()
    {
        if (Physics.CheckBox(transform.position, groundCheckDis, Quaternion.identity, LayerManager.GetLayerMask(ELayerName.Ground)))
        {
            isGround = true;

            if (isJumped && !isFirstFrameForGrounded)
            {
                isFirstFrameForGrounded = true;
                if (delayForJump != null) StopCoroutine(delayForJump);
                delayForJump = StartCoroutine(DelayGroundCheckCo(0.2f));
            }
        }
        else
        {
            isGround = false;
            isFirstFrameForGrounded = false;
        }
    }
    IEnumerator DelayGroundCheckCo(float time)
    {
        yield return new WaitForSeconds(time);
        if (isGround) isJumped = false;
    }
}
