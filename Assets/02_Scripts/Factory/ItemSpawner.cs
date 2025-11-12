using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ItemSpawner : MonoBehaviour
{
    [Header("Factory Dependency")]
    [SerializeField] private MonoBehaviour m_factoryProvider;
    private IItemFactory m_factory;

    [System.Serializable]
    public class ItemSpawnRule
    {
        public EItemType type;
        public int maxAliveCount;
        public float itemRadius = 1.0f;
    }

    [Header("Spawn Rules")]
    [SerializeField] private List<ItemSpawnRule> m_spawnRules;
    [SerializeField] private float m_spawnInterval = 1.5f;
    [SerializeField] private Vector2 m_spawnAreaRange = new Vector2(50f, 50f);
    [SerializeField] private float m_yOffset = 10.0f;
    [SerializeField] private int m_maxSpawnAttempts = 10;

    [Header("이벤트 구독")]
    [SerializeField] private IItemEventChannelSO onTouchItem;

    private Dictionary<EItemType, List<IItem>> m_activeItems = new Dictionary<EItemType, List<IItem>>();
    private Dictionary<EItemType, ItemSpawnRule> m_ruleMap = new Dictionary<EItemType, ItemSpawnRule>();

    private float m_timer;
    private List<ItemSpawnRule> m_availableRulesCache = new List<ItemSpawnRule>();

    void Awake()
    {
        m_factory = m_factoryProvider.GetComponent<IItemFactory>();
        if (m_factory == null)
        {
            Debug.LogError("IItemFactory가 주입되지 않았습니다!", this);
            this.enabled = false;
            return;
        }

        m_activeItems.Clear();
        m_ruleMap.Clear();
        foreach (var rule in m_spawnRules)
        {
            m_activeItems[rule.type] = new List<IItem>();
            m_ruleMap[rule.type] = rule;
        }

        onTouchItem.OnEvent += HandleItemCollected;
    }

    void OnDestroy()
    {
        onTouchItem.OnEvent -= HandleItemCollected;
    }

    private void HandleItemCollected(IItem item)
    {
        if (m_activeItems.TryGetValue(item.Type, out var itemList))
        {
            itemList.Remove(item);
        }
    }

    void Update()
    {
        m_timer += Time.deltaTime;
        if (m_timer >= m_spawnInterval)
        {
            m_timer = 0;
            TrySpawnItemsRandom();
        }
    }

    void TrySpawnItemsRandom()
    {
        m_availableRulesCache.Clear();
        foreach (var rule in m_spawnRules)
        {
            if (m_activeItems[rule.type].Count < rule.maxAliveCount)
            {
                m_availableRulesCache.Add(rule);
            }
        }

        if (m_availableRulesCache.Count > 0)
        {
            ItemSpawnRule ruleToSpawn = m_availableRulesCache[Random.Range(0, m_availableRulesCache.Count)];
            FindSpotAndSpawn(ruleToSpawn);
        }
    }

    void FindSpotAndSpawn(ItemSpawnRule rule)
    {
        for (int i = 0; i < m_maxSpawnAttempts; i++)
        {
            Vector3 randomPos = GetRandomWorldPosition();

            if (!IsPositionOccupied(randomPos, rule.itemRadius))
            {
                IItem newItem = m_factory.GetItem(rule.type);
                if (newItem == null) return;

                newItem.Initialize(randomPos, Quaternion.identity);
                m_activeItems[rule.type].Add(newItem);
                return;
            }
        }
    }

    bool IsPositionOccupied(Vector3 candidatePos, float candidateRadius)
    {
        foreach (var itemList in m_activeItems.Values)
        {
            foreach (var activeItem in itemList)
            {
                if (m_ruleMap.TryGetValue(activeItem.Type, out var activeItemRule))
                {
                    float requiredDistance = candidateRadius + activeItemRule.itemRadius;
                    float currentDistance = Vector3.Distance(
                        candidatePos, activeItem.GetGameObject().transform.position);

                    if (currentDistance < requiredDistance)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    Vector3 GetRandomWorldPosition()
    {
        float xPoint = Random.Range(-m_spawnAreaRange.x, m_spawnAreaRange.x);
        float zPoint = Random.Range(-m_spawnAreaRange.y, m_spawnAreaRange.y);
        Vector3 spawnPosition = transform.position + new Vector3(xPoint, 0, zPoint);
        spawnPosition.y = transform.position.y + m_yOffset;
        return spawnPosition;
    }
}