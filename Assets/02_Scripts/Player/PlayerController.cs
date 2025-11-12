using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Animator anim;
    private Rigidbody rb;

    [SerializeField] private float moveForce = 10.0f;
    [SerializeField] private float moveSpeed = 5.0f;
    [SerializeField] private float rotateSpeed = 5.0f;
    [SerializeField] private Transform mainCamera;

    private Vector3 inputDir;
    private Vector3 moveDir;

    private bool m_isMoving = false;
    private bool m_isIdle = true;
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
    }
    void Start()
    {

    }

    void Update()
    {
        inputDir = new Vector3(Managers.Input.InputX, 0, Managers.Input.InputY).normalized;
    }
    private void FixedUpdate()
    {
        MoveControl();
        AnimChangeIdle();
        LookRotate();
    }
    private void MoveControl()
    {
        moveDir = Quaternion.Euler(0.0f, mainCamera.eulerAngles.y, 0.0f) * inputDir;
        if (rb.velocity.magnitude < moveSpeed)
        {
            rb.AddForce(moveDir * moveForce, ForceMode.Force);
        }
    }
    private void AnimChangeIdle()
    {
        if (rb.velocity.magnitude < 0.1f && m_isMoving && !m_isIdle)
        {
            m_isIdle = true;
            m_isMoving = false;
            anim.CrossFade("Idle", 0.6f);
        }
        if (rb.velocity.magnitude > 0.1f && m_isIdle && !m_isMoving)
        {
            m_isIdle = false;
            m_isMoving = true;
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
}
