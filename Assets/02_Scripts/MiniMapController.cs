using System.Collections.Generic;
using UnityEngine;

public class MinimapController : MonoBehaviour
{
    [Header("매핑 대상")]
    [SerializeField] private ItemSpawner itemSpawner;
    [SerializeField] private Transform playerTransform;

    [Header("UI 요소")]
    [SerializeField] private GameObject redBallPrefab;
    [SerializeField] private GameObject blueBallPrefab;
    [SerializeField] private GameObject greenBallPrefab;
    [SerializeField] private GameObject blackBallPrefab;
    [SerializeField] private RectTransform mapRect;
    [SerializeField] private RectTransform playerIconRect;

    [Header("이벤트 구독")]
    [SerializeField] private IItemEventChannelSO onTouchItem;

    [Header("필터링")]
    [SerializeField] private List<EItemType> typesToShow;
    private HashSet<EItemType> m_filterableTypes;

    private Dictionary<IItem, GameObject> m_iconMap = new Dictionary<IItem, GameObject>();

    private Vector2 m_mapDimensions;
    private Vector2 m_spawnAreaDimensions;
    private Vector3 m_spawnerCenterPos;

    void Start()
    {
        m_mapDimensions = mapRect.sizeDelta;
        m_spawnAreaDimensions = itemSpawner.SpawnAreaRange;
        m_spawnerCenterPos = itemSpawner.transform.position;

        m_filterableTypes = new HashSet<EItemType>(typesToShow);

        itemSpawner.OnItemSpawned += HandleItemSpawned;
        onTouchItem.OnEvent += HandleItemReturned;
    }

    void OnDestroy()
    {
        itemSpawner.OnItemSpawned -= HandleItemSpawned;
        onTouchItem.OnEvent -= HandleItemReturned;
    }

    private void HandleItemSpawned(IItem item)
    {
        if (!m_filterableTypes.Contains(item.Type)) return;
        if (m_iconMap.ContainsKey(item)) return;

        CreateIcon(item);
    }

    private void HandleItemReturned(IItem item)
    {
        if (!m_filterableTypes.Contains(item.Type)) return;

        if (m_iconMap.TryGetValue(item, out GameObject iconInstance))
        {
            Destroy(iconInstance);
            m_iconMap.Remove(item);
        }
    }

    void LateUpdate()
    {
        if (playerTransform == null || playerIconRect == null) return;

        Vector2 uiPos = ConvertWorldToMinimapPos(playerTransform.position);
        playerIconRect.anchoredPosition = uiPos;
        Debug.Log(uiPos);
        float playerYRotation = playerTransform.eulerAngles.y;
        playerIconRect.localEulerAngles = new Vector3(0, 0, -playerYRotation + 180.0f);
    }

    // 새 아이콘 생성
    void CreateIcon(IItem item)
    {
        GameObject newIcon = null;
        switch (item.Type)
        {
            case EItemType.Blue:
                newIcon = Instantiate(blueBallPrefab, mapRect);
                break;
            case EItemType.Red:
                newIcon = Instantiate(redBallPrefab, mapRect);
                break;
            case EItemType.Green:
                newIcon = Instantiate(greenBallPrefab, mapRect);
                break;
            case EItemType.Black:
            case EItemType.FakeBlack:
                newIcon = Instantiate(blackBallPrefab, mapRect);
                break;
        }
        newIcon.name = $"Icon_{item.Type}";

        m_iconMap.Add(item, newIcon);
        UpdateIconPosition(item, newIcon.GetComponent<RectTransform>());
    }

    void UpdateIconPosition(IItem item, RectTransform iconRect)
    {
        Vector2 uiPos = ConvertWorldToMinimapPos(item.GetGameObject().transform.position);
        iconRect.anchoredPosition = uiPos;
    }

    Vector2 ConvertWorldToMinimapPos(Vector3 worldPos)
    {
        Vector3 relativePos = worldPos - m_spawnerCenterPos;

        float percentX = relativePos.x / m_spawnAreaDimensions.x;
        float percentZ = relativePos.z / m_spawnAreaDimensions.y; // 3D의 Z축이 2D의 Y축

        float uiX = Mathf.Clamp(percentX * (m_mapDimensions.x / 2), -m_mapDimensions.x / 2, m_mapDimensions.x / 2);
        float uiY = Mathf.Clamp(percentZ * (m_mapDimensions.y / 2), -m_mapDimensions.y / 2, m_mapDimensions.y / 2);

        return new Vector2(uiX, uiY);
    }
}