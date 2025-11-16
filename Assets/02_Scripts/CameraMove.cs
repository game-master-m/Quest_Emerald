using TMPro;
using UnityEngine;

public class CameraMove : MonoBehaviour
{
    public Transform target;

    [SerializeField] private Vector3 offset = new Vector3(0f, 2.0f, -5.0f);

    [SerializeField] private float smoothSpeed = 10.0f;
    [SerializeField] private float rotationSmoothSpeed = 10.0f;

    [Header("마우스 컨트롤 설정")]
    [SerializeField] private float mouseSensitivityX = 4.0f;
    [SerializeField] private float mouseSensitivityY = 2.0f;
    [SerializeField] private float minVerticalAngle = -30.0f;
    [SerializeField] private float maxVerticalAngle = 30.0f;

    [Header("정보 출력")]
    [SerializeField] private Transform rootItemDisc;
    [SerializeField] private TextMeshProUGUI itemDiscText;
    [SerializeField] private Vector3 rayOffset = new Vector3(0.0f, 2.0f, 0.0f);
    [SerializeField] private float rayDistance = 5.0f;
    private float m_currentYRotation = 0.0f;
    private float m_currentXRotation = 0.0f;

    [Header("이벤트 구독")]
    [SerializeField] private VoidEventChannelSO onGameOver;
    [SerializeField] private FloatEventChannelSO onGameSuccess;

    private bool isPlaying = false;
    void Start()
    {
        if (target == null)
        {
            return;
        }

        m_currentYRotation = transform.eulerAngles.y;
        m_currentXRotation = transform.eulerAngles.x;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        rootItemDisc.gameObject.SetActive(false);

        isPlaying = true;
    }
    private void OnEnable()
    {
        onGameOver.OnEvent += HandleGameOver;
        onGameSuccess.OnEvent += HandleGameSuccess;
    }
    private void OnDisable()
    {
        onGameOver.OnEvent -= HandleGameOver;
        onGameSuccess.OnEvent -= HandleGameSuccess;
    }
    private void HandleGameSuccess(float garbage)
    {
        isPlaying = false;
        rootItemDisc.gameObject.SetActive(false);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    private void HandleGameOver()
    {
        isPlaying = false;
        rootItemDisc.gameObject.SetActive(false);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && Cursor.lockState != CursorLockMode.Locked && isPlaying)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        if (Input.GetKeyDown(KeyCode.Escape) && isPlaying)
        {
            Cursor.lockState = CursorLockMode.None;
            rootItemDisc.gameObject.SetActive(false);
            Cursor.visible = true;
        }

        if (Cursor.lockState == CursorLockMode.Locked)
        {
            CheckItemDisc();
        }
    }
    private void CheckItemDisc()
    {
        Ray ray = new Ray(target.position + rayOffset, transform.forward);
        RaycastHit hitInfo;

        if (Physics.Raycast(ray, out hitInfo, rayDistance, LayerManager.GetLayerMask(ELayerName.Item)))
        {
            rootItemDisc.gameObject.SetActive(true);
            IItem hitItem = hitInfo.collider.GetComponent<IItem>();
            itemDiscText.text = hitItem.GetNameWhenMousePointed();
        }
        else
        {
            rootItemDisc.gameObject.SetActive(false);
        }
    }
    void LateUpdate()
    {
        if (target == null) return;
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            m_currentYRotation += Input.GetAxis("Mouse X") * mouseSensitivityX;
            m_currentXRotation -= Input.GetAxis("Mouse Y") * mouseSensitivityY;

            m_currentXRotation = Mathf.Clamp(m_currentXRotation, minVerticalAngle, maxVerticalAngle);
        }

        Quaternion rotation = Quaternion.Euler(m_currentXRotation, m_currentYRotation, 0);


        Vector3 desiredPosition = target.position + (rotation * offset);

        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.position = smoothedPosition;

        Quaternion desiredRotation = Quaternion.LookRotation(target.position - transform.position);
        Quaternion smoothedRotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotationSmoothSpeed * Time.deltaTime);

        transform.rotation = smoothedRotation;
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(target.position + rayOffset, transform.forward * rayDistance);
    }
}